using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting.Cost
{
    /// <summary>
    /// Request DTO for creating a cost transaction
    /// </summary>
    public class CreateCostTransactionRequest : ModelEntityBase
    {
        private decimal CostAmountValue;

        public decimal CostAmount

        {

            get { return this.CostAmountValue; }

            set { SetProperty(ref CostAmountValue, value); }

        }
        private bool IsCapitalizedValue;

        public bool IsCapitalized

        {

            get { return this.IsCapitalizedValue; }

            set { SetProperty(ref IsCapitalizedValue, value); }

        }
        private bool IsCashValue;

        public bool IsCash

        {

            get { return this.IsCashValue; }

            set { SetProperty(ref IsCashValue, value); }

        }
        private DateTime? TransactionDateValue;

        public DateTime? TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for cost allocation
    /// </summary>
    public class CostAllocationRequest : ModelEntityBase
    {
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime AllocationDateValue;

        public DateTime AllocationDate

        {

            get { return this.AllocationDateValue; }

            set { SetProperty(ref AllocationDateValue, value); }

        }
        private string AllocationMethodValue = string.Empty;

        public string AllocationMethod

        {

            get { return this.AllocationMethodValue; }

            set { SetProperty(ref AllocationMethodValue, value); }

        }
        private decimal? TotalOperatingCostsValue;

        public decimal? TotalOperatingCosts

        {

            get { return this.TotalOperatingCostsValue; }

            set { SetProperty(ref TotalOperatingCostsValue, value); }

        }
        private decimal? TotalCapitalCostsValue;

        public decimal? TotalCapitalCosts

        {

            get { return this.TotalCapitalCostsValue; }

            set { SetProperty(ref TotalCapitalCostsValue, value); }

        }
    }
}







