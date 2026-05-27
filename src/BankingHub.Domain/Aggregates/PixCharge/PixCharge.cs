using System.Text.Json;
using BankingHub.Domain.Aggregates.Invoice;
using BankingHub.Domain.Events;
using BankingHub.Domain.Exceptions;
using BankingHub.Domain.ValueObjects;

namespace BankingHub.Domain.Aggregates.PixCharge;

/// <summary>
/// Aggregate Root representing a Pix charge (Cob or CobV) issued by the Hub.
/// Holds the bank-issued data (EMV, link) and raw payloads for auditing.
/// </summary>
public class PixCharge : AggregateRoot<ChargeId>
{
    public TxId TxId { get; private set; } = null!;
    public InvoiceId InvoiceId { get; private set; } = null!;
    public string BankId { get; private set; } = null!;
    public PixChargeType ChargeType { get; private set; }
    public PixChargeStatus Status { get; private set; }
    public EmvCode Emv { get; private set; } = null!;
    public string? PixLink { get; private set; }
    public string RawPayload { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public Money? PaidAmount { get; private set; }

    private PixCharge() { }

    public static PixCharge Create(
        TxId txId,
        InvoiceId invoiceId,
        string bankId,
        PixChargeType chargeType,
        EmvCode emv,
        string? pixLink,
        object rawPayload)
    {
        if (string.IsNullOrWhiteSpace(bankId))
            throw new DomainException("BankId is required");

        var charge = new PixCharge
        {
            Id = ChargeId.CreateNew(),
            TxId = txId,
            InvoiceId = invoiceId,
            BankId = bankId,
            ChargeType = chargeType,
            Status = PixChargeStatus.Active,
            Emv = emv,
            PixLink = pixLink,
            RawPayload = JsonSerializer.Serialize(rawPayload),
            CreatedAt = DateTime.UtcNow
        };

        charge.AddDomainEvent(new PixChargeCreatedEvent(
            charge.Id, txId, invoiceId, bankId, chargeType));

        return charge;
    }

    public void MarkAsPaid(Money paidAmount, DateTime paidAt)
    {
        if (Status == PixChargeStatus.Paid)
            return;

        if (Status == PixChargeStatus.Canceled)
            throw new DomainException("Cannot mark a canceled charge as paid");

        Status = PixChargeStatus.Paid;
        PaidAmount = paidAmount;
        PaidAt = paidAt;
    }

    public void MarkAsExpired()
    {
        if (Status != PixChargeStatus.Active)
            return;

        Status = PixChargeStatus.Expired;
    }

    public void Cancel()
    {
        if (Status == PixChargeStatus.Paid)
            throw new DomainException("Cannot cancel a paid charge");

        Status = PixChargeStatus.Canceled;
    }
}
