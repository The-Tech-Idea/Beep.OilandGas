using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Stratigraphy
{
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
