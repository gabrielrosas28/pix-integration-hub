namespace ApiService.Domain.Entities;

public class Secret
{
    public int SecretID { get; set; }

    public int ClientId { get; set; }
    public string ClienteSecret { get; set; } = string.Empty;
    public string Certificado { get; set; } = string.Empty;
    public string SenhaCertificado { get; set; } = string.Empty;

    public int? ContaId { get; set; }
    public Conta? Conta { get; set; }
}