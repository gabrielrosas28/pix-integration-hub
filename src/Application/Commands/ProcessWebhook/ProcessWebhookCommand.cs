using MediatR;
using System.Text.Json;

namespace BankingHub.Application.Commands.ProcessWebhook;


public sealed record ProcessWebhookCommand(
    string BankId,
    IReadOnlyDictionary<string, string> Headers,
    JsonElement Body) : IRequest<ProcessWebhookResult>;

public sealed record ProcessWebhookResult(
    bool Accepted,
    string? TxId,
    string Message);
