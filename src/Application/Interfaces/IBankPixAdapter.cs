using System.Text.Json;
using Domain.Aggregates.PixCharge;

namespace Application.Interfaces;

/// <summary>
/// Contract for bank adapters.
/// RULE: This interface must remain stable when adding new banks.
/// Each bank implements it, translating their specifics into normalized DTOs.
/// </summary>
public interface IBankPixAdapter
{
    string BankId { get; }
    bool SupportsCob { get; }
    bool SupportsCobV { get; }

    Task<ChargeResponse> CreateCobAsync(ChargeRequest request, CancellationToken ct = default);
    Task<ChargeResponse> CreateCobVAsync(ChargeRequest request, CancellationToken ct = default);
    Task<QrCodeResponse> GetQrCodeAsync(string txId, PixChargeType type, CancellationToken ct = default);

    /// <summary>
    /// Queries the bank for the definitive payment status.
    /// CRITICAL: This is the only authoritative confirmation of payment.
    /// </summary>
    Task<ChargeStatusResponse> GetChargeStatusAsync(string txId, PixChargeType type, CancellationToken ct = default);

    bool ValidateWebhook(IReadOnlyDictionary<string, string> headers, JsonElement body);
    WebhookEvent ParseWebhookEvent(JsonElement body);
}

public interface IBankAdapterFactory
{
    IBankPixAdapter Get(string bankId);
    IReadOnlyCollection<string> GetAvailableBanks();
    bool IsSupported(string bankId);
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}

// Normalized DTOs shared between Application and Infrastructure layers

public sealed record ChargeRequest(
    string TxId,
    PixChargeType Type,
    decimal Amount,
    string PixKey,
    DateOnly? DueDate,
    int? ExpiresInSeconds,
    string? PayerMessage);

public sealed record ChargeResponse(
    string TxId,
    PixChargeStatus Status,
    string? BankChargeId,
    object Raw);

public sealed record QrCodeResponse(
    string TxId,
    string Emv,
    string? PixLink,
    object? Raw = null);

public sealed record ChargeStatusResponse(
    string TxId,
    PixChargeStatus Status,
    decimal? PaidAmount,
    DateTimeOffset? PaidAt,
    string? PaymentId,
    object Raw);

public sealed record WebhookEvent(
    string? TxId,
    WebhookEventType EventType,
    DateTimeOffset ReceivedAt,
    object Raw);

public enum WebhookEventType { PaymentConfirmed, PaymentCanceled, Unknown }
