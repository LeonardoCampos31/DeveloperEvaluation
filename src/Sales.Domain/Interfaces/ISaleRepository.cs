using Sales.Domain.Entities;

namespace Sales.Domain.Interfaces;

public interface ISaleRepository
{
    Task AddAsync(Sale sale, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Sale>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> GetBySaleNumerAsync(string saleNumber, CancellationToken cancellationToken = default);
}