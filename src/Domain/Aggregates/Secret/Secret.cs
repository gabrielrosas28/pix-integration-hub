using Domain.Exceptions;

namespace Domain.Aggregates.Secret;

public sealed record SecretId
{
    public Guid Value { get; }
    private SecretId(Guid value) => Value = value;
    public static SecretId CreateNew() => new(Guid.NewGuid());
    public static SecretId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public class Secret : AggregateRoot<SecretId>
{
    public int ClientId { get; private set; }
    public string ClientSecret { get; private set; } = null!;
    public string Certificate { get; private set; } = null!;
    public string CertificatePassword { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Secret() { }

    public static Secret Create(
        int clientId,
        string clientSecret,
        string certificate,
        string certificatePassword)
    {
        if (clientId <= 0)
            throw new DomainException("Client ID must be valid.");

        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new DomainException("Client secret is required.");

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
