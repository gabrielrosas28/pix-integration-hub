using ApiService.Infrastructure.Data;
using BankingHub.Application.Interfaces;
using BankingHub.Application.Queries.GetPixChargeStatus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankingHub.Application.Queries.GetPixChargeStatus;


public sealed record PixChargeStatusDto(
    string TxId,
    string InvoiceId,
    string ChargeType,
    decimal? Amount,
    string Status,
    string Emv,
    string? PixLink,
    DateTime CreatedAt,
    DateTime? UpdatedAt);


public sealed class GetPixChargeStatusHandler
    : IRequestHandler<GetPixChargeStatusQuery, PixChargeStatusDto?>
{
    private readonly ApplicationDbContext _db;
    private readonly IBankAdapterFactory _adapterFactory;
    private readonly ILogger<GetPixChargeStatusHandler> _logger;

    public GetPixChargeStatusHandler(
        ApplicationDbContext db,
        IBankAdapterFactory adapterFactory,
        ILogger<GetPixChargeStatusHandler> logger)
    {
        _db             = db;
        _adapterFactory = adapterFactory;
        _logger         = logger;
    }

    public async Task<PixChargeStatusDto?> Handle(
        GetPixChargeStatusQuery query,
        CancellationToken ct)
    {
        var charge = await _db.Charges
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TxId == query.TxId, ct);

        if (cobranca is null) return null;

        // Para cobranças ativas, faz consulta ativa ao banco para garantir dado fresco
        if (charge.Status is "active" or "open" or "created")
        {
            try
            {
                var adapter    = _adapterFactory.Get("ITAU"); // BankId resolvido pelo adapter factory
                var chargeType = charge.ChargeType.ToUpperInvariant() == "COBV"
                    ? PixChargeType.CobV
                    : PixChargeType.Cob;

                var bankStatus = await adapter.GetChargeStatusAsync(query.TxId, chargeType, ct);

                // Sincroniza status localmente se mudou
                if (bankStatus.Status == PixChargeStatus.Paid && charge.Status != "paid")
                {
                    var tracked = await _db.Charges
                        .FirstOrDefaultAsync(c => c.TxId == query.TxId, ct);
                    if (tracked is not null)
                    {
                        tracked.Status    = "paid";
                        tracked.UpdatedAt = DateTime.UtcNow;
                        await _db.SaveChangesAsync(ct);
                        cobranca = tracked;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Consulta ativa ao banco falhou para TxId={TxId}. Retornando status local.",
                    query.TxId);
            }
        }

        return new PixChargeStatusDto(
            TxId:       charge.TxId,
            InvoiceId:  charge.InvoiceID,
            ChargeType: charge.ChargeType,
            Amount:     charge.Amount,
            Status:     charge.Status,
            Emv:        charge.Emv,
            PixLink:    charge.PixLink,
            CreatedAt:  charge.CreatedAt,
            UpdatedAt:  charge.UpdatedAt);
    }
}
