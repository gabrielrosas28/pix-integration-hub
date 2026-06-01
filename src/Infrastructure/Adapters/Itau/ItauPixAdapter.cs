using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Interfaces;
using Domain.Aggregates.PixCharge;
using Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Adapters.Itau;

/// <summary>
/// Adapter for Itaú bank Pix integration.
/// This is the ONLY class that knows Itaú's endpoints, headers, and payload formats.
/// The application core never calls Itaú directly.
/// </summary>
public sealed class ItauPixAdapter : IBankPixAdapter
{
    private readonly HttpClient _http;
    private readonly ItauOptions _options;
    private readonly IItauTokenProvider _tokenProvider;
    private readonly ILogger<ItauPixAdapter> _logger;

    public ItauPixAdapter(
        HttpClient http,
        IOptions<ItauOptions> options,
        IItauTokenProvider tokenProvider,
        ILogger<ItauPixAdapter> logger)
    {
        _http = http;
        _options = options.Value;
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    public string BankId => "ITAU";
    public bool SupportsCob => true;
    public bool SupportsCobV => true;

    private string BaseUrl => _options.UseSandbox
        ? _options.SandboxBaseUrl
        : _options.ProductionBaseUrl;

    // ---- CreateCobAsync ----

    public async Task<ChargeResponse> CreateCobAsync(ChargeRequest request, CancellationToken ct = default)
    {
        if (request.Type != PixChargeType.Cob)
            throw new ArgumentException("Expected charge type COB.");

        var url = $"{BaseUrl}/cob/{request.TxId}";

        var payload = new
        {
            chave = request.PixKey,
            valor = new { original = request.Amount.ToString("0.00", CultureInfo.InvariantCulture) },
            calendario = new { expiracao = request.ExpiresInSeconds ?? 3600 },
            solicitacaoPagador = request.PayerMessage
        };

        return await SendChargeRequestAsync(url, payload, request.TxId, ct);
    }

    // ---- CreateCobVAsync ----

    public async Task<ChargeResponse> CreateCobVAsync(ChargeRequest request, CancellationToken ct = default)
    {
        if (request.Type != PixChargeType.CobV)
            throw new ArgumentException("Expected charge type COBV.");

        if (request.DueDate is null)
            throw new ArgumentException("CobV requires a DueDate.");

        var url = $"{BaseUrl}/cobv/{request.TxId}";

        var payload = new
        {
            chave = request.PixKey,
            valor = new { original = request.Amount.ToString("0.00", CultureInfo.InvariantCulture) },
            calendario = new { dataDeVencimento = request.DueDate.Value.ToString("yyyy-MM-dd") },
            solicitacaoPagador = request.PayerMessage
        };

        return await SendChargeRequestAsync(url, payload, request.TxId, ct);
    }

    private async Task<ChargeResponse> SendChargeRequestAsync(
        string url, object payload, string txId, CancellationToken ct)
    {
        var token = await _tokenProvider.GetAccessTokenAsync(ct);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequest.Headers.Add("x-correlationID", Guid.NewGuid().ToString());
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var response = await _http.SendAsync(httpRequest, ct);
        var rawText = await response.Content.ReadAsStringAsync(ct);

        _logger.LogDebug("Itaú response: Status={Status}, Body={Body}",
            (int)response.StatusCode, rawText);

        if (!response.IsSuccessStatusCode)
            throw new BankIntegrationException(
                $"Itaú CreateCharge failed: {(int)response.StatusCode}", BankId,
                (int)response.StatusCode);

        var rawObj = JsonDocument.Parse(rawText).RootElement.Clone();
        return new ChargeResponse(
            TxId: txId,
            Status: MapItauStatus(rawObj),
            BankChargeId: txId,
            Raw: rawObj);
    }

    // ---- GetQrCodeAsync ----

    public async Task<QrCodeResponse> GetQrCodeAsync(
        string txId, PixChargeType type, CancellationToken ct = default)
    {
        // Itaú returns the EMV/QR Code within the charge response itself.
        // We query the charge endpoint to extract it.
        var path = type == PixChargeType.CobV ? $"/cobv/{txId}" : $"/cob/{txId}";
        var url = $"{BaseUrl}{path}";

        var token = await _tokenProvider.GetAccessTokenAsync(ct);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("x-correlationID", Guid.NewGuid().ToString());

        using var response = await _http.SendAsync(request, ct);
        var rawText = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new BankIntegrationException(
                $"Itaú GetQrCode failed: {(int)response.StatusCode}", BankId,
                (int)response.StatusCode);

        using var doc = JsonDocument.Parse(rawText);
        var root = doc.RootElement.Clone();

        var emv = root.TryGetProperty("pixCopiaECola", out var emvEl)
            ? emvEl.GetString() ?? string.Empty
            : string.Empty;

        var pixLink = root.TryGetProperty("location", out var locationEl)
            ? locationEl.GetString()
            : null;

        return new QrCodeResponse(TxId: txId, Emv: emv, PixLink: pixLink, Raw: root);
    }

    // ---- GetChargeStatusAsync ---- (BUG CORRIGIDO: paths estavam invertidos)

    public async Task<ChargeStatusResponse> GetChargeStatusAsync(
        string txId, PixChargeType type, CancellationToken ct = default)
    {
        // FIXED: CobV → /cobv/{txId}, Cob → /cob/{txId}
        var path = type switch
        {
            PixChargeType.CobV => $"/cobv/{txId}",
            PixChargeType.Cob  => $"/cob/{txId}",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        var url = $"{BaseUrl}{path}";
        var token = await _tokenProvider.GetAccessTokenAsync(ct);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("x-correlationID", Guid.NewGuid().ToString());

        using var response = await _http.SendAsync(request, ct);
        var rawText = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new BankIntegrationException(
                $"Itaú GetChargeStatus failed: {(int)response.StatusCode}", BankId,
                (int)response.StatusCode);

        using var doc = JsonDocument.Parse(rawText);
        var root = doc.RootElement.Clone();

        var status = MapItauStatus(root);

        decimal? paidAmount = null;
        DateTimeOffset? paidAt = null;
        string? paymentId = null;

        // Itaú returns a "pix" array when payment is confirmed
        if (root.TryGetProperty("pix", out var pixArr)
            && pixArr.ValueKind == JsonValueKind.Array
            && pixArr.GetArrayLength() > 0)
        {
            var firstPix = pixArr[0];

            if (firstPix.TryGetProperty("valor", out var valorEl))
                paidAmount = decimal.Parse(valorEl.GetString()!, CultureInfo.InvariantCulture);

            if (firstPix.TryGetProperty("horario", out var horarioEl))
                paidAt = DateTimeOffset.Parse(horarioEl.GetString()!);

            if (firstPix.TryGetProperty("endToEndId", out var e2eEl))
                paymentId = e2eEl.GetString();
        }

        return new ChargeStatusResponse(
            TxId: txId,
            Status: status,
            PaidAmount: paidAmount,
            PaidAt: paidAt,
            PaymentId: paymentId,
            Raw: root);
    }

    // ---- ValidateWebhook ----

    public bool ValidateWebhook(IReadOnlyDictionary<string, string> headers, JsonElement body)
    {
        // Basic validation: check for required Itaú headers
        // In production, implement full signature/mTLS validation per Itaú documentation
        if (!headers.ContainsKey("x-webhook-signature") && !headers.ContainsKey("user-agent"))
        {
            _logger.LogWarning("Webhook missing required Itaú headers.");
            return false;
        }

        // Ensure body has "pix" array
        return body.TryGetProperty("pix", out var pix) && pix.ValueKind == JsonValueKind.Array;
    }

    // ---- ParseWebhookEvent ----

    public WebhookEvent ParseWebhookEvent(JsonElement body)
    {
        string? txId = null;
        var eventType = WebhookEventType.Unknown;

        if (body.TryGetProperty("pix", out var pixArr)
            && pixArr.ValueKind == JsonValueKind.Array
            && pixArr.GetArrayLength() > 0)
        {
            var first = pixArr[0];

            if (first.TryGetProperty("txid", out var txIdEl))
                txId = txIdEl.GetString();

            // Itaú signals a completed payment when "endToEndId" is present
            if (first.TryGetProperty("endToEndId", out _))
                eventType = WebhookEventType.PaymentConfirmed;
        }

        return new WebhookEvent(
            TxId: txId,
            EventType: eventType,
            ReceivedAt: DateTimeOffset.UtcNow,
            Raw: body);
    }

    // ---- Status mapping ----

    private static PixChargeStatus MapItauStatus(JsonElement root)
    {
        if (!root.TryGetProperty("status", out var statusEl))
            return PixChargeStatus.Unknown;

        return statusEl.GetString()?.ToUpperInvariant() switch
        {
            "ATIVA" or "ACTIVE"                    => PixChargeStatus.Active,
            "CONCLUIDA" or "CONCLUIDO"             => PixChargeStatus.Paid,
            "REMOVIDA_PELO_USUARIO_RECEBEDOR"      => PixChargeStatus.Canceled,
            "REMOVIDA_PELO_PSP"                    => PixChargeStatus.Canceled,
            "EXPIRADA"                             => PixChargeStatus.Expired,
            _                                      => PixChargeStatus.Unknown
        };
    }
}

public sealed class BankIntegrationException : Exception
{
    public string BankId { get; }
    public int StatusCode { get; }

    public BankIntegrationException(string message, string bankId, int statusCode = 0)
        : base(message)
    {
        BankId = bankId;
        StatusCode = statusCode;
    }
}
