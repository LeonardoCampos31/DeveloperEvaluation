using Sales.Domain.Entities;

namespace Sales.Domain.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ProductExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal?> GetProductPriceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}