namespace BankingHub.Domain.Services;

/// <summary>
/// Reconciles a Pix charge against the source-of-truth bank.
/// This is the only path that may transition an Invoice/PixCharge to Paid (§3.1.2).
/// Webhook receipt is not authoritative — reconciliation is.
/// </summary>
public interface IPixReconciliationService
{
    Task<ReconciliationResult> ReconcileAsync(
        string txId,
        string? bankId,
        CancellationToken ct);
}

public sealed record ReconciliationResult(
    string Status,
    bool IsPaid,
    DateTimeOffset? PaidAt,
    decimal? PaidAmount);
