using BEAPI.Database;
using BEAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BEAPI.Repositories
{
    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly BeContext _context;
        private readonly DbSet<T> _set;
        private IDbContextTransaction _transaction;

        public Repository(BeContext context)
        {
            _context = context;
            _set = _context.Set<T>();
        }

        public IQueryable<T> Get()
        {
            return _set.Where(x => true);
        }

        public async Task<List<T>?> GetValuesAsync(CancellationToken cancellationToken = default)
        {
            return await Get().ToListAsync(cancellationToken);
        }

        public async Task<T?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await Get().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _set.AddRangeAsync(entities, cancellationToken);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.CreationDate = DateTimeOffset.UtcNow;
            }
            await _set.AddAsync(entity, cancellationToken);
        }

        public void Update(T entity)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.ModificationDate = DateTimeOffset.UtcNow;
            }
            _set.Update(entity);
            _set.Update(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _set.RemoveRange(entities);
        }

        public void Delete(T entity)
        {
            _set.Remove(entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public void ClearChangeTracking()
        {
            _context.ChangeTracker.Clear();
        }

        public void BeginTransaction()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }
            _transaction = _context.Database.BeginTransaction();
        }
    }
}
