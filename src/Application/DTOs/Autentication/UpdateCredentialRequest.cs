namespace Application.DTOs;

public class UpdateCredentialRequest // Alterado de UpdateSecretRequest
{
    public int ClientId { get; set; }

    public string ClientSecretValue { get; set; } = string.Empty; // Alterado de ClienteSecret para ClientSecretValue

    public string Certificate { get; set; } = string.Empty;

    public string CertificatePassword { get; set; } = string.Empty;

    public int? AccountId { get; set; }
}