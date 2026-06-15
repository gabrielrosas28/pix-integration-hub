using Scalar.AspNetCore;
using ApiService.Filters;
using BankingHub.Application.MockPayments;
using ApiService.Infrastructure.Configuration;
using ApiService.Infrastructure.Data;
using ApiService.Infrastructure.MockPayments;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Infrastructure.Messaging.RabbitMQ;
using Application.Interfaces;
using Infrastructure.Services;
using Application.DTOs;

// Teste de envio de mensagem para RabbitMQ
try
{
    var producer = new RabbitMqProducer();

    await producer.SendMessageAsync(new PaymentCreatedMessage
    {
        UserId = 1,
        Amount = 100
    });
}
catch (Exception ex)
{
    Console.WriteLine($"RabbitMQ test send failed: {ex.Message}");
}
// Fim do teste

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.Configure<MockServerOptions>(builder.Configuration.GetSection("ExternalServices:MockServer"));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<ValidationFilter>();
});
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddHttpClient<IMockPaymentGateway, MockPaymentGateway>((serviceProvider, httpClient) =>
{
    var mockServerOptions = serviceProvider.GetRequiredService<IOptions<MockServerOptions>>().Value;
    httpClient.BaseAddress = new Uri(mockServerOptions.BaseUrl, UriKind.Absolute);
});

builder.Services.AddScoped<ProcessMockPaymentUseCase>();
builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<ICredentialService, CredentialService>(); // Alterado de ISecretService/SecretService
builder.Services.AddScoped<IChavePixService, ChavePixService>();
builder.Services.AddScoped<ICobrancaService, Infrastructure.Services.CobrancaService>();

// ===== Fluxo de webhook / cobrança Pix (MediatR + adapter Itaú) =====
// MediatR: descobre os handlers (inclui ProcessWebhookHandler) na assembly da Application
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
    typeof(BankingHub.Application.Commands.ProcessWebhook.ProcessWebhookCommand).Assembly));

// Abstração de persistência usada pelos handlers
builder.Services.AddScoped<Application.Interfaces.IApplicationDbContext>(
    sp => sp.GetRequiredService<ApplicationDbContext>());

// Notificação (usada por handlers de evento e reconciliação)
builder.Services.AddScoped<BankingHub.Application.Interfaces.INotificationService,
    Infrastructure.Services.NotificationService>();

// Opções do Itaú (token provider e adapter usam classes de options distintas)
builder.Services.AddSingleton(
    builder.Configuration.GetSection("Itau").Get<BankingHub.Infrastructure.Configuration.ItauOptions>()
    ?? new BankingHub.Infrastructure.Configuration.ItauOptions());
builder.Services.AddSingleton(
    builder.Configuration.GetSection("ItauAdapter").Get<Infrastructure.BankAdapters.Itau.ItauAdapterOptions>()
    ?? new Infrastructure.BankAdapters.Itau.ItauAdapterOptions());

// Token provider OAuth2 e o adapter Itaú (clientes HTTP tipados)
builder.Services.AddHttpClient<Infrastructure.BankAdapters.Itau.IItauTokenProvider,
    Infrastructure.BankAdapters.Itau.ItauTokenProvider>();
builder.Services.AddHttpClient<BankingHub.Application.Interfaces.IBankPixAdapter,
    Infrastructure.BankAdapters.Itau.ItauPixAdapter>();

// Factory que resolve o adapter por BankId
builder.Services.AddScoped<BankingHub.Application.Interfaces.IBankAdapterFactory,
    BankingHub.Infrastructure.BankAdapters.BankAdapterFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // UI interativa pra explorar/testar os endpoints no navegador (em /scalar/v1)
    app.MapScalarApiReference();
    // Raiz redireciona pra UI, pra http://localhost:5243/ abrir direto
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));

    // DEV: cria o schema do banco a partir do modelo atual (facilita testes locais).
    // Em produção, usar migrations (dotnet ef database update) em vez disto.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.MapControllers();
app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/payments/mock", async (
    CreateMockPaymentRequest request,
    ProcessMockPaymentUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var response = await useCase.ExecuteAsync(request, cancellationToken);
    return Results.Ok(response);
});

app.MapGet("/contas", async (IContaService contaService) =>
{
    var contas = await contaService.GetAllAsync();
    return Results.Ok(contas);
});

app.MapGet("/contas/{id}", async (int id, IContaService contaService) =>
{
    var conta = await contaService.GetByIdAsync(id);

    if (conta is null)
        return Results.NotFound();

    return Results.Ok(conta);
});

app.MapPost("/contas", async (
    CreateAccountRequest request,
    IContaService contaService) =>
{
    var conta = await contaService.CreateAsync(request);

    return Results.Ok(conta);
});

app.MapPut("/contas/{id}", async (
    int id,
    UpdateAccountRequest request,
    IContaService contaService) =>
{
    var conta = await contaService.UpdateAsync(id, request);

    if (conta is null)
        return Results.NotFound();

    return Results.Ok(conta);
});

app.MapDelete("/contas/{id}", async (
    int id,
    IContaService contaService) =>
{
    var deleted = await contaService.DeleteAsync(id);

    if (!deleted)
        return Results.NotFound();

    return Results.NoContent();
});

// --- Endpoints de Credentials (antigo /secrets) ---

app.MapGet("/credentials", async (ICredentialService credentialService) => // Alterado rota e interface
{
    var list = await credentialService.GetAllAsync();
    return Results.Ok(list);
});

app.MapGet("/credentials/{id:guid}", async (Guid id, ICredentialService credentialService) => // Alterado rota e interface
{
    var credential = await credentialService.GetByIdAsync(id); // Alterado nome da variável

    if (credential is null)
        return Results.NotFound();

    return Results.Ok(credential);
});

app.MapPost("/credentials", async (
    CreateCredentialRequest request, // Alterado tipo do request
    ICredentialService credentialService) => // Alterado interface
{
    var credential = await credentialService.CreateAsync(request); // Alterado variável e chamada

    return Results.Ok(credential);
});

app.MapPut("/credentials/{id:guid}", async (
    Guid id,
    UpdateCredentialRequest request, // Alterado tipo do request
    ICredentialService credentialService) => // Alterado interface
{
    var credential = await credentialService.UpdateAsync(id, request); // Alterado variável e chamada

    if (credential is null)
        return Results.NotFound();

    return Results.Ok(credential);
});

app.MapDelete("/credentials/{id:guid}", async (
    Guid id,
    ICredentialService credentialService) => // Alterado interface
{
    var deleted = await credentialService.DeleteAsync(id);

    if (!deleted)
        return Results.NotFound();

    return Results.NoContent();
});

// --- Fim dos Endpoints de Credentials ---

app.MapPost("/cobranca/v1/cob", async (
    Application.DTOs.CreateCobRequest request,
    Application.Interfaces.ICobrancaService cobrancaService,
    CancellationToken ct) =>
{
    var response = await cobrancaService.CreateCobAsync(request, ct);
    return Results.Created($"/cobranca/v1/{response.TxId}", response);
});

app.MapPost("/cobranca/v1/cobv", async (
    Application.DTOs.CreateCobvRequest request,
    Application.Interfaces.ICobrancaService cobrancaService,
    CancellationToken ct) =>
{
    try
    {
        var response = await cobrancaService.CreateCobvAsync(request, ct);
        return Results.Created($"/cobranca/v1/{response.TxId}", response);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}