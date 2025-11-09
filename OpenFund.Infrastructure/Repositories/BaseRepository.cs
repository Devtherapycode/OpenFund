using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenFund.Core.Interfaces.Repositories;
using OpenFund.Infrastructure.Context;

namespace OpenFund.Infrastructure.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    private readonly DbSet<TEntity> _dbSet;
    private readonly AppDbContext _appDbContext;

    public BaseRepository(AppDbContext dbContext)
    {
        _dbSet = dbContext.Set<TEntity>();
        _appDbContext = dbContext;
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<ICollection<TEntity>> GetAllBy(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(predicate).ToListAsync<TEntity>(cancellationToken);
    }

    public async Task Update(TEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Update(entity);
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Remove(TEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entity);
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
}