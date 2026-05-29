// Domain/Aggregates/Invoice/Invoice.cs
using Domain.Exceptions;
using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Aggregates.Invoice;

/// <summary>
/// Aggregate Root que representa uma Fatura no sistema.
/// Uma Invoice pode ter múltiplas cobranças Pix associadas.
/// </summary>
public class Invoice : AggregateRoot<InvoiceId>
{
    // Propriedades encapsuladas sÃ³ podem ser alteradas atravÃ©s de mÃ©todos do domÃ­nio
    public Money Amount { get; private set; } = null!;
    public DateOnly DueDate { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public string? ExternalReference { get; private set; }
    public TxId? TxId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string BankId { get; private set; } = null!;

    // Construtor privado para o EF Core
    private Invoice() { }

    /// <summary>
    /// Factory method para criar uma nova Invoice.
    /// Garante que a Invoice seja criada em estado vÃ¡lido.
    /// </summary>
    public static Invoice Create(
        Money amount,
        DateOnly dueDate,
        string bankId,
        string? externalReference = null)
    {
        // ValidaÃ§Ãµes de domÃ­nio
        if (amount.Value <= 0)
            throw new DomainException("O valor da fatura deve ser positivo");
        if (dueDate < DateOnly.FromDateTime(DateTime.Today))
            throw new DomainException("A data de vencimento nÃ£o pode ser no passado");
        if (string.IsNullOrWhiteSpace(bankId))
            throw new DomainException("O banco deve ser informado");

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

        // Dispara evento de domÃ­nio
        invoice.AddDomainEvent(new InvoiceCreatedEvent(invoice.Id, amount));
        
        return invoice;
    }

    /// <summary>
    /// Associa um TxId Pix Ã  Invoice.
    /// </summary>
    public void AssignTxId(TxId txId)
    {
        if (TxId is not null)
            throw new DomainException("Invoice jÃ¡ possui TxId associado");
        
        TxId = txId;
    }

    /// <summary>
    /// Marca a Invoice como paga apÃ³s confirmaÃ§Ã£o via consulta ativa.
    /// IMPORTANTE: Este mÃ©todo sÃ³ deve ser chamado apÃ³s validaÃ§Ã£o com o banco.
    /// </summary>
    public void MarkAsPaid(DateTime paidAt, Money paidAmount)
    {
        if (Status == InvoiceStatus.Paid)
            return; // IdempotÃªncia: jÃ¡ estÃ¡ pago
            
        if (Status == InvoiceStatus.Canceled)
            throw new DomainException("NÃ£o Ã© possÃ­vel pagar uma fatura cancelada");

        // Valida se o valor pago Ã© compatÃ­vel (tolerÃ¢ncia de centavos)
        if (Math.Abs(paidAmount.Value - Amount.Value) > 0.01m)
            throw new DomainException($"Valor pago ({paidAmount}) diverge do valor da fatura ({Amount})");

        Status = InvoiceStatus.Paid;
        PaidAt = paidAt;
        
        AddDomainEvent(new PaymentConfirmedEvent(Id, TxId!, paidAmount, paidAt));
    }

    /// <summary>
    /// Cancela a Invoice.
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == InvoiceStatus.Paid)
            throw new DomainException("NÃ£o Ã© possÃ­vel cancelar uma fatura paga");
            
        Status = InvoiceStatus.Canceled;
        AddDomainEvent(new InvoiceCanceledEvent(Id, reason));
    }

    /// <summary>
    /// Marca como vencida (chamado por job de background).
    /// </summary>
    public void MarkAsOverdue()
    {
        if (Status != InvoiceStatus.Open)
            return;
            
        if (DueDate >= DateOnly.FromDateTime(DateTime.Today))
            return; // Ainda nÃ£o venceu
            
        Status = InvoiceStatus.Overdue;
    }
}
