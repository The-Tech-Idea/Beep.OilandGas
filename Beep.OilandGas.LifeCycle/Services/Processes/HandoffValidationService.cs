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
/// Validates that all handoff contract requirements are met before a cross-role transition.
/// Checks required data fields, required documents, and validation rules.
/// Part of Phase 3 cross-role orchestration.
/// </summary>
public interface IHandoffValidationService
{
    /// <summary>
    /// Validate that a handoff from one role to another can proceed.
    /// Returns the validation result with any missing requirements.
    /// </summary>
    Task<HandoffValidationResult> ValidateHandoffAsync(
        string processInstanceId,
        string fromStepId,
        string toStepId,
        string entityType,
        string entityId,
        Dictionary<string, object> entityFields);

    /// <summary>
    /// Get the handoff contract between two steps.
    /// </summary>
    Task<ROLE_HANDOFF_CONTRACT?> GetHandoffContractAsync(
        string processDefinitionId, string fromStepId);

    /// <summary>
    /// Get all missing requirements for a handoff (for UI display).
    /// </summary>
    Task<List<string>> GetMissingRequirementsAsync(
        ROLE_HANDOFF_CONTRACT contract,
        Dictionary<string, object> entityFields);
}

public class HandoffValidationResult
{
    public bool IsValid { get; set; }
    public bool CanProceed { get; set; }
    public List<string> PassedChecks { get; set; } = new();
    public List<string> FailedChecks { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? BlockingReason { get; set; }
}

public class HandoffValidationService : IHandoffValidationService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<HandoffValidationService> _logger;

    public HandoffValidationService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<HandoffValidationService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task<HandoffValidationResult> ValidateHandoffAsync(
        string processInstanceId,
        string fromStepId,
        string toStepId,
        string entityType,
        string entityId,
        Dictionary<string, object> entityFields)
    {
        var result = new HandoffValidationResult { IsValid = true, CanProceed = true };

        // Get process instance to find the process definition
        var instanceRepo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROCESS_INSTANCE), _connectionName, "PROCESS_INSTANCE", null);

        var instanceFilters = new List<AppFilter>
        {
            new() { FieldName = "PROCESS_INSTANCE_ID", FilterValue = processInstanceId },
        };
        var instances = (await instanceRepo.GetAsync(instanceFilters))
            .OfType<PROCESS_INSTANCE>().ToList();

        if (instances.Count == 0)
        {
            result.IsValid = false;
            result.CanProceed = false;
            result.BlockingReason = $"Process instance {processInstanceId} not found.";
            return result;
        }

        var contract = await GetHandoffContractAsync(
            instances[0].PROCESS_DEFINITION_ID, fromStepId);

        if (contract is null)
        {
            result.PassedChecks.Add("No handoff contract — default transition allowed");
            return result;
        }

        // 1. Validate required data fields
        if (!string.IsNullOrWhiteSpace(contract.REQUIRED_DATA_FIELDS_JSON))
        {
            try
            {
                var requiredFields = JsonSerializer.Deserialize<List<string>>(contract.REQUIRED_DATA_FIELDS_JSON);
                if (requiredFields is not null)
                {
                    foreach (var field in requiredFields)
                    {
                        if (entityFields.TryGetValue(field, out var value) && value is not null)
                        {
                            result.PassedChecks.Add($"Field '{field}' is populated");
                        }
                        else
                        {
                            result.FailedChecks.Add($"Required field '{field}' is missing or empty");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to parse REQUIRED_DATA_FIELDS_JSON for contract {ContractId}",
                    contract.HANDOFF_CONTRACT_ID);
            }
        }

        // 2. Validate required documents
        if (!string.IsNullOrWhiteSpace(contract.REQUIRED_DOCUMENTS_JSON))
        {
            try
            {
                var requiredDocs = JsonSerializer.Deserialize<List<string>>(contract.REQUIRED_DOCUMENTS_JSON);
                if (requiredDocs is not null)
                {
                    foreach (var doc in requiredDocs)
                    {
                        result.Warnings.Add($"Document '{doc}' may be required — verify manually");
                    }
                }
            }
            catch { /* Non-blocking */ }
        }

        // 3. Evaluate validation rules
        if (!string.IsNullOrWhiteSpace(contract.VALIDATION_RULES_JSON))
        {
            try
            {
                var rules = JsonSerializer.Deserialize<List<string>>(contract.VALIDATION_RULES_JSON);
                if (rules is not null)
                {
                    foreach (var rule in rules)
                    {
                        if (rule.Contains(">") || rule.Contains("<") || rule.Contains("=="))
                        {
                            var routingService = new DynamicRoutingService();
                            if (routingService.EvaluateCondition(rule, entityFields))
                            {
                                result.PassedChecks.Add($"Rule passed: {rule}");
                            }
                            else
                            {
                                result.FailedChecks.Add($"Rule failed: {rule}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to evaluate validation rules for contract {ContractId}",
                    contract.HANDOFF_CONTRACT_ID);
            }
        }

        result.IsValid = result.FailedChecks.Count == 0;
        result.CanProceed = result.IsValid;
        if (!result.IsValid)
        {
            result.BlockingReason = $"Handoff validation failed: {string.Join("; ", result.FailedChecks)}";
        }

        _logger?.LogInformation(
            "Handoff validation for process {ProcessId}, {FromStep}→{ToStep}: Valid={IsValid}, Passed={PassedCount}, Failed={FailedCount}",
            processInstanceId, fromStepId, toStepId, result.IsValid, result.PassedChecks.Count, result.FailedChecks.Count);

        return result;
    }

    public async Task<ROLE_HANDOFF_CONTRACT?> GetHandoffContractAsync(
        string processDefinitionId, string fromStepId)
    {
        var repo = GetContractRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROCESS_DEFINITION_ID", FilterValue = processDefinitionId },
            new() { FieldName = "FROM_STEP_ID", FilterValue = fromStepId },
            new() { FieldName = "ACTIVE_IND", FilterValue = "Y" },
        };
        var results = await repo.GetAsync(filters);
        return results.OfType<ROLE_HANDOFF_CONTRACT>().FirstOrDefault();
    }

    public async Task<List<string>> GetMissingRequirementsAsync(
        ROLE_HANDOFF_CONTRACT contract,
        Dictionary<string, object> entityFields)
    {
        var missing = new List<string>();

        if (!string.IsNullOrWhiteSpace(contract.REQUIRED_DATA_FIELDS_JSON))
        {
            try
            {
                var fields = JsonSerializer.Deserialize<List<string>>(contract.REQUIRED_DATA_FIELDS_JSON);
                if (fields is not null)
                {
                    missing.AddRange(fields
                        .Where(f => !entityFields.ContainsKey(f) || entityFields[f] is null)
                        .Select(f => $"Missing data: {f}"));
                }
            }
            catch { }
        }

        return missing;
    }

    private PPDMGenericRepository GetContractRepo() =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(ROLE_HANDOFF_CONTRACT), _connectionName, "ROLE_HANDOFF_CONTRACT", null);
}
