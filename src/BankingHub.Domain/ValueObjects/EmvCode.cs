using BankingHub.Domain.Exceptions;

namespace BankingHub.Domain.ValueObjects;

/// <summary>
/// Value Object representing the EMV QR Code payload used in Pix "copy and paste".
/// </summary>
public sealed record EmvCode
{
    public string Value { get; }

    private EmvCode(string value) => Value = value;

    public static EmvCode From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("EmvCode cannot be empty");

        return new EmvCode(value);
    }

    public override string ToString() => Value;

    public static implicit operator string(EmvCode code) => code.Value;
}
