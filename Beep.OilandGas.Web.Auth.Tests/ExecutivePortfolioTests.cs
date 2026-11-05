using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ExecutivePortfolioTests
{
    private static CompanyClientContext Company(){var c=new CompanyClientContext();Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant","company"),[],[])));return c;}
    private static ExecutivePortfolioReview Review()=>new(new("tenant","company"),"actor","FIELD",DateTimeOffset.UtcNow,[new("field","Recorded name")]);
    [Fact] public async Task RecordedPortfolioSelectsOnlyItsAuthorizedIdentity(){using var c=Company();using var s=new ExecutivePortfolioState(c);await s.LoadAsync(()=>Task.FromResult(Review()));s.Select("field");Assert.Equal("Recorded name",s.Selected!.FieldName);Assert.Null(s.Selected.ActiveWells);Assert.Null(s.Selected.LifecyclePhase);s.Select("foreign");Assert.Null(s.Selected);}
    [Theory][InlineData("company")][InlineData("actor")][InlineData("source")][InlineData("retrieved")][InlineData("offset")][InlineData("identity")][InlineData("duplicate")][InlineData("nullrows")][InlineData("null")][InlineData("negative")]
    public async Task UnconfirmedPortfolioCannotPublish(string fault){using var c=Company();using var s=new ExecutivePortfolioState(c);var r=Review();r=fault switch{"company"=>r with{Company=new("tenant","other")},"actor"=>r with{Actor="other"},"source"=>r with{Source="other"},"retrieved"=>r with{RetrievedAtUtc=default},"offset"=>r with{RetrievedAtUtc=DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3))},"identity"=>r with{Assets=[new("","name")]},"duplicate"=>r with{Assets=[new("field","name"),new("field","name")]},"nullrows"=>r with{Assets=null!},"negative"=>r with{Assets=[new("field","name",null,-1)]},_=>null!};await Assert.ThrowsAsync<InvalidOperationException>(()=>s.LoadAsync(()=>Task.FromResult(r)));Assert.Null(s.Current);Assert.Null(s.Selected);}
    [Fact] public async Task ActualEmptyResultIsDifferentFromFailure(){using var c=Company();using var s=new ExecutivePortfolioState(c);await s.LoadAsync(()=>Task.FromResult(Review() with{Assets=[]}));Assert.NotNull(s.Current);Assert.Empty(s.Current.Assets);await Assert.ThrowsAsync<HttpRequestException>(()=>s.LoadAsync(()=>throw new HttpRequestException()));Assert.Null(s.Current);}
    [Fact] public async Task CompanyChangeClearsAndRefusesLateResponse(){using var c=Company();using var s=new ExecutivePortfolioState(c);await s.LoadAsync(()=>Task.FromResult(Review()));s.Select("field");var pending=new TaskCompletionSource<ExecutivePortfolioReview>();var load=s.LoadAsync(()=>pending.Task);c.Clear();Assert.Null(s.Current);Assert.Null(s.Selected);pending.SetResult(Review());await load;Assert.Null(s.Current);}
    [Fact] public async Task NoCompanyDoesNotRead(){using var c=new CompanyClientContext();using var s=new ExecutivePortfolioState(c);await s.LoadAsync(()=>throw new Exception());Assert.Null(s.Current);}
    [Fact] public async Task DisposedPortfolioCannotPublishAnAlreadyRunningReply(){using var c=Company();var s=new ExecutivePortfolioState(c);await s.LoadAsync(()=>Task.FromResult(Review()));s.Select("field");var pending=new TaskCompletionSource<ExecutivePortfolioReview>(TaskCreationOptions.RunContinuationsAsynchronously);var load=s.LoadAsync(()=>pending.Task);s.Dispose();pending.SetResult(Review());await load;Assert.Null(s.Current);Assert.Null(s.Selected);}
    [Fact] public void ActualFieldProjectionDoesNotInventPhaseWellsOrName(){var r=ExecutivePortfolioReviews.Create([new FIELD{FIELD_ID="field",FIELD_NAME=null}],new("tenant","company"),"actor",DateTimeOffset.UtcNow);Assert.Equal("field",Assert.Single(r.Assets).FieldId);Assert.Null(r.Assets[0].FieldName);Assert.Null(r.Assets[0].LifecyclePhase);Assert.Null(r.Assets[0].ActiveWells);}
    [Theory][InlineData("null")][InlineData("wrongtype")][InlineData("identity")]
    public void MissingOrWrongFieldSourceCannotProducePortfolio(string fault){IEnumerable<object>? rows=fault switch{"null"=>null,"wrongtype"=>[new object()],_=>[new FIELD{FIELD_ID=""}]};Assert.Throws<InvalidOperationException>(()=>ExecutivePortfolioReviews.Create(rows,new("tenant","company"),"actor",DateTimeOffset.UtcNow));}
}
