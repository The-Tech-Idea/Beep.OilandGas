using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Request to calculate and class amortization.
    /// </summary>
    public class CalculateAmortizationRequest : ModelEntityBase
    {
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string? CostCenterIdValue;

        public string? CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private DateTime PeriodStartDateValue;

        public DateTime PeriodStartDate

        {

            get { return this.PeriodStartDateValue; }

            set { SetProperty(ref PeriodStartDateValue, value); }

        }
        private DateTime PeriodEndDateValue;

        public DateTime PeriodEndDate

        {

            get { return this.PeriodEndDateValue; }

            set { SetProperty(ref PeriodEndDateValue, value); }

        }
        private decimal NetCapitalizedCostsValue;

        public decimal NetCapitalizedCosts

        {

            get { return this.NetCapitalizedCostsValue; }

            set { SetProperty(ref NetCapitalizedCostsValue, value); }

        }
        private decimal TotalProvedReservesBOEValue;

        public decimal TotalProvedReservesBOE

        {

            get { return this.TotalProvedReservesBOEValue; }

            set { SetProperty(ref TotalProvedReservesBOEValue, value); }

        }
        private decimal ProductionBOEValue;

        public decimal ProductionBOE

        {

            get { return this.ProductionBOEValue; }

            set { SetProperty(ref ProductionBOEValue, value); }

        }
        private string AccountingMethodValue = "Successful Efforts";

        public string AccountingMethod

        {

            get { return this.AccountingMethodValue; }

            set { SetProperty(ref AccountingMethodValue, value); }

        } // or "Full Cost"
    }

    /// <summary>
    /// Request to generate an amortization schedule.
    /// </summary>
    public class GenerateScheduleRequest : ModelEntityBase
    {
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string? CostCenterIdValue;

        public string? CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private int NumberOfPeriodsValue;

        public int NumberOfPeriods

        {

            get { return this.NumberOfPeriodsValue; }

            set { SetProperty(ref NumberOfPeriodsValue, value); }

        }
        private string PeriodTypeValue = "Monthly";

        public string PeriodType

        {

            get { return this.PeriodTypeValue; }

            set { SetProperty(ref PeriodTypeValue, value); }

        } // Monthly, Quarterly, Annual
        private decimal EstimatedProductionPerPeriodValue;

        public decimal EstimatedProductionPerPeriod

        {

            get { return this.EstimatedProductionPerPeriodValue; }

            set { SetProperty(ref EstimatedProductionPerPeriodValue, value); }

        }
        private string AccountingMethodValue = "Successful Efforts";

        public string AccountingMethod

        {

            get { return this.AccountingMethodValue; }

            set { SetProperty(ref AccountingMethodValue, value); }

        }
    }

    /// <summary>
    /// Amortization schedule with projected periods.
    /// </summary>
    public class AmortizationSchedule : ModelEntityBase
    {
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string? CostCenterIdValue;

        public string? CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private string PeriodTypeValue;

        public string PeriodType

        {

            get { return this.PeriodTypeValue; }

            set { SetProperty(ref PeriodTypeValue, value); }

        }
        private decimal BeginningNetCapitalizedCostsValue;

        public decimal BeginningNetCapitalizedCosts

        {

            get { return this.BeginningNetCapitalizedCostsValue; }

            set { SetProperty(ref BeginningNetCapitalizedCostsValue, value); }

        }
        private decimal TotalReservesBOEValue;

        public decimal TotalReservesBOE

        {

            get { return this.TotalReservesBOEValue; }

            set { SetProperty(ref TotalReservesBOEValue, value); }

        }
        private List<AmortizationSchedulePeriod> PeriodsValue = new List<AmortizationSchedulePeriod>();

        public List<AmortizationSchedulePeriod> Periods

        {

            get { return this.PeriodsValue; }

            set { SetProperty(ref PeriodsValue, value); }

        }
        private decimal TotalProjectedAmortizationValue;

        public decimal TotalProjectedAmortization

        {

            get { return this.TotalProjectedAmortizationValue; }

            set { SetProperty(ref TotalProjectedAmortizationValue, value); }

        }
        private decimal EndingNetCapitalizedCostsValue;

        public decimal EndingNetCapitalizedCosts

        {

            get { return this.EndingNetCapitalizedCostsValue; }

            set { SetProperty(ref EndingNetCapitalizedCostsValue, value); }

        }
    }

    /// <summary>
    /// A single period in an amortization schedule.
    /// </summary>
    public class AmortizationSchedulePeriod : ModelEntityBase
    {
        private int PeriodNumberValue;

        public int PeriodNumber

        {

            get { return this.PeriodNumberValue; }

            set { SetProperty(ref PeriodNumberValue, value); }

        }
        private DateTime PeriodStartDateValue;

        public DateTime PeriodStartDate

        {

            get { return this.PeriodStartDateValue; }

            set { SetProperty(ref PeriodStartDateValue, value); }

        }
        private DateTime PeriodEndDateValue;

        public DateTime PeriodEndDate

        {

            get { return this.PeriodEndDateValue; }

            set { SetProperty(ref PeriodEndDateValue, value); }

        }
        private decimal BeginningNetCapitalizedCostsValue;

        public decimal BeginningNetCapitalizedCosts

        {

            get { return this.BeginningNetCapitalizedCostsValue; }

            set { SetProperty(ref BeginningNetCapitalizedCostsValue, value); }

        }
        private decimal EstimatedProductionBOEValue;

        public decimal EstimatedProductionBOE

        {

            get { return this.EstimatedProductionBOEValue; }

            set { SetProperty(ref EstimatedProductionBOEValue, value); }

        }
        private decimal AmortizationRateValue;

        public decimal AmortizationRate

        {

            get { return this.AmortizationRateValue; }

            set { SetProperty(ref AmortizationRateValue, value); }

        }
        private decimal ProjectedAmortizationValue;

        public decimal ProjectedAmortization

        {

            get { return this.ProjectedAmortizationValue; }

            set { SetProperty(ref ProjectedAmortizationValue, value); }

        }
        private decimal EndingNetCapitalizedCostsValue;

        public decimal EndingNetCapitalizedCosts

        {

            get { return this.EndingNetCapitalizedCostsValue; }

            set { SetProperty(ref EndingNetCapitalizedCostsValue, value); }

        }
    }

    /// <summary>
    /// Amortization summary for a property or cost center.
    /// </summary>
    public class AmortizationSummary : ModelEntityBase
    {
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string? CostCenterIdValue;

        public string? CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private DateTime? AsOfDateValue;

        public DateTime? AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

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
        private decimal TotalReservesBOEValue;

        public decimal TotalReservesBOE

        {

            get { return this.TotalReservesBOEValue; }

            set { SetProperty(ref TotalReservesBOEValue, value); }

        }
        private decimal RemainingReservesBOEValue;

        public decimal RemainingReservesBOE

        {

            get { return this.RemainingReservesBOEValue; }

            set { SetProperty(ref RemainingReservesBOEValue, value); }

        }
        private decimal AmortizationRateValue;

        public decimal AmortizationRate

        {

            get { return this.AmortizationRateValue; }

            set { SetProperty(ref AmortizationRateValue, value); }

        }
        private int NumberOfRecordsValue;

        public int NumberOfRecords

        {

            get { return this.NumberOfRecordsValue; }

            set { SetProperty(ref NumberOfRecordsValue, value); }

        }
        private DateTime? FirstAmortizationDateValue;

        public DateTime? FirstAmortizationDate

        {

            get { return this.FirstAmortizationDateValue; }

            set { SetProperty(ref FirstAmortizationDateValue, value); }

        }
        private DateTime? LastAmortizationDateValue;

        public DateTime? LastAmortizationDate

        {

            get { return this.LastAmortizationDateValue; }

            set { SetProperty(ref LastAmortizationDateValue, value); }

        }
    }
}









