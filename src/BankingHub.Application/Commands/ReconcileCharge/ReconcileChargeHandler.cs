using BankingHub.Application.Common.Exceptions;
using BankingHub.Application.Interfaces;
using BankingHub.Domain.Aggregates.PixCharge;
using BankingHub.Domain.Repositories;
using BankingHub.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankingHub.Application.Commands.ReconcileCharge;

public sealed class ReconcileChargeHandler
    : IRequestHandler<ReconcileChargeCommand, ReconcileChargeResult>
{
    private readonly IPixChargeRepository _chargeRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IBankAdapterFactory _adapterFactory;
    private readonly ILogger<ReconcileChargeHandler> _logger;

    public ReconcileChargeHandler(
        IPixChargeRepository chargeRepository,
        IInvoiceRepository invoiceRepository,
        IBankAdapterFactory adapterFactory,
        ILogger<ReconcileChargeHandler> logger)
    {
        _chargeRepository = chargeRepository;
        _invoiceRepository = invoiceRepository;
        _adapterFactory = adapterFactory;
        _logger = logger;
    }

    public async Task<ReconcileChargeResult> Handle(
        ReconcileChargeCommand cmd,
        CancellationToken ct)
    {
        var charge = await _chargeRepository.GetByTxIdAsync(TxId.From(cmd.TxId), ct)
            ?? throw new NotFoundException("PixCharge", cmd.TxId);

        // Idempotency: if already paid, just return current state
        if (charge.Status == PixChargeStatus.Paid)
        {
            _logger.LogDebug("Charge already paid: TxId={TxId}", cmd.TxId);
            return new ReconcileChargeResult(
                charge.Status.ToString(), true, charge.PaidAt, charge.PaidAmount?.Value);
        }

        var bankId = cmd.BankId ?? charge.BankId;
        var adapter = _adapterFactory.Get(bankId);

        var bankStatus = await adapter.GetChargeStatusAsync(cmd.TxId, charge.ChargeType, ct);

        _logger.LogDebug(
            "Bank-side status for TxId={TxId}: {Status}",
            cmd.TxId, bankStatus.Status);

        if (bankStatus.Status != PixChargeStatus.Paid)
        {
            return new ReconcileChargeResult(
                bankStatus.Status.ToString(), false, null, null);
        }

        // Payment confirmed: update domain
        var invoice = await _invoiceRepository.GetByIdAsync(charge.InvoiceId, ct)
            ?? throw new InvalidOperationException(
                $"Invoice {charge.InvoiceId} not found for paid charge {cmd.TxId}");

        var paidAmount = Money.BRL(bankStatus.PaidAmount ?? 0);
        var paidAt = bankStatus.PaidAt?.UtcDateTime ?? DateTime.UtcNow;

        charge.MarkAsPaid(paidAmount, paidAt);
        invoice.MarkAsPaid(paidAt, paidAmount);

        _chargeRepository.Update(charge);
        _invoiceRepository.Update(invoice);

        _logger.LogInformation(
            "Reconciliation completed: TxId={TxId}, PaidAmount={Amount}",
            cmd.TxId, paidAmount);

        return new ReconcileChargeResult(
            PixChargeStatus.Paid.ToString(), true, paidAt, paidAmount.Value);
    }
}
