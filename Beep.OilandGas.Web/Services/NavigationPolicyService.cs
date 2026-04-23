using System.Text.Json;
using Beep.OilandGas.UserManagement.Models.Profile;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Service for managing role-aware and persona-aware navigation policies.
/// Determines which workflows and routes are accessible to the current user based on their persona and roles.
/// </summary>
public interface INavigationPolicyService
{
    /// <summary>
    /// Check if a given workflow/route is allowed for the user's current persona.
    /// </summary>
    bool IsWorkflowAllowed(PersonaDefinition? persona, string workflowKey);

    /// <summary>
    /// Parse the ALLOWED_WORKFLOWS_JSON from a persona and return the list of allowed workflow keys.
    /// </summary>
    List<string> GetAllowedWorkflows(PersonaDefinition? persona);

    /// <summary>
    /// Check if a user can access a specific route based on their persona's allowed workflows.
    /// </summary>
    bool CanAccessRoute(PersonaDefinition? persona, string routePath);
}

/// <summary>
/// Implementation of navigation policy service.
/// Manages workflow and route access based on persona definitions.
/// </summary>
public class NavigationPolicyService : INavigationPolicyService
{
    private static readonly Dictionary<string, string> CoreRouteToWorkflow = new(StringComparer.OrdinalIgnoreCase)
    {
        ["exploration"] = "exploration",
        ["development"] = "development",
        ["production"] = "production",
        ["reservoir"] = "reservoir",
        ["economics"] = "economics",
        ["accounting"] = "economics",
        ["hse"] = "hse",
        ["decommissioning"] = "hse",
        ["ppdm39/data-management"] = "data",
        ["ppdm39/setup"] = "data",
        ["ppdm39/process"] = "processes",
        ["ppdm39/compliance"] = "hse"
    };

    public bool IsWorkflowAllowed(PersonaDefinition? persona, string workflowKey)
    {
        if (persona == null || string.IsNullOrEmpty(workflowKey))
            return false;

        var allowedWorkflows = GetAllowedWorkflows(persona);
        return allowedWorkflows.Contains(workflowKey, StringComparer.OrdinalIgnoreCase);
    }

    public List<string> GetAllowedWorkflows(PersonaDefinition? persona)
    {
        var workflows = new List<string>();

        if (persona == null || string.IsNullOrEmpty(persona.ALLOWED_WORKFLOWS_JSON))
            return workflows;

        try
        {
            // Parse JSON array of workflow codes
            using (var doc = JsonDocument.Parse(persona.ALLOWED_WORKFLOWS_JSON))
            {
                var root = doc.RootElement;
                
                // Handle both string array and object array formats
                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in root.EnumerateArray())
                    {
                        if (element.ValueKind == JsonValueKind.String)
                        {
                            var workflow = element.GetString();
                            if (!string.IsNullOrEmpty(workflow))
                                workflows.Add(workflow);
                        }
                        else if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("workflow", out var workflowProp))
                        {
                            var workflow = workflowProp.GetString();
                            if (!string.IsNullOrEmpty(workflow))
                                workflows.Add(workflow);
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            // If JSON parsing fails, return empty list
            return workflows;
        }

        return workflows;
    }

    public bool CanAccessRoute(PersonaDefinition? persona, string routePath)
    {
        if (persona == null || string.IsNullOrEmpty(routePath))
            return true;

        // Normalize route path (remove leading slash, lowercase)
        var normalizedPath = routePath.TrimStart('/').ToLowerInvariant();

        if (string.IsNullOrEmpty(normalizedPath) || normalizedPath == "landing" || normalizedPath == "dashboard")
        {
            return true;
        }

        var allowedWorkflows = GetAllowedWorkflows(persona);

        if (allowedWorkflows.Count == 0)
        {
            return true;
        }

        foreach (var entry in CoreRouteToWorkflow)
        {
            if (normalizedPath.StartsWith(entry.Key, StringComparison.OrdinalIgnoreCase))
            {
                return allowedWorkflows.Contains(entry.Value, StringComparer.OrdinalIgnoreCase);
            }
        }

        // Check if any allowed workflow matches the route
        // Routes map to workflows by convention: "/exploration" -> "exploration", "/development" -> "development", etc.
        return allowedWorkflows.Any(workflow =>
            normalizedPath.StartsWith(workflow.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));
    }
}
