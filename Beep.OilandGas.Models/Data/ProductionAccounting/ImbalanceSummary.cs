using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ImbalanceSummary : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the total volume in barrels.
        /// </summary>
        private decimal TotalVolumeValue;

        public decimal TotalVolume

        {

            get { return this.TotalVolumeValue; }

            set { SetProperty(ref TotalVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the number of transactions.
        /// </summary>
        private int TransactionCountValue;

        public int TransactionCount

        {

            get { return this.TransactionCountValue; }

            set { SetProperty(ref TransactionCountValue, value); }

        }

        /// <summary>
        /// Gets or sets the average daily volume.
        /// </summary>
        private decimal AverageDailyVolumeValue;

        public decimal AverageDailyVolume

        {

            get { return this.AverageDailyVolumeValue; }

            set { SetProperty(ref AverageDailyVolumeValue, value); }

        }
    }
}
