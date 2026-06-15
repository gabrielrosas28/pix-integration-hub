namespace Application.DTOs;

public class UpdateAccountRequest
{
    public int CredentialId { get; set; }
    public string Documento { get; set; } = string.Empty;

    public int BankId { get; set; }

    public string NumeroConta { get; set; } = string.Empty;

    public string Agencia { get; set; } = string.Empty;
}
