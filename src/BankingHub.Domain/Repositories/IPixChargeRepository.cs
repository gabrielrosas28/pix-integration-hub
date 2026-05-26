using BankingHub.Domain.Aggregates.PixCharge;
using BankingHub.Domain.ValueObjects;

namespace BankingHub.Domain.Repositories;

public interface IPixChargeRepository
{
    Task<PixCharge?> GetByIdAsync(ChargeId id, CancellationToken ct);
    Task<PixCharge?> GetByTxIdAsync(TxId txId, CancellationToken ct);
    Task AddAsync(PixCharge charge, CancellationToken ct);
    void Update(PixCharge charge);
}
