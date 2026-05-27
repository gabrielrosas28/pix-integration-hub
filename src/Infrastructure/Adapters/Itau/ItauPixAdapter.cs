using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Globalization;
using BankingHub.Application.Interfaces;

namespace Infrastructure.BankAdapters.Itau;

/// <summary>
/// Adapter para integração com o Banco Itaú - Pix Recebimentos.
/// Esta classe conhece os endpoints, headers e payloads específicos do Itaú.
/// </summary>
public sealed class ItauPixAdapter : IBankPixAdapter
{
    private readonly HttpClient _http;
    private readonly ItauOptions _options;
    private readonly IItauTokenProvider _tokenProvider;

    public ItauPixAdapter(
        HttpClient http,
        ItauOptions options,
        IItauTokenProvider tokenProvider)
    {
        _http = http;
        _options = options;
        _tokenProvider = tokenProvider;
    }

    public string BankId => "ITAU";
    public bool SupportsCob => true;
    public bool SupportsCobV => true;
    private string BaseUrl => _options.UseSandbox 
        ? _options.SandboxBaseUrl 
        : _options.ProdutionBaseUrl;

    public Task<ChargeResponse> CreateCobAsync(ChargeRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("CreateCobAsync ainda não foi implementado para o Itaú.");
    }

    public Task<QrCodeResponse> GetQrCodeAsync(string txId, PixChargeType type, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("GetQrCodeAsync ainda não foi implementado para o Itaú.");
    }

    public bool ValidateWebhook(IReadOnlyDictionary<string, string> headers, JsonElement body)
    {
        throw new NotImplementedException("ValidateWebhook ainda não foi implementado para o Itaú.");
    }

    public WebhookEvent ParseWebhookEvent(JsonElement body)
    {
        throw new NotImplementedException("ParseWebhookEvent ainda não foi implementado para o Itaú.");
    }

/// <summary>
/// Cria uma cobrança CobV no Itaú.
/// Endpoint: PUT /cobv/{txid}
/// </summary>

    // Exemplo estruturado com base nos fragmentos de criação/emissão de cobrança
    public async Task<ChargeResponse> CreateCobVAsync(ChargeRequest request, CancellationToken ct)
    {

        if (request.Type != PixChargeType.CobV)
            throw new ArgumentException("Esperado tipo COBV");
        if (request.DueDate is null)
            throw new ArgumentException("CobV requer DueDate");

        // Monta URL - Itaú usa PUT para criar/atualizar
        var url = $"{BaseUrl}/cobv/{request.TxId}";

            // Payload específico do Itaú (formato pode variar por contrato
        var payload = new
        {
            chave = request.PixKey,
            valor = new
            {
                 original = request.Amount.ToString("0.00", CultureInfo.InvariantCulture)
            },
            calendario = new
            {
                dataDeVencimento = request.DueDate.Value.ToString("yyyy-MM-dd")
            },
            solicitacaoPagador = request.PayerMessage
        };

            // Obtém token OAuth2
            var token = await _tokenProvider.GetAccessTokenAsync(ct);

            // Monta request HTTP
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, url);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
            "application/json");

            // Adiciona headers obrigatórios do Itaú
            httpRequest.Headers.Add("x-correlationID", Guid.NewGuid().ToString());

            // Executa request
            using var response = await _http.SendAsync(httpRequest, ct);
            var rawText = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new BankIntegrationException(
                $"Itaú CreateCobV failed: {(int)response.StatusCode} - {BankId}",
                BankId,
                (int)response.StatusCode);
        }

            // Parseia response
            var rawObj = JsonDocument.Parse(rawText).RootElement.Clone();

            // Mapeia status do Itaú para status normalizado
            var status = MapItauStatus(rawObj);

            return new ChargeResponse(
                TxId: request.TxId,
                Status: status,
                BankChargeId: request.TxId,
                Raw: rawObj);  
    }

    /// <summary>
 /// Consulta status de uma cobrança.
 /// Este é o método crítico para confirmar pagamentos.
 /// </summary>

    public async Task<ChargeStatusResponse> GetChargeStatusAsync(
        string txId, 
        PixChargeType type,
        CancellationToken ct)
    {

        var path = type switch
        {
            PixChargeType.Cob => $"/cobv/{txId}",
            PixChargeType.CobV => $"/cob/{txId}",
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Tipo de cobrança não suportado")
        };
        var url = $"{BaseUrl}{path}";

        var token = await _tokenProvider.GetAccessTokenAsync(ct);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("x-correlationID", Guid.NewGuid().ToString());

        using var response = await _http.SendAsync(request, ct);
        var rawText = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new BankIntegrationException(
                $"Itaú GetChargeStatus failed: {(int)response.StatusCode}", BankId,
                (int)response.StatusCode);
        }

        using var doc = JsonDocument.Parse(rawText);
        var root = doc.RootElement.Clone();

        var status = MapItauStatus(root);

        decimal? paidAmount = null;
        DateTimeOffset? paidAt = null;
        string? payload = null;
        
        // Itaú retorna array "pix" quando há pagamento concluído
        if (root.TryGetProperty("pix", out var pixArr) && pixArr.ValueKind == JsonValueKind.Array && pixArr.GetArrayLength() > 0)
        {
            var firstPix = pixArr[0];

            if (firstPix.TryGetProperty("valor", out var valorE1))
                paidAmount = decimal.Parse(valorE1.GetString()!, CultureInfo.InvariantCulture);

            if (firstPix.TryGetProperty("horario", out var horarioE1))
                paidAt = DateTimeOffset.Parse(horarioE1.GetString()!);

            if (firstPix.TryGetProperty("endToEndId", out var e2eEl))
                payload = e2eEl.GetString();
        }

        return new ChargeStatusResponse(
            TxId: txId,
            Status: status,
            PaidAmount: paidAmount,
            PaidAt: paidAt,
            PaymentId: payload,
            Raw: root);

    }

     /// <summary>
 /// Mapeia status do Itaú para status normalizado.
 /// IMPORTANTE: Esta tabela deve ser atualizada conforme documentaç
 /// </summary>

    private static PixChargeStatus MapItauStatus(JsonElement root)
    {
        if (!root.TryGetProperty("status", out var statusE1))
            return PixChargeStatus.Unknown;

        return statusE1.GetString()?.ToUpperInvariant() switch
        {
            "ATIVA" or "ACTIVE" => PixChargeStatus.Active,
            "CONCLUIDA" or "CONCLUIDO" => PixChargeStatus.Paid,
            "REMOVIDA_PELO_USUARIO_RECEBEDOR" => PixChargeStatus.Canceled,
            "REMOVIDA_PELO_PSP" => PixChargeStatus.Canceled,
            "EXPIRADA" => PixChargeStatus.Expired,
            _ => PixChargeStatus.Unknown
        };
    }
}