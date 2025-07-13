using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DMS.Shared.Core.Data;
using DMS.Shared.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DMS.Shared.Data.Postgres
{
    /// <summary>
    /// Generic repository implementation for Entity Framework Core
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public class EfRepository<T> : IRepository<T> where T : class, IEntity
    {
        protected readonly DbContext Context;
        protected readonly DbSet<T> DbSet;

        public EfRepository(DbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            DbSet = context.Set<T>();
        }

        /// <inheritdoc />
        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await DbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await DbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            await DbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            var entitiesList = entities?.ToList() ?? throw new ArgumentNullException(nameof(entities));
            
            await DbSet.AddRangeAsync(entitiesList, cancellationToken);
            return entitiesList;
        }

        /// <inheritdoc />
        public Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            Context.Entry(entity).State = EntityState.Modified;
            return Task.FromResult(entity);
        }

        /// <inheritdoc />
        public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            DbSet.Remove(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            var entitiesList = entities?.ToList() ?? throw new ArgumentNullException(nameof(entities));
            
            DbSet.RemoveRange(entitiesList);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await DbSet.AnyAsync(predicate, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            return predicate == null
                ? await DbSet.CountAsync(cancellationToken)
                : await DbSet.CountAsync(predicate, cancellationToken);
        }
    }
}
