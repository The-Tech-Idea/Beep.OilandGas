using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class AgedItemsReport : ModelEntityBase
    {
        private System.String AccountNumberValue = string.Empty;
        /// <summary>
        /// Gets or sets the account number.
        /// </summary>
        public System.String AccountNumber
        {
            get { return this.AccountNumberValue; }
            set { SetProperty(ref AccountNumberValue, value); }
        }

        private System.DateTime AsOfDateValue;
        /// <summary>
        /// Gets or sets the as of date.
        /// </summary>
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private System.DateTime AnalysisDateValue;
        /// <summary>
        /// Gets or sets the analysis date.
        /// </summary>
        public System.DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private List<AgedItem> CurrentValue = new List<AgedItem>();
        /// <summary>
        /// Gets or sets current aged items.
        /// </summary>
        public List<AgedItem> Current
        {
            get { return this.CurrentValue; }
            set { SetProperty(ref CurrentValue, value); }
        }

        private List<AgedItem> _30to60DaysValue = new List<AgedItem>();
        /// <summary>
        /// Gets or sets 30-60 day aged items.
        /// </summary>
        public List<AgedItem> _30to60Days
        {
            get { return this._30to60DaysValue; }
            set { SetProperty(ref _30to60DaysValue, value); }
        }

        private List<AgedItem> _60to90DaysValue = new List<AgedItem>();
        /// <summary>
        /// Gets or sets 60-90 day aged items.
        /// </summary>
        public List<AgedItem> _60to90Days
        {
            get { return this._60to90DaysValue; }
            set { SetProperty(ref _60to90DaysValue, value); }
        }

        private List<AgedItem> Over90DaysValue = new List<AgedItem>();
        /// <summary>
        /// Gets or sets over 90 day aged items.
        /// </summary>
        public List<AgedItem> Over90Days
        {
            get { return this.Over90DaysValue; }
            set { SetProperty(ref Over90DaysValue, value); }
        }

        private System.Decimal CurrentTotalValue;
        /// <summary>
        /// Gets or sets current total.
        /// </summary>
        public System.Decimal CurrentTotal
        {
            get { return this.CurrentTotalValue; }
            set { SetProperty(ref CurrentTotalValue, value); }
        }

        private System.Decimal _30to60TotalValue;
        /// <summary>
        /// Gets or sets 30-60 day total.
        /// </summary>
        public System.Decimal _30to60Total
        {
            get { return this._30to60TotalValue; }
            set { SetProperty(ref _30to60TotalValue, value); }
        }

        private System.Decimal _60to90TotalValue;
        /// <summary>
        /// Gets or sets 60-90 day total.
        /// </summary>
        public System.Decimal _60to90Total
        {
            get { return this._60to90TotalValue; }
            set { SetProperty(ref _60to90TotalValue, value); }
        }

        private System.Decimal Over90TotalValue;
        /// <summary>
        /// Gets or sets over 90 day total.
        /// </summary>
        public System.Decimal Over90Total
        {
            get { return this.Over90TotalValue; }
            set { SetProperty(ref Over90TotalValue, value); }
        }

        private System.Decimal GrandTotalValue;
        /// <summary>
        /// Gets or sets grand total.
        /// </summary>
        public System.Decimal GrandTotal
        {
            get { return this.GrandTotalValue; }
            set { SetProperty(ref GrandTotalValue, value); }
        }
    }
}
