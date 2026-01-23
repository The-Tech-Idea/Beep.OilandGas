using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class LEASE_ACCOUNTING_ENTRY : ModelEntityBase
    {
        private string LEASE_ACCOUNTING_ENTRY_IDValue;
        public string LEASE_ACCOUNTING_ENTRY_ID
        {
            get { return this.LEASE_ACCOUNTING_ENTRY_IDValue; }
            set { SetProperty(ref LEASE_ACCOUNTING_ENTRY_IDValue, value); }
        }

        private string LEASE_IDValue;
        public string LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private DateTime? MEASUREMENT_DATEValue;
        public DateTime? MEASUREMENT_DATE
        {
            get { return this.MEASUREMENT_DATEValue; }
            set { SetProperty(ref MEASUREMENT_DATEValue, value); }
        }

        private decimal? ROU_ASSETValue;
        public decimal? ROU_ASSET
        {
            get { return this.ROU_ASSETValue; }
            set { SetProperty(ref ROU_ASSETValue, value); }
        }

        private decimal? LEASE_LIABILITYValue;
        public decimal? LEASE_LIABILITY
        {
            get { return this.LEASE_LIABILITYValue; }
            set { SetProperty(ref LEASE_LIABILITYValue, value); }
        }

        private decimal? INTEREST_EXPENSEValue;
        public decimal? INTEREST_EXPENSE
        {
            get { return this.INTEREST_EXPENSEValue; }
            set { SetProperty(ref INTEREST_EXPENSEValue, value); }
        }

        private decimal? AMORTIZATION_EXPENSEValue;
        public decimal? AMORTIZATION_EXPENSE
        {
            get { return this.AMORTIZATION_EXPENSEValue; }
            set { SetProperty(ref AMORTIZATION_EXPENSEValue, value); }
        }

        private string CURRENCY_CODEValue;
        public string CURRENCY_CODE
        {
            get { return this.CURRENCY_CODEValue; }
            set { SetProperty(ref CURRENCY_CODEValue, value); }
        }
    }
}


