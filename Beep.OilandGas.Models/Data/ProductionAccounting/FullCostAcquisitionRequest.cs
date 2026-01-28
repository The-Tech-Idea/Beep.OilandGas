using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class FullCostAcquisitionRequest : ModelEntityBase
    {
        private string PropertyIdValue = string.Empty;

        [Required(ErrorMessage = "PropertyId is required")]
        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        private decimal AcquisitionCostValue;


        [Required(ErrorMessage = "AcquisitionCost is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "AcquisitionCost must be greater than 0")]
        public decimal AcquisitionCost


        {


            get { return this.AcquisitionCostValue; }


            set { SetProperty(ref AcquisitionCostValue, value); }


        }

        private DateTime? AcquisitionDateValue;


        public DateTime? AcquisitionDate


        {


            get { return this.AcquisitionDateValue; }


            set { SetProperty(ref AcquisitionDateValue, value); }


        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
