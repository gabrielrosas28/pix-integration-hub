using BankingHub.Application.DTOs;
using BankingHub.Application.Queries.GetInvoice;
// usando MediatR para disparar os comandos e queries
using MediatR; 

namespace BankingHub.Application.Services
{
    public class InvoiceApplicationService
    {
        private readonly IMediator _mediator;

        public InvoiceApplicationService(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Orquestra a criação de uma Invoice chamando o Command correspondente
        public async Task<ChargeResponseDto> CreateInvoiceAsync(ChargeRequestDto request)
        {
            // O mapeamento do DTO para o Command pode ser feito aqui ou via AutoMapper
            var command = new CreateInvoiceCommand(request.Amount, request.CustomerId);
            return await _mediator.Send(command);
        }

        // Orquestra a busca de uma Invoice chamando a Query correspondente
        public async Task<InvoiceDto> GetInvoiceByIdAsync(Guid id)
        {
            var query = new GetInvoiceQuery(id);
            return await _mediator.Send(query);
        }
    }
}