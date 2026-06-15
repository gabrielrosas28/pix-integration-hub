using System.Text.Json;

namespace BankingHub.Application.Interfaces;

/// <summary>
/// Contrato do Adapter Bancário.
/// REGRA: Esta interface é estável e não deve mudar ao adicionar novos bancos.
/// Cada banco implementa esta interface traduzindo suas peculiaridades
/// para os DTOs normalizados.
/// </summary>
public interface IBankPixAdapter
{
    /// <summary>Identificador único do banco (ex: "ITAU", "BB", "BRADESCO").</summary>
    string BankId { get; }

    /// <summary>Indica se o banco suporta Cob (cobrança imediata).</summary>
    bool SupportsCob { get; }

    /// <summary>Indica se o banco suporta CobV (cobrança com vencimento).</summary>
    bool SupportsCobV { get; }

    /// <summary>Cria uma cobrança Pix imediata (Cob).</summary>
    Task<ChargeResponse> CreateCobAsync(ChargeRequest request, CancellationToken ct);

    /// <summary>Cria uma cobrança Pix com vencimento (CobV).</summary>
    Task<ChargeResponse> CreateCobVAsync(ChargeRequest request, CancellationToken ct);

    /// <summary>Obtém o QR Code (EMV + Pix Link) de uma cobrança.</summary>
    Task<QrCodeResponse> GetQrCodeAsync(string txId, PixChargeType type, CancellationToken ct);

    /// <summary>
    /// Consulta o status atual de uma cobrança.
    /// Este é o método que valida definitivamente se um Pix foi pago.
    /// </summary>
    Task<ChargeStatusResponse> GetChargeStatusAsync(string txId, PixChargeType type, CancellationToken ct);

    /// <summary>Valida autenticidade do webhook (assinatura, mTLS, IP, etc).</summary>
    bool ValidateWebhook(IReadOnlyDictionary<string, string> headers, JsonElement body);

    /// <summary>
    /// Parseia o payload do webhook para evento normalizado.
    /// IMPORTANTE: Não confiar neste evento para baixa — sempre validar via GetChargeStatusAsync.
    /// </summary>
    WebhookEvent ParseWebhookEvent(JsonElement body);
}

// IBankAdapterFactory está definida em IBankAdapterFactory.cs (mesmo namespace).

/// <summary>
/// Serviço de notificações em tempo real (SignalR).
/// </summary>
public interface INotificationService
{
    Task NotifyPaymentConfirmedAsync(
        string invoiceId,
        string txId,
        decimal amount,
        CancellationToken ct);
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



public enum PixChargeType    { Cob, CobV }
public enum PixChargeStatus  { Active, Paid, Expired, Canceled, Unknown }
public enum WebhookEventType { PaymentConfirmed, PaymentCanceled, Unknown }
