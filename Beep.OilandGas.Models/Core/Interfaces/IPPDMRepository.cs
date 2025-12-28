using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Generic repository interface for PPDM entities
    /// Provides standard CRUD operations with common column handling
    /// </summary>
    /// <typeparam name="T">PPDM entity type</typeparam>
    public interface IPPDMRepository<T> where T : class, IPPDMEntity
    {
        /// <summary>
        /// Gets the UnitOfWork instance
        /// </summary>
        IUnitofWork UnitOfWork { get; }

        /// <summary>
        /// Gets the common column handler
        /// </summary>
        ICommonColumnHandler CommonColumnHandler { get; }

        /// <summary>
        /// Gets an entity by its primary key
        /// </summary>
        Task<T> GetByIdAsync(object id);

        /// <summary>
        /// Gets all active entities (ACTIVE_IND = 'Y')
        /// </summary>
        Task<IEnumerable<T>> GetActiveAsync();

        /// <summary>
        /// Gets all entities matching a predicate
        /// </summary>
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets a single entity matching a predicate
        /// </summary>
        Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Inserts a new entity (automatically handles common columns)
        /// </summary>
        Task<T> InsertAsync(T entity, string userId);

        /// <summary>
        /// Inserts multiple entities in a batch
        /// </summary>
        Task<IEnumerable<T>> InsertBatchAsync(IEnumerable<T> entities, string userId);

        /// <summary>
        /// Updates an existing entity (automatically handles common columns)
        /// </summary>
        Task<T> UpdateAsync(T entity, string userId);

        /// <summary>
        /// Updates multiple entities in a batch
        /// </summary>
        Task<IEnumerable<T>> UpdateBatchAsync(IEnumerable<T> entities, string userId);

        /// <summary>
        /// Soft deletes an entity (sets ACTIVE_IND = 'N')
        /// </summary>
        Task<bool> SoftDeleteAsync(object id, string userId);

        /// <summary>
        /// Hard deletes an entity from the database
        /// </summary>
        Task<bool> DeleteAsync(object id);

        /// <summary>
        /// Checks if an entity exists
        /// </summary>
        Task<bool> ExistsAsync(object id);

        /// <summary>
        /// Gets the count of entities matching a predicate
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);

        /// <summary>
        /// Gets entities with pagination
        /// </summary>
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Expression<Func<T, bool>> predicate = null,
            Expression<Func<T, object>> orderBy = null,
            bool ascending = true);
    }
}

