using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class JointInterestOwner : ModelEntityBase
    {
        private string OwnerIdValue = string.Empty;

        public string OwnerId

        {

            get { return this.OwnerIdValue; }

            set { SetProperty(ref OwnerIdValue, value); }

        }
        private string OwnerNameValue = string.Empty;

        public string OwnerName

        {

            get { return this.OwnerNameValue; }

            set { SetProperty(ref OwnerNameValue, value); }

        }
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }
        private decimal AllocatedVolumeValue;

        public decimal AllocatedVolume

        {

            get { return this.AllocatedVolumeValue; }

            set { SetProperty(ref AllocatedVolumeValue, value); }

        }
        private decimal AllocatedRevenueValue;

        public decimal AllocatedRevenue

        {

            get { return this.AllocatedRevenueValue; }

            set { SetProperty(ref AllocatedRevenueValue, value); }

        }
        private decimal AllocatedCostsValue;

        public decimal AllocatedCosts

        {

            get { return this.AllocatedCostsValue; }

            set { SetProperty(ref AllocatedCostsValue, value); }

        }
        private decimal NetAmountValue;

        public decimal NetAmount

        {

            get { return this.NetAmountValue; }

            set { SetProperty(ref NetAmountValue, value); }

        }
    }
}
