using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class ENVIRONMENTAL_PERMIT_APPLICATION : ModelEntityBase {
        private String ENVIRONMENTAL_PERMIT_APPLICATION_IDValue;
        public String ENVIRONMENTAL_PERMIT_APPLICATION_ID
        {
            get { return this.ENVIRONMENTAL_PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref ENVIRONMENTAL_PERMIT_APPLICATION_IDValue, value); }
        }

        private String PERMIT_APPLICATION_IDValue;
        public String PERMIT_APPLICATION_ID
        {
            get { return this.PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref PERMIT_APPLICATION_IDValue, value); }
        }

        private String ENVIRONMENTAL_PERMIT_TYPEValue;
        public String ENVIRONMENTAL_PERMIT_TYPE
        {
            get { return this.ENVIRONMENTAL_PERMIT_TYPEValue; }
            set { SetProperty(ref ENVIRONMENTAL_PERMIT_TYPEValue, value); }
        }

        private String WASTE_TYPEValue;
        public String WASTE_TYPE
        {
            get { return this.WASTE_TYPEValue; }
            set { SetProperty(ref WASTE_TYPEValue, value); }
        }

        private Decimal? WASTE_VOLUMEValue;
        public Decimal? WASTE_VOLUME
        {
            get { return this.WASTE_VOLUMEValue; }
            set { SetProperty(ref WASTE_VOLUMEValue, value); }
        }

        private String WASTE_VOLUME_UNITValue;
        public String WASTE_VOLUME_UNIT
        {
            get { return this.WASTE_VOLUME_UNITValue; }
            set { SetProperty(ref WASTE_VOLUME_UNITValue, value); }
        }

        private String DISPOSAL_METHODValue;
        public String DISPOSAL_METHOD
        {
            get { return this.DISPOSAL_METHODValue; }
            set { SetProperty(ref DISPOSAL_METHODValue, value); }
        }

        private String ENVIRONMENTAL_IMPACTValue;
        public String ENVIRONMENTAL_IMPACT
        {
            get { return this.ENVIRONMENTAL_IMPACTValue; }
            set { SetProperty(ref ENVIRONMENTAL_IMPACTValue, value); }
        }

        private String MONITORING_PLANValue;
        public String MONITORING_PLAN
        {
            get { return this.MONITORING_PLANValue; }
            set { SetProperty(ref MONITORING_PLANValue, value); }
        }

        private String NORM_INVOLVED_INDValue;
        public String NORM_INVOLVED_IND
        {
            get { return this.NORM_INVOLVED_INDValue; }
            set { SetProperty(ref NORM_INVOLVED_INDValue, value); }
        }

        private String FACILITY_LOCATIONValue;
        public String FACILITY_LOCATION
        {
            get { return this.FACILITY_LOCATIONValue; }
            set { SetProperty(ref FACILITY_LOCATIONValue, value); }
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


        private bool NORMINVOLVEDValue;
        public object PROPOSED_DEPTH;

        public bool NORMINVOLVED
        {
            get { return this.NORMINVOLVEDValue; }
            set { SetProperty(ref NORMINVOLVEDValue, value); }
        }

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
        public object WELL_UWI { get; set; }
        public object TARGET_FORMATION { get; set; }
        public object DRILLING_METHOD { get; set; }
        public object SURFACE_OWNER_NOTIFIED { get; set; }
    }
}
