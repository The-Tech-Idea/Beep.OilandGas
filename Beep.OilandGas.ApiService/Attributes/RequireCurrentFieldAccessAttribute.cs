using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Beep.OilandGas.ApiService.Attributes;

/// <summary>
/// Enforces ABAC-style field scoping by validating user access to the active field context.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireCurrentFieldAccessAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string? _requiredPermission;

    public RequireCurrentFieldAccessAttribute(string? requiredPermission = null)
    {
        _requiredPermission = requiredPermission;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var principal = context.HttpContext.User;
        var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal?.Identity?.Name;

        var observability = context.HttpContext.RequestServices.GetService(typeof(IAuthorizationObservabilityService))
            as IAuthorizationObservabilityService;
        var correlationId = context.HttpContext.TraceIdentifier;
        var endpoint = context.HttpContext.Request.Path.ToString();
        var method = context.HttpContext.Request.Method;
        var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrWhiteSpace(userId))
        {
            context.Result = new UnauthorizedResult();
            await EmitObservationAsync(observability, new AuthorizationObservation
            {
                PolicyName = nameof(RequireCurrentFieldAccessAttribute),
                UserId = null,
                AssetType = "FIELD",
                RequiredPermission = _requiredPermission,
                Decision = "Denied",
                Reason = "Missing user identity",
                Endpoint = endpoint,
                HttpMethod = method,
                CorrelationId = correlationId,
                ClientIp = clientIp
            });
            return;
        }

        var fieldOrchestrator = context.HttpContext.RequestServices.GetService(typeof(IFieldOrchestrator)) as IFieldOrchestrator;
        var accessControlService = context.HttpContext.RequestServices.GetService(typeof(IAccessControlService)) as IAccessControlService;

        if (fieldOrchestrator == null || accessControlService == null)
        {
            context.Result = new StatusCodeResult(500);
            await EmitObservationAsync(observability, new AuthorizationObservation
            {
                PolicyName = nameof(RequireCurrentFieldAccessAttribute),
                UserId = userId,
                AssetType = "FIELD",
                RequiredPermission = _requiredPermission,
                Decision = "Error",
                Reason = "Authorization services unavailable",
                Endpoint = endpoint,
                HttpMethod = method,
                CorrelationId = correlationId,
                ClientIp = clientIp
            });
            return;
        }

        var fieldId = fieldOrchestrator.CurrentFieldId;
        if (string.IsNullOrWhiteSpace(fieldId))
        {
            context.Result = new BadRequestObjectResult(new { error = "No active field selected." });
            await EmitObservationAsync(observability, new AuthorizationObservation
            {
                PolicyName = nameof(RequireCurrentFieldAccessAttribute),
                UserId = userId,
                AssetType = "FIELD",
                RequiredPermission = _requiredPermission,
                Decision = "Denied",
                Reason = "No active field selected",
                Endpoint = endpoint,
                HttpMethod = method,
                CorrelationId = correlationId,
                ClientIp = clientIp
            });
            return;
        }

        try
        {
            var accessCheck = await accessControlService.CheckAssetAccessAsync(userId, fieldId, "FIELD", _requiredPermission);
            if (!accessCheck.HasAccess)
            {
                context.Result = new ForbidResult();
                await EmitObservationAsync(observability, new AuthorizationObservation
                {
                    PolicyName = nameof(RequireCurrentFieldAccessAttribute),
                    UserId = userId,
                    AssetId = fieldId,
                    AssetType = "FIELD",
                    RequiredPermission = _requiredPermission,
                    Decision = "Denied",
                    Reason = accessCheck.Reason ?? "Access denied",
                    Endpoint = endpoint,
                    HttpMethod = method,
                    CorrelationId = correlationId,
                    ClientIp = clientIp
                });
                return;
            }

            await EmitObservationAsync(observability, new AuthorizationObservation
            {
                PolicyName = nameof(RequireCurrentFieldAccessAttribute),
                UserId = userId,
                AssetId = fieldId,
                AssetType = "FIELD",
                RequiredPermission = _requiredPermission,
                Decision = "Allowed",
                Reason = accessCheck.Reason ?? "Access granted",
                Endpoint = endpoint,
                HttpMethod = method,
                CorrelationId = correlationId,
                ClientIp = clientIp
            });
        }
        catch (Exception ex)
        {
            context.Result = new StatusCodeResult(500);
            await EmitObservationAsync(observability, new AuthorizationObservation
            {
                PolicyName = nameof(RequireCurrentFieldAccessAttribute),
                UserId = userId,
                AssetId = fieldId,
                AssetType = "FIELD",
                RequiredPermission = _requiredPermission,
                Decision = "Error",
                Reason = ex.Message,
                Endpoint = endpoint,
                HttpMethod = method,
                CorrelationId = correlationId,
                ClientIp = clientIp
            });
        }
    }

    private static Task EmitObservationAsync(
        IAuthorizationObservabilityService? observability,
        AuthorizationObservation observation)
    {
        return observability == null
            ? Task.CompletedTask
            : observability.RecordPolicyEvaluationAsync(observation);
    }
}
