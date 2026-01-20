using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

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

        private DateTime? EFFECTIVE_DATEValue;
        public DateTime? EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private DateTime? EXPIRY_DATEValue;
        public DateTime? EXPIRY_DATE
        {
            get { return this.EXPIRY_DATEValue; }
            set { SetProperty(ref EXPIRY_DATEValue, value); }
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
        private String ACTIVE_INDValue;
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue;
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String REMARKValue;
        public String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private String SOURCEValue;
        public String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private String ROW_CREATED_BYValue;
        public String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private String ROW_CHANGED_BYValue;
        public String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
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

        private String ROW_QUALITYValue;
        public String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

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

        public object Components { get; set; }
    }
}



