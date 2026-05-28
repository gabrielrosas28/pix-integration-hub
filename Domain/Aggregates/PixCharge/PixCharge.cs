namespace ApiService.Domain.Entities;

public class Cobranca
{
    public int Id { get; private set; }
    public string TxId { get; private set; } = string.Empty;
    public string InvoiceID { get; private set; } = string.Empty;
    public string ChargeType { get; private set; } = string.Empty;
    public decimal? Amount { get; private set; }
    public string PixKey { get; private set; } = string.Empty;
    public DateTime? DueDate { get; private set; }
    public int? ExpiresInSeconds { get; private set; }
    public string PayerMessage { get; private set; } = string.Empty;
    public string Status { get; private set; } = "created";
    public string Emv { get; private set; } = string.Empty;
    public string PixLink { get; private set; } = string.Empty;
    public string Raw { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    private Cobranca() { }

    public static Cobranca Create(string txId, string invoiceId, string chargeType, decimal? amount, string pixKey, string emv, string pixLink)
    {
        if (string.IsNullOrWhiteSpace(txId)) throw new ArgumentException("TxId inválido.");
        if (string.IsNullOrWhiteSpace(pixKey)) throw new ArgumentException("Chave Pix obrigatória.");

        return new Cobranca
        {
            TxId = txId,
            InvoiceID = invoiceId,
            ChargeType = chargeType,
            Amount = amount,
            PixKey = pixKey,
            Status = "created",
            Emv = emv,
            PixLink = pixLink,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ConfirmPayment()
    {
        Status = "paid";
        UpdatedAt = DateTime.UtcNow;
    }
}

using Domain.Exceptions;
using Domain.ValueObjects;
using Domain.Events;

namespace Domain.Entities;

public class PixCharge : AggregateRoot<ChargeId>
{
    public TxId TxId { get; private set; }
    public InvoiceId InvoiceId { get; private set; }
    public PixChargeType ChargeType { get; private set; }
    public Money? Amount { get; private set; }
    public string KeyValue { get; private set; } 
    public PixChargeStatus Status { get; private set; }
    public string Emv { get; private set; }
    public string? PixLink { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    private PixCharge() { } // Required by EF Core

    public static PixCharge Create(
        TxId txId, 
        InvoiceId invoiceId, 
        PixChargeType chargeType, 
        Money? amount, 
        string keyValue, 
        string emv, 
        string? pixLink)
    {
        if (txId is null) throw new DomainException("TxId is required.");
        if (invoiceId is null) throw new DomainException("InvoiceId is required.");
        if (string.IsNullOrWhiteSpace(emv)) throw new DomainException("EMV code is required.");

        var charge = new PixCharge
        {
            Id = ChargeId.CreateNew(),
            TxId = txId,
            InvoiceId = invoiceId,
            ChargeType = chargeType,
            Amount = amount,
            KeyValue = keyValue,
            Status = PixChargeStatus.Active,
            Emv = emv,
            PixLink = pixLink,
            CreatedAt = DateTime.UtcNow
        };

        return charge;
    }

    public void ConfirmPayment(Money paidAmount, DateTime paidAt)
    {
        if (Status == PixChargeStatus.Paid)
            return; // Idempotency

        if (Status == PixChargeStatus.Canceled)
            throw new DomainException("Cannot confirm payment for a canceled charge.");

        if (Amount != null && Math.Abs(paidAmount.Value - Amount.Value) > 0.01m)
            throw new DomainException($"Paid amount ({paidAmount}) differs from charge amount ({Amount})");

        Status = PixChargeStatus.Paid;
        PaidAt = paidAt;

        // Dispara o evento de domínio 
        AddDomainEvent(new PaymentConfirmedEvent(InvoiceId, TxId, paidAmount, paidAt));
    }
}