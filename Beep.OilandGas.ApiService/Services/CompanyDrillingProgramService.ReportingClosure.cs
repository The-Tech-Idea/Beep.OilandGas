using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyDrillingProgramService
{
    private async Task<(PPDMProcessService Owner,Func<Task> Check,DrillingExecutionAssignment Original,ProcessInstance Process,IReadOnlyList<CROSS_PERSONA_TASK> Tasks,string NativeRevision)>
        CaptureReportingResponsibilityAsync(ClaimsPrincipal user,string id,bool management=false)
    {
        if(!LeaseSummaryValidator.Identifier(id) || !id.StartsWith(DrillingExecutionAssignment.InstancePrefix,StringComparison.Ordinal))
            throw new InvalidOperationException("Open an exact original reporting responsibility.");
        var actor=await account.RequireAsync();var selected=(await company.RefreshCompanyAsync()).Scope;var connection=await modules.ResolveAsync("LIFECYCLE");
        if(processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(connection) is not PPDMProcessService owner)
            throw new InvalidOperationException("The canonical reporting responsibility owner is required.");
        async Task Check()
        {
            var current=await company.RefreshCompanyAsync();
            if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=actor || await account.RequireAsync()!=actor || current.Scope!=selected || !current.Roles.Any(user.IsInRole) ||
                !new[]{"Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>current.Permissions.Contains(permission,StringComparer.Ordinal)) ||
                await modules.ResolveAsync("LIFECYCLE")!=connection)
                throw new UnauthorizedAccessException("The reporting account, company, standard view roles or source changed.");
            if(management && !new[]{"Drilling.Edit","Drilling.ManageWellProgram","Workflow.Start"}.All(permission=>current.Permissions.Contains(permission,StringComparer.Ordinal)))
                throw new UnauthorizedAccessException("Current reporting management authority is required.");
        }
        await Check();var graph=await owner.ReadRecordedExecutionAssignmentAsync(selected,id,Check);await Check();
        if(!management && actor!=graph.Original.Scope().ActorUserId && !graph.Original.Workflow.Authority.Recipients.Any(row=>row.UserId==actor) &&
            !PPDMProcessService.RecordedReportingReassignments(graph.Process).Any(row=>row.Scope().ActorUserId==actor || row.Review.Destination.Authority.Recipients.Any(recipient=>recipient.UserId==actor)))
            throw new UnauthorizedAccessException("Only an original reporting participant may open this responsibility.");
        return(owner,Check,graph.Original,graph.Process,graph.Tasks,graph.NativeRevision);
    }

    private async Task<(Func<Task> Check,Func<Task> Confirm,ExecutionRecorder? Manager)> CaptureReportingClosureAuthorityAsync(ClaimsPrincipal user,DrillingReportingGeneration responsibility,DrillingExecutionBasis executionBasis,bool write)
    {
        responsibility.Require();var original=responsibility.Root;var scope=executionBasis.Scope();original.Source.Basis.RequireSource(executionBasis,false,false);
        if(scope.ActorUserId!=responsibility.ManagerUserId || user.ActingUserId()!=scope.ActorUserId || await account.RequireAsync()!=scope.ActorUserId || (await company.RefreshCompanyAsync()).Scope!=scope.Company)
            throw new UnauthorizedAccessException("Only the accountable manager for this reporting generation may close or recover its original intent.");
        return await CaptureReportingManagementAuthorityAsync(user,executionBasis,write,responsibility.ManagerRole);
    }

    private async Task<(Func<Task> Check,Func<Task> Confirm,ExecutionRecorder? Manager)> CaptureReportingManagementAuthorityAsync(ClaimsPrincipal user,DrillingExecutionBasis executionBasis,bool write,string? requiredRole=null)
    {
        var scope=executionBasis.Scope();
        if(user.Identity?.IsAuthenticated!=true || user.ActingUserId()!=scope.ActorUserId || await account.RequireAsync()!=scope.ActorUserId || (await company.RefreshCompanyAsync()).Scope!=scope.Company)
            throw new UnauthorizedAccessException("Only the current reporting management account in its original company may act on this intent.");
        var basis=executionBasis.Source.Basis;var fundingSource=basis.Funding.Source;
        var access=await Capture(user,fundingSource.Current.Source.Review.InstanceId,fundingSource.AfeId,basis.Well.Uwi,write,!write,basis,"Drilling.Edit");
        var bindings=await CaptureExecutionBindingsAsync(executionBasis);
        var manager=write?await CaptureExecutionRecorderAsync(user,scope,reporting:true):null;
        if(manager is not null && (manager.Roles.Length==0 || requiredRole is not null && !manager.Roles.Contains(requiredRole,StringComparer.Ordinal)))
            throw new UnauthorizedAccessException("The configured reporting management role and mapped workspace are required.");
        async Task Check()
        {
            await access.Check();await bindings();await RequireReportViewAsync(user,scope);
            if(manager is not null)
            {
                await manager.Check();
                if(!(await company.RefreshCompanyAsync()).Permissions.Contains("Workflow.Start",StringComparer.Ordinal))
                    throw new UnauthorizedAccessException("Current configured reporting-management workflow authority is required.");
            }
        }
        async Task Confirm(){await Check();await access.Confirm();await Check();}
        await Confirm();return(Check,Confirm,manager);
    }

    private async Task<DrillingAcceptedCompletionTests> ReadReportingClosureEvidenceAsync(ClaimsPrincipal user,DrillingExecutionAssignment assignment,string completionId,Func<Task> authority,Func<Task> confirm,
        (DrillingExecutionBasis Basis,Func<Task> Check,Func<Task> Confirm)? executionAccess=null,bool confirmSource=true)
    {
        if(!LeaseSummaryValidator.Identifier(completionId) || !completionId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal))
            throw new InvalidOperationException("Choose an exact accepted completion review for this responsibility.");
        var access=executionAccess??await CaptureExecutionReadAsync(user,assignment.Source.Basis.ReviewInstanceId);var scope=access.Basis.Scope();assignment.Source.Basis.RequireSource(access.Basis,false,false);
        async Task Guard(){await authority();await access.Check();await RequireReportViewAsync(user,scope);}
        var execution=await native.ReadExecutionStatusAsync(access.Basis,Guard);assignment.RequireSource(execution,false);
        var owner=await ExecutionAssignmentOwner(execution);var first=await owner.ReadRecordedCompletionReviewAsync(scope.Company,completionId,Guard);
        if(first.Original.Scope().FieldId!=scope.FieldId || first.Original.Scope().CoreDatabase!=scope.CoreDatabase || first.Original.Scope().LifecycleDatabase!=scope.LifecycleDatabase ||
            first.Original.Source.Execution.Recorded?.DRILLING_OPERATION_ID!=assignment.OperationId)
            throw new UnauthorizedAccessException("The accepted completion belongs to another original reporting source.");
        var page=await native.ReadDailyReportEntryAsync(execution,first.Original.Source.LatestObservedReport!,Guard,()=>Task.CompletedTask);
        first.Original.RequireSource(page,false,false);
        var progress=PPDMProcessService.CompletionReviewProgress(first.Original,first.Process,first.Tasks,page,first.NativeRevision);
        async Task Confirm()
        {
            await Guard();if(confirmSource)await access.Confirm();await confirm();
            var well=await native.ReadProgramWellAsync(scope.CoreDatabase,scope.FieldId,assignment.Source.Basis.Source.Basis.Well.Uwi,true,Guard);
            if(well!=assignment.Source.Basis.Source.Basis.Well)throw new InvalidOperationException("The original active reporting well changed before closure.");
            var current=await owner.ReadRecordedCompletionReviewAsync(scope.Company,completionId,Guard);
            if(current.NativeRevision!=first.NativeRevision || current.Original.Revision()!=first.Original.Revision())
                throw new InvalidOperationException("The complete accepted completion review changed during closure qualification.");
            await Guard();
        }
        var evidence=await native.ReadAcceptedCompletionTestsAsync(progress,Guard,Confirm);
        var closing=await owner.ReadRecordedCompletionReviewAsync(scope.Company,completionId,Guard);
        if(closing.NativeRevision!=first.NativeRevision || closing.Original.Revision()!=first.Original.Revision())
            throw new InvalidOperationException("The accepted completion graph changed after the final native test read.");
        await Guard();return evidence;
    }

    public async Task<DrillingReportingClosureStatus> ReadReportingResponsibilityAsync(ClaimsPrincipal user,string assignmentId)
    {
        var original=await CaptureReportingResponsibilityAsync(user,assignmentId);
        var access=await CaptureExecutionReadAsync(user,original.Original.Source.Basis.ReviewInstanceId);var scope=access.Basis.Scope();
        async Task Guard(){await original.Check();await access.Check();await RequireReportViewAsync(user,scope);}
        var execution=await native.ReadExecutionStatusAsync(access.Basis,Guard);original.Original.RequireSource(execution,false);
        var closing=await original.Owner.ReadRecordedExecutionAssignmentAsync(scope.Company,assignmentId,Guard);
        if(original.NativeRevision!=closing.NativeRevision)throw new InvalidOperationException("The reporting responsibility changed during history reload.");
        var result=new DrillingReportingClosureStatus(PPDMProcessService.CurrentReportingGeneration(closing.Process),execution,PPDMProcessService.RecordedReportingReassignments(closing.Process),PPDMProcessService.RecordedReportingClosure(closing.Process),closing.NativeRevision,"",DateTimeOffset.UtcNow);
        result=result with{Revision=result.ContentRevision()};result.Require(scope.Company,scope.ActorUserId,assignmentId);await access.Confirm();
        var final=await original.Owner.ReadRecordedExecutionAssignmentAsync(scope.Company,assignmentId,Guard);
        if(final.NativeRevision!=closing.NativeRevision)throw new InvalidOperationException("The reporting generation changed during closing status proof.");
        await Guard();return CompanyInventorySourceReview.Copy(result);
    }

    public async Task<DrillingReportingClosureReview> ReviewReportingClosureAsync(ClaimsPrincipal user,string assignmentId,string completionId)
    {
        var original=await CaptureReportingResponsibilityAsync(user,assignmentId);
        var responsibility=PPDMProcessService.CurrentReportingGeneration(original.Process);
        var access=await CaptureExecutionReadAsync(user,original.Original.Source.Basis.ReviewInstanceId);
        var authority=await CaptureReportingClosureAuthorityAsync(user,responsibility,access.Basis,true);
        async Task Guard(){await original.Check();await authority.Check();}
        if(PPDMProcessService.RecordedReportingClosure(original.Process) is not null)throw new InvalidOperationException("This reporting responsibility is already closed. Open its recorded history.");
        var evidence=await ReadReportingClosureEvidenceAsync(user,original.Original,completionId,Guard,authority.Confirm,access);
        var closing=await original.Owner.ReadRecordedExecutionAssignmentAsync(original.Original.Scope().Company,assignmentId,Guard);
        if(original.NativeRevision!=closing.NativeRevision)throw new InvalidOperationException("The pending reporting responsibility changed during closure review.");
        var manager=authority.Manager!;
        var result=new DrillingReportingClosureReview(PPDMProcessService.CurrentReportingGeneration(closing.Process),evidence,PPDMProcessService.ReportingProcessRevision(closing.Process),closing.NativeRevision,
            responsibility.ManagerRole,manager.Persona!,manager.Revision!,"",DateTimeOffset.UtcNow);
        result=result with{Revision=result.ContentRevision()};result.Require();await Guard();return CompanyInventorySourceReview.Copy(result);
    }

    public async Task<DrillingReportingClosureReceipt> CloseReportingAsync(ClaimsPrincipal user,DrillingReportingClosure input,bool recover=false)
    {
        input.Require();var command=CompanyInventorySourceReview.Copy(input);var assignment=command.Review.Responsibility.Root;
        var authority=await CaptureReportingClosureAuthorityAsync(user,command.Review.Responsibility,command.Review.Evidence.Acceptance.Source.History.Execution.Basis,!recover);
        // One complete opening/closing program-facts pair spans this action, as in the existing
        // completion decision owner. Every guard still checks fresh authority/bindings; each
        // evidence phase independently requalifies the accepted graph and physical tests.
        (DrillingExecutionBasis Basis,Func<Task> Check,Func<Task> Confirm)? executionAccess=recover?null:await CaptureExecutionReadAsync(user,assignment.Source.Basis.ReviewInstanceId);
        int evidenceReads=0;
        async Task Guard()
        {
            await authority.Check();
            if(executionAccess is {} access)await access.Check();
            if(authority.Manager is {} manager && (manager.Persona!=command.Review.ManagerPersona || manager.Revision!=command.Review.AuthorityRevision))
                throw new UnauthorizedAccessException("The frozen reporting closure role/workspace authority changed. Recover the retained original.");
        }
        var owner=await ExecutionAssignmentOwner(assignment.Source);
        var receipt=await owner.RecordReportingClosureAsync(command,Guard,
            ()=>ReadReportingClosureEvidenceAsync(user,assignment,command.Review.Evidence.Acceptance.InstanceId,Guard,authority.Confirm,executionAccess,++evidenceReads>1),recover);
        receipt.Require(command);if(!recover && !receipt.Recorded)throw new InvalidOperationException("The original reporting closure is unconfirmed. Recover its retained intent.");
        await authority.Confirm();await Guard();return receipt;
    }
}
