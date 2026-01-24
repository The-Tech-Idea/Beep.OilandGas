using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class PERMIT_APPLICATION : ModelEntityBase {
        private String PERMIT_APPLICATION_IDValue;
        public String PERMIT_APPLICATION_ID
        {
            get { return this.PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref PERMIT_APPLICATION_IDValue, value); }
        }

        private String APPLICATION_TYPEValue;
        public String APPLICATION_TYPE
        {
            get { return this.APPLICATION_TYPEValue; }
            set { SetProperty(ref APPLICATION_TYPEValue, value); }
        }

        private String STATUSValue;
        public String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private String COUNTRYValue;
        public String COUNTRY
        {
            get { return this.COUNTRYValue; }
            set { SetProperty(ref COUNTRYValue, value); }
        }

        private String STATE_PROVINCEValue;
        public String STATE_PROVINCE
        {
            get { return this.STATE_PROVINCEValue; }
            set { SetProperty(ref STATE_PROVINCEValue, value); }
        }

        private String REGULATORY_AUTHORITYValue;
        public String REGULATORY_AUTHORITY
        {
            get { return this.REGULATORY_AUTHORITYValue; }
            set { SetProperty(ref REGULATORY_AUTHORITYValue, value); }
        }

        private DateTime? CREATED_DATEValue;
        public DateTime? CREATED_DATE
        {
            get { return this.CREATED_DATEValue; }
            set { SetProperty(ref CREATED_DATEValue, value); }
        }

        private DateTime? SUBMITTED_DATEValue;
        public DateTime? SUBMITTED_DATE
        {
            get { return this.SUBMITTED_DATEValue; }
            set { SetProperty(ref SUBMITTED_DATEValue, value); }
        }

        private DateTime? RECEIVED_DATEValue;
        public DateTime? RECEIVED_DATE
        {
            get { return this.RECEIVED_DATEValue; }
            set { SetProperty(ref RECEIVED_DATEValue, value); }
        }

        private DateTime? DECISION_DATEValue;
        public DateTime? DECISION_DATE
        {
            get { return this.DECISION_DATEValue; }
            set { SetProperty(ref DECISION_DATEValue, value); }
        }


        private String DECISIONValue;
        public String DECISION
        {
            get { return this.DECISIONValue; }
            set { SetProperty(ref DECISIONValue, value); }
        }

        private String REFERENCE_NUMBERValue;
        public String REFERENCE_NUMBER
        {
            get { return this.REFERENCE_NUMBERValue; }
            set { SetProperty(ref REFERENCE_NUMBERValue, value); }
        }

        private String APPLICANT_IDValue;
        public String APPLICANT_ID
        {
            get { return this.APPLICANT_IDValue; }
            set { SetProperty(ref APPLICANT_IDValue, value); }
        }

        private String OPERATOR_IDValue;
        public String OPERATOR_ID
        {
            get { return this.OPERATOR_IDValue; }
            set { SetProperty(ref OPERATOR_IDValue, value); }
        }

        private String RELATED_WELL_UWIValue;
        public String RELATED_WELL_UWI
        {
            get { return this.RELATED_WELL_UWIValue; }
            set { SetProperty(ref RELATED_WELL_UWIValue, value); }
        }

        private String RELATED_FACILITY_IDValue;
        public String RELATED_FACILITY_ID
        {
            get { return this.RELATED_FACILITY_IDValue; }
            set { SetProperty(ref RELATED_FACILITY_IDValue, value); }
        }

        private String FEES_DESCRIPTIONValue;
        public String FEES_DESCRIPTION
        {
            get { return this.FEES_DESCRIPTIONValue; }
            set { SetProperty(ref FEES_DESCRIPTIONValue, value); }
        }

        private String FEES_PAID_INDValue;
        public String FEES_PAID_IND
        {
            get { return this.FEES_PAID_INDValue; }
            set { SetProperty(ref FEES_PAID_INDValue, value); }
        }

        private String REMARKSValue;
        public String REMARKS
        {
            get { return this.REMARKSValue; }
            set { SetProperty(ref REMARKSValue, value); }
        }

        private String SUBMISSION_COMPLETE_INDValue;
        public String SUBMISSION_COMPLETE_IND
        {
            get { return this.SUBMISSION_COMPLETE_INDValue; }
            set { SetProperty(ref SUBMISSION_COMPLETE_INDValue, value); }
        }

        private String SUBMISSION_DESCRIPTIONValue;
        public String SUBMISSION_DESCRIPTION
        {
            get { return this.SUBMISSION_DESCRIPTIONValue; }
            set { SetProperty(ref SUBMISSION_DESCRIPTIONValue, value); }
        }

        // Standard PPDM columns

        // Optional PPDM properties
        private String AREA_IDValue;
        public String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private String AREA_TYPEValue;
        public String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private String BUSINESS_ASSOCIATE_IDValue;
        public String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

        private object ComponentsValue;

        public object Components

        {

            get { return this.ComponentsValue; }

            set { SetProperty(ref ComponentsValue, value); }

        }
    }
}


