using FluentValidation;

namespace BankingHub.Application.Commands.CreateInvoice;

public sealed class CreateInvoiceValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero")
            .PrecisionScale(10, 2, true)
            .WithMessage("Amount must have at most 2 decimal places");

        RuleFor(x => x.DueDate)
            .Must(date => date >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("DueDate cannot be in the past");

        RuleFor(x => x.BankId)
            .NotEmpty()
            .WithMessage("BankId is required");

        RuleFor(x => x.ExternalReference)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.ExternalReference))
            .WithMessage("ExternalReference must be at most 100 characters");
    }
}
