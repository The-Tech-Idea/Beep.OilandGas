using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.DrillingAndConstruction.Models;
/// <summary>Actual pending native task and original saved program admitted for its independent current recipient.</summary>
public sealed record DrillingProgramAssignedReview([property:JsonRequired]DrillingProgramReviewStart Original,[property:JsonRequired]DrillingProgramReviewSource Source,
    [property:JsonRequired]string StepInstanceId,[property:JsonRequired]string AssignedRole,[property:JsonRequired]string PersonaCode,[property:JsonRequired]string TaskId,
    [property:JsonRequired]string ProcessRevision,[property:JsonRequired]string TaskRevision,[property:JsonRequired]string NativeRevision,
    [property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    [JsonIgnore]public string InstanceId=>Original.InstanceId();
    [JsonIgnore]public int StageNumber=>StageIndex()+1;
    public int StageIndex()
    {
        var matches=Original.Workflow.Stages.Select((stage,index)=>(stage,index)).Where(item=>DrillingProgramReviewStartReceipt.StepId(InstanceId,item.stage.Definition.StepId)==StepInstanceId).ToArray();
        if(matches.Length!=1)throw new InvalidOperationException("The exact configured program review stage is unavailable.");return matches[0].index;
    }
    public DrillingProgramReviewStage Stage()=>Original.Workflow.Stages[StageIndex()];
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Original=Original.Revision(),Source=Source.Revision,StepInstanceId,AssignedRole,PersonaCode,TaskId,ProcessRevision,TaskRevision,NativeRevision}));
    public void Require(CompanyScope company,string actor,string instance)
    {
        Original.RequireSource(Source,false,false);var scope=Source.Basis.Scope();var first=Stage();
        if(scope.Company!=company || scope.ActorUserId!=actor || InstanceId!=instance || actor==Original.Scope().ActorUserId || actor==Original.Source.Program.ROW_CREATED_BY ||
            StepInstanceId!=DrillingProgramReviewStartReceipt.StepId(instance,first.Definition.StepId) || AssignedRole!=first.Definition.AssignedRole || !first.Authority.Recipients.Any(row=>row.UserId==actor && row.PersonaCode==PersonaCode) ||
            TaskId!="task-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Process=instance,Step=StepInstanceId,Persona=PersonaCode})) || !ProcessDefinitionRevision.IsValid(ProcessRevision) || !ProcessDefinitionRevision.IsValid(TaskRevision) || !ProcessDefinitionRevision.IsValid(NativeRevision) ||
            Revision!=ContentRevision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)throw new InvalidOperationException("The exact original program, pending task, independent current account and workspace must be confirmed.");
    }
}
