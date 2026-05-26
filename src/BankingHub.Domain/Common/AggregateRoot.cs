using MediatR;

namespace BankingHub.Domain.Common;

public abstract class AggregateRoot<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(INotification @event) => _domainEvents.Add(@event);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
