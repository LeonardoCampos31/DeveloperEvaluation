using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;

namespace Sales.Infrastructure.Repositories;

public class ProductRepository(ApplicationDbContext dbContext) : BaseRepository<Product>(dbContext), IProductRepository
{
    public async Task<bool> ProductExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Products.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<decimal?> GetProductPriceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Products
            .Where(p => p.Id == id)
            .Select(p => (decimal?)p.Price) // Use nullable decimal in case product not found
            .FirstOrDefaultAsync(cancellationToken);
    }
}