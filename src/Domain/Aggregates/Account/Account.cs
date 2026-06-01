using Domain.Exceptions;

namespace Domain.Aggregates.Account;

public sealed record AccountId
{
    public Guid Value { get; }
    private AccountId(Guid value) => Value = value;
    public static AccountId CreateNew() => new(Guid.NewGuid());
    public static AccountId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public class Account : AggregateRoot<AccountId>
{
    public string Name { get; private set; } = null!;
    public string Document { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private Account() { }

    public static Account Create(string name, string document)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Account name is required.");

        if (string.IsNullOrWhiteSpace(document))
            throw new DomainException("Document (CPF/CNPJ) is required.");

        return new Account
        {
            Id = AccountId.CreateNew(),
            Name = name,
            Document = document,
            CreatedAt = DateTime.UtcNow
        };
    }
}
