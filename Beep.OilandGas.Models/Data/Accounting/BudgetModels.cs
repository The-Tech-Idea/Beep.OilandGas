using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Budget Definition
    /// </summary>
    public partial class Budget : AccountingEntityBase
    {
        private System.String BudgetNameValue = string.Empty;
        public System.String BudgetName
        {
            get { return this.BudgetNameValue; }
            set { SetProperty(ref BudgetNameValue, value); }
        }

        private System.DateTime BudgetStartValue;
        public System.DateTime BudgetStart
        {
            get { return this.BudgetStartValue; }
            set { SetProperty(ref BudgetStartValue, value); }
        }

        private System.DateTime BudgetEndValue;
        public System.DateTime BudgetEnd
        {
            get { return this.BudgetEndValue; }
            set { SetProperty(ref BudgetEndValue, value); }
        }

        private System.DateTime CreatedDateValue;
        public System.DateTime CreatedDate
        {
            get { return this.CreatedDateValue; }
            set { SetProperty(ref CreatedDateValue, value); }
        }

        private System.String CreatedByValue = string.Empty;
        public System.String CreatedBy
        {
            get { return this.CreatedByValue; }
            set { SetProperty(ref CreatedByValue, value); }
        }

        private List<BudgetLine> BudgetLinesValue = new List<BudgetLine>();
        public List<BudgetLine> BudgetLines
        {
            get { return this.BudgetLinesValue; }
            set { SetProperty(ref BudgetLinesValue, value); }
        }

        private System.Decimal TotalBudgetedValue;
        public System.Decimal TotalBudgeted
        {
            get { return this.TotalBudgetedValue; }
            set { SetProperty(ref TotalBudgetedValue, value); }
        }

        private System.Int32 BudgetMonthsValue;
        public System.Int32 BudgetMonths
        {
            get { return this.BudgetMonthsValue; }
            set { SetProperty(ref BudgetMonthsValue, value); }
        }

        private System.String StatusValue = string.Empty;
        public System.String Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }
    }

    /// <summary>
    /// Budget Line Item
    /// </summary>
    public partial class BudgetLine : AccountingEntityBase
    {
        private System.String AccountNumberValue = string.Empty;
        public System.String AccountNumber
        {
            get { return this.AccountNumberValue; }
            set { SetProperty(ref AccountNumberValue, value); }
        }

        private System.String AccountNameValue = string.Empty;
        public System.String AccountName
        {
            get { return this.AccountNameValue; }
            set { SetProperty(ref AccountNameValue, value); }
        }

        private System.Decimal BudgetAmountValue;
        public System.Decimal BudgetAmount
        {
            get { return this.BudgetAmountValue; }
            set { SetProperty(ref BudgetAmountValue, value); }
        }
    }

    /// <summary>
    /// Budget Variance Report
    /// </summary>
    public partial class BudgetVarianceReport : AccountingEntityBase
    {
        private System.String ReportNameValue = string.Empty;
        public System.String ReportName
        {
            get { return this.ReportNameValue; }
            set { SetProperty(ref ReportNameValue, value); }
        }

        private System.DateTime GeneratedDateValue;
        public System.DateTime GeneratedDate
        {
            get { return this.GeneratedDateValue; }
            set { SetProperty(ref GeneratedDateValue, value); }
        }

        private System.DateTime AsOfDateValue;
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private System.String BudgetNameValue = string.Empty;
        public System.String BudgetName
        {
            get { return this.BudgetNameValue; }
            set { SetProperty(ref BudgetNameValue, value); }
        }

        private System.String BudgetPeriodValue = string.Empty;
        public System.String BudgetPeriod
        {
            get { return this.BudgetPeriodValue; }
            set { SetProperty(ref BudgetPeriodValue, value); }
        }

        private List<BudgetVarianceLine> VarianceLinesValue = new List<BudgetVarianceLine>();
        public List<BudgetVarianceLine> VarianceLines
        {
            get { return this.VarianceLinesValue; }
            set { SetProperty(ref VarianceLinesValue, value); }
        }

        private System.Decimal TotalBudgetValue;
        public System.Decimal TotalBudget
        {
            get { return this.TotalBudgetValue; }
            set { SetProperty(ref TotalBudgetValue, value); }
        }

        private System.Decimal TotalActualValue;
        public System.Decimal TotalActual
        {
            get { return this.TotalActualValue; }
            set { SetProperty(ref TotalActualValue, value); }
        }

        private System.Decimal TotalVarianceValue;
        public System.Decimal TotalVariance
        {
            get { return this.TotalVarianceValue; }
            set { SetProperty(ref TotalVarianceValue, value); }
        }

        private System.Decimal VariancePercentValue;
        public System.Decimal VariancePercent
        {
            get { return this.VariancePercentValue; }
            set { SetProperty(ref VariancePercentValue, value); }
        }

        private System.String VarianceStatusValue = string.Empty;
        public System.String VarianceStatus
        {
            get { return this.VarianceStatusValue; }
            set { SetProperty(ref VarianceStatusValue, value); }
        }
    }

    /// <summary>
    /// Budget Variance Line
    /// </summary>
    public partial class BudgetVarianceLine : AccountingEntityBase
    {
        private System.String AccountNumberValue = string.Empty;
        public System.String AccountNumber
        {
            get { return this.AccountNumberValue; }
            set { SetProperty(ref AccountNumberValue, value); }
        }

        private System.String AccountNameValue = string.Empty;
        public System.String AccountName
        {
            get { return this.AccountNameValue; }
            set { SetProperty(ref AccountNameValue, value); }
        }

        private System.Decimal BudgetAmountValue;
        public System.Decimal BudgetAmount
        {
            get { return this.BudgetAmountValue; }
            set { SetProperty(ref BudgetAmountValue, value); }
        }

        private System.Decimal ActualAmountValue;
        public System.Decimal ActualAmount
        {
            get { return this.ActualAmountValue; }
            set { SetProperty(ref ActualAmountValue, value); }
        }

        private System.Decimal VarianceValue;
        public System.Decimal Variance
        {
            get { return this.VarianceValue; }
            set { SetProperty(ref VarianceValue, value); }
        }

        private System.Decimal VariancePercentValue;
        public System.Decimal VariancePercent
        {
            get { return this.VariancePercentValue; }
            set { SetProperty(ref VariancePercentValue, value); }
        }

        private System.String StatusValue = string.Empty;
        public System.String Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }
    }

    /// <summary>
    /// Year-to-Date Performance
    /// </summary>
    public partial class YTDPerformance : AccountingEntityBase
    {
        private System.String BudgetNameValue = string.Empty;
        public System.String BudgetName
        {
            get { return this.BudgetNameValue; }
            set { SetProperty(ref BudgetNameValue, value); }
        }

        private System.DateTime AsOfDateValue;
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private System.DateTime CalculatedDateValue;
        public System.DateTime CalculatedDate
        {
            get { return this.CalculatedDateValue; }
            set { SetProperty(ref CalculatedDateValue, value); }
        }

        private System.DateTime BudgetYearStartValue;
        public System.DateTime BudgetYearStart
        {
            get { return this.BudgetYearStartValue; }
            set { SetProperty(ref BudgetYearStartValue, value); }
        }

        private System.Decimal ProportionOfYearElapsedValue;
        public System.Decimal ProportionOfYearElapsed
        {
            get { return this.ProportionOfYearElapsedValue; }
            set { SetProperty(ref ProportionOfYearElapsedValue, value); }
        }

        private System.Decimal MonthsElapsedValue;
        public System.Decimal MonthsElapsed
        {
            get { return this.MonthsElapsedValue; }
            set { SetProperty(ref MonthsElapsedValue, value); }
        }

        private System.Decimal FullYearBudgetValue;
        public System.Decimal FullYearBudget
        {
            get { return this.FullYearBudgetValue; }
            set { SetProperty(ref FullYearBudgetValue, value); }
        }

        private System.Decimal ProRataBudgetValue;
        public System.Decimal ProRataBudget
        {
            get { return this.ProRataBudgetValue; }
            set { SetProperty(ref ProRataBudgetValue, value); }
        }

        private System.Decimal YTDActualValue;
        public System.Decimal YTDActual
        {
            get { return this.YTDActualValue; }
            set { SetProperty(ref YTDActualValue, value); }
        }

        private System.Decimal YTDVarianceValue;
        public System.Decimal YTDVariance
        {
            get { return this.YTDVarianceValue; }
            set { SetProperty(ref YTDVarianceValue, value); }
        }

        private List<YTDItem> ItemsValue = new List<YTDItem>();
        public List<YTDItem> Items
        {
            get { return this.ItemsValue; }
            set { SetProperty(ref ItemsValue, value); }
        }

        private System.Int32 OnTrackCountValue;
        public System.Int32 OnTrackCount
        {
            get { return this.OnTrackCountValue; }
            set { SetProperty(ref OnTrackCountValue, value); }
        }

        private System.Int32 OffTrackCountValue;
        public System.Int32 OffTrackCount
        {
            get { return this.OffTrackCountValue; }
            set { SetProperty(ref OffTrackCountValue, value); }
        }
    }

    /// <summary>
    /// YTD Item
    /// </summary>
    public partial class YTDItem : AccountingEntityBase
    {
        private System.String AccountNumberValue = string.Empty;
        public System.String AccountNumber
        {
            get { return this.AccountNumberValue; }
            set { SetProperty(ref AccountNumberValue, value); }
        }

        private System.String AccountNameValue = string.Empty;
        public System.String AccountName
        {
            get { return this.AccountNameValue; }
            set { SetProperty(ref AccountNameValue, value); }
        }

        private System.Decimal FullYearBudgetValue;
        public System.Decimal FullYearBudget
        {
            get { return this.FullYearBudgetValue; }
            set { SetProperty(ref FullYearBudgetValue, value); }
        }

        private System.Decimal ProRataBudgetValue;
        public System.Decimal ProRataBudget
        {
            get { return this.ProRataBudgetValue; }
            set { SetProperty(ref ProRataBudgetValue, value); }
        }

        private System.Decimal YTDActualValue;
        public System.Decimal YTDActual
        {
            get { return this.YTDActualValue; }
            set { SetProperty(ref YTDActualValue, value); }
        }

        private System.Decimal YTDVarianceValue;
        public System.Decimal YTDVariance
        {
            get { return this.YTDVarianceValue; }
            set { SetProperty(ref YTDVarianceValue, value); }
        }

        private System.Boolean OnTrackValue;
        public System.Boolean OnTrack
        {
            get { return this.OnTrackValue; }
            set { SetProperty(ref OnTrackValue, value); }
        }
    }

    /// <summary>
    /// Budget Forecast
    /// </summary>
    public partial class BudgetForecast : AccountingEntityBase
    {
        private System.String BudgetNameValue = string.Empty;
        public System.String BudgetName
        {
            get { return this.BudgetNameValue; }
            set { SetProperty(ref BudgetNameValue, value); }
        }

        private System.DateTime AsOfDateValue;
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private System.DateTime ForecastDateValue;
        public System.DateTime ForecastDate
        {
            get { return this.ForecastDateValue; }
            set { SetProperty(ref ForecastDateValue, value); }
        }

        private System.DateTime YearEndValue;
        public System.DateTime YearEnd
        {
            get { return this.YearEndValue; }
            set { SetProperty(ref YearEndValue, value); }
        }

        private System.Int32 DaysRemainingValue;
        public System.Int32 DaysRemaining
        {
            get { return this.DaysRemainingValue; }
            set { SetProperty(ref DaysRemainingValue, value); }
        }

        private System.String ForecastMethodValue = string.Empty;
        public System.String ForecastMethod
        {
            get { return this.ForecastMethodValue; }
            set { SetProperty(ref ForecastMethodValue, value); }
        }

        private List<ForecastItem> ItemsValue = new List<ForecastItem>();
        public List<ForecastItem> Items
        {
            get { return this.ItemsValue; }
            set { SetProperty(ref ItemsValue, value); }
        }

        private System.Decimal TotalBudgetValue;
        public System.Decimal TotalBudget
        {
            get { return this.TotalBudgetValue; }
            set { SetProperty(ref TotalBudgetValue, value); }
        }

        private System.Decimal TotalYTDActualValue;
        public System.Decimal TotalYTDActual
        {
            get { return this.TotalYTDActualValue; }
            set { SetProperty(ref TotalYTDActualValue, value); }
        }

        private System.Decimal TotalForecastedYearEndValue;
        public System.Decimal TotalForecastedYearEnd
        {
            get { return this.TotalForecastedYearEndValue; }
            set { SetProperty(ref TotalForecastedYearEndValue, value); }
        }

        private System.Decimal TotalForecastedVarianceValue;
        public System.Decimal TotalForecastedVariance
        {
            get { return this.TotalForecastedVarianceValue; }
            set { SetProperty(ref TotalForecastedVarianceValue, value); }
        }

        private System.Int32 ExceedingBudgetCountValue;
        public System.Int32 ExceedingBudgetCount
        {
            get { return this.ExceedingBudgetCountValue; }
            set { SetProperty(ref ExceedingBudgetCountValue, value); }
        }
    }

    /// <summary>
    /// Forecast Item
    /// </summary>
    public partial class ForecastItem : AccountingEntityBase
    {
        private System.String AccountNumberValue = string.Empty;
        public System.String AccountNumber
        {
            get { return this.AccountNumberValue; }
            set { SetProperty(ref AccountNumberValue, value); }
        }

        private System.String AccountNameValue = string.Empty;
        public System.String AccountName
        {
            get { return this.AccountNameValue; }
            set { SetProperty(ref AccountNameValue, value); }
        }

        private System.Decimal FullYearBudgetValue;
        public System.Decimal FullYearBudget
        {
            get { return this.FullYearBudgetValue; }
            set { SetProperty(ref FullYearBudgetValue, value); }
        }

        private System.Decimal YTDActualValue;
        public System.Decimal YTDActual
        {
            get { return this.YTDActualValue; }
            set { SetProperty(ref YTDActualValue, value); }
        }

        private System.Decimal ForecastedYearEndValue;
        public System.Decimal ForecastedYearEnd
        {
            get { return this.ForecastedYearEndValue; }
            set { SetProperty(ref ForecastedYearEndValue, value); }
        }

        private System.Decimal ForecastedVarianceValue;
        public System.Decimal ForecastedVariance
        {
            get { return this.ForecastedVarianceValue; }
            set { SetProperty(ref ForecastedVarianceValue, value); }
        }

        private System.Decimal ForecastedVariancePercentValue;
        public System.Decimal ForecastedVariancePercent
        {
            get { return this.ForecastedVariancePercentValue; }
            set { SetProperty(ref ForecastedVariancePercentValue, value); }
        }

        private System.Boolean WillExceedBudgetValue;
        public System.Boolean WillExceedBudget
        {
            get { return this.WillExceedBudgetValue; }
            set { SetProperty(ref WillExceedBudgetValue, value); }
        }
    }

    /// <summary>
    /// Trend forecasting method
    /// </summary>
    public enum TrendMethod
    {
        LinearTrend,
        MovingAverage,
        ExponentialSmoothing
    }
}

