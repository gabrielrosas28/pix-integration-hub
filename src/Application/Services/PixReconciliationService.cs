using ApiService.Infrastructure.Data;
using BankingHub.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankingHub.Application.Services;


public class PixReconciliationService
{
    private readonly ApplicationDbContext _db;
    private readonly IBankAdapterFactory _adapterFactory;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PixReconciliationService> _logger;

    public PixReconciliationService(
        ApplicationDbContext db,
        IBankAdapterFactory adapterFactory,
        INotificationService notificationService,
        ILogger<PixReconciliationService> logger)
    {
        _db                  = db;
        _adapterFactory      = adapterFactory;
        _notificationService = notificationService;
        _logger              = logger;
    }

    
    public async Task ReconcilePendingPixChargesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Iniciando o processo de conciliação de Pix...");

        // Busca cobranças ativas com mais de 1 minuto — candidatas ao polling fallback
        var cutoff = DateTime.UtcNow.AddMinutes(-1);
        var pendingCharges = await _db.Charges
            .Where(c => (c.Status == "active" || c.Status == "created")
                        && c.TxId != string.Empty
                        && c.CreatedAt <= cutoff)
            .AsNoTracking()
            .ToListAsync(ct);

        if (!pendingCharges.Any())
        {
            _logger.LogInformation("Nenhuma cobrança Pix pendente para conciliação.");
            return;
        }

        _logger.LogInformation(
            "{Count} cobranças pendentes encontradas para conciliação.", pendingCharges.Count);

        foreach (var charge in pendingCharges)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                _logger.LogInformation(
                    "Verificando TxId={TxId} no banco...", charge.TxId);

                // Determina o adapter — por ora ITAU (Fase 1); extensível via BankId futuro
                var adapter    = _adapterFactory.Get("ITAU");
                var chargeType = charge.ChargeType.ToUpperInvariant() == "COBV"
                    ? PixChargeType.CobV
                    : PixChargeType.Cob;

                // Consulta ativa ao banco — FONTE DE VERDADE
                var bankStatus = await adapter.GetChargeStatusAsync(charge.TxId, chargeType, ct);

                if (bankStatus.Status == PixChargeStatus.Paid)
                {
                    _logger.LogWarning(
                        "Inconsistência detectada: TxId={TxId} pago no banco mas pendente localmente. Conciliando...",
                        charge.TxId);

                    // Atualiza status no banco de dados
                    var tracked = await _db.Charges
                        .FirstOrDefaultAsync(c => c.TxId == charge.TxId, ct);

                    if (tracked is not null)
                    {
                        tracked.Status    = "paid";
                        tracked.UpdatedAt = DateTime.UtcNow;
                        await _db.SaveChangesAsync(ct);
                    }

                    // Notifica via SignalR
                    await _notificationService.NotifyPaymentConfirmedAsync(
                        charge.InvoiceID,
                        charge.TxId,
                        bankStatus.PaidAmount ?? charge.Amount ?? 0m,
                        ct);
                }
                else if (bankStatus.Status == PixChargeStatus.Expired
                      || bankStatus.Status == PixChargeStatus.Canceled)
                {
                    var tracked = await _db.Charges
                        .FirstOrDefaultAsync(c => c.TxId == charge.TxId, ct);

                    if (tracked is not null)
                    {
                        tracked.Status    = bankStatus.Status.ToString().ToLower();
                        tracked.UpdatedAt = DateTime.UtcNow;
                        await _db.SaveChangesAsync(ct);
                    }

                    _logger.LogInformation(
                        "TxId={TxId} marcado como {Status} após conciliação.", charge.TxId, bankStatus.Status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Erro ao conciliar TxId={TxId}. Será tentado novamente.", charge.TxId);
            }
        }

        _logger.LogInformation("Conciliação de Pix concluída.");
    }
}
