using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ImpairmentRequest : ModelEntityBase
    {
        private string PropertyIdValue = string.Empty;

        [Required(ErrorMessage = "PropertyId is required")]
        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        private decimal CarryingValueValue;


        [Required(ErrorMessage = "CarryingValue is required")]
        [Range(0, double.MaxValue, ErrorMessage = "CarryingValue must be greater than or equal to 0")]
        public decimal CarryingValue


        {


            get { return this.CarryingValueValue; }


            set { SetProperty(ref CarryingValueValue, value); }


        }

        private decimal FairValueValue;


        [Required(ErrorMessage = "FairValue is required")]
        [Range(0, double.MaxValue, ErrorMessage = "FairValue must be greater than or equal to 0")]
        public decimal FairValue


        {


            get { return this.FairValueValue; }


            set { SetProperty(ref FairValueValue, value); }


        }

        private DateTime? ImpairmentDateValue;


        public DateTime? ImpairmentDate


        {


            get { return this.ImpairmentDateValue; }


            set { SetProperty(ref ImpairmentDateValue, value); }


        }
        private string? ReasonValue;

        public string? Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
    }
}
