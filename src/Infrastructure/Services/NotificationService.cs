using BankingHub.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Implementação simples de notificação (log). Em produção, notificaria
/// os clientes em tempo real via SignalR (PaymentNotificationHub do doc).
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task NotifyPaymentConfirmedAsync(
        string invoiceId,
        string txId,
        decimal amount,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Notificação de pagamento confirmado: Invoice={InvoiceId}, TxId={TxId}, Valor={Amount}",
            invoiceId, txId, amount);
        return Task.CompletedTask;
    }
}
