using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public interface IDevelopmentInvestmentAssignedReader
{
    Task<DevelopmentInvestmentAssignedReview> ReadAssignedAsync(ClaimsPrincipal user,string instanceId);
}

public sealed partial class CompanyDevelopmentInvestmentRoutingService : IDevelopmentInvestmentAssignedReader
{
    private sealed record InvestmentAccess(CompanyScope Company,string Actor,string Persona,PPDMProcessService Native,DevelopmentInvestmentHandoff Original,
        Beep.OilandGas.Models.Processes.ProcessInstance Process,DevelopmentReviewRoleStep Stage,Func<Task> Check,Func<Task> Confirm,Func<DevelopmentInvestmentReview,Task<string>> ReadFunding,Func<string,Task<DevelopmentInvestmentReview>> ReadSource);
    private async Task<InvestmentAccess> CaptureInvestmentAccessAsync(ClaimsPrincipal user,string instanceId,bool write,DevelopmentInvestmentStageDecision? command=null)
    {
        if(!LeaseSummaryValidator.Identifier(instanceId) || !instanceId.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Choose the exact assigned investment review.");
        var actor=user.ActingUserId();company.RequireActor(actor);var selected=(await company.RefreshCompanyAsync()).Scope;
        var core=await company.ResolveAsync();var name=await modules.ResolveAsync("LIFECYCLE");
        var lifecycle=new AssetDatabaseScope(name,await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],name));
        var nativeSource=editor.GetDataSource(name)??throw new InvalidOperationException("The original investment lifecycle source is unavailable.");
        string? persona=null;string[] roles=[];
        async Task<string> Authority()
        {
            var access=await company.RefreshCompanyAsync();company.RequireActor(actor);
            if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=actor || access.Scope!=selected || !access.Roles.Any(user.IsInRole) ||
                !(write?RecipientPermissions:new[]{"Accounting.View","DevelopmentPlanning.View","Workflow.View"}).All(permission=>access.Permissions.Contains(permission,StringComparer.Ordinal)) || await company.ResolveAsync()!=core ||
                await modules.ResolveAsync("LIFECYCLE")!=name || await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],name)!=lifecycle.Fingerprint || !ReferenceEquals(nativeSource,editor.GetDataSource(name)))
                throw new UnauthorizedAccessException("Current company investment review permissions, standard roles and original bindings are required.");
            MigrationConnectionTarget.Validate(editor,nativeSource,name);
            var profile=await workspaces.GetAsync(actor,selected);var workspace=await workspaces.ReviewWorkspacesAsync(actor,selected,accounts);
            if(write && (profile is null || !workspace.Personas.Any(item=>item.Code==profile.PersonaCode)))throw new UnauthorizedAccessException("The current investment receiving workspace is unavailable.");
            persona=profile?.PersonaCode;roles=access.Roles.ToArray();
            return JsonSerializer.Serialize(new{access.Scope,Roles=access.Roles.Order(StringComparer.Ordinal),Permissions=access.Permissions.Order(StringComparer.Ordinal),Profile=profile,workspace.Revision});
        }
        var authority=await Authority();var originalPersona=persona!;var originalRoles=roles;
        async Task Guard(){if(await Authority()!=authority)throw new UnauthorizedAccessException("The current independent investment receiving authority changed.");}
        if(processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(name) is not PPDMProcessService native)
            throw new InvalidOperationException("The canonical native investment receiving owner is required.");
        var process=await native.GetProcessInstanceAsync(instanceId)??throw new InvalidOperationException("The original investment assignment is unavailable.");
        var payload=JsonSerializer.Serialize(process.ProcessData);
        DevelopmentInvestmentHandoff original;
        try
        {
            if(DevelopmentInvestmentHandoff.OwnerCompany(payload)!=selected)throw new UnauthorizedAccessException("The investment assignment belongs to another selected company.");
            original=DevelopmentInvestmentHandoff.FromPayload(payload,instanceId,process.FieldId,process.EntityId);
        }
        catch(Exception invalid) when(invalid is JsonException or KeyNotFoundException or ArgumentException)
        {throw new InvalidOperationException("The original investment assignment ownership is unavailable.",invalid);}
        if(!write && command is null)command=PPDMProcessService.OwnInvestmentStageOriginal(process,actor);
        var scope=original.Scope();var stage=original.Workflow.Steps.SingleOrDefault(item=>item.StepId==(command?.Review.StepId??process.CurrentStepId))
            ??throw new InvalidOperationException("The original investment stage is unavailable.");
        if(scope.CoreDatabase!=core || scope.LifecycleDatabase!=lifecycle)throw new UnauthorizedAccessException("The investment assignment's original source bindings changed.");
        if(command is not null && (command.Scope().ActorUserId!=actor || command.Scope().Company!=selected || command.Review.InstanceId!=instanceId))throw new UnauthorizedAccessException("The retained stage decision belongs to another original account, company or assignment.");
        var receivingPersona=write?originalPersona:command?.Review.PersonaCode??originalPersona;
        if(actor==scope.ActorUserId || actor==original.Review.Source.Afe().CreatedBy || write && (!originalRoles.Contains(stage.AssignedRole,StringComparer.Ordinal) || !user.IsInRole(stage.AssignedRole)) ||
            !original.Option().Stages.Single(item=>item.StepId==stage.StepId).Recipients.Any(item=>item.UserId==actor && item.PersonaCode==receivingPersona))
            throw new UnauthorizedAccessException("Only the original independent receiving individual with the current configured role and workspace may open this assignment.");
        var sourceProcess=original.Review.Source.Current.Source.Review.InstanceId;
        var financialAuthority=await finance.CaptureInvestmentRecipientAuthorityAsync(user,sourceProcess,write);
        async Task Check(){await Guard();await financialAuthority.Guard();}
        await Check();return new(selected,actor,receivingPersona!,native,original,process,stage,Check,financialAuthority.Confirm,financialAuthority.ReadFunding,financialAuthority.ReadSource);
    }
    public async Task<DevelopmentInvestmentAssignedReview> ReadAssignedAsync(ClaimsPrincipal user,string instanceId)
    {
        var access=await CaptureInvestmentAccessAsync(user,instanceId,true);var original=access.Original;var process=access.Process;var stage=access.Stage;
        var scope=original.Scope();var sourceProcess=original.Review.Source.Current.Source.Review.InstanceId;var selected=access.Company;var actor=access.Actor;var originalPersona=access.Persona;var native=access.Native;
        Task Check()=>access.Check();var financialAuthority=(Confirm:access.Confirm,ReadFunding:access.ReadFunding);
        var financial=await access.ReadSource(process.EntityId);original.RequireSource(financial,false);await Check();
        var funding=await financialAuthority.ReadFunding(financial);
        async Task<DevelopmentInvestmentAssignedReview> Capture()
        {
            await Check();var recorded=await native.ReadRecordedInvestmentHandoffAsync(selected,scope.FieldId,process.EntityId,Check)
                ??throw new InvalidOperationException("The recorded investment assignment is no longer present.");
            if(recorded.Original.Revision()!=original.Revision())throw new InvalidOperationException("The original investment assignment changed.");
            var current=recorded.Process;
            var definition=await native.GetProcessDefinitionAsync(original.Workflow.ProcessId)??throw new InvalidOperationException("The configured investment workflow is unavailable.");
            DevelopmentReviewDefinition.RequireInvestment(definition);
            if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))!=original.Workflow.DefinitionRevision)throw new InvalidOperationException("The configured investment workflow changed.");
            var task=recorded.Tasks.Where(item=>item.TARGET_PERSONA_CODE==originalPersona && item.ASSIGNED_ROLE==stage.AssignedRole && item.TASK_STATUS=="PENDING" && item.PROCESS_STEP_INSTANCE_ID==current.StepInstances.Single(step=>step.StepId==stage.StepId).StepInstanceId).ToArray();
            var step=current.StepInstances.SingleOrDefault(item=>item.StepId==stage.StepId);
            if(task.Length!=1 || step is null || current.CurrentStepId!=stage.StepId || step.StepInstanceId!=task[0].PROCESS_STEP_INSTANCE_ID)
                throw new InvalidOperationException("The exact independent receiving task is unavailable or ambiguous.");
            var result=new DevelopmentInvestmentAssignedReview(financial,instanceId,original.Workflow.ProcessName,original.Workflow.Version,scope.ActorUserId,stage.StepId,
                step.StepInstanceId,stage.StepName,stage.AssignedRole,originalPersona,task[0].CROSS_TASK_ID,original.Workflow.DefinitionRevision,
                ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(current)),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task[0])),"",DateTimeOffset.UtcNow){FundingRationale=funding,StageNumber=original.Workflow.Steps.ToList().FindIndex(item=>item.StepId==stage.StepId)+1,StageCount=original.Workflow.Steps.Count,ReservedDecisionRevision=PPDMProcessService.InvestmentReservationStatus(current).Revision,ReservedBy=PPDMProcessService.InvestmentReservationStatus(current).Actor};
            result=result with{Revision=result.ContentRevision()};result.Require(selected,actor,instanceId);await Check();return result;
        }
        var first=await Capture();var closing=await Capture();if(first.Revision!=closing.Revision)throw new InvalidOperationException("The investment assignment or source changed during receiving review.");
        var after=await access.ReadSource(process.EntityId);
        if(after.Current.Revision!=financial.Current.Revision || after.Recorded!=financial.Recorded || !after.Blockers.SequenceEqual(financial.Blockers,StringComparer.Ordinal))
            throw new InvalidOperationException("The immutable AFE source or recorded financial decision changed during receiving review.");
        if(funding!=await financialAuthority.ReadFunding(after))throw new InvalidOperationException("The original funding basis changed during assignment review.");
        await financialAuthority.Confirm();await Check();return CompanyInventorySourceReview.Copy(closing);
    }
}
