using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.Models.Data.WorkOrder;

/// <summary>Explicit inspection acceptance of reported prerequisite references; inventory availability is separate.</summary>
public sealed record CompanyWorkOrderPermitPrerequisiteEvidence(string Kind,string ReportedReference,
    InspectionCondition? Inspection,bool Accepted,string State)
{
    public static IReadOnlyList<CompanyWorkOrderPermitPrerequisiteEvidence> Read(CompanyWorkOrderExecutionPermitIntent proposal,CompanyMaintenanceInspectionReview source,DateTime utc)
    {
        proposal.Require();source.Require(proposal.Context.Company,source.Stage.Context.ActorUserId,proposal.Task);if(utc==default || utc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("A recorded UTC prerequisite comparison time is required.");
        var result=new List<CompanyWorkOrderPermitPrerequisiteEvidence>();
        foreach(var pair in new[]{("ISOLATION",proposal.IsolationReference),("RISK",proposal.RiskReference),("PERSONNEL",proposal.PersonnelReference),("MATERIAL",proposal.MaterialReference)})
        {
            var candidates=source.Inspections.Where(row=>row.PermitPrerequisiteKind==pair.Item1 && row.RegulatoryRef==pair.Item2 && row.Jurisdiction==proposal.Jurisdiction).ToArray();
            if(candidates.Length!=1){result.Add(new(pair.Item1,pair.Item2,null,false,candidates.Length==0?"No exact recorded prerequisite inspection":"Ambiguous prerequisite inspection references"));continue;}
            var row=candidates[0];var accepted=row.Status=="PASS" && row.Inspector!=proposal.Context.ActorUserId && row.InspectDate is {} when && when<=utc;
            var state=accepted?"Recorded independent inspection acceptance":row.Status!="PASS"?"Inspection result is not accepted":row.Inspector==proposal.Context.ActorUserId?"The permit issuer cannot supply independent acceptance":"Inspection evidence time is after the comparison";
            result.Add(new(pair.Item1,pair.Item2,row,accepted,state));
        }
        return result.AsReadOnly();
    }
}
