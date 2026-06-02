using ApiService.Infrastructure.Data;
using ApiService.Domain.Entities;
using BankingHub.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankingHub.Application.Commands.CreatePixCharge;


public sealed class CreatePixChargeHandler
    : IRequestHandler<CreatePixChargeCommand, CreatePixChargeResult>
{
    private readonly ApplicationDbContext _db;
    private readonly IBankAdapterFactory _adapterFactory;
    private readonly ILogger<CreatePixChargeHandler> _logger;

    public CreatePixChargeHandler(
        ApplicationDbContext db,
        IBankAdapterFactory adapterFactory,
        ILogger<CreatePixChargeHandler> logger)
    {
        _db             = db;
        _adapterFactory = adapterFactory;
        _logger         = logger;
    }

    public async Task<CreatePixChargeResult> Handle(
        CreatePixChargeCommand cmd,
        CancellationToken ct)
    {
        // 1. Verifica se existe Invoice pendente para este InvoiceId
        var invoiceRow = await _db.Cobrancas
            .FirstOrDefaultAsync(
                c => c.InvoiceID == cmd.InvoiceId.ToString() && c.Status == "open",
                ct)
            ?? throw new KeyNotFoundException($"Invoice '{cmd.InvoiceId}' não encontrada ou não está aberta.");

        // 2. Gera TxId único seguindo padrão BACEN (até 35 chars alfanuméricos)
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random    = Guid.NewGuid().ToString("N")[..10].ToUpper();
        var txId      = $"PIX{timestamp}{random}"[..35];

        // 3. Determina tipo de cobrança
        var chargeType = cmd.ChargeType.ToUpperInvariant() switch
        {
            "COB"  => PixChargeType.Cob,
            "COBV" => PixChargeType.CobV,
            _      => throw new ArgumentException($"ChargeType inválido: '{cmd.ChargeType}'. Use 'COB' ou 'COBV'.")
        };

        // 4. Obtém o adapter do banco (BankId está armazenado no Raw da Invoice como fallback,
        //    mas o adapter default é ITAU enquanto somente Itaú está na Fase 1)
        var bankId  = string.IsNullOrWhiteSpace(invoiceRow.Raw) ? "ITAU" : invoiceRow.Raw;
        var adapter = _adapterFactory.Get(bankId);

        // 5. Monta request normalizado para o adapter
        var chargeRequest = new ChargeRequest(
            TxId:             txId,
            Type:             chargeType,
            Amount:           cmd.Amount,
            PixKey:           cmd.PixKey,
            DueDate:          cmd.DueDate,
            ExpiresInSeconds: cmd.ExpiresInSeconds,
            PayerMessage:     cmd.PayerMessage);

        // 6. Cria cobrança no banco via adapter
        ChargeResponse bankResponse;
        try
        {
            bankResponse = chargeType == PixChargeType.CobV
                ? await adapter.CreateCobVAsync(chargeRequest, ct)
                : await adapter.CreateCobAsync(chargeRequest, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cobrança no banco {BankId}", bankId);
            throw new InvalidOperationException(
                $"Falha na comunicação com o banco '{bankId}': {ex.Message}", ex);
        }

        // 7. Obtém QR Code EMV
        var qrCode = await adapter.GetQrCodeAsync(txId, chargeType, ct);

        // 8. Persiste a cobrança (atualiza o registro de Invoice e cria registro de PixCharge)
        // Atualiza invoice com TxId
        invoiceRow.TxId      = txId;
        invoiceRow.Status    = "active";
        invoiceRow.UpdatedAt = DateTime.UtcNow;

        // Cria registro separado de PixCharge
        var pixCharge = new Cobranca
        {
            TxId             = txId,
            InvoiceID        = cmd.InvoiceId.ToString(),
            ChargeType       = cmd.ChargeType.ToUpperInvariant(),
            Amount           = cmd.Amount,
            PixKey           = cmd.PixKey,
            DueDate          = cmd.DueDate.HasValue
                                   ? cmd.DueDate.Value.ToDateTime(TimeOnly.MinValue)
                                   : null,
            ExpiresInSeconds = cmd.ExpiresInSeconds,
            PayerMessage     = cmd.PayerMessage ?? string.Empty,
            Status           = bankResponse.Status.ToString().ToLower(),
            Emv              = qrCode.Emv,
            PixLink          = qrCode.PixLink ?? string.Empty,
            Raw              = bankResponse.Raw?.ToString() ?? string.Empty,
            CreatedAt        = DateTime.UtcNow
        };

        _db.Cobrancas.Add(pixCharge);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Cobrança Pix criada: TxId={TxId}, Invoice={InvoiceId}, Banco={BankId}",
            txId, cmd.InvoiceId, bankId);

        return new CreatePixChargeResult(
            TxId:    txId,
            Status:  bankResponse.Status.ToString(),
            Emv:     qrCode.Emv,
            PixLink: qrCode.PixLink);
    }
}
