using System.Text.RegularExpressions;
using BankingHub.Domain.Exceptions;

namespace BankingHub.Domain.ValueObjects;

/// <summary>
/// Value Object representing the unique identifier of a Pix charge.
/// Follows the BACEN standard: up to 35 alphanumeric characters.
/// </summary>
public sealed record TxId
{
    private static readonly Regex AllowedPattern = new("^[a-zA-Z0-9]+$", RegexOptions.Compiled);

    public string Value { get; }

    private TxId(string value) => Value = value;

    /// <summary>
    /// Creates a TxId from an existing value (e.g. returned by the bank).
    /// </summary>
    public static TxId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidTxIdException("TxId cannot be empty");

        if (value.Length > 35)
            throw new InvalidTxIdException("TxId cannot exceed 35 characters");

        if (!AllowedPattern.IsMatch(value))
            throw new InvalidTxIdException("TxId must contain only alphanumeric characters");

        return new TxId(value);
    }

    /// <summary>
    /// Generates a new unique TxId.
    /// Format: {prefix}{timestamp}{random} - maximum 35 chars.
    /// </summary>
    public static TxId Generate(string prefix = "PIX")
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..10].ToUpper();
        var value = $"{prefix}{timestamp}{random}";
        if (value.Length > 35) value = value[..35];

        return new TxId(value);
    }

    public override string ToString() => Value;

    public static implicit operator string(TxId txId) => txId.Value;
}
