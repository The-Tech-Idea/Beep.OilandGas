using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Measurement
{
    public class MeasurementValidationResult : ModelEntityBase
    {
        private string MeasurementIdValue;

        public string MeasurementId

        {

            get { return this.MeasurementIdValue; }

            set { SetProperty(ref MeasurementIdValue, value); }

        }
        private bool IsValidValue;

        public bool IsValid

        {

            get { return this.IsValidValue; }

            set { SetProperty(ref IsValidValue, value); }

        }
        private List<string> ValidationErrorsValue = new();

        public List<string> ValidationErrors

        {

            get { return this.ValidationErrorsValue; }

            set { SetProperty(ref ValidationErrorsValue, value); }

        }
        private List<string> ValidationWarningsValue = new();

        public List<string> ValidationWarnings

        {

            get { return this.ValidationWarningsValue; }

            set { SetProperty(ref ValidationWarningsValue, value); }

        }
        private DateTime ValidationDateValue = DateTime.UtcNow;

        public DateTime ValidationDate

        {

            get { return this.ValidationDateValue; }

            set { SetProperty(ref ValidationDateValue, value); }

        }
        private string ValidatedByValue;

        public string ValidatedBy

        {

            get { return this.ValidatedByValue; }

            set { SetProperty(ref ValidatedByValue, value); }

        }
    }
}
