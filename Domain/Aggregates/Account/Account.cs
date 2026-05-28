using Domain.Exceptions;
using Domain;

namespace Domain.Aggregates.Account;

public class Account : AggregateRoot<AccountId>
{
    public string Name { get; private set; }
    public string Document { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Account() { } // Required by EF Core

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