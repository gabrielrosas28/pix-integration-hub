using Application.Interfaces;
using BankingHub.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankingHub.Application.Commands.ProcessWebhook;


public sealed class ProcessWebhookHandler
    : IRequestHandler<ProcessWebhookCommand, ProcessWebhookResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IBankAdapterFactory _adapterFactory;
    private readonly ILogger<ProcessWebhookHandler> _logger;

    public ProcessWebhookHandler(
        IApplicationDbContext db,
        IBankAdapterFactory adapterFactory,
        ILogger<ProcessWebhookHandler> logger)
    {
        _db             = db;
        _adapterFactory = adapterFactory;
        _logger         = logger;
    }

    public async Task<ProcessWebhookResult> Handle(
        ProcessWebhookCommand cmd,
        CancellationToken ct)
    {
        // 1. Obtém o adapter e valida autenticidade do webhook
        var adapter = _adapterFactory.Get(cmd.BankId);

        if (!adapter.ValidateWebhook(cmd.Headers, cmd.Body))
        {
            _logger.LogWarning(
                "Webhook inválido rejeitado do banco {BankId}", cmd.BankId);
            return new ProcessWebhookResult(
                Accepted: false,
                TxId:     null,
                Message:  "Webhook com assinatura inválida.");
        }

        // 2. Parseia evento normalizado — apenas para extrair o TxId do gatilho
        var webhookEvent = adapter.ParseWebhookEvent(cmd.Body);

        if (string.IsNullOrWhiteSpace(webhookEvent.TxId))
        {
            _logger.LogWarning(
                "Webhook do banco {BankId} sem TxId identificável. Body: {Body}",
                cmd.BankId, cmd.Body);
            return new ProcessWebhookResult(
                Accepted: true,
                TxId:     null,
                Message:  "Webhook aceito mas sem TxId para processar.");
        }

        var txId = webhookEvent.TxId;

        _logger.LogInformation(
            "Webhook recebido: Banco={BankId}, TxId={TxId}, Evento={EventType}",
            cmd.BankId, txId, webhookEvent.EventType);

        // 3. REGRA CRÍTICA: Não confia no webhook para confirmar pagamento.
        //    Dispara consulta ativa ao banco para validar o status real.
        var cobranca = await _db.Cobrancas
            .FirstOrDefaultAsync(c => c.TxId == txId, ct);

        if (cobranca is null)
        {
            _logger.LogWarning("Cobrança não encontrada para TxId={TxId}", txId);
            return new ProcessWebhookResult(
                Accepted: true,
                TxId:     txId,
                Message:  "Webhook aceito. Cobrança não localizada internamente.");
        }

        // 4. Idempotência: se já está pago, ignora reprocessamento
        if (cobranca.Status == "paid")
        {
            _logger.LogDebug("Webhook idempotente: TxId={TxId} já está pago.", txId);
            return new ProcessWebhookResult(
                Accepted: true,
                TxId:     txId,
                Message:  "Pagamento já confirmado anteriormente.");
        }

        // 5. Consulta ativa ao banco — esta é a fonte de verdade
        var chargeType = cobranca.ChargeType.ToUpperInvariant() == "COBV"
            ? PixChargeType.CobV
            : PixChargeType.Cob;

        ChargeStatusResponse bankStatus;
        try
        {
            bankStatus = await adapter.GetChargeStatusAsync(txId, chargeType, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Falha na consulta ativa ao banco {BankId} para TxId={TxId}",
                cmd.BankId, txId);
            return new ProcessWebhookResult(
                Accepted: true,
                TxId:     txId,
                Message:  "Webhook aceito. Consulta ativa ao banco falhou — será reprocessado.");
        }

        // 6. Atualiza status somente se confirmado via consulta ativa
        if (bankStatus.Status == PixChargeStatus.Paid)
        {
            cobranca.Status    = "paid";
            cobranca.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Pagamento confirmado via consulta ativa: TxId={TxId}, Valor={Amount}, Data={PaidAt}",
                txId, bankStatus.PaidAmount, bankStatus.PaidAt);

            return new ProcessWebhookResult(
                Accepted: true,
                TxId:     txId,
                Message:  "Pagamento confirmado via consulta ativa ao banco.");
        }

        _logger.LogInformation(
            "Webhook processado. Status atual no banco: {Status}. TxId={TxId}",
            bankStatus.Status, txId);

        return new ProcessWebhookResult(
            Accepted: true,
            TxId:     txId,
            Message:  $"Webhook processado. Status: {bankStatus.Status}.");
    }
}
