namespace Application.DTOs;

public class UpdateAccountRequest
{
    public int CredentialId { get; set; }
    public string Document { get; set; } = string.Empty;

    public int BankId { get; set; }

    public string AccountNumber { get; set; } = string.Empty;

    public string Agency { get; set; } = string.Empty;
}
