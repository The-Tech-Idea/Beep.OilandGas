using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionAccounting.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
namespace Beep.OilandGas.ProductionAccounting.Services;

public partial class ProductionAccountingService
{
    private sealed class ProductionIntentPages<T>(ProductionAccountingService owner,string connection,string table,Func<T,string> groupKey,Func<T,string> identity,Func<T,string> command,Action<T> require,long maximumCharacters):PPDMGenericRepository(owner._editor,owner._commonColumnHandler,owner._defaults,owner._metadata,typeof(T),connection,table) where T:ModelEntityBase
    {
        public async Task<(T[] Entries,int Total)> ReadAsync(CompanyScope company,string actor,int page,int size)
        {
            var source=owner._editor.GetDataSource(connection) ??throw new InvalidOperationException("The original production intent page datasource is unavailable.");
            var raw=await PPDMDataSourceTransaction.WithSourceAsync(source,()=>{
                var result=source.GetEntity(table,[
                    new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="ROW_CREATED_BY",Operator="=",FilterValue=actor}],page,size);
                BeepOperationResult.EnsureSuccess(source.ErrorObject,"Paged read",table);return Task.FromResult(result);
            });
            if(raw is null || raw.PageNumber!=page || raw.PageSize!=size || raw.TotalRecords<0)throw new InvalidOperationException("The original production intent provider page metadata is unavailable.");
            var totalPages=raw.TotalRecords==0?0:(int)(((long)raw.TotalRecords+size-1)/size);
            if(raw.TotalPages!=totalPages || raw.HasNextPage!=((long)page*size<raw.TotalRecords) || raw.HasPreviousPage!=(page>1) || page>Math.Max(1,totalPages))throw new InvalidOperationException("The original production intent provider page counts are unconfirmed.");
            var rows=ConvertToTypedList(raw.Data,typeof(T)).Cast<T>().OrderBy(groupKey,StringComparer.Ordinal).ToArray();
            var expected=(int)Math.Min(size,Math.Max(0L,raw.TotalRecords-(long)(page-1)*size));
            if(rows.Length!=expected || rows.Select(groupKey).Distinct(StringComparer.Ordinal).Count()!=rows.Length || rows.Select(identity).Distinct(StringComparer.Ordinal).Count()!=rows.Length || rows.Sum(row=>(long)(command(row)?.Length ??0))>maximumCharacters)
                throw new InvalidOperationException("The complete bounded production intent page membership is unconfirmed.");
            foreach(var row in rows)require(row);
            return(CompanyInventorySourceReview.Copy(rows),raw.TotalRecords);
        }
    }
    public async Task<(IReadOnlyList<COMMERCIAL_TERMS_INTENT> Entries,int Total)> GetCommercialAmendmentIntentPageAsync(CompanyScope company,string actor,string connection,int page,int size,Func<Task> requireAuthority)
    {
        CompanyCommercialAmendmentPage.RequireRequest(page,size);APInvoiceOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl))throw new ArgumentException("The bounded original company account is required.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The commercial page datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The commercial page datasource changed.");}
        async Task<(COMMERCIAL_TERMS_INTENT[] Entries,int Total)> Read()
        {
            await Guard();_=CommercialIntentRepository(connection);var result=await new ProductionIntentPages<COMMERCIAL_TERMS_INTENT>(this,connection,"COMMERCIAL_TERMS_INTENT",row=>row.PROCESS_INSTANCE_ID,row=>row.COMMERCIAL_TERMS_INTENT_ID,row=>row.COMMAND_JSON,row=>{_=row.Restore(company,actor,row.PROCESS_INSTANCE_ID);},CompanyCommercialAmendmentQueue.MaximumCommandCharacters).ReadAsync(company,actor,page,size);await Guard();return result;
        }
        var first=await Read();var last=await Read();
        if(first.Total!=last.Total || JsonSerializer.Serialize(first.Entries)!=JsonSerializer.Serialize(last.Entries))throw new InvalidOperationException("The commercial provider page or total changed during discovery.");
        return(last.Entries,last.Total);
    }
    public async Task<IReadOnlyList<COMMERCIAL_TERMS_INTENT>> GetCommercialAmendmentIntentsAsync(CompanyScope company,string actor,string connection,Func<Task> requireAuthority)
    {
        APInvoiceOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl))throw new ArgumentException("The bounded original company account is required.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The commercial intent datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The commercial intent datasource changed.");}
        async Task<COMMERCIAL_TERMS_INTENT[]> Read()
        {
            await Guard();var rows=(await CommercialIntentRepository(connection).GetAsync([
                new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="ROW_CREATED_BY",Operator="=",FilterValue=actor}]))?.ToArray();
            if(rows is null || rows.Length>CompanyCommercialAmendmentQueue.MaximumEntries || rows.Any(row=>row is not COMMERCIAL_TERMS_INTENT))throw new InvalidOperationException("A complete bounded saved proposal set is required; no partial queue can be confirmed.");
            var entries=rows.Cast<COMMERCIAL_TERMS_INTENT>().OrderBy(row=>row.PROCESS_INSTANCE_ID,StringComparer.Ordinal).ToArray();
            foreach(var entry in entries)_=entry.Restore(company,actor,entry.PROCESS_INSTANCE_ID);
            if(entries.Sum(row=>(long)row.COMMAND_JSON.Length)>CompanyCommercialAmendmentQueue.MaximumCommandCharacters || entries.Select(row=>row.PROCESS_INSTANCE_ID).Distinct(StringComparer.Ordinal).Count()!=entries.Length || entries.Select(row=>row.COMMERCIAL_TERMS_INTENT_ID).Distinct(StringComparer.Ordinal).Count()!=entries.Length)
                throw new InvalidOperationException("The saved proposal set is too large or ambiguous; reconcile its complete evidence.");
            await Guard();return CompanyInventorySourceReview.Copy(entries);
        }
        var first=await Read();var last=await Read();
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(last))throw new InvalidOperationException("The complete saved proposal set changed during discovery.");return last;
    }
    private PPDMGenericRepository CommercialIntentRepository(string connection)
    {
        const string table="COMMERCIAL_TERMS_INTENT";
        var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("The commercial intent datasource is unavailable.");
        var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);
        var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!=table+"_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Review the commercial intent identity through the PRODUCTION Beep migration.");
        foreach(var (name,size) in new[]{(table+"_ID",32),("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("PROCESS_INSTANCE_ID",128),("SALES_CONTRACT_ID",128),("COMMAND_JSON",COMMERCIAL_TERMS_INTENT.MaximumCommandLength),("RECORDED_REVISION",64)})
        {
            var fields=schema!.Fields.Where(field=>field.FieldName==name).ToArray();
            if(fields is not {Length:1} || fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size)
                throw new InvalidOperationException("Review complete commercial intent metadata through the PRODUCTION Beep migration.");
        }
        return new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(COMMERCIAL_TERMS_INTENT),connection,table);
    }
    public async Task<COMMERCIAL_TERMS_INTENT?> GetCommercialAmendmentIntentAsync(CompanyScope company,string actor,string process,string connection,Func<Task> requireAuthority)
    {
        APInvoiceOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(process) || process.Length>128 || process.Any(char.IsControl) || string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl))throw new ArgumentException("The original company/account/process is required.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The commercial intent datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The commercial intent datasource changed.");}
        async Task<COMMERCIAL_TERMS_INTENT?> Read()
        {
            await Guard();var rows=(await CommercialIntentRepository(connection).GetAsync([
                new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
                new(){FieldName="ROW_CREATED_BY",Operator="=",FilterValue=actor},new(){FieldName="PROCESS_INSTANCE_ID",Operator="=",FilterValue=process}]))?.ToArray();
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not COMMERCIAL_TERMS_INTENT))throw new InvalidOperationException("A complete unique commercial intent set is required.");
            var entry=rows.SingleOrDefault() as COMMERCIAL_TERMS_INTENT;
            if(entry is not null)_=entry.Restore(company,actor,process);await Guard();return entry is null?null:CompanyInventorySourceReview.Copy(entry);
        }
        var first=await Read();var last=await Read();
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(last))throw new InvalidOperationException("The original commercial intent changed during read-back.");return last;
    }
    public async Task<COMMERCIAL_TERMS_INTENT> SaveCommercialAmendmentIntentAsync(CompanyCommercialAmendmentSubmission command,string connection,Func<Task> requireAuthority,Func<Task<RevenueSalesContractReview>> readCurrent)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();var review=command.Review;
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The commercial intent datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The commercial intent datasource changed.");PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);}
        COMMERCIAL_TERMS_INTENT? recorded=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{
            await Guard();var existing=await GetCommercialAmendmentIntentAsync(review.Company,review.ActorUserId,review.InstanceId,connection,Guard);
            if(existing is not null)
            {
                if(JsonSerializer.Serialize(existing.Restore(review.Company,review.ActorUserId,review.InstanceId))!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Reconcile the original saved amendment intent before proposing another.");
                recorded=existing;return;
            }
            var collision=(await CommercialIntentRepository(connection).GetAsync([new(){FieldName="COMMERCIAL_TERMS_INTENT_ID",Operator="=",FilterValue=command.Amendment.Preparation.IntentId}]))?.ToArray();
            if(collision is null || collision.Length!=0)throw new InvalidOperationException("The commercial intent identity is unavailable or already used.");
            var fresh=await readCurrent();
            if(JsonSerializer.Serialize(fresh.Contract)!=JsonSerializer.Serialize(review.PreviousNativeContract) ||
                JsonSerializer.Serialize(SalesContractTermsAmendment.NormalizeSource(fresh,review.Previous))!=JsonSerializer.Serialize(review.Source))
                throw new InvalidOperationException("The original reviewed amendment source changed before it could be saved.");
            var entry=COMMERCIAL_TERMS_INTENT.Create(command,DateTime.UtcNow);
            var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(COMMERCIAL_TERMS_INTENT),connection,"COMMERCIAL_TERMS_INTENT",rowAccess:new ProductionIntentInsertPolicy<COMMERCIAL_TERMS_INTENT>(entry,"COMMERCIAL_TERMS_INTENT",Guard));
            await writer.InsertAsync(entry,review.ActorUserId);await Guard();
            recorded=await GetCommercialAmendmentIntentAsync(review.Company,review.ActorUserId,review.InstanceId,connection,Guard);
            if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(entry))throw new InvalidOperationException("The saved commercial intent read-back is unconfirmed.");
        });
        async Task ClosingGuard(){await requireAuthority();if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The commercial intent datasource changed.");}
        var closing=await GetCommercialAmendmentIntentAsync(review.Company,review.ActorUserId,review.InstanceId,connection,ClosingGuard);
        if(recorded is null || JsonSerializer.Serialize(closing)!=JsonSerializer.Serialize(recorded))throw new InvalidOperationException("Commercial intent commit is uncertain. Reconcile its original process.");
        return CompanyInventorySourceReview.Copy(closing!);
    }
    private sealed class ProductionIntentInsertPolicy<T>(T expected,string ownedTable,Func<Task> guard):IPPDMRowAccessPolicy where T:ModelEntityBase
    {
        private readonly T original=CompanyInventorySourceReview.Copy(expected);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();
            if(type!=typeof(T) || table!=ownedTable || operation!=PPDMRowMutation.Insert || previous is not null || actor!=original.ROW_CREATED_BY || !ReferenceEquals(entity,expected))throw new InvalidOperationException("Only the original append-only production intent is authorized.");
            expected.ROW_CREATED_BY=original.ROW_CREATED_BY;expected.ROW_CHANGED_BY=original.ROW_CHANGED_BY;expected.ROW_CREATED_DATE=original.ROW_CREATED_DATE;expected.ROW_CHANGED_DATE=original.ROW_CHANGED_DATE;expected.ACTIVE_IND=original.ACTIVE_IND;
            if(JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The saved production intent changed before insertion.");
        }
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Intent insertion grants no reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Intent insertion grants no reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Intent insertion grants no reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Intent insertion grants no reads.");
    }
}
