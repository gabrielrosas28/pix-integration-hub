using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class PixKey : AggregateRoot<PixKeyId>
{
    public AccountId AccountId { get; private set; }
    public string KeyValue { get; private set; }
    public PixKeyType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PixKey() { } // Required by EF Core

    public static PixKey Create(AccountId accountId, string keyValue, PixKeyType type)
    {
        if (accountId is null)
            throw new DomainException("Pix key must be linked to a valid Account.");

        if (string.IsNullOrWhiteSpace(keyValue))
            throw new DomainException("Key value cannot be empty.");

        return new PixKey
        {
            Id = PixKeyId.CreateNew(),
            AccountId = accountId,
            KeyValue = keyValue,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };
    }
}