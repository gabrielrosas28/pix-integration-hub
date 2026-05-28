// BankingHub.Domain/Aggregates/Invoice/InvoiceId.cs
namespace BankingHub.Domain.Aggregates.Invoice;

public sealed record InvoiceId
{
    public Guid Value { get; }

    private InvoiceId(Guid value) => Value = value;

    public static InvoiceId CreateNew() => new(Guid.NewGuid());
    
    public static InvoiceId From(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}