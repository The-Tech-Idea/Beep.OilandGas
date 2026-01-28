using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Stratigraphy
{
    public class StratWellSection : ModelEntityBase
    {
        /// <summary>
        /// Unique Well Identifier (UWI)
        /// </summary>
        private string UWIValue;

        public string UWI

        {

            get { return this.UWIValue; }

            set { SetProperty(ref UWIValue, value); }

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
        /// Interpretation ID
        /// </summary>
        private string INTERP_IDValue;

        public string INTERP_ID

        {

            get { return this.INTERP_IDValue; }

            set { SetProperty(ref INTERP_IDValue, value); }

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
        /// Top Depth Datum
        /// </summary>
        private string TOP_DEPTH_DATUMValue;

        public string TOP_DEPTH_DATUM

        {

            get { return this.TOP_DEPTH_DATUMValue; }

            set { SetProperty(ref TOP_DEPTH_DATUMValue, value); }

        }

        /// <summary>
        /// Base Depth Datum
        /// </summary>
        private string BASE_DEPTH_DATUMValue;

        public string BASE_DEPTH_DATUM

        {

            get { return this.BASE_DEPTH_DATUMValue; }

            set { SetProperty(ref BASE_DEPTH_DATUMValue, value); }

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

    }
}
