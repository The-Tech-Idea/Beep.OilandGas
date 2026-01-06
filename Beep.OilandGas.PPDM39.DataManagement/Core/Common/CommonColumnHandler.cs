using System;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Common
{
    /// <summary>
    /// Handles common columns across all PPDM entities
    /// Automatically manages audit columns, metadata, and temporal tracking
    /// </summary>
    public class CommonColumnHandler : ICommonColumnHandler
    {
        /// <summary>
        /// Sets the created columns for a new entity
        /// </summary>
        public void SetCreatedColumns(IPPDMEntity entity, string userId)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            entity.ROW_CREATED_BY = userId;
            entity.ROW_CREATED_DATE = DateTime.UtcNow;

            // Also set changed columns on creation
            SetChangedColumns(entity, userId);
        }

        /// <summary>
        /// Sets the changed columns for an updated entity
        /// </summary>
        public void SetChangedColumns(IPPDMEntity entity, string userId)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            entity.ROW_CHANGED_BY = userId;
            entity.ROW_CHANGED_DATE = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets the active indicator
        /// </summary>
        public void SetActiveIndicator(IPPDMEntity entity, bool isActive)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.ACTIVE_IND = isActive ? "Y" : "N";
        }

        /// <summary>
        /// Sets the effective and expiry dates for row-level temporal tracking
        /// </summary>
        public void SetRowEffectiveDates(IPPDMEntity entity, DateTime? effectiveDate = null, DateTime? expiryDate = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.ROW_EFFECTIVE_DATE = effectiveDate ?? DateTime.UtcNow;
            entity.ROW_EXPIRY_DATE = expiryDate ?? DateTime.MinValue;
        }

        /// <summary>
        /// Generates a PPDM GUID if one doesn't exist
        /// </summary>
        public void GeneratePPDMGuid(IPPDMEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.PPDM_GUID))
            {
                entity.PPDM_GUID = Guid.NewGuid().ToString().ToUpper();
            }
        }

        /// <summary>
        /// Sets the data quality indicator
        /// </summary>
        public void SetDataQuality(IPPDMEntity entity, string qualityCode)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.ROW_QUALITY = qualityCode;
        }

        /// <summary>
        /// Prepares an entity for insertion (sets all creation-related columns)
        /// </summary>
        public void PrepareForInsert(IPPDMEntity entity, string userId)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Generate GUID if missing
            GeneratePPDMGuid(entity);

            // Set created columns
            SetCreatedColumns(entity, userId);

            // Set active indicator if not already set
            if (string.IsNullOrWhiteSpace(entity.ACTIVE_IND))
            {
                SetActiveIndicator(entity, true);
            }

            // Set row effective date if not set (default to MinValue means not set)
            if (entity.ROW_EFFECTIVE_DATE == DateTime.MinValue)
            {
                entity.ROW_EFFECTIVE_DATE = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Prepares an entity for update (sets all change-related columns)
        /// </summary>
        public void PrepareForUpdate(IPPDMEntity entity, string userId)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Set changed columns
            SetChangedColumns(entity, userId);
        }

        /// <summary>
        /// Soft deletes an entity by setting ACTIVE_IND to 'N' and updating change columns
        /// </summary>
        public void SoftDelete(IPPDMEntity entity, string userId)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            SetActiveIndicator(entity, false);
            SetChangedColumns(entity, userId);

            // Optionally set expiry date (default to MinValue means not set)
            if (entity.ROW_EXPIRY_DATE == DateTime.MinValue)
            {
                entity.ROW_EXPIRY_DATE = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Sets expiry date to mark entity as expired
        /// </summary>
        public void SetExpired(IPPDMEntity entity, DateTime? expiryDate = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.ROW_EXPIRY_DATE = expiryDate ?? DateTime.UtcNow;
        }

        public void SetCommonColumns(RECEIVABLE entity, string connName)
        {
            throw new NotImplementedException();
        }
    }
}

