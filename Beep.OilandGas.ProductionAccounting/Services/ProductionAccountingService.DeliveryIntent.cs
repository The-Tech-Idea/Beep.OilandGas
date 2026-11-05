using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionAccounting.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
namespace Beep.OilandGas.ProductionAccounting.Services;

public partial class ProductionAccountingService
{
    public async Task<(IReadOnlyList<DELIVERY_ACCEPTANCE_INTENT> Entries,int Total)> GetDeliveryAcceptanceIntentPageAsync(CompanyScope company,string actor,string connection,int page,int size,Func<Task> requireAuthority)
    {
        DELIVERY_ACCEPTANCE_INTENT.RequirePage(page,size);APInvoiceOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl))throw new ArgumentException("The bounded original company account is required.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The delivery page datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The delivery page datasource changed.");}
        async Task<(DELIVERY_ACCEPTANCE_INTENT[] Entries,int Total)> Read()
        {
            await Guard();_=DeliveryIntentRepository(connection);
            var result=await new ProductionIntentPages<DELIVERY_ACCEPTANCE_INTENT>(this,connection,"DELIVERY_ACCEPTANCE_INTENT",row=>row.SCOPE_KEY,row=>row.DELIVERY_ACCEPTANCE_INTENT_ID,row=>row.COMMAND_JSON,row=>{_=row.Restore(company,actor,row.SCOPE_KEY);},DELIVERY_ACCEPTANCE_INTENT.MaximumSetCharacters).ReadAsync(company,actor,page,size);
            await Guard();return result;
        }
        var first=await Read();var last=await Read();
        if(first.Total!=last.Total || JsonSerializer.Serialize(first.Entries)!=JsonSerializer.Serialize(last.Entries))throw new InvalidOperationException("The delivery provider page or total changed during discovery.");
        return(last.Entries,last.Total);
    }
    private PPDMGenericRepository DeliveryIntentRepository(string connection)
    {
        const string table="DELIVERY_ACCEPTANCE_INTENT";
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The delivery intent PRODUCTION datasource is unavailable.");
        var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);
        var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!=table+"_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Review the delivery journal key through the PRODUCTION Beep migration.");
        foreach(var (name,size) in new[]{(table+"_ID",32),("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("FIELD_ID",128),("RUN_TICKET_ID",128),("INTENT_KIND",11),
            ("SOURCE_REVISION",64),("SCOPE_KEY",64),("COMMAND_JSON",DELIVERY_ACCEPTANCE_INTENT.MaximumCommandLength),("RECORDED_REVISION",64)})
        {
            var fields=schema!.Fields.Where(field=>field.FieldName==name).ToArray();
            if(fields is not {Length:1} || fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size)
                throw new InvalidOperationException("Review complete delivery journal metadata through the PRODUCTION Beep migration.");
        }
        return new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(DELIVERY_ACCEPTANCE_INTENT),connection,table);
    }
    public Task<DELIVERY_ACCEPTANCE_INTENT?> GetDeliveryAcceptanceIntentAsync(CompanyScope company,string actor,string scopeKey,string connection,Func<Task> requireAuthority)
    {
        if(!ProcessDefinitionRevision.IsValid(scopeKey))throw new ArgumentException("The original delivery journal scope is required.");
        return ReadDeliveryIntentAsync(company,actor,"SCOPE_KEY",scopeKey,connection,requireAuthority);
    }
    public Task<DELIVERY_ACCEPTANCE_INTENT?> GetDeliveryAcceptanceIntentByIdAsync(CompanyScope company,string actor,string intent,string connection,Func<Task> requireAuthority)
    {
        CompanyDeliverySavedWorkReview.RequireIntentId(intent);
        return ReadDeliveryIntentAsync(company,actor,"DELIVERY_ACCEPTANCE_INTENT_ID",intent,connection,requireAuthority);
    }
    private async Task<DELIVERY_ACCEPTANCE_INTENT?> ReadDeliveryIntentAsync(CompanyScope company,string actor,string key,string value,string connection,Func<Task> requireAuthority)
    {
        APInvoiceOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl))throw new ArgumentException("The original company/account/delivery journal scope is required.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The delivery intent datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The original delivery journal datasource changed.");}
        async Task<DELIVERY_ACCEPTANCE_INTENT?> Read()
        {
            await Guard();var rows=(await DeliveryIntentRepository(connection).GetAsync([
                new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
                new(){FieldName="ROW_CREATED_BY",Operator="=",FilterValue=actor},new(){FieldName=key,Operator="=",FilterValue=value}]))?.ToArray();
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not DELIVERY_ACCEPTANCE_INTENT))throw new InvalidOperationException("One complete unique delivery journal entry or confirmed absence is required.");
            var entry=rows.SingleOrDefault() as DELIVERY_ACCEPTANCE_INTENT;
            if(entry is not null){if((key=="SCOPE_KEY"?entry.SCOPE_KEY:entry.DELIVERY_ACCEPTANCE_INTENT_ID)!=value)throw new InvalidOperationException("The provider returned a different original intent identity.");_=entry.Restore(company,actor,entry.SCOPE_KEY);}await Guard();return entry is null?null:CompanyInventorySourceReview.Copy(entry);
        }
        var first=await Read();var last=await Read();
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(last))throw new InvalidOperationException("The original delivery journal entry changed during read-back.");return last;
    }
    public Task<IReadOnlyList<DELIVERY_ACCEPTANCE_INTENT>> GetDeliveryAcceptanceIntentsAsync(CompanyScope company,string actor,string connection,Func<Task> requireAuthority)
        =>ReadDeliveryAcceptanceIntentsAsync(company,actor,connection,requireAuthority,null);
    public Task<IReadOnlyList<DELIVERY_ACCEPTANCE_INTENT>> GetDeliveryAcceptanceTicketIntentsAsync(CompanyScope company,string actor,string ticket,string connection,Func<Task> requireAuthority)
    {
        if(string.IsNullOrWhiteSpace(ticket) || ticket.Length>128 || ticket.Any(char.IsControl))throw new ArgumentException("The exact original native delivery ticket is required.");
        return ReadDeliveryAcceptanceIntentsAsync(company,actor,connection,requireAuthority,ticket);
    }
    private async Task<IReadOnlyList<DELIVERY_ACCEPTANCE_INTENT>> ReadDeliveryAcceptanceIntentsAsync(CompanyScope company,string actor,string connection,Func<Task> requireAuthority,string? ticket)
    {
        APInvoiceOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl))throw new ArgumentException("The bounded original company account is required.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The delivery intent datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The delivery journal datasource changed.");}
        async Task<DELIVERY_ACCEPTANCE_INTENT[]> Read()
        {
            await Guard();List<AppFilter> filters=[new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="ROW_CREATED_BY",Operator="=",FilterValue=actor}];
            if(ticket is not null)filters.Add(new(){FieldName="RUN_TICKET_ID",Operator="=",FilterValue=ticket});
            var rows=(await DeliveryIntentRepository(connection).GetAsync(filters))?.ToArray();
            if(rows is null || rows.Length>DELIVERY_ACCEPTANCE_INTENT.MaximumEntries || rows.Any(row=>row is not DELIVERY_ACCEPTANCE_INTENT))throw new InvalidOperationException("The complete bounded delivery journal set is unavailable; reconcile before loading a partial queue.");
            var entries=rows.Cast<DELIVERY_ACCEPTANCE_INTENT>().OrderBy(row=>row.SCOPE_KEY,StringComparer.Ordinal).ToArray();
            foreach(var entry in entries){_=entry.Restore(company,actor,entry.SCOPE_KEY);if(ticket is not null && entry.RUN_TICKET_ID!=ticket)throw new InvalidOperationException("The provider returned a different delivery ticket intent.");}
            if(entries.Sum(row=>(long)row.COMMAND_JSON.Length)>DELIVERY_ACCEPTANCE_INTENT.MaximumSetCharacters || entries.Select(row=>row.SCOPE_KEY).Distinct(StringComparer.Ordinal).Count()!=entries.Length || entries.Select(row=>row.DELIVERY_ACCEPTANCE_INTENT_ID).Distinct(StringComparer.Ordinal).Count()!=entries.Length)
                throw new InvalidOperationException("The delivery journal set is excessive or ambiguous; no partial set can be confirmed.");
            await Guard();return CompanyInventorySourceReview.Copy(entries);
        }
        var first=await Read();var last=await Read();
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(last))throw new InvalidOperationException("The complete delivery journal set changed during discovery.");return last;
    }
    public async Task<DELIVERY_ACCEPTANCE_INTENT> SaveDeliveryAcceptanceIntentAsync(RevenueDeliveryAcceptanceIntent command,string connection,Func<Task> requireAuthority,Func<Task<RevenueDeliveryAcceptanceReadReview>> readCurrent)
    {
        command=CompanyInventorySourceReview.Copy(command);command.RequireExecution();ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(readCurrent);
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The delivery intent datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The delivery journal datasource changed.");PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);}
        async Task<RevenueDeliveryAcceptanceReadReview> Current()
        {
            var current=CompanyInventorySourceReview.Copy(await readCurrent());command.RequireCurrent(current);
            if(current.Source.Source.Source.Company!=command.Company || current.Source.Source.Source.ActorUserId!=command.ActorUserId)throw new UnauthorizedAccessException("The original delivery journal account/source authority is required.");
            return current;
        }
        DELIVERY_ACCEPTANCE_INTENT? recorded=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{
            await Guard();var current=await Current();await Guard();
            var existing=await GetDeliveryAcceptanceIntentAsync(command.Company,command.ActorUserId,command.ScopeKey,connection,Guard);
            if(existing is not null)
            {
                if(JsonSerializer.Serialize(existing.Restore(command.Company,command.ActorUserId,command.ScopeKey))!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Reconcile the original saved delivery intent before proposing another.");
                recorded=existing;
            }
            else
            {
                var collision=(await DeliveryIntentRepository(connection).GetAsync([new(){FieldName="DELIVERY_ACCEPTANCE_INTENT_ID",Operator="=",FilterValue=command.IntentId}]))?.ToArray();
                if(collision is null || collision.Length!=0)throw new InvalidOperationException("The delivery intent identity is unavailable or already used.");
                var entry=DELIVERY_ACCEPTANCE_INTENT.Create(command,DateTime.UtcNow);
                var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(DELIVERY_ACCEPTANCE_INTENT),connection,"DELIVERY_ACCEPTANCE_INTENT",rowAccess:new ProductionIntentInsertPolicy<DELIVERY_ACCEPTANCE_INTENT>(entry,"DELIVERY_ACCEPTANCE_INTENT",Guard));
                await writer.InsertAsync(entry,command.ActorUserId);await Guard();
                recorded=await GetDeliveryAcceptanceIntentAsync(command.Company,command.ActorUserId,command.ScopeKey,connection,Guard);
                if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(entry))throw new InvalidOperationException("Saved delivery intent read-back is unconfirmed.");
            }
            var closing=await Current();await Guard();
            if(JsonSerializer.Serialize(closing)!=JsonSerializer.Serialize(current))throw new InvalidOperationException("The complete delivery source changed while saving its original intent.");
        });
        async Task ClosingGuard(){await requireAuthority();if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The delivery journal datasource changed.");}
        var durable=await GetDeliveryAcceptanceIntentAsync(command.Company,command.ActorUserId,command.ScopeKey,connection,ClosingGuard);
        await Current();await ClosingGuard();
        if(recorded is null || JsonSerializer.Serialize(durable)!=JsonSerializer.Serialize(recorded))throw new InvalidOperationException("Delivery intent commit is uncertain. Reconcile the original command.");
        return CompanyInventorySourceReview.Copy(durable!);
    }
}
