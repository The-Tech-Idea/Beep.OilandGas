using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class INVENTORY_REPORT_SUMMARY : Entity, IPPDMEntity
    {
        private System.String INVENTORY_REPORT_SUMMARY_IDValue;
        public System.String INVENTORY_REPORT_SUMMARY_ID
        {
            get { return this.INVENTORY_REPORT_SUMMARY_IDValue; }
            set { SetProperty(ref INVENTORY_REPORT_SUMMARY_IDValue, value); }
        }

        private System.Decimal? OPENING_INVENTORYValue;
        public System.Decimal? OPENING_INVENTORY
        {
            get { return this.OPENING_INVENTORYValue; }
            set { SetProperty(ref OPENING_INVENTORYValue, value); }
        }

        private System.Decimal? RECEIPTSValue;
        public System.Decimal? RECEIPTS
        {
            get { return this.RECEIPTSValue; }
            set { SetProperty(ref RECEIPTSValue, value); }
        }

        private System.Decimal? DELIVERIESValue;
        public System.Decimal? DELIVERIES
        {
            get { return this.DELIVERIESValue; }
            set { SetProperty(ref DELIVERIESValue, value); }
        }

        private System.Decimal? CLOSING_INVENTORYValue;
        public System.Decimal? CLOSING_INVENTORY
        {
            get { return this.CLOSING_INVENTORYValue; }
            set { SetProperty(ref CLOSING_INVENTORYValue, value); }
        }

        // Standard PPDM columns
        private System.String ACTIVE_INDValue;
        public System.String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private System.String PPDM_GUIDValue;
        public System.String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.DateTime? ROW_EFFECTIVE_DATEValue;
        public System.DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? ROW_EXPIRY_DATEValue;
        public System.DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
