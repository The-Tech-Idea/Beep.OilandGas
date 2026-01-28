using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class AllocationEntry : ModelEntityBase
    {
        private System.String SourceCostCenterValue = string.Empty;
        /// <summary>
        /// Gets or sets source cost center.
        /// </summary>
        public System.String SourceCostCenter
        {
            get { return this.SourceCostCenterValue; }
            set { SetProperty(ref SourceCostCenterValue, value); }
        }

        private System.String TargetCostCenterValue = string.Empty;
        /// <summary>
        /// Gets or sets target cost center.
        /// </summary>
        public System.String TargetCostCenter
        {
            get { return this.TargetCostCenterValue; }
            set { SetProperty(ref TargetCostCenterValue, value); }
        }

        private System.String AllocationBasisValue = string.Empty;
        /// <summary>
        /// Gets or sets allocation basis.
        /// </summary>
        public System.String AllocationBasis
        {
            get { return this.AllocationBasisValue; }
            set { SetProperty(ref AllocationBasisValue, value); }
        }

        private System.Decimal AllocationPercentValue;
        /// <summary>
        /// Gets or sets allocation percent.
        /// </summary>
        public System.Decimal AllocationPercent
        {
            get { return this.AllocationPercentValue; }
            set { SetProperty(ref AllocationPercentValue, value); }
        }

        private System.Decimal AllocationAmountValue;
        /// <summary>
        /// Gets or sets allocation amount.
        /// </summary>
        public System.Decimal AllocationAmount
        {
            get { return this.AllocationAmountValue; }
            set { SetProperty(ref AllocationAmountValue, value); }
        }

        private System.String DescriptionValue = string.Empty;
        /// <summary>
        /// Gets or sets description.
        /// </summary>
        public System.String Description
        {
            get { return this.DescriptionValue; }
            set { SetProperty(ref DescriptionValue, value); }
        }
    }
}
