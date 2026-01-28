using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class AssetDepreciationSummary : ModelEntityBase
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
}
