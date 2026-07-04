using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.Models.Processes;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Resolves the next step in a workflow dynamically based on entity attributes
/// rather than a static step sequence. Supports conditional branching expressions.
/// Part of Phase 2 workflow engine enhancement.
/// </summary>
public interface IDynamicRoutingService
{
    /// <summary>
    /// Given a completed step's conditional next steps, evaluate each condition
    /// against the entity context and return the first matching step ID.
    /// Falls back to the static NextStepId if no conditions match.
    /// </summary>
    Task<string?> ResolveNextStepAsync(
        ProcessStepDefinition completedStep,
        Dictionary<string, object> entityContext);

    /// <summary>
    /// Evaluate a single condition expression against the context.
    /// Supported operators: >, <, >=, <=, ==, !=, IN, CONTAINS, IS_NULL, IS_NOT_NULL.
    /// </summary>
    bool EvaluateCondition(string expression, Dictionary<string, object> context);

    /// <summary>
    /// Extract numeric field values from an entity for DoA evaluation context.
    /// </summary>
    Dictionary<string, object> ExtractNumericFields(object? entity);
}

public class DynamicRoutingService : IDynamicRoutingService
{
    private readonly ILogger<DynamicRoutingService> _logger;

    public DynamicRoutingService(ILogger<DynamicRoutingService>? logger = null)
    {
        _logger = logger;
    }

    public Task<string?> ResolveNextStepAsync(
        ProcessStepDefinition completedStep,
        Dictionary<string, object> entityContext)
    {
        // Check ConditionalNextSteps from step configuration
        if (completedStep.ConditionalNextSteps is { Count: > 0 })
        {
            foreach (var conditionalStep in completedStep.ConditionalNextSteps)
            {
                // ConditionalNextSteps items are expected in format: "condition_expression|step_id"
                // or as separate properties in StepConfiguration
                var parts = conditionalStep.Split('|', 2);
                if (parts.Length == 2)
                {
                    var condition = parts[0].Trim();
                    var stepId = parts[1].Trim();

                    if (EvaluateCondition(condition, entityContext))
                    {
                        _logger?.LogDebug("Condition '{Condition}' matched → routing to step {StepId}", condition, stepId);
                        return Task.FromResult<string?>(stepId);
                    }
                }
            }
        }

        // Check StepConfiguration for structured conditional routing
        if (completedStep.StepConfiguration != null &&
            completedStep.StepConfiguration.TryGetValue("conditionalNextSteps", out var condStepsObj) &&
            condStepsObj is JsonElement condStepsEl &&
            condStepsEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in condStepsEl.EnumerateArray())
            {
                if (item.TryGetProperty("condition", out var condProp) &&
                    item.TryGetProperty("nextStepId", out var stepProp))
                {
                    if (EvaluateCondition(condProp.GetString() ?? string.Empty, entityContext))
                    {
                        var stepId = stepProp.GetString();
                        _logger?.LogDebug("Structured condition matched → routing to step {StepId}", stepId);
                        return Task.FromResult(stepId);
                    }
                }
            }
        }

        // Fall back to static next step
        return Task.FromResult(completedStep.NextStepId);
    }

    public bool EvaluateCondition(string expression, Dictionary<string, object> context)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return true; // Empty condition = always true

        try
        {
            expression = expression.Trim();

            // Handle IS_NULL / IS_NOT_NULL
            if (expression.EndsWith("IS_NULL", StringComparison.OrdinalIgnoreCase))
            {
                var field = expression[..^7].Trim();
                return !context.ContainsKey(field) || context[field] is null;
            }
            if (expression.EndsWith("IS_NOT_NULL", StringComparison.OrdinalIgnoreCase))
            {
                var field = expression[..^11].Trim();
                return context.ContainsKey(field) && context[field] is not null;
            }

            // Handle CONTAINS
            var containsIdx = expression.IndexOf("CONTAINS", StringComparison.OrdinalIgnoreCase);
            if (containsIdx >= 0)
            {
                var field = expression[..containsIdx].Trim();
                var value = expression[(containsIdx + 8)..].Trim().Trim('\'', '"');
                return context.TryGetValue(field, out var ctxVal) &&
                       ctxVal?.ToString()?.Contains(value, StringComparison.OrdinalIgnoreCase) == true;
            }

            // Handle IN (value IN (a,b,c))
            var inIdx = expression.IndexOf(" IN ", StringComparison.OrdinalIgnoreCase);
            if (inIdx >= 0)
            {
                var field = expression[..inIdx].Trim();
                var listPart = expression[(inIdx + 4)..].Trim().Trim('(', ')');
                var values = listPart.Split(',').Select(v => v.Trim().Trim('\'', '"')).ToHashSet();
                return context.TryGetValue(field, out var ctxVal) &&
                       values.Contains(ctxVal?.ToString() ?? string.Empty);
            }

            // Handle comparison operators: >, <, >=, <=, ==, !=
            string[] operators = { ">=", "<=", "!=", "==", ">", "<" };
            foreach (var op in operators)
            {
                var opIdx = expression.IndexOf(op, StringComparison.Ordinal);
                if (opIdx < 0) continue;

                var field = expression[..opIdx].Trim();
                var valueStr = expression[(opIdx + op.Length)..].Trim();

                if (!context.TryGetValue(field, out var ctxVal) || ctxVal is null)
                    return false;

                var ctxDecimal = ConvertToDecimal(ctxVal);
                var cmpDecimal = ConvertToDecimal(ParseValue(valueStr));

                if (ctxDecimal.HasValue && cmpDecimal.HasValue)
                {
                    return op switch
                    {
                        ">" => ctxDecimal > cmpDecimal,
                        "<" => ctxDecimal < cmpDecimal,
                        ">=" => ctxDecimal >= cmpDecimal,
                        "<=" => ctxDecimal <= cmpDecimal,
                        "==" => ctxDecimal == cmpDecimal,
                        "!=" => ctxDecimal != cmpDecimal,
                        _ => false,
                    };
                }

                // String comparison for non-numeric values
                var ctxStr = ctxVal.ToString();
                var cmpStr = valueStr.Trim('\'', '"');
                return op switch
                {
                    "==" => string.Equals(ctxStr, cmpStr, StringComparison.OrdinalIgnoreCase),
                    "!=" => !string.Equals(ctxStr, cmpStr, StringComparison.OrdinalIgnoreCase),
                    _ => false,
                };
            }

            _logger?.LogWarning("Unrecognized condition expression: {Expression}", expression);
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to evaluate condition: {Expression}", expression);
            return false;
        }
    }

    public Dictionary<string, object> ExtractNumericFields(object? entity)
    {
        var fields = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (entity is null) return fields;

        foreach (var prop in entity.GetType().GetProperties())
        {
            var value = prop.GetValue(entity);
            if (value is not null)
            {
                fields[prop.Name] = value;
            }
        }

        return fields;
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
                string s when decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
                JsonElement je when je.ValueKind == JsonValueKind.Number => je.GetDecimal(),
                _ => null,
            };
        }
        catch { return null; }
    }

    private static object ParseValue(string str)
    {
        if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
            return d;
        return str.Trim('\'', '"');
    }
}
