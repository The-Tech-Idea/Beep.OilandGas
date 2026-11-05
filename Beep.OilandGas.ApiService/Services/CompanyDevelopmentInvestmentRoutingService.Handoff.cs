using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;

namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyDevelopmentInvestmentRoutingService
{
    public async Task<DevelopmentInvestmentHandoffStatus> ReadHandoffStatusAsync(ClaimsPrincipal user,string processId,string afeId)
    {
        var captured=await finance.CaptureInvestmentRecipientAuthorityAsync(user,processId,false);var initial=await captured.ReadSource(afeId);var scope=initial.Current.Source.Review.Scope;
        if(processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(scope.LifecycleDatabase.ConnectionName) is not PPDMProcessService native)throw new InvalidOperationException("The canonical native investment assignment reader is required.");
        async Task<DevelopmentInvestmentHandoffStatus> Read()
        {
            await captured.Guard();var recorded=await native.ReadRecordedInvestmentHandoffAsync(scope.Company,scope.FieldId,afeId,captured.Guard);recorded?.Original.RequireSource(initial,false);await captured.Guard();
            DateTimeOffset? started=null;if(recorded is {} actual){if(actual.Started.Kind==DateTimeKind.Local)throw new InvalidOperationException("The original investment assignment UTC audit is unavailable.");started=new(DateTime.SpecifyKind(actual.Started,DateTimeKind.Utc));}
            var result=new DevelopmentInvestmentHandoffStatus(initial,recorded?.Original.InstanceId(),recorded?.Original.Workflow.ProcessName,recorded?.Original.Scope().ActorUserId,started,recorded is {} progress?PPDMProcessService.InvestmentProgress(progress.Original,progress.Process,progress.Tasks,initial):null,"",DateTimeOffset.UtcNow);
            result=result with{Revision=result.ContentRevision()};result.Require(scope.Company,scope.ActorUserId,processId,afeId);return result;
        }
        var before=await Read();var closing=await Read();if(before.Revision!=closing.Revision)throw new InvalidOperationException("The recorded investment handoff changed during status review.");
        var source=await captured.ReadSource(afeId);if(source.Current.Revision!=initial.Current.Revision || source.Recorded!=initial.Recorded || !source.Blockers.SequenceEqual(initial.Blockers,StringComparer.Ordinal))throw new InvalidOperationException("The original investment source changed during assignment status review.");
        await captured.Confirm();return CompanyInventorySourceReview.Copy(closing);
    }
    public async Task<DevelopmentInvestmentHandoff> PrepareHandoffAsync(ClaimsPrincipal user,string processId,string afeId,DevelopmentInvestmentHandoffSelection selection)
    {
        if(selection is null || !LeaseSummaryValidator.Identifier(selection.ProcessId) || !ProcessDefinitionRevision.IsValid(selection.RoutingRevision))throw new InvalidOperationException("Choose a configured workflow from the current investment routing review.");
        var current=await ReadAsync(user,processId,afeId);if(current.Revision!=selection.RoutingRevision)throw new InvalidOperationException("The investment source, workflow or recipients changed. Inspect routing again.");
        var option=current.Options.SingleOrDefault(item=>item.ProcessId==selection.ProcessId)??throw new InvalidOperationException("The selected investment workflow is unavailable.");
        if(!option.Ready)throw new InvalidOperationException("Every configured investment stage needs current independent eligible recipients.");
        var scope=current.Source.Current.Source.Review.Scope;
        if(processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(scope.LifecycleDatabase.ConnectionName) is not PPDMProcessService native)throw new InvalidOperationException("The canonical lifecycle owner is required for investment assignment.");
        var definition=await native.GetProcessDefinitionAsync(option.ProcessId)??throw new InvalidOperationException("The selected investment definition is unavailable.");
        if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))!=option.DefinitionRevision)throw new InvalidOperationException("The selected investment definition changed.");
        var steps=DevelopmentReviewDefinition.RequireInvestment(definition);var workflow=new DevelopmentReviewWorkflowChoice(definition.ProcessId,definition.ProcessName,definition.Version,option.DefinitionRevision,
            steps.Select(step=>new DevelopmentReviewRoleStep(step.StepId,step.StepName,step.SequenceNumber,step.RequiredRoles[0],option.Stages.Single(stage=>stage.StepId==step.StepId).Recipients.Select(recipient=>recipient.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),step.SlaHours)).ToArray());
        var original=new DevelopmentInvestmentHandoff(current,workflow,Guid.NewGuid());original.Require();var authority=await finance.CaptureInvestmentHandoffAuthorityAsync(user,processId,true);await authority.Confirm();return original;
    }
    public async Task<DevelopmentInvestmentHandoffReceipt> SubmitHandoffAsync(ClaimsPrincipal user,DevelopmentInvestmentHandoff submission,bool recover=false)
    {
        submission.Require();var original=CompanyInventorySourceReview.Copy(submission);var scope=original.Scope();var processId=original.Review.Source.Current.Source.Review.InstanceId;
        var captured=await finance.CaptureInvestmentHandoffAuthorityAsync(user,processId,!recover);
        if(user.ActingUserId()!=scope.ActorUserId || company.Company.Scope!=scope.Company)throw new UnauthorizedAccessException("The original investment handoff belongs to another company or sender.");
        async Task Guard()
        {
            await captured.Guard();
            if(!recover)
                foreach(var expected in original.Option().Stages)
                {
                    var current=await ReadRecipients(original.Review.Source,expected.StepId,expected.AssignedRole,captured.Guard);
                    if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The original independent investment recipient authority changed. Recover the original outcome before preparing another handoff.");
                }
            await captured.Guard();
        }
        async Task<DevelopmentInvestmentReview> ReadSource()
        {
            await captured.Confirm();var current=await finance.ReviewInvestmentAsync(user,processId,original.Review.Source.AfeId);original.RequireSource(current);
            if(!recover && (current.BlocksRouting() || current.Current.Revision!=original.Review.Source.Current.Revision))throw new InvalidOperationException("The original investment source is no longer ready for this handoff.");
            await captured.Confirm();return current;
        }
        await Guard();
        if(processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(scope.LifecycleDatabase.ConnectionName) is not PPDMProcessService native)throw new InvalidOperationException("The canonical native investment lifecycle owner is required.");
        var receipt=recover?await native.RecoverInvestmentHandoffAsync(original,Guard,ReadSource):await native.StartInvestmentHandoffAsync(original,Guard,ReadSource);
        receipt.Require(original);if(!recover && !receipt.Recorded)throw new InvalidOperationException("The original investment assignment was not confirmed recorded. Recover its retained input.");return receipt;
    }
}
