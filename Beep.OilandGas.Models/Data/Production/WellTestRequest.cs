using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WellTestRequest : ModelEntityBase
    {
        private string? TestIdValue;

        public string? TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? WellboreIdValue;

        public string? WellboreId

        {

            get { return this.WellboreIdValue; }

            set { SetProperty(ref WellboreIdValue, value); }

        }
        
        // Test classification
        private string? TestTypeValue;

        public string? TestType

        {

            get { return this.TestTypeValue; }

            set { SetProperty(ref TestTypeValue, value); }

        } // e.g., "FLOW_TEST", "BUILDUP_TEST", "INTERFERENCE_TEST"
        private string? TestPurposeValue;

        public string? TestPurpose

        {

            get { return this.TestPurposeValue; }

            set { SetProperty(ref TestPurposeValue, value); }

        } // e.g., "PRODUCTION_TEST", "DRILLSTEM_TEST"
        private string? TestNameValue;

        public string? TestName

        {

            get { return this.TestNameValue; }

            set { SetProperty(ref TestNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Test dates
        private DateTime? TestStartDateValue;

        public DateTime? TestStartDate

        {

            get { return this.TestStartDateValue; }

            set { SetProperty(ref TestStartDateValue, value); }

        }
        private DateTime? TestEndDateValue;

        public DateTime? TestEndDate

        {

            get { return this.TestEndDateValue; }

            set { SetProperty(ref TestEndDateValue, value); }

        }
        private DateTime? TestDateValue;

        public DateTime? TestDate

        {

            get { return this.TestDateValue; }

            set { SetProperty(ref TestDateValue, value); }

        }
        
        // Test conditions
        private decimal? TestDurationValue;

        public decimal? TestDuration

        {

            get { return this.TestDurationValue; }

            set { SetProperty(ref TestDurationValue, value); }

        } // Hours
        private string? TestStatusValue;

        public string? TestStatus

        {

            get { return this.TestStatusValue; }

            set { SetProperty(ref TestStatusValue, value); }

        } // e.g., "COMPLETED", "ABANDONED", "IN_PROGRESS"
        
        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
    }
}
