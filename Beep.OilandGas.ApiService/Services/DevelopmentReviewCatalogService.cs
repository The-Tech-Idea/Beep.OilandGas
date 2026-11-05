using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.DevelopmentPlanning.Services;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Read-only workflow choices from the existing lifecycle owner and recorded role/persona mappings.</summary>
public sealed class DevelopmentReviewCatalogService(IDevelopmentPlanRequestAuthority authority,IDevelopmentPlanService plans,
    CompanyModuleConnection company,ModuleConnectionResolver modules,IProcessService processes,IRolePersonaReader personas,IDMEEditor editor)
{
    private static T Copy<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;

    public async Task<DevelopmentReviewCatalog> ReadAsync(ClaimsPrincipal user,string id,string expectedSourceRevision)
    {
        if(!LeaseSummaryValidator.Identifier(id) || !ProcessDefinitionRevision.IsValid(expectedSourceRevision))
            throw RefusalException.Invalid("Reload the recorded development draft before choosing a review workflow.");
        var scope=await authority.CaptureAsync(user,PermissionConstants.DevelopmentPlanning.View,false);
        var connection=await modules.ResolveAsync("LIFECYCLE");
        var binding=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],connection));
        if(processes is not BoundProcessService bound)throw new InvalidOperationException("The current bound lifecycle owner is required.");
        var process=await bound.CreateBoundServiceAsync(connection);
        var source=editor.GetDataSource(connection) ?? throw new InvalidOperationException("The configured lifecycle source is unavailable.");
        async Task Guard()
        {
            await scope.CheckCurrentAsync();
            var current=await company.RefreshCompanyAsync();
            if(current.Scope!=scope.Context.Company || current.UserId!=scope.Context.ActorUserId || !current.Permissions.Contains("Workflow.View",StringComparer.Ordinal))
                throw RefusalException.Forbidden("Current company Workflow View permission is required to inspect review assignments.");
            if(await modules.ResolveAsync("LIFECYCLE")!=connection || await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],connection)!=binding.Fingerprint ||
                !ReferenceEquals(source,editor.GetDataSource(connection)))
                throw RefusalException.Conflict("The lifecycle binding changed. Reload the recorded draft and review workflows.");
            MigrationConnectionTarget.Validate(editor,source,connection);
        }
        async Task<DevelopmentReviewCatalog> Capture()
        {
            await Guard();
            var plan=await plans.GetDevelopmentPlanAsync(scope.Context,scope.PlanningDatabase,id,scope.CheckCurrentAsync);
            plan.Require(scope.Context,id);
            if(plan.Plan is null || plan.Revision!=expectedSourceRevision)throw RefusalException.Conflict("The recorded development draft changed. Reload it before choosing a review workflow.");
            var definitions=await process.GetProcessDefinitionsByTypeAsync("DEVELOPMENT") ?? throw new InvalidOperationException("The development review workflow catalog is unavailable.");
            if(definitions.Count>128 || definitions.Any(definition=>definition is null || !definition.IsActive || definition.ProcessType!="DEVELOPMENT" || !LeaseSummaryValidator.Identifier(definition.ProcessId)) ||
                definitions.Select(definition=>definition.ProcessId).Distinct(StringComparer.Ordinal).Count()!=definitions.Count)
                throw new InvalidOperationException("The development workflow catalog is incomplete or ambiguous.");
            var choices=new List<DevelopmentReviewWorkflowChoice>();
            foreach(var definition in Copy(definitions).Where(definition=>definition.EntityType=="FIELD_DEVELOPMENT_PLAN").OrderBy(definition=>definition.ProcessId,StringComparer.Ordinal))
            {
                var admitted=DevelopmentReviewDefinition.Require(definition);
                var steps=new List<DevelopmentReviewRoleStep>();
                foreach(var step in admitted)
                {
                    await Guard();
                    var destinations=await personas.GetActivePersonasAsync(step.RequiredRoles[0]) ?? throw new InvalidOperationException("The configured review role destinations are unavailable.");
                    steps.Add(new(step.StepId,step.StepName,step.SequenceNumber,step.RequiredRoles[0],destinations.Order(StringComparer.Ordinal).ToArray(),step.SlaHours));
                }
                var choice=new DevelopmentReviewWorkflowChoice(definition.ProcessId,definition.ProcessName,definition.Version,
                    ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)),steps);choice.Require();choices.Add(choice);
            }
            await Guard();var result=new DevelopmentReviewCatalog(plan,binding,choices,DevelopmentReviewCatalog.RevisionFor(plan,binding,choices),DateTimeOffset.UtcNow);
            result.Require(scope.Context,id,expectedSourceRevision);return result;
        }
        var first=await Capture();var closing=await Capture();
        if(first.Revision!=closing.Revision)throw RefusalException.Conflict("The draft or configured review assignments changed. Reload before continuing.");
        await scope.ConfirmAsync();await Guard();return Copy(closing);
    }
}
