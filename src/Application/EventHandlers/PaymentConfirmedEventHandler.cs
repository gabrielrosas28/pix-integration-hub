// Application/EventHandlers/PaymentConfirmedEventHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Events;
using BankingHub.Application.Interfaces;

namespace Application.EventHandlers;

/// <summary>
/// Handler do evento de pagamento confirmado.
/// Vive na camada Application porque depende de INotificationService (Application)
/// e ILogger — o Domain não pode conhecer essas abstrações.
/// </summary>
public class PaymentConfirmedEventHandler : INotificationHandler<PaymentConfirmedEvent>
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
            "Pagamento confirmado: Invoice={InvoiceId}, TxId={TxId}, Valor={Amount}",
            notification.InvoiceId,
            notification.TxId,
            notification.Amount);

        // Notifica via SignalR
        await _notifications.NotifyPaymentConfirmedAsync(
            notification.InvoiceId.ToString(),
            notification.TxId.ToString(),
            notification.Amount.Value,
            ct);
    }
}
