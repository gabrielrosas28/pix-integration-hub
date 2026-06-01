using System.Text.Json;

namespace BankingHub.Application.Interfaces;

/// 
/// Contrato do Adapter Bancário.
/// REGRA: Esta interface é estável e não deve mudar ao adicionar novos
/// Cada banco implementa esta interface traduzindo suas peculiaridades
/// para os DTOs normalizados.
/// 


public interface IBankPixAdapter
{
/// 
/// Identificador único do banco (ex: "ITAU", "BB", "BRADESCO").
/// 


string BankId { get; }

/// 
/// Indica se o banco suporta Cob (cobrança imediata).
/// 


bool SupportsCob { get; }

/// 
/// Indica se o banco suporta CobV (cobrança com vencimento).
/// 


bool SupportsCobV { get; }

/// 
/// Cria uma cobrança Pix imediata (Cob).
/// 

Task CreateCobAsync(ChargeRequest request, Cancella

/// 
/// Cria uma cobrança Pix com vencimento (CobV).
/// 

Task CreateCobVAsync(ChargeRequest request, Cancell

/// 
/// Obtém o QR Code (EMV + Pix Link) de uma cobrança.
/// 

Task GetQrCodeAsync(string txId, PixChargeType typ

/// 
7. CAMADA DE INFRAESTRUTURA (INFRASTRUCTURE)
7.1 Implementação do Adapter Itaú
/// Consulta o status atual de uma cobrança.
/// Este é o método que valida definitivamente se um Pix foi pago.
/// 

Task GetChargeStatusAsync(string txId, PixCha

/// 
/// Valida autenticidade do webhook (assinatura, mTLS, IP, etc).
/// 

bool ValidateWebhook(IReadOnlyDictionary headers, J

/// 
/// Parseia o payload do webhook para evento normalizado.
/// IMPORTANTE: Não confiar neste evento para baixa - sempre valida
/// 

WebhookEvent ParseWebhookEvent(JsonElement body);
}

// DTOs Normalizados
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

public enum PixChargeType { Cob, CobV }
public enum PixChargeStatus { Active, Paid, Expired, Canceled, Unknown }
public enum WebhookEventType { PaymentConfirmed, PaymentCanceled, Unknown }