using System;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Request to create an AFE (Authorization for Expenditure)
    /// </summary>
    public class CreateAFERequest : ModelEntityBase
    {
        private string AfeNumberValue = string.Empty;

        [Required(ErrorMessage = "AfeNumber is required")]
        public string AfeNumber

        {

            get { return this.AfeNumberValue; }

            set { SetProperty(ref AfeNumberValue, value); }

        }

        private string? AfeNameValue;


        public string? AfeName


        {


            get { return this.AfeNameValue; }


            set { SetProperty(ref AfeNameValue, value); }


        }

        private string PropertyIdValue = string.Empty;


        [Required(ErrorMessage = "PropertyId is required")]
        public string PropertyId


        {


            get { return this.PropertyIdValue; }


            set { SetProperty(ref PropertyIdValue, value); }


        }

        private decimal BudgetAmountValue;


        [Range(0.01, double.MaxValue, ErrorMessage = "BudgetAmount must be greater than 0")]
        public decimal BudgetAmount


        {


            get { return this.BudgetAmountValue; }


            set { SetProperty(ref BudgetAmountValue, value); }


        }

        private DateTime? EffectiveDateValue;


        public DateTime? EffectiveDate


        {


            get { return this.EffectiveDateValue; }


            set { SetProperty(ref EffectiveDateValue, value); }


        }
    }

    /// <summary>
    /// Request for cost allocation
    /// </summary>
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

    /// <summary>
    /// Cost allocation target
    /// </summary>
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

    /// <summary>
    /// Request to create a cost transaction
    /// </summary>
    public class CreateCostTransactionRequest : ModelEntityBase
    {
        private DateTime TransactionDateValue;

        [Required(ErrorMessage = "TransactionDate is required")]
        public DateTime TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }

        private string CostCategoryValue = string.Empty;


        [Required(ErrorMessage = "CostCategory is required")]
        public string CostCategory


        {


            get { return this.CostCategoryValue; }


            set { SetProperty(ref CostCategoryValue, value); }


        }

        private decimal AmountValue;


        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount


        {


            get { return this.AmountValue; }


            set { SetProperty(ref AmountValue, value); }


        }

        private string? PropertyIdValue;


        public string? PropertyId


        {


            get { return this.PropertyIdValue; }


            set { SetProperty(ref PropertyIdValue, value); }


        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? LeaseIdValue;

        public string? LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string? VendorIdValue;

        public string? VendorId

        {

            get { return this.VendorIdValue; }

            set { SetProperty(ref VendorIdValue, value); }

        }
        private string? InvoiceNumberValue;

        public string? InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

        }
    }
}







