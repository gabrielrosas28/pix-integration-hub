// Domain/Repositories/IInvoiceRepository.cs
using Domain.Aggregates.Invoice;

namespace Domain.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(InvoiceId id, CancellationToken ct = default);
    Task AddAsync(Invoice invoice, CancellationToken ct = default);
    Task UpdateAsync(Invoice invoice, CancellationToken ct = default);
}