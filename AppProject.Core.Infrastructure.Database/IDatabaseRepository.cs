using System;
using AppProject.Core.Infrastructure.Database.Entities;

namespace AppProject.Core.Infrastructure.Database;

public interface IDatabaseRepository
{
    public Task InsertAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    public Task UpdateAsyn<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    public Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    public Task SaveAsync(CancellationToken cancellationToken = default);
}
