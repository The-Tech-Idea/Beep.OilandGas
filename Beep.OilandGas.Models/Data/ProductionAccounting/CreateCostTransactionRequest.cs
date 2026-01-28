using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
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
