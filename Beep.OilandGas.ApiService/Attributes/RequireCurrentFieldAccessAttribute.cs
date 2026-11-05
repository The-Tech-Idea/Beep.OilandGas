using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Attributes;

/// <summary>Restores the existing reviewed preference and independently enforces current application FIELD access.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireCurrentFieldAccessAttribute(string? requiredPermission = null, bool requireWrite = false) : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var http = context.HttpContext; var services = http.RequestServices;
        var observations = services.GetService<IAuthorizationObservabilityService>();
        string? actor = null; string? field = null;
        IFieldOrchestrator? restored = null;
        async Task Observe(string decision, string reason) => await (observations?.RecordPolicyEvaluationAsync(new AuthorizationObservation {
            PolicyName = nameof(RequireCurrentFieldAccessAttribute), UserId = actor, AssetType = "FIELD", AssetId = field,
            RequiredPermission = requiredPermission, Decision = decision, Reason = reason, Endpoint = http.Request.Path,
            HttpMethod = http.Request.Method, CorrelationId = http.TraceIdentifier, ClientIp = http.Connection.RemoteIpAddress?.ToString()
        }) ?? Task.CompletedTask);
        if (http.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult(); await Observe("Denied", "An authenticated application account is required."); return;
        }
        var account = services.GetService<RepositoryRequestAccount>(); var company = services.GetService<CompanyModuleConnection>();
        var selection = services.GetService<ICompanyFieldSelectionService>(); var access = services.GetService<IAccessControlService>();
        if (account is null || company is null || selection is null || access is null)
        {
            context.Result = new StatusCodeResult(500); await Observe("Error", "Current FIELD authorization services are unavailable."); return;
        }
        try
        {
            actor = await account.RequireAsync(http.RequestAborted); company.RequireActor(actor);
            var database = await company.ResolveAsync(token: http.RequestAborted); var scope = company.Company.Scope;
            var current = await selection.GetCurrentFieldSelectionAsync();
            FieldSelectionReviewValidator.RequireCurrent(current, scope, actor);
            if (current.SelectionContext!.Database != database) throw RefusalException.Conflict("The FIELD preference belongs to another database binding.");
            if (!current.SelectionAvailable)
            {
                context.Result = new BadRequestObjectResult(new { error = "Choose a currently available FIELD before opening its lifecycle." });
                await Observe("Denied", "No currently available FIELD preference."); return;
            }
            field = current.FieldId; var reviewed = current.SelectionContext;
            if (context.RouteData.Values.TryGetValue("fieldId", out var route) && route?.ToString() != field ||
                http.Request.Query.TryGetValue("fieldId", out var query) && (query.Count != 1 || query[0] != field))
                throw RefusalException.Conflict("The asserted FIELD differs from the reviewed current selection.");
            var check = await access.CheckAssetAccessAsync(actor, field, "FIELD", requiredPermission);
            if (check is null) throw RefusalException.Conflict("The current FIELD access decision is unavailable.");
            if (!check.HasAccess)
            {
                context.Result = new ForbidResult(); await Observe("Denied", "Current FIELD access and the requested application permission are required."); return;
            }
            if (requireWrite && (await access.CheckAssetAccessAsync(actor, field, "FIELD", "WRITE"))?.HasAccess != true)
            {
                context.Result = new ForbidResult(); await Observe("Denied", "Current FIELD write access is required."); return;
            }
            var orchestrator = services.GetRequiredService<IFieldOrchestrator>();
            restored = orchestrator;
            if (!await orchestrator.SetActiveFieldAsync(field) || orchestrator.CurrentFieldId != field)
                throw RefusalException.Conflict("The request's lifecycle FIELD could not be restored from the reviewed preference.");
            var readback = await selection.GetCurrentFieldSelectionAsync();
            FieldSelectionReviewValidator.RequireCurrent(readback, scope, actor);
            if (!readback.SelectionAvailable || readback.SelectionContext != reviewed || readback.FieldId != field ||
                await account.RequireAsync(http.RequestAborted) != actor || await company.ResolveAsync(token: http.RequestAborted) != database || company.Company.Scope != scope)
            {
                orchestrator.ClearActiveField();
                throw RefusalException.Conflict("Account, company, database or FIELD authority changed during lifecycle restoration.");
            }
            var confirmedAccess = await access.CheckAssetAccessAsync(actor, field, "FIELD", requiredPermission);
            if (confirmedAccess is null || !confirmedAccess.HasAccess)
                throw RefusalException.Forbidden("Current FIELD access or the requested application permission changed during restoration.");
            if (requireWrite && (await access.CheckAssetAccessAsync(actor, field, "FIELD", "WRITE"))?.HasAccess != true)
                throw RefusalException.Forbidden("Current FIELD write access changed during restoration.");
            await Observe("Allowed", "Current application FIELD authority and the reviewed profile selection were revalidated.");
        }
        catch (OperationCanceledException)
        {
            restored?.ClearActiveField();
            throw;
        }
        catch (Exception error)
        {
            restored?.ClearActiveField();
            await Observe(error is RefusalException ? "Denied" : "Error", "Current FIELD restoration/authorization was refused; the API reports the failure.");
            throw;
        }
    }
}
