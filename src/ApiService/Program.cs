using ApiService.Application.MockPayments;
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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.Configure<MockServerOptions>(builder.Configuration.GetSection("ExternalServices:MockServer"));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
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
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICredentialService, CredentialService>();
builder.Services.AddScoped<IPixKeyService, PixKeyService>();
builder.Services.AddScoped<IChargeService, ChargeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.UseHttpsRedirection();

app.MapPost("/payments/mock", async (
    CreateMockPaymentRequest request,
    ProcessMockPaymentUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var response = await useCase.ExecuteAsync(request, cancellationToken);
    return Results.Ok(response);
});

// Accounts
app.MapGet("/accounts", async (IAccountService accountService) =>
{
    var accounts = await accountService.GetAllAsync();
    return Results.Ok(accounts);
});

app.MapGet("/accounts/{id}", async (int id, IAccountService accountService) =>
{
    var account = await accountService.GetByIdAsync(id);
    if (account is null) return Results.NotFound();
    return Results.Ok(account);
});

app.MapPost("/accounts", async (CreateAccountRequest request, IAccountService accountService) =>
{
    var account = await accountService.CreateAsync(request);
    return Results.Ok(account);
});

app.MapPut("/accounts/{id}", async (int id, UpdateAccountRequest request, IAccountService accountService) =>
{
    var account = await accountService.UpdateAsync(id, request);
    if (account is null) return Results.NotFound();
    return Results.Ok(account);
});

app.MapDelete("/accounts/{id}", async (int id, IAccountService accountService) =>
{
    var deleted = await accountService.DeleteAsync(id);
    if (!deleted) return Results.NotFound();
    return Results.NoContent();
});

// Credentials
app.MapGet("/credentials", async (ICredentialService credentialService) =>
{
    var list = await credentialService.GetAllAsync();
    return Results.Ok(list);
});

app.MapGet("/credentials/{id}", async (int id, ICredentialService credentialService) =>
{
    var credential = await credentialService.GetByIdAsync(id);
    if (credential is null) return Results.NotFound();
    return Results.Ok(credential);
});

app.MapPost("/credentials", async (CreateCredentialRequest request, ICredentialService credentialService) =>
{
    var credential = await credentialService.CreateAsync(request);
    return Results.Ok(credential);
});

app.MapPut("/credentials/{id}", async (int id, UpdateCredentialRequest request, ICredentialService credentialService) =>
{
    var credential = await credentialService.UpdateAsync(id, request);
    if (credential is null) return Results.NotFound();
    return Results.Ok(credential);
});

app.MapDelete("/credentials/{id}", async (int id, ICredentialService credentialService) =>
{
    var deleted = await credentialService.DeleteAsync(id);
    if (!deleted) return Results.NotFound();
    return Results.NoContent();
});

// Charges
app.MapPost("/cobranca/v1/cob", async (
    CreateCobRequest request,
    IChargeService chargeService,
    CancellationToken ct) =>
{
    var response = await chargeService.CreateCobAsync(request, ct);
    return Results.Created($"/cobranca/v1/{response.TxId}", response);
});

app.MapPost("/cobranca/v1/cobv", async (
    CreateCobvRequest request,
    IChargeService chargeService,
    CancellationToken ct) =>
{
    try
    {
        var response = await chargeService.CreateCobvAsync(request, ct);
        return Results.Created($"/cobranca/v1/{response.TxId}", response);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();
