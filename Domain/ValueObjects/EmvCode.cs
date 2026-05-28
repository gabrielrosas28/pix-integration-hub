// Domain/ValueObjects/EmvCode.cs
using Domain.Exceptions;

namespace Domain.ValueObjects;

public sealed record EmvCode
{
    public string Value { get; }

    private EmvCode(string value) => Value = value;

    public static EmvCode From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("EMV code cannot be empty.");

        return new EmvCode(value);
    }

    public override string ToString() => Value;
    public static implicit operator string(EmvCode emvCode) => emvCode.Value;
}