using BankingHub.Domain.Aggregates.Invoice;
using BankingHub.Domain.ValueObjects;
using MediatR;

namespace BankingHub.Domain.Events;

public sealed record InvoiceCreatedEvent(
    InvoiceId InvoiceId,
    Money Amount,
    string BankId,
    string? ExternalReference) : INotification;
