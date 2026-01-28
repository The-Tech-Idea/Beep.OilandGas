using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class GovernmentalRoyaltyData : ModelEntityBase
    {
        private decimal RoyaltyVolumeValue;

        public decimal RoyaltyVolume

        {

            get { return this.RoyaltyVolumeValue; }

            set { SetProperty(ref RoyaltyVolumeValue, value); }

        }
        private decimal RoyaltyValueValue;

        public decimal RoyaltyValue

        {

            get { return this.RoyaltyValueValue; }

            set { SetProperty(ref RoyaltyValueValue, value); }

        }
        private decimal RoyaltyRateValue;

        public decimal RoyaltyRate

        {

            get { return this.RoyaltyRateValue; }

            set { SetProperty(ref RoyaltyRateValue, value); }

        }
    }
}
