using System.Text.Json;
using ApiService.Domain.Entities;
using ApiService.Infrastructure.Data;
using Application.Interfaces;
using BankingHub.Application.Commands.ProcessWebhook;
using BankingHub.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace UnitTests.Webhook;

/// <summary>
/// Cobre a regra central de negócio: o webhook NUNCA é fonte da verdade —
/// ele apenas dispara o processo. A confirmação do pagamento só ocorre via
/// consulta ativa ao banco (GetChargeStatusAsync). O processamento também é idempotente.
/// </summary>
public class ProcessWebhookHandlerTests
{
    private const string BankId = "ITAU";
    private const string TxId = "TX123ABC";

    private static ApplicationDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static JsonElement Body(string json = "{\"pix\":[{\"txid\":\"TX123ABC\"}]}")
        => JsonDocument.Parse(json).RootElement.Clone();

    private static Mock<IBankPixAdapter> AdapterWithValidWebhook(string? parsedTxId = TxId)
    {
        var adapter = new Mock<IBankPixAdapter>();
        adapter.Setup(a => a.ValidateWebhook(
                It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<JsonElement>()))
            .Returns(true);
        adapter.Setup(a => a.ParseWebhookEvent(It.IsAny<JsonElement>()))
            .Returns(new WebhookEvent(parsedTxId, WebhookEventType.PaymentConfirmed, DateTimeOffset.UtcNow, new object()));
        return adapter;
    }

    private static IBankAdapterFactory FactoryFor(IBankPixAdapter adapter)
    {
        var factory = new Mock<IBankAdapterFactory>();
        factory.Setup(f => f.Get(It.IsAny<string>())).Returns(adapter);
        return factory.Object;
    }

    private static ProcessWebhookHandler Handler(IApplicationDbContext db, IBankAdapterFactory factory)
        => new(db, factory, NullLogger<ProcessWebhookHandler>.Instance);

