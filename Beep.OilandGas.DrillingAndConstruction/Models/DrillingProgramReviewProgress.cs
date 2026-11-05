using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.DrillingAndConstruction.Models;
public sealed record DrillingProgramStageProgress([property:JsonRequired]int Number,[property:JsonRequired]string StepId,[property:JsonRequired]string Name,
    [property:JsonRequired]string AssignedRole,[property:JsonRequired]IReadOnlyList<string> PersonaCodes,[property:JsonRequired]string State,
    [property:JsonRequired]string? Reviewer,[property:JsonRequired]string? ReviewerPersona,[property:JsonRequired]string? Decision,[property:JsonRequired]string? Rationale,
    [property:JsonRequired]DateTimeOffset? AtUtc,[property:JsonRequired]Guid? IntentId,[property:JsonRequired]string? ApprovalId,[property:JsonRequired]string? HistoryId);
/// <summary>Complete ordered independent review outcome for an original program participant, read under current company/source authority.</summary>
public sealed record DrillingProgramReviewProgress([property:JsonRequired]DrillingProgramReviewStart Original,[property:JsonRequired]DrillingProgramReviewSource Source,
    [property:JsonRequired]string State,[property:JsonRequired]int PendingTasks,[property:JsonRequired]IReadOnlyList<DrillingProgramStageProgress> Stages,
    [property:JsonRequired]string NativeRevision,[property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    [JsonIgnore]public string InstanceId=>Original.InstanceId();
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Original=Original.Revision(),Source=Source.Revision,State,PendingTasks,Stages,NativeRevision}));
    public bool IsParticipant(string actor)=>actor==Original.Scope().ActorUserId || actor==Original.Source.Program.ROW_CREATED_BY || Original.Workflow.Stages.Any(stage=>stage.Authority.Recipients.Any(row=>row.UserId==actor));
    public void Require(CompanyScope company,string actor,string instance)
    {
        RequireFacts(company,actor,instance);if(!IsParticipant(actor))throw new InvalidOperationException("Only an original program participant can read this history view.");
    }
    public void RequireFacts(CompanyScope company,string actor,string instance)
    {
        Original.RequireSource(Source,false,false);var scope=Source.Basis.Scope();
        if(scope.Company!=company || scope.ActorUserId!=actor || InstanceId!=instance || State is not ("IN_REVIEW" or "APPROVED" or "REJECTED") || Stages is null || Stages.Count!=Original.Workflow.Stages.Count || Stages.Any(stage=>stage is null) ||
            !ProcessDefinitionRevision.IsValid(NativeRevision) || Revision!=ContentRevision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)throw new InvalidOperationException("The complete original company program outcome and current participant source must be confirmed.");
        bool waiting=false,rejected=false;int pending=0;DateTimeOffset? previous=null;
        for(int index=0;index<Stages.Count;index++)
        {
            var row=Stages[index];var expected=Original.Workflow.Stages[index];var configured=expected.Definition;
            if(row.Number!=index+1 || row.StepId!=configured.StepId || row.Name!=configured.StepName || row.AssignedRole!=configured.AssignedRole || row.PersonaCodes is null || !row.PersonaCodes.SequenceEqual(configured.PersonaCodes))throw new InvalidOperationException("The complete ordered program roles and workspaces changed.");
            if(row.State is "APPROVED" or "REJECTED")
            {
                if(waiting || rejected || row.Decision!=(row.State=="APPROVED"?"APPROVE":"REJECT") || !expected.Authority.Recipients.Any(recipient=>recipient.UserId==row.Reviewer && recipient.PersonaCode==row.ReviewerPersona) ||
                    row.Reviewer==Original.Scope().ActorUserId || row.Reviewer==Original.Source.Program.ROW_CREATED_BY || string.IsNullOrWhiteSpace(row.Rationale) || row.Rationale!=row.Rationale.Trim() || row.Rationale.Length>1600 || row.Rationale.Any(c=>char.IsControl(c) && c is not ('\r' or '\n' or '\t')) ||
                    row.AtUtc is not {} at || at.Offset!=TimeSpan.Zero || at<Original.Source.CheckedAtUtc || previous is not null && at<previous || at>CheckedAtUtc || row.IntentId is null || row.IntentId==Guid.Empty)throw new InvalidOperationException("The original independent program decision, reviewer, rationale or UTC audit is unavailable.");
                var digest=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=InstanceId,IntentId=row.IntentId.Value}));
                if(row.ApprovalId!="program-stage-approval-"+digest || row.HistoryId!="program-stage-decision-"+digest)throw new InvalidOperationException("The exact original program approval/history identity changed.");previous=at;rejected=row.State=="REJECTED";
            }
            else
            {
                var expectedState=rejected?"NOT_REACHED":waiting?"WAITING":"PENDING";
                if(row.State!=expectedState || row.Reviewer is not null || row.ReviewerPersona is not null || row.Decision is not null || row.Rationale is not null || row.AtUtc is not null || row.IntentId is not null || row.ApprovalId is not null || row.HistoryId is not null)throw new InvalidOperationException("Undecided program work cannot invent a decision or become approval.");
                if(row.State=="PENDING")pending=configured.PersonaCodes.Count;waiting=true;
            }
        }
        var expectedOutcome=rejected?"REJECTED":waiting?"IN_REVIEW":"APPROVED";
        if(State!=expectedOutcome || PendingTasks!=pending || Stages.Where(stage=>stage.IntentId is not null).Select(stage=>stage.IntentId).Distinct().Count()!=Stages.Count(stage=>stage.IntentId is not null))throw new InvalidOperationException("The program outcome or actual pending task count differs from its complete ordered decisions.");
        if(JsonSerializer.SerializeToUtf8Bytes(this).Length>DrillingProgramReviewStart.MaximumBodyLength)throw new InvalidOperationException("The complete original program outcome exceeds its supported payload bound.");
    }
}
