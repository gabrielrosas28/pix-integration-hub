using BankingHub.Domain.Events;
using BankingHub.Domain.Exceptions;
using BankingHub.Domain.ValueObjects;

namespace BankingHub.Domain.Aggregates.Invoice;

/// <summary>
/// Aggregate Root representing an Invoice in the system.
/// An Invoice may have multiple associated Pix charges.
/// </summary>
public class Invoice : AggregateRoot<InvoiceId>
{
    public Money Amount { get; private set; } = null!;
    public DateOnly DueDate { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public TxId? TxId { get; private set; }
    public string? ExternalReference { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string BankId { get; private set; } = null!;

    private Invoice() { }

    /// <summary>
    /// Factory method to create a new Invoice.
    /// Ensures the Invoice is created in a valid state.
    /// </summary>
    public static Invoice Create(
        Money amount,
        DateOnly dueDate,
        string bankId,
        string? externalReference = null)
    {
        if (amount.Value <= 0)
            throw new DomainException("Invoice amount must be positive");

        if (dueDate < DateOnly.FromDateTime(DateTime.Today))
            throw new DomainException("Due date cannot be in the past");

        if (string.IsNullOrWhiteSpace(bankId))
            throw new DomainException("BankId is required");

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

        invoice.AddDomainEvent(new InvoiceCreatedEvent(
            invoice.Id, amount, bankId, externalReference));

        return invoice;
    }

    /// <summary>
    /// Associates a Pix TxId with the Invoice.
    /// </summary>
    public void AssignTxId(TxId txId)
    {
        if (TxId is not null)
            throw new DomainException("Invoice already has an associated TxId");

        TxId = txId;
    }

    /// <summary>
    /// Marks the Invoice as paid after confirmation via active query to the bank.
    /// IMPORTANT: This method must only be called after bank-side validation.
    /// </summary>
    public void MarkAsPaid(DateTime paidAt, Money paidAmount)
    {
        if (Status == InvoiceStatus.Paid)
            return;

        if (Status == InvoiceStatus.Canceled)
            throw new DomainException("Cannot pay a canceled invoice");

        if (Math.Abs(paidAmount.Value - Amount.Value) > 0.01m)
            throw new DomainException(
                $"Paid amount ({paidAmount}) diverges from invoice amount ({Amount})");

        Status = InvoiceStatus.Paid;
        PaidAt = paidAt;

        AddDomainEvent(new PaymentConfirmedEvent(Id, TxId!, paidAmount, paidAt));
    }

    /// <summary>
    /// Cancels the Invoice.
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == InvoiceStatus.Paid)
            throw new DomainException("Cannot cancel a paid invoice");

        Status = InvoiceStatus.Canceled;
        AddDomainEvent(new InvoiceCanceledEvent(Id, reason));
    }

    /// <summary>
    /// Marks the Invoice as overdue (called by background job).
    /// </summary>
    public void MarkAsOverdue()
    {
        if (Status != InvoiceStatus.Open)
            return;

        if (DueDate >= DateOnly.FromDateTime(DateTime.Today))
            return;

        Status = InvoiceStatus.Overdue;
    }
}
