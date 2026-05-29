using Domain.Exceptions;
using Domain;

namespace Domain.Aggregates.Secret;

// 1. O ID Fortemente Tipado
public sealed record SecretId
{
    public Guid Value { get; }
    private SecretId(Guid value) => Value = value;
    public static SecretId CreateNew() => new(Guid.NewGuid());
    public static SecretId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

// 2. A Raiz de Agregação (Aggregate Root)
public class Secret : AggregateRoot<SecretId>
{
    // Mantivemos o ClientId como int para bater com a propriedade da Account, 
    // mas o ideal futuro seria usar o AccountId / ClientId fortemente tipado.
    public int ClientId { get; private set; } 
    
    public string ClientSecret { get; private set; } = null!;
    public string Certificate { get; private set; } = null!;
    public string CertificatePassword { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Secret() { } // Construtor exigido pelo EF Core

    public static Secret Create(
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

        return new Secret
        {
            Id = SecretId.CreateNew(),
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