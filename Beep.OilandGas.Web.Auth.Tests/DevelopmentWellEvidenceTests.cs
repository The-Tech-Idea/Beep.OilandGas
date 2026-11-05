using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Pages.PPDM39.Development;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DevelopmentWellEvidenceTests
{
    private const string Field = "field/واحد%2F";
    private const string Well = "well/واحد%2F";
    private static CompanyClientContext Company()
    {
        var company=new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(),"engineer",new(new("tenant","company"),["ConfiguredEngineerRole"],[])));
        return company;
    }
    private static FieldResponse Selection() => new() { FieldId=Field,FieldName="Recorded field",SelectionAvailable=true,
        SelectionContext=new(new("tenant","company"),new("core","binding"),"engineer","revision",Field) };
    private static WELL Row() => new() { UWI=Well,WELL_NAME="Recorded <well>",ASSIGNED_FIELD=Field,ACTIVE_IND="Y",CURRENT_CLASS="DEVELOPMENT",
        FINAL_TD=1500.25m,FINAL_TD_OUOM="ft",DRILL_TD=1400.75m,SPUD_DATE=new(2026,10,2),COMPLETION_DATE=null };
    private static Mock<IFieldSelectionContext> Fields(Func<FieldResponse>? next = null)
    {
        var fields=new Mock<IFieldSelectionContext>();
        fields.Setup(value=>value.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).Returns(()=>Task.FromResult((next ?? Selection)()));
        return fields;
    }
    [Theory]
    [InlineData("current")][InlineData("empty")][InlineData("duplicate")][InlineData("foreign-field")][InlineData("inactive")]
    [InlineData("class")][InlineData("identity")][InlineData("failed")][InlineData("no-field")][InlineData("foreign-company")]
    [InlineData("foreign-actor")][InlineData("revision")][InlineData("database")][InlineData("late-company")][InlineData("late-field")]
    public async Task RecordedWellPopulationRequiresUnchangedCompanyFieldAndCompleteIdentities(string mode)
    {
        using var company=Company();var selections=0;
        var fields=Fields(()=>
        {
            var current=Selection();++selections;
            if(mode=="no-field") { current.SelectionAvailable=false;current.FieldId="";current.SelectionContext=current.SelectionContext! with { FieldId=null }; }
            if(mode=="foreign-company") current.SelectionContext=current.SelectionContext! with { Company=new("tenant","other") };
            if(mode=="foreign-actor") current.SelectionContext=current.SelectionContext! with { ActorUserId="other" };
            if(selections>1 && mode=="revision") current.SelectionContext=current.SelectionContext! with { Revision="new-revision" };
            if(selections>1 && mode=="database") current.SelectionContext=current.SelectionContext! with { Database=new("other","binding") };
            return current;
        });
        var development=new Mock<IDevelopmentServiceClient>();var reads=0;var raw=Row();
        development.Setup(value=>value.GetWellsAsync(It.IsAny<CancellationToken>(),Field)).Returns(()=>
        {
            ++reads;
            if(mode=="failed") throw new HttpRequestException("Unavailable");
            if(mode=="foreign-field") raw.ASSIGNED_FIELD="other";
            if(mode=="inactive") raw.ACTIVE_IND="N";
            if(mode=="class") raw.CURRENT_CLASS="PRODUCTION";
            if(mode=="identity") raw.UWI="";
            if(mode=="late-company") { company.Clear();company.TrySelect(company.Version,"engineer",new(new("tenant","company"),["ConfiguredEngineerRole"],[])); }
            if(mode=="late-field") fields.Raise(value=>value.CurrentFieldChanged+=null!,"other");
            return Task.FromResult(mode=="empty" ? new List<WELL>() : mode=="duplicate" ? new List<WELL>{raw,raw} : [raw]);
        });
        using var state=new DevelopmentWellEvidenceState(company,fields.Object,development.Object,(_,_,sentence)=>sentence);
        await state.LoadAsync();
        if(mode is "current" or "empty")
        {
            Assert.True(state.Loaded);Assert.Null(state.Error);Assert.NotNull(state.LoadedAtUtc);
            Assert.Equal(Field,state.Review!.FieldId);
            if(mode=="current")
            {
                Assert.Equal(Well,Assert.Single(state.Rows).UWI);
                raw.WELL_NAME="Transport changed";Assert.Equal("Recorded <well>",Assert.Single(state.Rows).WELL_NAME);
                state.Rows[0].WELL_NAME="Consumer changed";Assert.Equal("Recorded <well>",Assert.Single(state.Rows).WELL_NAME);
            }
            else Assert.Empty(state.Rows);
        }
        else
        {
            Assert.False(state.Loaded);Assert.Empty(state.Rows);Assert.Null(state.Review);Assert.Null(state.Selected);Assert.Null(state.LoadedAtUtc);
            if(mode=="no-field") { Assert.True(state.NeedsField);Assert.Equal(0,reads); }
            else if(mode is not ("late-company" or "late-field")) Assert.NotNull(state.Error);
        }
        Assert.False(state.Loading);
    }
    [Theory][InlineData("current")][InlineData("absent")][InlineData("substituted")][InlineData("changed")][InlineData("disposed")]
    public async Task DetailReadsKeepOriginalWellAndClearAfterContextChange(string mode)
    {
        using var company=Company();var fields=Fields();var development=new Mock<IDevelopmentServiceClient>();
        development.Setup(value=>value.GetWellAsync(Well,It.IsAny<CancellationToken>(),Field)).ReturnsAsync(mode=="absent" ? null : mode=="substituted" ? new WELL{UWI="other",ASSIGNED_FIELD=Field,ACTIVE_IND="Y",CURRENT_CLASS="DEVELOPMENT"} : Row());
        var state=new DevelopmentWellEvidenceState(company,fields.Object,development.Object,(_,_,sentence)=>sentence);
        await state.LoadAsync(Well);
        if(mode=="changed") fields.Raise(value=>value.CurrentFieldChanged+=null!,"other");
        if(mode=="disposed") state.Dispose();
        if(mode=="current") Assert.Equal(Well,state.Selected!.UWI);
        else if(mode=="absent") { Assert.True(state.Loaded);Assert.True(state.NotFound);Assert.Null(state.Selected); }
        else { Assert.Null(state.Selected);Assert.Empty(state.Rows);Assert.False(state.Loaded); }
        state.Dispose();
        development.Verify(value=>value.AssignRigAsync(It.IsAny<string>(),It.IsAny<DevelopmentAssignRigRequest>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private sealed class Navigation : NavigationManager
    {
        public Navigation(string path)=>Initialize("https://render.invalid/","https://render.invalid"+path);
        protected override void NavigateToCore(string uri,bool forceLoad)=>throw new InvalidOperationException("Static rendering cannot navigate.");
    }
    [Theory][InlineData("newer-load")][InlineData("disposed")][InlineData("route-clear")][InlineData("failed-refresh")]
    public async Task LateReadsCannotReplaceNewerEvidenceOrRestoreClearedPages(string mode)
    {
        using var company=Company();var fields=Fields();var development=new Mock<IDevelopmentServiceClient>();
        var pending=new TaskCompletionSource<List<WELL>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls=0;
        development.Setup(value=>value.GetWellsAsync(It.IsAny<CancellationToken>(),Field)).Returns(()=>
        {
            ++calls;
            if(calls==1) return pending.Task;
            if(mode=="failed-refresh") throw new HttpRequestException("Refresh unavailable");
            var fresh=Row();fresh.WELL_NAME="Newer recorded evidence";return Task.FromResult(new List<WELL>{fresh});
        });
        using var state=new DevelopmentWellEvidenceState(company,fields.Object,development.Object,(_,_,sentence)=>sentence);
        var first=state.LoadAsync();
        if(mode=="disposed") state.Dispose();
        else if(mode=="route-clear") state.Clear();
        else await state.LoadAsync();
        pending.SetResult([Row()]);await first;
        if(mode=="newer-load") Assert.Equal("Newer recorded evidence",Assert.Single(state.Rows).WELL_NAME);
        else { Assert.Empty(state.Rows);Assert.Null(state.Review);Assert.False(state.Loaded); }
        if(mode=="failed-refresh") Assert.NotNull(state.Error);
        Assert.False(state.Loading);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task MissingCompanyOrAccountChangeDuringFieldReviewCannotReadDevelopmentWells(bool delayed)
    {
        using var company=Company();var fields=Fields();var development=new Mock<IDevelopmentServiceClient>(MockBehavior.Strict);
        var pending=new TaskCompletionSource<FieldResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        if(delayed) fields.Setup(value=>value.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).Returns(pending.Task);
        else company.Clear();
        using var state=new DevelopmentWellEvidenceState(company,fields.Object,development.Object,(_,_,sentence)=>sentence);
        var load=state.LoadAsync();
        if(delayed)
        {
            company.Clear();company.TrySelect(company.Version,"other",new(new("tenant","company"),["ConfiguredEngineerRole"],[]));
            pending.SetResult(Selection());
        }
        await load;Assert.Empty(state.Rows);Assert.Null(state.Review);Assert.False(state.Loaded);
        development.VerifyNoOtherCalls();
    }
    private sealed class NoBrowser : IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string name,object?[]? args)=>ValueTask.FromResult(default(T)!);
        public ValueTask<T> InvokeAsync<T>(string name,CancellationToken token,object?[]? args)=>InvokeAsync<T>(name,args);
    }
    [Theory][InlineData("list")][InlineData("detail")][InlineData("empty")][InlineData("new")][InlineData("failed")][InlineData("company-change")]
    [InlineData("reserved")][InlineData("conflicting")]
    public async Task ActualDesignRouteShowsRecordedSourceUnitsAndNoInventedDesignOutcomes(string mode)
    {
        using var company=Company();var fields=Fields();var development=new Mock<IDevelopmentServiceClient>();
        development.Setup(value=>value.GetWellsAsync(It.IsAny<CancellationToken>(),Field)).Returns(()=>mode=="failed" ? throw new HttpRequestException("Unavailable") : Task.FromResult(mode=="empty" ? new List<WELL>() : [Row()]));
        development.Setup(value=>value.GetWellAsync(Well,It.IsAny<CancellationToken>(),Field)).ReturnsAsync(Row());
        development.Setup(value=>value.GetWellAsync("new",It.IsAny<CancellationToken>(),Field)).ReturnsAsync(()=>{var row=Row();row.UWI="new";return row;});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton(fields.Object);services.AddSingleton(development.Object);
        services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<NavigationManager>(new Navigation(mode is "new" or "conflicting"?"/development/well-design/new":"/development/well-design"));
        services.AddSingleton(Mock.Of<IPersonaContextService>());services.AddSingleton(Mock.Of<IDrillingServiceClient>());services.AddSingleton(Mock.Of<IAfeServiceClient>());services.AddSingleton(Mock.Of<IDevelopmentDraftIntentStore>());
        services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WellDesign>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"WellUwi",mode=="reserved"?"new":mode=="conflicting"?"other":mode=="detail"?Well:null}}));
            if(mode=="company-change") { company.Clear();await Task.Yield(); }
            var html=root.ToHtmlString();Assert.Contains("Development well evidence",html);
            Assert.DoesNotContain("AFE (USD MM)",html);Assert.DoesNotContain("Submit for Approval",html);Assert.DoesNotContain("Record Spud",html);Assert.DoesNotContain("Complete Well",html);
            if(mode is "list" or "detail" or "reserved")
            {
                Assert.Contains("Recorded &lt;well&gt;",html);Assert.Contains("Company: tenant / company",html);
                if(mode=="list") Assert.Contains("/development/well-design?wellUwi="+Uri.EscapeDataString(Well),html);
                else
                {
                    Assert.Contains("Final total depth: 1500.25 ft",html);Assert.Contains("Drilling total depth: 1400.75 (unit not recorded)",html);
                    Assert.Contains("Spud date: 2026-10-02",html);Assert.Contains("Completion date: Not recorded",html);
                    Assert.Contains("Current status: Not recorded",html);Assert.DoesNotContain("DRAFT",html);
                    if(mode=="reserved") { Assert.Contains("UWI: new",html);Assert.DoesNotContain("Design authoring is not available",html); }
                }
            }
            else
            {
                Assert.DoesNotContain("Recorded &lt;well&gt;",html);
                if(mode=="empty") Assert.Contains("No recorded development wells were returned",html);
                if(mode=="failed") { Assert.Contains("Development well evidence could not be confirmed",html);Assert.DoesNotContain("0 recorded development wells",html); }
                if(mode=="new") { Assert.Contains("Select a company with drilling",html);development.Verify(value=>value.GetWellsAsync(It.IsAny<CancellationToken>(),It.IsAny<string?>()),Times.Never); }
                if(mode=="company-change") Assert.Contains("Choose a company",html);
                if(mode=="conflicting")
                {
                    Assert.Contains("Select a company with drilling",html);
                    development.Verify(value=>value.GetWellAsync(It.IsAny<string>(),It.IsAny<CancellationToken>(),It.IsAny<string?>()),Times.Never);
                }
            }
        });
    }
    private sealed class Transport : HttpMessageHandler
    {
        internal readonly List<string> Requests=[];
        internal bool Missing;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Requests.Add(request.RequestUri!.AbsoluteUri);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=Missing ? new StringContent("null",System.Text.Encoding.UTF8,"application/json") :
                request.RequestUri.AbsolutePath.EndsWith("/wells",StringComparison.Ordinal) ? JsonContent.Create(new[]{Row()}) : JsonContent.Create(Row())});
        }
    }
    [Fact]
    public async Task ActualTypedClientAssertsReviewedFieldOnListAndEncodedWellAndRefusesNullPopulation()
    {
        using var company=Company();var transport=new Transport();using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};
        var client=new DevelopmentServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<DevelopmentServiceClient>.Instance);
        Assert.Single(await client.GetWellsAsync(fieldId:Field));Assert.Equal(Well,(await client.GetWellAsync(Well,fieldId:Field))!.UWI);
        Assert.EndsWith("/wells?fieldId="+Uri.EscapeDataString(Field),transport.Requests[0]);
        Assert.EndsWith("/wells/"+Uri.EscapeDataString(Well)+"?fieldId="+Uri.EscapeDataString(Field),transport.Requests[1]);
        transport.Missing=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetWellsAsync(fieldId:Field));
        await Assert.ThrowsAsync<ArgumentException>(()=>client.GetWellsAsync(fieldId:" "));
    }
}
