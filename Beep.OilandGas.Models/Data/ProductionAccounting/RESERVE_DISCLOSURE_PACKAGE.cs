using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class RESERVE_DISCLOSURE_PACKAGE : ModelEntityBase
    {
        private string RESERVE_DISCLOSURE_IDValue;
        public string RESERVE_DISCLOSURE_ID
        {
            get { return this.RESERVE_DISCLOSURE_IDValue; }
            set { SetProperty(ref RESERVE_DISCLOSURE_IDValue, value); }
        }

        private string PROPERTY_IDValue;
        public string PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private DateTime? AS_OF_DATEValue;
        public DateTime? AS_OF_DATE
        {
            get { return this.AS_OF_DATEValue; }
            set { SetProperty(ref AS_OF_DATEValue, value); }
        }

        private decimal? TOTAL_PROVED_OILValue;
        public decimal? TOTAL_PROVED_OIL
        {
            get { return this.TOTAL_PROVED_OILValue; }
            set { SetProperty(ref TOTAL_PROVED_OILValue, value); }
        }

        private decimal? TOTAL_PROVED_GASValue;
        public decimal? TOTAL_PROVED_GAS
        {
            get { return this.TOTAL_PROVED_GASValue; }
            set { SetProperty(ref TOTAL_PROVED_GASValue, value); }
        }

        private decimal? PV10Value;
        public decimal? PV10
        {
            get { return this.PV10Value; }
            set { SetProperty(ref PV10Value, value); }
        }

        private decimal? DISCOUNT_RATEValue;
        public decimal? DISCOUNT_RATE
        {
            get { return this.DISCOUNT_RATEValue; }
            set { SetProperty(ref DISCOUNT_RATEValue, value); }
        }

        private string DISCLOSURE_NOTESValue;
        public string DISCLOSURE_NOTES
        {
            get { return this.DISCLOSURE_NOTESValue; }
            set { SetProperty(ref DISCLOSURE_NOTESValue, value); }
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

        private string REMARKValue;
        public string REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private string SOURCEValue;
        public string SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
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

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}

