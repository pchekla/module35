namespace Module35.Models.Repositories;

public interface IUnitOfWork: IDisposable
{
    int SaveChanges(bool ensureAutoHistory = false);
 
    IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = true) where TEntity : class;
} 