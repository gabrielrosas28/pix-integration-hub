using Domain.Aggregates.Invoice;
using Domain.Aggregates.PixCharge;
using Domain.ValueObjects;

namespace Domain.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(InvoiceId id, CancellationToken ct = default);
    Task AddAsync(Invoice invoice, CancellationToken ct = default);
    Task UpdateAsync(Invoice invoice, CancellationToken ct = default);
}

public interface IPixChargeRepository
{
    Task<PixCharge?> GetByTxIdAsync(TxId txId, CancellationToken ct = default);
    Task<List<PixCharge>> GetActiveChargesAsync(CancellationToken ct = default);
    Task AddAsync(PixCharge charge, CancellationToken ct = default);
    Task UpdateAsync(PixCharge charge, CancellationToken ct = default);
}
