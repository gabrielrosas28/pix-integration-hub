using AutoMapper;
// Substitua pelos namespaces corretos do projeto:
using BankingHub.Application.DTOs;
using BankingHub.Application.Queries.GetInvoice; 
// using BankingHub.Domain.Entities; // Exemplo de onde viriam suas entidades

namespace BankingHub.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Exemplo 1: Mapeando uma entidade "Invoice" do banco para o DTO de retorno
            // CreateMap<Invoice, InvoiceDto>();

            // Exemplo 2: Se os seus DTOs de Request precisarem se transformar em comandos
            // CreateMap<ChargeRequestDto, CreateInvoiceCommand>();
            // CreateMap<ChargeRequestDto, CreatePixChargeCommand>();

            // Exemplo 3: Mapeando respostas de serviços externos para seus DTOs internos
            // CreateMap<ExternalPixResponse, ChargeResponseDto>();
            // CreateMap<ExternalQrCode, QrCodeResponseDto>();
            
            // Exemplo 4: Mapeando o Webhook recebido para o DTO de evento
            // CreateMap<ExternalWebhookPayload, WebhookEventDto>();
        }
    }
}