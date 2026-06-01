using Application.Interfaces;
using Application.MockPayments;
using Infrastructure.Adapters.Abstractions;
using Infrastructure.Adapters.Itau;
using Infrastructure.Configuration;
using Infrastructure.MockPayments;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using ApiService.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ====== Database ======
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

// ====== Options ======
builder.Services.Configure<ItauOptions>(builder.Configuration.GetSection("Itau"));
builder.Services.Configure<MockServerOptions>(builder.Configuration.GetSection("ExternalServices:MockServer"));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

// ====== Application Services ======
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ISecretService, SecretService>();
builder.Services.AddScoped<IPixKeyService, PixKeyService>();
builder.Services.AddScoped<IChargeService, ChargeService>();

// ====== Bank Adapters ======
builder.Services.AddHttpClient<IItauTokenProvider, ItauTokenProvider>();
builder.Services.AddHttpClient<IBankPixAdapter, ItauPixAdapter>();
builder.Services.AddSingleton<IBankAdapterFactory, BankAdapterFactory>();

// ====== Mock Payment Gateway ======
builder.Services.AddHttpClient<IMockPaymentGateway, MockPaymentGateway>((sp, http) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MockServerOptions>>().Value;
    http.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
});
builder.Services.AddScoped<ProcessMockPaymentUseCase>();

// ====== MediatR (CQRS) ======
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(Application.Commands.CreatePixCharge.CreatePixChargeCommand).Assembly,
        typeof(Application.Commands.CreateInvoice.CreateInvoiceCommand).Assembly);
    cfg.AddOpenBehavior(typeof(Application.Behaviors.ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(Application.Behaviors.LoggingBehavior<,>));
});

// ====== FluentValidation ======
builder.Services.AddValidatorsFromAssemblyContaining<
    Application.Commands.CreatePixCharge.CreatePixChargeValidator>();

// ====== Controllers + JSON ======
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ====== OpenAPI ======
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// ====== Middleware pipeline ======
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.MapControllers();

// ====== Minimal API endpoints for mock payments ======
app.MapPost("/payments/mock", async (
    CreateMockPaymentRequest request,
    ProcessMockPaymentUseCase useCase,
    CancellationToken ct) =>
{
    var response = await useCase.ExecuteAsync(request, ct);
    return Results.Ok(response);
});

app.Run();
