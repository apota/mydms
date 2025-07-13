using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using DMS.Shared.Core.Data;
using DMS.Shared.Core.Models;

namespace DMS.Shared.Data.DynamoDb
{
    /// <summary>
    /// Generic repository implementation for DynamoDB
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public class DynamoDbRepository<T> : IRepository<T> where T : class, IEntity
    {
        protected readonly IDynamoDBContext Context;

        public DynamoDbRepository(IDynamoDBContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await Context.LoadAsync<T>(id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var results = Context.ScanAsync<T>(new List<ScanCondition>());
            return await results.GetRemainingAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            // DynamoDB doesn't directly support LINQ predicates, so we need to scan and filter in memory
            // In a real application, we would translate predicates to DynamoDB query expressions
            // For simple cases, this works but is not recommended for production use with large datasets
            var all = await GetAllAsync(cancellationToken);
            return all.Where(predicate.Compile());
        }

        /// <inheritdoc />
        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            // Ensure the entity has an ID
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
            
            await Context.SaveAsync(entity, cancellationToken);
            return entity;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            var entitiesList = entities?.ToList() ?? throw new ArgumentNullException(nameof(entities));
            
            var batch = Context.CreateBatchWrite<T>();
            
            foreach (var entity in entitiesList)
            {
                // Ensure each entity has an ID
                if (entity.Id == Guid.Empty)
                {
                    entity.Id = Guid.NewGuid();
                }
                
                batch.AddPutItem(entity);
            }
            
            await batch.ExecuteAsync(cancellationToken);
            return entitiesList;
        }

        /// <inheritdoc />
        public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            await Context.SaveAsync(entity, cancellationToken);
            return entity;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            
            await Context.DeleteAsync(entity, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            var entitiesList = entities?.ToList() ?? throw new ArgumentNullException(nameof(entities));
            
            var batch = Context.CreateBatchWrite<T>();
            
            foreach (var entity in entitiesList)
            {
                batch.AddDeleteItem(entity);
            }
            
            await batch.ExecuteAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            // Similar limitation as FindAsync
            var all = await GetAllAsync(cancellationToken);
            return all.Any(predicate.Compile());
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken);
            return predicate == null
                ? all.Count()
                : all.Count(predicate.Compile());
        }
    }
}
