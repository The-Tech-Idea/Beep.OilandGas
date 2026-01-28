using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public partial class MIT_RESULT : ModelEntityBase {
        private String MIT_RESULT_IDValue;
        public String MIT_RESULT_ID
        {
            get { return this.MIT_RESULT_IDValue; }
            set { SetProperty(ref MIT_RESULT_IDValue, value); }
        }

        private String INJECTION_PERMIT_APPLICATION_IDValue;
        public String INJECTION_PERMIT_APPLICATION_ID
        {
            get { return this.INJECTION_PERMIT_APPLICATION_IDValue; }
            set { SetProperty(ref INJECTION_PERMIT_APPLICATION_IDValue, value); }
        }

        private DateTime? TEST_DATEValue;
        public DateTime? TEST_DATE
        {
            get { return this.TEST_DATEValue; }
            set { SetProperty(ref TEST_DATEValue, value); }
        }

        private String TEST_TYPEValue;
        public String TEST_TYPE
        {
            get { return this.TEST_TYPEValue; }
            set { SetProperty(ref TEST_TYPEValue, value); }
        }

        private String PASSED_INDValue;
        public String PASSED_IND
        {
            get { return this.PASSED_INDValue; }
            set { SetProperty(ref PASSED_INDValue, value); }
        }

        private Decimal? TEST_PRESSUREValue;
        public Decimal? TEST_PRESSURE
        {
            get { return this.TEST_PRESSUREValue; }
            set { SetProperty(ref TEST_PRESSUREValue, value); }
        }

        private String TEST_RESULTValue;
        public String TEST_RESULT
        {
            get { return this.TEST_RESULTValue; }
            set { SetProperty(ref TEST_RESULTValue, value); }
        }

        private String REMARKSValue;
        public String REMARKS
        {
            get { return this.REMARKSValue; }
            set { SetProperty(ref REMARKSValue, value); }
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

    }
}
