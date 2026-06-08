using Domain.Exceptions;
using Domain.ValueObjects;
using Domain.Events;
using Domain.Aggregates.Invoice;

namespace Domain.Aggregates.PixCharge;

public class PixCharge : AggregateRoot<ChargeId>
{
    public TxId TxId { get; private set; } = null!;
    public InvoiceId InvoiceId { get; private set; } = null!;
    public PixChargeType ChargeType { get; private set; }
    public Money? Amount { get; private set; }
    public string PixKey { get; private set; } = null!;
    public DateTime? DueDate { get; private set; }
    public int? ExpiresInSeconds { get; private set; }
    public string PayerMessage { get; private set; } = null!;
    public PixChargeStatus Status { get; private set; }
    public EmvCode Emv { get; private set; } = null!;
    public string? PixLink { get; private set; }
    public string Raw { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private PixCharge() { } // EF Core

    public static PixCharge Create(
        TxId txId, 
        InvoiceId invoiceId, 
        PixChargeType chargeType, 
        Money? amount, 
        string pixKey, 
        DateTime? dueDate,
        int? expiresInSeconds,
        string payerMessage,
        EmvCode emv, 
        string? pixLink,
        string raw)
    {
        if (txId is null) throw new DomainException("TxId is required.");
        if (invoiceId is null) throw new DomainException("InvoiceId is required.");
        if (emv is null) throw new DomainException("EMV code is required.");

        return new PixCharge
        {
            Id = ChargeId.CreateNew(),
            TxId = txId,
            InvoiceId = invoiceId,
            ChargeType = chargeType,
            Amount = amount,
            PixKey = pixKey,
            DueDate = dueDate,
            ExpiresInSeconds = expiresInSeconds,
            PayerMessage = payerMessage ?? string.Empty,
            Status = PixChargeStatus.Active,
            Emv = emv,
            PixLink = pixLink,
            Raw = raw ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ConfirmPayment(Money paidAmount, DateTime paidAt)
    {
        if (Status == PixChargeStatus.Paid)
            return; // Idempotência

        if (Status == PixChargeStatus.Canceled)
            throw new DomainException("Cannot confirm payment for a canceled charge.");

        Status = PixChargeStatus.Paid;
        UpdatedAt = paidAt;

        AddDomainEvent(new PaymentConfirmedEvent(InvoiceId, TxId, paidAmount, paidAt));
    }
}