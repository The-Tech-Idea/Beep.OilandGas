using System;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Interface for handling common columns across all PPDM entities
    /// Provides automatic management of audit columns and metadata
    /// </summary>
    public interface ICommonColumnHandler
    {
        /// <summary>
        /// Sets the created columns (ROW_CREATED_BY, ROW_CREATED_DATE) for a new entity
        /// </summary>
        /// <param name="entity">The PPDM entity</param>
        /// <param name="userId">User identifier who is creating the entity</param>
        void SetCreatedColumns(IPPDMEntity entity, string userId);

        /// <summary>
        /// Sets the changed columns (ROW_CHANGED_BY, ROW_CHANGED_DATE) for an updated entity
        /// </summary>
        /// <param name="entity">The PPDM entity</param>
        /// <param name="userId">User identifier who is updating the entity</param>
        void SetChangedColumns(IPPDMEntity entity, string userId);

        /// <summary>
        /// Sets the active indicator
        /// </summary>
        /// <param name="entity">The PPDM entity</param>
        /// <param name="isActive">True if active, false if inactive</param>
        void SetActiveIndicator(IPPDMEntity entity, bool isActive);

        /// <summary>
        /// Sets the effective and expiry dates for row-level temporal tracking
        /// </summary>
        /// <param name="entity">The PPDM entity</param>
        /// <param name="effectiveDate">Effective date (optional)</param>
        /// <param name="expiryDate">Expiry date (optional)</param>
        void SetRowEffectiveDates(IPPDMEntity entity, DateTime? effectiveDate = null, DateTime? expiryDate = null);

        /// <summary>
        /// Generates a PPDM GUID if one doesn't exist
        /// </summary>
        /// <param name="entity">The PPDM entity</param>
        void GeneratePPDMGuid(IPPDMEntity entity);

        /// <summary>
        /// Sets the data quality indicator
        /// </summary>
        /// <param name="entity">The PPDM entity</param>
        /// <param name="qualityCode">Quality code (e.g., 'GOOD', 'FAIR', 'POOR')</param>
        void SetDataQuality(IPPDMEntity entity, string qualityCode);

        /// <summary>
        /// Prepares an entity for insertion (sets all creation-related columns)
        /// </summary>
        /// <param name="entity">The PPDM entity</param>
        /// <param name="userId">User identifier</param>
        void PrepareForInsert(IPPDMEntity entity, string userId);

        /// <summary>
        /// Prepares an entity for update (sets all change-related columns)
        /// </summary>
        /// <param name="entity">The PPDM entity</param>
        /// <param name="userId">User identifier</param>
        void PrepareForUpdate(IPPDMEntity entity, string userId);

        /// <summary>
        /// Soft deletes an entity by setting ACTIVE_IND to 'N' and updating change columns
        /// </summary>
        /// <param name="entity">The PPDM entity</param>
        /// <param name="userId">User identifier</param>
        void SoftDelete(IPPDMEntity entity, string userId);

        /// <summary>
        /// Sets expiry date to mark entity as expired
        /// </summary>
        /// <param name="entity">The PPDM entity</param>
        /// <param name="expiryDate">Expiry date (defaults to current date if null)</param>
        void SetExpired(IPPDMEntity entity, DateTime? expiryDate = null);
    }
}

