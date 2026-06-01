// Domain.ValueObjects/TxId.cs
using Domain.Exceptions;
namespace Domain.ValueObjects;

/// <summary>
/// Value Object que representa o identificador único de uma cobrança Pix.
/// Segue o padrão definido pelo BACEN: até 35 caracteres alfanuméricos.
/// </summary>
public sealed record TxId
{
    public string Value { get; }

    private TxId(string value) => Value = value;

    /// <summary>
    /// Cria um TxId a partir de um valor existente (ex: retornado pelo banco).
    /// </summary>
    public static TxId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidTxIdException("TxId não pode ser vazio");
            
        if (value.Length > 35)
            throw new InvalidTxIdException("TxId não pode exceder 35 caracteres");
            
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, "^[a-zA-Z0-9]+$"))
            throw new InvalidTxIdException("TxId deve conter apenas caracteres alfanuméricos");

        return new TxId(value);
    }

    /// <summary>
    /// Gera um novo TxId único.
    /// Formato: {prefixo}{timestamp}{random} = máximo 35 chars
    /// </summary>
    public static TxId Generate(string prefix = "PIX")
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..10].ToUpper();
        var value = $"{prefix}{timestamp}{random}"[..35];
        
        return new TxId(value);
    }

    public override string ToString() => Value;

    // Conversão implícita para string
    public static implicit operator string(TxId txId) => txId.Value;
}