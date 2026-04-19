using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class DepreciationEntry : ModelEntityBase
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
}
