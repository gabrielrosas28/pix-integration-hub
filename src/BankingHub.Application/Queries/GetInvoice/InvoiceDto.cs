namespace BankingHub.Application.Queries.GetInvoice;

public sealed record InvoiceDto(
    Guid Id,
    decimal Amount,
    string Currency,
    DateOnly DueDate,
    string Status,
    string? TxId,
    string? ExternalReference,
    DateTime CreatedAt,
    DateTime? PaidAt,
    string BankId);
