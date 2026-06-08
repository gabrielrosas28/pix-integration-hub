using AutoMapper;
using BankingHub.Application.Commands.CreatePixCharge;
using BankingHub.Application.DTOs;
using BankingHub.Application.Queries.GetInvoice;
using BankingHub.Application.Queries.GetPixChargeStatus;

namespace BankingHub.Application.Mappings;


public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ChargeRequestDto → CreatePixChargeCommand
        CreateMap<ChargeRequestDto, CreatePixChargeCommand>()
            .ConstructUsing(src => new CreatePixChargeCommand(
                src.InvoiceId,
                src.ChargeType,
                src.Amount,
                src.PixKey,
                src.DueDate,
                src.ExpiresInSeconds,
                src.PayerMessage));

        // CreatePixChargeResult → ChargeResponseDto  (QrCodeBase64 preenchido na Presentation)
        CreateMap<CreatePixChargeResult, ChargeResponseDto>()
            .ConstructUsing(src => new ChargeResponseDto(
                src.TxId,
                src.Status,
                src.Emv,
                src.PixLink,
                string.Empty));

        // InvoiceDto (já é record — mapeamento direto caso necessário)
        CreateMap<InvoiceDto, InvoiceDto>();

        // PixChargeStatusDto
        CreateMap<PixChargeStatusDto, PixChargeStatusDto>();
    }
}
