using Beep.OilandGas.PPDM39.Core;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Manages workflow version history and in-flight instance migration.
/// When a process definition changes, existing running instances can be migrated
/// to the new version with step remapping.
/// Part of Phase 2 workflow engine enhancement.
/// </summary>
public interface IWorkflowVersioningService
{
    /// <summary>
    /// Create a new version snapshot from a process definition.
    /// </summary>
    Task<WORKFLOW_VERSION> CreateVersionAsync(
        ProcessDefinition definition, string changeDescription, string userId);

    /// <summary>
    /// Get version history for a process definition.
    /// </summary>
    Task<List<WORKFLOW_VERSION>> GetVersionHistoryAsync(string processDefinitionId);

    /// <summary>
    /// Get the latest version of a process definition.
    /// </summary>
    Task<WORKFLOW_VERSION?> GetLatestVersionAsync(string processDefinitionId);

    /// <summary>
    /// Migrate an in-flight process instance to a new version.
    /// Maps completed steps from old version to new version using step remapping.
    /// </summary>
    Task<VersionMigrationResult> MigrateInstanceAsync(
        string processInstanceId, string targetVersionId, string userId);

    /// <summary>
    /// Get the current version a process instance is running against.
    /// </summary>
    Task<string?> GetInstanceVersionAsync(string processInstanceId);
}

public class VersionMigrationResult
{
    public bool Success { get; set; }
    public string? FromVersion { get; set; }
    public string? ToVersion { get; set; }
    public List<string> RemappedStepIds { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class WorkflowVersioningService : IWorkflowVersioningService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<WorkflowVersioningService> _logger;

    public WorkflowVersioningService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<WorkflowVersioningService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task<WORKFLOW_VERSION> CreateVersionAsync(
        ProcessDefinition definition, string changeDescription, string userId)
    {
        var existingVersions = await GetVersionHistoryAsync(definition.ProcessId);
        var latestVersion = existingVersions.OrderByDescending(v =>
            ParseVersion(v.VERSION_NUMBER)).FirstOrDefault();

        var newVersionNum = latestVersion is null
            ? "1.0"
            : IncrementVersion(latestVersion.VERSION_NUMBER, changeDescription);

        var snapshot = JsonSerializer.Serialize(definition);

        var version = new WORKFLOW_VERSION
        {
            PROCESS_DEFINITION_ID = definition.ProcessId,
            VERSION_NUMBER = newVersionNum,
            CHANGE_DESCRIPTION = changeDescription,
            PREVIOUS_VERSION_ID = latestVersion?.VERSION_ID,
            PROCESS_CONFIG_SNAPSHOT = snapshot,
            EFFECTIVE_DATE = DateTime.UtcNow,
            CREATED_BY = userId,
        };

        var repo = GetRepo();
        await repo.InsertAsync(version, userId);

        // Update the process definition's version field
        definition.Version = newVersionNum;

        _logger?.LogInformation(
            "Created workflow version {Version} for process {ProcessId}: {Description}",
            newVersionNum, definition.ProcessId, changeDescription);

        return version;
    }

    public async Task<List<WORKFLOW_VERSION>> GetVersionHistoryAsync(string processDefinitionId)
    {
        var repo = GetRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROCESS_DEFINITION_ID", FilterValue = processDefinitionId },
        };

        var results = await repo.GetAsync(filters);
        return results.OfType<WORKFLOW_VERSION>()
            .OrderByDescending(v => ParseVersion(v.VERSION_NUMBER))
            .ToList();
    }

    public async Task<WORKFLOW_VERSION?> GetLatestVersionAsync(string processDefinitionId)
    {
        var history = await GetVersionHistoryAsync(processDefinitionId);
        return history.FirstOrDefault();
    }

    public async Task<VersionMigrationResult> MigrateInstanceAsync(
        string processInstanceId, string targetVersionId, string userId)
    {
        var result = new VersionMigrationResult { Success = true };

        try
        {
            // Get the target version
            var repo = GetRepo();
            var versionFilters = new List<AppFilter>
            {
                new() { FieldName = "VERSION_ID", FilterValue = targetVersionId },
            };
            var versions = (await repo.GetAsync(versionFilters))
                .OfType<WORKFLOW_VERSION>().ToList();

            if (versions.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = $"Version {targetVersionId} not found.";
                return result;
            }

            var targetVersion = versions[0];
            result.ToVersion = targetVersion.VERSION_NUMBER;

            // Get current instance version from instance data
            var currentVersion = await GetInstanceVersionAsync(processInstanceId);
            result.FromVersion = currentVersion;

            // Parse step remapping
            var remapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(targetVersion.STEP_REMAPPING_JSON))
            {
                var mappingObj = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    targetVersion.STEP_REMAPPING_JSON);
                if (mappingObj is not null)
                {
                    foreach (var kvp in mappingObj)
                        remapping[kvp.Key] = kvp.Value;
                }
            }

