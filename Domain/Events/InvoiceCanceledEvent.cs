// Domain/Events/InvoiceCanceledEvent.cs
using Domain.Aggregates.Invoice;
using MediatR;

namespace Domain.Events;

public sealed record InvoiceCanceledEvent(
    InvoiceId InvoiceId,
    string Reason
) : INotification;
