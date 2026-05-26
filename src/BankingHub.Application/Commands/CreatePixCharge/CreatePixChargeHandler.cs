using BankingHub.Application.Common.Exceptions;
using BankingHub.Application.Interfaces;
using BankingHub.Domain.Aggregates.Invoice;
using BankingHub.Domain.Aggregates.PixCharge;
using BankingHub.Domain.Repositories;
using BankingHub.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using ValidationException = BankingHub.Application.Common.Exceptions.ValidationException;

namespace BankingHub.Application.Commands.CreatePixCharge;

/// <summary>
/// Handler that processes the Pix charge creation command.
/// Orchestrates the application logic without containing business rules
/// (those live inside the Invoice and PixCharge aggregates).
/// </summary>
public sealed class CreatePixChargeHandler
    : IRequestHandler<CreatePixChargeCommand, CreatePixChargeResult>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPixChargeRepository _chargeRepository;
    private readonly IBankAdapterFactory _adapterFactory;
    private readonly ILogger<CreatePixChargeHandler> _logger;

    public CreatePixChargeHandler(
        IInvoiceRepository invoiceRepository,
        IPixChargeRepository chargeRepository,
        IBankAdapterFactory adapterFactory,
        ILogger<CreatePixChargeHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _chargeRepository = chargeRepository;
        _adapterFactory = adapterFactory;
        _logger = logger;
    }

    public async Task<CreatePixChargeResult> Handle(
        CreatePixChargeCommand cmd,
        CancellationToken ct)
    {
        // 1. Fetch the Invoice
        var invoice = await _invoiceRepository.GetByIdAsync(
            InvoiceId.From(cmd.InvoiceId), ct)
            ?? throw new NotFoundException("Invoice", cmd.InvoiceId);

        // 2. Generate unique TxId
        var txId = TxId.Generate();

        // 3. Resolve the adapter for the invoice's bank
        var adapter = _adapterFactory.Get(invoice.BankId);

        // 4. Determine charge type
        var chargeType = cmd.ChargeType.ToUpperInvariant() switch
        {
            "COB" => PixChargeType.Cob,
            "COBV" => PixChargeType.CobV,
            _ => throw new ValidationException("Invalid charge type. Expected 'COB' or 'COBV'")
        };

        // 5. Build normalized request
        var chargeRequest = new ChargeRequest(
            TxId: txId.Value,
            Type: chargeType,
            Amount: cmd.Amount,
            PixKey: cmd.PixKey,
            DueDate: cmd.DueDate,
            ExpiresInSeconds: cmd.ExpiresInSeconds,
            PayerMessage: cmd.PayerMessage);

        // 6. Create the charge in the bank through the adapter
        ChargeResponse bankResponse;
        try
        {
            bankResponse = chargeType == PixChargeType.CobV
                ? await adapter.CreateCobVAsync(chargeRequest, ct)
                : await adapter.CreateCobAsync(chargeRequest, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating charge in bank {BankId}", invoice.BankId);
            throw new IntegrationException($"Bank communication failed for {invoice.BankId}", ex);
        }

        // 7. Get QR Code
        var qrCode = await adapter.GetQrCodeAsync(txId.Value, chargeType, ct);

        // 8. Persist the charge in the domain
        var pixCharge = PixCharge.Create(
            txId: txId,
            invoiceId: invoice.Id,
            bankId: invoice.BankId,
            chargeType: chargeType,
            emv: EmvCode.From(qrCode.Emv),
            pixLink: qrCode.PixLink,
            rawPayload: bankResponse.Raw);

        await _chargeRepository.AddAsync(pixCharge, ct);

        // 9. Associate TxId with the Invoice
        invoice.AssignTxId(txId);
        _invoiceRepository.Update(invoice);

        _logger.LogInformation(
            "Pix charge created: TxId={TxId}, Invoice={InvoiceId}, Bank={BankId}",
            txId, invoice.Id, invoice.BankId);

        return new CreatePixChargeResult(
            TxId: txId.Value,
            Status: bankResponse.Status.ToString(),
            Emv: qrCode.Emv,
            PixLink: qrCode.PixLink);
    }
}
