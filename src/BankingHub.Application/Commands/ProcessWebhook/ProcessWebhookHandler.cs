using BankingHub.Application.Interfaces;
using BankingHub.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankingHub.Application.Commands.ProcessWebhook;

public sealed class ProcessWebhookHandler
    : IRequestHandler<ProcessWebhookCommand, ProcessWebhookResult>
{
    private readonly IBankAdapterFactory _adapterFactory;
    private readonly IPixReconciliationService _reconciliation;
    private readonly ILogger<ProcessWebhookHandler> _logger;

    public ProcessWebhookHandler(
        IBankAdapterFactory adapterFactory,
        IPixReconciliationService reconciliation,
        ILogger<ProcessWebhookHandler> logger)
    {
        _adapterFactory = adapterFactory;
        _reconciliation = reconciliation;
        _logger = logger;
    }

    public async Task<ProcessWebhookResult> Handle(
        ProcessWebhookCommand cmd,
        CancellationToken ct)
    {
        if (!_adapterFactory.IsSupported(cmd.BankId))
            return new ProcessWebhookResult(false, null, $"Unsupported bank: {cmd.BankId}");

        var adapter = _adapterFactory.Get(cmd.BankId);

        if (!adapter.ValidateWebhook(cmd.Headers, cmd.Body))
        {
            _logger.LogWarning("Webhook authenticity validation failed for bank {BankId}", cmd.BankId);
            return new ProcessWebhookResult(false, null, "Webhook authenticity validation failed");
        }

        var evt = adapter.ParseWebhookEvent(cmd.Body);
        if (string.IsNullOrWhiteSpace(evt.TxId))
        {
            _logger.LogWarning("Webhook received without TxId: bank={BankId}, type={Type}", cmd.BankId, evt.EventType);
            return new ProcessWebhookResult(true, null, "Webhook accepted without TxId; nothing to reconcile");
        }

        _logger.LogInformation(
            "Webhook accepted: TxId={TxId}, Bank={BankId}, Type={Type}. Triggering reconciliation.",
            evt.TxId, cmd.BankId, evt.EventType);

        await _reconciliation.ReconcileAsync(evt.TxId!, cmd.BankId, ct);

        return new ProcessWebhookResult(true, evt.TxId, "Reconciliation triggered");
    }
}
