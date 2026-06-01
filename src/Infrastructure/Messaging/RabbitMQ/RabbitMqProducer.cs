using System.Text;
using System.Text.Json;
using Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infrastructure.Messaging.RabbitMQ;

public sealed record PaymentCreatedMessage(int UserId, decimal Amount);

public sealed class RabbitMqProducer
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqProducer> _logger;

    public RabbitMqProducer(IOptions<RabbitMqOptions> options, ILogger<RabbitMqProducer> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendMessageAsync(PaymentCreatedMessage message, CancellationToken ct = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        await using var connection = await factory.CreateConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        await channel.QueueDeclareAsync(_options.QueueName, durable: false, exclusive: false, autoDelete: false, cancellationToken: ct);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(exchange: "", routingKey: _options.QueueName, body: body, cancellationToken: ct);

        _logger.LogInformation("Message sent to queue {Queue}: {Message}", _options.QueueName, json);
    }
}
