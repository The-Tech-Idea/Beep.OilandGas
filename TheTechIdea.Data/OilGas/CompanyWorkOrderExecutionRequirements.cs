using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace TheTechIdea.Data.OilGas;

/// <summary>Current native requirements compared with recorded approval; not execution readiness.</summary>
public sealed record CompanyWorkOrderExecutionRequirements(FieldSelectionReview Context,CompanyWorkOrderRecordedApproval Approval,
    WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements,string OriginalRequirementPopulation,
    string OriginalOrderRevision,DateTime ComparedAtUtc,IReadOnlyList<string> Blockers)
{
    public static string Population(string field,string order,IReadOnlyList<InspectionCondition> rows)
    {
        WorkOrderInspectionRules.RequireProjected(field,order,rows);
        if(rows.Count==0 || rows.Any(row=>!ProcessDefinitionRevision.IsValid(row.RequirementRevision)))throw new InvalidOperationException("Complete immutable native requirements are required for execution comparison.");
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(rows.OrderBy(row=>row.CondSeq).Select(row=>new{row.InspectionId,row.FieldId,row.WorkOrderId,row.CondSeq,row.CondType,row.RequiredText,row.Jurisdiction,row.RegulatoryRef,row.RequirementActor,row.PermitPrerequisiteKind,row.RequirementRevision,row.ApplicableStage}).ToArray()));
    }
    public static string OrderRevision(WorkOrderDetailModel order)
    {
        RecordedWorkOrderComponentChecks.Require(order);var canonical=CompanyInventorySourceReview.Copy(order);
        canonical.Components.Sort((left,right)=>StringComparer.Ordinal.Compare(left.ComponentId,right.ComponentId));
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(canonical));
    }
    public static string[] GetBlockers(CompanyWorkOrderRecordedApproval approval,string original,IReadOnlyList<InspectionCondition> current,string originalOrder,WorkOrderDetailModel order,DateTime utc)
    {
        if(utc==default || utc.Kind!=DateTimeKind.Utc || !ProcessDefinitionRevision.IsValid(original) || !ProcessDefinitionRevision.IsValid(originalOrder))throw new InvalidOperationException("Original order/population proof and explicit current UTC are required.");
        var result=new List<string>();var task=approval.Permit.Task;
        if(originalOrder!=OrderRevision(order))result.Add("Original approved order scope or component population changed; execution review is required.");
        if(original!=Population(task.FieldId,task.WorkOrderId,current))result.Add("Original approved native requirements or applicability changed; execution review is required.");
        if(current.Any(row=>(approval.RequirementEvidenceVersion!=2 || row.ApplicableStage=="PRE_START") && row.Status!="PASS"))result.Add("Current pre-start inspection evidence is not accepted.");
        if(utc<approval.ValidFromUtc)result.Add("Recorded permit validity has not started.");if(utc>=approval.ValidToUtc)result.Add("Recorded permit validity has expired.");return result.ToArray();
    }
    public void Require(CompanyScope company,string actor,CompanyWorkOrderExecutionPermitReference query)
    {
        FieldSelectionReviewValidator.RequireReview(Context,company,actor);Approval.Require(company,query);
        if(Order is null || Requirements is null || Blockers is null || Context.FieldId!=query.Task.FieldId || Order.InstanceId!=query.Task.WorkOrderId || Order.FieldId!=query.Task.FieldId || Order.Source!="WORK_ORDER" || ComparedAtUtc<Approval.ApprovedAtUtc || !Blockers.SequenceEqual(GetBlockers(Approval,OriginalRequirementPopulation,Requirements,OriginalOrderRevision,Order,ComparedAtUtc),StringComparer.Ordinal))throw new InvalidOperationException("Complete original company/actor/order/current requirement comparison is unavailable.");
        RecordedWorkOrderComponentChecks.Require(Order);
        if(Approval.RequirementEvidenceVersion==2)WorkOrderInspectionApplicability.RequireComplete(Requirements);
    }
}
