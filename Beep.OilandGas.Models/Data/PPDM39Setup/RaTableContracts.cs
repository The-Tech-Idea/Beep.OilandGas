using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>Metadata about a PPDM R_* (reference/association) table.</summary>
    public class RaTableInfo : ModelEntityBase
    {
        private string tableNameValue = string.Empty;
        public string TableName { get => tableNameValue; set => SetProperty(ref tableNameValue, value); }

        private string? descriptionValue;
        public string? Description { get => descriptionValue; set => SetProperty(ref descriptionValue, value); }

        private int rowCountValue;
        public int RowCount { get => rowCountValue; set => SetProperty(ref rowCountValue, value); }

        private string? categoryValue;
        public string? Category { get => categoryValue; set => SetProperty(ref categoryValue, value); }
    }

    /// <summary>A category bucket of RA tables for grouped display in the wizard.</summary>
    public class RaTableCategory : ModelEntityBase
    {
        private string categoryValue = string.Empty;
        public string Category { get => categoryValue; set => SetProperty(ref categoryValue, value); }

        private List<RaTableInfo> tablesValue = new();
        public List<RaTableInfo> Tables { get => tablesValue; set => SetProperty(ref tablesValue, value); }
    }

    /// <summary>Result of exporting RA table data to CSV or another interchange format.</summary>
    public class RaTableExportResult : ModelEntityBase
    {
        private bool successValue;
        public bool Success { get => successValue; set => SetProperty(ref successValue, value); }

        private string messageValue = string.Empty;
        public string Message { get => messageValue; set => SetProperty(ref messageValue, value); }

        private int tablesExportedValue;
        public int TablesExported { get => tablesExportedValue; set => SetProperty(ref tablesExportedValue, value); }

        private string? outputPathValue;
        public string? OutputPath { get => outputPathValue; set => SetProperty(ref outputPathValue, value); }

        private List<string> exportedTablesValue = new();
        public List<string> ExportedTables { get => exportedTablesValue; set => SetProperty(ref exportedTablesValue, value); }

        private List<string> errorsValue = new();
        public List<string> Errors { get => errorsValue; set => SetProperty(ref errorsValue, value); }
    }
}
