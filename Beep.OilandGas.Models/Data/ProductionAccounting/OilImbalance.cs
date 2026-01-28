using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class OilImbalance : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the imbalance identifier.
        /// </summary>
        private string ImbalanceIdValue = string.Empty;

        public string ImbalanceId

        {

            get { return this.ImbalanceIdValue; }

            set { SetProperty(ref ImbalanceIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the period start date.
        /// </summary>
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }

        /// <summary>
        /// Gets or sets the period end date.
        /// </summary>
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }

        /// <summary>
        /// Gets or sets the nominated volume in barrels.
        /// </summary>
        private decimal NominatedVolumeValue;

        public decimal NominatedVolume

        {

            get { return this.NominatedVolumeValue; }

            set { SetProperty(ref NominatedVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the actual volume in barrels.
        /// </summary>
        private decimal ActualVolumeValue;

        public decimal ActualVolume

        {

            get { return this.ActualVolumeValue; }

            set { SetProperty(ref ActualVolumeValue, value); }

        }

        /// <summary>
        /// Gets the imbalance amount (actual - nominated).
        /// </summary>
        public decimal ImbalanceAmount => ActualVolume - NominatedVolume;

        /// <summary>
        /// Gets the imbalance percentage.
        /// </summary>
        public decimal ImbalancePercentage => NominatedVolume > 0
            ? (ImbalanceAmount / NominatedVolume) * 100m
            : 0;

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        private ImbalanceStatus StatusValue = ImbalanceStatus.PendingReconciliation;

        public ImbalanceStatus Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }

        /// <summary>
        /// Gets or sets the tolerance percentage (0-100).
        /// </summary>
        private decimal TolerancePercentageValue = 2.0m;

        public decimal TolerancePercentage

        {

            get { return this.TolerancePercentageValue; }

            set { SetProperty(ref TolerancePercentageValue, value); }

        }

        /// <summary>
        /// Gets whether the imbalance is within tolerance.
        /// </summary>
        public bool IsWithinTolerance => Math.Abs(ImbalancePercentage) <= TolerancePercentage;
    }
}
