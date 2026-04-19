using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class YTDItem : ModelEntityBase
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
}
