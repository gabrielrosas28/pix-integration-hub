using ApiService.Application.MockPayments;
using ApiService.Infrastructure.Configuration;
using ApiService.Infrastructure.Data;
using ApiService.Infrastructure.MockPayments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Infrastructure.Messaging.RabbitMQ;
using Application.Interfaces;
using Infrastructure.Services;
using Application.DTOs;
// Teste de envio de mensagem para RabbitMQ
var producer = new RabbitMqProducer();

await producer.SendMessageAsync(new PaymentCreatedMessage
{
    UserId = 1,
    Amount = 100
});
// FIm do teste
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<MockServerOptions>(builder.Configuration.GetSection("ExternalServices:MockServer"));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<IMockPaymentGateway, MockPaymentGateway>((serviceProvider, httpClient) =>
{
    var mockServerOptions = serviceProvider.GetRequiredService<IOptions<MockServerOptions>>().Value;
    httpClient.BaseAddress = new Uri(mockServerOptions.BaseUrl, UriKind.Absolute);
});
builder.Services.AddScoped<ProcessMockPaymentUseCase>();
builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<ISecretService, SecretService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
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
    CreateContaRequest request,
    IContaService contaService) =>
{
    var conta = await contaService.CreateAsync(request);

    return Results.Ok(conta);
});

app.MapPut("/contas/{id}", async (
    int id,
    UpdateContaRequest request,
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

app.MapGet("/secrets", async (ISecretService secretService) =>
{
    var list = await secretService.GetAllAsync();
    return Results.Ok(list);
});

app.MapGet("/secrets/{id}", async (int id, ISecretService secretService) =>
{
    var secret = await secretService.GetByIdAsync(id);

    if (secret is null)
        return Results.NotFound();

    return Results.Ok(secret);
});

app.MapPost("/secrets", async (
    CreateSecretRequest request,
    ISecretService secretService) =>
{
    var secret = await secretService.CreateAsync(request);

    return Results.Ok(secret);
});

app.MapPut("/secrets/{id}", async (
    int id,
    UpdateSecretRequest request,
    ISecretService secretService) =>
{
    var secret = await secretService.UpdateAsync(id, request);

    if (secret is null)
        return Results.NotFound();

    return Results.Ok(secret);
});

app.MapDelete("/secrets/{id}", async (
    int id,
    ISecretService secretService) =>
{
    var deleted = await secretService.DeleteAsync(id);

    if (!deleted)
        return Results.NotFound();

    return Results.NoContent();
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
