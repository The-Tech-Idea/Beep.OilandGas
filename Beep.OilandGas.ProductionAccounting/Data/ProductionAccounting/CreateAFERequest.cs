using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
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
}
