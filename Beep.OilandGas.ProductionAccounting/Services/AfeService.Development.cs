using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ProductionAccounting.Services;

public partial class AfeService
{
    public Task<string> ReadDevelopmentFundingRationaleAsync(DevelopmentAfeReview basis,string afeId,Func<Task> authority)
        =>OwnedDevelopment(basis,authority,async(source,connection,guard)=>{
            var before=await DevelopmentOriginal(source,connection,basis,afeId,guard)??throw new InvalidOperationException("The original funding request is unavailable.");
            if(basis.Afes.Count(row=>row.AfeId==afeId)!=1 || DevelopmentAfeCommand.Facts(before)!=basis.Afes.Single(row=>row.AfeId==afeId))
                throw new InvalidOperationException("The original funding rationale differs from the exact reviewed request.");
            var command=DevelopmentAfeCommand.FromRecord(before);
            var closing=await DevelopmentOriginal(source,connection,basis,afeId,guard);
            if(JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original funding rationale changed during its source read.");
            await guard();return command.Rationale;
        });
    private sealed class DevelopmentRows(AfeService owner,Type type,string connection,string table):PPDMGenericRepository(owner._editor,owner._commonColumnHandler,owner._defaults,owner._metadata,type,connection,table)
    {public object[] Map(object raw,Type target)=>ConvertToTypedList(raw,target).ToArray();}
    internal async Task<T[]> ReadDevelopmentRows<T>(IDataSource source,string connection,string table,List<AppFilter> filters,Func<Task> guard)
    {
        await guard();var raw=await source.GetEntityAsync(table,filters)??throw new InvalidOperationException("The development AFE source returned no read outcome.");
        if(source.ErrorObject is {} status && !BeepOperationResult.IsSuccess(status))throw new InvalidOperationException("The development AFE source read failed.");
        var rows=new DevelopmentRows(this,typeof(T),connection,table).Map(raw,typeof(T));if(rows.Any(row=>row is not T))throw new InvalidOperationException("The development AFE source returned an unexpected record type.");await guard();return CompanyInventorySourceReview.Copy(rows.Cast<T>().ToArray());
    }
    private static AppFilter AfeFilter(string name,string value)=>new(){FieldName=name,Operator="=",FilterValue=value};
    internal async Task<T> OwnedDevelopment<T>(DevelopmentAfeReview basis,Func<Task> authority,Func<IDataSource,string,Func<Task>,Task<T>> action)
    {
        basis.Require();var database=basis.BudgetDatabase;var source=_editor.GetDataSource(database.ConnectionName)??throw new InvalidOperationException("The original PRODUCTION AFE source is unavailable.");
        async Task Guard(bool owned)
        {
            await authority();if(!ReferenceEquals(source,_editor.GetDataSource(database.ConnectionName)))throw new InvalidOperationException("The original development AFE datasource changed.");
            MigrationConnectionTarget.Validate(_editor,source,database.ConnectionName);
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireOwnedBudgetSchema(source);
        }
        await Guard(false);T result=default!;await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{await Guard(true);result=await action(source,database.ConnectionName,()=>Guard(true));await Guard(true);});
        await Guard(false);return CompanyInventorySourceReview.Copy(result);
    }
    internal async Task<AFE?> DevelopmentOriginal(IDataSource source,string connection,DevelopmentAfeReview basis,string id,Func<Task> guard)
    {
        var rows=await ReadDevelopmentRows<AFE>(source,connection,"AFE",[AfeFilter("AFE_ID",id)],guard);
        if(rows.Length>1)throw new InvalidOperationException("The original development AFE identity is ambiguous.");
        var row=rows.SingleOrDefault();if(row is not null)RequireDevelopmentRow(row,basis);return row;
    }
    private static void RequireDevelopmentRow(AFE row,DevelopmentAfeReview basis)
    {
        var scope=basis.Source.Review.Scope;
        if(row.OWNER_TENANT_ID!=scope.Company.TenantId || row.OWNER_COMPANY_BA_ID!=scope.Company.BusinessAssociateId || row.FIELD_ID!=scope.FieldId || row.FDP_BASELINE_ID!=basis.Source.Baseline!.FDP_ID)
            throw new UnauthorizedAccessException("The development AFE belongs to another original company/FIELD/baseline.");
        var original=DevelopmentAfeCommand.FromRecord(row);
        if(original.Review.BudgetDatabase!=basis.BudgetDatabase || original.Review.Source.Review.Scope.CoreDatabase!=scope.CoreDatabase ||
            DevelopmentPlanRelease.ReviewEvidenceRevision(original.Review.Source.Review)!=DevelopmentPlanRelease.ReviewEvidenceRevision(basis.Source.Review) ||
            JsonSerializer.Serialize(original.Review.Source.Baseline)!=JsonSerializer.Serialize(basis.Source.Baseline))throw new InvalidOperationException("The original AFE source or native bindings changed.");
    }
    internal async Task<AFE[]> DevelopmentPopulation(IDataSource source,string connection,DevelopmentAfeReview basis,Func<Task> guard)
    {
        var scope=basis.Source.Review.Scope;
        var rows=await ReadDevelopmentRows<AFE>(source,connection,"AFE",[AfeFilter("OWNER_TENANT_ID",scope.Company.TenantId),AfeFilter("OWNER_COMPANY_BA_ID",scope.Company.BusinessAssociateId),AfeFilter("FDP_BASELINE_ID",basis.Source.Baseline!.FDP_ID)],guard);
        if(rows.Length>DevelopmentAfeReview.MaximumAfes || rows.Select(row=>row.AFE_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length)throw new InvalidOperationException("The complete bounded unique development AFE population is required.");
        foreach(var row in rows)RequireDevelopmentRow(row,basis);
        var found=rows.ToDictionary(row=>row.AFE_ID,StringComparer.Ordinal);
        // Exact known book identities remain observable when a recorded row's FIELD/ownership/active flag was altered.
        foreach(var book in basis.Books.Books)
        {
            var id=DevelopmentAfeCommand.IdFor(scope.Company,basis.Source.Baseline.FDP_ID,book.BookId);var row=await DevelopmentOriginal(source,connection,basis,id,guard);
            if(row is not null){if(found.TryGetValue(id,out var listed) && JsonSerializer.Serialize(listed)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The development AFE population changed during lookup.");found[id]=row;}
        }
        if(found.Count>DevelopmentAfeReview.MaximumAfes)throw new InvalidOperationException("The development AFE population exceeds its bound.");return found.Values.OrderBy(row=>row.AFE_ID,StringComparer.Ordinal).ToArray();
    }
    public Task<DevelopmentAfeReview> ReviewDevelopmentAfesAsync(DevelopmentAfeReview basis,Func<Task> authority)
    {
        basis.Require();basis=CompanyInventorySourceReview.Copy(basis);
        return OwnedDevelopment(basis,authority,async(source,connection,guard)=>{
            var first=await DevelopmentPopulation(source,connection,basis,guard);var closing=await DevelopmentPopulation(source,connection,basis,guard);
            if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The recorded development AFE population changed during review.");
            var result=basis with{Afes=closing.Select(DevelopmentAfeCommand.Facts).ToArray(),CheckedAtUtc=DateTimeOffset.UtcNow};result=result with{Revision=result.ContentRevision()};result.Require();return result;
        });
    }
    public async Task<DevelopmentAfeReceipt> RecordDevelopmentAfeAsync(DevelopmentAfeCommand original,Func<Task> authority,Func<Task<DevelopmentAfeReview>> readCurrent,bool recover=false)
    {
        original.Require();original=CompanyInventorySourceReview.Copy(original);var basis=original.Review;AFE? recorded=null;
        await OwnedDevelopment(basis,authority,async(source,connection,guard)=>{
            recorded=await DevelopmentOriginal(source,connection,basis,original.AfeId(),guard);
            if(recorded is not null)
            {
                if(DevelopmentAfeCommand.FromRecord(recorded).Revision()!=original.Revision())throw new InvalidOperationException("Another original request owns this baseline/book AFE. Inspect it before preparing more work.");return true;
            }
            return true;
        });
        if(recorded is null && !recover)
        {
            original.RequireCurrent(await readCurrent());
            await OwnedDevelopment(basis,authority,async(source,connection,guard)=>{
                if(await DevelopmentOriginal(source,connection,basis,original.AfeId(),guard) is not null)throw new InvalidOperationException("Another request reserved this baseline/book AFE.");
                async Task Book()
                {
                    RequireOwnedBudgetSchema(source,"COMPANY_LEDGER_BOOK",typeof(COMPANY_LEDGER_BOOK));var scope=basis.Source.Review.Scope;
                    var rows=await ReadDevelopmentRows<COMPANY_LEDGER_BOOK>(source,connection,"COMPANY_LEDGER_BOOK",[AfeFilter("OWNER_TENANT_ID",scope.Company.TenantId),AfeFilter("OWNER_COMPANY_BA_ID",scope.Company.BusinessAssociateId),AfeFilter("BOOK_ID",original.BookId)],guard);
                    var current=CompanyLedgerBookReview.Create(scope.Company,scope.ActorUserId,rows);
                    if(current.Books.Count!=1 || current.Books[0]!=original.Book())throw new InvalidOperationException("The exact configured book/currency changed inside native AFE admission.");
                }
                await Book();var rows=await DevelopmentPopulation(source,connection,basis,guard);original.RequireCurrent(basis with{Afes=rows.Select(DevelopmentAfeCommand.Facts).ToArray(),Revision=(basis with{Afes=rows.Select(DevelopmentAfeCommand.Facts).ToArray()}).ContentRevision()});
                var scope=basis.Source.Review.Scope;var numbers=await ReadDevelopmentRows<AFE>(source,connection,"AFE",[AfeFilter("OWNER_TENANT_ID",scope.Company.TenantId),AfeFilter("OWNER_COMPANY_BA_ID",scope.Company.BusinessAssociateId),AfeFilter("BOOK_ID",original.BookId),AfeFilter("AFE_NUMBER",original.Number)],guard);
                if(numbers.Length!=0)throw new InvalidOperationException("This AFE number is already reserved in the selected company/book.");
                var row=original.ToRecord(DateTime.UtcNow);await guard();BeepOperationResult.EnsureSuccess(source.InsertEntity("AFE",row),"Insert development AFE","AFE");
                if(source.ErrorObject is {} write && !BeepOperationResult.IsSuccess(write))throw new InvalidOperationException("The native development AFE insert is unconfirmed.");
                recorded=await DevelopmentOriginal(source,connection,basis,original.AfeId(),guard);
                if(recorded is null || DevelopmentAfeCommand.FromRecord(recorded).Revision()!=original.Revision())throw new InvalidOperationException("The original development AFE could not be read back.");
                await Book();var closing=await DevelopmentPopulation(source,connection,basis,guard);var current=basis with{Afes=closing.Select(DevelopmentAfeCommand.Facts).ToArray()};current=current with{Revision=current.ContentRevision()};original.RequireCurrent(current);return true;
            });
            original.RequireCurrent(await readCurrent());
        }
        var closingRow=await OwnedDevelopment(basis,authority,async(source,connection,guard)=>await DevelopmentOriginal(source,connection,basis,original.AfeId(),guard));
        if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(closingRow))throw new InvalidOperationException("The original AFE or its absence changed during acknowledgement.");
        var receipt=new DevelopmentAfeReceipt(original.Revision(),closingRow,DateTimeOffset.UtcNow);receipt.Require(original);return receipt;
    }
}
