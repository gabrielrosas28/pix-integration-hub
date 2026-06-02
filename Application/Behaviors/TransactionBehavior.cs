using MediatR;
using System.Transactions;

namespace BankingHub.Application.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // Substitua pelo seu mecanismo de persistência ou Unit of Work real se preferir
    // private readonly IUnitOfWork _unitOfWork; 

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Se for um Command (modifica dados), abre transação. Se for Query (busca), apenas passa direto.
        if (!typeof(TRequest).Name.EndsWith("Command"))
        {
            return await next();
        }

        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        
        var response = await next();
        
        // Se chegou até aqui sem dar exception no Handler, commita
        transactionScope.Complete();
        
        return response;
    }
}