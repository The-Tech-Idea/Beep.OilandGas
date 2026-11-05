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
    private const string WorkOrderCostTable="ACCOUNTING_COST";
    private void RequireWorkOrderActualCostSchema(IDataSource source)
    {
        if(!source.CheckEntityExist(WorkOrderCostTable))throw new InvalidOperationException("Review the existing PRODUCTION actual-cost Beep migration before valuation.");
        var actual=source.GetEntityStructure(new EntityStructure{EntityName=WorkOrderCostTable,DatasourceEntityName=WorkOrderCostTable},true);var expected=_editor.classCreator.ConvertToEntityStructure(typeof(ACCOUNTING_COST));
        if(actual?.Fields is null || expected?.Fields is null || actual.Fields.GroupBy(row=>row.FieldName,StringComparer.OrdinalIgnoreCase).Any(group=>group.Count()!=1) ||
            !actual.Fields.Where(row=>row.IsKey).Select(row=>row.FieldName).SequenceEqual(new[]{"ACCOUNTING_COST_ID"},StringComparer.Ordinal))throw new InvalidOperationException("Complete actual-cost shape and unique native identity are required.");
        foreach(var field in expected.Fields)
        {
            var found=actual.Fields.SingleOrDefault(row=>row.FieldName==field.FieldName);
            if(found is null || found.Fieldtype!=field.Fieldtype || !field.AllowDBNull && found.AllowDBNull || field.Size1>0 && found.Size1<field.Size1 || field.IsKey && !found.IsKey)
                throw new InvalidOperationException("Actual-cost company/source/rate/currency/body capacity differs from its explicit Beep manifest.");
        }
    }
    private Task<IReadOnlyList<ACCOUNTING_COST>> ReadWorkOrderActualCostRowsAsync(FieldSelectionReview context,AssetDatabaseScope database,CompanyWorkOrderLaborCostQuery query,Func<Task> authority,bool owned,string? id=null,bool privateRead=false)
    {
        query.Require();return ReadWorkOrderActualCostRowsAsync(context,database,query.Budget,authority,owned,id,privateRead);
    }
    public async Task<CompanyWorkOrderActualCostReview> ReadWorkOrderActualCostsAsync(CompanyWorkOrderBudgetDecisionStatus budget,Func<Task> authority)
    {
        budget.Require();var first=await ReadWorkOrderActualCostRowsAsync(budget.Current.Context,budget.Current.BudgetDatabase,budget.Query,authority,false);
        var closing=await ReadWorkOrderActualCostRowsAsync(budget.Current.Context,budget.Current.BudgetDatabase,budget.Query,authority,false);
        if(JsonSerializer.Serialize(first.OrderBy(row=>row.ACCOUNTING_COST_ID,StringComparer.Ordinal))!=JsonSerializer.Serialize(closing.OrderBy(row=>row.ACCOUNTING_COST_ID,StringComparer.Ordinal)))
            throw new InvalidOperationException("Complete original actual-cost membership changed during observation.");
        var result=new CompanyWorkOrderActualCostReview(CompanyInventorySourceReview.Copy(budget),closing.Select(CompanyWorkOrderActualCostProof.Facts).OrderBy(row=>row.Id,StringComparer.Ordinal).ToArray(),DateTime.UtcNow);
        result.Require();_=result.RecordedTotal();_=result.RemainingOriginalBudget();await authority();return result;
    }
    private async Task<IReadOnlyList<ACCOUNTING_COST>> ReadWorkOrderActualCostRowsAsync(FieldSelectionReview context,AssetDatabaseScope database,CompanyWorkOrderBudgetDecisionQuery query,Func<Task> authority,bool owned,string? id=null,bool privateRead=false)
    {
        query.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);if(context.FieldId!=query.Budget.FieldId)throw new UnauthorizedAccessException("Original actual-cost FIELD is required.");
        var source=_editor.GetDataSource(database.ConnectionName)??throw new InvalidOperationException("Original actual-cost PRODUCTION source is unavailable.");
        async Task Guard()
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,_editor.GetDataSource(database.ConnectionName)))throw new InvalidOperationException("Original actual-cost datasource changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireWorkOrderActualCostSchema(source);
        }
        await Guard();var filters=new List<AppFilter>{new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId},
            new(){FieldName="WORK_ORDER_ID",Operator="=",FilterValue=query.Budget.WorkOrderId}};if(id is not null)filters.Add(new(){FieldName="ACCOUNTING_COST_ID",Operator="=",FilterValue=id});
        var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(ACCOUNTING_COST),database.ConnectionName,WorkOrderCostTable);var orderRows=(await repo.GetAsync(filters)).ToArray();
        // AFE correlation must also expose legacy/unowned rows rather than silently showing an empty total.
        var afeFilters=new List<AppFilter>{new(){FieldName="AFE_ID",Operator="=",FilterValue=query.AfeId}};if(id is not null)afeFilters.Add(new(){FieldName="ACCOUNTING_COST_ID",Operator="=",FilterValue=id});
        await Guard();var afeRows=(await repo.GetAsync(afeFilters)).ToArray();
        if(orderRows.Length>CompanyWorkOrderLaborCostReview.MaximumCosts || afeRows.Length>CompanyWorkOrderLaborCostReview.MaximumCosts || orderRows.Concat(afeRows).Any(value=>value is not ACCOUNTING_COST))throw new InvalidOperationException("Complete bounded typed cost populations are required.");
        if(orderRows.Cast<ACCOUNTING_COST>().Select(row=>row.ACCOUNTING_COST_ID).Distinct(StringComparer.Ordinal).Count()!=orderRows.Length ||
            afeRows.Cast<ACCOUNTING_COST>().Select(row=>row.ACCOUNTING_COST_ID).Distinct(StringComparer.Ordinal).Count()!=afeRows.Length)throw new InvalidOperationException("Each complete cost population must have unique native identities.");
        var merged=new Dictionary<string,ACCOUNTING_COST>(StringComparer.Ordinal);
        foreach(var value in orderRows.Concat(afeRows))
        {
            var row=(ACCOUNTING_COST)value;var captured=CompanyInventorySourceReview.Copy(row);
            if(merged.TryGetValue(row.ACCOUNTING_COST_ID,out var previous) && JsonSerializer.Serialize(previous)!=JsonSerializer.Serialize(captured))throw new InvalidOperationException("Actual-cost correlation changed between complete observations.");
            merged[row.ACCOUNTING_COST_ID]=captured;
        }
        var raw=merged.Values.Cast<object>().ToArray();
        if(raw.Length>CompanyWorkOrderLaborCostReview.MaximumCosts || raw.Any(value=>value is not ACCOUNTING_COST) || raw.Cast<ACCOUNTING_COST>().Select(row=>row.ACCOUNTING_COST_ID).Distinct(StringComparer.Ordinal).Count()!=raw.Length)
            throw new InvalidOperationException("Complete bounded unique original work-order actual-cost population is required.");
        var result=new List<ACCOUNTING_COST>();foreach(var value in raw)
        {
            var row=(ACCOUNTING_COST)value;if(row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.FIELD_ID!=context.FieldId || row.WORK_ORDER_ID!=query.Budget.WorkOrderId ||
                row.AFE_ID!=query.AfeId || row.BOOK_ID!=query.Budget.BookId || privateRead && row.ROW_CREATED_BY!=context.ActorUserId)
                throw new UnauthorizedAccessException("Original actual-cost company/order/AFE/book/actor closes before private valuation proof.");result.Add(CompanyInventorySourceReview.Copy(row));
        }
        await Guard();foreach(var row in result)
        {
            var budget=CompanyWorkOrderActualCostProof.Budget(row);if(budget.Context.Database!=context.Database || budget.BudgetDatabase!=database)
                throw new InvalidOperationException("Original actual-cost native bindings changed.");
        }
        return result;
    }
    public async Task<CompanyWorkOrderLaborCostReview> ReviewWorkOrderLaborCostAsync(CompanyWorkOrderBudgetDecisionStatus budget,CompanyWorkOrderLaborCostQuery query,CompanyWorkOrderLaborUsageFacts labor,Func<Task> authority)
    {
        var basis=new CompanyWorkOrderLaborCostReview(CompanyInventorySourceReview.Copy(budget),query,labor,[],DateTime.UtcNow);basis.Require();
        var first=await ReadWorkOrderActualCostRowsAsync(budget.Current.Context,budget.Current.BudgetDatabase,query,authority,false);
        var closing=await ReadWorkOrderActualCostRowsAsync(budget.Current.Context,budget.Current.BudgetDatabase,query,authority,false);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete original actual-cost membership changed during review.");
        var result=basis with{Costs=closing.Select(CompanyWorkOrderActualCostProof.Facts).ToArray(),ComparedAtUtc=DateTime.UtcNow};result.Require();await authority();return result;
    }
    public async Task<CompanyWorkOrderLaborCostRecovery> RecoverWorkOrderLaborCostAsync(FieldSelectionReview context,AssetDatabaseScope database,CompanyWorkOrderLaborCostReference reference,Func<Task> authority)
    {
        reference.Require();var id="wo-cost-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=context.Company,reference.IntentId}));
        async Task<ACCOUNTING_COST> Read()
        {
            var rows=await ReadWorkOrderActualCostRowsAsync(context,database,reference.Query,authority,false,id,true);if(rows.Count!=1)throw new InvalidOperationException("Original actual cost is unconfirmed; recovery cannot record a replacement.");
            var row=rows[0];var recovery=new CompanyWorkOrderLaborCostRecovery(CompanyWorkOrderLaborCostCommand.Read(row),row);recovery.Require(context.Company,context.ActorUserId,reference);return row;
        }
        var first=await Read();var closing=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original actual-cost history changed during recovery.");
        var result=new CompanyWorkOrderLaborCostRecovery(CompanyWorkOrderLaborCostCommand.Read(first),first);result.Require(context.Company,context.ActorUserId,reference);await authority();return result;
    }
    public async Task<CompanyWorkOrderLaborCostRecovery> RecordWorkOrderLaborCostAsync(CompanyWorkOrderLaborCostCommand original,Func<Task> authority,Func<Task<CompanyWorkOrderLaborCostReview>> readCurrent)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var context=command.Review.Budget.Current.Context;var database=command.Review.Budget.Current.BudgetDatabase;
        var source=_editor.GetDataSource(database.ConnectionName)??throw new InvalidOperationException("Original actual-cost PRODUCTION source is unavailable.");
        async Task Guard(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,_editor.GetDataSource(database.ConnectionName)))throw new InvalidOperationException("Original actual-cost PRODUCTION binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireWorkOrderActualCostSchema(source);RequireOwnedBudgetSchema(source);
        }
        var existing=await ReadWorkOrderActualCostRowsAsync(context,database,command.Review.Query,()=>Guard(false),false,command.Id(),true);
        if(existing.Count==1){command.RequireRecorded(existing[0]);return await RecoverWorkOrderLaborCostAsync(context,database,command.Reference(),()=>Guard(false));}
        await Guard(false);command.RequireCurrent(CompanyInventorySourceReview.Copy(await readCurrent()));await Guard(false);ACCOUNTING_COST? recorded=null;
        async Task Proof(bool owned,string? exclude=null)
        {
            var costs=await ReadWorkOrderActualCostRowsAsync(context,database,command.Review.Query,()=>Guard(owned),owned);
            var current=command.Review with{Costs=costs.Where(row=>row.ACCOUNTING_COST_ID!=exclude).Select(CompanyWorkOrderActualCostProof.Facts).ToArray(),ComparedAtUtc=DateTime.UtcNow};command.RequireCurrent(current);
            var drafts=await ReadWorkOrderBudgetRowsAsync(context,database,command.Review.Query.Budget.Budget,()=>Guard(owned),owned,command.Review.Query.Budget.AfeId);
            if(drafts.Count!=1 || CompanyWorkOrderBudgetCommand.Facts(drafts[0])!=command.Review.Budget.Original())throw new InvalidOperationException("Original approved native AFE budget changed before actual-cost admission.");
            if(owned)
            {
                var core=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original labor CORE source is unavailable.");
                if(ReferenceEquals(core,source))
                {
                    var laborOwner=new Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder.WorkOrderExecutionPermitService(_editor,_commonColumnHandler,_defaults,_metadata);
                    var labor=await laborOwner.ReadOwnedLaborFactsAsync(context,command.Review.Labor.WorkOrderId,command.Review.Labor.LaborId,()=>Guard(true));
                    if(labor!=command.Review.Labor)throw new InvalidOperationException("Original owned actual labor source changed before valuation.");
                }
                var approvals=new AuthorizationWorkflowService(_editor,_commonColumnHandler,_defaults,_metadata);
                var fact=await approvals.ReadOwnedWorkOrderBudgetDecisionFactsAsync(context,database,command.Review.Query.Budget,command.Review.Budget.Original(),()=>Guard(true));
                if(fact!=command.Review.Budget.Recorded || fact.Decision!=WorkOrderBudgetDecision.Approve)throw new InvalidOperationException("Original owned independent approval changed before actual-cost recording.");
            }
        }
        await Proof(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Proof(true);var row=command.Prepare(DateTime.UtcNow);var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(ACCOUNTING_COST),database.ConnectionName,WorkOrderCostTable,
                rowAccess:new WorkOrderLaborCostInsertPolicy(row,command,()=>Guard(true)));
            await repo.InsertAsync(row,context.ActorUserId);var rows=await ReadWorkOrderActualCostRowsAsync(context,database,command.Review.Query,()=>Guard(true),true,command.Id(),true);
            if(rows.Count!=1)throw new InvalidOperationException("Original actual-cost insert is unconfirmed.");recorded=rows[0];command.RequireRecorded(recorded);
            if(recorded.ROW_CREATED_DATE!=row.ROW_CREATED_DATE)throw new InvalidOperationException("Original actual-cost audit changed during readback.");await Proof(true,command.Id());
        });
        var closing=CompanyInventorySourceReview.Copy(await readCurrent());command.RequireCurrent(closing with{Costs=closing.Costs.Where(row=>row.Id!=command.Id()).ToArray()});
        await Proof(false,command.Id());var recovery=await RecoverWorkOrderLaborCostAsync(context,database,command.Reference(),()=>Guard(false));
        if(recorded is null || JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(recovery.Recorded))throw new InvalidOperationException("Original actual-cost acknowledgement is unconfirmed; recover its original reference without replay.");return recovery;
    }
    private sealed class WorkOrderLaborCostInsertPolicy(ACCOUNTING_COST original,CompanyWorkOrderLaborCostCommand command,Func<Task> authority):IPPDMRowAccessPolicy
    {
        private readonly ACCOUNTING_COST expected=CompanyInventorySourceReview.Copy(original);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await authority();if(type!=typeof(ACCOUNTING_COST) || table!=WorkOrderCostTable || !ReferenceEquals(entity,original) || previous is not null || operation!=PPDMRowMutation.Insert || actor!=expected.ROW_CREATED_BY)
                throw new InvalidOperationException("Only the exact original owned actual-cost insert is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.SOURCE=expected.SOURCE;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;
            original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;command.RequireRecorded(original);
        }
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Actual-cost insert grants no general row access.");
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Actual-cost insert grants no general row access.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Actual-cost insert grants no general row access.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Actual-cost insert grants no general row access.");
    }
}
