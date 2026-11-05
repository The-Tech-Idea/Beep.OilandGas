using Beep.OilandGas.Web.Services;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class AccountingPeriodCheckTests
{
    private static readonly DateTime End=new(2026,9,30);
    private static CompanyClientContext Company(){var c=new CompanyClientContext();Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant","A"),["Accountant"],[])));return c;}
    private static PeriodCloseChecklist Checks()=>new(){PeriodEndDate=End,ChecklistGeneratedAt=DateTime.UtcNow,IsReadyToClose=true,
        Items=new[]{"GL_BALANCE","AP_CLOSE","AR_CLOSE"}.Select(id=>new PeriodCloseChecklistItem{RuleId=id,Name=id,Description="Executed",Module=id=="GL_BALANCE"?"GL":id[..2],IsMandatory=true,IsComplete=id!="AP_CLOSE"}).ToList()};
    private static AccountingPeriodCheckReview Review()=>AccountingPeriodCheckReviews.Create(new("tenant","A"),"actor","field","book",End,Checks());
    [Fact] public void ActualCheckResultsAndScopeArePreservedInsteadOfOwnerReadyFlag()
    {
        var review=Review();Assert.Equal(3,review.Checks.Count);Assert.False(Assert.Single(review.Checks.Where(row=>row.RuleId=="AP_CLOSE")).Passed);
        Assert.Equal("BOOK",review.Checks[0].Scope);Assert.Equal("COMPANY_DATABASE",review.Checks[1].Scope);Assert.Equal("book",review.BookId);
    }
    [Theory][InlineData("missing")][InlineData("duplicate")][InlineData("date")][InlineData("time")]
    public void IncompleteExecutedCheckEvidenceCannotBecomeReadiness(string fault)
    {
        var checks=Checks();switch(fault){case "missing":checks.Items.RemoveAt(0);break;case "duplicate":checks.Items.Add(checks.Items[0]);break;case "date":checks.PeriodEndDate=End.AddDays(-1);break;default:checks.ChecklistGeneratedAt=DateTime.SpecifyKind(checks.ChecklistGeneratedAt,DateTimeKind.Unspecified);break;}
        Assert.Throws<InvalidOperationException>(()=>AccountingPeriodCheckReviews.Create(new("tenant","A"),"actor","field","book",End,checks));
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("field")][InlineData("book")][InlineData("date")][InlineData("scope")]
    public async Task ReviewMustMatchCompanyAccountFieldBookAndDate(string fault)
    {
        using var c=Company();using var state=new AccountingPeriodCheckState(c);var review=Review();
        review=fault switch {"company"=>review with {Company=new("tenant","B")},"actor"=>review with {ActorUserId="other"},"field"=>review with {FieldId="other"},"book"=>review with {BookId="other"},"date"=>review with {PeriodEnd=End.AddDays(-1)},_=>review with {Checks=review.Checks.Select(row=>row with {Scope="BOOK"}).ToArray()}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync(End,"book",()=>Task.FromResult<string?>("field"),(_,_)=>Task.FromResult(review)));Assert.Null(state.Current);
    }
    [Fact] public async Task CompanySwitchClearsPreviouslyDisplayedChecksAndRejectsLateResults()
    {
        using var c=Company();using var state=new AccountingPeriodCheckState(c);await state.LoadAsync(End,"book",()=>Task.FromResult<string?>("field"),(_,_)=>Task.FromResult(Review()));Assert.NotNull(state.Current);
        var pending=new TaskCompletionSource<AccountingPeriodCheckReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        var load=state.LoadAsync(End,"book",()=>Task.FromResult<string?>("field"),(_,_)=>pending.Task);c.Clear();Assert.Null(state.Current);pending.SetResult(Review());await load;Assert.Null(state.Current);
    }
    [Fact] public async Task InvalidationDuringFieldLookupCannotStartChecks()
    {
        using var c=Company();using var state=new AccountingPeriodCheckState(c);var field=new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);var calls=0;
        var load=state.LoadAsync(End,"book",()=>field.Task,(_,_)=>{calls++;return Task.FromResult(Review());});state.Clear();field.SetResult("field");await load;Assert.Equal(0,calls);Assert.Null(state.Current);
    }
    [Fact] public async Task FailedReloadClearsOldChecks()
    {
        using var c=Company();using var state=new AccountingPeriodCheckState(c);await state.LoadAsync(End,"book",()=>Task.FromResult<string?>("field"),(_,_)=>Task.FromResult(Review()));
        await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadAsync(End,"book",()=>Task.FromResult<string?>("field"),(_,_)=>throw new HttpRequestException()));Assert.Null(state.Current);
    }
}
