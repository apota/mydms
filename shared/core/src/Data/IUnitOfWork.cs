using System;
using System.Threading;
using System.Threading.Tasks;

namespace DMS.Shared.Core.Data
{
    /// <summary>
    /// Interface for the Unit of Work pattern to handle transaction management
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Begins a new transaction
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing the operation</returns>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Commits the current transaction
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing the operation</returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing the operation</returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Saves all changes made in this context to the database
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The number of state entries written to the database</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
