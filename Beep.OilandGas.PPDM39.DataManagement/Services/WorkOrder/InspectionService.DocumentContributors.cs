using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public sealed partial class InspectionService:ICompanyWorkOrderDocumentContributorOwner,ICompanyWorkOrderDocumentContributorCore
{
    public Task<CompanyWorkOrderDocumentContributors> ReadDocumentContributorsAsync(FieldSelectionReview context,CompanyWorkOrderPermitDocumentQuery query,string expectedReference,Func<Task> authority)=>ReadDocumentContributorsCoreAsync(context,query,expectedReference,authority,false);
    public Task<CompanyWorkOrderDocumentContributors> ReadDocumentContributorsCoreAsync(FieldSelectionReview context,CompanyWorkOrderPermitDocumentQuery query,string expectedReference,Func<Task> authority)=>ReadDocumentContributorsCoreAsync(context,query,expectedReference,authority,true);
    public async Task RequireApprovalDocumentCoreAsync(FieldSelectionReview context,CompanyWorkOrderAcceptedDocumentReview expected,Func<Task> authority)
    {
        expected.Evidence.Require();expected.Require(expected.Evidence.PermitReview.Proposal,expected.Evidence.PermitReview.CurrentSource);
        if(expected.Evidence.PermitReview.Context!=context || expected.CurrentContributors is null || expected.Blockers.Count!=0)throw new InvalidOperationException("Exact original complete accepted-document proof is required for the owned approval boundary.");
        var original=expected.Evidence.Source;
        // The approval owner invokes this before and after its exact mutation. Each observation must match retained proof.
        var current=await ReadDocumentContributorsCoreAsync(context,original.Query,original.ExpectedReference,authority,true,false,true);
        if(CompanyWorkOrderPermitDocumentFacts.CurrentProof(current.Source)!=CompanyWorkOrderPermitDocumentFacts.CurrentProof(original) || CompanyWorkOrderDocumentContributors.EvidenceProof(current)!=CompanyWorkOrderDocumentContributors.EvidenceProof(expected.CurrentContributors))throw new InvalidOperationException("Complete original document/content/contributor proof changed at the owned approval boundary.");
    }
    public async Task RequireExecutionDocumentCoreAsync(FieldSelectionReview executor,CompanyWorkOrderAcceptedDocumentReview expected,Func<Task> authority)
    {
        var history=expected.Evidence.PermitReview;expected.Evidence.Require();expected.Require(history.Proposal,history.CurrentSource);
        if(executor.Company!=history.Context.Company || executor.Database!=history.Context.Database || executor.FieldId!=history.Proposal.Task.FieldId)
            throw new UnauthorizedAccessException("Execution document proof requires the original company/FIELD/native database.");
        FieldSelectionReviewValidator.RequireReview(executor,history.Context.Company,executor.ActorUserId);
        var utc=DateTime.UtcNow;
        if(expected.ContributorComparisonVersion!=1 || expected.CurrentContributors is null || expected.Blockers.Count!=0 || utc<history.Proposal.ValidFromUtc || utc>=history.Proposal.ValidToUtc)
            throw new InvalidOperationException("Current execution requires complete contributor-aware document acceptance within its recorded permit window.");
        var original=expected.Evidence.Source;
        var source=editor.GetDataSource(executor.Database.ConnectionName)??throw new InvalidOperationException("Original execution document core source is unavailable.");
        async Task Parent()
        {
            RequireDocumentSourceOwnership(source,true);
            if(!ReferenceEquals(source,editor.GetDataSource(executor.Database.ConnectionName)))throw new InvalidOperationException("Original execution document core target changed.");
            await authority();await database.RequireContextAsync(executor.Company,executor.ActorUserId,executor.Database);
            await requireOrder(executor.ActorUserId,original.Query.Permit.Task.FieldId,original.Query.Permit.Task.WorkOrderId,"WRITE");await authority();
            if(!ReferenceEquals(source,editor.GetDataSource(executor.Database.ConnectionName)))throw new InvalidOperationException("Original execution document core target changed.");
            RequireDocumentSourceOwnership(source,true);
        }
        // The complete parent is observed around this one owned document observation.
        // Every inner source guard still refreshes authority and enforces native ownership;
        // the enclosing permit owner repeats this complete observation around its mutation.
        await Parent();
        var current=await ReadDocumentContributorsCoreAsync(executor,original.Query,original.ExpectedReference,authority,true,true,true);
        // Evidence hashes omit the viewing actor while retaining company, binding, membership, bytes and native people.
        if(CompanyWorkOrderPermitDocumentFacts.EvidenceProof(current.Source)!=CompanyWorkOrderPermitDocumentFacts.EvidenceProof(original) ||
            CompanyWorkOrderDocumentContributors.EvidenceProof(current)!=CompanyWorkOrderDocumentContributors.EvidenceProof(expected.CurrentContributors) ||
            CompanyWorkOrderPermitDocumentReview.GetBlockers(history.Proposal,current.Source).Any(value=>value!=CompanyWorkOrderPermitDocumentReview.PendingAcceptanceBlocker))
            throw new InvalidOperationException("Complete original document/content/contributor proof changed at the execution boundary.");
        await Parent();
    }
    private async Task<CompanyWorkOrderDocumentContributors> ReadDocumentContributorsCoreAsync(FieldSelectionReview context,CompanyWorkOrderPermitDocumentQuery query,string expectedReference,Func<Task> authority,bool owned,bool completeGuard=false,bool approvalObservation=false)
    {
        query.Require();var source=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original document contributor source is unavailable.");
        if(approvalObservation){if(!owned)throw new InvalidOperationException("An approval observation requires its owned serializable boundary.");PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);}
        var parentBookends=owned && !completeGuard;
        async Task Guard(){RequireDocumentSourceOwnership(source,owned);await authority();if(!completeGuard && !parentBookends){await database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);await requireOrder(context.ActorUserId,query.Permit.Task.FieldId,query.Permit.Task.WorkOrderId,"WRITE");}if(!ReferenceEquals(source,editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original contributor native target changed.");RequireDocumentSourceOwnership(source,owned);}
        async Task Parent(){await Guard();await database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);await requireOrder(context.ActorUserId,query.Permit.Task.FieldId,query.Permit.Task.WorkOrderId,"WRITE");await Guard();}
        if(parentBookends)await Parent();
        var original=await ReadDocumentSourceAsync(context,query,expectedReference,Guard,null,owned,true,true);if(original.InformationItem is not {} info || original.Document is null || !string.IsNullOrWhiteSpace(info.AccessCondition) || !string.IsNullOrWhiteSpace(info.UseCondition))throw new UnauthorizedAccessException("Current unrestricted linked document membership must close before private contributor identity reads.");
        var link=original.Catalog.Links.Single(row=>row.ComponentId==query.ComponentId);
        async Task<CompanyPermitDocumentContributorFacts[]> Read()
        {
            await Guard();var links=await ReadDocumentLinksAsync(context,query.Permit,Guard,owned,true);if(DocumentDigest(links)!=original.Catalog.PopulationRevision)throw new InvalidOperationException("Original document membership changed before contributor reads.");
            var raw=(await (await DocumentRepository(typeof(RM_CREATOR),["INFO_ITEM_SUBTYPE","INFORMATION_ITEM_ID","CREATOR_ID"],source,context)).GetAsync([new(){FieldName="INFORMATION_ITEM_ID",Operator="=",FilterValue=link.InformationItemId},new(){FieldName="INFO_ITEM_SUBTYPE",Operator="=",FilterValue=link.InformationItemSubtype},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}])).ToArray();await Guard();
            if(raw.Length>256 || raw.Any(value=>value is not RM_CREATOR row || row.INFORMATION_ITEM_ID!=link.InformationItemId || row.INFO_ITEM_SUBTYPE!=link.InformationItemSubtype || row.ACTIVE_IND!="Y" || !LeaseSummaryValidator.Identifier(row.CREATOR_ID)))throw new UnauthorizedAccessException("Complete linked document contributor population is foreign or unavailable.");
            var rows=raw.Cast<RM_CREATOR>().ToArray();if(rows.Select(row=>row.CREATOR_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length)throw new InvalidOperationException("Document contributor identities are ambiguous.");var result=new List<CompanyPermitDocumentContributorFacts>();
            foreach(var originalCreator in rows.OrderBy(row=>row.CREATOR_ID,StringComparer.Ordinal))
            {
                RequireDocumentDateKinds(originalCreator.EFFECTIVE_DATE,originalCreator.EXPIRY_DATE);var creator=CompanyInventorySourceReview.Copy(originalCreator);BUSINESS_ASSOCIATE? person=null;
                if(LeaseSummaryValidator.Identifier(creator.CREATOR_BA_ID) && creator.EFFECTIVE_DATE is {} from && creator.EXPIRY_DATE is {} to && from<=DateTime.UtcNow && to>DateTime.UtcNow)
                {
                    await Guard();var parents=(await (await DocumentRepository(typeof(BUSINESS_ASSOCIATE),["BUSINESS_ASSOCIATE_ID"],source,context)).GetAsync([new(){FieldName="BUSINESS_ASSOCIATE_ID",Operator="=",FilterValue=creator.CREATOR_BA_ID},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}])).ToArray();await Guard();
                    if(parents.Length>1 || parents.Any(value=>value is not BUSINESS_ASSOCIATE row || row.BUSINESS_ASSOCIATE_ID!=creator.CREATOR_BA_ID || row.ACTIVE_IND!="Y"))throw new UnauthorizedAccessException("Exact referenced contributor identity is foreign or ambiguous.");if(parents.Length==1){var actual=(BUSINESS_ASSOCIATE)parents[0];RequireDocumentDateKinds(actual.EFFECTIVE_DATE,actual.EXPIRY_DATE);person=CompanyInventorySourceReview.Copy(actual);}
                }
                // Names are reported document metadata; exact BA keys and native parent proof establish identity.
                result.Add(new(creator.CREATOR_ID,creator.CREATOR_BA_ID,creator.CREATOR_TYPE,creator.CREATOR_BA_TYPE,creator.CREATOR_NAME,creator.EFFECTIVE_DATE,creator.EXPIRY_DATE,person?.BA_TYPE,person?.EFFECTIVE_DATE,person?.EXPIRY_DATE,person is null?null:DocumentDigest(person),DocumentDigest(creator)));
            }
            await Guard();return result.ToArray();
        }
        var first=await Read();
        if(approvalObservation){if(parentBookends)await Parent();else await Guard();return CompanyWorkOrderDocumentContributors.Create(original,first,DateTime.UtcNow);}
        var closing=await Read();var closingSource=await ReadDocumentSourceAsync(context,query,expectedReference,Guard,null,owned,true,true);await Guard();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing) || CompanyWorkOrderPermitDocumentFacts.CurrentProof(original)!=CompanyWorkOrderPermitDocumentFacts.CurrentProof(closingSource))throw new InvalidOperationException("Complete original document/contributor/native identity population changed during review.");if(parentBookends)await Parent();return CompanyWorkOrderDocumentContributors.Create(closingSource,closing,DateTime.UtcNow);
    }
}
