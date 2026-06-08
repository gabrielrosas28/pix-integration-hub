namespace Infrastructure.Messaging.RabbitMQ;

public class PaymentCreatedMessage
{
    public int UserId { get; set; }
    public decimal Amount { get; set; }
}