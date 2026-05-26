using BankingHub.Domain.Aggregates.Invoice;
using BankingHub.Domain.ValueObjects;

namespace BankingHub.Domain.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(InvoiceId id, CancellationToken ct);
    Task<Invoice?> GetByTxIdAsync(TxId txId, CancellationToken ct);
    Task AddAsync(Invoice invoice, CancellationToken ct);
    void Update(Invoice invoice);
}
