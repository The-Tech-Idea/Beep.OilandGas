using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class COST_SHARING : Entity, IPPDMEntity
    {
        private System.String COST_SHARING_IDValue;
        public System.String COST_SHARING_ID
        {
            get { return this.COST_SHARING_IDValue; }
            set { SetProperty(ref COST_SHARING_IDValue, value); }
        }

        private System.String UNIT_OPERATING_AGREEMENT_IDValue;
        public System.String UNIT_OPERATING_AGREEMENT_ID
        {
            get { return this.UNIT_OPERATING_AGREEMENT_IDValue; }
            set { SetProperty(ref UNIT_OPERATING_AGREEMENT_IDValue, value); }
        }

        private System.String BASED_ON_WORKING_INTERESTValue;
        public System.String BASED_ON_WORKING_INTEREST
        {
            get { return this.BASED_ON_WORKING_INTERESTValue; }
            set { SetProperty(ref BASED_ON_WORKING_INTERESTValue, value); }
        }

        private System.String BASED_ON_TRACT_PARTICIPATIONValue;
        public System.String BASED_ON_TRACT_PARTICIPATION
        {
            get { return this.BASED_ON_TRACT_PARTICIPATIONValue; }
            set { SetProperty(ref BASED_ON_TRACT_PARTICIPATIONValue, value); }
        }

        private System.Decimal? OPERATOR_OVERHEAD_PERCENTAGEValue;
        public System.Decimal? OPERATOR_OVERHEAD_PERCENTAGE
        {
            get { return this.OPERATOR_OVERHEAD_PERCENTAGEValue; }
            set { SetProperty(ref OPERATOR_OVERHEAD_PERCENTAGEValue, value); }
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
