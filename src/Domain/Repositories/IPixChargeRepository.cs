// Domain/Repositories/IPixChargeRepository.cs
using Domain.Aggregates.PixCharge;
using Domain.ValueObjects;

namespace Domain.Repositories;

public interface IPixChargeRepository
{
    Task<PixCharge?> GetByTxIdAsync(TxId txId, CancellationToken ct = default);
    Task AddAsync(PixCharge charge, CancellationToken ct = default);
    Task UpdateAsync(PixCharge charge, CancellationToken ct = default);
}