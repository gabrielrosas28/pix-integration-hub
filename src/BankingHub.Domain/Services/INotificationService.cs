namespace BankingHub.Domain.Services;

/// <summary>
/// Pushes real-time notifications about payment events to interested consumers
/// (e.g. SignalR clients). Implemented by Infrastructure.
/// Lives in Domain because Domain event handlers (§5.3) depend on it.
/// </summary>
public interface INotificationService
{
    Task NotifyPaymentConfirmedAsync(
        string invoiceId,
        string txId,
        decimal amount,
        CancellationToken ct);
}
