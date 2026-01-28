using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class TractParticipation : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the tract identifier.
        /// </summary>
        private string TractIdValue = string.Empty;

        public string TractId

        {

            get { return this.TractIdValue; }

            set { SetProperty(ref TractIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        private string UnitIdValue = string.Empty;

        public string UnitId

        {

            get { return this.UnitIdValue; }

            set { SetProperty(ref UnitIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the participating area identifier.
        /// </summary>
        private string ParticipatingAreaIdValue = string.Empty;

        public string ParticipatingAreaId

        {

            get { return this.ParticipatingAreaIdValue; }

            set { SetProperty(ref ParticipatingAreaIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the tract participation percentage (0-100).
        /// </summary>
        private decimal ParticipationPercentageValue;

        public decimal ParticipationPercentage

        {

            get { return this.ParticipationPercentageValue; }

            set { SetProperty(ref ParticipationPercentageValue, value); }

        }

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the net revenue interest (decimal, 0-1).
        /// </summary>
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the tract acreage.
        /// </summary>
        private decimal? TractAcreageValue;

        public decimal? TractAcreage

        {

            get { return this.TractAcreageValue; }

            set { SetProperty(ref TractAcreageValue, value); }

        }
    }
}
