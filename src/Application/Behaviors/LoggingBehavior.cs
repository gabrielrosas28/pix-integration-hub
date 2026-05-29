using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BankingHub.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("[START] Executando comando {RequestName}", requestName);
        
        var timer = Stopwatch.StartNew();
        try
        {
            var response = await next();
            timer.Stop();
            _logger.LogInformation("[END] Comando {RequestName} finalizado com sucesso em {ElapsedMilliseconds}ms", requestName, timer.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.LogError(ex, "[ERROR] Comando {RequestName} falhou após {ElapsedMilliseconds}ms", requestName, timer.ElapsedMilliseconds);
            throw;
        }
    }
}