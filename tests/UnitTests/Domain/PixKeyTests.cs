using Domain.Aggregates.Account;
using Domain.Aggregates.PixKey;
using Domain.Exceptions;
using Xunit;

namespace UnitTests.Domain;

public class PixKeyTests
{
    [Fact]
    public void Create_WithValidData_BuildsPixKey()
    {
        var accountId = AccountId.CreateNew();

        var pixKey = PixKey.Create(accountId, "user@example.com", PixKeyType.Email);

        Assert.Equal(accountId, pixKey.AccountId);
        Assert.Equal("user@example.com", pixKey.KeyValue);
        Assert.Equal(PixKeyType.Email, pixKey.Type);
        Assert.NotNull(pixKey.Id);
    }

    [Fact]
    public void Create_WithNullAccount_Throws()
    {
        Assert.Throws<DomainException>(() =>
            PixKey.Create(null!, "user@example.com", PixKeyType.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyKeyValue_Throws(string keyValue)
    {
        var accountId = AccountId.CreateNew();

        Assert.Throws<DomainException>(() =>
            PixKey.Create(accountId, keyValue, PixKeyType.Random));
    }
}
