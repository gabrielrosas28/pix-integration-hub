namespace BankingHub.Infrastructure.Adapters.Itau.Abstractions;

/// <summary>
/// Define o contrato para gerenciamento de tokens OAuth2 do Itaú.
/// </summary>
public interface IItauTokenProvider
{
    /// <summary>
    /// Obtém um token de acesso OAuth2 válido, utilizando cache quando disponível.
    /// </summary>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Token de acesso OAuth2.</returns>
    Task<string> GetAccessTokenAsync(CancellationToken ct);
}
