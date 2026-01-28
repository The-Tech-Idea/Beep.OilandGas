using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Export
{
    public class ExportHistoryResponse : ModelEntityBase
    {
        private string ExportIdValue;

        public string ExportId

        {

            get { return this.ExportIdValue; }

            set { SetProperty(ref ExportIdValue, value); }

        }
        private string ExportTypeValue;

        public string ExportType

        {

            get { return this.ExportTypeValue; }

            set { SetProperty(ref ExportTypeValue, value); }

        }
        private string ExportFormatValue;

        public string ExportFormat

        {

            get { return this.ExportFormatValue; }

            set { SetProperty(ref ExportFormatValue, value); }

        }
        private DateTime ExportDateValue;

        public DateTime ExportDate

        {

            get { return this.ExportDateValue; }

            set { SetProperty(ref ExportDateValue, value); }

        }
        private string ExportedByValue;

        public string ExportedBy

        {

            get { return this.ExportedByValue; }

            set { SetProperty(ref ExportedByValue, value); }

        }
        private string FilePathValue;

        public string FilePath

        {

            get { return this.FilePathValue; }

            set { SetProperty(ref FilePathValue, value); }

        }
        private int RecordCountValue;

        public int RecordCount

        {

            get { return this.RecordCountValue; }

            set { SetProperty(ref RecordCountValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}
