using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Adapters.Itau;

public interface IItauTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
}

/// <summary>
/// Manages Itaú OAuth2 tokens with automatic in-memory caching.
/// Renews the token automatically before expiry.
/// </summary>
public sealed class ItauTokenProvider : IItauTokenProvider
{
    private readonly HttpClient _http;
    private readonly ItauOptions _options;
    private readonly ILogger<ItauTokenProvider> _logger;

    private string? _cachedToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public ItauTokenProvider(
        HttpClient http,
        IOptions<ItauOptions> options,
        ILogger<ItauTokenProvider> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(_cachedToken)
            && DateTimeOffset.UtcNow < _expiresAt.AddSeconds(-30))
        {
            return _cachedToken;
        }

        await _lock.WaitAsync(ct);
        try
        {
            // Double-check after acquiring lock
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
        _logger.LogDebug("Refreshing Itaú OAuth2 token.");

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl);

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
            ?? throw new InvalidOperationException("Token missing in Itaú response.");

        var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var ei)
            ? ei.GetInt32()
            : 300;

        _expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, expiresIn));

        _logger.LogInformation("Itaú token refreshed, expires at {ExpiresAt}", _expiresAt);

        return _cachedToken;
    }
}
