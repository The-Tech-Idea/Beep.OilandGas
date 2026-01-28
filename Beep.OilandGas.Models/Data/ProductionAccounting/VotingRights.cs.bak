using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class VotingRights : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets whether voting is based on working interest.
        /// </summary>
        private bool BasedOnWorkingInterestValue = true;

        public bool BasedOnWorkingInterest

        {

            get { return this.BasedOnWorkingInterestValue; }

            set { SetProperty(ref BasedOnWorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the minimum voting threshold percentage (0-100).
        /// </summary>
        private decimal MinimumVotingThresholdValue = 50m;

        public decimal MinimumVotingThreshold

        {

            get { return this.MinimumVotingThresholdValue; }

            set { SetProperty(ref MinimumVotingThresholdValue, value); }

        }

        /// <summary>
        /// Gets or sets whether unanimous consent is required for major decisions.
        /// </summary>
        private bool UnanimousConsentRequiredValue = false;

        public bool UnanimousConsentRequired

        {

            get { return this.UnanimousConsentRequiredValue; }

            set { SetProperty(ref UnanimousConsentRequiredValue, value); }

        }
    }
}
