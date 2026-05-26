using MediatR;

namespace BankingHub.Application.Queries.GetInvoice;

public sealed record GetInvoiceQuery(Guid InvoiceId) : IRequest<InvoiceDto>;
