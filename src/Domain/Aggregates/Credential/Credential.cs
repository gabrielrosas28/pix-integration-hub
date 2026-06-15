using Domain.Exceptions;
using Domain;

namespace Domain.Aggregates.Credential; // Alterado o namespace de Secret para Credential

// 1. O ID Fortemente Tipado (Alterado de SecretId para CredentialId)
public sealed record CredentialId
{
    public Guid Value { get; }
    private CredentialId(Guid value) => Value = value;
    public static CredentialId CreateNew() => new(Guid.NewGuid());
    public static CredentialId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

// 2. A Raiz de Agregação (Aggregate Root) (Alterado de Secret para Credential)
public class Credential : AggregateRoot<CredentialId>
{
    // Mantivemos o ClientId como int para bater com a propriedade da Account
    public int ClientId { get; private set; } 
    
    // Como conversamos sobre o conceito, esta propriedade guarda o "segredo" dentro da Credencial
    public string ClientSecret { get; private set; } = null!;
    public string Certificate { get; private set; } = null!;
    public string CertificatePassword { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Credential() { } // Construtor exigido pelo EF Core

    public static Credential Create(
        int clientId, 
        string clientSecret, 
        string certificate, 
        string certificatePassword)
    {
        if (clientId <= 0)
            throw new DomainException("Client ID must be valid.");

        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new DomainException("Client Secret is required.");

        if (string.IsNullOrWhiteSpace(certificate))
            throw new DomainException("Certificate is required.");

        if (string.IsNullOrWhiteSpace(certificatePassword))
            throw new DomainException("Certificate password is required.");

        return new Credential
        {
            Id = CredentialId.CreateNew(), // Alterado para CredentialId
            ClientId = clientId,
            ClientSecret = clientSecret,
            Certificate = certificate,
            CertificatePassword = certificatePassword,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Método de domínio para atualizar o certificado caso ele vença (regras de negócio)
    public void UpdateCertificate(string newCertificate, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newCertificate))
            throw new DomainException("New certificate is required.");

        if (string.IsNullOrWhiteSpace(newPassword))
            throw new DomainException("New certificate password is required.");

        Certificate = newCertificate;
        CertificatePassword = newPassword;
        UpdatedAt = DateTime.UtcNow;
    }
}