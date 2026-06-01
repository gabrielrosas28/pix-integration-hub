using MediatR;

namespace BankingHub.Application.Commands.CreateInvoice;


public sealed record CreateInvoiceCommand(
    decimal Amount,
    DateOnly DueDate,
    string BankId,
    string? ExternalReference = null) : IRequest<CreateInvoiceResult>;

public sealed record CreateInvoiceResult(
    Guid InvoiceId,
    string Status,
    decimal Amount,
    DateOnly DueDate,
    string BankId);
