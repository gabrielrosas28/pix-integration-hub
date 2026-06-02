using MediatR;
using BankingHub.Application.Queries.GetPixChargeStatus;

namespace BankingHub.Application.Queries.GetPixChargeStatus;


public sealed record GetPixChargeStatusQuery(string TxId) : IRequest<PixChargeStatusDto?>;
