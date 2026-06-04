using FluentValidation;
using BankingHub.Application.Commands.CreateInvoice;

namespace BankingHub.Application.Commands.CreateInvoice;


public sealed class CreateInvoiceValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount deve ser maior que zero.")
            .PrecisionScale(10, 2, true)
            .WithMessage("Amount deve ter no máximo 2 casas decimais.");

        RuleFor(x => x.DueDate)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("DueDate não pode ser no passado.");

        RuleFor(x => x.BankId)
            .NotEmpty()
            .WithMessage("BankId é obrigatório.");
    }
}
