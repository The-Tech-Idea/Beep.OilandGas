using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Stratigraphy
{
    /// <summary>
    /// Data Transfer Object for Stratigraphic Column
    /// </summary>
    public class StratColumn : ModelEntityBase
    {
        /// <summary>
        /// Stratigraphic Column ID (Primary Key)
        /// </summary>
        private string STRAT_COLUMN_IDValue;

        public string STRAT_COLUMN_ID

        {

            get { return this.STRAT_COLUMN_IDValue; }

            set { SetProperty(ref STRAT_COLUMN_IDValue, value); }

        }

        /// <summary>
        /// Stratigraphic Column Name
        /// </summary>
        private string STRAT_COLUMN_NAMEValue;

        public string STRAT_COLUMN_NAME

        {

            get { return this.STRAT_COLUMN_NAMEValue; }

            set { SetProperty(ref STRAT_COLUMN_NAMEValue, value); }

        }

        /// <summary>
        /// Stratigraphic Column Type
        /// </summary>
        private string STRAT_COLUMN_TYPEValue;

        public string STRAT_COLUMN_TYPE

        {

            get { return this.STRAT_COLUMN_TYPEValue; }

            set { SetProperty(ref STRAT_COLUMN_TYPEValue, value); }

        }

        /// <summary>
        /// Area ID
        /// </summary>
        private string AREA_IDValue;

        public string AREA_ID

        {

            get { return this.AREA_IDValue; }

            set { SetProperty(ref AREA_IDValue, value); }

        }

        /// <summary>
        /// Area Type
        /// </summary>
        private string AREA_TYPEValue;

        public string AREA_TYPE

        {

            get { return this.AREA_TYPEValue; }

            set { SetProperty(ref AREA_TYPEValue, value); }

        }

        /// <summary>
        /// Business Associate ID
        /// </summary>
        private string BUSINESS_ASSOCIATE_IDValue;

        public string BUSINESS_ASSOCIATE_ID

        {

            get { return this.BUSINESS_ASSOCIATE_IDValue; }

            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }

        }

        /// <summary>
        /// Source
        /// </summary>

        /// <summary>
        /// Source Document ID
        /// </summary>
        private string SOURCE_DOCUMENT_IDValue;

        public string SOURCE_DOCUMENT_ID

        {

            get { return this.SOURCE_DOCUMENT_IDValue; }

            set { SetProperty(ref SOURCE_DOCUMENT_IDValue, value); }

        }

        /// <summary>
        /// Effective Date
        /// </summary>

        /// <summary>
        /// Expiry Date
        /// </summary>

        /// <summary>
        /// Remark
        /// </summary>

        /// <summary>
        /// Active Indicator ('Y' or 'N')
        /// </summary>

        /// <summary>
        /// PPDM GUID
        /// </summary>

        /// <summary>
        /// Row Created By
        /// </summary>

        /// <summary>
        /// Row Created Date
        /// </summary>

        /// <summary>
        /// Row Changed By
        /// </summary>

        /// <summary>
        /// Row Changed Date
        /// </summary>

        /// <summary>
        /// Column Units (related entities)
        /// </summary>
        private List<StratColumnUnit> UnitsValue = new List<StratColumnUnit>();

        public List<StratColumnUnit> Units

        {

            get { return this.UnitsValue; }

            set { SetProperty(ref UnitsValue, value); }

        }
    }

    /// <summary>
    /// Data Transfer Object for Stratigraphic Column Unit
    /// </summary>
    public class StratColumnUnit : ModelEntityBase
    {
        /// <summary>
        /// Stratigraphic Column ID
        /// </summary>
        private string STRAT_COLUMN_IDValue;

        public string STRAT_COLUMN_ID

        {

            get { return this.STRAT_COLUMN_IDValue; }

            set { SetProperty(ref STRAT_COLUMN_IDValue, value); }

        }

        /// <summary>
        /// Stratigraphic Name Set ID
        /// </summary>
        private string STRAT_NAME_SET_IDValue;

        public string STRAT_NAME_SET_ID

        {

            get { return this.STRAT_NAME_SET_IDValue; }

            set { SetProperty(ref STRAT_NAME_SET_IDValue, value); }

        }

        /// <summary>
        /// Stratigraphic Unit ID
        /// </summary>
        private string STRAT_UNIT_IDValue;

        public string STRAT_UNIT_ID

        {

            get { return this.STRAT_UNIT_IDValue; }

            set { SetProperty(ref STRAT_UNIT_IDValue, value); }

        }

        /// <summary>
        /// Top Depth
        /// </summary>
        private decimal? TOP_DEPTHValue;

        public decimal? TOP_DEPTH

        {

            get { return this.TOP_DEPTHValue; }

            set { SetProperty(ref TOP_DEPTHValue, value); }

        }

        /// <summary>
        /// Base Depth
        /// </summary>
        private decimal? BASE_DEPTHValue;

        public decimal? BASE_DEPTH

        {

            get { return this.BASE_DEPTHValue; }

            set { SetProperty(ref BASE_DEPTHValue, value); }

        }

        /// <summary>
        /// Depth Unit of Measure
        /// </summary>
        private string DEPTH_OUOMValue;

        public string DEPTH_OUOM

        {

            get { return this.DEPTH_OUOMValue; }

            set { SetProperty(ref DEPTH_OUOMValue, value); }

        }

        /// <summary>
        /// Active Indicator
        /// </summary>

    }
}


