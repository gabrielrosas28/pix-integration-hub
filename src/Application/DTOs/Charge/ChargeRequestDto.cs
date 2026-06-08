namespace BankingHub.Application.DTOs;


public sealed record ChargeRequestDto(
    Guid InvoiceId,
    string ChargeType,
    decimal Amount,
    string PixKey,
    DateOnly? DueDate,
    int? ExpiresInSeconds,
    string? PayerMessage);
