using System;

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
        public decimal ConstructionCost { get; set; }

        /// <summary>
        /// Period interest rate
        /// </summary>
        public decimal InterestRate { get; set; }

        /// <summary>
        /// Number of periods to capitalize
        /// </summary>
        public int Periods { get; set; }

        /// <summary>
        /// Weighted average cost of capital
        /// </summary>
        public decimal WACC { get; set; }
    }

    /// <summary>
    /// DTO for production data used in amortization calculations.
    /// </summary>
    public class ProductionData : ModelEntityBase
    {
        /// <summary>
        /// Oil production in barrels
        /// </summary>
        public decimal OilVolume { get; set; }

        /// <summary>
        /// Gas production in Mcf
        /// </summary>
        public decimal GasVolume { get; set; }

        /// <summary>
        /// Water production in barrels
        /// </summary>
        public decimal WaterVolume { get; set; }

        /// <summary>
        /// Production period date
        /// </summary>
        public DateTime PeriodDate { get; set; }

        /// <summary>
        /// Oil API gravity
        /// </summary>
        public decimal? OilAPIGravity { get; set; }

        /// <summary>
        /// Gas specific gravity
        /// </summary>
        public decimal? GasSpecificGravity { get; set; }
    }

    /// <summary>
    /// DTO for proved reserves data used in units-of-production amortization.
    /// </summary>
    public class ProvedReserves : ModelEntityBase
    {
        /// <summary>
        /// Proved oil reserves in barrels
        /// </summary>
        public decimal ProvedOilReserves { get; set; }

        /// <summary>
        /// Proved gas reserves in Mcf
        /// </summary>
        public decimal ProvedGasReserves { get; set; }

        /// <summary>
        /// Proved reserves as of date
        /// </summary>
        public DateTime AsOfDate { get; set; }

        /// <summary>
        /// Reserve estimation method (volumetric, performance, or analog)
        /// </summary>
        public string EstimationMethod { get; set; } = "Volumetric";

        /// <summary>
        /// Confidence level (1P, 2P, 3P)
        /// </summary>
        public string ConfidenceLevel { get; set; } = "1P";
    }

    /// <summary>
    /// DTO for cost capitalization tracking in Successful Efforts method.
    /// </summary>
    public class CapitalizedCostData : ModelEntityBase
    {
        /// <summary>
        /// Well or project identifier
        /// </summary>
        public string WellId { get; set; } = string.Empty;

        /// <summary>
        /// Drilling and completion costs
        /// </summary>
        public decimal DrillingCompletionCosts { get; set; }

        /// <summary>
        /// Acquisition costs
        /// </summary>
        public decimal AcquisitionCosts { get; set; }

        /// <summary>
        /// Capitalized interest
        /// </summary>
        public decimal CapitalizedInterest { get; set; }

        /// <summary>
        /// Total capitalized costs before amortization
        /// </summary>
        public decimal TotalCapitalizedCosts { get; set; }

        /// <summary>
        /// Date capitalized
        /// </summary>
        public DateTime DateCapitalized { get; set; }
    }

    /// <summary>
    /// DTO for dry hole or unsuccessful well costs.
    /// </summary>
    public class UnsuccessfulWellData : ModelEntityBase
    {
        /// <summary>
        /// Well identifier
        /// </summary>
        public string WellId { get; set; } = string.Empty;

        /// <summary>
        /// Total drilling and completion costs (to be expensed)
        /// </summary>
        public decimal DrillingCompletionCosts { get; set; }

        /// <summary>
        /// Acquisition costs
        /// </summary>
        public decimal AcquisitionCosts { get; set; }

        /// <summary>
        /// Date well determined unsuccessful
        /// </summary>
        public DateTime DateDetermined { get; set; }

        /// <summary>
        /// Reason well was unsuccessful
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for impairment analysis results (ASC 360).
    /// </summary>
    public class ImpairmentAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Property group identifier
        /// </summary>
        public string PropertyGroupId { get; set; } = string.Empty;

        /// <summary>
        /// Carrying amount (book value)
        /// </summary>
        public decimal CarryingAmount { get; set; }

        /// <summary>
        /// Expected undiscounted future cash flows
        /// </summary>
        public decimal UndiscountedCashFlows { get; set; }

        /// <summary>
        /// Is impairment indicated?
        /// </summary>
        public bool IsImpairmentIndicator { get; set; }

        /// <summary>
        /// Fair value of asset (if impaired)
        /// </summary>
        public decimal? FairValue { get; set; }

        /// <summary>
        /// Impairment charge (if applicable)
        /// </summary>
        public decimal? ImpairmentCharge { get; set; }

        /// <summary>
        /// Analysis date
        /// </summary>
        public DateTime AnalysisDate { get; set; }
    }
}

