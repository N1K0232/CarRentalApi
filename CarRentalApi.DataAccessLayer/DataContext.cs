using CarRentalApi.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;
using System.Reflection;

namespace CarRentalApi.DataAccessLayer;

public class DataContext : DbContext, IDataContext
{
    private static readonly MethodInfo queryFilterMethod = typeof(DataContext)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilter));

    private readonly ValueConverter<string, string> trimStringConverter;


    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        trimStringConverter = new ValueConverter<string, string>(v => v.Trim(), v => v.Trim());
    }


    public void Delete<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        Set<TEntity>().Remove(entity);
    }

    public void Delete<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
    {
        Set<TEntity>().RemoveRange(entities);
    }

    public void Edit<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        Set<TEntity>().Update(entity);
    }

    public Task<bool> ExistsAsync<TEntity>(Guid id) where TEntity : BaseEntity
    {
        return ExistsAsyncInternal<TEntity>(e => e.Id == id);
    }

    public Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : BaseEntity
    {
        return ExistsAsyncInternal<TEntity>(predicate);
    }

    public Task<TEntity> GetAsync<TEntity>(Guid id) where TEntity : BaseEntity
    {
        var set = GetDataInternal<TEntity>();
        return set.FirstOrDefaultAsync(e => e.Id == id);
    }

    public IQueryable<TEntity> GetData<TEntity>(bool ignoreQueryFilters = false, bool trackingChanges = false) where TEntity : BaseEntity
    {
        return GetDataInternal<TEntity>(ignoreQueryFilters, trackingChanges);
    }

    public void Insert<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        Set<TEntity>().Add(entity);
    }

    public Task<int> SaveAsync()
    {
        var entries = GetEntries();

        foreach (var entry in entries)
        {
            BaseEntity baseEntity = entry.Entity as BaseEntity;

            if (entry.State is EntityState.Added)
            {
                baseEntity.CreationDate = DateTime.UtcNow;
                baseEntity.UpdatedDate = null;

                if (baseEntity is DeletableEntity deletableEntity)
                {
                    deletableEntity.IsDeleted = false;
                    deletableEntity.DeletedDate = null;
                }
            }

            if (entry.State is EntityState.Modified)
            {
                baseEntity.UpdatedDate = DateTime.UtcNow;

                if (baseEntity is DeletableEntity deletableEntity)
                {
                    deletableEntity.IsDeleted = false;
                    deletableEntity.DeletedDate = null;
                }
            }

            if (entry.State is EntityState.Deleted)
            {
                if (baseEntity is DeletableEntity deletableEntity)
                {
                    entry.State = EntityState.Modified;
                    deletableEntity.IsDeleted = true;
                    deletableEntity.DeletedDate = DateTime.UtcNow;
                }
            }
        }

        return SaveChangesAsync();
    }

    public Task ExecuteTransactionAsync(Func<Task> action)
    {
        var strategy = Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(async () =>
        {
            using var transaction = await Database.BeginTransactionAsync().ConfigureAwait(false);
            await action.Invoke().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        var entities = modelBuilder.Model.GetEntityTypes()
           .Where(t => typeof(DeletableEntity).IsAssignableFrom(t.ClrType))
           .ToList();

        foreach (var type in entities.Select(t => t.ClrType))
        {
            var methods = SetGlobalQueryMethods(type);

            foreach (var method in methods)
            {
                var genericMethod = method.MakeGenericMethod(type);
                genericMethod.Invoke(this, new object[] { modelBuilder });
            }
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string))
                {
                    modelBuilder.Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(trimStringConverter);
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private void SetQueryFilter<TEntity>(ModelBuilder builder) where TEntity : DeletableEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted && e.DeletedDate == null);
    }

    private static IEnumerable<MethodInfo> SetGlobalQueryMethods(Type type)
    {
        var methods = new List<MethodInfo>();

        if (typeof(DeletableEntity).IsAssignableFrom(type))
        {
            methods.Add(queryFilterMethod);
        }

        return methods;
    }

    private Task<bool> ExistsAsyncInternal<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : BaseEntity
    {
        var set = GetDataInternal<TEntity>();
        return set.AnyAsync(predicate);
    }

    private IQueryable<TEntity> GetDataInternal<TEntity>(bool ignoreQueryFilters = false, bool trackingChanges = false) where TEntity : BaseEntity
    {
        var set = Set<TEntity>().AsQueryable();

        if (ignoreQueryFilters)
        {
            set = set.IgnoreQueryFilters();
        }

        return trackingChanges ?
            set.AsTracking() :
            set.AsNoTrackingWithIdentityResolution();
    }

    private IEnumerable<EntityEntry> GetEntries()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity.GetType().IsSubclassOf(typeof(BaseEntity)))
            .ToList();

        return entries.Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
    }
}