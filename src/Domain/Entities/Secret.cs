namespace ApiService.Domain.Entities;

public class Secret
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public string ClienteSecret { get; set; } = string.Empty;
    public string Certificado { get; set; } = string.Empty;
    public string SenhaCertificado { get; set; } = string.Empty;
}