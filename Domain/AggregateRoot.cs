// BankingHub.Domain/AggregateRoot.cs
using MediatR;

namespace BankingHub.Domain;

public abstract class AggregateRoot<TId>
{
    public TId Id { get; protected set; } = default!;
    
    private readonly List<INotification> _domainEvents = new();
    
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(INotification eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}