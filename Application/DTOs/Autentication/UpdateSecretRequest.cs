namespace Application.DTOs;

public class UpdateSecretRequest
{
    public int ClientId { get; set; }

    public string ClienteSecret { get; set; } = string.Empty;

    public string Certificado { get; set; } = string.Empty;

    public string SenhaCertificado { get; set; } = string.Empty;

    public int? ContaId { get; set; }
}
