using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class AmortizationCalculationRequest : ModelEntityBase
    {
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        private decimal NetCapitalizedCostsValue;


        [Required(ErrorMessage = "NetCapitalizedCosts is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "NetCapitalizedCosts must be greater than 0")]
        public decimal NetCapitalizedCosts


        {


            get { return this.NetCapitalizedCostsValue; }


            set { SetProperty(ref NetCapitalizedCostsValue, value); }


        }

        private decimal TotalProvedReservesBOEValue;


        [Required(ErrorMessage = "TotalProvedReservesBOE is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "TotalProvedReservesBOE must be greater than 0")]
        public decimal TotalProvedReservesBOE


        {


            get { return this.TotalProvedReservesBOEValue; }


            set { SetProperty(ref TotalProvedReservesBOEValue, value); }


        }

        private decimal ProductionBOEValue;


        [Required(ErrorMessage = "ProductionBOE is required")]
        [Range(0, double.MaxValue, ErrorMessage = "ProductionBOE must be greater than or equal to 0")]
        public decimal ProductionBOE


        {


            get { return this.ProductionBOEValue; }


            set { SetProperty(ref ProductionBOEValue, value); }


        }

        private DateTime? CalculationDateValue;


        public DateTime? CalculationDate


        {


            get { return this.CalculationDateValue; }


            set { SetProperty(ref CalculationDateValue, value); }


        }
    }
}
