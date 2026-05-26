using BankingHub.Application.Common.Interfaces;
using BankingHub.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankingHub.Application.Common.EventHandlers;

/// <summary>
/// Reacts to PaymentConfirmedEvent by pushing a real-time notification to
/// interested clients via the notification service.
/// </summary>
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

    public async Task Handle(PaymentConfirmedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Payment confirmed: Invoice={InvoiceId}, TxId={TxId}, Amount={Amount}",
            notification.InvoiceId, notification.TxId, notification.Amount);

        await _notifications.NotifyPaymentConfirmedAsync(
            notification.InvoiceId.ToString(),
            notification.TxId.ToString(),
            notification.Amount.Value,
            cancellationToken);
    }
}
