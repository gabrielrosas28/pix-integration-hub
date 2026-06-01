// BankingHub.Domain/Events/InvoiceCreatedEvent.cs
using Domain.Aggregates.Invoice;
using Domain.ValueObjects;
using MediatR;

namespace Domain.Events;

public sealed record InvoiceCreatedEvent(
    InvoiceId InvoiceId,
    Money Amount
) : INotification;