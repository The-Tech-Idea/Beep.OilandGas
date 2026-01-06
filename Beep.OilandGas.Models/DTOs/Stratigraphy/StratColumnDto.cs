using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Stratigraphy
{
    /// <summary>
    /// Data Transfer Object for Stratigraphic Column
    /// </summary>
    public class StratColumnDto
    {
        /// <summary>
        /// Stratigraphic Column ID (Primary Key)
        /// </summary>
        public string STRAT_COLUMN_ID { get; set; }

        /// <summary>
        /// Stratigraphic Column Name
        /// </summary>
        public string STRAT_COLUMN_NAME { get; set; }

        /// <summary>
        /// Stratigraphic Column Type
        /// </summary>
        public string STRAT_COLUMN_TYPE { get; set; }

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
        /// Source Document ID
        /// </summary>
        public string SOURCE_DOCUMENT_ID { get; set; }

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

        /// <summary>
        /// Column Units (related entities)
        /// </summary>
        public List<StratColumnUnitDto> Units { get; set; } = new List<StratColumnUnitDto>();
    }

    /// <summary>
    /// Data Transfer Object for Stratigraphic Column Unit
    /// </summary>
    public class StratColumnUnitDto
    {
        /// <summary>
        /// Stratigraphic Column ID
        /// </summary>
        public string STRAT_COLUMN_ID { get; set; }

        /// <summary>
        /// Stratigraphic Name Set ID
        /// </summary>
        public string STRAT_NAME_SET_ID { get; set; }

        /// <summary>
        /// Stratigraphic Unit ID
        /// </summary>
        public string STRAT_UNIT_ID { get; set; }

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
        /// Active Indicator
        /// </summary>
        public string ACTIVE_IND { get; set; }
    }
}




