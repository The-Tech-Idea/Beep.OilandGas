using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep;
namespace Beep.OilandGas.PermitsAndApplications.Services;

public partial class PermitApplicationLifecycleService
{
    private async Task<(PERMIT_DOCUMENT_STAGE Row,PermitDocumentStagingProof Proof,IDataSource Core)?> DocumentMembershipAsync(PermitApplicationDataScope scope,string applicationId,Guid intent)
    {
        if(await DataAccess.RequireReadScopeAsync()!=scope)throw new UnauthorizedAccessException("Current document membership context changed.");
        var permits=OpenReviewedSource(scope);PPDMDataSourceTransaction.RequireIdle(permits);
        await StageSchemaAsync(permits,typeof(PERMIT_DOCUMENT_STAGE),["PERMIT_DOCUMENT_STAGE_ID"]);
        var repository=await CreateRepositoryAsync<PERMIT_DOCUMENT_STAGE>(nameof(PERMIT_DOCUMENT_STAGE),applicationId);
        var row=await repository.GetByIdAsync(intent.ToString("N")) as PERMIT_DOCUMENT_STAGE;
        if(row is null)return null;
        if(row.SEED_JSON is not {Length:>0 and <=2097152})throw new InvalidOperationException("Document membership ownership is unconfirmed.");
        var seed=JsonSerializer.Deserialize<PermitDocumentStageSeed>(row.SEED_JSON)??throw new InvalidOperationException("Document membership seed is absent.");
        row.Restore(seed.Original);var origin=seed.Original.Review.Context;
        if(origin.Field.Company!=scope.Field.Company || origin.Field.FieldId!=scope.Field.FieldId || origin.Field.Database!=scope.Field.Database || origin.Database!=scope.Database ||
            seed.Original.Review.ReadApplication().PERMIT_APPLICATION_ID!=applicationId || seed.Original.IntentId!=intent)
            throw new UnauthorizedAccessException("Staged document membership is foreign to the current company/FIELD/application/bindings.");
        var proof=row.ReadProof(seed.Original);if(proof is null)return null;
        var core=Editor.GetDataSource(scope.Field.Database.ConnectionName)??throw new InvalidOperationException("The current native document target is unavailable.");
        if(Editor.OpenDataSource(scope.Field.Database.ConnectionName)!=ConnectionState.Open)throw new InvalidOperationException("The current native document target is closed.");
        await DocumentReadGuardAsync(scope,proof,permits,core);return(row,proof,core);
    }
    private async Task DocumentReadGuardAsync(PermitApplicationDataScope scope,PermitDocumentStagingProof proof,IDataSource permits,IDataSource core)
    {
        if(await DataAccess.RequireReadScopeAsync()!=scope || !ReferenceEquals(permits,Editor.GetDataSource(scope.Database.ConnectionName)) ||
            !ReferenceEquals(core,Editor.GetDataSource(scope.Field.Database.ConnectionName)))throw new UnauthorizedAccessException("Document read authority or source changed.");
        PPDMDataSourceTransaction.RequireIdle(permits);PPDMDataSourceTransaction.RequireIdle(core);
        MigrationConnectionTarget.Validate(Editor,permits,scope.Database.ConnectionName);MigrationConnectionTarget.Validate(Editor,core,scope.Field.Database.ConnectionName);
        if(proof.Seed.PermitTarget!=MigrationConnectionTarget.Fingerprint(permits.Dataconnection.ConnectionProp) || proof.Seed.CoreTarget!=MigrationConnectionTarget.Fingerprint(core.Dataconnection.ConnectionProp))
            throw new InvalidOperationException("Recorded document membership targets changed; explicit binding reconciliation is required.");
    }
    private async Task<object[]> CurrentDocumentRowsAsync(PermitApplicationDataScope scope,string applicationId,(PERMIT_DOCUMENT_STAGE Row,PermitDocumentStagingProof Proof,IDataSource Core) membership)
    {
        var permits=OpenReviewedSource(scope);
        async Task Guard()=>await DocumentReadGuardAsync(scope,membership.Proof,permits,membership.Core);
        await Guard();var vocabulary=await ReadDocumentVocabularyAsync(membership.Proof.Seed.Original.Document,membership.Core,Guard);
        var rows=await ReadStageCoreAsync(membership.Proof.Seed,membership.Core,Guard);RequireStageRevisions(membership.Proof,rows);
        var closing=await DocumentMembershipAsync(scope,applicationId,membership.Proof.Seed.Original.IntentId);
        if(closing is null || PermitApplicationCommandOutcome.Payload(closing.Value.Row)!=PermitApplicationCommandOutcome.Payload(membership.Row))
            throw new InvalidOperationException("Document membership changed during private content reads.");
        if(await ReadDocumentVocabularyAsync(membership.Proof.Seed.Original.Document,membership.Core,Guard)!=vocabulary)throw new PermitDocumentEvidenceChangedException("Native document vocabulary changed while reading content.");
        await Guard();return rows;
    }
    private async Task<IReadOnlyList<PermitAttachmentDocumentEvidence>> ReadLinkedDocumentsAsync(PermitApplicationDataScope scope,PERMIT_APPLICATION parent,IReadOnlyList<APPLICATION_ATTACHMENT> attachments)
    {
        var result=new List<PermitAttachmentDocumentEvidence>();
        foreach(var attachment in attachments.OrderBy(row=>row.APPLICATION_ATTACHMENT_ID,StringComparer.Ordinal))
        {
            if(attachment.DOCUMENT_STAGE_ID is null){result.Add(new(attachment.APPLICATION_ATTACHMENT_ID,"UNLINKED",null));continue;}
            if(!Guid.TryParseExact(attachment.DOCUMENT_STAGE_ID,"N",out var id))throw new InvalidOperationException("Attachment document membership identifier is invalid.");
            var membership=await DocumentMembershipAsync(scope,parent.PERMIT_APPLICATION_ID,id);
            if(membership is null){result.Add(new(attachment.APPLICATION_ATTACHMENT_ID,"UNLINKED",null));continue;}
            var receipt=PermitDocumentStageReceipt.From(membership.Value.Proof);receipt.RequireFor(parent.PERMIT_APPLICATION_ID,scope);
            var state="VERIFIED";
            if(!receipt.Matches(attachment) || DateTimeOffset.UtcNow<receipt.Document.EffectiveFromUtc || DateTimeOffset.UtcNow>=receipt.Document.EffectiveToUtc)state="CHANGED";
            else
            {
                try { await CurrentDocumentRowsAsync(scope,parent.PERMIT_APPLICATION_ID,membership.Value); }
                catch(PermitDocumentEvidenceChangedException changed)
                {
                    if(_failures is null)throw;
                    _failures.ReportHandled(changed,"reviewing linked permit document evidence","the document is marked CHANGED and cannot be used as current evidence");
                    state="CHANGED";
                }
            }
            result.Add(new(attachment.APPLICATION_ATTACHMENT_ID,state,receipt));
        }
        return result;
    }
    private Task RequireProposedDocumentAsync(PermitApplicationCommand original)=>DataAccess.WithReadAuthorityAsync(async()=>{await RequireProposedDocumentCoreAsync(original);return true;});
    private async Task RequireProposedDocumentCoreAsync(PermitApplicationCommand original)
    {
        var proposed=original.Attachment!.Document;var scope=original.Review.Context;
        var membership=await DocumentMembershipAsync(scope,proposed.ApplicationId,proposed.StageIntentId)??throw new InvalidOperationException("A confirmed current application-owned staged document is required.");
        var current=PermitDocumentStageReceipt.From(membership.Proof);
        if(current!=proposed || DateTimeOffset.UtcNow<current.Document.EffectiveFromUtc || DateTimeOffset.UtcNow>=current.Document.EffectiveToUtc)
            throw new InvalidOperationException("The proposed document receipt, version or validity changed.");
        await CurrentDocumentRowsAsync(scope,proposed.ApplicationId,membership);
    }
    public Task<PermitAttachmentContent> ReadAttachmentContentAsync(PermitAttachmentContentQuery query)=>DataAccess.WithReadAuthorityAsync(()=>ReadAttachmentContentCoreAsync(query));
    private async Task<PermitAttachmentContent> ReadAttachmentContentCoreAsync(PermitAttachmentContentQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);query.Require();var scope=await DataAccess.RequireReadScopeAsync();
        if(scope!=query.Review.Context)throw new UnauthorizedAccessException("The download belongs to another current context.");
        var applicationId=query.Review.ReadApplication().PERMIT_APPLICATION_ID;var current=await ReviewSourceAsync(applicationId);query.Review.RequireCurrent(current);
        var evidence=current.Supporting.Documents.Single(row=>row.AttachmentId==query.AttachmentId);
        if(evidence.State!="VERIFIED" || evidence.Receipt is not {} receipt)throw new InvalidOperationException("Current attachment content is unconfirmed.");
        var membership=await DocumentMembershipAsync(scope,applicationId,receipt.StageIntentId)??throw new InvalidOperationException("Current attachment membership is unavailable.");
        var rows=await CurrentDocumentRowsAsync(scope,applicationId,membership);
        var bytes=((RM_FILE_CONTENT)rows.Single(row=>row is RM_FILE_CONTENT)).FILE_CONTENT.ToArray();receipt.Document.RequireContent(bytes);
        query.Review.RequireCurrent(await ReviewSourceAsync(applicationId));
        if(await DataAccess.RequireReadScopeAsync()!=scope)throw new UnauthorizedAccessException("Download authority changed before delivery.");
        return new(query,receipt,bytes);
    }
}
