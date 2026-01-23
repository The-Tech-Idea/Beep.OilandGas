using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// DTO for interest capitalization data used in Successful Efforts accounting.
    /// </summary>
    public class InterestCapitalizationData : ModelEntityBase
    {
        /// <summary>
        /// Total construction in progress cost
        /// </summary>
        private decimal ConstructionCostValue;

        public decimal ConstructionCost

        {

            get { return this.ConstructionCostValue; }

            set { SetProperty(ref ConstructionCostValue, value); }

        }

        /// <summary>
        /// Period interest rate
        /// </summary>
        private decimal InterestRateValue;

        public decimal InterestRate

        {

            get { return this.InterestRateValue; }

            set { SetProperty(ref InterestRateValue, value); }

        }

        /// <summary>
        /// Number of periods to capitalize
        /// </summary>
        private int PeriodsValue;

        public int Periods

        {

            get { return this.PeriodsValue; }

            set { SetProperty(ref PeriodsValue, value); }

        }

        /// <summary>
        /// Weighted average cost of capital
        /// </summary>
        private decimal WACCValue;

        public decimal WACC

        {

            get { return this.WACCValue; }

            set { SetProperty(ref WACCValue, value); }

        }
    }

    /// <summary>
    /// DTO for production data used in amortization calculations.
    /// </summary>
    public class ProductionData : ModelEntityBase
    {
        /// <summary>
        /// Oil production in barrels
        /// </summary>
        private decimal OilVolumeValue;

        public decimal OilVolume

        {

            get { return this.OilVolumeValue; }

            set { SetProperty(ref OilVolumeValue, value); }

        }

        /// <summary>
        /// Gas production in Mcf
        /// </summary>
        private decimal GasVolumeValue;

        public decimal GasVolume

        {

            get { return this.GasVolumeValue; }

            set { SetProperty(ref GasVolumeValue, value); }

        }

        /// <summary>
        /// Water production in barrels
        /// </summary>
        private decimal WaterVolumeValue;

        public decimal WaterVolume

        {

            get { return this.WaterVolumeValue; }

            set { SetProperty(ref WaterVolumeValue, value); }

        }

        /// <summary>
        /// Production period date
        /// </summary>
        private DateTime PeriodDateValue;

        public DateTime PeriodDate

        {

            get { return this.PeriodDateValue; }

            set { SetProperty(ref PeriodDateValue, value); }

        }

        /// <summary>
        /// Oil API gravity
        /// </summary>
        private decimal? OilAPIGravityValue;

        public decimal? OilAPIGravity

        {

            get { return this.OilAPIGravityValue; }

            set { SetProperty(ref OilAPIGravityValue, value); }

        }

        /// <summary>
        /// Gas specific gravity
        /// </summary>
        private decimal? GasSpecificGravityValue;

        public decimal? GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
    }

    /// <summary>
    /// DTO for proved reserves data used in units-of-production amortization.
    /// </summary>
    public class ProvedReserves : ModelEntityBase
    {
        /// <summary>
        /// Proved oil reserves in barrels
        /// </summary>
        private decimal ProvedOilReservesValue;

        public decimal ProvedOilReserves

        {

            get { return this.ProvedOilReservesValue; }

            set { SetProperty(ref ProvedOilReservesValue, value); }

        }

        /// <summary>
        /// Proved gas reserves in Mcf
        /// </summary>
        private decimal ProvedGasReservesValue;

        public decimal ProvedGasReserves

        {

            get { return this.ProvedGasReservesValue; }

            set { SetProperty(ref ProvedGasReservesValue, value); }

        }

        /// <summary>
        /// Proved reserves as of date
        /// </summary>
        private DateTime AsOfDateValue;

        public DateTime AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

        }

        /// <summary>
        /// Reserve estimation method (volumetric, performance, or analog)
        /// </summary>
        private string EstimationMethodValue = "Volumetric";

        public string EstimationMethod

        {

            get { return this.EstimationMethodValue; }

            set { SetProperty(ref EstimationMethodValue, value); }

        }

        /// <summary>
        /// Confidence level (1P, 2P, 3P)
        /// </summary>
        private string ConfidenceLevelValue = "1P";

        public string ConfidenceLevel

        {

            get { return this.ConfidenceLevelValue; }

            set { SetProperty(ref ConfidenceLevelValue, value); }

        }
    }

    /// <summary>
    /// DTO for cost capitalization tracking in Successful Efforts method.
    /// </summary>
    public class CapitalizedCostData : ModelEntityBase
    {
        /// <summary>
        /// Well or project identifier
        /// </summary>
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }

        /// <summary>
        /// Drilling and completion costs
        /// </summary>
        private decimal DrillingCompletionCostsValue;

        public decimal DrillingCompletionCosts

        {

            get { return this.DrillingCompletionCostsValue; }

            set { SetProperty(ref DrillingCompletionCostsValue, value); }

        }

        /// <summary>
        /// Acquisition costs
        /// </summary>
        private decimal AcquisitionCostsValue;

        public decimal AcquisitionCosts

        {

            get { return this.AcquisitionCostsValue; }

            set { SetProperty(ref AcquisitionCostsValue, value); }

        }

        /// <summary>
        /// Capitalized interest
        /// </summary>
        private decimal CapitalizedInterestValue;

        public decimal CapitalizedInterest

        {

            get { return this.CapitalizedInterestValue; }

            set { SetProperty(ref CapitalizedInterestValue, value); }

        }

        /// <summary>
        /// Total capitalized costs before amortization
        /// </summary>
        private decimal TotalCapitalizedCostsValue;

        public decimal TotalCapitalizedCosts

        {

            get { return this.TotalCapitalizedCostsValue; }

            set { SetProperty(ref TotalCapitalizedCostsValue, value); }

        }

        /// <summary>
        /// Date capitalized
        /// </summary>
        private DateTime DateCapitalizedValue;

        public DateTime DateCapitalized

        {

            get { return this.DateCapitalizedValue; }

            set { SetProperty(ref DateCapitalizedValue, value); }

        }
    }

    /// <summary>
    /// DTO for dry hole or unsuccessful well costs.
    /// </summary>
    public class UnsuccessfulWellData : ModelEntityBase
    {
        /// <summary>
        /// Well identifier
        /// </summary>
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }

        /// <summary>
        /// Total drilling and completion costs (to be expensed)
        /// </summary>
        private decimal DrillingCompletionCostsValue;

        public decimal DrillingCompletionCosts

        {

            get { return this.DrillingCompletionCostsValue; }

            set { SetProperty(ref DrillingCompletionCostsValue, value); }

        }

        /// <summary>
        /// Acquisition costs
        /// </summary>
        private decimal AcquisitionCostsValue;

        public decimal AcquisitionCosts

        {

            get { return this.AcquisitionCostsValue; }

            set { SetProperty(ref AcquisitionCostsValue, value); }

        }

        /// <summary>
        /// Date well determined unsuccessful
        /// </summary>
        private DateTime DateDeterminedValue;

        public DateTime DateDetermined

        {

            get { return this.DateDeterminedValue; }

            set { SetProperty(ref DateDeterminedValue, value); }

        }

        /// <summary>
        /// Reason well was unsuccessful
        /// </summary>
        private string ReasonValue = string.Empty;

        public string Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
    }

    /// <summary>
    /// DTO for impairment analysis results (ASC 360).
    /// </summary>
    public class ImpairmentAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Property group identifier
        /// </summary>
        private string PropertyGroupIdValue = string.Empty;

        public string PropertyGroupId

        {

            get { return this.PropertyGroupIdValue; }

            set { SetProperty(ref PropertyGroupIdValue, value); }

        }

        /// <summary>
        /// Carrying amount (book value)
        /// </summary>
        private decimal CarryingAmountValue;

        public decimal CarryingAmount

        {

            get { return this.CarryingAmountValue; }

            set { SetProperty(ref CarryingAmountValue, value); }

        }

        /// <summary>
        /// Expected undiscounted future cash flows
        /// </summary>
        private decimal UndiscountedCashFlowsValue;

        public decimal UndiscountedCashFlows

        {

            get { return this.UndiscountedCashFlowsValue; }

            set { SetProperty(ref UndiscountedCashFlowsValue, value); }

        }

        /// <summary>
        /// Is impairment indicated?
        /// </summary>
        private bool IsImpairmentIndicatorValue;

        public bool IsImpairmentIndicator

        {

            get { return this.IsImpairmentIndicatorValue; }

            set { SetProperty(ref IsImpairmentIndicatorValue, value); }

        }

        /// <summary>
        /// Fair value of asset (if impaired)
        /// </summary>
        private decimal? FairValueValue;

        public decimal? FairValue

        {

            get { return this.FairValueValue; }

            set { SetProperty(ref FairValueValue, value); }

        }

        /// <summary>
        /// Impairment charge (if applicable)
        /// </summary>
        private decimal? ImpairmentChargeValue;

        public decimal? ImpairmentCharge

        {

            get { return this.ImpairmentChargeValue; }

            set { SetProperty(ref ImpairmentChargeValue, value); }

        }

        /// <summary>
        /// Analysis date
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
    }
}




