using System.Net.Http.Json;
using ApiService.Application.MockPayments;
using ApiService.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ApiService.Infrastructure.MockPayments;

public sealed class MockPaymentGateway : IMockPaymentGateway
{
    private readonly HttpClient httpClient;
    private readonly MockServerOptions options;

    public MockPaymentGateway(HttpClient httpClient, IOptions<MockServerOptions> options)
    {
        this.httpClient = httpClient;
        this.options = options.Value;

        if (this.httpClient.BaseAddress is null)
        {
            this.httpClient.BaseAddress = new Uri(this.options.BaseUrl, UriKind.Absolute);
        }
    }

    public async Task<MockPaymentResponse> SendAsync(CreateMockPaymentRequest request, CancellationToken cancellationToken)
    {
        var path = request.Scenario switch
        {
            MockPaymentScenario.Error => "/psp/pay/error",
            MockPaymentScenario.Timeout => "/psp/pay/timeout",
            _ => "/psp/pay"
        };

        using var response = await httpClient.PostAsJsonAsync(path, request, cancellationToken);
        var body = await response.Content.ReadFromJsonAsync<MockPaymentResponse>(cancellationToken: cancellationToken);

        if (body is not null)
        {
            return body;
        }

        return new MockPaymentResponse(
            response.IsSuccessStatusCode ? "approved" : "error",
            string.Empty,
            response.ReasonPhrase);
    }
}