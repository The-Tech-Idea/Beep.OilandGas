using Beep.OilandGas.PPDM39.Core;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Evaluates Delegation of Authority thresholds against entity field values.
/// Given an entity (e.g., an AFE with ESTIMATED_COST = $750,000),
/// determines which approval levels are required.
/// </summary>
public interface IDoAEvaluationService
{
    /// <summary>
    /// Evaluate all DOA thresholds for a given entity and return the required approval levels
    /// in sequence order (LEVEL_1 first, then LEVEL_2, etc.).
    /// </summary>
    Task<List<DoaApprovalLevel>> EvaluateThresholdsAsync(
        string entityType,
        Dictionary<string, object> entityFields,
        string? processType = null);

    /// <summary>
    /// Get the escalation path for a given DOA rule.
    /// </summary>
    Task<DoaEscalationPath?> GetEscalationPathAsync(string doaId, string approvalLevel);

    /// <summary>
    /// Get all active DOA rules for an entity type.
    /// </summary>
    Task<List<DELEGATION_OF_AUTHORITY>> GetRulesForEntityAsync(string entityType, string? processType = null);
}

public class DoaApprovalLevel
{
    public string Level { get; set; } = string.Empty;
    public string RequiredRole { get; set; } = string.Empty;
    public decimal ThresholdValue { get; set; }
    public string ComparisonOperator { get; set; } = string.Empty;
    public bool RequiresUnanimous { get; set; }
    public int ApprovalSequence { get; set; }
    public string? EscalationRole { get; set; }
    public int? EscalationHours { get; set; }
    public string DoaId { get; set; } = string.Empty;
}

public class DoaEscalationPath
{
    public string PrimaryRole { get; set; } = string.Empty;
    public string? EscalationRole { get; set; }
    public int? EscalationHours { get; set; }
}

public class DoAEvaluationService : IDoAEvaluationService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<DoAEvaluationService> _logger;

    public DoAEvaluationService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<DoAEvaluationService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task<List<DoaApprovalLevel>> EvaluateThresholdsAsync(
        string entityType,
        Dictionary<string, object> entityFields,
        string? processType = null)
    {
        var rules = await GetRulesForEntityAsync(entityType, processType);
        var triggeredLevels = new List<DoaApprovalLevel>();

        foreach (var rule in rules.OrderBy(r => r.APPROVAL_SEQUENCE))
        {
            if (!entityFields.TryGetValue(rule.FIELD_NAME, out var fieldValue))
                continue;

            if (fieldValue is null)
                continue;

            var decimalValue = ConvertToDecimal(fieldValue);
            if (decimalValue is null)
                continue;

            if (EvaluateThreshold(decimalValue.Value, rule))
            {
                triggeredLevels.Add(new DoaApprovalLevel
                {
                    Level = rule.APPROVAL_LEVEL,
                    RequiredRole = rule.REQUIRED_ROLE,
                    ThresholdValue = rule.THRESHOLD_VALUE,
                    ComparisonOperator = rule.COMPARISON_OPERATOR,
                    RequiresUnanimous = string.Equals(rule.REQUIRES_UNANIMOUS, "Y", StringComparison.OrdinalIgnoreCase),
                    ApprovalSequence = rule.APPROVAL_SEQUENCE,
                    EscalationRole = rule.ESCALATION_ROLE,
                    EscalationHours = rule.ESCALATION_HOURS,
                    DoaId = rule.DOA_ID,
                });
            }
        }

        _logger?.LogDebug(
            "DOA evaluation for {EntityType}: {TriggeredCount} levels triggered out of {TotalRules} rules",
            entityType, triggeredLevels.Count, rules.Count);

        return triggeredLevels.OrderBy(l => l.ApprovalSequence).ToList();
    }

    public async Task<DoaEscalationPath?> GetEscalationPathAsync(string doaId, string approvalLevel)
    {
        var repo = GetRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "DOA_ID", FilterValue = doaId },
            new() { FieldName = "APPROVAL_LEVEL", FilterValue = approvalLevel },
        };

        var results = await repo.GetAsync(filters);
        var rule = results.OfType<DELEGATION_OF_AUTHORITY>().FirstOrDefault();

        if (rule?.ESCALATION_ROLE is null)
            return null;

        return new DoaEscalationPath
        {
            PrimaryRole = rule.REQUIRED_ROLE,
            EscalationRole = rule.ESCALATION_ROLE,
            EscalationHours = rule.ESCALATION_HOURS,
        };
    }

    public async Task<List<DELEGATION_OF_AUTHORITY>> GetRulesForEntityAsync(
        string entityType, string? processType = null)
    {
        var repo = GetRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "ENTITY_TYPE", FilterValue = entityType },
            new() { FieldName = "ACTIVE_IND", FilterValue = "Y" },
        };

        var results = await repo.GetAsync(filters);
        var rules = results.OfType<DELEGATION_OF_AUTHORITY>().ToList();

        if (!string.IsNullOrWhiteSpace(processType))
        {
            rules = rules.Where(r =>
                string.IsNullOrWhiteSpace(r.PROCESS_TYPE) ||
                string.Equals(r.PROCESS_TYPE, processType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return rules;
    }

    private static bool EvaluateThreshold(decimal fieldValue, DELEGATION_OF_AUTHORITY rule)
    {
        return rule.COMPARISON_OPERATOR?.ToUpperInvariant() switch
        {
            "GREATER_THAN" => fieldValue > rule.THRESHOLD_VALUE,
            "GREATER_THAN_OR_EQUAL" => fieldValue >= rule.THRESHOLD_VALUE,
            "LESS_THAN" => fieldValue < rule.THRESHOLD_VALUE,
            "LESS_THAN_OR_EQUAL" => fieldValue <= rule.THRESHOLD_VALUE,
            "BETWEEN" => rule.THRESHOLD_VALUE_MAX.HasValue &&
                         fieldValue >= rule.THRESHOLD_VALUE &&
                         fieldValue <= rule.THRESHOLD_VALUE_MAX.Value,
            "EQUAL" => fieldValue == rule.THRESHOLD_VALUE,
            _ => false,
        };
    }

    private static decimal? ConvertToDecimal(object value)
    {
        try
        {
            return value switch
            {
                decimal d => d,
                int i => i,
                long l => l,
                double dbl => (decimal)dbl,
                float f => (decimal)f,
                string s when decimal.TryParse(s, out var parsed) => parsed,
                JsonElement je when je.ValueKind == JsonValueKind.Number => je.GetDecimal(),
                _ => null,
            };
        }
        catch
        {
            return null;
        }
    }

    private PPDMGenericRepository GetRepo()
    {
        return new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(DELEGATION_OF_AUTHORITY),
            _connectionName, "DELEGATION_OF_AUTHORITY", null);
    }
}
