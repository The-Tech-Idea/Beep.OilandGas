using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyDrillingProgramService
{
    private async Task<DrillingExecutionAssignmentOption> ExecutionReportingOptionAsync(ProcessDefinition definition,DrillingExecutionStatus source,Func<Task> guard)
    {
        await guard();var step=DevelopmentReviewDefinition.RequireExecutionReporting(definition);
        var authority=await recipients.ReadExecutionReportingAsync(source,step.StepId,step.RequiredRoles[0]);
        var option=new DrillingExecutionAssignmentOption(definition.ProcessId,definition.ProcessName,definition.Version,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)),
            new(step.StepId,step.StepName,step.SequenceNumber,step.RequiredRoles[0],authority.Recipients.Select(row=>row.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),step.SlaHours),authority);
        option.Require();await guard();return option;
    }

    private async Task<IReadOnlyList<DrillingExecutionAssignmentOption>> ExecutionReportingOptionsAsync(PPDMProcessService owner,DrillingExecutionStatus source,Func<Task> guard)
    {
        await guard();var definitions=CompanyInventorySourceReview.Copy(await owner.GetProcessDefinitionsByTypeAsync("DRILLING")??throw new InvalidOperationException("The complete configured reporting catalog is unavailable."));
        if(definitions.Count>128 || definitions.Any(row=>row is null || !row.IsActive || row.ProcessType!="DRILLING" || !LeaseSummaryValidator.Identifier(row.ProcessId)) || definitions.Select(row=>row.ProcessId).Distinct(StringComparer.Ordinal).Count()!=definitions.Count)
            throw new InvalidOperationException("The complete configured reporting catalog is ambiguous or exceeds its bound.");
        var options=new List<DrillingExecutionAssignmentOption>();
        foreach(var definition in definitions.Where(row=>row.EntityType=="DRILLING_OPERATION").OrderBy(row=>row.ProcessId,StringComparer.Ordinal))
            options.Add(await ExecutionReportingOptionAsync(definition,source,guard));
        await guard();return options;
    }

    private sealed record ReportingTransferRead(DrillingReportingReassignmentCatalog Catalog,PPDMProcessService Owner,DrillingExecutionBasis Basis,Func<Task> Check,Func<Task> Confirm);

    public async Task<DrillingReportingReassignmentCatalog> ReadReportingReassignmentCatalogAsync(ClaimsPrincipal user,string assignmentId)
        =>(await CaptureReportingTransferCatalogAsync(user,assignmentId,true)).Catalog;

    private async Task<ReportingTransferRead> CaptureReportingTransferCatalogAsync(ClaimsPrincipal user,string assignmentId,bool closeSource)
    {
        var recorded=await CaptureReportingResponsibilityAsync(user,assignmentId,true);
        if(PPDMProcessService.RecordedReportingClosure(recorded.Process) is not null)throw new InvalidOperationException("Closed reporting responsibility cannot be transferred.");
        var access=await CaptureExecutionReadAsync(user,recorded.Original.Source.Basis.ReviewInstanceId);
        var authority=await CaptureReportingManagementAuthorityAsync(user,access.Basis,true);
        async Task Guard(){await recorded.Check();await access.Check();await authority.Check();}
        async Task Confirm(){await access.Confirm();await authority.Confirm();await Guard();}
        var source=await native.ReadExecutionStatusAsync(access.Basis,Guard);recorded.Original.RequireSource(source,false);
        var well=await native.ReadProgramWellAsync(access.Basis.Scope().CoreDatabase,access.Basis.Scope().FieldId,access.Basis.Source.Basis.Well.Uwi,true,Guard);
        if(well!=access.Basis.Source.Basis.Well)throw new InvalidOperationException("The original active reporting well changed.");
        var first=await ExecutionReportingOptionsAsync(recorded.Owner,source,Guard);var closing=await ExecutionReportingOptionsAsync(recorded.Owner,source,Guard);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The configured reporting workflows or complete receiving authority changed.");
        var execution=await native.ReadExecutionStatusAsync(access.Basis,Guard);
        if(execution.Revision!=source.Revision)throw new InvalidOperationException("The recorded execution changed during transfer catalog loading.");
        if(closeSource)await Confirm();
        var graph=await recorded.Owner.ReadRecordedExecutionAssignmentAsync(access.Basis.Scope().Company,assignmentId,Guard);
        if(graph.NativeRevision!=recorded.NativeRevision)throw new InvalidOperationException("The current reporting generation changed during transfer catalog loading.");
        var manager=authority.Manager!;var result=new DrillingReportingReassignmentCatalog(PPDMProcessService.CurrentReportingGeneration(graph.Process),execution,closing,manager.Roles,manager.Persona!,manager.Revision!,graph.NativeRevision,"",DateTimeOffset.UtcNow);
        result=result with{Revision=result.ContentRevision()};result.Require(access.Basis.Scope().Company,access.Basis.Scope().ActorUserId,assignmentId);await Guard();
        return new(CompanyInventorySourceReview.Copy(result),recorded.Owner,access.Basis,Guard,Confirm);
    }

    public async Task<DrillingReportingReassignmentReview> ReviewReportingReassignmentAsync(ClaimsPrincipal user,DrillingReportingReassignmentSelection selection)
    {
        // The public catalog closes its own source pair. Within one review action,
        // keep the same opening reader until the native owner's closing callback;
        // nesting another complete catalog pair would repeat whole program walks.
        selection.Require();var read=await CaptureReportingTransferCatalogAsync(user,selection.AssignmentId,false);var catalog=read.Catalog;
        if(catalog.Revision!=selection.CatalogRevision || !catalog.ManagerRoles.Contains(selection.ManagerRole,StringComparer.Ordinal))throw new InvalidOperationException("Reload the current transfer catalog before reviewing this selection.");
        var destination=catalog.Options.SingleOrDefault(row=>row.ProcessId==selection.ProcessId && row.Ready)??throw new InvalidOperationException("Select one current configured receiving workflow with eligible recipients.");
        async Task Guard()=>await read.Check();
        var owner=read.Owner;int reads=0;
        async Task<DrillingExecutionStatus> Execution()
        {
            await Guard();var source=await native.ReadExecutionStatusAsync(read.Basis,Guard);
            if(source.Revision!=catalog.Execution.Revision)throw new InvalidOperationException("The displayed transfer execution changed.");
            var well=await native.ReadProgramWellAsync(read.Basis.Scope().CoreDatabase,read.Basis.Scope().FieldId,read.Basis.Source.Basis.Well.Uwi,true,Guard);
            if(well!=read.Basis.Source.Basis.Well)throw new InvalidOperationException("The original active transfer well changed.");
            if(++reads>1)await read.Confirm();await Guard();return source;
        }
        async Task<DrillingExecutionAssignmentOption> Destination()
        {
            var definition=await owner.GetProcessDefinitionAsync(destination.ProcessId)??throw new InvalidOperationException("The configured reporting workflow is unavailable.");
            return await ExecutionReportingOptionAsync(definition,catalog.Execution,Guard);
        }
        var result=await owner.ReviewReportingReassignmentAsync(catalog.Scope().Company,selection.AssignmentId,destination,selection.ManagerRole,catalog.ManagerPersona,catalog.AuthorityRevision,Guard,Execution,Destination);
        if(result.Responsibility.ContentRevision()!=catalog.Responsibility.ContentRevision() || result.NativeRevision!=catalog.NativeRevision)throw new InvalidOperationException("The displayed reporting generation changed before transfer review.");
        result.Require();await Guard();return result;
    }

    public async Task<DrillingReportingReassignmentReceipt> ReassignReportingAsync(ClaimsPrincipal user,DrillingReportingReassignment input,bool recover=false)
    {
        input.Require();var command=CompanyInventorySourceReview.Copy(input);var basis=command.Review.Execution.Basis;
        var authority=await CaptureReportingManagementAuthorityAsync(user,basis,!recover,command.Review.ManagerRole);
        var access=recover?((DrillingExecutionBasis Basis,Func<Task> Check,Func<Task> Confirm)?)null:await CaptureExecutionReadAsync(user,basis.ReviewInstanceId);
        async Task Guard()
        {
            await authority.Check();if(access is {} current)await current.Check();
            if(authority.Manager is {} manager && (manager.Persona!=command.Review.ManagerPersona || manager.Revision!=command.Review.AuthorityRevision))throw new UnauthorizedAccessException("The frozen transfer management authority changed. Recover the retained original.");
        }
        var owner=await ExecutionAssignmentOwner(command.Review.Execution);int reads=0;
        async Task<DrillingExecutionStatus> Execution()
        {
            var current=access??throw new InvalidOperationException("Recovery must not read live transfer execution.");await Guard();
            var source=await native.ReadExecutionStatusAsync(current.Basis,Guard);
            var well=await native.ReadProgramWellAsync(current.Basis.Scope().CoreDatabase,current.Basis.Scope().FieldId,current.Basis.Source.Basis.Well.Uwi,true,Guard);
            if(well!=current.Basis.Source.Basis.Well)throw new InvalidOperationException("The original active transfer well changed.");
            if(++reads>1){await current.Confirm();await authority.Confirm();}await Guard();return source;
        }
        async Task<DrillingExecutionAssignmentOption> Destination()
        {
            if(recover)throw new InvalidOperationException("Recovery must not open the live receiving workflow.");
            var definition=await owner.GetProcessDefinitionAsync(command.Review.Destination.ProcessId)??throw new InvalidOperationException("The configured reporting workflow is unavailable.");
            return await ExecutionReportingOptionAsync(definition,command.Review.Execution,Guard);
        }
        var result=await owner.RecordReportingReassignmentAsync(command,Guard,Execution,Destination,recover);result.Require(command);
        if(!recover && !result.Recorded)throw new InvalidOperationException("The original reporting transfer is unconfirmed. Recover its retained intent.");
        await authority.Confirm();await Guard();return result;
    }
}
