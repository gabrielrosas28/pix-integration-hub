namespace BankingHub.Domain.Aggregates.PixCharge;

public sealed record ChargeId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static ChargeId CreateNew() => new(Guid.NewGuid());
    public static ChargeId From(Guid value) => new(value);
}
