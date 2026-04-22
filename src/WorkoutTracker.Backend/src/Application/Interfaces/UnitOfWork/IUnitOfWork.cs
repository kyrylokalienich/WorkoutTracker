using WorkoutTracker.Application.Interfaces.Repositories;

namespace WorkoutTracker.Application.Interfaces.UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<T, int> Repository<T>() where T : class;
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
