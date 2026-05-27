using System.Text.Json;
using BankingHub.Domain.Aggregates.PixCharge;

namespace BankingHub.Application.Interfaces;

/// <summary>
/// Bank adapter contract. This interface is stable and must not change when a new
/// bank is added. Each bank implementation translates its specifics to the
/// normalized DTOs below.
/// </summary>
public interface IBankPixAdapter
{
    /// <summary>Unique bank identifier (e.g. "ITAU", "BB", "BRADESCO").</summary>
    string BankId { get; }

    /// <summary>Indicates whether the bank supports Cob (immediate charge).</summary>
    bool SupportsCob { get; }

    /// <summary>Indicates whether the bank supports CobV (charge with due date).</summary>
    bool SupportsCobV { get; }

    /// <summary>Creates an immediate Pix charge (Cob).</summary>
    Task<ChargeResponse> CreateCobAsync(ChargeRequest request, CancellationToken ct);

    /// <summary>Creates a Pix charge with due date (CobV).</summary>
    Task<ChargeResponse> CreateCobVAsync(ChargeRequest request, CancellationToken ct);

    /// <summary>Retrieves the QR Code (EMV + Pix Link) of a charge.</summary>
    Task<QrCodeResponse> GetQrCodeAsync(string txId, PixChargeType type, CancellationToken ct);

    /// <summary>
    /// Queries the current status of a charge.
    /// This is the method that definitively validates whether a Pix was paid.
    /// </summary>
    Task<ChargeStatusResponse> GetChargeStatusAsync(string txId, PixChargeType type, CancellationToken ct);

    /// <summary>Validates the authenticity of a webhook (signature, mTLS, IP, etc).</summary>
    bool ValidateWebhook(IReadOnlyDictionary<string, string> headers, JsonElement body);

    /// <summary>
    /// Parses the webhook payload into a normalized event.
    /// IMPORTANT: Do not trust this event for settlement — always validate via GetChargeStatusAsync.
    /// </summary>
    WebhookEvent ParseWebhookEvent(JsonElement body);
}

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

public enum WebhookEventType
{
    PaymentConfirmed = 0,
    PaymentCanceled = 1,
    Unknown = 99
}

// Enums consumed by this contract — defined in the Domain to keep the
// Clean Architecture dependency rule (Application -> Domain only):
//   - BankingHub.Domain.Aggregates.PixCharge.PixChargeType   { Cob, CobV }
//   - BankingHub.Domain.Aggregates.PixCharge.PixChargeStatus { Active, Paid, Expired, Canceled, Unknown }
// See §6.2 of the architecture doc, which shows them here for readability,
// and §4.2, which locates them in the Domain.

