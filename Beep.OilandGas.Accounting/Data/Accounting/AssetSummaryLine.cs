using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class AssetSummaryLine : ModelEntityBase
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
