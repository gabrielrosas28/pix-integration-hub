namespace Domain.Aggregates.Account;

public sealed record AccountId
{
    public Guid Value { get; }
    private AccountId(Guid value) => Value = value;
    public static AccountId CreateNew() => new(Guid.NewGuid());
    public static AccountId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}