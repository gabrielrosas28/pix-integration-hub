namespace Application.DTOs;

public class CreateAccountRequest
{
    public int CredentialId { get; set; }
    public string Document { get; set; } = string.Empty;
    public int BankId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
}
