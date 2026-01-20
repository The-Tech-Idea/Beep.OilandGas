using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Export
{
    public class ExportToCsvRequest : ModelEntityBase
    {
        public string EntityType { get; set; }
        public List<string>? EntityIds { get; set; }
        public string FilePath { get; set; }
        public Dictionary<string, object>? Filters { get; set; }
    }

    public class ExportToExcelRequest : ModelEntityBase
    {
        public string EntityType { get; set; }
        public List<string>? EntityIds { get; set; }
        public string FilePath { get; set; }
        public Dictionary<string, object>? Filters { get; set; }
        public string? SheetName { get; set; }
    }

    public class ExportToJsonRequest : ModelEntityBase
    {
        public string EntityType { get; set; }
        public List<string>? EntityIds { get; set; }
        public string FilePath { get; set; }
        public Dictionary<string, object>? Filters { get; set; }
    }

    public class ExportResult : ModelEntityBase
    {
        public string ExportId { get; set; }
        public string ExportType { get; set; }
        public string ExportFormat { get; set; }
        public string FilePath { get; set; }
        public int RecordCount { get; set; }
        public DateTime ExportDate { get; set; } = DateTime.UtcNow;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ExportHistoryResponse : ModelEntityBase
    {
        public string ExportId { get; set; }
        public string ExportType { get; set; }
        public string ExportFormat { get; set; }
        public DateTime ExportDate { get; set; }
        public string ExportedBy { get; set; }
        public string FilePath { get; set; }
        public int RecordCount { get; set; }
        public string Status { get; set; }
    }

    public class ScheduleExportRequest : ModelEntityBase
    {
        public string ExportType { get; set; }
        public string ExportFormat { get; set; }
        public string ScheduleFrequency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OutputPath { get; set; }
        public Dictionary<string, object>? ExportParameters { get; set; }
    }
}





