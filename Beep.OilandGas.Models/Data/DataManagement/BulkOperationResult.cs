using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class BulkOperationResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int ProcessedCountValue;

        public int ProcessedCount

        {

            get { return this.ProcessedCountValue; }

            set { SetProperty(ref ProcessedCountValue, value); }

        }
        private int SuccessCountValue;

        public int SuccessCount

        {

            get { return this.SuccessCountValue; }

            set { SetProperty(ref SuccessCountValue, value); }

        }
        private int FailureCountValue;

        public int FailureCount

        {

            get { return this.FailureCountValue; }

            set { SetProperty(ref FailureCountValue, value); }

        }
        private List<BulkOperationError> ErrorsValue = new List<BulkOperationError>();

        public List<BulkOperationError> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
        private TimeSpan DurationValue;

        public TimeSpan Duration

        {

            get { return this.DurationValue; }

            set { SetProperty(ref DurationValue, value); }

        }
    }
}
