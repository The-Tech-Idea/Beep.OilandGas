using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class YTDPerformance : ModelEntityBase
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
}
