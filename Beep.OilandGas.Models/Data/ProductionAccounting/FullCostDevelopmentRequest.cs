using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class FullCostDevelopmentRequest : ModelEntityBase
    {
        private string PropertyIdValue = string.Empty;

        [Required(ErrorMessage = "PropertyId is required")]
        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        private decimal DevelopmentCostValue;


        [Required(ErrorMessage = "DevelopmentCost is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "DevelopmentCost must be greater than 0")]
        public decimal DevelopmentCost


        {


            get { return this.DevelopmentCostValue; }


            set { SetProperty(ref DevelopmentCostValue, value); }


        }

        private DateTime? CostDateValue;


        public DateTime? CostDate


        {


            get { return this.CostDateValue; }


            set { SetProperty(ref CostDateValue, value); }


        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
