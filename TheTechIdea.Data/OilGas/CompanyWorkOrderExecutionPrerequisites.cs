using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

/// <summary>Fresh executor-owned prerequisite comparison. It records no start, consumption or completion.</summary>
public sealed record CompanyWorkOrderExecutionPrerequisites(CompanyWorkOrderExecutionRequirements Requirements,
    CompanyWorkOrderMaterialReservationFacts Material,CompanyWorkOrderPersonnelAssignmentFacts Personnel,
    IReadOnlyList<CompanyWorkOrderDocumentAcceptanceFacts> Documents,string ApprovedPrerequisiteRevision,
    bool StockSharesCore,DateTime ComparedAtUtc,string Revision)
{
    public static string Digest(CompanyWorkOrderExecutionPrerequisites value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=""}));
    /// <summary>Stable native facts for rechecking a retained command; this digest grants no authority.</summary>
    public string SourceRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Requirements.Context,Requirements.Approval,
        Order=CompanyWorkOrderExecutionRequirements.OrderRevision(Requirements.Order),Rows=Requirements.Requirements.OrderBy(row=>row.CondSeq).ToArray(),
        Requirements.OriginalOrderRevision,Requirements.OriginalRequirementPopulation,Material,Personnel,Documents=Documents.OrderBy(row=>row.Kind,StringComparer.Ordinal).ToArray(),ApprovedPrerequisiteRevision,StockSharesCore}));
    public void Require(CompanyScope company,string actor,CompanyWorkOrderExecutionPermitReference query)
    {
        Requirements.Require(company,actor,query);Material.Require(company,query.Task,Material.IntentId);Personnel.Require();
        if(Requirements.Blockers.Count!=0 || ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc<Requirements.ComparedAtUtc || ComparedAtUtc>=Requirements.Approval.ValidToUtc ||
            Personnel.Company!=company || Personnel.Database!=Requirements.Context.Database || Personnel.Permit!=query || Personnel.ValidFromUtc>Requirements.Approval.ValidFromUtc || Personnel.ValidToUtc<Requirements.Approval.ValidToUtc ||
            Material.ValidFromUtc>Requirements.Approval.ValidFromUtc || Material.ValidToUtc<Requirements.Approval.ValidToUtc ||
            Documents is null || Documents.Count!=2 || !Documents.Select(row=>row.Kind).Order(StringComparer.Ordinal).SequenceEqual(new[]{"ISOLATION","RISK"},StringComparer.Ordinal) ||
            !ProcessDefinitionRevision.IsValid(ApprovedPrerequisiteRevision) || Revision!=Digest(this))throw new InvalidOperationException("Complete current executor-owned prerequisite comparison is unavailable.");
        var material=Requirements.Requirements.Where(row=>row.PermitPrerequisiteKind=="MATERIAL" && row.RegulatoryRef==Material.AllocationId).ToArray();
        var personnel=Requirements.Requirements.Where(row=>row.PermitPrerequisiteKind=="PERSONNEL" && row.PersonnelAcceptance?.PermitIntentId==query.IntentId && row.PersonnelAcceptance.AssignmentId==Personnel.AssignmentId).ToArray();
        if(material.Length!=1 || material[0].MaterialAcceptance is not {} acceptedMaterial || acceptedMaterial.PermitIntentId!=query.IntentId || acceptedMaterial.AllocationId!=Material.AllocationId || acceptedMaterial.AllocationIntentId!=Material.IntentId || acceptedMaterial.ReservationRevision!=Material.Revision ||
            personnel.Length!=1 || personnel[0].PersonnelAcceptance is not {} acceptedPersonnel || acceptedPersonnel.AssignmentRevision!=Personnel.RecordedRevision)
            throw new InvalidOperationException("Exact recorded material/personnel acceptance is not part of current requirements.");
        foreach(var document in Documents)
        {
            var rows=Requirements.Requirements.Where(row=>row.PermitPrerequisiteKind==document.Kind && row.DocumentAcceptance?.PermitIntentId==query.IntentId && row.DocumentAcceptance.ComponentId==document.ComponentId).ToArray();
            if(rows.Length!=1 || rows[0].DocumentAcceptance!=document || document.PermitIntentId!=query.IntentId || document.ContributorEvidenceVersion!=1 ||
                !ProcessDefinitionRevision.IsValid(document.ContributorProof) || document.AcceptedAtUtc.Kind!=DateTimeKind.Utc || document.AcceptedAtUtc>Requirements.Approval.ApprovedAtUtc)
                throw new InvalidOperationException("Exact contributor-aware document acceptance is not part of current requirements.");
        }
    }
}
