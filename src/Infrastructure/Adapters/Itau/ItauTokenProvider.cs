// BankingHub.Infrastructure/BankAdapters/Itau/ItauTokenProvider.cs

using BankingHub.Infrastructure.Adapters.Itau.Abstractions;
using BankingHub.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BankingHub.Infrastructure.Adapters.Itau;

/// <summary>
/// Gerencia tokens OAuth2 do Itaú com cache automático.
/// Renova o token automaticamente antes de expirar.
/// </summary>
public sealed class ItauTokenProvider : IItauTokenProvider
{
    private readonly HttpClient _http;
    private readonly ItauOptions _options;
    private readonly ILogger<ItauTokenProvider> _logger;

    // Cache em memória (para produção, considere distributed cache)
    private string? _cachedToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public ItauTokenProvider(
        HttpClient http,
        ItauOptions options,
        ILogger<ItauTokenProvider> logger)
    {
        _http = http;
        _options = options;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        // Verifica cache (com margem de 30 segundos)
        if (!string.IsNullOrWhiteSpace(_cachedToken)
            && DateTimeOffset.UtcNow < _expiresAt.AddSeconds(-30))
        {
            return _cachedToken;
        }

        // Lock para evitar múltiplas renovações simultâneas
        await _lock.WaitAsync(ct);
        try
        {
            // Double-check após obter lock
            if (!string.IsNullOrWhiteSpace(_cachedToken)
                && DateTimeOffset.UtcNow < _expiresAt.AddSeconds(-30))
            {
                return _cachedToken;
            }

            return await RefreshTokenAsync(ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<string> RefreshTokenAsync(CancellationToken ct)
    {
        _logger.LogDebug("Renovando token OAuth2 do Itaú");

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl);

        // OAuth2 client_credentials
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["scope"] = "cobv.read cobv.write cob.read cob.write pix.read pix.write"
        });

        using var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        _cachedToken = doc.RootElement.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("Token ausente na resposta do Itaú");

        var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var ei)
            ? ei.GetInt32()
            : 300;

        _expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, expiresIn));

        _logger.LogInformation("Token Itaú renovado, expira em {ExpiresAt}", _expiresAt);

        return _cachedToken;
    }
}