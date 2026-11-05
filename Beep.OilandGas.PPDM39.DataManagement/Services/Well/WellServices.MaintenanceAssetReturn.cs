using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using NativeWell=Beep.OilandGas.PPDM39.Models.WELL;
namespace Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;

public partial class WellServices:ICompanyMaintenanceAssetReturnOwner
{
    private static readonly System.Diagnostics.ActivitySource AssetReturnActivities=new("Beep.OilandGas.WorkOrderPermitDecision");
    private const string AssetReturnTable="WORK_ORDER_ASSET_RETURN";
    private sealed record AssetReturnSnapshot(NativeWell[] Wells,WELL_STATUS[] Statuses,R_WELL_STATUS[] References,R_TIMEZONE[] Zones)
    {
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public CompanyMaintenanceQualifiedReturnProof? QualifiedProof { get; init; }
    }
    private static string AssetReturnGuid(string value)=>Guid.ParseExact(ProcessDefinitionRevision.FromJson(value)[..32],"N").ToString("D").ToUpperInvariant();
    private static string AssetReturnRowDigest(WORK_ORDER_ASSET_RETURN row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    private PPDMGenericRepository AssetReturnRepository()=>new(_editor,_wellRepository.CommonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_ASSET_RETURN),_connectionName,AssetReturnTable);
    private void RequireAssetReturnSchema(TheTechIdea.Beep.IDataSource source,CompanyMaintenanceAssetReturnCommand? command=null)
    {
        if(!source.CheckEntityExist(AssetReturnTable))throw new InvalidOperationException("Review and apply the explicit PPDM_CORE Beep asset-return intent migration before activation.");
        var actual=source.GetEntityStructure(new EntityStructure{EntityName=AssetReturnTable,DatasourceEntityName=AssetReturnTable},true);var expected=_editor.classCreator.ConvertToEntityStructure(typeof(WORK_ORDER_ASSET_RETURN));
        if(actual?.Fields is null || expected?.Fields is null || actual.Fields.GroupBy(row=>row.FieldName,StringComparer.OrdinalIgnoreCase).Any(group=>group.Count()!=1) || !actual.Fields.Where(row=>row.IsKey).Select(row=>row.FieldName).SequenceEqual(new[]{"RETURN_ID"},StringComparer.Ordinal))throw new InvalidOperationException("Complete native return-intent schema and unique original-source key are required.");
        foreach(var field in expected.Fields){var found=actual.Fields.SingleOrDefault(row=>row.FieldName==field.FieldName);if(found is null || found.Fieldtype!=field.Fieldtype || !field.AllowDBNull && found.AllowDBNull || field.Size1>0 && found.Size1<field.Size1 || field.IsKey && !found.IsKey)throw new InvalidOperationException("Return intent type/nullability/body capacity differs from its explicit Beep manifest.");}
        if(command is null || command.Wells.Count==0)return;var status=source.GetEntityStructure(new EntityStructure{EntityName="WELL_STATUS",DatasourceEntityName="WELL_STATUS"},true);var statusModel=_editor.classCreator.ConvertToEntityStructure(typeof(WELL_STATUS));var keys=new[]{"UWI","SOURCE","STATUS_ID"};
        if(status?.Fields is null || !status.Fields.Where(row=>row.IsKey).Select(row=>row.FieldName).Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal),StringComparer.Ordinal) || status.Fields.GroupBy(row=>row.FieldName,StringComparer.OrdinalIgnoreCase).Any(group=>group.Count()!=1))throw new InvalidOperationException("Native well status requires its complete unique UWI/SOURCE/STATUS_ID key.");
        foreach(var field in statusModel.Fields){var found=status.Fields.SingleOrDefault(row=>row.FieldName==field.FieldName);if(found is null || found.Fieldtype!=field.Fieldtype)throw new InvalidOperationException("Complete canonical native well-status column types are required.");}
        var id=status.Fields.Single(row=>row.FieldName=="STATUS_ID");var remark=status.Fields.Single(row=>row.FieldName=="REMARK");if(id.Size1>0 && id.Size1<35 || remark.Size1>0 && remark.Size1<command.Reason.Length)throw new InvalidOperationException("Native status identity or reason capacity cannot retain this original return.");
    }
    private async Task<AssetReturnSnapshot> ReadAssetReturnSnapshot(CompanyMaintenanceAssetReturnCommand command,Func<Task> guard)
    {
        if(command.Wells.Count==0){await guard();return new([],[],[],[]);}
        async Task<T[]> Read<T>(List<AppFilter> filters) where T:class
        {
            await guard();var raw=(await MaintenanceWellProofRepository<T>().GetAsync(filters)).ToArray();if(raw.Length>256 || raw.Any(row=>row is not T || filters.Any(filter=>typeof(T).GetProperty(filter.FieldName)?.GetValue(row)?.ToString()!=filter.FilterValue)))throw new InvalidOperationException("Complete bounded native asset-return rows must honor exact source predicates.");var rows=raw.Cast<T>().Select(CompanyInventorySourceReview.Copy).ToArray();await guard();return rows;
        }
        var wells=new List<NativeWell>();var statuses=new List<WELL_STATUS>();foreach(var target in command.Wells.OrderBy(row=>row.WellId,StringComparer.Ordinal))
        {
            var parent=await Read<NativeWell>([new(){FieldName="UWI",Operator="=",FilterValue=target.WellId},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]);if(parent.Length!=1)throw new UnauthorizedAccessException("Every original affected well requires current company parent authority.");wells.Add(parent[0]);statuses.AddRange(await Read<WELL_STATUS>([new(){FieldName="UWI",Operator="=",FilterValue=target.WellId}]));
        }
        if(statuses.Count>4096 || statuses.Any(row=>!LeaseSummaryValidator.Identifier(row.UWI) || !LeaseSummaryValidator.Identifier(row.SOURCE) || !LeaseSummaryValidator.Identifier(row.STATUS_ID)) || statuses.Select(row=>(row.UWI,row.SOURCE,row.STATUS_ID)).Distinct().Count()!=statuses.Count)throw new InvalidOperationException("Complete unique native well-status membership is required.");
        var references=new List<R_WELL_STATUS>();foreach(var facet in CompanyMaintenanceWellEvidence.RequiredFacets)references.AddRange(await Read<R_WELL_STATUS>([new(){FieldName="STATUS_TYPE",Operator="=",FilterValue=facet}]));
        var zones=new List<R_TIMEZONE>();foreach(var zone in statuses.Select(row=>row.TIMEZONE).Where(value=>!string.IsNullOrWhiteSpace(value)).Append("UTC").Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))zones.AddRange(await Read<R_TIMEZONE>([new(){FieldName="TIMEZONE",Operator="=",FilterValue=zone}]));
        var snapshot=new AssetReturnSnapshot(wells.OrderBy(row=>row.UWI,StringComparer.Ordinal).ToArray(),statuses.OrderBy(row=>row.UWI,StringComparer.Ordinal).ThenBy(row=>row.SOURCE,StringComparer.Ordinal).ThenBy(row=>row.STATUS_ID,StringComparer.Ordinal).ToArray(),references.OrderBy(row=>row.STATUS_TYPE,StringComparer.Ordinal).ThenBy(row=>row.STATUS,StringComparer.Ordinal).ToArray(),zones.OrderBy(row=>row.TIMEZONE,StringComparer.Ordinal).ToArray());if(JsonSerializer.Serialize(snapshot).Length>2*1024*1024)throw new InvalidOperationException("Complete native asset-return proof exceeds its retained snapshot capacity.");return snapshot;
    }
    private static DateTime AssetReturnWireTime(DateTime utc,WELL_STATUS original,string? companyZone)
    {
        if(original.EFFECTIVE_DATE?.Kind==DateTimeKind.Utc)return utc;var zone=string.IsNullOrWhiteSpace(original.TIMEZONE)?companyZone:original.TIMEZONE;if(string.IsNullOrWhiteSpace(zone))throw new InvalidOperationException("Original condition wire-time interpretation is unconfirmed.");return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(utc,TimeZoneInfo.FindSystemTimeZoneById(zone)),DateTimeKind.Unspecified);
    }
    private AssetReturnSnapshot ExpectedAssetReturnSnapshot(CompanyMaintenanceAssetReturnCommand command,AssetReturnSnapshot original,DateTime utc)
    {
        command.Require();var expected=CompanyInventorySourceReview.Copy(original);
        if(command.QualifiedAssets is not null)
        {
            if(_qualifiedReturnParticipant is null || original.QualifiedProof is null || original.QualifiedProof.RecordedAtUtc!=utc)throw new InvalidOperationException("The complete original qualified participant proof and common audit UTC are required.");
            _qualifiedReturnParticipant.RequireMaintenanceQualifiedReturnProof(command,original.QualifiedProof);
        }
        else if(original.QualifiedProof is not null)throw new InvalidOperationException("Qualified proof cannot extend an original well-only command.");
        if(command.Wells.Count==0)
        {
            if(original.Wells.Length!=0 || original.Statuses.Length!=0 || original.References.Length!=0 || original.Zones.Length!=0)throw new InvalidOperationException("A qualified-only command cannot retain unrelated well proof.");
            return expected;
        }
        var statuses=expected.Statuses.ToList();var context=command.Review.Assets.Context;var zones=original.Zones.Where(row=>row.TIMEZONE=="UTC").ToArray();
        if(zones.Length!=1 || zones[0].ACTIVE_IND!="Y")throw new InvalidOperationException("A recorded native UTC reference is required for the new activation status.");var temporal=true;var zoneFrom=MaintenanceWellUtc(zones[0].EFFECTIVE_DATE,"UTC",ref temporal);var zoneTo=MaintenanceWellUtc(zones[0].EXPIRY_DATE,"UTC",ref temporal);if(!temporal || zoneFrom>command.ActivatedAtUtc || zoneTo<=command.ActivatedAtUtc)throw new InvalidOperationException("The native UTC reference does not cover actual activation.");
        foreach(var target in command.Wells)
        {
            var evidence=command.Review.Assets.Wells.Single(row=>row.WellId==target.WellId);var parent=original.Wells.Single(row=>row.UWI==target.WellId);if(MaintenanceWellRowRevision(parent)!=evidence.WellRevision)throw new InvalidOperationException("Original native well changed before asset return.");var valid=true;var from=MaintenanceWellUtc(parent.EFFECTIVE_DATE,_wellEvidenceTimeZoneId,ref valid);var to=MaintenanceWellUtc(parent.EXPIRY_DATE,_wellEvidenceTimeZoneId,ref valid);if(!valid || from is null || from>command.ActivatedAtUtc || to<=command.ActivatedAtUtc)throw new InvalidOperationException("Native well period does not cover explicit activation UTC.");
            var fact=evidence.Facets.Single(row=>row.Facet=="Condition" && row.ActiveIndicator=="Y" && row.Covers(evidence.ComparedAtUtc));var before=statuses.Single(row=>row.UWI==target.WellId && row.SOURCE==fact.Source && row.STATUS_ID==fact.StatusId);if(MaintenanceWellRowRevision(before)!=fact.Revision || statuses.Any(row=>row.UWI==target.WellId && row.SOURCE==target.RecordingSource && row.STATUS_ID==command.StatusId(target.WellId)))throw new InvalidOperationException("Exact original condition or unique successor key differs.");
            var expired=CompanyInventorySourceReview.Copy(before);expired.ACTIVE_IND="N";expired.EXPIRY_DATE=AssetReturnWireTime(command.ActivatedAtUtc,before,_wellEvidenceTimeZoneId);expired.END_TIME=expired.EXPIRY_DATE;expired.ROW_CHANGED_BY=context.ActorUserId;expired.ROW_CHANGED_DATE=utc;statuses[statuses.IndexOf(before)]=expired;
            var active=CompanyInventorySourceReview.Copy(before);active.STATUS_ID=command.StatusId(target.WellId);active.SOURCE=target.RecordingSource;active.ACTIVE_IND="Y";active.STATUS="Active";active.TIMEZONE="UTC";active.EFFECTIVE_DATE=command.ActivatedAtUtc;active.START_TIME=command.ActivatedAtUtc;active.STATUS_DATE=command.ActivatedAtUtc;active.EXPIRY_DATE=null;active.END_TIME=null;active.PERCENT_CAPABILITY=target.ObservedCapabilityPercent!.Value;active.REMARK=command.Reason;active.PPDM_GUID=AssetReturnGuid(command.ReturnId()+"|"+target.WellId);active.ROW_CREATED_BY=context.ActorUserId;active.ROW_CREATED_DATE=utc;active.ROW_CHANGED_BY=context.ActorUserId;active.ROW_CHANGED_DATE=utc;active.ROW_EFFECTIVE_DATE=utc;active.ROW_EXPIRY_DATE=null;active.ROW_QUALITY=null;statuses.Add(active);
        }
        return expected with{Statuses=statuses.OrderBy(row=>row.UWI,StringComparer.Ordinal).ThenBy(row=>row.SOURCE,StringComparer.Ordinal).ThenBy(row=>row.STATUS_ID,StringComparer.Ordinal).ToArray()};
    }
    private WORK_ORDER_ASSET_RETURN PrepareAssetReturnIntent(CompanyMaintenanceAssetReturnCommand command,string before,string after,DateTime utc)
    {
        command.Require();var context=command.Review.Assets.Context;var row=new WORK_ORDER_ASSET_RETURN{RETURN_ID=command.ReturnId(),OWNER_TENANT_ID=context.Company.TenantId,OWNER_COMPANY_BA_ID=context.Company.BusinessAssociateId,FIELD_ID=context.FieldId!,WORK_ORDER_ID=command.Reference().Query.Task.WorkOrderId,PROCESS_INSTANCE_ID=command.Reference().Query.Task.InstanceId,SUBMISSION_ID=command.Reference().Query.SubmissionId,ACTOR_USER_ID=context.ActorUserId,ASSIGNED_ROLE=command.Review.Physical.Assignment.Stage.AssignedRole,INTENT_ID=command.IntentId.ToString("D"),ACTIVATED_AT_UTC=command.ActivatedAtUtc,RECORDED_AT_UTC=utc,SOURCE_REVISION=command.Review.SourceRevision(),COMMAND_JSON=JsonSerializer.Serialize(command),BEFORE_JSON=before,AFTER_JSON=after,ACTIVE_IND="Y",SOURCE="OILGAS_ASSET_RETURN",PPDM_GUID=AssetReturnGuid(command.ReturnId()),ROW_CREATED_BY=context.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_BY=context.ActorUserId,ROW_CHANGED_DATE=utc,ROW_EFFECTIVE_DATE=utc};row.RECORDED_REVISION=AssetReturnRowDigest(row);return row;
    }
    private async Task<WORK_ORDER_ASSET_RETURN?> ReadAssetReturnIntent(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<Task> guard,bool privateRead)
    {
        await guard();var rows=(await AssetReturnRepository().GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId},new(){FieldName="PROCESS_INSTANCE_ID",Operator="=",FilterValue=query.Task.InstanceId},new(){FieldName="SUBMISSION_ID",Operator="=",FilterValue=query.SubmissionId}])).ToArray();
        if(rows.Length>1 || rows.Any(row=>row is not WORK_ORDER_ASSET_RETURN))throw new InvalidOperationException("One original complete native asset-return intent is required.");if(rows.Length==0)return null;var row=CompanyInventorySourceReview.Copy((WORK_ORDER_ASSET_RETURN)rows[0]);if(row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.FIELD_ID!=context.FieldId || row.WORK_ORDER_ID!=query.Task.WorkOrderId || row.PROCESS_INSTANCE_ID!=query.Task.InstanceId || row.SUBMISSION_ID!=query.SubmissionId || privateRead && row.ACTOR_USER_ID!=context.ActorUserId)throw new UnauthorizedAccessException("Original native asset-return company/account/FIELD/report closes before private proof decoding.");await guard();return row;
    }
    private async Task<CompanyMaintenanceAssetReturnRecovery> AssetReturnReceipt(FieldSelectionReview context,CompanyMaintenanceAssetReturnReference reference,Func<string?,Task> authority,Func<Task> guard,bool privateRead=true,bool owned=false)
    {
        using var activity=AssetReturnActivities.StartActivity(owned?"maintenance.asset_return.receipt.owned":"maintenance.asset_return.receipt.current");
        var row=await ReadAssetReturnIntent(context,reference.Query,guard,privateRead)??throw new InvalidOperationException("Original asset return is unconfirmed; recovery cannot activate it.");if(row.INTENT_ID!=reference.IntentId.ToString("D"))throw new UnauthorizedAccessException("Original asset-return reference belongs to another intent.");if(string.IsNullOrWhiteSpace(row.COMMAND_JSON) || string.IsNullOrWhiteSpace(row.BEFORE_JSON) || string.IsNullOrWhiteSpace(row.AFTER_JSON) || row.COMMAND_JSON.Length>CompanyMaintenanceAssetReturnCommand.MaximumBodyLength || row.BEFORE_JSON.Length>2*1024*1024 || row.AFTER_JSON.Length>2*1024*1024)throw new InvalidOperationException("Original bounded return proof is unavailable.");var command=JsonSerializer.Deserialize<CompanyMaintenanceAssetReturnCommand>(row.COMMAND_JSON)??throw new InvalidOperationException("Original asset-return body is unavailable.");command.Require();await authority(command.Review.Physical.Assignment.Stage.AssignedRole);
        var before=JsonSerializer.Deserialize<AssetReturnSnapshot>(row.BEFORE_JSON)??throw new InvalidOperationException("Original complete native preimage is unavailable.");var expected=ExpectedAssetReturnSnapshot(command,before,row.RECORDED_AT_UTC);if(command.Reference()!=reference || command.Review.Assets.Context.Company!=context.Company || command.Review.Assets.Context.Database!=context.Database || command.Review.Assets.Context.ActorUserId!=row.ACTOR_USER_ID || privateRead && row.ACTOR_USER_ID!=context.ActorUserId || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(PrepareAssetReturnIntent(command,row.BEFORE_JSON,row.AFTER_JSON,row.RECORDED_AT_UTC)) || JsonSerializer.Serialize(expected)!=row.AFTER_JSON)throw new InvalidOperationException("Original native return body/company/actor/UTC/preimage/effect proof differs.");
        var actual=await ReadAssetReturnSnapshot(command,guard);
        if(command.QualifiedAssets is not null)
        {
            await _qualifiedReturnParticipant!.VerifyMaintenanceQualifiedReturnAsync(command,before.QualifiedProof!,context,owned,privateRead,guard);
            actual=actual with{QualifiedProof=before.QualifiedProof};
        }
        if(JsonSerializer.Serialize(actual)!=row.AFTER_JSON)throw new InvalidOperationException("Actual complete native well/status/reference graph differs from the recorded return.");if(privateRead)foreach(var well in actual.Wells)if(await _companyWellAccess!.GetEffectiveAccessLevelAsync(typeof(NativeWell),"WELL",well) is not ("WRITE" or "DELETE"))throw new UnauthorizedAccessException("Every original affected well requires current WRITE authority for private return recovery.");var closing=await ReadAssetReturnIntent(context,reference.Query,guard,privateRead);if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original immutable return intent changed during recovery.");var result=new CompanyMaintenanceAssetReturnRecovery(command,new(context.Company,context.Database,command.Review.Assets.Context.ActorUserId,command.IntentId,row.RETURN_ID,reference.Query,command.Wells.Select(value=>value.WellId).Order(StringComparer.Ordinal).ToArray(),row.ACTIVATED_AT_UTC,row.RECORDED_AT_UTC,row.RECORDED_REVISION));result.Require(context.Company,command.Review.Assets.Context.ActorUserId,reference);await guard();return result;
    }
    public async Task<CompanyMaintenanceAssetReturnRecovery> RecoverMaintenanceAssetReturnAsync(FieldSelectionReview context,CompanyMaintenanceAssetReturnReference reference,Func<string?,Task> authority)
    {
        reference.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);if(context.Database.ConnectionName!=_connectionName || _companyWellAccess is null)throw new UnauthorizedAccessException("Original reviewed company CORE well owner is required.");var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original CORE source is unavailable.");async Task Guard(){PPDMDataSourceTransaction.RequireIdle(source);await authority(null);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original asset-return source changed.");RequireAssetReturnSchema(source);}await Guard();return await AssetReturnReceipt(context,reference,authority,Guard);
    }
    public async Task<CompanyMaintenanceAssetReturnFacts?> ReadMaintenanceAssetReturnFactsAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<Task> authority)
    {
        query.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        if(context.Database.ConnectionName!=_connectionName || _companyWellAccess is null)throw new UnauthorizedAccessException("Current company CORE well read owner is required for recorded return facts.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original native return source is unavailable.");
        async Task Guard(){PPDMDataSourceTransaction.RequireIdle(source);await authority();if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original native return source changed during public observation.");RequireAssetReturnSchema(source);}
        await Guard();var row=await ReadAssetReturnIntent(context,query,Guard,false);if(row is null){await Guard();return null;}
        if(!Guid.TryParseExact(row.INTENT_ID,"D",out var id) || id==Guid.Empty)throw new InvalidOperationException("Original recorded return reference is invalid.");
        var retained=await AssetReturnReceipt(context,new(query,id),_=>authority(),Guard,false);var facts=CompanyMaintenanceAssetReturnFacts.From(retained);await Guard();return facts;
    }
    public async Task<CompanyMaintenanceAssetReturnRecovery> RecordMaintenanceAssetReturnAsync(CompanyMaintenanceAssetReturnCommand original,Func<string?,Task> authority,Func<bool,Task<CompanyWorkOrderReturnReview>> readPhysical,Func<bool,Task<CompanyMaintenanceClosureFacts?>> readClosure)
    {
        using var activity=AssetReturnActivities.StartActivity("maintenance.asset_return.native.record");
        var command=CompanyInventorySourceReview.Copy(original);command.Require();if(command.QualifiedAssets is not null && _qualifiedReturnParticipant is null)throw new InvalidOperationException("Complete qualified native return participation must be joined before any asset can change.");var context=command.Review.Assets.Context;var role=command.Review.Physical.Assignment.Stage.AssignedRole;if(context.Database.ConnectionName!=_connectionName || _companyWellAccess is null)throw new UnauthorizedAccessException("Original company CORE well owner is required.");var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original CORE asset source is unavailable.");
        async Task Guard(bool owned){if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority(role);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original CORE asset-return binding changed.");RequireAssetReturnSchema(source,command);}
        await Guard(false);var existing=await ReadAssetReturnIntent(context,command.Reference().Query,()=>Guard(false),true);if(existing is not null){if(existing.COMMAND_JSON!=JsonSerializer.Serialize(command))throw new InvalidOperationException("An original asset return exists with another body/intent.");return await RecoverMaintenanceAssetReturnAsync(context,command.Reference(),authority);}
        async Task Physical(bool owned)
        {
            using var physicalActivity=AssetReturnActivities.StartActivity(owned?"maintenance.asset_return.physical.owned":"maintenance.asset_return.physical.current");
            await Guard(owned);var physical=await readPhysical(owned);physical.RequireAcceptance();if(physical.SourceRevision()!=command.Review.Physical.SourceRevision())throw new InvalidOperationException("Current independent physical source changed before asset activation.");var closure=await readClosure(owned);if(!owned && closure is null || closure is not null && JsonSerializer.Serialize(closure)!=JsonSerializer.Serialize(command.Review.Assets.Closure))throw new InvalidOperationException("Original recorded closure changed around asset return.");await Guard(owned);
        }
        async Task Assets(bool owned)
        {
            using var wellActivity=AssetReturnActivities.StartActivity(owned?"maintenance.asset_return.wells.owned":"maintenance.asset_return.wells.current");
            foreach(var well in command.Review.Assets.Wells){var current=await ReadMaintenanceWellEvidenceCore(context,well.WellId,()=>Guard(owned),owned);if(current.SourceRevision()!=well.SourceRevision())throw new InvalidOperationException("Original current native well/facet/reference/rights evidence changed before return.");}
        }
        await Physical(false);await Assets(false);
        using(AssetReturnActivities.StartActivity("maintenance.asset_return.transaction"))await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Physical(true);await Assets(true);if(await ReadAssetReturnIntent(context,command.Reference().Query,()=>Guard(true),true) is not null)throw new InvalidOperationException("Original native return was concurrently recorded; recover it without replacement.");var before=await ReadAssetReturnSnapshot(command,()=>Guard(true));
            if(command.QualifiedAssets is not null)
            {
                using var preparationActivity=AssetReturnActivities.StartActivity("maintenance.asset_return.qualified.prepare");
                before=before with{QualifiedProof=await _qualifiedReturnParticipant!.PrepareMaintenanceQualifiedReturnAsync(command,()=>Guard(true))};
            }
            var utc=before.QualifiedProof?.RecordedAtUtc??DateTime.UtcNow;if(utc<command.Review.Physical.Completion.ComparedAtUtc || utc<command.ActivatedAtUtc)throw new InvalidOperationException("Native activation audit precedes its explicit UTC/source comparison.");var after=ExpectedAssetReturnSnapshot(command,before,utc);
            if(JsonSerializer.Serialize(before).Length>2*1024*1024 || JsonSerializer.Serialize(after).Length>2*1024*1024)throw new InvalidOperationException("Complete original mixed-asset snapshots exceed the retained native intent capacity.");
            foreach(var target in command.Wells)
            {
                var fact=command.Review.Assets.Wells.Single(row=>row.WellId==target.WellId).Facets.Single(row=>row.Facet=="Condition" && row.ActiveIndicator=="Y" && row.Covers(command.Review.Assets.Wells.Single(value=>value.WellId==target.WellId).ComparedAtUtc));var previous=before.Statuses.Single(row=>row.UWI==target.WellId && row.SOURCE==fact.Source && row.STATUS_ID==fact.StatusId);var expired=after.Statuses.Single(row=>row.UWI==target.WellId && row.SOURCE==fact.Source && row.STATUS_ID==fact.StatusId);var active=after.Statuses.Single(row=>row.UWI==target.WellId && row.SOURCE==target.RecordingSource && row.STATUS_ID==command.StatusId(target.WellId));
                await AssetReturnWriter(expired,previous,"WELL_STATUS",["UWI","SOURCE","STATUS_ID"],context.ActorUserId,PPDMRowMutation.Update,()=>Guard(true)).UpdateAsync(expired,context.ActorUserId);await AssetReturnWriter(active,null,"WELL_STATUS",["UWI","SOURCE","STATUS_ID"],context.ActorUserId,PPDMRowMutation.Insert,()=>Guard(true)).InsertAsync(active,context.ActorUserId);
            }
            if(command.QualifiedAssets is not null)
            {
                using var applyActivity=AssetReturnActivities.StartActivity("maintenance.asset_return.qualified.apply");
                await _qualifiedReturnParticipant!.ApplyMaintenanceQualifiedReturnAsync(command,before.QualifiedProof!,()=>Guard(true));
            }
            // The final owned receipt validates the entire actual well/qualified graph and retained sources
            // after inserting this uncommitted intent. Any mismatch rolls back every effect and the intent.
            var expectedJson=JsonSerializer.Serialize(after);var row=PrepareAssetReturnIntent(command,JsonSerializer.Serialize(before),expectedJson,utc);await AssetReturnWriter(row,null,AssetReturnTable,["RETURN_ID"],context.ActorUserId,PPDMRowMutation.Insert,()=>Guard(true)).InsertAsync(row,context.ActorUserId);await Physical(true);await AssetReturnReceipt(context,command.Reference(),authority,()=>Guard(true),owned:true);
        });await Physical(false);return await RecoverMaintenanceAssetReturnAsync(context,command.Reference(),authority);
    }
    private PPDMGenericRepository AssetReturnWriter<T>(T row,T? before,string table,string[] keys,string actor,PPDMRowMutation operation,Func<Task> authority) where T:class=>new(_editor,_wellRepository.CommonColumnHandler,_defaults,_metadata,typeof(T),_connectionName,table,rowAccess:new AssetReturnRowPolicy<T>(row,before,table,keys,actor,operation,authority));
    private sealed class AssetReturnRowPolicy<T>(T prepared,T? original,string expectedTable,string[] keys,string actor,PPDMRowMutation operation,Func<Task> authority):IPPDMRowAccessPolicy where T:class
    {
        private readonly T expected=CompanyInventorySourceReview.Copy(prepared);
        private readonly T? previous=original is null?null:CompanyInventorySourceReview.Copy(original);
        private async Task Require(Type type,string table){await authority();if(type!=typeof(T) || table!=expectedTable)throw new InvalidOperationException("Owned asset-return policy grants no other table/type operation.");}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table){await Require(type,table);return [];}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table){await Require(type,table);if(previous is null || operation!=PPDMRowMutation.Update)throw new InvalidOperationException("This return insert grants no general read.");return [keys.Select(key=>new AppFilter{FieldName=key,Operator="=",FilterValue=typeof(T).GetProperty(key)!.GetValue(previous)!.ToString()}).ToArray()];}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows){await Require(type,table);if(previous is null || rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(previous))throw new InvalidOperationException("Only the complete exact native condition preimage may be read for this update.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool includeInherited=true)=>throw new InvalidOperationException("Owned asset-return mutation grants no general effective access.");
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? persisted,PPDMRowMutation requested,string? user)
        {
            await Require(type,table);if(!ReferenceEquals(entity,prepared) || requested!=operation || user!=actor || (previous is null)!=(persisted is null) || previous is not null && JsonSerializer.Serialize(persisted)!=JsonSerializer.Serialize(previous))throw new InvalidOperationException("Exact original native row, compound identity, actor and mutation are required.");foreach(var name in new[]{"PPDM_GUID","ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","ROW_EFFECTIVE_DATE","ROW_EXPIRY_DATE","ROW_QUALITY"}){var property=typeof(T).GetProperty(name)??throw new InvalidOperationException("Canonical native audit columns are required.");property.SetValue(prepared,property.GetValue(expected));}if(JsonSerializer.Serialize(prepared)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Only the frozen declared native business/audit effect may be persisted.");
        }
    }
}
