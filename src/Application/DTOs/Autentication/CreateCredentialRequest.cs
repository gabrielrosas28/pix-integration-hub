namespace Application.DTOs;

public class CreateCredentialRequest // Alterado de CreateSecretRequest
{
    public int ClientId { get; set; }

    public string ClientSecretValue { get; set; } = string.Empty; // Alterado de ClienteSecret para ClientSecretValue

    public string Certificado { get; set; } = string.Empty;

    public string SenhaCertificado { get; set; } = string.Empty;

    public int? ContaId { get; set; }
}