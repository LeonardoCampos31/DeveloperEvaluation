using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;

namespace Sales.Infrastructure.Repositories;

public class SaleRepository(ApplicationDbContext dbContext) : BaseRepository<Sale>(dbContext), ISaleRepository
{
    public override async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Sale>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Sales
            .Include(s => s.Items)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        DbContext.Entry(sale).State = EntityState.Modified;
     
        return Task.CompletedTask;
    }
    
    public async Task<bool> GetBySaleNumerAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await DbContext.Sales
            .Include(s => s.Items)
            .AnyAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }
}