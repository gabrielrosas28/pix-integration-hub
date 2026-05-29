// Domain/ValueObjects/TxId.cs
using Domain.Exceptions;
namespace Domain.ValueObjects;

/// <summary>
/// Value Object que representa o identificador Ãºnico de uma cobranÃ§a Pix.
/// Segue o padrÃ£o definido pelo BACEN: atÃ© 35 caracteres alfanumÃ©ricos.
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
            throw new InvalidTxIdException("TxId nÃ£o pode ser vazio");
            
        if (value.Length > 35)
            throw new InvalidTxIdException("TxId nÃ£o pode exceder 35 caracteres");
            
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, "^[a-zA-Z0-9]+$"))
            throw new InvalidTxIdException("TxId deve conter apenas caracteres alfanumÃ©ricos");

        return new TxId(value);
    }

    /// <summary>
    /// Gera um novo TxId Ãºnico.
    /// Formato: {prefixo}{timestamp}{random} = mÃ¡ximo 35 chars
    /// </summary>
    public static TxId Generate(string prefix = "PIX")
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..10].ToUpper();
        var value = $"{prefix}{timestamp}{random}"[..35];
        
        return new TxId(value);
    }

    public override string ToString() => Value;

    // ConversÃ£o implÃ­cita para string
    public static implicit operator string(TxId txId) => txId.Value;
}
