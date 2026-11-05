using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.DrillingAndConstruction.Models;

public sealed record DrillingCompletionAssignedReview(
    [property:JsonRequired]DrillingCompletionReviewStart Original,
    [property:JsonRequired]DrillingDailyReportPage Source,
    [property:JsonRequired]string TaskId,
    [property:JsonRequired]string StepInstanceId,
    [property:JsonRequired]string AssignedRole,
    [property:JsonRequired]string PersonaCode,
    [property:JsonRequired]string ProcessRevision,
    [property:JsonRequired]string TaskRevision,
    [property:JsonRequired]string NativeRevision,
    [property:JsonRequired]string Revision,
    [property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    [JsonIgnore]public string InstanceId=>Original.InstanceId();
    [JsonIgnore]public bool IsCurrentClaim=>Source.History.ReportedCompletion?.ReportId==Original.ClaimId;
    [JsonIgnore]public int StageNumber=>StageIndex()+1;
    public int StageIndex()
    {
        var matches=Original.Workflow.Stages.Select((stage,index)=>(stage,index)).Where(item=>DrillingCompletionReviewStartReceipt.StepId(InstanceId,item.stage.Definition.StepId)==StepInstanceId).ToArray();
        if(matches.Length!=1)throw new InvalidOperationException("The exact configured completion review stage is unavailable.");return matches[0].index;
    }
    public DrillingCompletionReviewStage Stage()=>Original.Workflow.Stages[StageIndex()];
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Original=Original.Revision(),Source=Source.Revision,TaskId,StepInstanceId,AssignedRole,PersonaCode,ProcessRevision,TaskRevision,NativeRevision}));
    public void Require(CompanyScope company,string actor,string id)
    {
        Original.RequireSource(Source,false,false);var scope=Source.History.Execution.Basis.Scope();var stage=Stage();
        if(scope.Company!=company || scope.ActorUserId!=actor || InstanceId!=id || actor==Original.Scope().ActorUserId || actor==Original.Source.ReportedCompletion!.RecorderUserId ||
            !stage.Authority.Recipients.Any(row=>row.UserId==actor && row.PersonaCode==PersonaCode) || AssignedRole!=stage.Definition.AssignedRole ||
            StepInstanceId!=DrillingCompletionReviewStartReceipt.StepId(id,stage.Definition.StepId) || TaskId!=DrillingCompletionReviewStartReceipt.TaskId(id,StepInstanceId,PersonaCode) ||
            !ProcessDefinitionRevision.IsValid(ProcessRevision) || !ProcessDefinitionRevision.IsValid(TaskRevision) || !ProcessDefinitionRevision.IsValid(NativeRevision) ||
            CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || Revision!=ContentRevision())
            throw new InvalidOperationException("The original independent completion task, recipient/workspace and checked claim are unconfirmed.");
    }
}
