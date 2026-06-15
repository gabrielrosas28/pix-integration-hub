// Domain/Events/PaymentConfirmedEvent.cs
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
/// <remarks>
/// O handler deste evento vive na camada Application
/// (Application/EventHandlers/PaymentConfirmedEventHandler.cs), pois depende de
/// INotificationService e ILogger — o Domain não pode referenciar a Application.
/// </remarks>
public sealed record PaymentConfirmedEvent(
    InvoiceId InvoiceId,
    TxId TxId,
    Money Amount,
    DateTime PaidAt) : INotification;