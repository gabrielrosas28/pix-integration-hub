using Domain.Aggregates.Invoice;
using Domain.ValueObjects;
using MediatR;

namespace Domain.Events;

/// <summary>
/// Evento disparado quando um pagamento é confirmado via consulta ativa.
/// </summary>
public sealed record PaymentConfirmedEvent(
    InvoiceId InvoiceId,
    TxId TxId,
    Money Amount,
    DateTime PaidAt) : INotification;