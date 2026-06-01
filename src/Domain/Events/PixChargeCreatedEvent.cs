// Domain/Events/PixChargeCreatedEvent.cs
using Domain.Aggregates.Invoice;
using Domain.ValueObjects;
using MediatR;

namespace Domain.Events;

public sealed record PixChargeCreatedEvent(
    ChargeId ChargeId,
    InvoiceId InvoiceId
) : INotification;