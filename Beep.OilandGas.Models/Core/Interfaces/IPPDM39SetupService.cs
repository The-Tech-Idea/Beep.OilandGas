using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for PPDM39 database setup operations
    /// Platform-agnostic interface for database setup, script execution, and connection management
    /// </summary>
    public interface IPPDM39SetupService
    {
        void SetProgressTracking(IProgressTrackingService progressTracking);
        DatabaseDriverInfo? GetDriverInfo(string databaseType);
        List<string> GetAvailableDatabaseTypes();
        DriverInfo CheckDriver(string databaseType);
        Task<SchemaPrivilegeCheckResult> CheckSchemaPrivilegesAsync(ConnectionConfig config, string? schemaName = null);
        Task<CreateSchemaResult> CreateSchemaAsync(ConnectionConfig config, string schemaName);
        Task<ConnectionTestResult> TestConnectionAsync(ConnectionConfig config);
        Task<List<ScriptInfo>> GetAvailableScriptsAsync(string databaseType);
        List<ScriptInfo> GetAvailableScripts(string databaseType);
        Task<ScriptExecutionResult> ExecuteScriptAsync(ConnectionConfig config, string scriptName, string? operationId = null);
        Task<Beep.OilandGas.Models.Data.AllScriptsExecutionResult> ExecuteAllScriptsAsync(ConnectionConfig config, List<string> scriptNames, string? operationId = null);
        List<DatabaseConnectionListItem> GetAllConnections();
        string? GetCurrentConnectionName();
        SetCurrentDatabaseResult SetCurrentConnection(string connectionName);
        SaveConnectionResult UpdateConnection(string originalConnectionName, ConnectionConfig config, bool testAfterSave = true);
        DeleteConnectionResult DeleteConnection(string connectionName);
        ConnectionConfig? GetConnectionByName(string connectionName);
        SaveConnectionResult SaveConnection(ConnectionConfig config, bool testAfterSave = true, bool openAfterSave = false);
        Task<DropDatabaseResult> DropDatabaseAsync(string connectionName, string? schemaName, bool dropIfExists = true);
        Task<RecreateDatabaseResult> RecreateDatabaseAsync(string connectionName, string? schemaName, bool backupFirst = false, string? backupPath = null);
        Task<CopyDatabaseResult> CopyDatabaseAsync(CopyDatabaseRequest request, string? operationId = null);

        // ── Reference Data Seeding ──────────────────────────────────────────
        /// <summary>
        /// Seeds WSC v3 well-status reference tables:
        ///   R_WELL_STATUS_TYPE, R_WELL_STATUS, R_WELL_STATUS_QUAL, R_WELL_STATUS_QUAL_VALUE.
        /// Idempotent — safe to call multiple times.
        /// </summary>
        Task<SeedingOperationResult> SeedWellStatusFacetsAsync(string connectionName, string userId = "SYSTEM", string? operationId = null);

        /// <summary>
        /// Seeds all enum-backed reference tables (R_WELL_CLASS, R_FLUID_TYPE, PPDM_UNIT_OF_MEASURE, etc.)
        /// using the <c>EnumReferenceDataSeeder</c>.
        /// </summary>
        Task<SeedingOperationResult> SeedEnumReferenceDataAsync(string connectionName, string userId = "SYSTEM", string? operationId = null);

        /// <summary>
        /// Runs both <see cref="SeedWellStatusFacetsAsync"/> and <see cref="SeedEnumReferenceDataAsync"/>
        /// in sequence and returns an aggregate result.
        /// </summary>
        Task<SeedingOperationResult> SeedAllReferenceDataAsync(string connectionName, string userId = "SYSTEM", string? operationId = null);
    }

    /// <summary>
    /// Result from a reference-data seeding operation.
    /// </summary>
    public class SeedingOperationResult
    {
        public bool         Success        { get; set; }
        public string       Message        { get; set; } = string.Empty;
        public int          TotalInserted  { get; set; }
        public List<string> Details        { get; set; } = new();
        public List<string> Errors         { get; set; } = new();
    }

    /// <summary>
    /// Database driver information
    /// </summary>
    

    /// <summary>
    /// Driver availability information
    /// </summary>
    

    /// <summary>
    /// Schema privilege check result
    /// </summary>
    

    /// <summary>
    /// Create schema result
    /// </summary>
    

    /// <summary>
    /// Database connection list item
    /// </summary>
    

    /// <summary>
    /// Delete connection result
    /// </summary>
    
}





