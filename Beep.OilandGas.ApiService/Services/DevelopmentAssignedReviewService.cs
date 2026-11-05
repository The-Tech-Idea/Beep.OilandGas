using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Repository.Models;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.DevelopmentPlanning.Services;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Services;

public interface IDevelopmentAssignedReviewReader
{
    Task<DevelopmentAssignedReview> ReadAsync(ClaimsPrincipal user,string processId);
}
public interface IDevelopmentReviewProgressReader
{
    Task<DevelopmentReviewProgress> ReadProgressAsync(ClaimsPrincipal user,string processId);
    Task<DevelopmentReviewProgress?> ReadForSourceAsync(ClaimsPrincipal user,DevelopmentPlanSourceReview source);
}
public interface IDevelopmentPlanBaselineService
{
    Task<DevelopmentPlanBaselineReview> ReadBaselineAsync(ClaimsPrincipal user,string processId,string expectedReviewRevision);
    Task<DevelopmentPlanBaselineReview> ReadBudgetSourceAsync(ClaimsPrincipal user,string processId);
    Task<DevelopmentPlanReleaseReceipt> ApproveBaselineAsync(ClaimsPrincipal user,DevelopmentPlanRelease command,bool recover=false);
}
public interface IDevelopmentReviewDecisionService
{
    Task<DevelopmentReviewDecisionReceipt> DecideAsync(ClaimsPrincipal user,DevelopmentReviewDecision command,bool recover=false);
}

