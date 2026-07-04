using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.UserManagement.Security;

/// <summary>
/// Requirement that enforces field-level data access scoping.
/// The user must either have global scope ("*") or be assigned to the requested field.
/// </summary>
public class FieldScopeRequirement : IAuthorizationRequirement
{
    /// <summary>The field being requested.</summary>
    public string? FieldId { get; }

    /// <summary>The permission being checked (for audit context).</summary>
    public string? PermissionCode { get; }

    public FieldScopeRequirement(string? fieldId = null, string? permissionCode = null)
    {
        FieldId = fieldId;
        PermissionCode = permissionCode;
    }
}

/// <summary>
/// Authorization handler that enforces field-level scoping.
/// Reads the "field_scope" claim from the JWT and compares against the requested field.
/// A claim value of "*" or "GLOBAL" grants access to all fields.
/// </summary>
public class FieldScopeAuthorizationHandler : AuthorizationHandler<FieldScopeRequirement>
{
    private readonly ILogger<FieldScopeAuthorizationHandler> _logger;

    public FieldScopeAuthorizationHandler(ILogger<FieldScopeAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        FieldScopeRequirement requirement)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        if (requirement is null) throw new ArgumentNullException(nameof(requirement));

        // No specific field requested — allow (the caller will filter at the data layer)
        if (string.IsNullOrWhiteSpace(requirement.FieldId))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Read field_scope claim (comma-separated FIELD_IDs or "*" for global)
        var fieldScopeClaim = context.User.FindFirst("field_scope");
        if (fieldScopeClaim is null)
        {
            _logger.LogWarning(
                "FieldScope check failed: no 'field_scope' claim present for user. RequiredField={FieldId}",
                requirement.FieldId);
            // No claim means no field access — deny by default
            return Task.CompletedTask;
        }

        var scopes = fieldScopeClaim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Global scope — access to all fields
        if (scopes.Contains("*") || scopes.Contains("GLOBAL", StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if the user has the specific field in their scope
        if (scopes.Contains(requirement.FieldId, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        _logger.LogWarning(
            "FieldScope check denied: user scope={UserScopes}, required={FieldId}, permission={Permission}",
            fieldScopeClaim.Value, requirement.FieldId, requirement.PermissionCode);

        return Task.CompletedTask;
    }
}