            // Get instance steps
            var stepRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PROCESS_STEP_INSTANCE), _connectionName, "PROCESS_STEP_INSTANCE", null);

            var stepFilters = new List<AppFilter>
            {
                new() { FieldName = "PROCESS_INSTANCE_ID", FilterValue = processInstanceId },
            };

            var steps = (await stepRepo.GetAsync(stepFilters))
                .OfType<PROCESS_STEP_INSTANCE>().ToList();

            // Remap step IDs for completed steps
            foreach (var step in steps)
            {
                if (remapping.TryGetValue(step.STEP_ID, out var newStepId))
                {
                    var oldStepId = step.STEP_ID;
                    step.STEP_ID = newStepId;
                    await stepRepo.UpdateAsync(step, userId);
                    result.RemappedStepIds.Add($"{oldStepId} → {newStepId}");
                }
            }

            // Update instance to reference new version
            var instanceRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PROCESS_INSTANCE), _connectionName, "PROCESS_INSTANCE", null);

            var instanceFilters = new List<AppFilter>
            {
                new() { FieldName = "PROCESS_INSTANCE_ID", FilterValue = processInstanceId },
            };
            var instances = (await instanceRepo.GetAsync(instanceFilters))
                .OfType<PROCESS_INSTANCE>().ToList();

            if (instances.Count > 0)
            {
                var instance = instances[0];
                var instanceData = string.IsNullOrWhiteSpace(instance.INSTANCE_DATA_JSON)
                    ? new Dictionary<string, object>()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(instance.INSTANCE_DATA_JSON) ?? new();

                instanceData["migratedFromVersion"] = currentVersion ?? "unknown";
                instanceData["migratedToVersion"] = targetVersion.VERSION_NUMBER;
                instanceData["migratedAt"] = DateTime.UtcNow.ToString("O");
                instanceData["migratedBy"] = userId;
                instance.INSTANCE_DATA_JSON = JsonSerializer.Serialize(instanceData);

                await instanceRepo.UpdateAsync(instance, userId);
            }

            _logger?.LogInformation(
                "Migrated instance {InstanceId} from v{From} to v{To}: {RemappedCount} steps remapped",
                processInstanceId, currentVersion, targetVersion.VERSION_NUMBER, result.RemappedStepIds.Count);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger?.LogError(ex, "Failed to migrate instance {InstanceId}", processInstanceId);
        }

        return result;
    }

    public async Task<string?> GetInstanceVersionAsync(string processInstanceId)
    {
        var repo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROCESS_INSTANCE), _connectionName, "PROCESS_INSTANCE", null);

        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROCESS_INSTANCE_ID", FilterValue = processInstanceId },
        };

        var results = await repo.GetAsync(filters);
        var instance = results.OfType<PROCESS_INSTANCE>().FirstOrDefault();

        if (instance?.INSTANCE_DATA_JSON is null) return null;

        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(instance.INSTANCE_DATA_JSON);
            return data?.TryGetValue("workflowVersion", out var ver) == true ? ver.ToString() : null;
        }
        catch
        {
            return null;
        }
    }

    private PPDMGenericRepository GetRepo() =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(WORKFLOW_VERSION), _connectionName, "WORKFLOW_VERSION", null);

    private static Version ParseVersion(string version)
    {
        return Version.TryParse(version, out var v) ? v : new Version(0, 0);
    }

    private static string IncrementVersion(string currentVersion, string changeDescription)
    {
        var v = ParseVersion(currentVersion);
        // Major changes (breaking step changes) increment major; minor changes increment minor
        var isMajor = changeDescription.Contains("BREAKING", StringComparison.OrdinalIgnoreCase) ||
                      changeDescription.Contains("major", StringComparison.OrdinalIgnoreCase);
        return isMajor
            ? $"{v.Major + 1}.0"
            : $"{v.Major}.{v.Minor + 1}";
    }
}
