using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Depreciation Method
    /// </summary>
    public enum DepreciationMethod
    {
        StraightLine,
        DoubleDeclining,
        UnitsOfProduction,
        MACRS
    }

    /// <summary>
    /// Fixed Asset
    /// </summary>
    public partial class FixedAsset : AccountingEntityBase
    {
        private System.String AssetIdValue = string.Empty;
        /// <summary>
        /// Gets or sets the asset id.
        /// </summary>
        public System.String AssetId
        {
            get { return this.AssetIdValue; }
            set { SetProperty(ref AssetIdValue, value); }
        }

        private System.String AssetNameValue = string.Empty;
        /// <summary>
        /// Gets or sets the asset name.
        /// </summary>
        public System.String AssetName
        {
            get { return this.AssetNameValue; }
            set { SetProperty(ref AssetNameValue, value); }
        }

        private System.Decimal PurchasePriceValue;
        /// <summary>
        /// Gets or sets purchase price.
        /// </summary>
        public System.Decimal PurchasePrice
        {
            get { return this.PurchasePriceValue; }
            set { SetProperty(ref PurchasePriceValue, value); }
        }

        private System.DateTime PurchaseDateValue;
        /// <summary>
        /// Gets or sets purchase date.
        /// </summary>
        public System.DateTime PurchaseDate
        {
            get { return this.PurchaseDateValue; }
            set { SetProperty(ref PurchaseDateValue, value); }
        }

        private System.Decimal SalvageValueValue;
        /// <summary>
        /// Gets or sets salvage value.
        /// </summary>
        public System.Decimal SalvageValue
        {
            get { return this.SalvageValueValue; }
            set { SetProperty(ref SalvageValueValue, value); }
        }

        private System.Int32 UsefulLifeYearsValue;
        /// <summary>
        /// Gets or sets useful life years.
        /// </summary>
        public System.Int32 UsefulLifeYears
        {
            get { return this.UsefulLifeYearsValue; }
            set { SetProperty(ref UsefulLifeYearsValue, value); }
        }

        private DepreciationMethod DepreciationMethodValue;
        /// <summary>
        /// Gets or sets depreciation method.
        /// </summary>
        public DepreciationMethod DepreciationMethod
        {
            get { return this.DepreciationMethodValue; }
            set { SetProperty(ref DepreciationMethodValue, value); }
        }

        private System.Decimal DepreciableBaseValue;
        /// <summary>
        /// Gets or sets depreciable base.
        /// </summary>
        public System.Decimal DepreciableBase
        {
            get { return this.DepreciableBaseValue; }
            set { SetProperty(ref DepreciableBaseValue, value); }
        }

        private List<DepreciationEntry> DepreciationScheduleValue = new List<DepreciationEntry>();
        /// <summary>
        /// Gets or sets depreciation schedule.
        /// </summary>
        public List<DepreciationEntry> DepreciationSchedule
        {
            get { return this.DepreciationScheduleValue; }
            set { SetProperty(ref DepreciationScheduleValue, value); }
        }

        private System.Decimal TotalScheduledDepreciationValue;
        /// <summary>
        /// Gets or sets total scheduled depreciation.
        /// </summary>
        public System.Decimal TotalScheduledDepreciation
        {
            get { return this.TotalScheduledDepreciationValue; }
            set { SetProperty(ref TotalScheduledDepreciationValue, value); }
        }

        private System.DateTime CreatedDateValue;
        /// <summary>
        /// Gets or sets created date.
        /// </summary>
        public System.DateTime CreatedDate
        {
            get { return this.CreatedDateValue; }
            set { SetProperty(ref CreatedDateValue, value); }
        }

        private System.String CreatedByValue = string.Empty;
        /// <summary>
        /// Gets or sets created by.
        /// </summary>
        public System.String CreatedBy
        {
            get { return this.CreatedByValue; }
            set { SetProperty(ref CreatedByValue, value); }
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
    /// Depreciation Entry
    /// </summary>
    public partial class DepreciationEntry : AccountingEntityBase
    {
        private System.Int32 YearValue;
        /// <summary>
        /// Gets or sets year.
        /// </summary>
        public System.Int32 Year
        {
            get { return this.YearValue; }
            set { SetProperty(ref YearValue, value); }
        }

        private System.Decimal AnnualDepreciationValue;
        /// <summary>
        /// Gets or sets annual depreciation.
        /// </summary>
        public System.Decimal AnnualDepreciation
        {
            get { return this.AnnualDepreciationValue; }
            set { SetProperty(ref AnnualDepreciationValue, value); }
        }

        private System.Decimal AccumulatedDepreciationValue;
        /// <summary>
        /// Gets or sets accumulated depreciation.
        /// </summary>
        public System.Decimal AccumulatedDepreciation
        {
            get { return this.AccumulatedDepreciationValue; }
            set { SetProperty(ref AccumulatedDepreciationValue, value); }
        }

        private System.Decimal BookValueValue;
        /// <summary>
        /// Gets or sets book value.
        /// </summary>
        public System.Decimal BookValue
        {
            get { return this.BookValueValue; }
            set { SetProperty(ref BookValueValue, value); }
        }

        private System.String MethodValue = string.Empty;
        /// <summary>
        /// Gets or sets method.
        /// </summary>
        public System.String Method
        {
            get { return this.MethodValue; }
            set { SetProperty(ref MethodValue, value); }
        }
    }

    /// <summary>
    /// Asset Depreciation Summary
    /// </summary>
    public partial class AssetDepreciationSummary : AccountingEntityBase
    {
        private System.DateTime AsOfDateValue;
        /// <summary>
        /// Gets or sets as of date.
        /// </summary>
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
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

        private List<AssetSummaryLine> AssetsValue = new List<AssetSummaryLine>();
        /// <summary>
        /// Gets or sets assets.
        /// </summary>
        public List<AssetSummaryLine> Assets
        {
            get { return this.AssetsValue; }
            set { SetProperty(ref AssetsValue, value); }
        }

        private System.Decimal TotalFixedAssetsCostValue;
        /// <summary>
        /// Gets or sets total fixed assets cost.
        /// </summary>
        public System.Decimal TotalFixedAssetsCost
        {
            get { return this.TotalFixedAssetsCostValue; }
            set { SetProperty(ref TotalFixedAssetsCostValue, value); }
        }

        private System.Decimal TotalAccumulatedDepreciationValue;
        /// <summary>
        /// Gets or sets total accumulated depreciation.
        /// </summary>
        public System.Decimal TotalAccumulatedDepreciation
        {
            get { return this.TotalAccumulatedDepreciationValue; }
            set { SetProperty(ref TotalAccumulatedDepreciationValue, value); }
        }

        private System.Decimal TotalNetBookValueValue;
        /// <summary>
        /// Gets or sets total net book value.
        /// </summary>
        public System.Decimal TotalNetBookValue
        {
            get { return this.TotalNetBookValueValue; }
            set { SetProperty(ref TotalNetBookValueValue, value); }
        }
    }

    /// <summary>
    /// Asset Summary Line
    /// </summary>
    public partial class AssetSummaryLine : AccountingEntityBase
    {
        private System.String AssetIdValue = string.Empty;
        /// <summary>
        /// Gets or sets asset id.
        /// </summary>
        public System.String AssetId
        {
            get { return this.AssetIdValue; }
            set { SetProperty(ref AssetIdValue, value); }
        }

        private System.String AssetNameValue = string.Empty;
        /// <summary>
        /// Gets or sets asset name.
        /// </summary>
        public System.String AssetName
        {
            get { return this.AssetNameValue; }
            set { SetProperty(ref AssetNameValue, value); }
        }

        private System.Decimal CostBasisValue;
        /// <summary>
        /// Gets or sets cost basis.
        /// </summary>
        public System.Decimal CostBasis
        {
            get { return this.CostBasisValue; }
            set { SetProperty(ref CostBasisValue, value); }
        }

        private System.Decimal AccumulatedDepreciationValue;
        /// <summary>
        /// Gets or sets accumulated depreciation.
        /// </summary>
        public System.Decimal AccumulatedDepreciation
        {
            get { return this.AccumulatedDepreciationValue; }
            set { SetProperty(ref AccumulatedDepreciationValue, value); }
        }

        private System.Decimal NetBookValueValue;
        /// <summary>
        /// Gets or sets net book value.
        /// </summary>
        public System.Decimal NetBookValue
        {
            get { return this.NetBookValueValue; }
            set { SetProperty(ref NetBookValueValue, value); }
        }
    }
}

