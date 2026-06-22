namespace ApiService.Domain.Entities;

public class Credential
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string ClienteCredential { get; set; } = string.Empty;

    public string Certificate { get; set; } = string.Empty;

    public string PasswordCertificate { get; set; } = string.Empty;

    public int? AccountId { get; set; }
    public Account? Account { get; set; }
}
