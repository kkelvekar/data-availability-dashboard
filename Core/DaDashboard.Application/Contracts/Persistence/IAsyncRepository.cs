using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.Application.Contracts.Persistence
{
    /// <summary>
    /// Defines a generic repository interface with asynchronous CRUD operations for entities.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IAsyncRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves an entity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity.</param>
        /// <returns>The entity instance if found; otherwise, null.</returns>
        Task<T?> GetByIdAsync(Guid id);
        /// <summary>
        /// Retrieves all entities in a read-only list.
        /// </summary>
        /// <returns>A read-only list of all entities.</returns>
        Task<IReadOnlyList<T>> ListAllAsync();
        /// <summary>
        /// Adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity.</returns>
        Task<T> AddAsync(T entity);
        /// <summary>
        /// Updates an existing entity in the repository.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        void UpdateAsync(T entity);
        /// <summary>
        /// Deletes an existing entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        void DeleteAsync(T entity);
    }
}
