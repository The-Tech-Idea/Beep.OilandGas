using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class FileImportResult : ModelEntityBase
    {
        private string FilePathValue = string.Empty;

        public string FilePath

        {

            get { return this.FilePathValue; }

            set { SetProperty(ref FilePathValue, value); }

        }
        private int TotalRowsValue;

        public int TotalRows

        {

            get { return this.TotalRowsValue; }

            set { SetProperty(ref TotalRowsValue, value); }

        }
        private int SuccessCountValue;

        public int SuccessCount

        {

            get { return this.SuccessCountValue; }

            set { SetProperty(ref SuccessCountValue, value); }

        }
        private int ErrorCountValue;

        public int ErrorCount

        {

            get { return this.ErrorCountValue; }

            set { SetProperty(ref ErrorCountValue, value); }

        }
        private List<FileImportError> ErrorsValue = new List<FileImportError>();

        public List<FileImportError> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
    }
}
