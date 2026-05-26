namespace BankingHub.Application.Common.Interfaces;

/// <summary>
/// Abstraction over an async message bus (RabbitMQ, etc.) for fire-and-forget
/// reconciliation requests and integration events.
/// </summary>
public interface IMessageQueue
{
    Task PublishAsync<TMessage>(string topic, TMessage message, CancellationToken ct)
        where TMessage : notnull;
}
