using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class LEASE_ACCOUNTING_ENTRY : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
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

        private string ACTIVE_INDValue;
        public string ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private string PPDM_GUIDValue;
        public string PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private string ROW_CREATED_BYValue;
        public string ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private string ROW_CHANGED_BYValue;
        public string ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }
    }
}
