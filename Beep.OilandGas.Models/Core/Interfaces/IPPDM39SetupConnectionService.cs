using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Manages database connection lifecycle: save, update, delete, test, driver resolution,
    /// and connection enumeration. Owns the canonical save/load/mask/open/close path
    /// so controllers and wizard steps do not duplicate BeepDM connection handling.
    /// </summary>
    public interface IPPDM39SetupConnectionService
    {
        // ── Driver resolution ──────────────────────────────────────────────
        List<string> GetAvailableDatabaseTypes();
        DatabaseDriverInfo? GetDriverInfo(string databaseType);
        DriverInfo CheckDriver(string databaseType);
        bool InstallDriver(string databaseType);

        // ── Connection CRUD ────────────────────────────────────────────────
        List<DatabaseConnectionListItem> GetAllConnections();
        ConnectionConfig? GetConnectionByName(string connectionName);
        string? GetCurrentConnectionName();
        SetCurrentDatabaseResult SetCurrentConnection(string connectionName);
        SaveConnectionResult SaveConnection(ConnectionConfig config, bool testAfterSave = true, bool openAfterSave = false);
        SaveConnectionResult UpdateConnection(string originalConnectionName, ConnectionConfig config, bool testAfterSave = true);
        DeleteConnectionResult DeleteConnection(string connectionName);

        // ── Connection testing ─────────────────────────────────────────────
        Task<ConnectionTestResult> TestConnectionAsync(ConnectionConfig config);

        // ── Provider capability ────────────────────────────────────────────
        ProviderCapabilityInfo GetProviderCapabilities(string databaseType);
    }
}
