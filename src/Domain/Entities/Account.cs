namespace ApiService.Domain.Entities;

public class Account
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int? CredentialId { get; set; }
    public Credential? Credential { get; set; }
    public string Document { get; set; } = string.Empty;
    public int BankId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
}
