using System;
using System.Data;
using AppProject.Core.Infrastructure.Database.Entities;
using AppProject.Exceptions;

namespace AppProject.Core.Infrastructure.Database;

public class DatabaseRepository(
    ApplicationDbContext applicationDbContext) 
    : IDatabaseRepository
{
    public async Task InsertAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        // essa linha ainda não salva no banco. Ela apenas prepara a inserção em memória.
        await applicationDbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsyn<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        applicationDbContext.Set<TEntity>().Update(entity);
    }

    public Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        applicationDbContext.Set<TEntity>().Remove(entity);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // confirma no banco de dados.
            await applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DBConcurrencyException concurrencyException)
        {
            throw new AppException(ExceptionCode.Concurrency, innerException: concurrencyException);
        }
    }
}
