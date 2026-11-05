using System.Text.Json;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService:ICompanyMaintenanceReturnHandoffOwner
{
    public async Task<CompanyMaintenanceReturnHandoffReview> ReviewReturnHandoffAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,CompanyWorkOrderCompletionReview completion,Func<string?,Task> authority)
        =>await ReviewReturnHandoffCoreAsync(context,query,completion,authority,false);
    private async Task<CompanyMaintenanceReturnHandoffReview> ReviewReturnHandoffCoreAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,CompanyWorkOrderCompletionReview completion,Func<string?,Task> authority,bool owned)
    {
        completion=CompanyInventorySourceReview.Copy(completion);query.Require();completion.Require();
        if(completion.Report.Query!=query || completion.Context.Company!=context.Company || completion.Context.ActorUserId!=context.ActorUserId || completion.Context.FieldId!=context.FieldId)
            throw new UnauthorizedAccessException("Current independent reviewer/company/FIELD/report must match the configured return review.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original return review lifecycle source is unavailable.");
        void Transaction(){if(owned)PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
        async Task Guard(string? role){Transaction();await authority(role);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original return review lifecycle source changed.");Transaction();}
        async Task<CompanyMaintenanceReturnHandoffReview> Read()
        {
            await Guard(null);var stage=await ReviewExecutionReviewerStageAsync(context,query.Task,Guard);
            var proof=await ReadCompletionDecisionEvidence(context,query)??throw new InvalidOperationException("Record independent verification before reviewing the configured return approval role.");
            var decision=CompanyWorkOrderCompletionDecisionFacts.From(CompletionDecisionResult(proof));
            var owner=await ReadExecutionHandoffOwner(context,query.Task);var templates=owner.Definition.Steps.OrderBy(row=>row.SequenceNumber).ToArray();var index=Array.FindIndex(templates,row=>row.StepId==stage.StepId);
            if(index!=2 || index+1>=templates.Length || templates[index+1].StepType!="APPROVAL" || !templates[index+1].RequiresApproval || templates[index+1].RequiredRoles.Count!=1)
                throw new InvalidOperationException("Configure an explicit immediately following return approval step and one responsible role.");
            var next=owner.Instance.StepInstances.Single(row=>row.StepId==templates[index+1].StepId);
            var destinations=(await _personaReader!.GetActivePersonasAsync(next.RequiredRole!)).ToArray();
            if(destinations.Length is <1 or >128 || destinations.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>64) || destinations.Distinct(StringComparer.Ordinal).Count()!=destinations.Length)throw new InvalidOperationException("A complete unique active persona population is required for the configured return role.");
            if(owner.Instance.Status!=ProcessStatus.IN_PROGRESS || next.Status!=StepStatus.BLOCKED || next.StartDate is not null || next.CompletionDate is not null || !string.IsNullOrEmpty(next.CompletedBy) || !string.IsNullOrEmpty(next.AssignedTo) || next.RequiredRole!=templates[index+1].RequiredRoles[0])
                throw new InvalidOperationException("The future return approval assignment must remain blocked and match its original configured role.");
            var graph=await MaintenanceAcceptanceNativeGraph(query.Task.InstanceId,owner.Start.ProcessId);
            if(ProcessDefinitionRevision.FromJson(graph)!=stage.ProcessRevision)throw new InvalidOperationException("Original configured review graph changed around the recorded decision.");
            var result=new CompanyMaintenanceReturnHandoffReview(stage,decision,completion,next.StepId,next.StepInstanceId,next.RequiredRole!);result.Require();await Guard(stage.AssignedRole);return result;
        }
        var first=await Read();var closing=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Recorded verification or configured return role changed during observation.");await Guard(closing.Stage.AssignedRole);return closing;
    }
}
