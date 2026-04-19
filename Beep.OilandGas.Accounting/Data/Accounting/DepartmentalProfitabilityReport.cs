using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class DepartmentalProfitabilityReport : ModelEntityBase
    {
        private System.DateTime PeriodStartValue;
        /// <summary>
        /// Gets or sets period start.
        /// </summary>
        public System.DateTime PeriodStart
        {
            get { return this.PeriodStartValue; }
            set { SetProperty(ref PeriodStartValue, value); }
        }

        private System.DateTime PeriodEndValue;
        /// <summary>
        /// Gets or sets period end.
        /// </summary>
        public System.DateTime PeriodEnd
        {
            get { return this.PeriodEndValue; }
            set { SetProperty(ref PeriodEndValue, value); }
        }

        private System.DateTime GeneratedDateValue;
        /// <summary>
        /// Gets or sets generated date.
        /// </summary>
        public System.DateTime GeneratedDate
        {
            get { return this.GeneratedDateValue; }
            set { SetProperty(ref GeneratedDateValue, value); }
        }

        private List<DepartmentProfitability> DepartmentsValue = new List<DepartmentProfitability>();
        /// <summary>
        /// Gets or sets departments.
        /// </summary>
        public List<DepartmentProfitability> Departments
        {
            get { return this.DepartmentsValue; }
            set { SetProperty(ref DepartmentsValue, value); }
        }

        private System.Decimal TotalRevenueValue;
        /// <summary>
        /// Gets or sets total revenue.
        /// </summary>
        public System.Decimal TotalRevenue
        {
            get { return this.TotalRevenueValue; }
            set { SetProperty(ref TotalRevenueValue, value); }
        }

        private System.Decimal TotalDirectCostsValue;
        /// <summary>
        /// Gets or sets total direct costs.
        /// </summary>
        public System.Decimal TotalDirectCosts
        {
            get { return this.TotalDirectCostsValue; }
            set { SetProperty(ref TotalDirectCostsValue, value); }
        }

        private System.Decimal TotalAllocatedOverheadValue;
        /// <summary>
        /// Gets or sets total allocated overhead.
        /// </summary>
        public System.Decimal TotalAllocatedOverhead
        {
            get { return this.TotalAllocatedOverheadValue; }
            set { SetProperty(ref TotalAllocatedOverheadValue, value); }
        }

        private System.Decimal TotalProfitValue;
        /// <summary>
        /// Gets or sets total profit.
        /// </summary>
        public System.Decimal TotalProfit
        {
            get { return this.TotalProfitValue; }
            set { SetProperty(ref TotalProfitValue, value); }
        }
    }
}
