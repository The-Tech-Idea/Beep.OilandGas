using System.Data;
using System.Text.Json;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ProductionAccounting.Services;

public partial class AuthorizationWorkflowService
{
    private const string BudgetDecisionTable="APPROVAL_WORKFLOW";
    private void RequireBudgetDecisionSchema(IDataSource source)
    {
        if(!source.CheckEntityExist(BudgetDecisionTable))throw new InvalidOperationException("Review the existing PRODUCTION approval-workflow Beep migration before independent budget decisions.");
        var actual=source.GetEntityStructure(new EntityStructure{EntityName=BudgetDecisionTable,DatasourceEntityName=BudgetDecisionTable},true);var expected=_editor.classCreator.ConvertToEntityStructure(typeof(APPROVAL_WORKFLOW));
        if(actual?.Fields is null || expected?.Fields is null || actual.Fields.GroupBy(row=>row.FieldName,StringComparer.OrdinalIgnoreCase).Any(group=>group.Count()!=1) ||
            !actual.Fields.Where(row=>row.IsKey).Select(row=>row.FieldName).SequenceEqual(new[]{"APPROVAL_WORKFLOW_ID"},StringComparer.Ordinal))throw new InvalidOperationException("Complete approval-workflow schema and unique native identity are required.");
        foreach(var field in expected.Fields)
        {
            var found=actual.Fields.SingleOrDefault(row=>row.FieldName==field.FieldName);
            if(found is null || found.Fieldtype!=field.Fieldtype || !field.AllowDBNull && found.AllowDBNull || field.Size1>0 && found.Size1<field.Size1 || field.IsKey && !found.IsKey)
                throw new InvalidOperationException("Approval-workflow company/order/book/currency/body capacity differs from its explicit Beep manifest.");
        }
    }
    private async Task<APPROVAL_WORKFLOW?> ReadBudgetDecisionAsync(FieldSelectionReview context,AssetDatabaseScope database,CompanyWorkOrderBudgetDecisionQuery query,Func<Task> authority,bool owned,bool privateRead)
    {
        FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);query.Require();if(context.FieldId!=query.Budget.FieldId)throw new UnauthorizedAccessException("Original budget decision FIELD is required.");
        var source=_editor.GetDataSource(database.ConnectionName)??throw new InvalidOperationException("Original approval PRODUCTION source is unavailable.");
        async Task Guard()
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,_editor.GetDataSource(database.ConnectionName)))throw new InvalidOperationException("Original budget decision datasource changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireBudgetDecisionSchema(source);
        }
        await Guard();var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(APPROVAL_WORKFLOW),database.ConnectionName,BudgetDecisionTable);
        // Complete AFE correlation includes unowned/legacy rows; they cannot silently become an empty decision population.
        var rows=(await repo.GetAsync([new(){FieldName="ENTITY_NAME",Operator="=",FilterValue="AFE"},new(){FieldName="ENTITY_ID",Operator="=",FilterValue=query.AfeId}])).ToArray();
        if(rows.Length>1 || rows.Any(value=>value is not APPROVAL_WORKFLOW))throw new InvalidOperationException("One complete original AFE decision population is required.");
        if(rows.Length==0){await Guard();return null;}var row=(APPROVAL_WORKFLOW)rows[0];
        if(row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.ENTITY_NAME!="AFE" || row.ENTITY_ID!=query.AfeId ||
            row.APPROVAL_WORKFLOW_ID!=CompanyWorkOrderBudgetDecisionCommand.IdFor(context.Company,query.AfeId) || row.FIELD_ID!=query.Budget.FieldId || row.WORK_ORDER_ID!=query.Budget.WorkOrderId || row.BOOK_ID!=query.Budget.BookId ||
            privateRead && row.ROW_CREATED_BY!=context.ActorUserId)throw new UnauthorizedAccessException("Original decision company/order/AFE/book/reviewer closes before its private audit.");
        var captured=CompanyInventorySourceReview.Copy(row);await Guard();var command=CompanyWorkOrderBudgetDecisionCommand.Read(captured);
        if(command.Review.Query!=query || command.Review.Current.Context.Database!=context.Database || command.Review.Current.BudgetDatabase!=database)
            throw new InvalidOperationException("Original independent decision reference and native bindings differ.");return captured;
    }
    public async Task<CompanyWorkOrderBudgetDecisionReview> ReviewWorkOrderBudgetDecisionAsync(CompanyWorkOrderBudgetReview current,CompanyWorkOrderBudgetDecisionQuery query,Func<Task> authority)
    {
        var review=new CompanyWorkOrderBudgetDecisionReview(CompanyInventorySourceReview.Copy(current),query);review.Require();
        var first=await ReadBudgetDecisionAsync(review.Current.Context,review.Current.BudgetDatabase,query,authority,false,false);
        var closing=await ReadBudgetDecisionAsync(review.Current.Context,review.Current.BudgetDatabase,query,authority,false,false);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original independent decision membership changed during review.");
        if(first is not null)throw new InvalidOperationException("An original independent decision is already recorded; recover its original reference instead of replacing it.");
        await authority();return review;
    }
    public async Task<CompanyWorkOrderBudgetDecisionFacts?> ReadWorkOrderBudgetDecisionFactsAsync(FieldSelectionReview context,AssetDatabaseScope database,CompanyWorkOrderBudgetDecisionQuery query,CompanyWorkOrderBudgetFacts draft,Func<Task> authority)
    {
        draft.Require();var first=await ReadBudgetDecisionAsync(context,database,query,authority,false,false);
        var closing=await ReadBudgetDecisionAsync(context,database,query,authority,false,false);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete independent decision membership/audit changed during public observation.");
        if(first is null){await authority();return null;}var result=CompanyWorkOrderBudgetDecisionFacts.From(first);result.Require(context.Company,query,draft);await authority();return result;
    }
    internal async Task<CompanyWorkOrderBudgetDecisionFacts> ReadOwnedWorkOrderBudgetDecisionFactsAsync(FieldSelectionReview context,AssetDatabaseScope database,CompanyWorkOrderBudgetDecisionQuery query,CompanyWorkOrderBudgetFacts draft,Func<Task> authority)
    {
        var first=await ReadBudgetDecisionAsync(context,database,query,authority,true,false)??throw new InvalidOperationException("Original independent budget decision is required inside owned financial admission.");
        var closing=await ReadBudgetDecisionAsync(context,database,query,authority,true,false);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original decision history changed inside owned cost admission.");
        var result=CompanyWorkOrderBudgetDecisionFacts.From(first);result.Require(context.Company,query,draft);await authority();return result;
    }
    public async Task<CompanyWorkOrderBudgetDecisionRecovery> RecoverWorkOrderBudgetDecisionAsync(FieldSelectionReview context,AssetDatabaseScope database,CompanyWorkOrderBudgetDecisionReference reference,Func<Task> authority)
    {
        reference.Require();var first=await ReadBudgetDecisionAsync(context,database,reference.Query,authority,false,true)??throw new InvalidOperationException("Original independent budget decision is unconfirmed; recovery cannot record one.");
        var result=new CompanyWorkOrderBudgetDecisionRecovery(CompanyWorkOrderBudgetDecisionCommand.Read(first),first);result.Require(context.Company,context.ActorUserId,reference);
        var closing=await ReadBudgetDecisionAsync(context,database,reference.Query,authority,false,true);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original independent decision history changed during recovery.");await authority();return result;
    }
    public async Task<CompanyWorkOrderBudgetDecisionRecovery> RecordWorkOrderBudgetDecisionAsync(CompanyWorkOrderBudgetDecisionCommand original,Func<Task> authority,Func<Task<CompanyWorkOrderBudgetDecisionReview>> readCurrent)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var context=command.Review.Current.Context;var database=command.Review.Current.BudgetDatabase;
        var reference=command.Reference();var source=_editor.GetDataSource(database.ConnectionName)??throw new InvalidOperationException("Original budget decision PRODUCTION source is unavailable.");
        async Task Guard(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,_editor.GetDataSource(database.ConnectionName)))throw new InvalidOperationException("Original independent decision PRODUCTION binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireBudgetDecisionSchema(source);
        }
        var existing=await ReadBudgetDecisionAsync(context,database,reference.Query,()=>Guard(false),false,true);
        if(existing is not null){command.RequireRecorded(existing);return await RecoverWorkOrderBudgetDecisionAsync(context,database,reference,()=>Guard(false));}
        await Guard(false);command.RequireCurrent(CompanyInventorySourceReview.Copy(await readCurrent()));await Guard(false);APPROVAL_WORKFLOW? recorded=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            if(await ReadBudgetDecisionAsync(context,database,reference.Query,()=>Guard(true),true,false) is not null)throw new InvalidOperationException("Another original decision already reserves this AFE.");
            // Exact AFE proof is closed in the same PRODUCTION-owned command; separate CORE/book close outside it.
            var afeRepo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(AFE),database.ConnectionName,"AFE");
            await Guard(true);var rows=(await afeRepo.GetAsync([new(){FieldName="AFE_ID",Operator="=",FilterValue=reference.Query.AfeId}])).ToArray();
            if(rows.Length!=1 || rows[0] is not AFE afe || afe.OWNER_TENANT_ID!=context.Company.TenantId || afe.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || afe.FIELD_ID!=reference.Query.Budget.FieldId ||
                afe.WORK_ORDER_ID!=reference.Query.Budget.WorkOrderId || afe.BOOK_ID!=reference.Query.Budget.BookId)throw new UnauthorizedAccessException("One original owned native draft AFE is required before independent decision.");
            var draft=CompanyInventorySourceReview.Copy(afe);await Guard(true);
            if(CompanyWorkOrderBudgetCommand.Facts(draft)!=command.Review.Original())throw new InvalidOperationException("Original native AFE budget audit or reviewed scope changed.");
            var row=command.Prepare(DateTime.UtcNow);var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(APPROVAL_WORKFLOW),database.ConnectionName,BudgetDecisionTable,
                rowAccess:new WorkOrderBudgetDecisionInsertPolicy(row,command,()=>Guard(true)));
            await repo.InsertAsync(row,context.ActorUserId);recorded=await ReadBudgetDecisionAsync(context,database,reference.Query,()=>Guard(true),true,true)??throw new InvalidOperationException("Original decision insertion is unconfirmed.");command.RequireRecorded(recorded);
            if(recorded.ROW_CREATED_DATE!=row.ROW_CREATED_DATE)throw new InvalidOperationException("Original independent decision audit changed during readback.");
            var after=(await afeRepo.GetAsync([new(){FieldName="AFE_ID",Operator="=",FilterValue=reference.Query.AfeId}])).ToArray();
            if(after.Length!=1 || after[0] is not AFE closing || JsonSerializer.Serialize(draft)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original draft changed around the independent decision.");await Guard(true);
        });
        // The current callback supplies the same original draft and current scope/book, independently of decision membership.
        command.RequireCurrent(CompanyInventorySourceReview.Copy(await readCurrent()));await Guard(false);
        var recovery=await RecoverWorkOrderBudgetDecisionAsync(context,database,reference,()=>Guard(false));
        if(recorded is null || JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(recovery.Recorded))throw new InvalidOperationException("Original decision acknowledgement is unconfirmed; recover without replay.");return recovery;
    }
    private sealed class WorkOrderBudgetDecisionInsertPolicy(APPROVAL_WORKFLOW original,CompanyWorkOrderBudgetDecisionCommand command,Func<Task> authority):IPPDMRowAccessPolicy
    {
        private readonly APPROVAL_WORKFLOW expected=CompanyInventorySourceReview.Copy(original);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await authority();if(type!=typeof(APPROVAL_WORKFLOW) || table!=BudgetDecisionTable || !ReferenceEquals(entity,original) || previous is not null || operation!=PPDMRowMutation.Insert || actor!=expected.ROW_CREATED_BY)
                throw new InvalidOperationException("Only the exact original independent budget decision insert is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.SOURCE=expected.SOURCE;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;
            original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;command.RequireRecorded(original);
        }
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Decision insert grants no general row access.");
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Decision insert grants no general row access.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Decision insert grants no general row access.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Decision insert grants no general row access.");
    }
}
