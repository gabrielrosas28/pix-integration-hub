namespace Application.MockPayments;

public sealed record CreateMockPaymentRequest(
    decimal Amount,
    string MerchantId,
    MockPaymentScenario Scenario);

public sealed record MockPaymentResponse(
    string Status,
    string TransactionId,
    string? Message);

public enum MockPaymentScenario { Success = 0, Error = 1, Timeout = 2 }

public interface IMockPaymentGateway
{
    Task<MockPaymentResponse> SendAsync(CreateMockPaymentRequest request, CancellationToken ct);
}

public sealed class ProcessMockPaymentUseCase
{
    private readonly IMockPaymentGateway _gateway;

    public ProcessMockPaymentUseCase(IMockPaymentGateway gateway)
        => _gateway = gateway;

    public Task<MockPaymentResponse> ExecuteAsync(CreateMockPaymentRequest request, CancellationToken ct)
        => _gateway.SendAsync(request, ct);
}
