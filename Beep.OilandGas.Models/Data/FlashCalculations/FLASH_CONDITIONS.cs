using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.FlashCalculations
{
    public partial class FLASH_CONDITIONS : ModelEntityBase {
        private String FLASH_CONDITIONS_IDValue;
        public String FLASH_CONDITIONS_ID
        {
            get { return this.FLASH_CONDITIONS_IDValue; }
            set { SetProperty(ref FLASH_CONDITIONS_IDValue, value); }
        }

        private String FLASH_CALCULATION_RESULT_IDValue;
        public String FLASH_CALCULATION_RESULT_ID
        {
            get { return this.FLASH_CALCULATION_RESULT_IDValue; }
            set { SetProperty(ref FLASH_CALCULATION_RESULT_IDValue, value); }
        }

        private Decimal PRESSUREValue;
        public Decimal PRESSURE
        {
            get { return this.PRESSUREValue; }
            set { SetProperty(ref PRESSUREValue, value); }
        }

        private Decimal TEMPERATUREValue;
        public Decimal TEMPERATURE
        {
            get { return this.TEMPERATUREValue; }
            set { SetProperty(ref TEMPERATUREValue, value); }
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
        private List<FLASH_COMPONENT> FeedCompositionValue;
        public List<FLASH_COMPONENT> FEED_COMPOSITION
        {
            get { return this.FeedCompositionValue; }
            set { SetProperty(ref FeedCompositionValue, value); }
        }
       
    }
}
