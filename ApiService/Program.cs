using ApiService.Application.MockPayments;
using ApiService.Infrastructure.Configuration;
using ApiService.Infrastructure.Data;
using ApiService.Infrastructure.MockPayments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
