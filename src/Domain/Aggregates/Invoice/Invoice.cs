using Domain.Events;
using Domain.ValueObjects;
using Domain.Exceptions;

namespace Domain.Aggregates.Invoice;

public sealed record InvoiceId
{
    public Guid Value { get; }
    private InvoiceId(Guid value) => Value = value;
    public static InvoiceId CreateNew() => new(Guid.NewGuid());
    public static InvoiceId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public enum InvoiceStatus { Open, Paid, Canceled, Overdue }

/// <summary>
/// Aggregate Root representing a billing invoice.
/// An Invoice can have multiple associated Pix charges.
/// </summary>
public class Invoice : AggregateRoot<InvoiceId>
{
    public Money Amount { get; private set; } = null!;
    public DateOnly DueDate { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public string? ExternalReference { get; private set; }
    public TxId? TxId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string BankId { get; private set; } = null!;

    private Invoice() { }

    public static Invoice Create(
        Money amount,
        DateOnly dueDate,
        string bankId,
        string? externalReference = null)
    {
        if (amount.Value <= 0)
            throw new DomainException("Invoice amount must be positive.");

        if (dueDate < DateOnly.FromDateTime(DateTime.Today))
            throw new DomainException("Due date cannot be in the past.");

        if (string.IsNullOrWhiteSpace(bankId))
            throw new DomainException("Bank ID is required.");

        var invoice = new Invoice
        {
            Id = InvoiceId.CreateNew(),
            Amount = amount,
            DueDate = dueDate,
            Status = InvoiceStatus.Open,
            BankId = bankId,
            ExternalReference = externalReference,
            CreatedAt = DateTime.UtcNow
        };

        invoice.AddDomainEvent(new InvoiceCreatedEvent(invoice.Id, amount));

        return invoice;
    }

    public void AssignTxId(TxId txId)
    {
        if (TxId is not null)
            throw new DomainException("Invoice already has a TxId assigned.");

        TxId = txId;
    }

    /// <summary>
    /// Marks the invoice as paid after active confirmation from the bank.
    /// IMPORTANT: must only be called after validating with the bank.
    /// </summary>
    public void MarkAsPaid(DateTime paidAt, Money paidAmount)
    {
        if (Status == InvoiceStatus.Paid)
            return; // Idempotency

        if (Status == InvoiceStatus.Canceled)
            throw new DomainException("Cannot pay a canceled invoice.");

        if (Math.Abs(paidAmount.Value - Amount.Value) > 0.01m)
            throw new DomainException(
                $"Paid amount ({paidAmount}) does not match invoice amount ({Amount}).");

        Status = InvoiceStatus.Paid;
        PaidAt = paidAt;

        AddDomainEvent(new PaymentConfirmedEvent(Id, TxId!, paidAmount, paidAt));
    }

    public void Cancel(string reason)
    {
        if (Status == InvoiceStatus.Paid)
            throw new DomainException("Cannot cancel a paid invoice.");

        Status = InvoiceStatus.Canceled;
        AddDomainEvent(new InvoiceCanceledEvent(Id, reason));
    }

    public void MarkAsOverdue()
    {
        if (Status != InvoiceStatus.Open)
            return;

        if (DueDate >= DateOnly.FromDateTime(DateTime.Today))
            return;

        Status = InvoiceStatus.Overdue;
    }
}
