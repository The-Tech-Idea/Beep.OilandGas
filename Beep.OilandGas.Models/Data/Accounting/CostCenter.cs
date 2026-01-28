using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class CostCenter : ModelEntityBase
    {
        private System.String CostCenterIdValue = string.Empty;
        /// <summary>
        /// Gets or sets the cost center identifier.
        /// </summary>
        public System.String CostCenterId
        {
            get { return this.CostCenterIdValue; }
            set { SetProperty(ref CostCenterIdValue, value); }
        }

        private System.String CostCenterNameValue = string.Empty;
        /// <summary>
        /// Gets or sets the cost center name.
        /// </summary>
        public System.String CostCenterName
        {
            get { return this.CostCenterNameValue; }
            set { SetProperty(ref CostCenterNameValue, value); }
        }

        private System.String CostCenterTypeValue = string.Empty;
        /// <summary>
        /// Gets or sets the cost center type.
        /// </summary>
        public System.String CostCenterType
        {
            get { return this.CostCenterTypeValue; }
            set { SetProperty(ref CostCenterTypeValue, value); }
        }

        private System.String AllocationBasisTypeValue = string.Empty;
        /// <summary>
        /// Gets or sets allocation basis type.
        /// </summary>
        public System.String AllocationBasisType
        {
            get { return this.AllocationBasisTypeValue; }
            set { SetProperty(ref AllocationBasisTypeValue, value); }
        }

        private System.Decimal AllocationBasisValueValue;
        /// <summary>
        /// Gets or sets allocation basis value.
        /// </summary>
        public System.Decimal AllocationBasisValue
        {
            get { return this.AllocationBasisValueValue; }
            set { SetProperty(ref AllocationBasisValueValue, value); }
        }

        private System.Decimal ActivityUnitsValue;
        /// <summary>
        /// Gets or sets activity units.
        /// </summary>
        public System.Decimal ActivityUnits
        {
            get { return this.ActivityUnitsValue; }
            set { SetProperty(ref ActivityUnitsValue, value); }
        }

        private System.Decimal TotalCostValue;
        /// <summary>
        /// Gets or sets total cost.
        /// </summary>
        public System.Decimal TotalCost
        {
            get { return this.TotalCostValue; }
            set { SetProperty(ref TotalCostValue, value); }
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

        private System.Decimal RevenueValue;
        /// <summary>
        /// Gets or sets revenue.
        /// </summary>
        public System.Decimal Revenue
        {
            get { return this.RevenueValue; }
            set { SetProperty(ref RevenueValue, value); }
        }

        private System.Decimal AllocatedCostsValue;
        /// <summary>
        /// Gets or sets allocated costs.
        /// </summary>
        public System.Decimal AllocatedCosts
        {
            get { return this.AllocatedCostsValue; }
            set { SetProperty(ref AllocatedCostsValue, value); }
        }

        private System.Int32 AllocationSequenceValue;
        /// <summary>
        /// Gets or sets allocation sequence.
        /// </summary>
        public System.Int32 AllocationSequence
        {
            get { return this.AllocationSequenceValue; }
            set { SetProperty(ref AllocationSequenceValue, value); }
        }
    }
}
