using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CostAllocationTarget : ModelEntityBase
    {
        private string TargetIdValue = string.Empty;

        [Required(ErrorMessage = "TargetId is required")]
        public string TargetId

        {

            get { return this.TargetIdValue; }

            set { SetProperty(ref TargetIdValue, value); }

        }

        private string TargetTypeValue = string.Empty;


        public string TargetType


        {


            get { return this.TargetTypeValue; }


            set { SetProperty(ref TargetTypeValue, value); }


        } // Well, Lease, Property, etc.
        private decimal? AllocationPercentageValue;

        public decimal? AllocationPercentage

        {

            get { return this.AllocationPercentageValue; }

            set { SetProperty(ref AllocationPercentageValue, value); }

        }
        private decimal? AllocationAmountValue;

        public decimal? AllocationAmount

        {

            get { return this.AllocationAmountValue; }

            set { SetProperty(ref AllocationAmountValue, value); }

        }
    }
}
