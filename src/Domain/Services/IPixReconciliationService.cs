// Domain/Services/IPixReconciliationService.cs
using Domain.ValueObjects;

namespace Domain.Services;

public interface IPixReconciliationService
{
    Task ReconcileAsync(TxId txId, CancellationToken ct = default);
}