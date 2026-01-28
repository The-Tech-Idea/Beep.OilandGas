using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class INJECTION_PERMIT_APPLICATION : ModelEntityBase {
        private String INJECTION_PERMIT_APPLICATION_IDValue;
        public String INJECTION_PERMIT_APPLICATION_ID
        {
            get { return this.INJECTION_PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref INJECTION_PERMIT_APPLICATION_IDValue, value); }
        }

        private String PERMIT_APPLICATION_IDValue;
        public String PERMIT_APPLICATION_ID
        {
            get { return this.PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref PERMIT_APPLICATION_IDValue, value); }
        }

        private String INJECTION_TYPEValue;
        public String INJECTION_TYPE
        {
            get { return this.INJECTION_TYPEValue; }
            set { SetProperty(ref INJECTION_TYPEValue, value); }
        }

        private String INJECTION_ZONEValue;
        public String INJECTION_ZONE
        {
            get { return this.INJECTION_ZONEValue; }
            set { SetProperty(ref INJECTION_ZONEValue, value); }
        }

        private String INJECTION_FLUIDValue;
        public String INJECTION_FLUID
        {
            get { return this.INJECTION_FLUIDValue; }
            set { SetProperty(ref INJECTION_FLUIDValue, value); }
        }

        private Decimal? MAXIMUM_INJECTION_PRESSUREValue;
        public Decimal? MAXIMUM_INJECTION_PRESSURE
        {
            get { return this.MAXIMUM_INJECTION_PRESSUREValue; }
            set { SetProperty(ref MAXIMUM_INJECTION_PRESSUREValue, value); }
        }

        private Decimal? MAXIMUM_INJECTION_RATEValue;
        public Decimal? MAXIMUM_INJECTION_RATE
        {
            get { return this.MAXIMUM_INJECTION_RATEValue; }
            set { SetProperty(ref MAXIMUM_INJECTION_RATEValue, value); }
        }

        private String INJECTION_RATE_UNITValue;
        public String INJECTION_RATE_UNIT
        {
            get { return this.INJECTION_RATE_UNITValue; }
            set { SetProperty(ref INJECTION_RATE_UNITValue, value); }
        }

        private String MONITORING_REQUIREMENTSValue;
        public String MONITORING_REQUIREMENTS
        {
            get { return this.MONITORING_REQUIREMENTSValue; }
            set { SetProperty(ref MONITORING_REQUIREMENTSValue, value); }
        }

        private String INJECTION_WELL_UWIValue;
        public String INJECTION_WELL_UWI
        {
            get { return this.INJECTION_WELL_UWIValue; }
            set { SetProperty(ref INJECTION_WELL_UWIValue, value); }
        }

        private String IS_CO2_STORAGE_INDValue;
        public String IS_CO2_STORAGE_IND
        {
            get { return this.IS_CO2_STORAGE_INDValue; }
            set { SetProperty(ref IS_CO2_STORAGE_INDValue, value); }
        }

        private String IS_GAS_STORAGE_INDValue;
        public String IS_GAS_STORAGE_IND
        {
            get { return this.IS_GAS_STORAGE_INDValue; }
            set { SetProperty(ref IS_GAS_STORAGE_INDValue, value); }
        }

        private String IS_BRINE_MINING_INDValue;
        public String IS_BRINE_MINING_IND
        {
            get { return this.IS_BRINE_MINING_INDValue; }
            set { SetProperty(ref IS_BRINE_MINING_INDValue, value); }
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

        public string WELL_UWI { get; set; }
        public PermitApplicationType APPLICATION_TYPE { get; set; }
        public PermitApplicationStatus STATUS { get; set; }
        public Country COUNTRY { get; set; }
        public StateProvince STATE_PROVINCE { get; set; }
        public RegulatoryAuthority REGULATORY_AUTHORITY { get; set; }
        public DateTime? CREATED_DATE { get; set; }
        public DateTime? SUBMITTED_DATE { get; set; }
        public DateTime? RECEIVED_DATE { get; set; }
        public DateTime? DECISION_DATE { get; set; }
        public string DECISION { get; set; }
        public string REFERENCE_NUMBER { get; set; }
        public string FEES_DESCRIPTION { get; set; }
        public bool FEES_PAID { get; set; }
        public string REMARKS { get; set; }
        public bool SUBMISSION_COMPLETE { get; set; }
        public string SUBMISSION_DESCRIPTION { get; set; }
        public List<APPLICATION_ATTACHMENT> ATTACHMENTS { get; set; }
        public List<APPLICATION_AREA> AREAS { get; set; }
        public object COMPONENTS { get; set; }
    }
}
