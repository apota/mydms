using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DMS.InventoryManagement.Core.Data
{
    /// <summary>
    /// Generic repository interface for data access
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets all entities
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();
        
        /// <summary>
        /// Gets an entity by ID
        /// </summary>
        Task<T> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Finds entities based on a predicate
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        /// <summary>
        /// Adds a new entity
        /// </summary>
        Task<T> AddAsync(T entity);
        
        /// <summary>
        /// Updates an existing entity
        /// </summary>
        Task<T> UpdateAsync(T entity);
        
        /// <summary>
        /// Deletes an entity
        /// </summary>
        Task DeleteAsync(T entity);
        
        /// <summary>
        /// Deletes an entity by ID
        /// </summary>
        Task DeleteByIdAsync(Guid id);
        
        /// <summary>
        /// Counts entities based on a predicate
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
        
        /// <summary>
        /// Gets a paged list of entities
        /// </summary>
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageIndex, 
            int pageSize, 
            Expression<Func<T, bool>> filter = null,
            Expression<Func<T, object>> orderBy = null,
            bool ascending = true);
        
        /// <summary>
        /// Checks if any entities match the predicate
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    }
}
