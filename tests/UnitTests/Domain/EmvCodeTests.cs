using Domain.Exceptions;
using Domain.ValueObjects;
using Xunit;

namespace UnitTests.Domain;

public class EmvCodeTests
{
    [Fact]
    public void From_WithValidValue_CreatesEmvCode()
    {
        var emv = EmvCode.From("00020126");

        Assert.Equal("00020126", emv.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void From_WithEmptyOrWhitespace_Throws(string value)
    {
        Assert.Throws<DomainException>(() => EmvCode.From(value));
    }

    [Fact]
    public void ImplicitStringConversion_ReturnsUnderlyingValue()
    {
        var emv = EmvCode.From("EMV-PAYLOAD");

        string asString = emv;

        Assert.Equal("EMV-PAYLOAD", asString);
    }
}
