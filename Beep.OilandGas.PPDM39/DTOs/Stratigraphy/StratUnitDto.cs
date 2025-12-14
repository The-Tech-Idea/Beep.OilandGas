using System;

namespace Beep.OilandGas.PPDM39.DTOs.Stratigraphy
{
    /// <summary>
    /// Data Transfer Object for Stratigraphic Unit
    /// </summary>
    public class StratUnitDto
    {
        /// <summary>
        /// Stratigraphic Name Set ID
        /// </summary>
        public string STRAT_NAME_SET_ID { get; set; }

        /// <summary>
        /// Stratigraphic Unit ID (Primary Key)
        /// </summary>
        public string STRAT_UNIT_ID { get; set; }

        /// <summary>
        /// Abbreviation
        /// </summary>
        public string ABBREVIATION { get; set; }

        /// <summary>
        /// Short Name
        /// </summary>
        public string SHORT_NAME { get; set; }

        /// <summary>
        /// Long Name
        /// </summary>
        public string LONG_NAME { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string DESCRIPTION { get; set; }

        /// <summary>
        /// Stratigraphic Type
        /// </summary>
        public string STRAT_TYPE { get; set; }

        /// <summary>
        /// Stratigraphic Unit Type
        /// </summary>
        public string STRAT_UNIT_TYPE { get; set; }

        /// <summary>
        /// Stratigraphic Status
        /// </summary>
        public string STRAT_STATUS { get; set; }

        /// <summary>
        /// Preferred Indicator ('Y' or 'N')
        /// </summary>
        public string PREFERRED_IND { get; set; }

        /// <summary>
        /// Ordinal Age Code
        /// </summary>
        public decimal? ORDINAL_AGE_CODE { get; set; }

        /// <summary>
        /// Current Status Date
        /// </summary>
        public DateTime? CURRENT_STATUS_DATE { get; set; }

        /// <summary>
        /// Effective Date
        /// </summary>
        public DateTime? EFFECTIVE_DATE { get; set; }

        /// <summary>
        /// Expiry Date
        /// </summary>
        public DateTime? EXPIRY_DATE { get; set; }

        /// <summary>
        /// Area ID
        /// </summary>
        public string AREA_ID { get; set; }

        /// <summary>
        /// Area Type
        /// </summary>
        public string AREA_TYPE { get; set; }

        /// <summary>
        /// Business Associate ID
        /// </summary>
        public string BUSINESS_ASSOCIATE_ID { get; set; }

        /// <summary>
        /// Source
        /// </summary>
        public string SOURCE { get; set; }

        /// <summary>
        /// Remark
        /// </summary>
        public string REMARK { get; set; }

        /// <summary>
        /// Active Indicator ('Y' or 'N')
        /// </summary>
        public string ACTIVE_IND { get; set; }

        /// <summary>
        /// PPDM GUID
        /// </summary>
        public string PPDM_GUID { get; set; }

        /// <summary>
        /// Row Created By
        /// </summary>
        public string ROW_CREATED_BY { get; set; }

        /// <summary>
        /// Row Created Date
        /// </summary>
        public DateTime? ROW_CREATED_DATE { get; set; }

        /// <summary>
        /// Row Changed By
        /// </summary>
        public string ROW_CHANGED_BY { get; set; }

        /// <summary>
        /// Row Changed Date
        /// </summary>
        public DateTime? ROW_CHANGED_DATE { get; set; }
    }
}

