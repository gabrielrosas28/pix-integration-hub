using Domain.Exceptions;
using Xunit;

namespace UnitTests.Domain;

public class DomainExceptionsTests
{
    [Fact]
    public void DomainException_IsAnException()
    {
        var ex = new DomainException("boom");

        Assert.IsAssignableFrom<Exception>(ex);
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public void InvalidTxIdException_IsADomainException()
    {
        var ex = new InvalidTxIdException("bad txid");

        Assert.IsAssignableFrom<DomainException>(ex);
        Assert.Equal("bad txid", ex.Message);
    }
}
