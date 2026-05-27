using BankingHub.Application.Exceptions;
using BankingHub.Application.Interfaces;
using BankingHub.Domain.Aggregates.PixCharge;
using BankingHub.Domain.Repositories;
using BankingHub.Domain.Services;
using BankingHub.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BankingHub.Application.Services;

/// <summary>
/// Implementation of <see cref="IPixReconciliationService"/>.
/// Owns the only path that may transition an Invoice/PixCharge to Paid (§3.1.2).
/// </summary>
public sealed class PixReconciliationService : IPixReconciliationService
{
    private readonly IPixChargeRepository _chargeRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IBankAdapterFactory _adapterFactory;
    private readonly ILogger<PixReconciliationService> _logger;

    public PixReconciliationService(
        IPixChargeRepository chargeRepository,
        IInvoiceRepository invoiceRepository,
        IBankAdapterFactory adapterFactory,
        ILogger<PixReconciliationService> logger)
    {
        _chargeRepository = chargeRepository;
        _invoiceRepository = invoiceRepository;
        _adapterFactory = adapterFactory;
        _logger = logger;
    }

    public async Task<ReconciliationResult> ReconcileAsync(
        string txId,
        string? bankId,
        CancellationToken ct)
    {
        var charge = await _chargeRepository.GetByTxIdAsync(TxId.From(txId), ct)
            ?? throw new NotFoundException("PixCharge", txId);

        if (charge.Status == PixChargeStatus.Paid)
        {
            _logger.LogDebug("Charge already paid: TxId={TxId}", txId);
            return new ReconciliationResult(
                charge.Status.ToString(), true, charge.PaidAt, charge.PaidAmount?.Value);
        }

        var resolvedBankId = bankId ?? charge.BankId;
        var adapter = _adapterFactory.Get(resolvedBankId);

        var bankStatus = await adapter.GetChargeStatusAsync(txId, charge.ChargeType, ct);

        _logger.LogDebug(
            "Bank-side status for TxId={TxId}: {Status}",
            txId, bankStatus.Status);

        if (bankStatus.Status != PixChargeStatus.Paid)
        {
            return new ReconciliationResult(
                bankStatus.Status.ToString(), false, null, null);
        }

        var invoice = await _invoiceRepository.GetByIdAsync(charge.InvoiceId, ct)
            ?? throw new InvalidOperationException(
                $"Invoice {charge.InvoiceId} not found for paid charge {txId}");

        var paidAmount = Money.BRL(bankStatus.PaidAmount ?? 0);
        var paidAt = bankStatus.PaidAt?.UtcDateTime ?? DateTime.UtcNow;

        charge.MarkAsPaid(paidAmount, paidAt);
        invoice.MarkAsPaid(paidAt, paidAmount);

        _chargeRepository.Update(charge);
        _invoiceRepository.Update(invoice);

        _logger.LogInformation(
            "Reconciliation completed: TxId={TxId}, PaidAmount={Amount}",
            txId, paidAmount);

        return new ReconciliationResult(
            PixChargeStatus.Paid.ToString(), true, paidAt, paidAmount.Value);
    }
}
