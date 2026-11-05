using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;
public sealed class AccountingPeriodCheckState : IDisposable
{
    private readonly CompanyClientContext company;
    public AccountingPeriodCheckState(CompanyClientContext company){this.company=company;company.Changed+=Clear;}
    private int generation;private bool disposed;
    public int Version => generation;
    public AccountingPeriodCheckReview? Current {get;private set;}
    public void Clear(){++generation;Current=null;}
    public async Task LoadAsync(DateTime end,string book,Func<Task<string?>> fieldLookup,Func<DateTime,string,Task<AccountingPeriodCheckReview>> read)
    {
        if(disposed)return;Clear();var version=generation;var scope=company.Snapshot;
        if(scope.Selection is null || end==default || end.TimeOfDay!=TimeSpan.Zero || string.IsNullOrWhiteSpace(book))return;
        var field=await fieldLookup();if(disposed || version!=generation || scope!=company.Snapshot || string.IsNullOrWhiteSpace(field))return;
        var result=await read(end,book);if(disposed || version!=generation || scope!=company.Snapshot)return;
        if(result is null || result.Company!=scope.Selection.Scope || result.ActorUserId!=scope.Selection.UserId || result.FieldId!=field || result.BookId!=book || result.PeriodEnd!=end ||
            result.GeneratedAtUtc==default || result.GeneratedAtUtc.Kind!=DateTimeKind.Utc || result.Checks is null ||
            result.Checks.Any(row=>row is null || string.IsNullOrWhiteSpace(row.RuleId) || string.IsNullOrWhiteSpace(row.Name) || row.Scope!=(row.RuleId=="GL_BALANCE" ? "BOOK" : "COMPANY_DATABASE")) ||
            result.Checks.Select(row=>row.RuleId).Distinct(StringComparer.Ordinal).Count()!=result.Checks.Count ||
            !new[]{"GL_BALANCE","AP_CLOSE","AR_CLOSE"}.All(id=>result.Checks.Any(row=>row.RuleId==id && row.Mandatory)))
            throw new InvalidOperationException("Period checks do not match the current company, FIELD, book and date.");
        Current=result;
    }
    public void Dispose(){disposed=true;company.Changed-=Clear;Clear();}
}
