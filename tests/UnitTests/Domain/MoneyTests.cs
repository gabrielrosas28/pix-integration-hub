using Domain.Exceptions;
using Domain.ValueObjects;
using Xunit;

namespace UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void BRL_WithValidValue_SetsValueAndCurrency()
    {
        var money = Money.BRL(10.50m);

        Assert.Equal(10.50m, money.Value);
        Assert.Equal("BRL", money.Currency);
    }

    [Theory]
    [InlineData(10.567, 10.57)]
    [InlineData(10.999, 11.00)]
    [InlineData(10.001, 10.00)]
    public void BRL_RoundsToTwoDecimals(decimal input, decimal expected)
    {
        var money = Money.BRL(input);

        Assert.Equal(expected, money.Value);
    }

    [Fact]
    public void BRL_WithNegativeValue_Throws()
    {
        Assert.Throws<DomainException>(() => Money.BRL(-1m));
    }

    [Fact]
    public void Zero_IsZeroBRL()
    {
        Assert.Equal(0m, Money.Zero.Value);
        Assert.Equal("BRL", Money.Zero.Currency);
    }

    [Fact]
    public void ComparisonOperators_WorkByValue()
    {
        var ten = Money.BRL(10m);
        var five = Money.BRL(5m);

        Assert.True(ten > five);
        Assert.True(five < ten);
        Assert.True(ten >= Money.BRL(10m));
        Assert.True(five <= Money.BRL(5m));
    }

    [Fact]
    public void ValueEquality_HoldsForSameAmountAndCurrency()
    {
        Assert.Equal(Money.BRL(10m), Money.BRL(10m));
    }
}
