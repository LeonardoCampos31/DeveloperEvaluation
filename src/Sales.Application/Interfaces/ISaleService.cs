using Sales.Application.DTOs;

namespace Sales.Application.Interfaces;

public interface ISaleService
{
    Task<IEnumerable<SaleDto>> GetAllAsync();
    Task<SaleDto> CreateAsync(CreateSaleRequest request);
    Task DeleteAsync(Guid id);
}