using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace TheTechIdea.Data.OilGas;

/// <summary>Original-start/current inspection comparison only; never submitted work or financial completion.</summary>
public sealed record CompanyWorkOrderSubmissionInspectionReview(CompanyWorkOrderExecutionRequirements Current,
    CompanyWorkOrderRecordedStart Start,DateTime ComparedAtUtc,IReadOnlyList<string> Blockers,string Revision)
{
    public static CompanyWorkOrderSubmissionInspectionReview Create(CompanyWorkOrderExecutionStartRecovery original,
        CompanyWorkOrderExecutionRequirements current,DateTime utc)
    {
        var context=current.Context;var command=original.Command;var query=current.Approval.Permit;
        original.Require(context.Company,context.ActorUserId,new(query,command.IntentId));current.Require(context.Company,context.ActorUserId,query);
        var before=command.Review.Prerequisites.Requirements;
        if(before.Context.Database!=context.Database || before.Approval!=current.Approval || before.OriginalOrderRevision!=current.OriginalOrderRevision || before.OriginalRequirementPopulation!=current.OriginalRequirementPopulation)
            throw new InvalidOperationException("Original start and current immutable approved order/requirement proof differ.");
        var row=original.Recorded;
        var start=new CompanyWorkOrderRecordedStart(context.Company,query,command.IntentId,row.START_ID,row.PERMIT_ID,row.EXECUTOR_USER_ID,row.EMPLOYEE_BA_ID,command.Review.Stage.AssignedRole,command.Review.Stage.StepId,row.STARTED_AT_UTC,row.ROW_CREATED_DATE!.Value,row.PREREQUISITE_REVISION,row.RECORDED_REVISION){QualifiedWorkAssets=command.Review.QualifiedWorkAssets};
        var result=new CompanyWorkOrderSubmissionInspectionReview(CompanyInventorySourceReview.Copy(current),start,utc,GetBlockers(current,start,utc),"");
        result=result with{Revision=Digest(result)};result.Require(context.Company,context.ActorUserId,new(query,command.IntentId));return result;
    }
    public static string Digest(CompanyWorkOrderSubmissionInspectionReview value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=""}));
    public string SourceRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Current.Context,Current.Approval,Order=CompanyWorkOrderExecutionRequirements.OrderRevision(Current.Order),Requirements=Current.Requirements.OrderBy(row=>row.CondSeq).ToArray(),Current.OriginalRequirementPopulation,Current.OriginalOrderRevision,Start,Blockers}));
    private static string[] GetBlockers(CompanyWorkOrderExecutionRequirements current,CompanyWorkOrderRecordedStart start,DateTime utc)
    {
        if(utc.Kind!=DateTimeKind.Utc || utc<current.ComparedAtUtc || utc<start.RecordedAtUtc)throw new InvalidOperationException("Explicit current UTC after the recorded start and inspection comparison is required.");
        var blockers=current.Blockers.ToList();
        if(current.Approval.RequirementEvidenceVersion!=2){blockers.Add("Original permit has no explicit approved work-submission applicability; its historical requirements cannot be reclassified.");return blockers.ToArray();}
        WorkOrderInspectionApplicability.RequireComplete(current.Requirements);
        var applicable=current.Requirements.Where(row=>row.ApplicableStage=="WORK_SUBMISSION").ToArray();
        if(applicable.Length==0)blockers.Add("No explicitly approved work-submission inspection requirements are configured.");
        if(applicable.Any(row=>row.Status!="PASS"))blockers.Add("Current work-submission inspection evidence is not accepted.");
        if(applicable.Any(row=>row.Status=="PASS" && (row.InspectDate is not {} at || at.Kind!=DateTimeKind.Utc || at<start.StartedAtUtc || at>utc || at>=current.Approval.ValidToUtc)))
            blockers.Add("Work-submission inspection UTC must follow the actual start and remain inside the original permit window.");
        return blockers.ToArray();
    }
    public void Require(CompanyScope company,string actor,CompanyWorkOrderExecutionStartReference reference)
    {
        reference.Require();Current.Require(company,actor,reference.Permit);Start.Require(company,reference.Permit);
        if(Start.QualifiedWorkAssets is {} assets)
        {
            if(assets.Context.Database!=Current.Context.Database)throw new InvalidOperationException("Original qualified work-start CORE differs from current submission.");
            CompanyMaintenanceQualifiedWorkScope.RequireSnapshot(assets,Current.Order,assets.Context,Start.StartedAtUtc);
        }
        if(Start.IntentId!=reference.IntentId || Start.ExecutorUserId!=actor || !Blockers.SequenceEqual(GetBlockers(Current,Start,ComparedAtUtc),StringComparer.Ordinal) || Revision!=Digest(this))
            throw new InvalidOperationException("Complete original executor/start/current inspection comparison is unavailable or differs.");
    }
}
