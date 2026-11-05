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
    public async Task<CompanyWorkOrderMaterialCostReview> ReviewWorkOrderMaterialCostAsync(CompanyWorkOrderBudgetDecisionStatus budget,CompanyWorkOrderMaterialCostQuery query,CompanyWorkOrderMaterialCostSource material,Func<Task> authority)
    {
        var basis=new CompanyWorkOrderMaterialCostReview(CompanyInventorySourceReview.Copy(budget),query,material,[],DateTime.UtcNow);basis.Require();
        var first=await ReadWorkOrderActualCostRowsAsync(budget.Current.Context,budget.Current.BudgetDatabase,query.Budget,authority,false);
        var closing=await ReadWorkOrderActualCostRowsAsync(budget.Current.Context,budget.Current.BudgetDatabase,query.Budget,authority,false);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete original actual-cost membership changed during review.");
        var result=basis with{Costs=closing.Select(CompanyWorkOrderActualCostProof.Facts).ToArray(),ComparedAtUtc=DateTime.UtcNow};result.Require();await authority();return result;
    }
    public async Task<CompanyWorkOrderMaterialCostRecovery> RecoverWorkOrderMaterialCostAsync(FieldSelectionReview context,AssetDatabaseScope database,CompanyWorkOrderMaterialCostReference reference,Func<Task> authority)
    {
        reference.Require();var id="wo-cost-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=context.Company,reference.IntentId}));
        async Task<ACCOUNTING_COST> Read()
        {
            var rows=await ReadWorkOrderActualCostRowsAsync(context,database,reference.Query.Budget,authority,false,id,true);if(rows.Count!=1)throw new InvalidOperationException("Original actual cost is unconfirmed; recovery cannot record a replacement.");
            var row=rows[0];var recovery=new CompanyWorkOrderMaterialCostRecovery(CompanyWorkOrderMaterialCostCommand.Read(row),row);recovery.Require(context.Company,context.ActorUserId,reference);return row;
        }
        var first=await Read();var closing=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original actual-cost history changed during recovery.");
        var result=new CompanyWorkOrderMaterialCostRecovery(CompanyWorkOrderMaterialCostCommand.Read(first),first);result.Require(context.Company,context.ActorUserId,reference);await authority();return result;
    }
    public async Task<CompanyWorkOrderMaterialCostRecovery> RecordWorkOrderMaterialCostAsync(CompanyWorkOrderMaterialCostCommand original,Func<Task> authority,Func<Task<CompanyWorkOrderMaterialCostReview>> readCurrent,Func<Task<CompanyWorkOrderMaterialCostSource>> readOwnedSource)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var context=command.Review.Budget.Current.Context;var database=command.Review.Budget.Current.BudgetDatabase;
        var source=_editor.GetDataSource(database.ConnectionName)??throw new InvalidOperationException("Original actual-cost PRODUCTION source is unavailable.");
        async Task Guard(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,_editor.GetDataSource(database.ConnectionName)))throw new InvalidOperationException("Original actual-cost PRODUCTION binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireWorkOrderActualCostSchema(source);RequireOwnedBudgetSchema(source);
        }
        var existing=await ReadWorkOrderActualCostRowsAsync(context,database,command.Review.Query.Budget,()=>Guard(false),false,command.Id(),true);
        if(existing.Count==1){command.RequireRecorded(existing[0]);return await RecoverWorkOrderMaterialCostAsync(context,database,command.Reference(),()=>Guard(false));}
        await Guard(false);command.RequireCurrent(CompanyInventorySourceReview.Copy(await readCurrent()));await Guard(false);ACCOUNTING_COST? recorded=null;
        async Task Proof(bool owned,string? exclude=null)
        {
            var costs=await ReadWorkOrderActualCostRowsAsync(context,database,command.Review.Query.Budget,()=>Guard(owned),owned);
            var current=command.Review with{Costs=costs.Where(row=>row.ACCOUNTING_COST_ID!=exclude).Select(CompanyWorkOrderActualCostProof.Facts).ToArray(),ComparedAtUtc=DateTime.UtcNow};command.RequireCurrent(current);
            var drafts=await ReadWorkOrderBudgetRowsAsync(context,database,command.Review.Query.Budget.Budget,()=>Guard(owned),owned,command.Review.Query.Budget.AfeId);
            if(drafts.Count!=1 || CompanyWorkOrderBudgetCommand.Facts(drafts[0])!=command.Review.Budget.Original())throw new InvalidOperationException("Original approved native AFE budget changed before actual-cost admission.");
            if(owned)
            {
                var material=await readOwnedSource();if(material!=command.Review.Material)throw new InvalidOperationException("Original owned material consumption changed before valuation.");
                var approvals=new AuthorizationWorkflowService(_editor,_commonColumnHandler,_defaults,_metadata);
                var fact=await approvals.ReadOwnedWorkOrderBudgetDecisionFactsAsync(context,database,command.Review.Query.Budget,command.Review.Budget.Original(),()=>Guard(true));
                if(fact!=command.Review.Budget.Recorded || fact.Decision!=WorkOrderBudgetDecision.Approve)throw new InvalidOperationException("Original owned independent approval changed before actual-cost recording.");
            }
        }
        await Proof(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Proof(true);var row=command.Prepare(DateTime.UtcNow);var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(ACCOUNTING_COST),database.ConnectionName,WorkOrderCostTable,
                rowAccess:new WorkOrderMaterialCostInsertPolicy(row,command,()=>Guard(true)));
            await repo.InsertAsync(row,context.ActorUserId);var rows=await ReadWorkOrderActualCostRowsAsync(context,database,command.Review.Query.Budget,()=>Guard(true),true,command.Id(),true);
            if(rows.Count!=1)throw new InvalidOperationException("Original actual-cost insert is unconfirmed.");recorded=rows[0];command.RequireRecorded(recorded);
            if(recorded.ROW_CREATED_DATE!=row.ROW_CREATED_DATE)throw new InvalidOperationException("Original actual-cost audit changed during readback.");await Proof(true,command.Id());
        });
        var closing=CompanyInventorySourceReview.Copy(await readCurrent());command.RequireCurrent(closing with{Costs=closing.Costs.Where(row=>row.Id!=command.Id()).ToArray()});
        await Proof(false,command.Id());var recovery=await RecoverWorkOrderMaterialCostAsync(context,database,command.Reference(),()=>Guard(false));
        if(recorded is null || JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(recovery.Recorded))throw new InvalidOperationException("Original actual-cost acknowledgement is unconfirmed; recover its original reference without replay.");return recovery;
    }
    private sealed class WorkOrderMaterialCostInsertPolicy(ACCOUNTING_COST original,CompanyWorkOrderMaterialCostCommand command,Func<Task> authority):IPPDMRowAccessPolicy
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
