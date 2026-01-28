using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class ImportResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int TotalRecordsValue;

        public int TotalRecords

        {

            get { return this.TotalRecordsValue; }

            set { SetProperty(ref TotalRecordsValue, value); }

        }
        private int ImportedCountValue;

        public int ImportedCount

        {

            get { return this.ImportedCountValue; }

            set { SetProperty(ref ImportedCountValue, value); }

        }
        private int SkippedCountValue;

        public int SkippedCount

        {

            get { return this.SkippedCountValue; }

            set { SetProperty(ref SkippedCountValue, value); }

        }
        private int ErrorCountValue;

        public int ErrorCount

        {

            get { return this.ErrorCountValue; }

            set { SetProperty(ref ErrorCountValue, value); }

        }
        private List<ImportError> ErrorsValue = new List<ImportError>();

        public List<ImportError> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
        private string OutputPathValue;

        public string OutputPath

        {

            get { return this.OutputPathValue; }

            set { SetProperty(ref OutputPathValue, value); }

        }
    }
}
