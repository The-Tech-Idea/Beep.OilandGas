using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Security.Cryptography;
namespace Beep.OilandGas.Models.Data.WorkOrder;
public sealed record CompanyWorkOrderPermitDocumentQuery(CompanyWorkOrderExecutionPermitReference Permit,string Kind,string ComponentId)
{
    public void Require(){if(Permit is null || Kind is not ("ISOLATION" or "RISK") || !LeaseSummaryValidator.Identifier(ComponentId))throw new ArgumentException("Select the original permit, document prerequisite kind and exact recorded order component.");Permit.Require();}
}
public sealed record CompanyWorkOrderDocumentLink(string ComponentId,string InformationItemId,string InformationItemSubtype,DateTime? EffectiveFrom,DateTime? EffectiveTo,string Revision);
public sealed record CompanyWorkOrderDocumentCatalog(FieldSelectionReview Context,CompanyWorkOrderExecutionPermitReference Permit,IReadOnlyList<CompanyWorkOrderDocumentLink> Links,string PopulationRevision,DateTime ComparedAtUtc)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderExecutionPermitReference permit)
    {
        permit.Require();FieldSelectionReviewValidator.RequireReview(Context,company,actor);
        if(Permit!=permit || Context.FieldId!=permit.Task.FieldId || Links is null || Links.Count>4096 || Links.Select(row=>row.ComponentId).Distinct(StringComparer.Ordinal).Count()!=Links.Count || !ProcessDefinitionRevision.IsValid(PopulationRevision) || ComparedAtUtc==default || ComparedAtUtc.Kind!=DateTimeKind.Utc || Links.Any(row=>!LeaseSummaryValidator.Identifier(row.ComponentId) || !LeaseSummaryValidator.Identifier(row.InformationItemId) || !LeaseSummaryValidator.Identifier(row.InformationItemSubtype) || !ProcessDefinitionRevision.IsValid(row.Revision)))throw new InvalidOperationException("Complete current order/FIELD document membership is unavailable.");
    }
}
public sealed record CompanyPermitInformationItemFacts(string Id,string Subtype,string DocumentId,string? Title,string? Version,string? AccessCondition,string? UseCondition,DateTime? EffectiveFrom,DateTime? EffectiveTo,string Revision);
public sealed record CompanyPermitSourceDocumentFacts(string Id,string? Title,string? Type,string? Issue,string? Language,string? Publisher,DateTime? PublicationDate,DateTime? EffectiveFrom,DateTime? EffectiveTo,string? RecordedBy,string Revision);
public sealed record CompanyPermitFileFacts(string Id,string InformationItemId,string Subtype,string? Format,long ActualBytes,decimal ReportedSize,string? SizeUnit,string? ContentRevision,DateTime? EffectiveFrom,DateTime? EffectiveTo,string Revision);
public sealed record CompanyWorkOrderPermitDocumentFacts(CompanyWorkOrderDocumentCatalog Catalog,CompanyWorkOrderPermitDocumentQuery Query,string ExpectedReference,CompanyPermitInformationItemFacts? InformationItem,CompanyPermitSourceDocumentFacts? Document,CompanyPermitFileFacts? File,DateTime ComparedAtUtc,string Revision)
{
    public const int MaximumContentBytes=8*1024*1024;
    public void Require()
    {
        if(Query is null || Catalog is null)throw new InvalidOperationException("Complete original document query/catalog is required.");Query.Require();Catalog.Require(Catalog.Context.Company,Catalog.Context.ActorUserId,Query.Permit);
        if(!LeaseSummaryValidator.Identifier(ExpectedReference) || Catalog.Links.Count(row=>row.ComponentId==Query.ComponentId)!=1 || ComparedAtUtc==default || ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc<Catalog.ComparedAtUtc)throw new InvalidOperationException("Exact original document link/reference/comparison is unavailable.");
        var link=Catalog.Links.Single(row=>row.ComponentId==Query.ComponentId);
        if(InformationItem is {} info && (info.Id!=link.InformationItemId || info.Subtype!=link.InformationItemSubtype || info.DocumentId!=ExpectedReference || !ProcessDefinitionRevision.IsValid(info.Revision)) || Document is {} doc && (InformationItem is null || doc.Id!=ExpectedReference || !ProcessDefinitionRevision.IsValid(doc.Revision)) || File is {} file && (InformationItem is null || Document is null || file.InformationItemId!=link.InformationItemId || file.Subtype!=link.InformationItemSubtype || !LeaseSummaryValidator.Identifier(file.Id) || !ProcessDefinitionRevision.IsValid(file.Revision) || file.ActualBytes<0 || file.ActualBytes>MaximumContentBytes || file.ActualBytes>0 && !ProcessDefinitionRevision.IsValid(file.ContentRevision)))throw new UnauthorizedAccessException("Document metadata/content belongs to another recorded link or lacks complete proof.");
        if(Revision!=Digest(this))throw new InvalidOperationException("Complete original document source changed.");
    }
    public static string Digest(CompanyWorkOrderPermitDocumentFacts facts)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(facts with{Revision=""}));
    public static string CurrentProof(CompanyWorkOrderPermitDocumentFacts source)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{source.Catalog.Context,source.Catalog.Permit,source.Catalog.Links,source.Catalog.PopulationRevision,source.Query,source.ExpectedReference,source.InformationItem,source.Document,source.File}));
    public static string EvidenceProof(CompanyWorkOrderPermitDocumentFacts source)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{source.Catalog.Context.Company,source.Catalog.Context.Database,source.Catalog.Context.FieldId,source.Catalog.Permit,source.Catalog.Links,source.Catalog.PopulationRevision,source.Query,source.ExpectedReference,source.InformationItem,source.Document,source.File}));
    public static CompanyWorkOrderPermitDocumentFacts Create(CompanyWorkOrderDocumentCatalog catalog,CompanyWorkOrderPermitDocumentQuery query,string expected,CompanyPermitInformationItemFacts? info,CompanyPermitSourceDocumentFacts? document,CompanyPermitFileFacts? file,DateTime utc){var result=new CompanyWorkOrderPermitDocumentFacts(catalog,query,expected,info,document,file,utc,"");result=result with{Revision=Digest(result)};result.Require();return result;}
}
public sealed record CompanyWorkOrderPermitDocumentReview(CompanyWorkOrderExecutionPermitApprovalReview PermitReview,CompanyWorkOrderPermitDocumentFacts Source,IReadOnlyList<string> Blockers)
{
    public const string PendingAcceptanceBlocker="Independent acceptance of the exact referenced document and its applicable isolation/risk evidence is still required; source metadata/content proof does not approve a permit or authorize work.";
    public static string[] GetBlockers(CompanyWorkOrderExecutionPermitIntent permit,CompanyWorkOrderPermitDocumentFacts source)
    {
        source.Require();var result=new List<string>();
        var link=source.Catalog.Links.Single(row=>row.ComponentId==source.Query.ComponentId);if(!Covers(link.EffectiveFrom,link.EffectiveTo))result.Add("Recorded order/FIELD document link does not establish the complete permit window.");
        if(source.InformationItem is not {} info)result.Add("Exact active linked information item is unavailable.");
        else if(!string.IsNullOrWhiteSpace(info.AccessCondition) || !string.IsNullOrWhiteSpace(info.UseCondition))result.Add("Recorded document has unmapped access/use/security restrictions; private document/content access is withheld.");
        else if(string.IsNullOrWhiteSpace(info.Title) || string.IsNullOrWhiteSpace(info.Version) || !Covers(info.EffectiveFrom,info.EffectiveTo))result.Add("Recorded document information-item title/version/effective UTC proof is unconfirmed for the full permit window.");
        if(source.Document is not {} document)result.Add("Exact active referenced source document is unavailable.");
        else if(string.IsNullOrWhiteSpace(document.Title) || string.IsNullOrWhiteSpace(document.Type) || string.IsNullOrWhiteSpace(document.Issue) || string.IsNullOrWhiteSpace(document.Language) || string.IsNullOrWhiteSpace(document.RecordedBy) || !Covers(document.EffectiveFrom,document.EffectiveTo))result.Add("Recorded source-document identity/type/issue/language/recording-account/effective UTC proof is unconfirmed for the full permit window.");
        if(source.File is not {} file)result.Add("One unambiguous active linked file is unavailable.");
        else if(file.ActualBytes==0 || string.IsNullOrWhiteSpace(file.Format) || file.SizeUnit!="BYTE" || file.ReportedSize!=file.ActualBytes || !Covers(file.EffectiveFrom,file.EffectiveTo))result.Add("Actual document content, format, explicit BYTE size or effective UTC proof is unconfirmed; no size/unit/date default is inferred.");
        result.Add(PendingAcceptanceBlocker);return result.ToArray();
        bool Covers(DateTime? start,DateTime? end)=>start is {} from && end is {} to && from.Kind==DateTimeKind.Utc && to.Kind==DateTimeKind.Utc && from<=permit.ValidFromUtc && to>=permit.ValidToUtc && source.ComparedAtUtc>=from && source.ComparedAtUtc<to;
    }
    public void Require()
    {if(PermitReview?.Context is not {} context || Source?.Query is not {} query)throw new InvalidOperationException("Complete original document reviewer/query is required.");Require(context.Company,context.ActorUserId,query);}
    public void Require(CompanyScope company,string actor,CompanyWorkOrderPermitDocumentQuery query)
    {
        if(PermitReview is null || Source is null || Blockers is null)throw new InvalidOperationException("Complete original permit/document review is required.");query.Require();PermitReview.Require(company,actor,query.Permit);Source.Require();var expected=query.Kind=="ISOLATION"?PermitReview.Proposal.IsolationReference:PermitReview.Proposal.RiskReference;
        if(Source.Query!=query || Source.Catalog.Context!=PermitReview.Context || Source.ExpectedReference!=expected || !Blockers.SequenceEqual(GetBlockers(PermitReview.Proposal,Source),StringComparer.Ordinal))throw new InvalidOperationException("Exact original permit/document membership/source or blockers changed.");
    }
}
public interface ICompanyWorkOrderPermitDocumentSourceOwner
{
    Task<CompanyWorkOrderDocumentCatalog> ReadPermitDocumentCatalogAsync(FieldSelectionReview context,CompanyWorkOrderExecutionPermitReference permit,Func<Task> requireAuthority);
    Task<CompanyWorkOrderPermitDocumentFacts> ReadPermitDocumentSourceAsync(FieldSelectionReview context,CompanyWorkOrderPermitDocumentQuery query,string expectedReference,Func<Task> requireAuthority);
    Task<CompanyWorkOrderPermitDocumentContent> ReadPermitDocumentContentAsync(CompanyWorkOrderPermitDocumentReview original,Func<Task> requireAuthority);
}
public sealed record CompanyWorkOrderPermitDocumentContent(CompanyWorkOrderPermitDocumentReview Review,byte[] Bytes)
{
    public void Require(CompanyWorkOrderPermitDocumentReview original)
    {
        ArgumentNullException.ThrowIfNull(original);original.Require();if(Review is null)throw new InvalidOperationException("Closing document review is unavailable.");if(original.PermitReview?.Context is not {} context)throw new InvalidOperationException("Original document reviewer context is required.");original.Require(context.Company,context.ActorUserId,original.Source.Query);Review.Require(context.Company,context.ActorUserId,original.Source.Query);
        if(original.Blockers.Count!=1 || original.Blockers[0]!=CompanyWorkOrderPermitDocumentReview.PendingAcceptanceBlocker || !Review.Blockers.SequenceEqual(original.Blockers,StringComparer.Ordinal) || CompanyWorkOrderPermitDocumentFacts.CurrentProof(original.Source)!=CompanyWorkOrderPermitDocumentFacts.CurrentProof(Review.Source) || original.PermitReview.RecordedRevision!=Review.PermitReview.RecordedRevision || original.PermitReview.CurrentSource.SnapshotRevision!=Review.PermitReview.CurrentSource.SnapshotRevision || Bytes is null || Bytes.Length==0 || Bytes.Length>CompanyWorkOrderPermitDocumentFacts.MaximumContentBytes || Bytes.LongLength!=Review.Source.File?.ActualBytes || Convert.ToHexString(SHA256.HashData(Bytes))!=Review.Source.File.ContentRevision)throw new InvalidOperationException("Exact original authorized document content/version/source changed or is unconfirmed.");
    }
}
