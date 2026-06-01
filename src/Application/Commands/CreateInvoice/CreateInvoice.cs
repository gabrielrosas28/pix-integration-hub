using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Aggregates.Invoice;
using Domain.Repositories;
using Domain.ValueObjects;
using Application.Interfaces;

namespace Application.Commands.CreateInvoice;

// ---- Command ----

public sealed record CreateInvoiceCommand(
    decimal Amount,
    DateOnly DueDate,
    string BankId,
    string? ExternalReference = null) : IRequest<CreateInvoiceResult>;

public sealed record CreateInvoiceResult(
    Guid InvoiceId,
    string Status,
    decimal Amount,
    DateOnly DueDate);

// ---- Handler ----

public sealed class CreateInvoiceHandler
    : IRequestHandler<CreateInvoiceCommand, CreateInvoiceResult>
{
    private readonly IInvoiceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateInvoiceHandler> _logger;

    public CreateInvoiceHandler(
        IInvoiceRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateInvoiceHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CreateInvoiceResult> Handle(
        CreateInvoiceCommand cmd,
        CancellationToken ct)
    {
        var invoice = Invoice.Create(
            amount: Money.BRL(cmd.Amount),
            dueDate: cmd.DueDate,
            bankId: cmd.BankId,
            externalReference: cmd.ExternalReference);

        await _repository.AddAsync(invoice, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Invoice created: Id={InvoiceId}, Amount={Amount}, Bank={BankId}",
            invoice.Id, invoice.Amount, invoice.BankId);

        return new CreateInvoiceResult(
            InvoiceId: invoice.Id.Value,
            Status: invoice.Status.ToString(),
            Amount: invoice.Amount.Value,
            DueDate: invoice.DueDate);
    }
}

// ---- Validator ----

public sealed class CreateInvoiceValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.")
            .PrecisionScale(10, 2, true)
            .WithMessage("Amount must have at most 2 decimal places.");

        RuleFor(x => x.DueDate)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Due date cannot be in the past.");

        RuleFor(x => x.BankId)
            .NotEmpty()
            .WithMessage("BankId is required.");
    }
}
