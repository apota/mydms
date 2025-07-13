using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DMS.Shared.Core.Models;

namespace DMS.Shared.Core.Data
{
    /// <summary>
    /// Generic repository interface for data access operations
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IRepository<T> where T : class, IEntity
    {
        /// <summary>
        /// Gets an entity by its identifier
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The entity if found, otherwise null</returns>
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of all entities</returns>
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Finds entities that match the predicate
        /// </summary>
        /// <param name="predicate">The filter expression</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of matching entities</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The added entity</returns>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds multiple new entities
        /// </summary>
        /// <param name="entities">The entities to add</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The added entities</returns>
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated entity</returns>
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Removes an entity
        /// </summary>
        /// <param name="entity">The entity to remove</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing the operation</returns>
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Removes multiple entities
        /// </summary>
        /// <param name="entities">The entities to remove</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing the operation</returns>
        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if any entity matches the predicate
        /// </summary>
        /// <param name="predicate">The filter expression</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>True if any entity matches the predicate, otherwise false</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Counts entities that match the predicate
        /// </summary>
        /// <param name="predicate">The filter expression</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The count of matching entities</returns>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    }
}
