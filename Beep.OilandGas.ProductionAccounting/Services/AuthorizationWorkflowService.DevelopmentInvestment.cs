using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ProductionAccounting.Services;

public partial class AuthorizationWorkflowService
{
    private static AppFilter InvestmentFilter(string field,string value)=>new(){FieldName=field,Operator="=",FilterValue=value};
    private async Task<(AFE Request,APPROVAL_WORKFLOW? Decision)> ReadDevelopmentInvestment(AfeService budgets,IDataSource source,string connection,DevelopmentAfeReview basis,string afeId,Func<Task> guard)
    {
        basis.Require();if(basis.Afes.Count(row=>row.AfeId==afeId)!=1)throw new InvalidOperationException("Choose one exact reviewed development AFE.");
        var request=await budgets.DevelopmentOriginal(source,connection,basis,afeId,guard)??throw new InvalidOperationException("The original recorded development AFE is unavailable.");
        if(DevelopmentAfeCommand.Facts(request)!=basis.Afes.Single(row=>row.AfeId==afeId))throw new InvalidOperationException("The exact native AFE request changed during independent review.");
        RequireBudgetDecisionSchema(source);var scope=basis.Source.Review.Scope;
        var rows=await budgets.ReadDevelopmentRows<APPROVAL_WORKFLOW>(source,connection,BudgetDecisionTable,[InvestmentFilter("ENTITY_NAME","AFE"),InvestmentFilter("ENTITY_ID",afeId)],guard);
        if(rows.Length>1)throw new InvalidOperationException("The complete original development investment decision is ambiguous.");
        var id=DevelopmentInvestmentCommand.IdFor(scope.Company,afeId);
        var exact=await budgets.ReadDevelopmentRows<APPROVAL_WORKFLOW>(source,connection,BudgetDecisionTable,[InvestmentFilter("APPROVAL_WORKFLOW_ID",id)],guard);
        if(exact.Length>1 || rows.Length!=exact.Length || rows.Length==1 && JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(exact[0]))throw new InvalidOperationException("The original investment decision was moved, retagged or duplicated.");
        var decision=rows.SingleOrDefault();
        if(decision is not null)
        {
            if(decision.APPROVAL_WORKFLOW_ID!=id || decision.OWNER_TENANT_ID!=scope.Company.TenantId || decision.OWNER_COMPANY_BA_ID!=scope.Company.BusinessAssociateId ||
                decision.ENTITY_NAME!="AFE" || decision.ENTITY_ID!=afeId || decision.FIELD_ID!=scope.FieldId || decision.FDP_BASELINE_ID!=request.FDP_BASELINE_ID || decision.BOOK_ID!=request.BOOK_ID)
                throw new UnauthorizedAccessException("Original investment company/FIELD/AFE/book ownership closes before private proof.");
            var original=DevelopmentInvestmentCommand.FromRecord(decision);var owner=original.Review.Current.Source.Review.Scope;
            if(owner.Company!=scope.Company || owner.FieldId!=scope.FieldId || owner.CoreDatabase!=scope.CoreDatabase || original.Review.Current.BudgetDatabase!=basis.BudgetDatabase ||
                original.Review.Afe()!=DevelopmentAfeCommand.Facts(request) || DevelopmentPlanRelease.ReviewEvidenceRevision(original.Review.Current.Source.Review)!=DevelopmentPlanRelease.ReviewEvidenceRevision(basis.Source.Review) ||
                JsonSerializer.Serialize(original.Review.Current.Source.Baseline)!=JsonSerializer.Serialize(basis.Source.Baseline))throw new InvalidOperationException("The original investment decision source/AFE/native bindings changed.");
        }
        await guard();return(request,decision);
    }
    private static DevelopmentInvestmentReview InvestmentReview(DevelopmentAfeReview basis,AFE request,APPROVAL_WORKFLOW? decision)
    {
        var original=DevelopmentAfeCommand.FromRecord(request);var scope=basis.Source.Review.Scope;var blockers=new List<string>();
        if(request.ROW_CREATED_BY==scope.ActorUserId)blockers.Add(DevelopmentInvestmentReview.MakerDecisionBlocker);
        if(basis.Books.Books.SingleOrDefault(book=>book.BookId==request.BOOK_ID)!=original.Book())blockers.Add("The original configured company book/currency changed or is unavailable.");
        if(decision is not null)blockers.Add("The original independent investment decision is already recorded.");
        var result=new DevelopmentInvestmentReview(basis,request.AFE_ID,decision is null?null:DevelopmentInvestmentCommand.Facts(decision),blockers,DateTimeOffset.UtcNow);result.Require();return result;
    }
    public Task<DevelopmentInvestmentReview> ReviewDevelopmentInvestmentAsync(DevelopmentAfeReview basis,string afeId,AfeService budgets,Func<Task> authority)
    {
        basis.Require();basis=CompanyInventorySourceReview.Copy(basis);
        return budgets.OwnedDevelopment(basis,authority,async(source,connection,guard)=>{
            var first=await ReadDevelopmentInvestment(budgets,source,connection,basis,afeId,guard);var closing=await ReadDevelopmentInvestment(budgets,source,connection,basis,afeId,guard);
            if(JsonSerializer.Serialize(new{first.Request,first.Decision})!=JsonSerializer.Serialize(new{closing.Request,closing.Decision}))throw new InvalidOperationException("The original AFE/investment history changed during review.");return InvestmentReview(basis,closing.Request,closing.Decision);
        });
    }
    public async Task ConfirmDevelopmentInvestmentQueueAsync(IReadOnlyList<DevelopmentInvestmentReview> observed,AfeService budgets,Func<Task> authority)
    {
        if(observed is null || observed.Count>DevelopmentInvestmentQueue.MaximumItems)throw new InvalidOperationException("The complete bounded investment queue observation is required.");
        await authority();if(observed.Count==0)return;foreach(var item in observed)item.Require();var first=observed[0].Current;var scope=first.Source.Review.Scope;
        if(observed.Any(item=>item.Current.BudgetDatabase!=first.BudgetDatabase || item.Current.Source.Review.Scope.Company!=scope.Company || item.Current.Source.Review.Scope.ActorUserId!=scope.ActorUserId || item.Current.Source.Review.Scope.CoreDatabase!=scope.CoreDatabase))throw new UnauthorizedAccessException("One original company/account/native binding is required for the investment queue.");
        await budgets.OwnedDevelopment(first,authority,async(source,connection,guard)=>{
            foreach(var item in observed)
            {
                var current=await ReadDevelopmentInvestment(budgets,source,connection,item.Current,item.AfeId,guard);
                var outcome=current.Decision is null?null:DevelopmentInvestmentCommand.Facts(current.Decision);
                if(outcome!=item.Recorded)throw new InvalidOperationException("An investment decision changed before the complete queue could be confirmed. Refresh current work.");
            }
            await guard();return true;
        });
    }
    public async Task<DevelopmentInvestmentReceipt> RecordDevelopmentInvestmentAsync(DevelopmentInvestmentCommand original,AfeService budgets,Func<Task> authority,Func<Task<DevelopmentInvestmentReview>> readCurrent,bool recover=false)
    {
        original.Require();original=CompanyInventorySourceReview.Copy(original);var basis=original.Review.Current;APPROVAL_WORKFLOW? recorded=null;
        async Task<APPROVAL_WORKFLOW?> Original()
        {
            return await budgets.OwnedDevelopment(basis,authority,async(source,connection,guard)=>{
                var rows=await ReadDevelopmentInvestment(budgets,source,connection,basis,original.Review.AfeId,guard);
                if(rows.Decision is not null && DevelopmentInvestmentCommand.FromRecord(rows.Decision).Revision()!=original.Revision())throw new InvalidOperationException("Another original investment decision owns this AFE. Inspect its recorded outcome.");return rows.Decision;
            });
        }
        recorded=await Original();
        if(recorded is null && !recover)
        {
            original.RequireCurrent(await readCurrent());
            recorded=await budgets.OwnedDevelopment(basis,authority,async(source,connection,guard)=>{
                var before=await ReadDevelopmentInvestment(budgets,source,connection,basis,original.Review.AfeId,guard);
                if(before.Decision is not null)throw new InvalidOperationException("Another independent decision reserved this original AFE.");
                var current=InvestmentReview(basis,before.Request,null);original.RequireCurrent(current);if(current.Blockers.Count!=0)throw new InvalidOperationException("The current native investment source is not ready for an independent decision.");
                async Task Book()
                {
                    budgets.RequireOwnedBudgetSchema(source,"COMPANY_LEDGER_BOOK",typeof(COMPANY_LEDGER_BOOK));var scope=basis.Source.Review.Scope;
                    var books=await budgets.ReadDevelopmentRows<COMPANY_LEDGER_BOOK>(source,connection,"COMPANY_LEDGER_BOOK",[InvestmentFilter("OWNER_TENANT_ID",scope.Company.TenantId),InvestmentFilter("OWNER_COMPANY_BA_ID",scope.Company.BusinessAssociateId),InvestmentFilter("BOOK_ID",before.Request.BOOK_ID)],guard);
                    var observed=CompanyLedgerBookReview.Create(scope.Company,scope.ActorUserId,books);
                    if(observed.Books.Count!=1 || observed.Books[0]!=DevelopmentAfeCommand.FromRecord(before.Request).Book() || observed.Books[0]!=basis.Books.Books.SingleOrDefault(book=>book.BookId==before.Request.BOOK_ID))throw new InvalidOperationException("The original native book/currency differs during independent decision admission.");
                }
                async Task Population()
                {
                    var rows=await budgets.DevelopmentPopulation(source,connection,basis,guard);var population=basis with{Afes=rows.Select(DevelopmentAfeCommand.Facts).ToArray()};population=population with{Revision=population.ContentRevision()};
                    original.RequireCurrent(new(population,original.Review.AfeId,null,[],DateTimeOffset.UtcNow));
                }
                await Book();await Population();var row=original.ToRecord(DateTime.UtcNow);await guard();BeepOperationResult.EnsureSuccess(source.InsertEntity(BudgetDecisionTable,row),"Record development investment decision",BudgetDecisionTable);
                if(source.ErrorObject is {} error && !BeepOperationResult.IsSuccess(error))throw new InvalidOperationException("The original native investment insert is unconfirmed.");
                var after=await ReadDevelopmentInvestment(budgets,source,connection,basis,original.Review.AfeId,guard);
                if(after.Decision is null || DevelopmentInvestmentCommand.FromRecord(after.Decision).Revision()!=original.Revision() || JsonSerializer.Serialize(before.Request)!=JsonSerializer.Serialize(after.Request))throw new InvalidOperationException("The original investment decision/AFE readback is unconfirmed.");
                await Book();await Population();await guard();return after.Decision;
            });
            original.RequireCurrent(await readCurrent());
        }
        var closing=await Original();if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original investment decision or checked absence changed during acknowledgement.");
        var result=new DevelopmentInvestmentReceipt(original.Revision(),closing,DateTimeOffset.UtcNow);result.Require(original);return result;
    }
}
