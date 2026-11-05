using System.Text.Json;
namespace TheTechIdea.Data.OilGas;
public sealed record CompanyPermitDocumentContributorFacts(string CreatorId,string? PersonBaId,string? CreatorType,string? ReportedBaType,string? ReportedName,DateTime? EffectiveFromUtc,DateTime? EffectiveToUtc,string? ActualBaType,DateTime? PersonEffectiveFromUtc,DateTime? PersonEffectiveToUtc,string? PersonRevision,string Revision);
public sealed record CompanyWorkOrderDocumentContributors(CompanyWorkOrderPermitDocumentFacts Source,IReadOnlyList<CompanyPermitDocumentContributorFacts> Contributors,DateTime ComparedAtUtc,string Revision)
{
    public void Require()
    {
        Source.Require();if(Contributors is null || Contributors.Count>256 || Contributors.Any(row=>row is null || !LeaseSummaryValidator.Identifier(row.CreatorId) || !ProcessDefinitionRevision.IsValid(row.Revision) || row.PersonRevision is not null && !ProcessDefinitionRevision.IsValid(row.PersonRevision)) || Contributors.Select(row=>row.CreatorId).Distinct(StringComparer.Ordinal).Count()!=Contributors.Count || ComparedAtUtc==default || ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc<Source.ComparedAtUtc || Revision!=Digest(this))throw new InvalidOperationException("Complete current recorded document contributor population is unavailable.");
    }
    public static string Digest(CompanyWorkOrderDocumentContributors result)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(result with{Revision=""}));
    public static CompanyWorkOrderDocumentContributors Create(CompanyWorkOrderPermitDocumentFacts source,IReadOnlyList<CompanyPermitDocumentContributorFacts> people,DateTime utc){var result=new CompanyWorkOrderDocumentContributors(source,people,utc,"");result=result with{Revision=Digest(result)};result.Require();return result;}
    public static string EvidenceProof(CompanyWorkOrderDocumentContributors result)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Source=CompanyWorkOrderPermitDocumentFacts.EvidenceProof(result.Source),result.Contributors}));
}
public sealed record CompanyWorkOrderDocumentContributorReview(CompanyWorkOrderPermitDocumentReview Document,CompanyWorkOrderDocumentContributors Contributors,CompanyPersonnelIdentityCatalog Identities,CompanyNativePersonFacts? ReviewerPerson,DateTime ComparedAtUtc,IReadOnlyList<string> Blockers)
{
    public static string[] GetBlockers(CompanyWorkOrderPermitDocumentReview document,CompanyWorkOrderDocumentContributors contributors,CompanyPersonnelIdentityCatalog identities,CompanyNativePersonFacts? reviewer,DateTime utc,string? reviewerUserId=null)
    {
        document.Require();contributors.Require();identities.Require();var permit=document.PermitReview.Proposal;var actor=reviewerUserId??document.PermitReview.Context.ActorUserId;var result=document.Blockers.Where(value=>value!=CompanyWorkOrderPermitDocumentReview.PendingAcceptanceBlocker).ToList();
        var links=identities.Versions.Where(row=>row.UserId==actor && row.EffectiveFromUtc<=utc && row.EffectiveToUtc>utc).ToArray();
        if(links.Length!=1 || reviewer is null)result.Add("One authoritative current company account/native person identity is required for the reviewer.");
        else{var link=links[0];reviewer.Require(document.PermitReview.Context.Company,link.PersonBaId);if(link.RecordedBy==actor || link.PersonType!=reviewer.Type || !Covers(link.EffectiveFromUtc,link.EffectiveToUtc) || !Covers(reviewer.EffectiveFromUtc,reviewer.EffectiveToUtc) || !Covers(reviewer.Employment.EffectiveDate,reviewer.Employment.ExpiryDate))result.Add("Independent reviewer identity/native person relationship/type/effective UTC does not establish the full permit window.");}
        if(contributors.Contributors.Count==0)result.Add("No complete native document contributor population is recorded; names or recording accounts do not prove independence.");
        foreach(var person in contributors.Contributors)
        {
            if(!LeaseSummaryValidator.Identifier(person.PersonBaId) || string.IsNullOrWhiteSpace(person.CreatorType) || string.IsNullOrWhiteSpace(person.ReportedBaType) || person.PersonRevision is null || string.IsNullOrWhiteSpace(person.ActualBaType) || person.ReportedBaType!=person.ActualBaType || !Covers(person.EffectiveFromUtc,person.EffectiveToUtc) || !Covers(person.PersonEffectiveFromUtc,person.PersonEffectiveToUtc))result.Add($"Contributor {person.CreatorId} identity/type/effective UTC is unconfirmed; no name or role mapping is inferred.");
            if(reviewer is not null && person.ActualBaType!=reviewer.Type)result.Add($"Contributor {person.CreatorId} native type does not match the explicitly verified person type; group/company identities are not inferred to exclude the reviewer.");
            if(reviewer is not null && person.PersonBaId==reviewer.PersonBaId)result.Add($"Reviewer is recorded contributor {person.CreatorId}; contributor independence is not established.");
        }
        result.Add(CompanyWorkOrderPermitDocumentReview.PendingAcceptanceBlocker);return result.ToArray();
        bool Covers(DateTime? start,DateTime? end)=>start is {} from && end is {} to && from.Kind==DateTimeKind.Utc && to.Kind==DateTimeKind.Utc && from<=permit.ValidFromUtc && to>=permit.ValidToUtc && from<=utc && to>utc;
    }
    public void Require()
    {
        Document.Require();Contributors.Require();Identities.Require();var context=Document.PermitReview.Context;
        if(ComparedAtUtc==default || ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc<Contributors.ComparedAtUtc || ComparedAtUtc<Identities.ComparedAtUtc || Identities.Context.Company!=context.Company || Identities.Context.Database!=context.Database || Identities.Context.ActorUserId!=context.ActorUserId || CompanyWorkOrderPermitDocumentFacts.CurrentProof(Contributors.Source)!=CompanyWorkOrderPermitDocumentFacts.CurrentProof(Document.Source) || Blockers is null || !Blockers.SequenceEqual(GetBlockers(Document,Contributors,Identities,ReviewerPerson,ComparedAtUtc),StringComparer.Ordinal))throw new InvalidOperationException("Complete original document/contributor/reviewer mapping comparison changed.");
    }
}
public interface ICompanyWorkOrderDocumentContributorOwner
{
    Task<CompanyWorkOrderDocumentContributors> ReadDocumentContributorsAsync(FieldSelectionReview context,CompanyWorkOrderPermitDocumentQuery query,string expectedReference,Func<Task> authority);
}

/// <summary>Complete native document proof inside an already owned serializable core transaction.</summary>
public interface ICompanyWorkOrderDocumentContributorCore
{
    Task<CompanyWorkOrderDocumentContributors> ReadDocumentContributorsCoreAsync(FieldSelectionReview context,CompanyWorkOrderPermitDocumentQuery query,string expectedReference,Func<Task> authority);
    Task RequireApprovalDocumentCoreAsync(FieldSelectionReview context,CompanyWorkOrderAcceptedDocumentReview expected,Func<Task> authority);
    Task RequireExecutionDocumentCoreAsync(FieldSelectionReview executor,CompanyWorkOrderAcceptedDocumentReview expected,Func<Task> authority);
}
