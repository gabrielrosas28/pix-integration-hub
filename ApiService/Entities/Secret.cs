namespace ApiService.Domain.Entities;

public class Secret
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public string ClienteSecret { get; set; }
    public string Certificado { get; set; }
    public string SenhaCertificado { get; set; }

    public Conta Conta { get; set; }
}