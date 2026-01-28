using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class RevenueSharing : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets whether revenue is shared based on net revenue interest.
        /// </summary>
        private bool BasedOnNetRevenueInterestValue = true;

        public bool BasedOnNetRevenueInterest

        {

            get { return this.BasedOnNetRevenueInterestValue; }

            set { SetProperty(ref BasedOnNetRevenueInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets whether revenue is shared based on tract participation.
        /// </summary>
        private bool BasedOnTractParticipationValue = false;

        public bool BasedOnTractParticipation

        {

            get { return this.BasedOnTractParticipationValue; }

            set { SetProperty(ref BasedOnTractParticipationValue, value); }

        }
    }
}
