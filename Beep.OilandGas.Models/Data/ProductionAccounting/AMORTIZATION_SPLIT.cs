using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class AMORTIZATION_SPLIT : Entity, IPPDMEntity
    {
        private string AMORTIZATION_SPLIT_IDValue;
        public string AMORTIZATION_SPLIT_ID
        {
            get { return this.AMORTIZATION_SPLIT_IDValue; }
            set { SetProperty(ref AMORTIZATION_SPLIT_IDValue, value); }
        }

        private string PROPERTY_IDValue;
        public string PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private DateTime? PERIOD_END_DATEValue;
        public DateTime? PERIOD_END_DATE
        {
            get { return this.PERIOD_END_DATEValue; }
            set { SetProperty(ref PERIOD_END_DATEValue, value); }
        }

        private decimal? OIL_DEPLETIONValue;
        public decimal? OIL_DEPLETION
        {
            get { return this.OIL_DEPLETIONValue; }
            set { SetProperty(ref OIL_DEPLETIONValue, value); }
        }

        private decimal? GAS_DEPLETIONValue;
        public decimal? GAS_DEPLETION
        {
            get { return this.GAS_DEPLETIONValue; }
            set { SetProperty(ref GAS_DEPLETIONValue, value); }
        }

        private decimal? WORKING_INTEREST_DEPLETIONValue;
        public decimal? WORKING_INTEREST_DEPLETION
        {
            get { return this.WORKING_INTEREST_DEPLETIONValue; }
            set { SetProperty(ref WORKING_INTEREST_DEPLETIONValue, value); }
        }

        private decimal? NON_WORKING_DEPLETIONValue;
        public decimal? NON_WORKING_DEPLETION
        {
            get { return this.NON_WORKING_DEPLETIONValue; }
            set { SetProperty(ref NON_WORKING_DEPLETIONValue, value); }
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

        private string ROW_QUALITYValue;
        public string ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
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
