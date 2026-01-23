using System;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Request for amortization calculation
    /// </summary>
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

    /// <summary>
    /// Request for impairment calculation
    /// </summary>
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

    /// <summary>
    /// Request for full cost exploration
    /// </summary>
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

    /// <summary>
    /// Request for full cost development
    /// </summary>
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

    /// <summary>
    /// Request for full cost acquisition
    /// </summary>
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

    /// <summary>
    /// Request for ceiling test
    /// </summary>
    public class CeilingTestRequest : ModelEntityBase
    {
        private string PropertyIdValue = string.Empty;

        [Required(ErrorMessage = "PropertyId is required")]
        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        private DateTime? TestDateValue;


        public DateTime? TestDate


        {


            get { return this.TestDateValue; }


            set { SetProperty(ref TestDateValue, value); }


        }
        private decimal? OilPriceValue;

        public decimal? OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private decimal? GasPriceValue;

        public decimal? GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
    }
}







