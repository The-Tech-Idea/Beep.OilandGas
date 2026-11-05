using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.DevelopmentPlanning.Services;

/// <summary>Company-owned FDP drafts. All native operations use one reviewed binding and owned Beep transaction.</summary>
public partial class DevelopmentPlanService : IDevelopmentPlanService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _columns;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly Func<Task<string>> _resolve;
    private readonly ILogger<DevelopmentPlanService>? _logger;
    private const string Plans="FIELD_DEVELOPMENT_PLAN", References="R_DEVELOPMENT_PLANNING_REFERENCE_CODE";
    public DevelopmentPlanService(IDMEEditor editor,ICommonColumnHandler columns,IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,Func<Task<string>> resolveConnection,ILogger<DevelopmentPlanService>? logger=null)
    {
        _editor=editor;_columns=columns;_defaults=defaults;_metadata=metadata;
        _resolve=resolveConnection ?? throw new ArgumentNullException(nameof(resolveConnection));_logger=logger;
    }
    private static T Copy<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
    private sealed class NativeRows(IDMEEditor editor,ICommonColumnHandler columns,IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,Type type,string connection,string table)
        : PPDMGenericRepository(editor,columns,defaults,metadata,type,connection,table)
    {
        public object[] Map(object raw,Type target)=>ConvertToTypedList(raw,target).ToArray();
    }
    private static void RequireContext(FieldSelectionReview current)
    {
        FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);
        if(current.FieldId is null || !LeaseSummaryValidator.Identifier(current.Company.TenantId) ||
            !LeaseSummaryValidator.Identifier(current.Company.BusinessAssociateId) || !LeaseSummaryValidator.Identifier(current.ActorUserId))
            throw new InvalidOperationException("A current company/account/FIELD draft context is required.");
    }
    private Task<T> Owned<T>(FieldSelectionReview current,AssetDatabaseScope planning,Func<Task> authority,Func<IDataSource,string,Func<Task>,Task<T>> action)
    {
        RequireContext(current);return Owned(planning,authority,action);
    }
    private async Task<T> Owned<T>(AssetDatabaseScope planning,Func<Task> authority,Func<IDataSource,string,Func<Task>,Task<T>> action)
    {
        ArgumentNullException.ThrowIfNull(authority);await authority();
        var connection=await _resolve();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The company development-planning binding is unavailable.");
        if(connection!=planning.ConnectionName)throw new InvalidOperationException("The original development-planning binding changed.");
        var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("The reviewed development-planning source is unavailable.");
        MigrationConnectionTarget.Validate(_editor,source,connection);
        async Task Guard()
        {
            await authority();
            if(await _resolve()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))
                throw new InvalidOperationException("The original development-planning source changed.");
            MigrationConnectionTarget.Validate(_editor,source,connection);
            PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        }
        T result=default!;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{await Guard();RequireSchema(source);await Guard();result=await action(source,connection,Guard);await Guard();});
        await authority();if(await _resolve()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The development-planning binding changed after transaction completion.");
        MigrationConnectionTarget.Validate(_editor,source,connection);
        return Copy(result);
    }
    private static void RequireSchema(IDataSource source)
    {
        EntityStructure Schema(string table)=>source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true)
            ?? throw new InvalidOperationException("Development-planning schema is unavailable; review its Beep migration.");
        var plans=Schema(Plans);var refs=Schema(References);
        void Field(EntityStructure schema,string name,Type type,int width=0,bool? nullable=null)
        {
            var fields=schema.Fields?.Where(field=>field.FieldName==name).ToArray();
            if(fields is not {Length:1} || fields[0].Fieldtype!=type.FullName || width>0 && Math.Max(fields[0].Size,fields[0].Size1)<width ||
                nullable is {} optional && fields[0].AllowDBNull!=optional)
                throw new InvalidOperationException($"Review development-planning column {schema.EntityName}.{name} through Beep Migration.");
        }
        var keys=plans.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!="FDP_ID")throw new InvalidOperationException("An exact FDP_ID primary key is required.");
        foreach(var (name,width) in new[]{("FDP_ID",128),("TENANT_ID",128),("BUSINESS_ASSOCIATE_ID",128),("DRAFT_INTENT_ID",36),("DRAFT_REQUEST_REVISION",64)})Field(plans,name,typeof(string),width,false);
        foreach(var (name,width) in new[]{("FIELD_ID",128),("FDP_NAME",200),("FDP_STATUS",5),("DEVELOPMENT_CONCEPT_DESC",4000),("CAPEX_CURRENCY",3),("ROW_CREATED_BY",128),("ACTIVE_IND",1),("APPROVED_BY",128)})Field(plans,name,typeof(string),width);
        Field(plans,"FDP_VERSION",typeof(int));Field(plans,"TOTAL_CAPEX_MM",typeof(decimal));
        foreach(var name in new[]{"FIRST_PRODUCTION_DATE","FIELD_LIFE_END","ROW_CREATED_DATE","APPROVAL_DATE","SUBMISSION_DATE"})Field(plans,name,typeof(DateTime));
        if(!refs.Fields.Where(field=>field.IsKey).Select(field=>field.FieldName).Order(StringComparer.Ordinal).SequenceEqual(new[]{"REFERENCE_CODE","REFERENCE_SET"}))
            throw new InvalidOperationException("The development reference set/code primary key is required.");
        Field(refs,"REFERENCE_SET",typeof(string),128,false);Field(refs,"REFERENCE_CODE",typeof(string),128,false);Field(refs,"ACTIVE_IND",typeof(string),1);
    }
    private async Task<T[]> Read<T>(IDataSource source,string connection,string table,List<AppFilter> filters,Func<Task> guard)
    {
        await guard();RequireSchema(source);
        var raw=await source.GetEntityAsync(table,filters) ?? throw new InvalidOperationException("The development source returned no confirmed rowset.");
        if(source.ErrorObject is {} readResult && !BeepOperationResult.IsSuccess(readResult))throw new InvalidOperationException("The development source read failed.");
        await guard();var mapper=new NativeRows(_editor,_columns,_defaults,_metadata,typeof(T),connection,table);
        var mapped=mapper.Map(raw,typeof(T));
        if(mapped.Any(row=>row is not T))throw new InvalidOperationException("The development source returned an unexpected record type.");
        return Copy(mapped.Cast<T>().ToArray());
    }
    private static AppFilter Filter(string field,string value)=>new(){FieldName=field,Operator="=",FilterValue=value};
    private static List<AppFilter> Scope(FieldSelectionReview current,string? id=null)
    {
        var filters=new List<AppFilter>{Filter("TENANT_ID",current.Company.TenantId),Filter("BUSINESS_ASSOCIATE_ID",current.Company.BusinessAssociateId),Filter("FIELD_ID",current.FieldId!),Filter("ACTIVE_IND","Y")};
        if(id is not null)
        {
            // The original identity must remain observable when its FIELD or active flag was changed.
            // A changed record is an unconfirmed outcome, never a confirmed absence.
            return [Filter("FDP_ID",id)];
        }
        filters.Add(Filter("FDP_STATUS","DRAFT"));return filters;
    }
    private static void RequireRows(FIELD_DEVELOPMENT_PLAN[] rows,FieldSelectionReview current,string? id=null)=>
        DevelopmentPlanDraftReview.RequirePopulation(rows,current,id);
    private async Task<string[]> Currencies(IDataSource source,string connection,Func<Task> guard)
    {
        var rows=await Read<R_DEVELOPMENT_PLANNING_REFERENCE_CODE>(source,connection,References,[Filter("REFERENCE_SET","COST_CURRENCY"),Filter("ACTIVE_IND","Y")],guard);
        if(rows.Length>1000 || rows.Any(row=>row.REFERENCE_SET!="COST_CURRENCY" || row.ACTIVE_IND!="Y" || row.REFERENCE_CODE is not {Length:3} || row.REFERENCE_CODE.Any(c=>c is <'A' or >'Z')) ||
            rows.Select(row=>row.REFERENCE_CODE).Distinct(StringComparer.Ordinal).Count()!=rows.Length)throw new InvalidOperationException("The active development cost-currency catalog is unavailable or ambiguous.");
        return rows.Select(row=>row.REFERENCE_CODE).Order(StringComparer.Ordinal).ToArray();
    }
    private async Task RequireReferences(IDataSource source,string connection,CreateDevelopmentPlan request,Func<Task> guard)
    {
        var rows=await Read<R_DEVELOPMENT_PLANNING_REFERENCE_CODE>(source,connection,References,[Filter("REFERENCE_SET","FDP_STATUS"),Filter("REFERENCE_CODE","DRAFT"),Filter("ACTIVE_IND","Y")],guard);
        if(rows.Length!=1 || rows[0].REFERENCE_SET!="FDP_STATUS" || rows[0].REFERENCE_CODE!="DRAFT" || rows[0].ACTIVE_IND!="Y")throw new InvalidOperationException("The active configured FDP draft status is required.");
        if(request.CapexCurrency is not null && !(await Currencies(source,connection,guard)).Contains(request.CapexCurrency,StringComparer.Ordinal))throw new InvalidOperationException("Choose an active configured development cost currency.");
    }
    public Task<DevelopmentPlanDraftOptions> GetDraftOptionsAsync(FieldSelectionReview current,AssetDatabaseScope planning,Func<Task> authority)=>Owned(current,planning,authority,async(source,connection,guard)=>new DevelopmentPlanDraftOptions(current,planning,await Currencies(source,connection,guard)));
    public Task<DevelopmentPlanDraftList> GetDevelopmentPlansAsync(FieldSelectionReview current,AssetDatabaseScope planning,Func<Task> authority)=>Owned(current,planning,authority,async(source,connection,guard)=>
    {
        var rows=await Read<FIELD_DEVELOPMENT_PLAN>(source,connection,Plans,Scope(current),guard);RequireRows(rows,current);
        var closing=await Read<FIELD_DEVELOPMENT_PLAN>(source,connection,Plans,Scope(current),guard);RequireRows(closing,current);
        if(JsonSerializer.Serialize(rows.OrderBy(row=>row.FDP_ID,StringComparer.Ordinal))!=JsonSerializer.Serialize(closing.OrderBy(row=>row.FDP_ID,StringComparer.Ordinal)))throw new InvalidOperationException("The development drafts changed during review.");
        return new DevelopmentPlanDraftList(current,planning,closing.OrderByDescending(row=>row.ROW_CREATED_DATE).ThenBy(row=>row.FDP_ID,StringComparer.Ordinal).ToArray(),DateTimeOffset.UtcNow);
    });
    public Task<DevelopmentPlanSourceReview> GetDevelopmentPlanAsync(FieldSelectionReview current,AssetDatabaseScope planning,string id,Func<Task> authority)
    {
        if(!LeaseSummaryValidator.Identifier(id))throw new ArgumentException("A recorded development draft identity is required.");
        return Owned(current,planning,authority,async(source,connection,guard)=>
        {
            var first=await Read<FIELD_DEVELOPMENT_PLAN>(source,connection,Plans,Scope(current,id),guard);RequireRows(first,current,id);
            var closing=await Read<FIELD_DEVELOPMENT_PLAN>(source,connection,Plans,Scope(current,id),guard);RequireRows(closing,current,id);
            if(first.Length>1 || closing.Length>1 || JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))
                throw new InvalidOperationException("The recorded development draft is ambiguous or changed during review.");
            var row=closing.SingleOrDefault();
            var result=new DevelopmentPlanSourceReview(current,planning,id,row,DevelopmentPlanSourceReview.RevisionFor(current,planning,id,row),DateTimeOffset.UtcNow);
            result.Require(current,id);return result;
        });
    }
    public async Task<FIELD_DEVELOPMENT_PLAN> CreateDevelopmentPlanAsync(CreateDevelopmentPlan request,Func<Task> authority)
    {
        var original=DevelopmentPlanDraftReview.Capture(request,request.SelectionContext);var id=DevelopmentPlanDraftReview.PlanIdFor(original);
        await Owned(original.SelectionContext,original.PlanningDatabase,authority,async(source,connection,guard)=>
        {
            var rows=await Read<FIELD_DEVELOPMENT_PLAN>(source,connection,Plans,Scope(original.SelectionContext,id),guard);RequireRows(rows,original.SelectionContext,id);
            if(rows.Length>1)throw new InvalidOperationException("The original draft identity is ambiguous.");
            if(rows.Length==1){DevelopmentPlanDraftReview.RequireRecorded(original,rows[0]);return true;}
            await RequireReferences(source,connection,original,guard);await guard();
            var row=new FIELD_DEVELOPMENT_PLAN{FDP_ID=id,TENANT_ID=original.SelectionContext.Company.TenantId,BUSINESS_ASSOCIATE_ID=original.SelectionContext.Company.BusinessAssociateId,
                DRAFT_INTENT_ID=original.IntentId.ToString("D"),DRAFT_REQUEST_REVISION=DevelopmentPlanDraftReview.RequestRevision(original),FIELD_ID=original.FieldId,FDP_NAME=original.PlanName,
                FDP_VERSION=1,FDP_STATUS="DRAFT",FIRST_PRODUCTION_DATE=original.PlannedFirstProductionDate?.ToDateTime(TimeOnly.MinValue),FIELD_LIFE_END=original.PlannedFieldLifeEndDate?.ToDateTime(TimeOnly.MinValue),
                TOTAL_CAPEX_MM=original.EstimatedCapexMillions,CAPEX_CURRENCY=original.CapexCurrency ?? "",DEVELOPMENT_CONCEPT_DESC=original.Description ?? "",ACTIVE_IND="Y",PPDM_GUID=Guid.NewGuid().ToString("D"),
                ROW_CREATED_BY=original.SelectionContext.ActorUserId,ROW_CREATED_DATE=DateTime.UtcNow};
            BeepOperationResult.EnsureSuccess(source.InsertEntity(Plans,row),"Insert",Plans);
            if(source.ErrorObject is {} writeResult && !BeepOperationResult.IsSuccess(writeResult))
                throw new InvalidOperationException("The development source did not confirm the draft insert.");
            await guard();
            rows=await Read<FIELD_DEVELOPMENT_PLAN>(source,connection,Plans,Scope(original.SelectionContext,id),guard);RequireRows(rows,original.SelectionContext,id);
            if(rows.Length!=1)throw new InvalidOperationException("The original draft write could not be read back.");
            DevelopmentPlanDraftReview.RequireRecorded(original,rows[0]);await RequireReferences(source,connection,original,guard);return true;
        });
        var recovered=await RecoverDevelopmentPlanAsync(original,original.SelectionContext,original.PlanningDatabase,authority);
        return recovered.Plan ?? throw new InvalidOperationException("The committed original draft could not be confirmed. Recover its original intent before further work.");
    }
    public Task<DevelopmentPlanDraftRecovery> RecoverDevelopmentPlanAsync(CreateDevelopmentPlan original,FieldSelectionReview current,AssetDatabaseScope planning,Func<Task> authority)
    {
        original=DevelopmentPlanDraftReview.Capture(original,original.SelectionContext);
        if(current.Company!=original.SelectionContext.Company || current.ActorUserId!=original.SelectionContext.ActorUserId || current.Database!=original.SelectionContext.Database || current.FieldId!=original.FieldId || planning!=original.PlanningDatabase)
            throw new InvalidOperationException("Return to the original account/company/FIELD/database to recover this draft.");
        return Owned(current,planning,authority,async(source,connection,guard)=>
        {
            var id=DevelopmentPlanDraftReview.PlanIdFor(original);
            var first=await Read<FIELD_DEVELOPMENT_PLAN>(source,connection,Plans,Scope(current,id),guard);RequireRows(first,current,id);
            var closing=await Read<FIELD_DEVELOPMENT_PLAN>(source,connection,Plans,Scope(current,id),guard);RequireRows(closing,current,id);
            if(first.Length>1 || closing.Length>1 || JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original draft outcome is ambiguous or changed during recovery.");
            var row=closing.SingleOrDefault();if(row is not null)DevelopmentPlanDraftReview.RequireRecorded(original,row);
            return new DevelopmentPlanDraftRecovery(current,planning,original.IntentId,DevelopmentPlanDraftReview.RequestRevision(original),row,DateTimeOffset.UtcNow);
        });
    }
}