    private static async Task SeedCobrancaAsync(ApplicationDbContext db, string status, string chargeType = "Cob")
    {
        db.Cobrancas.Add(new Cobranca
        {
            TxId = TxId,
            InvoiceID = "INV-1",
            ChargeType = chargeType,
            Amount = 10m,
            Status = status,
            Emv = "EMV",
            PixLink = "pix://x",
            Raw = string.Empty,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task InvalidSignature_IsRejected_WithoutQueryingBank()
    {
        var adapter = new Mock<IBankPixAdapter>();
        adapter.Setup(a => a.ValidateWebhook(
                It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<JsonElement>()))
            .Returns(false);

        using var db = NewDb();
        var handler = Handler(db, FactoryFor(adapter.Object));

        var result = await handler.Handle(
            new ProcessWebhookCommand(BankId, new Dictionary<string, string>(), Body()), default);

        Assert.False(result.Accepted);
        adapter.Verify(a => a.GetChargeStatusAsync(
            It.IsAny<string>(), It.IsAny<PixChargeType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WebhookWithoutTxId_IsAcceptedButNotProcessed()
    {
        var adapter = AdapterWithValidWebhook(parsedTxId: null);

        using var db = NewDb();
        var handler = Handler(db, FactoryFor(adapter.Object));

        var result = await handler.Handle(
            new ProcessWebhookCommand(BankId, new Dictionary<string, string>(), Body()), default);

        Assert.True(result.Accepted);
        Assert.Null(result.TxId);
        adapter.Verify(a => a.GetChargeStatusAsync(
            It.IsAny<string>(), It.IsAny<PixChargeType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UnknownCharge_IsAccepted_WithoutQueryingBank()
    {
        var adapter = AdapterWithValidWebhook();

        using var db = NewDb(); // sem cobrança cadastrada
        var handler = Handler(db, FactoryFor(adapter.Object));

        var result = await handler.Handle(
            new ProcessWebhookCommand(BankId, new Dictionary<string, string>(), Body()), default);

        Assert.True(result.Accepted);
        Assert.Equal(TxId, result.TxId);
        adapter.Verify(a => a.GetChargeStatusAsync(
            It.IsAny<string>(), It.IsAny<PixChargeType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AlreadyPaidCharge_IsIdempotent_AndDoesNotQueryBank()
    {
        var adapter = AdapterWithValidWebhook();

        using var db = NewDb();
        await SeedCobrancaAsync(db, status: "paid");
        var handler = Handler(db, FactoryFor(adapter.Object));

        var result = await handler.Handle(
            new ProcessWebhookCommand(BankId, new Dictionary<string, string>(), Body()), default);

        Assert.True(result.Accepted);
        adapter.Verify(a => a.GetChargeStatusAsync(
            It.IsAny<string>(), It.IsAny<PixChargeType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActiveQueryConfirmsPayment_MarksChargeAsPaid()
    {
        var adapter = AdapterWithValidWebhook();
        adapter.Setup(a => a.GetChargeStatusAsync(TxId, It.IsAny<PixChargeType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChargeStatusResponse(TxId, PixChargeStatus.Paid, 10m, DateTimeOffset.UtcNow, "e2e", new object()));

        using var db = NewDb();
        await SeedCobrancaAsync(db, status: "created");
        var handler = Handler(db, FactoryFor(adapter.Object));

        var result = await handler.Handle(
            new ProcessWebhookCommand(BankId, new Dictionary<string, string>(), Body()), default);

        Assert.True(result.Accepted);
        var stored = await db.Cobrancas.SingleAsync(c => c.TxId == TxId);
        Assert.Equal("paid", stored.Status);
        Assert.NotNull(stored.UpdatedAt);
    }

    [Fact]
    public async Task ActiveQuerySaysNotPaid_DoesNotMarkAsPaid_EvenIfWebhookClaimsConfirmed()
    {
        // Regra crítica: o webhook dizia PaymentConfirmed, mas a consulta ativa
        // retorna Active — então a cobrança NÃO pode ser marcada como paga.
        var adapter = AdapterWithValidWebhook();
        adapter.Setup(a => a.GetChargeStatusAsync(TxId, It.IsAny<PixChargeType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChargeStatusResponse(TxId, PixChargeStatus.Active, null, null, null, new object()));

        using var db = NewDb();
        await SeedCobrancaAsync(db, status: "created");
        var handler = Handler(db, FactoryFor(adapter.Object));

        var result = await handler.Handle(
            new ProcessWebhookCommand(BankId, new Dictionary<string, string>(), Body()), default);

        Assert.True(result.Accepted);
        var stored = await db.Cobrancas.SingleAsync(c => c.TxId == TxId);
        Assert.Equal("created", stored.Status);
        Assert.Null(stored.UpdatedAt);
    }

    [Fact]
    public async Task ActiveQueryFails_IsAcceptedForRetry_AndChargeStaysUnpaid()
    {
        var adapter = AdapterWithValidWebhook();
        adapter.Setup(a => a.GetChargeStatusAsync(TxId, It.IsAny<PixChargeType>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("banco indisponível"));

        using var db = NewDb();
        await SeedCobrancaAsync(db, status: "created");
        var handler = Handler(db, FactoryFor(adapter.Object));

        var result = await handler.Handle(
            new ProcessWebhookCommand(BankId, new Dictionary<string, string>(), Body()), default);

        Assert.True(result.Accepted);
        var stored = await db.Cobrancas.SingleAsync(c => c.TxId == TxId);
        Assert.Equal("created", stored.Status);
    }

    [Fact]
    public async Task CobvCharge_QueriesBankWithCobVType()
    {
        var adapter = AdapterWithValidWebhook();
        adapter.Setup(a => a.GetChargeStatusAsync(TxId, It.IsAny<PixChargeType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChargeStatusResponse(TxId, PixChargeStatus.Paid, 10m, DateTimeOffset.UtcNow, "e2e", new object()));

        using var db = NewDb();
        await SeedCobrancaAsync(db, status: "created", chargeType: "Cobv");
        var handler = Handler(db, FactoryFor(adapter.Object));

        await handler.Handle(
            new ProcessWebhookCommand(BankId, new Dictionary<string, string>(), Body()), default);

        adapter.Verify(a => a.GetChargeStatusAsync(
            TxId, PixChargeType.CobV, It.IsAny<CancellationToken>()), Times.Once);
    }
}
