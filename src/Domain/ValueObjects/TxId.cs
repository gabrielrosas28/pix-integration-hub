using Domain.Exceptions;

namespace Domain.ValueObjects;

/// <summary>
/// Value Object representing the unique identifier of a Pix charge.
/// Follows the BACEN standard: up to 35 alphanumeric characters.
/// </summary>
public sealed record TxId
{
    public string Value { get; }

    private TxId(string value) => Value = value;

    public static TxId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidTxIdException("TxId cannot be empty.");

        if (value.Length > 35)
            throw new InvalidTxIdException("TxId cannot exceed 35 characters.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(value, "^[a-zA-Z0-9]+$"))
            throw new InvalidTxIdException("TxId must contain only alphanumeric characters.");

        return new TxId(value);
    }

    /// <summary>
    /// Generates a new unique TxId.
    /// Format: {prefix}{timestamp}{random} — max 35 chars.
    /// </summary>
    public static TxId Generate(string prefix = "PIX")
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..10].ToUpper();
        var value = $"{prefix}{timestamp}{random}"[..35];
        return new TxId(value);
    }

    public override string ToString() => Value;

    public static implicit operator string(TxId txId) => txId.Value;
}
