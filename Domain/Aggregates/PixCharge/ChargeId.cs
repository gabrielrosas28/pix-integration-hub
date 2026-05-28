namespace Domain.ValueObjects;

public sealed record ChargeId
{
    public Guid Value { get; }
    private ChargeId(Guid value) => Value = value;
    public static ChargeId CreateNew() => new(Guid.NewGuid());
    public static ChargeId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}