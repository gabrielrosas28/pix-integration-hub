namespace BankingHub.Application.DTOs;


public sealed record ChargeResponseDto(
    string TxId,
    string Status,
    string Emv,
    string? PixLink,
    string QrCodeBase64);
