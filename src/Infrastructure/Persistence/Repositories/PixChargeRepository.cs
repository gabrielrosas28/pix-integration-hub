// Infrastructure/Persistence/Repositories/PixChargeRepository.cs
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Aggregates.PixCharge;
using Domain.Repositories;
using Domain.ValueObjects;

namespace Infrastructure.Persistence.Repositories;

public sealed class PixChargeRepository : IPixChargeRepository
{
    private readonly BankingDbContext _context;

    public PixChargeRepository(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<PixCharge?> GetByTxIdAsync(TxId txId, CancellationToken ct = default)
    {
        return await _context.PixCharges
            .FirstOrDefaultAsync(x => x.TxId == txId, ct);
    }

    public async Task AddAsync(PixCharge charge, CancellationToken ct = default)
    {
        await _context.PixCharges.AddAsync(charge, ct);
    }

    public Task UpdateAsync(PixCharge charge, CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return Task.FromCanceled(ct);
        }

        // Informa explicitamente ao EF Core que o estado da cobrança Pix foi modificado.
        _context.PixCharges.Update(charge);
        
        return Task.CompletedTask;
    }
}