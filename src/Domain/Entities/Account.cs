using Domain.Aggregates.Credential;

namespace ApiService.Domain.Entities;

public class Account
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int? CredentialId { get; set; } // Alterado de SecretId para CredentialId
    public Credential? Credential { get; set; } // Alterado de Secret? Secret para Credential? Credential

    public string Document { get; set; } = string.Empty;

    public int BankId { get; set; }

    public string AccountNumber { get; set; } = string.Empty;

    public string Agency { get; set; } = string.Empty;
}