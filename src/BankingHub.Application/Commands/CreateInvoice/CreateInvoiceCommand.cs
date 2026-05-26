using MediatR;

namespace BankingHub.Application.Commands.CreateInvoice;

/// <summary>
/// Creates a new Invoice in the system. The Invoice is the unit of charge
/// from the merchant's perspective; one or more PixCharges may be issued
/// against it later.
/// </summary>
public sealed record CreateInvoiceCommand(
    decimal Amount,
    DateOnly DueDate,
    string BankId,
    string? ExternalReference) : IRequest<CreateInvoiceResult>;

public sealed record CreateInvoiceResult(
    Guid InvoiceId,
    string Status);
