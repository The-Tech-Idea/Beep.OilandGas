using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Stratigraphy
{
    public class StratUnit : ModelEntityBase
    {
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
        /// Stratigraphic Unit ID (Primary Key)
        /// </summary>
        private string STRAT_UNIT_IDValue;

        public string STRAT_UNIT_ID

        {

            get { return this.STRAT_UNIT_IDValue; }

            set { SetProperty(ref STRAT_UNIT_IDValue, value); }

        }

        /// <summary>
        /// Abbreviation
        /// </summary>
        private string ABBREVIATIONValue;

        public string ABBREVIATION

        {

            get { return this.ABBREVIATIONValue; }

            set { SetProperty(ref ABBREVIATIONValue, value); }

        }

        /// <summary>
        /// Short Name
        /// </summary>
        private string SHORT_NAMEValue;

        public string SHORT_NAME

        {

            get { return this.SHORT_NAMEValue; }

            set { SetProperty(ref SHORT_NAMEValue, value); }

        }

        /// <summary>
        /// Long Name
        /// </summary>
        private string LONG_NAMEValue;

        public string LONG_NAME

        {

            get { return this.LONG_NAMEValue; }

            set { SetProperty(ref LONG_NAMEValue, value); }

        }

        /// <summary>
        /// Description
        /// </summary>
        private string DESCRIPTIONValue;

        public string DESCRIPTION

        {

            get { return this.DESCRIPTIONValue; }

            set { SetProperty(ref DESCRIPTIONValue, value); }

        }

        /// <summary>
        /// Stratigraphic Type
        /// </summary>
        private string STRAT_TYPEValue;

        public string STRAT_TYPE

        {

            get { return this.STRAT_TYPEValue; }

            set { SetProperty(ref STRAT_TYPEValue, value); }

        }

        /// <summary>
        /// Stratigraphic Unit Type
        /// </summary>
        private string STRAT_UNIT_TYPEValue;

        public string STRAT_UNIT_TYPE

        {

            get { return this.STRAT_UNIT_TYPEValue; }

            set { SetProperty(ref STRAT_UNIT_TYPEValue, value); }

        }

        /// <summary>
        /// Stratigraphic Status
        /// </summary>
        private string STRAT_STATUSValue;

        public string STRAT_STATUS

        {

            get { return this.STRAT_STATUSValue; }

            set { SetProperty(ref STRAT_STATUSValue, value); }

        }

        /// <summary>
        /// Preferred Indicator ('Y' or 'N')
        /// </summary>
        private string PREFERRED_INDValue;

        public string PREFERRED_IND

        {

            get { return this.PREFERRED_INDValue; }

            set { SetProperty(ref PREFERRED_INDValue, value); }

        }

        /// <summary>
        /// Ordinal Age Code
        /// </summary>
        private decimal? ORDINAL_AGE_CODEValue;

        public decimal? ORDINAL_AGE_CODE

        {

            get { return this.ORDINAL_AGE_CODEValue; }

            set { SetProperty(ref ORDINAL_AGE_CODEValue, value); }

        }

        /// <summary>
        /// Current Status Date
        /// </summary>
        private DateTime? CURRENT_STATUS_DATEValue;

        public DateTime? CURRENT_STATUS_DATE

        {

            get { return this.CURRENT_STATUS_DATEValue; }

            set { SetProperty(ref CURRENT_STATUS_DATEValue, value); }

        }

        /// <summary>
        /// Effective Date
        /// </summary>

        /// <summary>
        /// Expiry Date
        /// </summary>

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

    }
}
