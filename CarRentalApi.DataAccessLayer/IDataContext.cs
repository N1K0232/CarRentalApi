using CarRentalApi.DataAccessLayer.Entities.Common;
using System.Linq.Expressions;

namespace CarRentalApi.DataAccessLayer;

public interface IDataContext
{
    void Delete<TEntity>(TEntity entity) where TEntity : BaseEntity;

    void Delete<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

    void Edit<TEntity>(TEntity entity) where TEntity : BaseEntity;

    Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : BaseEntity;

    Task<TEntity> GetAsync<TEntity>(Guid id) where TEntity : BaseEntity;

    IQueryable<TEntity> GetData<TEntity>(bool ignoreQueryFilters = false, bool trackingChanges = false) where TEntity : BaseEntity;

    void Insert<TEntity>(TEntity entity) where TEntity : BaseEntity;

    Task SaveAsync();

    Task ExecuteTransactionAsync(Func<Task> action);
}