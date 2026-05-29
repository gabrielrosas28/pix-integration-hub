// BankingHub.Domain/Events/PaymentConfirmedEvent.cs
using MediatR;
using Domain.Aggregates.Invoice;
using Domain.ValueObjects; 

namespace Domain.Events;

/// <summary>
/// Evento disparado quando um pagamento é confirmado via consulta ativa.
/// Subscribers podem reagir a este evento para:
/// - Enviar notificações
/// - Atualizar sistemas externos
/// - Gerar relatórios
/// </summary>
public sealed record PaymentConfirmedEvent(
    InvoiceId InvoiceId,
    TxId TxId,
    Money Amount,
    DateTime PaidAt) : INotification;

// Handler para o evento
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