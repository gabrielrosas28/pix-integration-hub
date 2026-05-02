using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Infrastructure.Messaging.RabbitMQ;

public class RabbitMqProducer
{
    public async Task SendMessageAsync(PaymentCreatedMessage message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var queueName = "payment-queue";

        await channel.QueueDeclareAsync(queueName, false, false, false);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync("", queueName, body);

        Console.WriteLine($"Mensagem enviada: {json}");
    }
}