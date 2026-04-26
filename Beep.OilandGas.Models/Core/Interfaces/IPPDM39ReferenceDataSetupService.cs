using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Manages reference data seeding: LOV import, R_* table seeding, well-status facets,
    /// enum reference data, module-scoped seeding, and RA table extraction.
    /// All seeding operations are idempotent.
    /// </summary>
    public interface IPPDM39ReferenceDataSetupService
    {
        // ── Seeding entry points ───────────────────────────────────────────
        Task<SeedingOperationResult> SeedAllReferenceDataAsync(string connectionName, string userId = "SYSTEM", string? operationId = null);
        Task<SeedingOperationResult> SeedCategoryAsync(string connectionName, string category, string userId = "SYSTEM", string? operationId = null);
        Task<SeedingOperationResult> SeedWellStatusFacetsAsync(string connectionName, string userId = "SYSTEM", string? operationId = null);
        Task<SeedingOperationResult> SeedEnumReferenceDataAsync(string connectionName, string userId = "SYSTEM", string? operationId = null);
        Task<SeedingOperationResult> SeedSelectedModulesAsync(string connectionName, List<string> modules, string userId = "SYSTEM", string? operationId = null);
        Task<SeedingOperationResult> RunSeedWorkflowAsync(string connectionName, string workflowName, string userId = "SYSTEM", string? operationId = null);

        // ── Seed catalog and module discovery ──────────────────────────────
        List<string> GetSeedCategories();
        List<string> GetAvailableModules();
        Task<SeedingValidationResult> ValidateSeedDataAsync(string connectionName);

        // ── LOV management ─────────────────────────────────────────────────
        Task<List<LovValueItem>> GetLovByTypeAsync(string valueType);
        Task<List<LovValueItem>> GetLovByCategoryAsync(string category);
        Task<List<LovValueItem>> GetLovByModuleAsync(string module);
        Task<List<LovValueItem>> GetLovBySourceAsync(string source);
        Task<List<LovHierarchyItem>> GetHierarchicalLovAsync(string valueType);
        Task<List<LovValueItem>> SearchLovAsync(string query, string? valueType = null);
        List<string> GetLovTypes();
        Task<LovValueItem> CreateLovEntryAsync(LovCreateRequest request, string userId);
        Task<LovValueItem> UpdateLovEntryAsync(string id, LovUpdateRequest request, string userId);
        Task<bool> DeleteLovEntryAsync(string id, string userId);
        Task<SeedingOperationResult> ImportLovAsync(LOVImportRequest request, string userId);

        // ── RA table extraction ────────────────────────────────────────────
        Task<List<RaTableInfo>> ExtractRaTablesAsync(string connectionName);
        Task<List<RaTableCategory>> GetCategorizedRaTablesAsync(string connectionName);
        Task<RaTableExportResult> ExportRaTablesAsync(string connectionName, List<string>? tableNames = null);
    }
}
