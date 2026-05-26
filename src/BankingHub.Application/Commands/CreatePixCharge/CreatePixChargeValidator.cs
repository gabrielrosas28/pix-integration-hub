using FluentValidation;

namespace BankingHub.Application.Commands.CreatePixCharge;

/// <summary>
/// Validator for the Pix charge creation command.
/// Runs format and simple business-rule validations.
/// Complex domain validations live in the aggregate.
/// </summary>
public sealed class CreatePixChargeValidator : AbstractValidator<CreatePixChargeCommand>
{
    public CreatePixChargeValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty()
            .WithMessage("InvoiceId is required");

        RuleFor(x => x.ChargeType)
            .NotEmpty()
            .Must(x => x.ToUpperInvariant() is "COB" or "COBV")
            .WithMessage("ChargeType must be 'COB' or 'COBV'");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero")
            .PrecisionScale(10, 2, true)
            .WithMessage("Amount must have at most 2 decimal places");

        RuleFor(x => x.PixKey)
            .NotEmpty()
            .MaximumLength(77)
            .WithMessage("PixKey is required and must be at most 77 characters");

        RuleFor(x => x.DueDate)
            .Must(date => date is null || date >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("DueDate cannot be in the past")
            .When(x => x.ChargeType.ToUpperInvariant() == "COBV");

        RuleFor(x => x.ExpiresInSeconds)
            .InclusiveBetween(60, 86400)
            .When(x => x.ExpiresInSeconds.HasValue)
            .WithMessage("ExpiresInSeconds must be between 60 and 86400");

        RuleFor(x => x.PayerMessage)
            .MaximumLength(140)
            .When(x => !string.IsNullOrEmpty(x.PayerMessage))
            .WithMessage("PayerMessage must be at most 140 characters");
    }
}
