using Domain.Exceptions;
using Domain.ValueObjects;
using Domain.Aggregates.Account;
using Domain.Aggregates.PixCharge; // Para reaproveitar o PixChargeStatus
using Domain;

namespace Domain.Aggregates.Audit;

// 1. O ID Fortemente Tipado da Auditoria
public sealed record AuditId
{
    public Guid Value { get; }
    private AuditId(Guid value) => Value = value;
    public static AuditId CreateNew() => new(Guid.NewGuid());
    public static AuditId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

// 2. A Raiz de Agregação (Aggregate Root)
public class Audit : AggregateRoot<AuditId>
{
    public AccountId AccountId { get; private set; } = null!;
    public DateTime RegisteredAt { get; private set; }
    public PixChargeStatus PaymentStatus { get; private set; }
    public TxId TxId { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Raw { get; private set; } = null!;
    public Money Amount { get; private set; } = null!;
    public string ActionPayload { get; private set; } = null!;
    public string? ConfirmationPayload { get; private set; }
    public DateTime? PaymentDate { get; private set; }
    public DateTime? ConfirmationTime { get; private set; }

    private Audit() { } // Construtor privado para o EF Core (Aviso CS8618 evitado com = null!)

    public static Audit Create(
        AccountId accountId,
        PixChargeStatus paymentStatus,
        TxId txId,
        string description,
        string raw,
        Money amount,
        string actionPayload)
    {
        // Validações de domínio
        if (accountId is null) throw new DomainException("Account ID is required.");
        if (txId is null) throw new DomainException("TxId is required.");
        if (amount is null) throw new DomainException("Amount is required.");
        if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required.");

        return new Audit
        {
            Id = AuditId.CreateNew(),
            AccountId = accountId,
            RegisteredAt = DateTime.UtcNow,
            PaymentStatus = paymentStatus,
            TxId = txId,
            Description = description,
            Raw = raw ?? string.Empty,
            Amount = amount,
            ActionPayload = actionPayload ?? string.Empty
        };
    }

    // Método de domínio para atualizar a auditoria quando o pagamento é confirmado
    public void ConfirmPayment(string confirmationPayload, DateTime paymentDate, DateTime confirmationTime)
    {
        if (PaymentStatus == PixChargeStatus.Paid)
            return; // Evita sobrescrever se já estiver pago

        PaymentStatus = PixChargeStatus.Paid;
        ConfirmationPayload = confirmationPayload;
        PaymentDate = paymentDate;
        ConfirmationTime = confirmationTime;
    }
}