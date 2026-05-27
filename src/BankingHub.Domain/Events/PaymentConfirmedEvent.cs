using BankingHub.Domain.Aggregates.Invoice;
using BankingHub.Domain.Services;
using BankingHub.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankingHub.Domain.Events;

/// <summary>
/// Event raised when a payment is confirmed via active query to the bank.
/// Subscribers may react by sending notifications, updating external systems,
/// or generating reports.
/// </summary>
public sealed record PaymentConfirmedEvent(
    InvoiceId InvoiceId,
    TxId TxId,
    Money Amount,
    DateTime PaidAt) : INotification;

public sealed class PaymentConfirmedEventHandler : INotificationHandler<PaymentConfirmedEvent>
{
    private readonly INotificationService _notifications;
    private readonly ILogger<PaymentConfirmedEventHandler> _logger;

    public PaymentConfirmedEventHandler(
        INotificationService notifications,
        ILogger<PaymentConfirmedEventHandler> logger)
    {
        _notifications = notifications;
        _logger = logger;
    }

    public async Task Handle(PaymentConfirmedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Payment confirmed: Invoice={InvoiceId}, TxId={TxId}, Amount={Amount}",
            notification.InvoiceId, notification.TxId, notification.Amount);

        await _notifications.NotifyPaymentConfirmedAsync(
            notification.InvoiceId.ToString(),
            notification.TxId.ToString(),
            notification.Amount.Value,
            ct);
    }
}
