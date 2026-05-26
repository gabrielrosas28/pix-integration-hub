using BankingHub.Domain.Aggregates.Invoice;
using BankingHub.Domain.Aggregates.PixCharge;
using BankingHub.Domain.ValueObjects;
using MediatR;

namespace BankingHub.Domain.Events;

public sealed record PixChargeCreatedEvent(
    ChargeId ChargeId,
    TxId TxId,
    InvoiceId InvoiceId,
    string BankId,
    PixChargeType ChargeType) : INotification;
