using Domain.Exceptions;
using Domain.Aggregates.Account; 
namespace Domain.Aggregates.PixKey;

// 1. O Enum específico para a chave
public enum PixKeyType
{
    Cpf,
    Cnpj,
    Email,
    Phone,
    Random
}

//O ID fortemente tipado 
public sealed record PixKeyId
{
    public Guid Value { get; }
    private PixKeyId(Guid value) => Value = value;
    public static PixKeyId CreateNew() => new(Guid.NewGuid());
    public static PixKeyId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

//A Raiz de Agregação da Chave Pix
public class PixKey : AggregateRoot<PixKeyId>
{
    public AccountId AccountId { get; private set; } = null!;
    public string KeyValue { get; private set; } = null!;
    public PixKeyType Type { get; private set; }

    private PixKey() { } // Construtor privado para EF Core

    public static PixKey Create(AccountId accountId, string keyValue, PixKeyType type)
    {
        if (accountId is null)
            throw new DomainException("Account ID is required.");

        if (string.IsNullOrWhiteSpace(keyValue))
            throw new DomainException("Key value is required.");

        return new PixKey
        {
            Id = PixKeyId.CreateNew(),
            AccountId = accountId,
            KeyValue = keyValue,
            Type = type
        };
    }
}