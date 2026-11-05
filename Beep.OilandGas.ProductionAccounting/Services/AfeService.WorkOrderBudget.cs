using System.Data;
using System.Text.Json;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ProductionAccounting.Services;

public partial class AfeService
{
    internal void RequireOwnedBudgetSchema(IDataSource source,string table="AFE",Type? model=null)
    {
        if(!source.CheckEntityExist(table))throw new InvalidOperationException("Review the existing PRODUCTION AFE Beep migration before budget creation.");
        var actual=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var expected=_editor.classCreator.ConvertToEntityStructure(model??typeof(AFE));
        if(actual?.Fields is null || expected?.Fields is null || actual.Fields.GroupBy(row=>row.FieldName,StringComparer.OrdinalIgnoreCase).Any(group=>group.Count()!=1) ||
            !actual.Fields.Where(row=>row.IsKey).Select(row=>row.FieldName).SequenceEqual(expected.Fields.Where(row=>row.IsKey).Select(row=>row.FieldName),StringComparer.Ordinal))throw new InvalidOperationException("The unique native AFE identity and complete physical source shape are required.");
        foreach(var field in expected.Fields)
        {
            var found=actual.Fields.SingleOrDefault(row=>row.FieldName==field.FieldName);
            if(found is null || found.Fieldtype!=field.Fieldtype || !field.AllowDBNull && found.AllowDBNull || field.Size1>0 && found.Size1<field.Size1 || field.IsKey && !found.IsKey)
                throw new InvalidOperationException("AFE budget ownership/currency/body capacity differs from its explicit Beep manifest.");
        }
    }
    private Task<IReadOnlyList<AFE>> ReadWorkOrderBudgetRowsAsync(FieldSelectionReview context,AssetDatabaseScope database,CompanyWorkOrderBudgetQuery query,Func<Task> authority,bool owned,string? afeId=null)
    {
        query.Require();return ReadWorkOrderBudgetRowsAsync(context,database,new CompanyWorkOrderFinancialQuery(query.WorkOrderId,query.FieldId),authority,owned,afeId);
    }
    public async Task<IReadOnlyList<CompanyWorkOrderBudgetFacts>> ReadWorkOrderBudgetFactsAsync(FieldSelectionReview context,AssetDatabaseScope database,CompanyWorkOrderFinancialQuery query,Func<Task> authority)
    {
        query.Require();var first=await ReadWorkOrderBudgetRowsAsync(context,database,query,authority,false);var closing=await ReadWorkOrderBudgetRowsAsync(context,database,query,authority,false);
        if(JsonSerializer.Serialize(first.OrderBy(row=>row.AFE_ID,StringComparer.Ordinal))!=JsonSerializer.Serialize(closing.OrderBy(row=>row.AFE_ID,StringComparer.Ordinal)))
            throw new InvalidOperationException("Complete original native budget membership changed during discovery.");
        var result=closing.Select(CompanyWorkOrderBudgetCommand.Facts).OrderBy(row=>row.AfeId,StringComparer.Ordinal).ToArray();await authority();return result;
    }
    private async Task<IReadOnlyList<AFE>> ReadWorkOrderBudgetRowsAsync(FieldSelectionReview context,AssetDatabaseScope database,CompanyWorkOrderFinancialQuery query,Func<Task> authority,bool owned,string? afeId=null)
    {
        FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);query.Require();if(query.FieldId!=context.FieldId)throw new UnauthorizedAccessException("The original budget FIELD is required.");
        var source=_editor.GetDataSource(database.ConnectionName)??throw new InvalidOperationException("Original budget PRODUCTION source is unavailable.");
        async Task Guard()
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,_editor.GetDataSource(database.ConnectionName)))throw new InvalidOperationException("Original budget PRODUCTION source changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireOwnedBudgetSchema(source);
        }
        await Guard();var filters=new List<AppFilter>{new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},
            new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId},new(){FieldName="WORK_ORDER_ID",Operator="=",FilterValue=query.WorkOrderId}};
        if(afeId is not null)filters.Add(new(){FieldName="AFE_ID",Operator="=",FilterValue=afeId});
        var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(AFE),database.ConnectionName,"AFE");
        var rows=(await repo.GetAsync(filters)).ToArray();
        if(rows.Length>CompanyWorkOrderBudgetReview.MaximumBudgets || rows.Any(value=>value is not AFE) || rows.Cast<AFE>().Select(row=>row.AFE_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length)
            throw new InvalidOperationException("Complete bounded unique native work-order AFE membership is required.");
        var result=new List<AFE>();foreach(var value in rows)
        {
            var row=(AFE)value;if(row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.WORK_ORDER_ID!=query.WorkOrderId || row.FIELD_ID!=query.FieldId)
                throw new UnauthorizedAccessException("Original AFE company/order/FIELD closes before its private budget body.");
            result.Add(CompanyInventorySourceReview.Copy(row));
        }
        await Guard();foreach(var row in result)
        {
            var original=CompanyWorkOrderBudgetCommand.Read(row);if(original.Review.Context.Database!=context.Database || original.Review.BudgetDatabase!=database)
                throw new InvalidOperationException("Original AFE CORE/PRODUCTION binding differs.");
        }
        return result;
    }
    public async Task<CompanyWorkOrderBudgetReview> ReviewWorkOrderBudgetAsync(CompanyWorkOrderBudgetReview basis,Func<Task> authority)
    {
        var original=CompanyInventorySourceReview.Copy(basis);original.Require();var first=await ReadWorkOrderBudgetRowsAsync(original.Context,original.BudgetDatabase,original.Query,authority,false);
        var closing=await ReadWorkOrderBudgetRowsAsync(original.Context,original.BudgetDatabase,original.Query,authority,false);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete native work-order budget membership changed during review.");
        var result=original with{Budgets=closing.Select(CompanyWorkOrderBudgetCommand.Facts).ToArray(),ComparedAtUtc=DateTime.UtcNow};result.Require();await authority();return result;
    }
    public async Task<CompanyWorkOrderBudgetRecovery> RecoverWorkOrderBudgetAsync(FieldSelectionReview context,AssetDatabaseScope database,CompanyWorkOrderBudgetReference reference,Func<Task> authority)
    {
        reference.Require();var id="wo-afe-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=context.Company,reference.IntentId}));
        async Task<AFE> Read()
        {
            var rows=await ReadWorkOrderBudgetRowsAsync(context,database,reference.Query,authority,false,id);
            if(rows.Count!=1)throw new InvalidOperationException("Original AFE budget is unconfirmed; recovery cannot create a replacement.");
            var row=rows[0];if(row.AFE_ID!=id || row.ROW_CREATED_BY!=context.ActorUserId || row.BOOK_ID!=reference.Query.BookId)throw new UnauthorizedAccessException("Original private AFE budget maker/order/book is required.");return row;
        }
        var first=await Read();var closing=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original budget history changed during recovery.");
        var result=new CompanyWorkOrderBudgetRecovery(CompanyWorkOrderBudgetCommand.Read(first),first);result.Require(context.Company,context.ActorUserId,reference);await authority();return result;
    }
    public async Task<CompanyWorkOrderBudgetRecovery> CreateWorkOrderBudgetAsync(CompanyWorkOrderBudgetCommand original,Func<Task> authority,Func<Task<CompanyWorkOrderBudgetReview>> readCurrent)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var context=command.Review.Context;var database=command.Review.BudgetDatabase;
        var reference=new CompanyWorkOrderBudgetReference(command.Review.Query,command.IntentId);var source=_editor.GetDataSource(database.ConnectionName)??throw new InvalidOperationException("Original AFE PRODUCTION source is unavailable.");
        async Task Guard(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,_editor.GetDataSource(database.ConnectionName)))throw new InvalidOperationException("Original AFE PRODUCTION binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireOwnedBudgetSchema(source);
        }
        var existing=await ReadWorkOrderBudgetRowsAsync(context,database,reference.Query,()=>Guard(false),false,command.AfeId());
        if(existing.Count==1){command.RequireRecorded(existing[0]);return await RecoverWorkOrderBudgetAsync(context,database,reference,()=>Guard(false));}
        await Guard(false);var current=CompanyInventorySourceReview.Copy(await readCurrent());command.RequireCurrent(current);await Guard(false);AFE? recorded=null;
        async Task Population(bool owned,string? exclude=null)
        {
            var rows=await ReadWorkOrderBudgetRowsAsync(context,database,reference.Query,()=>Guard(owned),owned);
            var reviewed=current with{Budgets=rows.Where(row=>row.AFE_ID!=exclude).Select(CompanyWorkOrderBudgetCommand.Facts).ToArray(),ComparedAtUtc=DateTime.UtcNow};command.RequireCurrent(reviewed);
        }
        // Current CORE work scope and configured book are read outside this PRODUCTION transaction.
        // Complete AFE membership is closed inside owned isolation; separate CORE changes remain recoverable.
        await Population(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Population(true);var row=command.Prepare(DateTime.UtcNow);var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(AFE),database.ConnectionName,"AFE",
                rowAccess:new WorkOrderBudgetInsertPolicy(row,command,()=>Guard(true)));
            await repo.InsertAsync(row,context.ActorUserId);var rows=await ReadWorkOrderBudgetRowsAsync(context,database,reference.Query,()=>Guard(true),true,command.AfeId());
            if(rows.Count!=1)throw new InvalidOperationException("Original AFE insert is unconfirmed.");recorded=rows[0];command.RequireRecorded(recorded);
            if(recorded.ROW_CREATED_DATE!=row.ROW_CREATED_DATE)throw new InvalidOperationException("Original budget audit changed during readback.");await Population(true,command.AfeId());
        });
        var closing=CompanyInventorySourceReview.Copy(await readCurrent());
        command.RequireCurrent(closing with{Budgets=closing.Budgets.Where(row=>row.AfeId!=command.AfeId()).ToArray()});
        await Population(false,command.AfeId());var recovery=await RecoverWorkOrderBudgetAsync(context,database,reference,()=>Guard(false));
        if(recorded is null || JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(recovery.Recorded))throw new InvalidOperationException("Original AFE acknowledgement is unconfirmed; recover its original reference without replay.");return recovery;
    }
    private sealed class WorkOrderBudgetInsertPolicy(AFE original,CompanyWorkOrderBudgetCommand command,Func<Task> authority):IPPDMRowAccessPolicy
    {
        private readonly AFE expected=CompanyInventorySourceReview.Copy(original);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await authority();if(type!=typeof(AFE) || table!="AFE" || !ReferenceEquals(entity,original) || previous is not null || operation!=PPDMRowMutation.Insert || actor!=expected.ROW_CREATED_BY)
                throw new InvalidOperationException("Only the exact original owned work-order AFE insert is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.SOURCE=expected.SOURCE;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;
            original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;command.RequireRecorded(original);
        }
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Budget insert grants no general row access.");
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Budget insert grants no general row access.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Budget insert grants no general row access.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Budget insert grants no general row access.");
    }
}
