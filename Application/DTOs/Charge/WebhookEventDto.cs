namespace BankingHub.Application.DTOs;


public sealed record WebhookEventDto(
    string BankId,
    IReadOnlyDictionary<string, string> Headers,
    string RawBody);
