using BankingHub.Domain.Common;

namespace BankingHub.Domain.Aggregates.Invoice;

public sealed record InvoiceId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static InvoiceId CreateNew() => new(Guid.NewGuid());
    public static InvoiceId From(Guid value) => new(value);
}
