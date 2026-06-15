using Application.Interfaces;
using BankingHub.Application.Queries.GetInvoice;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankingHub.Application.Queries.GetInvoice;


public sealed class GetInvoiceHandler : IRequestHandler<GetInvoiceQuery, InvoiceDto?>
{
    private readonly IApplicationDbContext _db;

    public GetInvoiceHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<InvoiceDto?> Handle(GetInvoiceQuery query, CancellationToken ct)
    {
        var row = await _db.Cobrancas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.InvoiceID == query.InvoiceId.ToString(),
                ct);

        if (row is null) return null;

        return new InvoiceDto(
            InvoiceId:  row.InvoiceID,
            TxId:       row.TxId,
            ChargeType: row.ChargeType,
            Amount:     row.Amount,
            Status:     row.Status,
            Emv:        row.Emv,
            PixLink:    row.PixLink,
            CreatedAt:  row.CreatedAt,
            UpdatedAt:  row.UpdatedAt);
    }
}
