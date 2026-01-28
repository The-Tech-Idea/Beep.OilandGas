using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CostAllocationRequest : ModelEntityBase
    {
        private string CostTransactionIdValue = string.Empty;

        [Required(ErrorMessage = "CostTransactionId is required")]
        public string CostTransactionId

        {

            get { return this.CostTransactionIdValue; }

            set { SetProperty(ref CostTransactionIdValue, value); }

        }

        private string AllocationMethodValue = string.Empty;


        [Required(ErrorMessage = "AllocationMethod is required")]
        public string AllocationMethod


        {


            get { return this.AllocationMethodValue; }


            set { SetProperty(ref AllocationMethodValue, value); }


        }

        private List<CostAllocationTarget> TargetsValue = new();


        public List<CostAllocationTarget> Targets


        {


            get { return this.TargetsValue; }


            set { SetProperty(ref TargetsValue, value); }


        }
    }
}
