using System;

namespace DMS.Shared.Core.Models
{
    /// <summary>
    /// Base interface for all entities within the system that require unique identification
    /// </summary>
    public interface IEntity<TId>
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity
        /// </summary>
        TId Id { get; set; }
    }

    /// <summary>
    /// Base interface for entities with GUID identifiers
    /// </summary>
    public interface IEntity : IEntity<Guid>
    {
    }

    /// <summary>
    /// Base interface for entities that track creation and update timestamps
    /// </summary>
    public interface IAuditableEntity : IEntity
    {
        /// <summary>
        /// Gets or sets the date and time when the entity was created
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user ID that created the entity
        /// </summary>
        string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was last updated
        /// </summary>
        DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user ID that last updated the entity
        /// </summary>
        string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Base interface for entities that can be soft-deleted
    /// </summary>
    public interface ISoftDeleteEntity : IEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was deleted
        /// </summary>
        DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Gets or sets the user ID that deleted the entity
        /// </summary>
        string? DeletedBy { get; set; }
    }
}
