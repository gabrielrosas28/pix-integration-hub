using Domain.Aggregates.Credential;
using Domain.Exceptions;
using Xunit;

namespace UnitTests.Domain;

public class CredentialTests
{
    [Fact]
    public void Create_WithValidData_BuildsCredential()
    {
        var credential = Credential.Create(
            clientId: 1,
            clientSecret: "secret",
            certificate: "cert",
            certificatePassword: "pwd");

        Assert.Equal(1, credential.ClientId);
        Assert.Equal("secret", credential.ClientSecret);
        Assert.Equal("cert", credential.Certificate);
        Assert.Equal("pwd", credential.CertificatePassword);
        Assert.NotNull(credential.Id);
        Assert.Null(credential.UpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithInvalidClientId_Throws(int clientId)
    {
        Assert.Throws<DomainException>(() =>
            Credential.Create(clientId, "secret", "cert", "pwd"));
    }

    [Theory]
    [InlineData("", "cert", "pwd")]
    [InlineData("secret", "", "pwd")]
    [InlineData("secret", "cert", "")]
    public void Create_WithMissingRequiredField_Throws(string secret, string cert, string pwd)
    {
        Assert.Throws<DomainException>(() =>
            Credential.Create(1, secret, cert, pwd));
    }

    [Fact]
    public void UpdateCertificate_WithValidData_UpdatesAndStampsUpdatedAt()
    {
        var credential = Credential.Create(1, "secret", "cert", "pwd");

        credential.UpdateCertificate("new-cert", "new-pwd");

        Assert.Equal("new-cert", credential.Certificate);
        Assert.Equal("new-pwd", credential.CertificatePassword);
        Assert.NotNull(credential.UpdatedAt);
    }

    [Theory]
    [InlineData("", "pwd")]
    [InlineData("cert", "")]
    public void UpdateCertificate_WithMissingField_Throws(string cert, string pwd)
    {
        var credential = Credential.Create(1, "secret", "cert", "pwd");

        Assert.Throws<DomainException>(() => credential.UpdateCertificate(cert, pwd));
    }
}
