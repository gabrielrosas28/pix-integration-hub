using BankingHub.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankingHub.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that wraps a command in a single Unit of Work commit.
/// Queries are skipped (they don't write). Commands trigger SaveChanges after
/// a successful handler execution.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var isCommand = typeof(TRequest).Name.EndsWith("Command", StringComparison.Ordinal);
        if (!isCommand)
            return await next(cancellationToken);

        var response = await next(cancellationToken);

        var changes = await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogDebug(
            "Committed {Changes} changes for {RequestName}",
            changes, typeof(TRequest).Name);

        return response;
    }
}
