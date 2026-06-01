using MediatR;
using BankingHub.Application.Queries.GetInvoice;

namespace BankingHub.Application.Queries.GetInvoice;


public sealed record GetInvoiceQuery(Guid InvoiceId) : IRequest<InvoiceDto?>;
