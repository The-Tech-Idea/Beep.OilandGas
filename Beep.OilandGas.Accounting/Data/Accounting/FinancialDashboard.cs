using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class FinancialDashboard : ModelEntityBase
    {
        private System.DateTime AsOfDateValue;
        /// <summary>
        /// Gets or sets the as of date.
        /// </summary>
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private System.DateTime GeneratedDateValue;
        /// <summary>
        /// Gets or sets the generated date.
        /// </summary>
        public System.DateTime GeneratedDate
        {
            get { return this.GeneratedDateValue; }
            set { SetProperty(ref GeneratedDateValue, value); }
        }

        private List<KPI> KPIsValue = new List<KPI>();
        /// <summary>
        /// Gets or sets KPIs.
        /// </summary>
        public List<KPI> KPIs
        {
            get { return this.KPIsValue; }
            set { SetProperty(ref KPIsValue, value); }
        }
    }
}
