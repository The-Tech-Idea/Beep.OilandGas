using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class FixedAsset : ModelEntityBase
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
}