/// <summary>One current individual authority for assigned draft reads and canonical lifecycle review decisions.</summary>
public sealed class DevelopmentAssignedReviewService(RepositoryRequestAccount account,CompanyModuleConnection company,ModuleConnectionResolver modules,
    RepositoryDbContext repository,IRepositoryAccessService accounts,RepositoryPersonaService workspaces,IUserAssetAccessStore grants,
    IAccessControlService resources,IDevelopmentPlanService plans,IProcessService processes,IDMEEditor editor,DevelopmentReviewRecipientAuthority destinations)
    : IDevelopmentAssignedReviewReader,IDevelopmentReviewDecisionService,IDevelopmentReviewProgressReader,IDevelopmentPlanBaselineService
{
    private sealed record Access(CompanyScope Company,string Actor,AssetDatabaseScope Core,AssetDatabaseScope Planning,AssetDatabaseScope Lifecycle,
        string Persona,string[] Roles,IReadOnlyList<AppUserAssetAccess> Grants,PPDMProcessService Native,Func<Task> Check);
    private async Task<Access> CaptureAccessAsync(ClaimsPrincipal user,bool reviewer=true)
    {
        var actor=await account.RequireAsync();company.RequireActor(actor);var selected=company.Company.Scope;var core=await company.ResolveAsync();
        async Task<AssetDatabaseScope> Binding(string module){var name=await modules.ResolveAsync(module);return new(name,await modules.GetMigrationBindingFingerprintAsync([module],name));}
        var planning=await Binding("DEVELOPMENT_PLANNING");var lifecycle=await Binding("LIFECYCLE");
        var source=editor.GetDataSource(lifecycle.ConnectionName)??throw new InvalidOperationException("The original lifecycle source is unavailable.");
        string? persona=null;string[] roles=[];List<AppUserAssetAccess> capturedGrants=[];
        async Task<string> Snapshot()
        {
            if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=actor || await account.RequireAsync()!=actor)throw new UnauthorizedAccessException("The assigned review account changed.");
            var current=await company.RefreshCompanyAsync();company.RequireActor(actor);
            if(current.Scope!=selected || !current.Roles.Any(user.IsInRole) ||
                !(reviewer?new[]{"DevelopmentPlanning.View","DevelopmentPlanning.Approve","Workflow.View","Workflow.Approve"}:new[]{"DevelopmentPlanning.View","Workflow.View"}).All(permission=>current.Permissions.Contains(permission,StringComparer.Ordinal)) ||
                await company.ResolveAsync()!=core || await Binding("DEVELOPMENT_PLANNING")!=planning || await Binding("LIFECYCLE")!=lifecycle || !ReferenceEquals(source,editor.GetDataSource(lifecycle.ConnectionName)))
                throw new UnauthorizedAccessException("Current company development review permissions, standard roles or module bindings changed.");
            MigrationConnectionTarget.Validate(editor,source,lifecycle.ConnectionName);
            var profile=await repository.Set<AppUserPersona>().AsNoTracking().SingleOrDefaultAsync(row=>row.UserId==actor && row.TenantId==selected.TenantId && row.BusinessAssociateId==selected.BusinessAssociateId);
            var workspace=await workspaces.ReviewWorkspacesAsync(actor,selected,accounts);
            var mapped=profile is not null && workspace.Personas.Any(row=>row.Code==profile.PersonaCode);
            if(reviewer && !mapped)throw new UnauthorizedAccessException("The current assigned workspace is unavailable.");
            var rows=await grants.ReadAsync(actor,core.Fingerprint,selected.BusinessAssociateId);
            persona=mapped?profile!.PersonaCode:"";roles=current.Roles.ToArray();capturedGrants=rows;
            return JsonSerializer.Serialize(new{Roles=current.Roles.Order(StringComparer.Ordinal),Permissions=current.Permissions.Order(StringComparer.Ordinal),Profile=profile,workspace.Revision,Grants=rows.OrderBy(row=>row.Id,StringComparer.Ordinal)});
        }
        var captured=await Snapshot();var initialPersona=persona!;var initialRoles=roles;var initialGrants=capturedGrants;
        async Task Check(){if(await Snapshot()!=captured)throw new UnauthorizedAccessException("The current independent review authority changed. Reload assigned work.");}
        if(processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(lifecycle.ConnectionName) is not PPDMProcessService native)
            throw new InvalidOperationException("The canonical bound lifecycle owner is required for development review.");
        return new(selected,actor,core,planning,lifecycle,initialPersona,initialRoles,initialGrants,native,Check);
    }
    private static void RequireRecipient(ClaimsPrincipal user,Access access,DevelopmentReviewStart owner,string stepId)
    {
        if(owner.Source.Context.Company!=access.Company)throw new UnauthorizedAccessException("The development review belongs to another selected company.");
        if(owner.Source.Context.Database!=access.Core || owner.Source.PlanningDatabase!=access.Planning || owner.LifecycleDatabase!=access.Lifecycle)
            throw new InvalidOperationException("The assigned source's original database bindings changed.");
        var stage=owner.ReceivingSteps.SingleOrDefault(row=>row.StepId==stepId)??throw new InvalidOperationException("The recorded configured stage is unavailable.");
        if(access.Actor==owner.Source.Plan!.ROW_CREATED_BY || access.Actor==owner.Source.Context.ActorUserId || !access.Roles.Contains(stage.AssignedRole,StringComparer.Ordinal) || !user.IsInRole(stage.AssignedRole) ||
            !stage.Recipients.Any(row=>row.UserId==access.Actor && row.PersonaCode==access.Persona))
            throw new UnauthorizedAccessException("Only the original independent recipient with the current configured role may open this review.");
        if(!access.Grants.Any(row=>row.UserId==access.Actor && row.DatabaseScope==access.Core.Fingerprint && row.OrganizationId==access.Company.BusinessAssociateId &&
            row.AssetType=="FIELD" && row.AssetId==owner.Source.Context.FieldId && row.FacilityType is null && row.IsActive && row.AccessLevel is "READ" or "WRITE" or "DELETE"))
            throw new UnauthorizedAccessException("A current original FIELD grant is required for this development review.");
    }
    private Task<DevelopmentPlanSourceReview> ReadSourceAsync(ClaimsPrincipal user,Access access,DevelopmentReviewStart owner,string stepId)
        =>ReadSourceAsync(access,owner,async()=>{await access.Check();RequireRecipient(user,access,owner,stepId);});
    private async Task<DevelopmentPlanSourceReview> ReadSourceAsync(Access access,DevelopmentReviewStart owner,Func<Task> check)
    {
        async Task Field(){await check();if(!(await resources.CheckAssetAccessAsync(access.Actor,owner.Source.Context.FieldId!,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current original FIELD READ access is required.");await check();}
        await Field();
        // Persisted maker context supplies internal source identity only; current individual authority admits the read.
        var source=await plans.GetDevelopmentPlanAsync(owner.Source.Context,access.Planning,owner.Source.PlanId,check);owner.RequireSource(source);
        await Field();return source;
    }
    private static void RequireSourceScope(Access access,DevelopmentReviewStart owner)
    {
        if(owner.Source.Context.Company!=access.Company)throw new UnauthorizedAccessException("The recorded review belongs to another selected company.");
        if(owner.Source.Context.Database!=access.Core || owner.Source.PlanningDatabase!=access.Planning || owner.LifecycleDatabase!=access.Lifecycle)
            throw new InvalidOperationException("The recorded review's original database bindings changed.");
        if(!access.Grants.Any(row=>row.UserId==access.Actor && row.DatabaseScope==access.Core.Fingerprint && row.OrganizationId==access.Company.BusinessAssociateId &&
            row.AssetType=="FIELD" && row.AssetId==owner.Source.Context.FieldId && row.FacilityType is null && row.IsActive && row.AccessLevel is "READ" or "WRITE" or "DELETE"))
            throw new UnauthorizedAccessException("Current exact original FIELD authority is required for review history.");
    }
    private static void RequireParticipant(ClaimsPrincipal user,Access access,DevelopmentReviewStart owner)
    {
        RequireSourceScope(access,owner);
        if(access.Actor==owner.Source.Context.ActorUserId || access.Actor==owner.Source.Plan!.ROW_CREATED_BY)return;
        var stage=owner.ReceivingSteps.FirstOrDefault(step=>access.Roles.Contains(step.AssignedRole,StringComparer.Ordinal) && user.IsInRole(step.AssignedRole) &&
            step.Recipients.Any(recipient=>recipient.UserId==access.Actor && recipient.PersonaCode==access.Persona));
        if(stage is null)throw new UnauthorizedAccessException("Only the recorded requester, maker or current original recipient may inspect this review history.");
        RequireRecipient(user,access,owner,stage.StepId);
    }
    private async Task<DevelopmentReviewProgress> CaptureProgressAsync(ClaimsPrincipal user,Access access,string processId,bool budgetSource=false)
    {
        await access.Check();var snapshot=await access.Native.ReadDevelopmentReviewOutcomeAsync(processId,access.Check);var process=snapshot.Process;
        var owner=DevelopmentReviewStart.FromPayload(JsonSerializer.Serialize(process.ProcessData),processId,process.FieldId,process.EntityId);
        async Task Check()
        {
            await access.Check();
            if(budgetSource)
            {
                if(process.CurrentState!="REVIEW_APPROVED" || !(await company.RefreshCompanyAsync()).Permissions.Contains("Accounting.View",StringComparer.Ordinal))
                    throw new UnauthorizedAccessException("Approved FDP scope and current company Accounting View authority are required for financial preparation.");
                RequireSourceScope(access,owner);
            }
            else RequireParticipant(user,access,owner);
        }
        await Check();var source=await ReadSourceAsync(access,owner,Check);
        var result=new DevelopmentReviewProgress(new(access.Company,access.Actor,process.FieldId,access.Core,access.Planning,access.Lifecycle),source.Plan!,source.Revision,
            processId,owner.Workflow.ProcessName,owner.Workflow.Version,process.StartedBy,process.CurrentState,snapshot.Stages,snapshot.History,"",DateTimeOffset.UtcNow);
        result=result with{Revision=result.ContentRevision()};result.Require(access.Company,access.Actor,processId);await Check();return result;
    }
    public async Task<DevelopmentReviewProgress> ReadProgressAsync(ClaimsPrincipal user,string processId)
    {
        if(!LeaseSummaryValidator.Identifier(processId))throw RefusalException.Invalid("Choose a recorded development review.");
        var access=await CaptureAccessAsync(user,false);var first=await CaptureProgressAsync(user,access,processId);var closing=await CaptureProgressAsync(user,access,processId);
        if(first.Revision!=closing.Revision)throw new InvalidOperationException("The recorded review/source changed during its read.");await access.Check();return closing;
    }
    public async Task<DevelopmentReviewProgress?> ReadForSourceAsync(ClaimsPrincipal user,DevelopmentPlanSourceReview source)
    {
        source.Require(source.Context,source.PlanId);if(source.Plan is null)throw RefusalException.Conflict("Inspect an existing recorded draft before looking up its review.");
        var access=await CaptureAccessAsync(user,false);
        if(source.Context.Company!=access.Company || source.Context.ActorUserId!=access.Actor || source.Context.Database!=access.Core || source.PlanningDatabase!=access.Planning)
            throw new UnauthorizedAccessException("The displayed draft belongs to another current actor, company or target.");
        Task<string?> Find()=>access.Native.FindDevelopmentReviewAsync(access.Company,source.Context.FieldId!,source.PlanId,access.Check);
        var id=await Find();var result=id is null?null:await ReadProgressAsync(user,id);
        if(result is not null && (result.SourceRevision!=source.Revision || result.Scope.FieldId!=source.Context.FieldId || result.Plan.FDP_ID!=source.PlanId))
            throw new InvalidOperationException("The recorded review differs from the displayed draft.");
        if(id!=await Find())throw new InvalidOperationException("The recorded review or its absence changed during lookup.");await access.Check();return result;
    }
    public async Task<DevelopmentAssignedReview> ReadAsync(ClaimsPrincipal user,string processId)
    {
        if(!LeaseSummaryValidator.Identifier(processId))throw RefusalException.Invalid("Choose an assigned development review from the company inbox.");
        var access=await CaptureAccessAsync(user);
        async Task<DevelopmentAssignedReview> Capture()
        {
            await access.Check();var graph=await access.Native.ReadDevelopmentReviewAssignmentAsync(processId,access.Check);var process=graph.Process;
            var owner=DevelopmentReviewStart.FromPayload(JsonSerializer.Serialize(process.ProcessData),processId,process.FieldId,process.EntityId);
            var active=process.StepInstances.Single(row=>row.StepId==process.CurrentStepId);RequireRecipient(user,access,owner,active.StepId);await access.Check();
            var source=await ReadSourceAsync(user,access,owner,active.StepId);
            var matching=graph.Tasks.Where(task=>task.TARGET_PERSONA_CODE==access.Persona && task.ASSIGNED_ROLE==active.RequiredRole).ToArray();
            if(matching.Length!=1)throw new InvalidOperationException("The original individual persona task is missing or ambiguous.");
            var result=new DevelopmentAssignedReview(new(access.Company,access.Actor,process.FieldId,access.Core,access.Planning,access.Lifecycle),source.Plan!,source.Revision,processId,owner.Workflow.ProcessName,
                owner.Workflow.Version,process.StartedBy,active.StepId,active.StepInstanceId,active.StepName,active.RequiredRole!,access.Persona,matching[0].CROSS_TASK_ID,
                owner.Workflow.DefinitionRevision,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process)),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(matching[0])),"",DateTimeOffset.UtcNow);
            result=result with{Revision=result.ContentRevision()};result.Require(access.Company,access.Actor,processId);await access.Check();return result;
        }
        var first=await Capture();var closing=await Capture();if(first.Revision!=closing.Revision)throw new InvalidOperationException("The assigned task, process or immutable draft changed during review.");
        await access.Check();return JsonSerializer.Deserialize<DevelopmentAssignedReview>(JsonSerializer.Serialize(closing))!;
    }
    public async Task<DevelopmentReviewDecisionReceipt> DecideAsync(ClaimsPrincipal user,DevelopmentReviewDecision command,bool recover=false)
    {
        try{command.Require();command=JsonSerializer.Deserialize<DevelopmentReviewDecision>(JsonSerializer.Serialize(command))!;}
        catch(Exception invalid) when(invalid is ArgumentException or InvalidOperationException or JsonException){throw RefusalException.Invalid("Review the assigned stage and retain its exact decision and rationale before submitting.");}
        var access=await CaptureAccessAsync(user);var review=command.Review;
        var process=await access.Native.ReadDevelopmentReviewProgressAsync(review.InstanceId,access.Check);
        var owner=DevelopmentReviewStart.FromPayload(JsonSerializer.Serialize(process.ProcessData),process.InstanceId,process.FieldId,process.EntityId);
        RequireRecipient(user,access,owner,review.StepId);
        if(review.Scope!=new DevelopmentAssignedReviewScope(access.Company,access.Actor,process.FieldId,access.Core,access.Planning,access.Lifecycle) || review.AssignedRole!=owner.Workflow.Steps.Single(row=>row.StepId==review.StepId).AssignedRole ||
            review.PersonaCode!=access.Persona || review.SourceRevision!=owner.Source.Revision || review.DefinitionRevision!=owner.Workflow.DefinitionRevision)
            throw RefusalException.Conflict("The retained decision belongs to another original assignment or source.");
        var index=owner.Workflow.Steps.ToList().FindIndex(step=>step.StepId==review.StepId);DevelopmentReviewRecipientStep? next=null;
        async Task<DevelopmentReviewRecipientStep> ReadNext()
        {
            var template=owner.Workflow.Steps[index+1];var current=await destinations.ReadAsync(owner.Source,template.StepId,template.AssignedRole,review.Scope);
            if(JsonSerializer.Serialize(current.Recipients)!=JsonSerializer.Serialize(owner.ReceivingSteps[index+1].Recipients))
                throw RefusalException.Conflict("The next configured independent recipients changed. Reconcile the recorded assignment before routing.");
            return current;
        }
        if(!recover && command.Decision=="APPROVE" && index+1<owner.Workflow.Steps.Count)next=await ReadNext();
        async Task Check()
        {
            await access.Check();RequireRecipient(user,access,owner,review.StepId);
            if(next is not null && JsonSerializer.Serialize(await ReadNext())!=JsonSerializer.Serialize(next))throw RefusalException.Conflict("Next-stage recipient authority changed before routing.");
            await access.Check();
        }
        Task<DevelopmentPlanSourceReview> Source()=>ReadSourceAsync(user,access,owner,review.StepId);
        await Check();var receipt=recover?await access.Native.RecoverDevelopmentDecisionAsync(command,Check,Source):await access.Native.DecideDevelopmentReviewAsync(command,Check,Source);
        receipt.Require(command);if(!recover && !receipt.Recorded)throw new InvalidOperationException("The independent review decision was not confirmed recorded.");return receipt;
    }
    public async Task<DevelopmentPlanBaselineReview> ReadBaselineAsync(ClaimsPrincipal user,string processId,string expectedReviewRevision)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !ProcessDefinitionRevision.IsValid(expectedReviewRevision))throw RefusalException.Invalid("Inspect the recorded review before checking its FDP baseline.");
        var access=await CaptureAccessAsync(user,false);var first=await CaptureProgressAsync(user,access,processId);
        if(first.Revision!=expectedReviewRevision)throw RefusalException.Conflict("The displayed review changed. Reload before opening its baseline.");
        var result=await plans.GetBaselineAsync(first,access.Check);result.Require(first);
        var closing=await CaptureProgressAsync(user,access,processId);
        if(closing.Revision!=first.Revision)throw RefusalException.Conflict("The recorded review changed during its FDP baseline lookup.");await access.Check();return result;
    }
    public async Task<DevelopmentPlanReleaseReceipt> ApproveBaselineAsync(ClaimsPrincipal user,DevelopmentPlanRelease command,bool recover=false)
    {
        ArgumentNullException.ThrowIfNull(command);command.Require();command=JsonSerializer.Deserialize<DevelopmentPlanRelease>(JsonSerializer.Serialize(command))!;
        var access=await CaptureAccessAsync(user,!recover);var review=command.Review;
        if(review.Scope.ActorUserId!=access.Actor || review.Scope.Company!=access.Company)throw RefusalException.Forbidden("The retained baseline approval belongs to another original actor or company.");
        var process=await access.Native.ReadDevelopmentReviewProgressAsync(review.InstanceId,access.Check);
        var owner=DevelopmentReviewStart.FromPayload(JsonSerializer.Serialize(process.ProcessData),process.InstanceId,process.FieldId,process.EntityId);
        var final=owner.Workflow.Steps[^1];
        async Task Check()
        {
            await access.Check();RequireRecipient(user,access,owner,final.StepId);
            if(!recover && !access.Grants.Any(row=>row.UserId==access.Actor && row.DatabaseScope==access.Core.Fingerprint && row.OrganizationId==access.Company.BusinessAssociateId &&
                row.AssetType=="FIELD" && row.AssetId==review.Scope.FieldId && row.FacilityType is null && row.IsActive && row.AccessLevel is "WRITE" or "DELETE"))
                throw new UnauthorizedAccessException("Current exact original FIELD WRITE authority is required to approve the FDP baseline.");
        }
        async Task Confirm()
        {
            await Check();var current=await CaptureProgressAsync(user,access,review.InstanceId);
            if(current.Revision!=review.Revision || current.WorkflowState!="REVIEW_APPROVED")throw RefusalException.Conflict("The approved source/review changed. Preserve and recover the original baseline intent.");
            if(!(await resources.CheckAssetAccessAsync(access.Actor,review.Scope.FieldId,"FIELD",recover?"READ":"WRITE")).HasAccess)
                throw new UnauthorizedAccessException("Current original FIELD authority is required for the FDP baseline.");await Check();
        }
        // Completed lifecycle/source/FIELD reads bracket the planning transaction; only fresh app authority is read inside it.
        await Confirm();var receipt=await plans.ApproveBaselineAsync(command,Check,recover);receipt.Require(command);await Confirm();
        if(!recover && receipt.Result.Baseline is null)throw new InvalidOperationException("The original FDP baseline approval was not confirmed recorded. Recover its original intent.");return receipt;
    }

    public async Task<DevelopmentPlanBaselineReview> ReadBudgetSourceAsync(ClaimsPrincipal user,string processId)
    {
        if(!LeaseSummaryValidator.Identifier(processId))throw RefusalException.Invalid("Choose a recorded approved FDP baseline before opening an AFE request.");
        var access=await CaptureAccessAsync(user,false);var first=await CaptureProgressAsync(user,access,processId,true);
        var result=await plans.GetBaselineAsync(first,access.Check);result.Require(first);
        if(result.Baseline is null)throw RefusalException.Conflict("Record the approved FDP baseline before preparing its company AFE.");
        var closing=await CaptureProgressAsync(user,access,processId,true);if(closing.Revision!=first.Revision)throw RefusalException.Conflict("The approved FDP scope changed during financial source review.");await access.Check();return result;
    }

}
