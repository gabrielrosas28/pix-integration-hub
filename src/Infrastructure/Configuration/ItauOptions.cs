namespace BankingHub.Infrastructure.Configuration;

/// <summary>
/// Opções de configuração para integração com o Itaú.
/// </summary>
public class ItauOptions
{
    /// <summary>
    /// Identificador do cliente OAuth2.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Segredo do cliente OAuth2.
    /// </summary>
    public string ClientCredential { get; set; } = string.Empty;

    /// <summary>
    /// URL do endpoint de tokens OAuth2.
    /// </summary>
    public string TokenUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL base da API do Itaú (ex: https://api.itau.com.br).
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Chave de API ou identificador adicional.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
