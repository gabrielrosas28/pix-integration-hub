using BankingHub.Domain.Aggregates.Invoice;
using BankingHub.Domain.ValueObjects;
using MediatR;

namespace BankingHub.Domain.Events;

/// <summary>
/// Event raised when a payment is confirmed via active query to the bank.
/// Subscribers may react by sending notifications, updating external systems,
/// or generating reports.
/// </summary>
public sealed record PaymentConfirmedEvent(
    InvoiceId InvoiceId,
    TxId TxId,
    Money Amount,
    DateTime PaidAt) : INotification;
