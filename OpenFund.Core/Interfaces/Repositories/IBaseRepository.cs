using System.Linq.Expressions;

namespace OpenFund.Core.Interfaces.Repositories;

public interface IBaseRepository<TEntity>
{
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<ICollection<TEntity>> GetAllBy(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task Update(TEntity entity, CancellationToken cancellationToken);
    Task Remove(TEntity entity, CancellationToken cancellationToken);
}