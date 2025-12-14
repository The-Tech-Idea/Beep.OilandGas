using System;
using System.ComponentModel;

namespace Beep.OilandGas.PPDM39.Core.Interfaces
{
    /// <summary>
    /// Base interface for all PPDM 3.9 entities
    /// Defines common properties that all PPDM entities should implement
    /// Note: PPDM model classes inherit from Entity (class) and implement this interface
    /// </summary>
    public interface IPPDMEntity : INotifyPropertyChanged
    {
        /// <summary>
        /// Active indicator - 'Y' for active, 'N' for inactive
        /// </summary>
        string ACTIVE_IND { get; set; }

        /// <summary>
        /// User who created the row
        /// </summary>
        string ROW_CREATED_BY { get; set; }

        /// <summary>
        /// Date and time when the row was created
        /// </summary>
        DateTime ROW_CREATED_DATE { get; set; }

        /// <summary>
        /// User who last changed the row
        /// </summary>
        string ROW_CHANGED_BY { get; set; }

        /// <summary>
        /// Date and time when the row was last changed
        /// </summary>
        DateTime ROW_CHANGED_DATE { get; set; }

        /// <summary>
        /// Effective date for the row
        /// </summary>
        DateTime ROW_EFFECTIVE_DATE { get; set; }

        /// <summary>
        /// Expiry date for the row
        /// </summary>
        DateTime ROW_EXPIRY_DATE { get; set; }

        /// <summary>
        /// Data quality indicator
        /// </summary>
        string ROW_QUALITY { get; set; }

        /// <summary>
        /// PPDM globally unique identifier
        /// </summary>
        string PPDM_GUID { get; set; }

        // Optional properties - not all PPDM entities have these
        // Models can implement these as needed
        
        /// <summary>
        /// Area identifier (optional)
        /// </summary>
        string AREA_ID { get; set; }

        /// <summary>
        /// Area type (optional)
        /// </summary>
        string AREA_TYPE { get; set; }

        /// <summary>
        /// Business associate identifier (optional)
        /// </summary>
        string BUSINESS_ASSOCIATE_ID { get; set; }

        /// <summary>
        /// Effective date for business purposes (optional)
        /// </summary>
        DateTime EFFECTIVE_DATE { get; set; }

        /// <summary>
        /// Expiry date for business purposes (optional)
        /// </summary>
        DateTime EXPIRY_DATE { get; set; }

        /// <summary>
        /// Data source (optional)
        /// </summary>
        string SOURCE { get; set; }

        /// <summary>
        /// Remarks or notes (optional)
        /// </summary>
        string REMARK { get; set; }
    }
}

