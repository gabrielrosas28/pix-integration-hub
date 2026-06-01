using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Repositories;
using Domain.ValueObjects;
using Application.Interfaces;

namespace Application.Commands.ProcessWebhook;

// ---- Command ----

public sealed record ProcessWebhookCommand(
    string BankId,
    IReadOnlyDictionary<string, string> Headers,
    JsonElement Body) : IRequest<ProcessWebhookResult>;

public sealed record ProcessWebhookResult(bool Accepted, string? Reason = null);

// ---- Handler ----

public sealed class ProcessWebhookHandler
    : IRequestHandler<ProcessWebhookCommand, ProcessWebhookResult>
{
    private readonly IBankAdapterFactory _adapterFactory;
    private readonly IPixChargeRepository _chargeRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessWebhookHandler> _logger;

    public ProcessWebhookHandler(
        IBankAdapterFactory adapterFactory,
        IPixChargeRepository chargeRepository,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProcessWebhookHandler> logger)
    {
        _adapterFactory = adapterFactory;
        _chargeRepository = chargeRepository;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ProcessWebhookResult> Handle(
        ProcessWebhookCommand cmd,
        CancellationToken ct)
    {
        var adapter = _adapterFactory.Get(cmd.BankId);

        // 1. Validate webhook authenticity (signature, mTLS, IP)
        if (!adapter.ValidateWebhook(cmd.Headers, cmd.Body))
        {
            _logger.LogWarning("Invalid webhook received from bank {BankId}", cmd.BankId);
            return new ProcessWebhookResult(false, "Webhook validation failed.");
        }

        // 2. Parse event — webhook is ONLY a trigger, never the final confirmation
        var webhookEvent = adapter.ParseWebhookEvent(cmd.Body);

        if (webhookEvent.TxId is null)
            return new ProcessWebhookResult(false, "TxId not found in webhook payload.");

        _logger.LogInformation(
            "Webhook received: Bank={BankId}, TxId={TxId}, Event={EventType}",
            cmd.BankId, webhookEvent.TxId, webhookEvent.EventType);

        // 3. Confirm via active query — CRITICAL RULE: webhook is never the source of truth
        var bankStatus = await adapter.GetChargeStatusAsync(
            webhookEvent.TxId,
            Domain.Aggregates.PixCharge.PixChargeType.CobV, // will query both endpoints
            ct);

        if (bankStatus.Status != Domain.Aggregates.PixCharge.PixChargeStatus.Paid)
        {
            _logger.LogInformation(
                "Webhook trigger for TxId={TxId}: bank status is {Status}, no action taken.",
                webhookEvent.TxId, bankStatus.Status);
            return new ProcessWebhookResult(true);
        }

        // 4. Load charge and invoice
        var txId = TxId.From(webhookEvent.TxId);
        var charge = await _chargeRepository.GetByTxIdAsync(txId, ct);

        if (charge is null)
        {
            _logger.LogWarning("Charge not found for TxId={TxId}", webhookEvent.TxId);
            return new ProcessWebhookResult(false, "Charge not found.");
        }

        if (charge.Status == Domain.Aggregates.PixCharge.PixChargeStatus.Paid)
        {
            _logger.LogDebug("Charge {TxId} already marked as paid — idempotent skip.", webhookEvent.TxId);
            return new ProcessWebhookResult(true);
        }

        var invoice = await _invoiceRepository.GetByIdAsync(charge.InvoiceId, ct);
        if (invoice is null)
        {
            _logger.LogError("Invoice not found for charge TxId={TxId}", webhookEvent.TxId);
            return new ProcessWebhookResult(false, "Invoice not found.");
        }

        // 5. Confirm payment in domain
        charge.ConfirmPayment(
            Domain.ValueObjects.Money.BRL(bankStatus.PaidAmount ?? 0),
            bankStatus.PaidAt ?? DateTimeOffset.UtcNow,
            bankStatus.PaymentId);

        invoice.MarkAsPaid(
            (bankStatus.PaidAt ?? DateTimeOffset.UtcNow).UtcDateTime,
            Domain.ValueObjects.Money.BRL(bankStatus.PaidAmount ?? 0));

        await _chargeRepository.UpdateAsync(charge, ct);
        await _invoiceRepository.UpdateAsync(invoice, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Payment confirmed: TxId={TxId}, Amount={Amount}",
            webhookEvent.TxId, bankStatus.PaidAmount);

        return new ProcessWebhookResult(true);
    }
}
