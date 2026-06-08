using Application.DTOs;

namespace Application.Interfaces;

public interface IChargeService
{
    Task<CobResponse> CreateCobAsync(CreateCobRequest request, CancellationToken cancellationToken = default);
    Task<CobResponse> CreateCobvAsync(CreateCobvRequest request, CancellationToken cancellationToken = default);
}
