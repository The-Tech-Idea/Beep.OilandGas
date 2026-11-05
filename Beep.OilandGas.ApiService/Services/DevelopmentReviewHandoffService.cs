using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.DevelopmentPlanning.Services;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>API authority for a recorded draft handoff; native writes remain with the canonical lifecycle owner.</summary>
public sealed class DevelopmentReviewHandoffService(IDevelopmentPlanRequestAuthority authority,IDevelopmentPlanService plans,
    DevelopmentReviewCatalogService catalog,DevelopmentReviewRecipientAuthority recipients,CompanyModuleConnection company,
    ModuleConnectionResolver modules,IProcessService processes,IDMEEditor editor)
{
    public async Task<DevelopmentReviewStart> PrepareAsync(ClaimsPrincipal user,string planId,DevelopmentReviewSelection selection)
    {
        if(selection is null || !LeaseSummaryValidator.Identifier(selection.ProcessId) || !ProcessDefinitionRevision.IsValid(selection.SourceRevision) || !ProcessDefinitionRevision.IsValid(selection.CatalogRevision))
            throw RefusalException.Invalid("Choose a workflow from the reviewed development source catalog.");
        var scope=await authority.CaptureAsync(user,PermissionConstants.DevelopmentPlanning.Create,true);
        await RequireStartPermission(scope.Context);
        var current=await catalog.ReadAsync(user,planId,selection.SourceRevision);
        if(current.Revision!=selection.CatalogRevision || current.Source.Context!=scope.Context || current.Source.PlanningDatabase!=scope.PlanningDatabase)
            throw RefusalException.Conflict("The source or workflow catalog changed. Inspect it again before preparing a handoff.");
        var workflow=current.Choices.SingleOrDefault(choice=>choice.ProcessId==selection.ProcessId)??throw RefusalException.Conflict("The selected workflow is no longer configured for this source.");
        var receiving=new List<DevelopmentReviewRecipientStep>();
        foreach(var step in workflow.Steps)receiving.Add(await recipients.ReadAsync(current.Source,step.StepId,step.AssignedRole));
        var qualified=workflow with{Steps=workflow.Steps.Select(step=>step with{PersonaCodes=receiving.Single(row=>row.StepId==step.StepId).Recipients.Select(row=>row.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray()}).ToArray()};
        var original=new DevelopmentReviewStart(current.Source,current.LifecycleDatabase,qualified,receiving,Guid.NewGuid());original.Require();
        var bound=await CaptureAsync(user,original,true);await bound.Check();original.RequireSource(await bound.ReadSource());await bound.Check();return original;
    }
    public async Task<DevelopmentReviewStartReceipt> SubmitAsync(ClaimsPrincipal user,DevelopmentReviewStart original,bool recover=false)
    {
        ArgumentNullException.ThrowIfNull(original);original.Require();original=JsonSerializer.Deserialize<DevelopmentReviewStart>(JsonSerializer.Serialize(original))!;
        var captured=await CaptureAsync(user,original,!recover);
        var result=recover?await captured.Native.RecoverDevelopmentReviewAsync(original,captured.Check,captured.ReadSource)
            :await captured.Native.StartDevelopmentReviewAsync(original,captured.Check,captured.ReadSource);
        result.Require(original);if(!recover && !result.Recorded)throw new InvalidOperationException("The development review handoff was not recorded.");return result;
    }
    private async Task RequireStartPermission(FieldSelectionReview context,bool start=true)
    {
        var current=await company.RefreshCompanyAsync();company.RequireActor(context.ActorUserId);
        if(current.Scope!=context.Company || start && !current.Permissions.Contains("Workflow.Start",StringComparer.Ordinal) || !current.Permissions.Contains("Workflow.View",StringComparer.Ordinal))
            throw RefusalException.Forbidden("Current company Workflow Start and View permissions are required to request development review.");
    }
    private async Task<(PPDMProcessService Native,Func<Task> Check,Func<Task<DevelopmentPlanSourceReview>> ReadSource)> CaptureAsync(ClaimsPrincipal user,DevelopmentReviewStart original,bool write)
    {
        var scope=await authority.CaptureAsync(user,PermissionConstants.DevelopmentPlanning.Create,write,original.Source.Context,write);
        if(scope.PlanningDatabase!=original.Source.PlanningDatabase)throw RefusalException.Conflict("The original development-planning target changed.");
        var binding=original.LifecycleDatabase;var source=editor.GetDataSource(binding.ConnectionName)??throw new InvalidOperationException("The original lifecycle source is unavailable.");
        async Task Check()
        {
            await scope.CheckCurrentAsync();await RequireStartPermission(scope.Context,write);
            if(await modules.ResolveAsync("LIFECYCLE")!=binding.ConnectionName || await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],binding.ConnectionName)!=binding.Fingerprint ||
                !ReferenceEquals(source,editor.GetDataSource(binding.ConnectionName)))throw RefusalException.Conflict("The original development lifecycle target changed.");
            MigrationConnectionTarget.Validate(editor,source,binding.ConnectionName);
            foreach(var expected in write?original.ReceivingSteps:[])
            {
                var current=await recipients.ReadAsync(original.Source,expected.StepId,expected.AssignedRole);
                if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(expected))throw RefusalException.Conflict("The independent development recipient authority changed. Recover the original outcome before preparing another handoff.");
            }
            await scope.CheckCurrentAsync();
        }
        async Task<DevelopmentPlanSourceReview> ReadSource()
        {
            await scope.ConfirmAsync();await Check();
            var current=await plans.GetDevelopmentPlanAsync(scope.Context,scope.PlanningDatabase,original.Source.PlanId,scope.CheckCurrentAsync);
            original.RequireSource(current);await scope.ConfirmAsync();await Check();return current;
        }
        await Check();
        if(processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(binding.ConnectionName) is not PPDMProcessService native)
            throw new InvalidOperationException("The canonical bound native lifecycle owner is required for development review.");
        return(native,Check,ReadSource);
    }
}
