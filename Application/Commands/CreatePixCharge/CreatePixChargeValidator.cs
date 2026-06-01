using FluentValidation;
using BankingHub.Application.Commands.CreatePixCharge;

namespace BankingHub.Application.Commands.CreatePixCharge;


public sealed class CreatePixChargeValidator : AbstractValidator<CreatePixChargeCommand>
{
    public CreatePixChargeValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty()
            .WithMessage("InvoiceId é obrigatório.");

        RuleFor(x => x.ChargeType)
            .NotEmpty()
            .Must(x => x.ToUpperInvariant() is "COB" or "COBV")
            .WithMessage("ChargeType deve ser 'COB' ou 'COBV'.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount deve ser maior que zero.")
            .PrecisionScale(10, 2, true)
            .WithMessage("Amount deve ter no máximo 2 casas decimais.");

        RuleFor(x => x.PixKey)
            .NotEmpty()
            .MaximumLength(77)
            .WithMessage("PixKey é obrigatória e deve ter no máximo 77 caracteres.");

        RuleFor(x => x.DueDate)
            .Must(date => date is null || date >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("DueDate não pode ser no passado.")
            .When(x => x.ChargeType.ToUpperInvariant() == "COBV");

        RuleFor(x => x.ExpiresInSeconds)
            .InclusiveBetween(60, 86400)
            .When(x => x.ExpiresInSeconds.HasValue)
            .WithMessage("ExpiresInSeconds deve estar entre 60 e 86400 segundos.");

        RuleFor(x => x.PayerMessage)
            .MaximumLength(140)
            .When(x => !string.IsNullOrEmpty(x.PayerMessage))
            .WithMessage("PayerMessage deve ter no máximo 140 caracteres.");
    }
}
