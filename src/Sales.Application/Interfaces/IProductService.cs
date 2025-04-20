using Sales.Application.DTOs;

namespace Sales.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto> CreateAsync(ProductDto dto);
}
