using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CostSharing : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets whether costs are shared based on working interest.
        /// </summary>
        private bool BasedOnWorkingInterestValue = true;

        public bool BasedOnWorkingInterest

        {

            get { return this.BasedOnWorkingInterestValue; }

            set { SetProperty(ref BasedOnWorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets whether costs are shared based on tract participation.
        /// </summary>
        private bool BasedOnTractParticipationValue = false;

        public bool BasedOnTractParticipation

        {

            get { return this.BasedOnTractParticipationValue; }

            set { SetProperty(ref BasedOnTractParticipationValue, value); }

        }

        /// <summary>
        /// Gets or sets the operator's overhead percentage (0-100).
        /// </summary>
        private decimal OperatorOverheadPercentageValue = 0m;

        public decimal OperatorOverheadPercentage

        {

            get { return this.OperatorOverheadPercentageValue; }

            set { SetProperty(ref OperatorOverheadPercentageValue, value); }

        }
    }
}
