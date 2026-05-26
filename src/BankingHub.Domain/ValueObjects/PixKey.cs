using BankingHub.Domain.Exceptions;

namespace BankingHub.Domain.ValueObjects;

/// <summary>
/// Value Object representing a Pix key.
/// Pix keys can be CPF, CNPJ, email, phone or random UUID.
/// Max length defined by BACEN is 77 characters.
/// </summary>
public sealed record PixKey
{
    public string Value { get; }

    private PixKey(string value) => Value = value;

    public static PixKey From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("PixKey cannot be empty");

        if (value.Length > 77)
            throw new DomainException("PixKey cannot exceed 77 characters");

        return new PixKey(value);
    }

    public override string ToString() => Value;

    public static implicit operator string(PixKey key) => key.Value;
}
