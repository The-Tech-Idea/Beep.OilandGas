using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class DepartmentProfitability : ModelEntityBase
    {
        private System.String DepartmentIdValue = string.Empty;
        /// <summary>
        /// Gets or sets department id.
        /// </summary>
        public System.String DepartmentId
        {
            get { return this.DepartmentIdValue; }
            set { SetProperty(ref DepartmentIdValue, value); }
        }

        private System.String DepartmentNameValue = string.Empty;
        /// <summary>
        /// Gets or sets department name.
        /// </summary>
        public System.String DepartmentName
        {
            get { return this.DepartmentNameValue; }
            set { SetProperty(ref DepartmentNameValue, value); }
        }

        private System.Decimal RevenueValue;
        /// <summary>
        /// Gets or sets revenue.
        /// </summary>
        public System.Decimal Revenue
        {
            get { return this.RevenueValue; }
            set { SetProperty(ref RevenueValue, value); }
        }

        private System.Decimal DirectCostsValue;
        /// <summary>
        /// Gets or sets direct costs.
        /// </summary>
        public System.Decimal DirectCosts
        {
            get { return this.DirectCostsValue; }
            set { SetProperty(ref DirectCostsValue, value); }
        }

        private System.Decimal AllocatedOverheadValue;
        /// <summary>
        /// Gets or sets allocated overhead.
        /// </summary>
        public System.Decimal AllocatedOverhead
        {
            get { return this.AllocatedOverheadValue; }
            set { SetProperty(ref AllocatedOverheadValue, value); }
        }

        private System.Decimal ContributionValue;
        /// <summary>
        /// Gets or sets contribution.
        /// </summary>
        public System.Decimal Contribution
        {
            get { return this.ContributionValue; }
            set { SetProperty(ref ContributionValue, value); }
        }

        private System.Decimal ProfitValue;
        /// <summary>
        /// Gets or sets profit.
        /// </summary>
        public System.Decimal Profit
        {
            get { return this.ProfitValue; }
            set { SetProperty(ref ProfitValue, value); }
        }

        private System.Decimal ContributionMarginValue;
        /// <summary>
        /// Gets or sets contribution margin.
        /// </summary>
        public System.Decimal ContributionMargin
        {
            get { return this.ContributionMarginValue; }
            set { SetProperty(ref ContributionMarginValue, value); }
        }

        private System.Decimal ProfitMarginValue;
        /// <summary>
        /// Gets or sets profit margin.
        /// </summary>
        public System.Decimal ProfitMargin
        {
            get { return this.ProfitMarginValue; }
            set { SetProperty(ref ProfitMarginValue, value); }
        }

        private System.Decimal ROIValue;
        /// <summary>
        /// Gets or sets ROI.
        /// </summary>
        public System.Decimal ROI
        {
            get { return this.ROIValue; }
            set { SetProperty(ref ROIValue, value); }
        }
    }
}
