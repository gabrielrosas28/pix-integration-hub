// BankingHub.Domain/ValueObjects/Money.cs
namespace BankingHub.Domain.ValueObjects;

/// <summary>
/// Value Object para valores monetários.
/// Garante precisão decimal e validações de negócio.
/// </summary>
public sealed record Money
{
    public decimal Value { get; }
    public string Currency { get; }

    private Money(decimal value, string currency)
    {
        Value = value;
        Currency = currency;
    }

    public static Money BRL(decimal value)
    {
        if (value < 0)
            throw new DomainException("Valor monetário não pode ser negativo");
            
        // Arredonda para 2 casas decimais
        return new Money(Math.Round(value, 2), "BRL");
    }

    public static Money Zero => new(0, "BRL");

    public override string ToString() => $"{Currency} {Value:N2}";

    // Operadores para facilitar comparações
    public static bool operator >(Money a, Money b) => a.Value > b.Value;
    public static bool operator <(Money a, Money b) => a.Value < b.Value;
    public static bool operator >=(Money a, Money b) => a.Value >= b.Value;
    public static bool operator <=(Money a, Money b) => a.Value <= b.Value;
}

