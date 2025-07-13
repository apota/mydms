using System;
using System.Threading.Tasks;

namespace DMS.InventoryManagement.Core.Data
{
    /// <summary>
    /// Interface for the Unit of Work pattern
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Saves all changes made in this context to the database
        /// </summary>
        Task<int> SaveChangesAsync();
        
        /// <summary>
        /// Begins a transaction
        /// </summary>
        Task BeginTransactionAsync();
        
        /// <summary>
        /// Commits the transaction
        /// </summary>
        Task CommitTransactionAsync();
        
        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        Task RollbackTransactionAsync();
    }
}
