using System.Net.Http.Json;
using Application.MockPayments;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.MockPayments;

public sealed class MockPaymentGateway : IMockPaymentGateway
{
    private readonly HttpClient _http;

    public MockPaymentGateway(HttpClient http, IOptions<MockServerOptions> options)
    {
        _http = http;
        if (_http.BaseAddress is null)
            _http.BaseAddress = new Uri(options.Value.BaseUrl, UriKind.Absolute);
    }

    public async Task<MockPaymentResponse> SendAsync(
        CreateMockPaymentRequest request, CancellationToken ct)
    {
        var path = request.Scenario switch
        {
            MockPaymentScenario.Error   => "/psp/pay/error",
            MockPaymentScenario.Timeout => "/psp/pay/timeout",
            _                          => "/psp/pay"
        };

        using var response = await _http.PostAsJsonAsync(path, request, ct);
        var body = await response.Content.ReadFromJsonAsync<MockPaymentResponse>(cancellationToken: ct);

        return body ?? new MockPaymentResponse(
            response.IsSuccessStatusCode ? "approved" : "error",
            string.Empty,
            response.ReasonPhrase);
    }
}
