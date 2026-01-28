using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ExchangeTerms : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the exchange ratio (if applicable).
        /// </summary>
        private decimal? ExchangeRatioValue;

        public decimal? ExchangeRatio

        {

            get { return this.ExchangeRatioValue; }

            set { SetProperty(ref ExchangeRatioValue, value); }

        }

        /// <summary>
        /// Gets or sets the minimum volume per exchange in barrels.
        /// </summary>
        private decimal? MinimumVolumeValue;

        public decimal? MinimumVolume

        {

            get { return this.MinimumVolumeValue; }

            set { SetProperty(ref MinimumVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum volume per exchange in barrels.
        /// </summary>
        private decimal? MaximumVolumeValue;

        public decimal? MaximumVolume

        {

            get { return this.MaximumVolumeValue; }

            set { SetProperty(ref MaximumVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the quality specifications.
        /// </summary>
        private string? QualitySpecificationsValue;

        public string? QualitySpecifications

        {

            get { return this.QualitySpecificationsValue; }

            set { SetProperty(ref QualitySpecificationsValue, value); }

        }

        /// <summary>
        /// Gets or sets the notice period in days.
        /// </summary>
        private int NoticePeriodDaysValue = 30;

        public int NoticePeriodDays

        {

            get { return this.NoticePeriodDaysValue; }

            set { SetProperty(ref NoticePeriodDaysValue, value); }

        }
    }
}
