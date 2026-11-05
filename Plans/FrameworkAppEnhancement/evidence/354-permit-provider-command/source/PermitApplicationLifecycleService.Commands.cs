using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.PermitsAndApplications.Services;

public partial class PermitApplicationLifecycleService
{
    private IDataSource OpenReviewedSource(PermitApplicationDataScope context)
    {
        var source = Editor.GetDataSource(context.Database.ConnectionName) ?? throw new InvalidOperationException("The permit command datasource is unavailable.");
        if (Editor.OpenDataSource(context.Database.ConnectionName) != ConnectionState.Open || !ReferenceEquals(source, Editor.GetDataSource(context.Database.ConnectionName)))
            throw new InvalidOperationException("The permit command datasource could not be opened and retained.");
        return source;
    }
    private async Task ValidateCommandSchemaAsync(IDataSource source, PermitApplicationCommandOutcome? outcome)
    {
        foreach (var (type, key) in new[] {(typeof(PERMIT_APPLICATION), "PERMIT_APPLICATION_ID"), (typeof(PERMIT_STATUS_HISTORY), "PERMIT_STATUS_HISTORY_ID"),
            (typeof(PERMIT_APPLICATION_COMMAND), "PERMIT_APPLICATION_COMMAND_ID")})
        {
            var metadata = await Metadata.GetTableMetadataAsync(type.Name);
            if (metadata is null || metadata.EntityTypeName != type.FullName || metadata.PrimaryKeyColumn != key)
                throw new NativeSchemaCompatibilityException("The permit command requires exact canonical entity metadata and primary keys.");
            var shape = source.GetEntityStructure(new EntityStructure {EntityName = type.Name, DatasourceEntityName = type.Name}, true);
            if (shape?.Fields is null || !shape.Fields.Where(field => field.IsKey).Select(field => field.FieldName).SequenceEqual([key]))
                throw new NativeSchemaCompatibilityException("Install the canonical permit command schema using Beep Migration before writing.");
            var requirements = new List<(string Name, Type Type, int Width)> { (key, typeof(string), type == typeof(PERMIT_APPLICATION_COMMAND) ? 32 : 128),
                ("ACTIVE_IND", typeof(string), 1), ("ROW_CREATED_BY", typeof(string), 128), ("ROW_CHANGED_BY", typeof(string), 128),
                ("ROW_CREATED_DATE", typeof(DateTime), 0), ("ROW_CHANGED_DATE", typeof(DateTime), 0) };
            if (type == typeof(PERMIT_APPLICATION)) requirements.AddRange([("OWNER_TENANT_ID", typeof(string),64), ("OWNER_COMPANY_ID",typeof(string),128),
                ("FIELD_ID",typeof(string),128),("SUBMITTED_BY_USER_ID",typeof(string),128),("SUBMITTED_DATE",typeof(DateTime),0),("DECISION_DATE",typeof(DateTime),0)]);
            if(type==typeof(PERMIT_APPLICATION) && outcome?.Original.Action is "EDIT_DRAFT" or "CREATE_DRAFT")
            {
                var header=outcome.Original.Action=="CREATE_DRAFT"?outcome.Original.CreationReview!.Proposal.Header:outcome.Original.DraftChanges!;
                requirements.Add(("APPLICANT_ID",typeof(string),header.Applicant.Length));
                requirements.Add(("REMARKS",typeof(string),Math.Max(1,header.Description?.Length??0)));
                foreach(var name in new[]{"APPLICATION_TYPE","COUNTRY","STATE_PROVINCE","REGULATORY_AUTHORITY"})
                    if(shape.Fields.Count(field=>field.FieldName==name)!=1)throw new NativeSchemaCompatibilityException("The permit schema cannot preserve draft classifications.");
            }
            if(type==typeof(PERMIT_APPLICATION) && outcome?.Original.Action=="CREATE_DRAFT")
            {
                var proposal=outcome.Original.CreationReview!.Proposal;
                requirements.AddRange([("WELL_ID",typeof(string),Math.Max(1,proposal.WellId?.Length??0)),
                    ("FACILITY_ID",typeof(string),Math.Max(1,proposal.FacilityId?.Length??0)),("FACILITY_TYPE",typeof(string),Math.Max(1,proposal.FacilityType?.Length??0)),
                    ("CREATED_DATE",typeof(DateTime),0),("SUBMISSION_COMPLETE_IND",typeof(string),1),("PPDM_GUID",typeof(string),36),("ROW_EFFECTIVE_DATE",typeof(DateTime),0)]);
            }
            if (type == typeof(PERMIT_STATUS_HISTORY)) requirements.AddRange([("PERMIT_APPLICATION_ID",typeof(string),128),("STATUS",typeof(string),32),
                ("STATUS_REMARKS",typeof(string),4096),("UPDATED_BY",typeof(string),128),("STATUS_DATE",typeof(DateTime),0)]);
            if (type == typeof(PERMIT_APPLICATION_COMMAND)) requirements.AddRange([("OWNER_TENANT_ID",typeof(string),64),("OWNER_COMPANY_ID",typeof(string),128),
                ("FIELD_ID",typeof(string),128),("PERMIT_APPLICATION_ID",typeof(string),128),("SOURCE_REVISION",typeof(string),64),
                ("OUTCOME_JSON",typeof(string),outcome is null ? 1 : JsonSerializer.Serialize(outcome).Length),("OUTCOME_HASH",typeof(string),64)]);
            foreach (var field in requirements)
            {
                var columns = shape.Fields.Where(column => column.FieldName == field.Name).ToArray();
                if (columns.Length != 1 || columns[0].Fieldtype != field.Type.FullName || field.Width > 0 && Math.Max(columns[0].Size,columns[0].Size1) < field.Width)
                    throw new NativeSchemaCompatibilityException($"The native permit schema cannot preserve {type.Name}.{field.Name}: required {field.Type.FullName} width {field.Width}; observed {string.Join(", ", columns.Select(column => $"{column.Fieldtype} width {Math.Max(column.Size,column.Size1)}"))}.");
            }
        }
    }
    public async Task<PermitApplicationCommandOutcome?> RecoverCommandAsync(PermitApplicationCommand original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require();
        var context = original.Review.Context;
        if (await DataAccess.RequireReadScopeAsync() != context) throw new UnauthorizedAccessException("The permit original belongs to another current context.");
        var source = OpenReviewedSource(context); PPDMDataSourceTransaction.RequireIdle(source);
        await ValidateCommandSchemaAsync(source, null);
        var id = original.Review.ReadApplication().PERMIT_APPLICATION_ID;
        var repository = await CreateRepositoryAsync<PERMIT_APPLICATION_COMMAND>(nameof(PERMIT_APPLICATION_COMMAND), id);
        var first = await repository.GetByIdAsync(original.IntentId.ToString("N")) as PERMIT_APPLICATION_COMMAND;
        if (first is null)
        {
            if(await DataAccess.RequireReadScopeAsync()!=context)throw RefusalException.Forbidden("The permit recovery context changed before confirming absence.");
            PPDMDataSourceTransaction.RequireIdle(source);return null;
        }
        var outcome = first.Restore(context);
        if (outcome.Original != original) throw new InvalidOperationException("This permit intent belongs to a different retained original.");
        var history = await CreateRepositoryAsync<PERMIT_STATUS_HISTORY>(nameof(PERMIT_STATUS_HISTORY), id);
        var saved = await history.GetByIdAsync(outcome.ReadHistory().PERMIT_STATUS_HISTORY_ID) as PERMIT_STATUS_HISTORY;
        if (saved is null || PermitApplicationCommandOutcome.Payload(saved) != PermitApplicationCommandOutcome.Payload(outcome.ReadHistory()))
            throw new InvalidOperationException("The recorded command history changed; the permit outcome is unconfirmed.");
        var second = await repository.GetByIdAsync(original.IntentId.ToString("N")) as PERMIT_APPLICATION_COMMAND;
        if (second is null || first.OUTCOME_JSON != second.OUTCOME_JSON || first.OUTCOME_HASH != second.OUTCOME_HASH || second.Restore(context) != outcome || await DataAccess.RequireReadScopeAsync() != context)
            throw new InvalidOperationException("The retained permit outcome changed during recovery.");
        PPDMDataSourceTransaction.RequireIdle(source); return outcome;
    }
    public async Task<PermitApplicationCommandOutcome> RecordCommandAsync(PermitApplicationCommand original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require();
        // A confirmed retained original is reconciled without requiring a new mutation permission or effect.
        var recovered = await RecoverCommandAsync(original); if (recovered is not null) return recovered;
        var context = original.Review.Context; var actor = context.Field.ActorUserId;
        var source = OpenReviewedSource(context);
        await DataAccess.WithCommandAuthorityAsync(original, async authority =>
        {
            async Task Guard()
            {
                await authority();
                if (!ReferenceEquals(source,Editor.GetDataSource(context.Database.ConnectionName))) throw new InvalidOperationException("The permit command datasource changed.");
                PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
            }
            await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
            {
                await Guard(); await ValidateCommandSchemaAsync(source,null);
                var id = original.Review.ReadApplication().PERMIT_APPLICATION_ID;
                var recordRepository = await CreateRepositoryAsync<PERMIT_APPLICATION_COMMAND>(nameof(PERMIT_APPLICATION_COMMAND), id);
                var prior = await recordRepository.GetByIdAsync(original.IntentId.ToString("N")) as PERMIT_APPLICATION_COMMAND;
                if (prior is not null)
                {
                    if (prior.Restore(context).Original != original) throw new InvalidOperationException("The permit intent belongs to another original.");
                    return;
                }
                if(original.Action=="CREATE_DRAFT")
                {
                    if(await GetByIdAsync(id) is not null)
                        throw RefusalException.Conflict("The proposed application identity is already recorded. Only its exact creation receipt can confirm this original.");
                }
                else if (original.Review != await ReviewSourceAsync(id))
                    throw RefusalException.Conflict("The permit source changed. Review its current evidence before creating a new original.");
                await Guard();
                if(original.Action=="EDIT_DRAFT" && original.DraftChanges!.ChangesClassification(original.Review.ReadApplication()))
                {
                    foreach(var type in new[]{typeof(DRILLING_PERMIT_APPLICATION),typeof(ENVIRONMENTAL_PERMIT_APPLICATION),typeof(INJECTION_PERMIT_APPLICATION),typeof(APPLICATION_ATTACHMENT)})
                    {
                        var dependencies=new PPDMGenericRepository(Editor,CommonColumnHandler,Defaults,Metadata,type,context.Database.ConnectionName,type.Name,rowAccess:DataAccess.ForRead(type,type.Name,id));
                        if((await dependencies.GetAsync(new List<AppFilter>())).Any())
                            throw RefusalException.Conflict("Draft classification changes require separately reviewed subtype/document amendments while supporting records are active.");
                    }
                }
                var prepared = PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow);
                await ValidateCommandSchemaAsync(source,prepared);
                var application = prepared.ReadApplication(); var history = prepared.ReadHistory();
                async Task Write(object row, PPDMRowMutation operation, object? previous = null)
                {
                    await Guard();
                    var type = row.GetType();
                    var policy = new PermitCommandMutation(DataAccess.ForRead(type,type.Name,type == typeof(PERMIT_APPLICATION) ? null : id),source,row,previous,operation,actor,Guard);
                    var repository = new PPDMGenericRepository(Editor,CommonColumnHandler,Defaults,Metadata,type,context.Database.ConnectionName,type.Name,rowAccess:policy);
                    if (operation == PPDMRowMutation.Update) await repository.UpdateAsync(row,actor); else await repository.InsertAsync(row,actor);
                }
                if(original.Action=="CREATE_DRAFT")await Write(application,PPDMRowMutation.Insert);
                else await Write(application,PPDMRowMutation.Update,original.Review.ReadApplication());
                await Write(history,PPDMRowMutation.Insert);
                var savedApplication = await GetByIdAsync(id) ?? throw new InvalidOperationException("The saved permit is unavailable.");
                var historyRepository = await CreateRepositoryAsync<PERMIT_STATUS_HISTORY>(nameof(PERMIT_STATUS_HISTORY),id);
                var savedHistory = await historyRepository.GetByIdAsync(history.PERMIT_STATUS_HISTORY_ID) as PERMIT_STATUS_HISTORY ?? throw new InvalidOperationException("The saved permit history is unavailable.");
                var saved = prepared with {ApplicationJson=JsonSerializer.Serialize(savedApplication),HistoryJson=JsonSerializer.Serialize(savedHistory)}; saved.Require();
                await ValidateCommandSchemaAsync(source,saved);
                var expected = PERMIT_APPLICATION_COMMAND.Create(saved); await Write(expected,PPDMRowMutation.Insert);
                var recorded = await recordRepository.GetByIdAsync(original.IntentId.ToString("N")) as PERMIT_APPLICATION_COMMAND;
                if (recorded is null || recorded.Restore(context) != saved) throw new InvalidOperationException("The permit command original/result could not be read back.");
                if (PermitApplicationCommandOutcome.Payload(await GetByIdAsync(id) ?? throw new InvalidOperationException("The saved permit disappeared.")) != PermitApplicationCommandOutcome.Payload(savedApplication) ||
                    PermitApplicationCommandOutcome.Payload(await historyRepository.GetByIdAsync(history.PERMIT_STATUS_HISTORY_ID) ?? throw new InvalidOperationException("The saved history disappeared.")) != PermitApplicationCommandOutcome.Payload(savedHistory))
                    throw new InvalidOperationException("The permit result changed before commit.");
                await Guard();
            });
            return true;
        });
        return await RecoverCommandAsync(original) ?? throw new InvalidOperationException("The permit commit is unconfirmed. Retain the original and reconcile it.");
    }
    private sealed class PermitCommandMutation(IPPDMRowAccessPolicy reads,IDataSource source,object expected,object? previous,
        PPDMRowMutation operation,string actor,Func<Task> guard) : IPPDMRowAccessPolicy
    {
        private readonly string payload = PermitApplicationCommandOutcome.Payload(expected,"ROW_CREATED_DATE","ROW_CHANGED_DATE");
        private readonly string? before = previous is null ? null : PermitApplicationCommandOutcome.Payload(previous);
        private readonly DateTime? changedAfter = (expected as IPPDMEntity)?.ROW_CHANGED_DATE;
        private readonly DateTime? createdExpected = (expected as IPPDMEntity)?.ROW_CREATED_DATE;
        private int used;
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>reads.GetReadConjunctionsAsync(type,table);
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>reads.GetReadConstraintsAsync(type,table);
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>reads.ValidateRowsAsync(type,table,rows);
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>reads.GetEffectiveAccessLevelAsync(type,table,row,inherited);
        public async Task ValidateMutationAsync(Type type,string table,object row,object? existing,PPDMRowMutation mutation,string? user)
        {
            PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
            if (type!=expected.GetType() || table!=type.Name || row.GetType()!=type || mutation!=operation || user!=actor ||
                PermitApplicationCommandOutcome.Payload(row,"ROW_CREATED_DATE","ROW_CHANGED_DATE")!=payload ||
                (existing is null ? null : PermitApplicationCommandOutcome.Payload(existing))!=before || Interlocked.Exchange(ref used,1)!=0)
                throw new UnauthorizedAccessException("The permit mutation differs from its reviewed owned command.");
            if (row is not IPPDMEntity audit || audit.ACTIVE_IND != "Y" || audit.ROW_CHANGED_BY != actor ||
                audit.ROW_CHANGED_DATE is not {Kind: DateTimeKind.Utc} changed || changedAfter is not { } earliest || changed < earliest ||
                operation == PPDMRowMutation.Insert && (audit.ROW_CREATED_BY != actor || audit.ROW_CREATED_DATE is not {Kind: DateTimeKind.Utc} created ||
                    created < earliest || changed < created) ||
                operation == PPDMRowMutation.Update && audit.ROW_CREATED_DATE != createdExpected)
                throw new UnauthorizedAccessException("The permit mutation cannot preserve its reviewed actor and audit timestamps.");
            await guard(); PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        }
    }
}
