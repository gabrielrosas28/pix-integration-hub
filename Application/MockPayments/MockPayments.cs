namespace ApiService.Application.MockPayments;

public sealed record CreateMockPaymentRequest(
    decimal Amount,
    string MerchantId,
    MockPaymentScenario Scenario);

public sealed record MockPaymentResponse(
    string Status,
    string TransactionId,
    string? Message);

public enum MockPaymentScenario
{
    Success = 0,
    Error = 1,
    Timeout = 2
}

public interface IMockPaymentGateway
{
    Task<MockPaymentResponse> SendAsync(CreateMockPaymentRequest request, CancellationToken cancellationToken);
}

public sealed class ProcessMockPaymentUseCase
{
    private readonly IMockPaymentGateway mockPaymentGateway;

    public ProcessMockPaymentUseCase(IMockPaymentGateway mockPaymentGateway)
    {
        this.mockPaymentGateway = mockPaymentGateway;
    }

    public Task<MockPaymentResponse> ExecuteAsync(CreateMockPaymentRequest request, CancellationToken cancellationToken)
    {
        return mockPaymentGateway.SendAsync(request, cancellationToken);
    }
}