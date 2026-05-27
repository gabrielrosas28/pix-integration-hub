namespace BankingHub.Domain;

public abstract record StronglyTypedId<TValue>(TValue Value) where TValue : notnull
{
    public sealed override string ToString() => Value.ToString() ?? string.Empty;
}
