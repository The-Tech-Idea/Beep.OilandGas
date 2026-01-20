using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Allocation Method
    /// </summary>
    public enum AllocationMethod
    {
        DirectAllocation,
        StepDown,
        Reciprocal,
        ActivityBasedCosting
    }

    /// <summary>
    /// Cost Center
    /// </summary>
    public partial class CostCenter : AccountingEntityBase
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

    /// <summary>
    /// Allocation Base
    /// </summary>
    public partial class AllocationBase : AccountingEntityBase
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

        private System.String ActivityNameValue = string.Empty;
        /// <summary>
        /// Gets or sets the activity name.
        /// </summary>
        public System.String ActivityName
        {
            get { return this.ActivityNameValue; }
            set { SetProperty(ref ActivityNameValue, value); }
        }

        private System.Decimal ActivityPercentValue;
        /// <summary>
        /// Gets or sets the activity percent.
        /// </summary>
        public System.Decimal ActivityPercent
        {
            get { return this.ActivityPercentValue; }
            set { SetProperty(ref ActivityPercentValue, value); }
        }

        private System.Decimal ActivityUnitsValue;
        /// <summary>
        /// Gets or sets the activity units.
        /// </summary>
        public System.Decimal ActivityUnits
        {
            get { return this.ActivityUnitsValue; }
            set { SetProperty(ref ActivityUnitsValue, value); }
        }
    }

    /// <summary>
    /// Allocation Result
    /// </summary>
    public partial class AllocationResult : AccountingEntityBase
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

        private AllocationMethod AllocationMethodValue;
        /// <summary>
        /// Gets or sets allocation method.
        /// </summary>
        public AllocationMethod AllocationMethod
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

    /// <summary>
    /// Allocation Entry
    /// </summary>
    public partial class AllocationEntry : AccountingEntityBase
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

    /// <summary>
    /// Departmental Profitability
    /// </summary>
    public partial class DepartmentProfitability : AccountingEntityBase
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

    /// <summary>
    /// Departmental Profitability Report
    /// </summary>
    public partial class DepartmentalProfitabilityReport : AccountingEntityBase
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

