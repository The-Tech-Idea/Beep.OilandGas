using System;

namespace Beep.OilandGas.Models.DTOs.Stratigraphy
{
    /// <summary>
    /// Data Transfer Object for Well Stratigraphic Section
    /// Represents stratigraphic interpretations for wells
    /// </summary>
    public class StratWellSectionDto
    {
        /// <summary>
        /// Unique Well Identifier (UWI)
        /// </summary>
        public string UWI { get; set; }

        /// <summary>
        /// Stratigraphic Name Set ID
        /// </summary>
        public string STRAT_NAME_SET_ID { get; set; }

        /// <summary>
        /// Stratigraphic Unit ID
        /// </summary>
        public string STRAT_UNIT_ID { get; set; }

        /// <summary>
        /// Interpretation ID
        /// </summary>
        public string INTERP_ID { get; set; }

        /// <summary>
        /// Top Depth
        /// </summary>
        public decimal? TOP_DEPTH { get; set; }

        /// <summary>
        /// Base Depth
        /// </summary>
        public decimal? BASE_DEPTH { get; set; }

        /// <summary>
        /// Depth Unit of Measure
        /// </summary>
        public string DEPTH_OUOM { get; set; }

        /// <summary>
        /// Top Depth Datum
        /// </summary>
        public string TOP_DEPTH_DATUM { get; set; }

        /// <summary>
        /// Base Depth Datum
        /// </summary>
        public string BASE_DEPTH_DATUM { get; set; }

        /// <summary>
        /// Effective Date
        /// </summary>
        public DateTime? EFFECTIVE_DATE { get; set; }

        /// <summary>
        /// Expiry Date
        /// </summary>
        public DateTime? EXPIRY_DATE { get; set; }

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




