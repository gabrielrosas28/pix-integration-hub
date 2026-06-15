using Domain.Exceptions;
using Domain.ValueObjects;
using Xunit;

namespace UnitTests.Domain;

public class TxIdTests
{
    [Fact]
    public void From_WithValidAlphanumericValue_CreatesTxId()
    {
        var txId = TxId.From("PIX123ABC");

        Assert.Equal("PIX123ABC", txId.Value);
    }

    [Fact]
    public void From_WithExactly35Characters_IsAccepted()
    {
        var value = new string('A', 35);

        var txId = TxId.From(value);

        Assert.Equal(35, txId.Value.Length);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void From_WithEmptyOrWhitespace_Throws(string? value)
    {
        Assert.Throws<InvalidTxIdException>(() => TxId.From(value!));
    }

    [Fact]
    public void From_WithMoreThan35Characters_Throws()
    {
        var value = new string('A', 36);

        Assert.Throws<InvalidTxIdException>(() => TxId.From(value));
    }

    [Theory]
    [InlineData("PIX-123")]
    [InlineData("tx_id")]
    [InlineData("com espaco")]
    [InlineData("acentuação")]
    public void From_WithNonAlphanumericValue_Throws(string value)
    {
        Assert.Throws<InvalidTxIdException>(() => TxId.From(value));
    }

    [Fact]
    public void ImplicitStringConversion_ReturnsUnderlyingValue()
    {
        var txId = TxId.From("ABC123");

        string asString = txId;

        Assert.Equal("ABC123", asString);
    }

    [Fact]
    public void Generate_ProducesValidTxIdWithin35Chars()
    {
        // Comportamento esperado: gerar um TxId válido (até 35 caracteres alfanuméricos).
        var txId = TxId.Generate();

        Assert.NotNull(txId);
        Assert.InRange(txId.Value.Length, 1, 35);
        Assert.Matches("^[a-zA-Z0-9]+$", txId.Value);
    }
}
