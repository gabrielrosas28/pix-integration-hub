// Infrastructure/Persistence/Repositories/InvoiceRepository.cs
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Aggregates.Invoice;
using Domain.Repositories;
using ApiService.Infrastructure.Data;

namespace Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _context;

    public InvoiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(InvoiceId id, CancellationToken ct = default)
    {
        return await _context.Invoices
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(Invoice invoice, CancellationToken ct = default)
    {
        await _context.Invoices.AddAsync(invoice, ct);
    }

    public Task UpdateAsync(Invoice invoice, CancellationToken ct = default)
    {
        // Se a operação lançar um cancelamento antes de executar, respeita o token
        if (ct.IsCancellationRequested)
        {
            return Task.FromCanceled(ct);
        }

        // Informa explicitamente ao EF Core que o estado da entidade foi modificado.
        // Como o método da sua interface retorna uma Task, encapsulamos o retorno em Task.CompletedTask
        _context.Invoices.Update(invoice);
        
        return Task.CompletedTask;
    }
}