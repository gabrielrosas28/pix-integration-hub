using MediatR;
using Domain.Aggregates.Invoice;
using Domain.ValueObjects;

namespace Domain.Events;

public sealed record InvoiceCreatedEvent(
    InvoiceId InvoiceId,
    Money Amount) : INotification;

public sealed record InvoiceCanceledEvent(
    InvoiceId InvoiceId,
    string Reason) : INotification;

public sealed record PaymentConfirmedEvent(
    InvoiceId InvoiceId,
    TxId TxId,
    Money Amount,
    DateTime PaidAt) : INotification;

public sealed record PixChargeCreatedEvent(
    Domain.Aggregates.PixCharge.ChargeId ChargeId,
    InvoiceId InvoiceId) : INotification;
