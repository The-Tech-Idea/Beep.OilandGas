using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    /// <summary>
    /// Request to calculate and record amortization.
    /// </summary>
    public class CalculateAmortizationRequest
    {
        public string? PropertyId { get; set; }
        public string? CostCenterId { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public decimal NetCapitalizedCosts { get; set; }
        public decimal TotalProvedReservesBOE { get; set; }
        public decimal ProductionBOE { get; set; }
        public string AccountingMethod { get; set; } = "Successful Efforts"; // or "Full Cost"
    }

    /// <summary>
    /// Request to generate an amortization schedule.
    /// </summary>
    public class GenerateScheduleRequest
    {
        public string? PropertyId { get; set; }
        public string? CostCenterId { get; set; }
        public DateTime StartDate { get; set; }
        public int NumberOfPeriods { get; set; }
        public string PeriodType { get; set; } = "Monthly"; // Monthly, Quarterly, Annual
        public decimal EstimatedProductionPerPeriod { get; set; }
        public string AccountingMethod { get; set; } = "Successful Efforts";
    }

    /// <summary>
    /// Amortization schedule with projected periods.
    /// </summary>
    public class AmortizationSchedule
    {
        public string? PropertyId { get; set; }
        public string? CostCenterId { get; set; }
        public DateTime StartDate { get; set; }
        public string PeriodType { get; set; }
        public decimal BeginningNetCapitalizedCosts { get; set; }
        public decimal TotalReservesBOE { get; set; }
        public List<AmortizationSchedulePeriod> Periods { get; set; } = new List<AmortizationSchedulePeriod>();
        public decimal TotalProjectedAmortization { get; set; }
        public decimal EndingNetCapitalizedCosts { get; set; }
    }

    /// <summary>
    /// A single period in an amortization schedule.
    /// </summary>
    public class AmortizationSchedulePeriod
    {
        public int PeriodNumber { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public decimal BeginningNetCapitalizedCosts { get; set; }
        public decimal EstimatedProductionBOE { get; set; }
        public decimal AmortizationRate { get; set; }
        public decimal ProjectedAmortization { get; set; }
        public decimal EndingNetCapitalizedCosts { get; set; }
    }

    /// <summary>
    /// Amortization summary for a property or cost center.
    /// </summary>
    public class AmortizationSummary
    {
        public string? PropertyId { get; set; }
        public string? CostCenterId { get; set; }
        public DateTime? AsOfDate { get; set; }
        public decimal TotalCapitalizedCosts { get; set; }
        public decimal AccumulatedAmortization { get; set; }
        public decimal NetCapitalizedCosts { get; set; }
        public decimal TotalReservesBOE { get; set; }
        public decimal RemainingReservesBOE { get; set; }
        public decimal AmortizationRate { get; set; }
        public int NumberOfRecords { get; set; }
        public DateTime? FirstAmortizationDate { get; set; }
        public DateTime? LastAmortizationDate { get; set; }
    }
}




