using System.Data;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using System.Text.Json;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.PermitsAndApplications.Services;

public partial class PermitApplicationLifecycleService
{
    private async Task StageSchemaAsync(IDataSource source,Type type,string[] keys,object? proposed=null)
    {
        var definition=await Metadata.GetTableMetadataAsync(type.Name);
        var shape=source.GetEntityStructure(type.Name,true);
        if(definition?.EntityTypeName!=type.FullName || definition is null ||
            !definition.PrimaryKeyColumn.Split(',').Select(x=>x.Trim()).Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal)) ||
            shape?.Fields is null || !shape.Fields.Where(x=>x.IsKey).Select(x=>x.FieldName).Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal)))
            throw new NativeSchemaCompatibilityException("Install exact canonical document staging keys/metadata through Beep Migration before writing.");
        foreach(var property in type.GetProperties().Where(x=>x.CanRead && x.CanWrite))
        {
            var columns=shape.Fields.Where(x=>x.FieldName==property.Name).ToArray();var value=proposed is null?null:property.GetValue(proposed);
            if(columns.Length!=1 || columns[0].Fieldtype!=(Nullable.GetUnderlyingType(property.PropertyType)??property.PropertyType).FullName ||
                value is string text && Math.Max(columns[0].Size,columns[0].Size1)<Math.Max(1,text.Length))
                throw new NativeSchemaCompatibilityException($"The native staging schema cannot preserve {type.Name}.{property.Name}.");
        }
    }
    private async Task<PERMIT_DOCUMENT_STAGE?> ReadStageAsync(PermitDocumentStageOriginal original,IDataSource source)
    {
        var context=original.Review.Context;
        if(await DataAccess.RequireReadScopeAsync()!=context)throw new UnauthorizedAccessException("Document staging belongs to another current context.");
        await StageSchemaAsync(source,typeof(PERMIT_DOCUMENT_STAGE),["PERMIT_DOCUMENT_STAGE_ID"]);
        var repository=await CreateRepositoryAsync<PERMIT_DOCUMENT_STAGE>(nameof(PERMIT_DOCUMENT_STAGE),original.Review.ReadApplication().PERMIT_APPLICATION_ID);
        var row=await repository.GetByIdAsync(original.IntentId.ToString("N")) as PERMIT_DOCUMENT_STAGE;
        if(row is not null)
        {
            var seed=row.Restore(original);MigrationConnectionTarget.Validate(Editor,source,context.Database.ConnectionName);
            var registered=Editor.ConfigEditor.DataConnections.Where(x=>x.ConnectionName==context.Field.Database.ConnectionName).ToArray();
            if(registered.Length!=1 || seed.PermitTarget!=MigrationConnectionTarget.Fingerprint(source.Dataconnection.ConnectionProp) || seed.CoreTarget!=MigrationConnectionTarget.Fingerprint(registered[0]))
                throw new InvalidOperationException("Recorded staging ownership belongs to another configured native target. Review binding reconciliation before retry.");
        }
        if(!ReferenceEquals(source,Editor.GetDataSource(context.Database.ConnectionName)) || await DataAccess.RequireReadScopeAsync()!=context)
            throw new InvalidOperationException("The staging journal target or authority changed.");
        return row;
    }
    public Task<PermitDocumentStagingProof?> RecoverDocumentStageAsync(PermitDocumentStageOriginal original)=>DataAccess.WithReadAuthorityAsync(()=>RecoverDocumentStageCoreAsync(original));
    private async Task<PermitDocumentStagingProof?> RecoverDocumentStageCoreAsync(PermitDocumentStageOriginal original)
    {
        ArgumentNullException.ThrowIfNull(original);original.Require();
        var source=OpenReviewedSource(original.Review.Context);PPDMDataSourceTransaction.RequireIdle(source);
        var first=await ReadStageAsync(original,source);var proof=first?.ReadProof(original);
        var closing=await ReadStageAsync(original,source);
        if((first is null)!=(closing is null) || first is not null && PermitApplicationCommandOutcome.Payload(first)!=PermitApplicationCommandOutcome.Payload(closing!))
            throw new InvalidOperationException("The recorded staging result changed during recovery.");
        PPDMDataSourceTransaction.RequireIdle(source);return proof;
    }
    public async Task<PermitDocumentStagingProof> StageDocumentAsync(PermitDocumentStageOriginal original,byte[] content)
    {
        ArgumentNullException.ThrowIfNull(original);original.Require();original.Document.RequireContent(content);
        // Copy before awaiting: transport buffer mutation must never alter the retained upload evidence.
        content=content.ToArray();
        var recorded=await RecoverDocumentStageAsync(original);if(recorded is not null)return recorded;
        var context=original.Review.Context;var actor=context.Field.ActorUserId;
        var permits=OpenReviewedSource(context);
        var core=Editor.GetDataSource(context.Field.Database.ConnectionName)??throw new InvalidOperationException("The current native document CORE target is unavailable.");
        if(Editor.OpenDataSource(context.Field.Database.ConnectionName)!=ConnectionState.Open || core.DatasourceName!=context.Field.Database.ConnectionName)
            throw new InvalidOperationException("The native document CORE target could not be retained.");
        MigrationConnectionTarget.Validate(Editor,permits,context.Database.ConnectionName);MigrationConnectionTarget.Validate(Editor,core,context.Field.Database.ConnectionName);
        var permitTarget=MigrationConnectionTarget.Fingerprint(permits.Dataconnection.ConnectionProp);var coreTarget=MigrationConnectionTarget.Fingerprint(core.Dataconnection.ConnectionProp);
        await DataAccess.WithDocumentStageAuthorityAsync(original,async authority=>
        {
            async Task Guard(IDataSource? owned=null)
            {
                await authority();
                MigrationConnectionTarget.Validate(Editor,permits,context.Database.ConnectionName);MigrationConnectionTarget.Validate(Editor,core,context.Field.Database.ConnectionName);
                if(await DataAccess.RequireReadScopeAsync()!=context || !ReferenceEquals(permits,Editor.GetDataSource(context.Database.ConnectionName)) ||
                    !ReferenceEquals(core,Editor.GetDataSource(context.Field.Database.ConnectionName)) ||
                    permitTarget!=MigrationConnectionTarget.Fingerprint(permits.Dataconnection.ConnectionProp) || coreTarget!=MigrationConnectionTarget.Fingerprint(core.Dataconnection.ConnectionProp))
                    throw new InvalidOperationException("The reviewed staging account, target or binding changed.");
                if(owned is not null)PPDMDataSourceTransaction.RequireOwned(owned,IsolationLevel.Serializable);
            }
            async Task Root()
            { await Guard();original.Review.RequireCurrent(await ReviewSourceAsync(original.Review.ReadApplication().PERMIT_APPLICATION_ID)); }
            await Root();PPDMDataSourceTransaction.RequireIdle(permits);PPDMDataSourceTransaction.RequireIdle(core);
            foreach(var type in new[]{typeof(SOURCE_DOCUMENT),typeof(RM_INFORMATION_ITEM),typeof(RM_FILE_CONTENT)})
                await StageSchemaAsync(core,type,StageKeys(type));
            // Persist application/company ownership and a server nonce BEFORE any private native lookup.
            await PPDMDataSourceTransaction.ExecuteAsync(permits,async()=>
            {
                await Guard(permits);
                if(await ReadStageAsync(original,permits) is null)
                {
                    var row=PERMIT_DOCUMENT_STAGE.Create(new(original,Guid.NewGuid(),DateTimeOffset.UtcNow,coreTarget,permitTarget));
                    await StageSchemaAsync(permits,typeof(PERMIT_DOCUMENT_STAGE),["PERMIT_DOCUMENT_STAGE_ID"],row);
                    await StageJournalWriteAsync(original,permits,row,null,()=>Guard(permits));
                    if(await ReadStageAsync(original,permits) is null)throw new InvalidOperationException("The staging ownership seed could not be read back.");
                }
                await Guard(permits);
            });
            await Root();var journal=await ReadStageAsync(original,permits)??throw new InvalidOperationException("The native staging ownership seed is unavailable.");
            var seed=journal.Restore(original);PermitDocumentStagingProof? proof=null;
            // Independent CORE transaction. A failed/lost response leaves the original and journal pending.
            await PPDMDataSourceTransaction.ExecuteAsync(core,async()=>
            {
                await Guard(core);var rows=await ReadStageCoreAsync(seed,core,()=>Guard(core));
                if(rows.Length==0)
                {
                    foreach(var row in seed.BuildRows(content))
                    {
                        await Guard(core);var type=row.GetType();await StageSchemaAsync(core,type,StageKeys(type),row);
                        var reads=new StageCoreReads(seed,type,()=>Guard(core));
                        var policy=new PermitCommandMutation(reads,core,row,null,PPDMRowMutation.Insert,actor,()=>Guard(core));
                        await new PPDMGenericRepository(Editor,CommonColumnHandler,Defaults,Metadata,type,core.DatasourceName,type.Name,rowAccess:policy).InsertAsync(row,actor);
                    }
                    rows=await ReadStageCoreAsync(seed,core,()=>Guard(core));
                }
                if(rows.Length!=3)throw new InvalidOperationException("Native staging rows are partial; do not adopt, repair or duplicate an unconfirmed original.");
                proof=new(seed,PermitDocumentStagingProof.Revision(rows[0]),PermitDocumentStagingProof.Revision(rows[1]),PermitDocumentStagingProof.Revision(rows[2]),DateTimeOffset.UtcNow);
                proof.Require(original);await Guard(core);
            });
            await Root();
            // Verify committed native bytes/headers before publishing the journal confirmation.
            var committed=await ReadStageCoreAsync(seed,core,()=>Guard());
            RequireStageRevisions(proof!,committed);
            await PPDMDataSourceTransaction.ExecuteAsync(permits,async()=>
            {
                await Guard(permits);var before=await ReadStageAsync(original,permits)??throw new InvalidOperationException("Staging ownership disappeared.");
                if(before.STAGED_JSON is not null)
                {
                    var prior=before.ReadProof(original)!;
                    if(prior.Seed!=seed || prior.DocumentRevision!=proof!.DocumentRevision || prior.InformationRevision!=proof.InformationRevision || prior.FileRevision!=proof.FileRevision)
                        throw new InvalidOperationException("Another staging result belongs to this original.");
                    return;
                }
                var after=JsonSerializer.Deserialize<PERMIT_DOCUMENT_STAGE>(JsonSerializer.Serialize(before))!;
                after.Confirm(proof!);
                after.ROW_CHANGED_BY=actor;after.ROW_CHANGED_DATE=DateTime.UtcNow;
                await StageSchemaAsync(permits,typeof(PERMIT_DOCUMENT_STAGE),["PERMIT_DOCUMENT_STAGE_ID"],after);
                await StageJournalWriteAsync(original,permits,after,before,()=>Guard(permits));
                if((await ReadStageAsync(original,permits))?.ReadProof(original)!=proof)throw new InvalidOperationException("Native staging confirmation could not be read back.");
                await Guard(permits);
            });
            await Root();RequireStageRevisions(proof!,await ReadStageCoreAsync(seed,core,()=>Guard()));return true;
        });
        return await RecoverDocumentStageAsync(original)??throw new InvalidOperationException("Document staging is unconfirmed; retain and reconcile the same original.");
    }
    private async Task StageJournalWriteAsync(PermitDocumentStageOriginal original,IDataSource source,PERMIT_DOCUMENT_STAGE row,PERMIT_DOCUMENT_STAGE? before,Func<Task> guard)
    {
        var actor=original.Review.Context.Field.ActorUserId;var operation=before is null?PPDMRowMutation.Insert:PPDMRowMutation.Update;
        var reads=DataAccess.ForRead(typeof(PERMIT_DOCUMENT_STAGE),nameof(PERMIT_DOCUMENT_STAGE),row.PERMIT_APPLICATION_ID);
        var policy=new PermitCommandMutation(reads,source,row,before,operation,actor,guard);
        var repository=new PPDMGenericRepository(Editor,CommonColumnHandler,Defaults,Metadata,typeof(PERMIT_DOCUMENT_STAGE),source.DatasourceName,nameof(PERMIT_DOCUMENT_STAGE),rowAccess:policy);
        if(before is null)await repository.InsertAsync(row,actor);else await repository.UpdateAsync(row,actor);
    }
    private static string[] StageKeys(Type type)=>type==typeof(SOURCE_DOCUMENT)?["SOURCE_DOCUMENT_ID"]:
        type==typeof(RM_INFORMATION_ITEM)?["INFORMATION_ITEM_ID","INFO_ITEM_SUBTYPE"]:type==typeof(RM_FILE_CONTENT)?["FILE_ID"]:throw new InvalidOperationException("Exact native document type required.");
    private async Task<object[]> ReadStageCoreAsync(PermitDocumentStageSeed seed,IDataSource core,Func<Task> guard)
    {
        var rows=new List<object>();
        foreach(var type in new[]{typeof(SOURCE_DOCUMENT),typeof(RM_INFORMATION_ITEM),typeof(RM_FILE_CONTENT)})
        {
            await guard();await StageSchemaAsync(core,type,StageKeys(type));var policy=new StageCoreReads(seed,type,guard);
            var repo=new PPDMGenericRepository(Editor,CommonColumnHandler,Defaults,Metadata,type,core.DatasourceName,type.Name,rowAccess:policy);
            var found=(await repo.GetAsync([])).ToArray();if(found.Length>1)throw new PermitDocumentEvidenceChangedException("Native staged document population is ambiguous.");rows.AddRange(found);
        }
        await guard();return rows.ToArray();
    }
    private static void RequireStageRevisions(PermitDocumentStagingProof proof,object[] rows)
    {
        if(rows.Length!=3 || PermitDocumentStagingProof.Revision(rows[0])!=proof.DocumentRevision || PermitDocumentStagingProof.Revision(rows[1])!=proof.InformationRevision || PermitDocumentStagingProof.Revision(rows[2])!=proof.FileRevision)
            throw new PermitDocumentEvidenceChangedException("Native staged headers/content changed across transaction confirmation.");
    }
    private sealed class StageCoreReads(PermitDocumentStageSeed seed,Type expected,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private void Canonical(Type type,string table){if(type!=expected || table!=type.Name)throw new UnauthorizedAccessException("Native staging row type changed.");}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {
            Canonical(type,table);await guard();
            AppFilter Equal(string key,string value)=>new(){FieldName=key,Operator="=",FilterValue=value};
            return type==typeof(SOURCE_DOCUMENT)?[[Equal("SOURCE_DOCUMENT_ID",seed.DocumentId),Equal("ACTIVE_IND","Y")]]:
                [[Equal("INFORMATION_ITEM_ID",seed.InformationId),Equal("INFO_ITEM_SUBTYPE",seed.Original.Document.InformationSubtype),Equal("ACTIVE_IND","Y")]];
        }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table){Canonical(type,table);await guard();return [];}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {Canonical(type,table);await guard();if(rows.Count>1)throw new PermitDocumentEvidenceChangedException("Native staging rows are ambiguous.");foreach(var row in rows){if(row.GetType()!=type)throw new UnauthorizedAccessException("Unexpected staged row.");seed.RequireRow(row);}await guard();}
        public async Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool includeInherited=true){await ValidateRowsAsync(type,table,[row]);return "READ";}
        public Task ValidateMutationAsync(Type type,string table,object row,object? previous,PPDMRowMutation operation,string? actor)=>throw new UnauthorizedAccessException("Native document staging writes require their exact owned command.");
    }
}
