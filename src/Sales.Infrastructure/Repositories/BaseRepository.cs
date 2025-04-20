using Microsoft.EntityFrameworkCore;

namespace Sales.Infrastructure.Repositories;

// Optional base class for common methods
public abstract class BaseRepository<T>(ApplicationDbContext dbContext)
    where T : class
{
    protected readonly ApplicationDbContext DbContext = dbContext;

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<T>().AddAsync(entity, cancellationToken);
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<T>().ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}