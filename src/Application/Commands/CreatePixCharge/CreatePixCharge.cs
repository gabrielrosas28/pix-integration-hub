using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Aggregates.Invoice;
using Domain.Aggregates.PixCharge;
using Domain.ValueObjects;
using Application.Interfaces;

namespace Application.Commands.CreatePixCharge;

// ---- Command ----

public sealed record CreatePixChargeCommand(
    Guid InvoiceId,
    string ChargeType,       // "COB" or "COBV"
    decimal Amount,
    string PixKey,
    DateOnly? DueDate,
    int? ExpiresInSeconds,
    string? PayerMessage) : IRequest<CreatePixChargeResult>;

public sealed record CreatePixChargeResult(
    string TxId,
    string Status,
    string Emv,
    string? PixLink);

// ---- Handler ----

public sealed class CreatePixChargeHandler
    : IRequestHandler<CreatePixChargeCommand, CreatePixChargeResult>
{
    private readonly Domain.Repositories.IInvoiceRepository _invoiceRepository;
    private readonly Domain.Repositories.IPixChargeRepository _chargeRepository;
    private readonly IBankAdapterFactory _adapterFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePixChargeHandler> _logger;

    public CreatePixChargeHandler(
        Domain.Repositories.IInvoiceRepository invoiceRepository,
        Domain.Repositories.IPixChargeRepository chargeRepository,
        IBankAdapterFactory adapterFactory,
        IUnitOfWork unitOfWork,
        ILogger<CreatePixChargeHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _chargeRepository = chargeRepository;
        _adapterFactory = adapterFactory;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CreatePixChargeResult> Handle(
        CreatePixChargeCommand cmd,
        CancellationToken ct)
    {
        // 1. Load the invoice
        var invoice = await _invoiceRepository.GetByIdAsync(
            InvoiceId.From(cmd.InvoiceId), ct)
            ?? throw new KeyNotFoundException("Invoice not found.");

        // 2. Generate a unique TxId
        var txId = TxId.Generate();

        // 3. Get the bank adapter
        var adapter = _adapterFactory.Get(invoice.BankId);

        // 4. Determine charge type
        var chargeType = cmd.ChargeType.ToUpperInvariant() switch
        {
            "COB"  => PixChargeType.Cob,
            "COBV" => PixChargeType.CobV,
            _ => throw new ArgumentException($"Invalid charge type: {cmd.ChargeType}. Use 'COB' or 'COBV'.")
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

        // 6. Create the charge at the bank via adapter
        ChargeResponse bankResponse;
        try
        {
            bankResponse = chargeType == PixChargeType.CobV
                ? await adapter.CreateCobVAsync(chargeRequest, ct)
                : await adapter.CreateCobAsync(chargeRequest, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create charge at bank {BankId}", invoice.BankId);
            throw new InvalidOperationException($"Communication failure with bank {invoice.BankId}.", ex);
        }

        // 7. Fetch QR Code
        var qrCode = await adapter.GetQrCodeAsync(txId.Value, chargeType, ct);

        // 8. Persist the charge in the domain
        var pixCharge = PixCharge.Create(
            txId: txId,
            invoiceId: invoice.Id,
            bankId: invoice.BankId,
            chargeType: chargeType,
            amount: Money.BRL(cmd.Amount),
            pixKey: cmd.PixKey,
            dueDate: cmd.DueDate.HasValue ? cmd.DueDate.Value.ToDateTime(TimeOnly.MinValue) : null,
            expiresInSeconds: cmd.ExpiresInSeconds,
            payerMessage: cmd.PayerMessage ?? string.Empty,
            emv: EmvCode.From(qrCode.Emv),
            pixLink: qrCode.PixLink,
            raw: bankResponse.Raw?.ToString() ?? string.Empty);

        await _chargeRepository.AddAsync(pixCharge, ct);

        // 9. Associate TxId with the invoice
        invoice.AssignTxId(txId);
        await _invoiceRepository.UpdateAsync(invoice, ct);

        // 10. Commit
        await _unitOfWork.SaveChangesAsync(ct);

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

// ---- Validator ----

public sealed class CreatePixChargeValidator : AbstractValidator<CreatePixChargeCommand>
{
    public CreatePixChargeValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty()
            .WithMessage("InvoiceId is required.");

        RuleFor(x => x.ChargeType)
            .NotEmpty()
            .Must(x => x.ToUpperInvariant() is "COB" or "COBV")
            .WithMessage("ChargeType must be 'COB' or 'COBV'.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.")
            .PrecisionScale(10, 2, true)
            .WithMessage("Amount must have at most 2 decimal places.");

        RuleFor(x => x.PixKey)
            .NotEmpty()
            .MaximumLength(77)
            .WithMessage("PixKey is required and must not exceed 77 characters.");

        RuleFor(x => x.DueDate)
            .Must(date => date is null || date >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("DueDate cannot be in the past.")
            .When(x => x.ChargeType.ToUpperInvariant() == "COBV");

        RuleFor(x => x.ExpiresInSeconds)
            .InclusiveBetween(60, 86400)
            .When(x => x.ExpiresInSeconds.HasValue)
            .WithMessage("ExpiresInSeconds must be between 60 and 86400.");

        RuleFor(x => x.PayerMessage)
            .MaximumLength(140)
            .When(x => !string.IsNullOrEmpty(x.PayerMessage))
            .WithMessage("PayerMessage must not exceed 140 characters.");
    }
}
