// BankingHub.Domain/Events/InvoiceCanceledEvent.cs
using BankingHub.Domain.Aggregates.Invoice;
using MediatR;

namespace BankingHub.Domain.Events;

public sealed record InvoiceCanceledEvent(
    InvoiceId InvoiceId,
    string Reason
) : INotification;