using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.BankAdapters.Itau;

public sealed class ItauAdapterOptions
{
    public bool UseSandbox { get; set; }
    public string SandboxBaseUrl { get; set; } = string.Empty;
    public string ProdutionBaseUrl { get; set; } = string.Empty;
}

public interface IItauTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
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
