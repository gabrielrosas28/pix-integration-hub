using System.Text.Json;
using MediatR;

namespace BankingHub.Application.Commands.ProcessWebhook;

/// <summary>
/// Receives an incoming webhook payload from a bank.
/// IMPORTANT: per architecture rule §3.1.2 the webhook is ONLY a trigger.
/// Settlement is always confirmed via an active query (reconciliation).
/// </summary>
public sealed record ProcessWebhookCommand(
    string BankId,
    IReadOnlyDictionary<string, string> Headers,
    JsonElement Body) : IRequest<ProcessWebhookResult>;

public sealed record ProcessWebhookResult(
    bool Accepted,
    string? TxId,
    string Reason);
