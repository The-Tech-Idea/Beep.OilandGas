using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Common
{
    public partial class UNPROVED_PROPERTY : ModelEntityBase
    {
        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.String PROPERTY_NAMEValue;
        public System.String PROPERTY_NAME
        {
            get { return this.PROPERTY_NAMEValue; }
            set { SetProperty(ref PROPERTY_NAMEValue, value); }
        }

        private System.Decimal? ACQUISITION_COSTValue;
        public System.Decimal? ACQUISITION_COST
        {
            get { return this.ACQUISITION_COSTValue; }
            set { SetProperty(ref ACQUISITION_COSTValue, value); }
        }

        private System.DateTime? ACQUISITION_DATEValue;
        public System.DateTime? ACQUISITION_DATE
        {
            get { return this.ACQUISITION_DATEValue; }
            set { SetProperty(ref ACQUISITION_DATEValue, value); }
        }

        private System.String PROPERTY_TYPEValue;
        public System.String PROPERTY_TYPE
        {
            get { return this.PROPERTY_TYPEValue; }
            set { SetProperty(ref PROPERTY_TYPEValue, value); }
        }

        private System.Decimal? WORKING_INTERESTValue;
        public System.Decimal? WORKING_INTEREST
        {
            get { return this.WORKING_INTERESTValue; }
            set { SetProperty(ref WORKING_INTERESTValue, value); }
        }

        private System.Decimal? NET_REVENUE_INTERESTValue;
        public System.Decimal? NET_REVENUE_INTEREST
        {
            get { return this.NET_REVENUE_INTERESTValue; }
            set { SetProperty(ref NET_REVENUE_INTERESTValue, value); }
        }

        private System.Decimal? ACCUMULATED_IMPAIRMENTValue;
        public System.Decimal? ACCUMULATED_IMPAIRMENT
        {
            get { return this.ACCUMULATED_IMPAIRMENTValue; }
            set { SetProperty(ref ACCUMULATED_IMPAIRMENTValue, value); }
        }

        private System.String IS_PROVEDValue;
        public System.String IS_PROVED
        {
            get { return this.IS_PROVEDValue; }
            set { SetProperty(ref IS_PROVEDValue, value); }
        }

        private System.DateTime? PROVED_DATEValue;
        public System.DateTime? PROVED_DATE
        {
            get { return this.PROVED_DATEValue; }
            set { SetProperty(ref PROVED_DATEValue, value); }
        }

        private System.String COST_CENTER_IDValue;
        public System.String COST_CENTER_ID
        {
            get { return this.COST_CENTER_IDValue; }
            set { SetProperty(ref COST_CENTER_IDValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


