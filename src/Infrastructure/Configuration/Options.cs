namespace Infrastructure.Configuration;

/// <summary>
/// Configuration options for Itaú bank integration.
/// Populated from appsettings.json → section "Itau".
/// </summary>
public sealed class ItauOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = string.Empty;
    public string SandboxBaseUrl { get; set; } = string.Empty;
    public string ProductionBaseUrl { get; set; } = string.Empty;
    public bool UseSandbox { get; set; } = true;
}

public sealed class MockServerOptions
{
    public string BaseUrl { get; set; } = "http://localhost:1080";
}

public sealed class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string QueueName { get; set; } = "payment-queue";
}
