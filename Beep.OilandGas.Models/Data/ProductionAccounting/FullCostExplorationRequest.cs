using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class FullCostExplorationRequest : ModelEntityBase
    {
        private string PropertyIdValue = string.Empty;

        [Required(ErrorMessage = "PropertyId is required")]
        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        private decimal ExplorationCostValue;


        [Required(ErrorMessage = "ExplorationCost is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "ExplorationCost must be greater than 0")]
        public decimal ExplorationCost


        {


            get { return this.ExplorationCostValue; }


            set { SetProperty(ref ExplorationCostValue, value); }


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
