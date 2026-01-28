using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class CostAllocationResult : ModelEntityBase
    {
        private System.DateTime AllocationDateValue;
        /// <summary>
        /// Gets or sets allocation date.
        /// </summary>
        public System.DateTime AllocationDate
        {
            get { return this.AllocationDateValue; }
            set { SetProperty(ref AllocationDateValue, value); }
        }

        private CostAllocationMethod AllocationMethodValue;
        /// <summary>
        /// Gets or sets allocation method.
        /// </summary>
        public CostAllocationMethod AllocationMethod
        {
            get { return this.AllocationMethodValue; }
            set { SetProperty(ref AllocationMethodValue, value); }
        }

        private System.String PerformedByValue = string.Empty;
        /// <summary>
        /// Gets or sets performed by.
        /// </summary>
        public System.String PerformedBy
        {
            get { return this.PerformedByValue; }
            set { SetProperty(ref PerformedByValue, value); }
        }

        private System.DateTime PerformedDateValue;
        /// <summary>
        /// Gets or sets performed date.
        /// </summary>
        public System.DateTime PerformedDate
        {
            get { return this.PerformedDateValue; }
            set { SetProperty(ref PerformedDateValue, value); }
        }

        private List<AllocationEntry> AllocationEntriesValue = new List<AllocationEntry>();
        /// <summary>
        /// Gets or sets allocation entries.
        /// </summary>
        public List<AllocationEntry> AllocationEntries
        {
            get { return this.AllocationEntriesValue; }
            set { SetProperty(ref AllocationEntriesValue, value); }
        }

        private System.Decimal TotalAllocatedValue;
        /// <summary>
        /// Gets or sets total allocated.
        /// </summary>
        public System.Decimal TotalAllocated
        {
            get { return this.TotalAllocatedValue; }
            set { SetProperty(ref TotalAllocatedValue, value); }
        }

        private System.Int32 AllocationCountValue;
        /// <summary>
        /// Gets or sets allocation count.
        /// </summary>
        public System.Int32 AllocationCount
        {
            get { return this.AllocationCountValue; }
            set { SetProperty(ref AllocationCountValue, value); }
        }

        private System.String StatusValue = string.Empty;
        /// <summary>
        /// Gets or sets status.
        /// </summary>
        public System.String Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }
    }
}
