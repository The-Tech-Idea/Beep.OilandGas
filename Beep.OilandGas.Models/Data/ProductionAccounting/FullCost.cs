using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Request to create a cost center.
    /// </summary>
    public class CreateCostCenterRequest : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private string CostCenterNameValue = string.Empty;

        public string CostCenterName

        {

            get { return this.CostCenterNameValue; }

            set { SetProperty(ref CostCenterNameValue, value); }

        }
        private string CostCenterTypeValue = string.Empty;

        public string CostCenterType

        {

            get { return this.CostCenterTypeValue; }

            set { SetProperty(ref CostCenterTypeValue, value); }

        } // Country, Region, Field
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    /// <summary>
    /// Cost center rollup summary.
    /// </summary>
    public class CostCenterRollup : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private string CostCenterNameValue = string.Empty;

        public string CostCenterName

        {

            get { return this.CostCenterNameValue; }

            set { SetProperty(ref CostCenterNameValue, value); }

        }
        private decimal AcquisitionCostsValue;

        public decimal AcquisitionCosts

        {

            get { return this.AcquisitionCostsValue; }

            set { SetProperty(ref AcquisitionCostsValue, value); }

        }
        private decimal ExplorationCostsValue;

        public decimal ExplorationCosts

        {

            get { return this.ExplorationCostsValue; }

            set { SetProperty(ref ExplorationCostsValue, value); }

        }
        private decimal DevelopmentCostsValue;

        public decimal DevelopmentCosts

        {

            get { return this.DevelopmentCostsValue; }

            set { SetProperty(ref DevelopmentCostsValue, value); }

        }
        private decimal TotalCapitalizedCostsValue;

        public decimal TotalCapitalizedCosts

        {

            get { return this.TotalCapitalizedCostsValue; }

            set { SetProperty(ref TotalCapitalizedCostsValue, value); }

        }
        private decimal AccumulatedAmortizationValue;

        public decimal AccumulatedAmortization

        {

            get { return this.AccumulatedAmortizationValue; }

            set { SetProperty(ref AccumulatedAmortizationValue, value); }

        }
        private decimal NetCapitalizedCostsValue;

        public decimal NetCapitalizedCosts

        {

            get { return this.NetCapitalizedCostsValue; }

            set { SetProperty(ref NetCapitalizedCostsValue, value); }

        }
        private DateTime? AsOfDateValue;

        public DateTime? AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

        }
    }

    /// <summary>
    /// Result of a ceiling test.
    /// </summary>
    public class CeilingTestResult : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private decimal NetCapitalizedCostsValue;

        public decimal NetCapitalizedCosts

        {

            get { return this.NetCapitalizedCostsValue; }

            set { SetProperty(ref NetCapitalizedCostsValue, value); }

        }
        private decimal PresentValueOfFutureNetRevenuesValue;

        public decimal PresentValueOfFutureNetRevenues

        {

            get { return this.PresentValueOfFutureNetRevenuesValue; }

            set { SetProperty(ref PresentValueOfFutureNetRevenuesValue, value); }

        }
        private decimal CeilingValue;

        public decimal Ceiling

        {

            get { return this.CeilingValue; }

            set { SetProperty(ref CeilingValue, value); }

        }
        private bool ImpairmentNeededValue;

        public bool ImpairmentNeeded

        {

            get { return this.ImpairmentNeededValue; }

            set { SetProperty(ref ImpairmentNeededValue, value); }

        }
        private decimal ImpairmentAmountValue;

        public decimal ImpairmentAmount

        {

            get { return this.ImpairmentAmountValue; }

            set { SetProperty(ref ImpairmentAmountValue, value); }

        }
        private DateTime TestDateValue = DateTime.UtcNow;

        public DateTime TestDate

        {

            get { return this.TestDateValue; }

            set { SetProperty(ref TestDateValue, value); }

        }
    }
}








