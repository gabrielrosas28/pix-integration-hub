namespace BankingHub.Application.Common.Interfaces;

/// <summary>
/// Pushes real-time notifications about payment events to interested consumers
/// (e.g. SignalR clients). Implemented by Infrastructure.
/// </summary>
public interface INotificationService
{
    Task NotifyPaymentConfirmedAsync(
        string invoiceId,
        string txId,
        decimal amount,
        CancellationToken ct);
}
