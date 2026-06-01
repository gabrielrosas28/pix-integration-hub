namespace Domain.Services;

public interface INotificationService
{
    Task NotifyPaymentConfirmedAsync(
        string invoiceId,
        string txId,
        decimal amount,
        CancellationToken ct = default);
}

public interface IPixReconciliationService
{
    Task ReconcileAsync(Domain.ValueObjects.TxId txId, CancellationToken ct = default);
}
