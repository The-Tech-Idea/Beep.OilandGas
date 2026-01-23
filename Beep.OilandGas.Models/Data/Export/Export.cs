using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Export
{
    public class ExportToCsvRequest : ModelEntityBase
    {
        private string EntityTypeValue;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private List<string>? EntityIdsValue;

        public List<string>? EntityIds

        {

            get { return this.EntityIdsValue; }

            set { SetProperty(ref EntityIdsValue, value); }

        }
        private string FilePathValue;

        public string FilePath

        {

            get { return this.FilePathValue; }

            set { SetProperty(ref FilePathValue, value); }

        }
        public Dictionary<string, object>? Filters { get; set; }
    }

    public class ExportToExcelRequest : ModelEntityBase
    {
        private string EntityTypeValue;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private List<string>? EntityIdsValue;

        public List<string>? EntityIds

        {

            get { return this.EntityIdsValue; }

            set { SetProperty(ref EntityIdsValue, value); }

        }
        private string FilePathValue;

        public string FilePath

        {

            get { return this.FilePathValue; }

            set { SetProperty(ref FilePathValue, value); }

        }
        public Dictionary<string, object>? Filters { get; set; }
        private string? SheetNameValue;

        public string? SheetName

        {

            get { return this.SheetNameValue; }

            set { SetProperty(ref SheetNameValue, value); }

        }
    }

    public class ExportToJsonRequest : ModelEntityBase
    {
        private string EntityTypeValue;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private List<string>? EntityIdsValue;

        public List<string>? EntityIds

        {

            get { return this.EntityIdsValue; }

            set { SetProperty(ref EntityIdsValue, value); }

        }
        private string FilePathValue;

        public string FilePath

        {

            get { return this.FilePathValue; }

            set { SetProperty(ref FilePathValue, value); }

        }
        public Dictionary<string, object>? Filters { get; set; }
    }

    public class ExportResult : ModelEntityBase
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
        private DateTime ExportDateValue = DateTime.UtcNow;

        public DateTime ExportDate

        {

            get { return this.ExportDateValue; }

            set { SetProperty(ref ExportDateValue, value); }

        }
        private bool IsSuccessValue;

        public bool IsSuccess

        {

            get { return this.IsSuccessValue; }

            set { SetProperty(ref IsSuccessValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }

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

    public class ScheduleExportRequest : ModelEntityBase
    {
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
        private string ScheduleFrequencyValue;

        public string ScheduleFrequency

        {

            get { return this.ScheduleFrequencyValue; }

            set { SetProperty(ref ScheduleFrequencyValue, value); }

        }
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? EndDateValue;

        public DateTime? EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private string OutputPathValue;

        public string OutputPath

        {

            get { return this.OutputPathValue; }

            set { SetProperty(ref OutputPathValue, value); }

        }
        public Dictionary<string, object>? ExportParameters { get; set; }
    }
}








