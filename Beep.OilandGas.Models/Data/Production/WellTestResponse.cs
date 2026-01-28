using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WellTestResponse : ModelEntityBase
    {
        private string TestIdValue = string.Empty;

        public string TestId

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

        }
        private string? TestPurposeValue;

        public string? TestPurpose

        {

            get { return this.TestPurposeValue; }

            set { SetProperty(ref TestPurposeValue, value); }

        }
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

        }
        private string? TestStatusValue;

        public string? TestStatus

        {

            get { return this.TestStatusValue; }

            set { SetProperty(ref TestStatusValue, value); }

        }
        
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
        private string? PreferredIndValue;

        public string? PreferredInd

        {

            get { return this.PreferredIndValue; }

            set { SetProperty(ref PreferredIndValue, value); }

        }
        
        // Audit fields
        private DateTime? CreateDateValue;

        public DateTime? CreateDate

        {

            get { return this.CreateDateValue; }

            set { SetProperty(ref CreateDateValue, value); }

        }
        private string? CreateUserValue;

        public string? CreateUser

        {

            get { return this.CreateUserValue; }

            set { SetProperty(ref CreateUserValue, value); }

        }
        private DateTime? UpdateDateValue;

        public DateTime? UpdateDate

        {

            get { return this.UpdateDateValue; }

            set { SetProperty(ref UpdateDateValue, value); }

        }
        private string? UpdateUserValue;

        public string? UpdateUser

        {

            get { return this.UpdateUserValue; }

            set { SetProperty(ref UpdateUserValue, value); }

        }
    }
}
