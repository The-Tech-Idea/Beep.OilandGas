using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Pages.PPDM39.Production;
using Beep.OilandGas.Web.Components.BusinessProcess;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ProductionInterventionPageTests
{
    private const string Well="well/الكويت%2F";
    private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    private sealed class Auth:AuthenticationStateProvider
    {
        internal ClaimsPrincipal User=new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"reviewer"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"fixture"));
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(User));
        internal void SignOut(){User=new(new ClaimsIdentity());NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
        internal void OtherRole(){User=new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"reviewer"),new(ClaimTypes.Role,"OtherCompanyRole")],"fixture"));NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("Static page check has no browser.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    private sealed class Nav:NavigationManager
    {public Nav()=>Initialize("http://fixture/","http://fixture/production/intervention");protected override void NavigateToCore(string uri,bool forceLoad)=>Uri=ToAbsoluteUri(uri).ToString();protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class Components:IComponentActivator
    {internal InterventionDecisions? Page;public IComponent CreateInstance(Type type){var value=(IComponent)Activator.CreateInstance(type)!;if(value is InterventionDecisions page)Page=page;return value;}}
    private sealed class HttpFixture:HttpMessageHandler
    {
        internal static FieldSelectionReview Context=new(new("tenant","company"),new("owned","fingerprint"),"reviewer","revision","field");
        internal ProductionInterventionCandidateDto Source=new(){Company=new("tenant","company"),Actor="reviewer",FieldId="field",WellId=Well,SourceRevision=new string('A',64),SourcePreparer="engineer",SourceObservationNumber=1,LatestObservationNumber=1,RetrievedAtUtc=DateTimeOffset.UtcNow,InterventionType="WORKOVER_CANDIDATE",Problem="Recorded original problem",Status="PENDING"};
        internal ProductionInterventionNativeRecovery? Native;
        private WELL NativeWell=new(){UWI=Well,ASSIGNED_FIELD="field",OPERATOR="company",ACTIVE_IND="Y"};
        private WELL_ACTIVITY Original=new(){UWI=Well,SOURCE="engineer-source",ACTIVITY_OBS_NO=1,ACTIVE_IND="Y",ACTIVITY_TYPE_ID="WORKOVER_CANDIDATE",ROW_CREATED_BY="engineer",REMARK="Recorded original problem"};
        internal HttpFixture(){Source.SelectionContext=Context;Source.SourceRevision=ProductionInterventionActivityChecks.Revision([Original],Context.Company,"reviewer","field",Well);}
        internal int Writes,Reads;internal bool FailFollowup,LoseAcknowledgement,RetainOnly;
        internal HttpFixture CopyStore()=>new(){Source=CompanyInventorySourceReview.Copy(Source),Native=Native is null?null:CompanyInventorySourceReview.Copy(Native),Writes=Writes};
        internal TaskCompletionSource<bool>? HoldRead;
        internal TaskCompletionSource<bool> ReadStarted=new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal ProductionInterventionDecisionRequest? Recorded;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            var path=request.RequestUri!.AbsolutePath;
            if(request.Method==HttpMethod.Get && path.Contains("/intervention-decisions/",StringComparison.Ordinal)){
                Reads++;if(HoldRead is {} hold){HoldRead=null;ReadStarted.TrySetResult(true);await hold.Task.WaitAsync(token);}if(FailFollowup)throw new HttpRequestException("Lost original native recovery read");return Json(Native??new(Context.Company,"reviewer",Guid.Parse(path.Split('/').Last()),null,null));
            }
            if(request.Method==HttpMethod.Get && path.EndsWith("/intervention-candidates",StringComparison.Ordinal)){
                Reads++;if(HoldRead is {} hold){HoldRead=null;ReadStarted.TrySetResult(true);await hold.Task.WaitAsync(token);}if(Writes>0 && FailFollowup)throw new HttpRequestException("Lost recorded-source follow-up");return Json(new[]{Source});
            }
            if(request.Method==HttpMethod.Get && path.EndsWith("/performance",StringComparison.Ordinal))return Json<object?>(null);
            if(request.Method==HttpMethod.Post && path.EndsWith("/intervention-decisions",StringComparison.Ordinal)){
                Assert.Equal("/api/field/current/production/intervention-decisions",path);Assert.Equal("?wellId="+Uri.EscapeDataString(Well),request.RequestUri.Query);
                Recorded=await request.Content!.ReadFromJsonAsync<ProductionInterventionDecisionRequest>(token);Writes++;
                var review=ProductionInterventionNativeReview.Create(Context,NativeWell,[Original]);var command=new ProductionInterventionNativeCommand(Recorded!.IntentId,review,Recorded,2);
                var intent=WELL_INTERVENTION_INTENT.Create(command);intent.ROW_CREATED_BY=intent.ROW_CHANGED_BY="reviewer";intent.ROW_CREATED_DATE=intent.ROW_CHANGED_DATE=DateTime.UtcNow;
                if(RetainOnly){Native=new(Context.Company,"reviewer",Recorded.IntentId,intent,null);throw new HttpRequestException("Original reservation retained; disposition not confirmed");}
                var activity=command.NewActivity();activity.ROW_CREATED_BY=activity.ROW_CHANGED_BY="reviewer";activity.ROW_CREATED_DATE=activity.ROW_CHANGED_DATE=DateTime.UtcNow;
                var outcome=WELL_INTERVENTION_RESULT.Create(intent,activity);outcome.ROW_CREATED_BY=outcome.ROW_CHANGED_BY="reviewer";outcome.ROW_CREATED_DATE=outcome.ROW_CHANGED_DATE=DateTime.UtcNow;
                Native=new(Context.Company,"reviewer",Recorded.IntentId,intent,outcome);
                Source.Status=Recorded.Decision!;Source.SourceRevision=ProductionInterventionActivityChecks.Revision([Original,activity],Context.Company,"reviewer","field",Well);Source.LatestObservationNumber=2;Source.LatestActivityType="WORKOVER_"+Recorded.Decision;Source.LatestRecordedBy="reviewer";Source.LatestActivityRemark=Recorded.Note;Source.LatestDecisionId=Recorded.IntentId;
                if(LoseAcknowledgement)throw new HttpRequestException("Native decision acknowledgement lost");
                return Json(outcome.Restore(intent));
            }
            throw new InvalidOperationException("Unexpected typed client route: "+request.Method+" "+path);
        }
        private static HttpResponseMessage Json<T>(T value)=>new(HttpStatusCode.OK){Content=JsonContent.Create(value)};
    }
    private sealed class Circuit:IAsyncDisposable
    {
        internal CompanyClientContext Company=new();internal Auth Authentication=new();internal HttpFixture Handler=new();internal IServiceProvider Provider;internal Mock<IFieldSelectionContext> Fields=new();
        internal Circuit(HttpFixture? store=null)
        {
            if(store is not null)Handler=store;
            Assert.True(Company.TrySelect(Company.Clear(),"reviewer",new(new("tenant","company"),["ConfiguredCompanyRole"],[])));
            var fields=Fields;fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("field");fields.Setup(value=>value.GetAvailableFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new(){FieldId="field",FieldName="Recorded FIELD"}]);fields.Setup(value=>value.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("owned","fingerprint"),"reviewer","revision","field")});
            var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<NavigationManager,Nav>();services.AddSingleton(Company);services.AddSingleton<AuthenticationStateProvider>(Authentication);services.AddSingleton(fields.Object);services.AddScoped<ProductionInterventionDecisionState>();services.AddScoped(provider=>new FieldSelectionState(Company,fields.Object,(_,_,message)=>message));services.AddSingleton<IComponentActivator,Components>();var failures=Mock.Of<IFailureReporter>();services.AddSingleton(new OilGasCallFailures(failures));services.AddSingleton(new ApiClient(new HttpClient(Handler){BaseAddress=new("http://fixture/")},failures,Company));services.AddSingleton<IProductionServiceClient,ProductionServiceClient>();Provider=services.BuildServiceProvider();
        }
        internal HtmlRenderer Renderer()=>new(Provider,Provider.GetRequiredService<ILoggerFactory>());
        public async ValueTask DisposeAsync(){Company.Dispose();if(Provider is IAsyncDisposable asyncProvider)await asyncProvider.DisposeAsync();Handler.Dispose();}
    }
    private static Task Invoke(object page,string method,params object[] arguments)=>(Task)page.GetType().GetMethod(method,Private)!.Invoke(page,arguments)!;
    private static Task SelectRecorded(object page)=>Invoke(page,"SelectCandidateAsync",((List<ProductionInterventionCandidateDto>)page.GetType().GetField("_candidates",Private)!.GetValue(page)!).Single());
    private static void Field(object page,string name,object value)=>page.GetType().GetField(name,Private)!.SetValue(page,value);
    private static void Refresh(object page)=>typeof(ComponentBase).GetMethod("StateHasChanged",Private)!.Invoke(page,null);
    [Theory][InlineData("signout")][InlineData("role")]
    public async Task CurrentStandardCompanyRolesAreRequiredBeforePageLoadsPrivateSource(string change)
    {
        await using var circuit=new Circuit();if(change=="signout")circuit.Authentication.SignOut();else circuit.Authentication.OtherRole();await using var renderer=circuit.Renderer();await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<InterventionDecisions>();Assert.Equal(0,circuit.Handler.Reads);Assert.DoesNotContain("Recorded original problem",root.ToHtmlString());Assert.Contains("Intervention source is unconfirmed",root.ToHtmlString());Assert.Equal(0,circuit.Handler.Writes);
        });
    }
    [Theory][InlineData("signout")][InlineData("role")][InlineData("field")][InlineData("company")]
    public async Task ActualLateAcknowledgementReviewKeepsOriginalAndHidesSourceAfterContextDrift(string change)
    {
        await using var circuit=new Circuit();var state=circuit.Provider.GetRequiredService<ProductionInterventionDecisionState>();await using var renderer=circuit.Renderer();await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<InterventionDecisions>();var page=((Components)circuit.Provider.GetRequiredService<IComponentActivator>()).Page!;await SelectRecorded(page);Field(page,"_decisionNote","Original private late reason");circuit.Handler.FailFollowup=true;await Invoke(page,"RejectIntervention");Assert.True(state.HasAcknowledgement("field",Well));circuit.Handler.FailFollowup=false;var held=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);circuit.Handler.HoldRead=held;var review=Invoke(page,"ReviewAcknowledgedDecisionAsync");await circuit.Handler.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            if(change=="signout")circuit.Authentication.SignOut();if(change=="role")circuit.Authentication.OtherRole();if(change=="field")circuit.Fields.Raise(value=>value.CurrentFieldChanged+=null,"other");if(change=="company"){Assert.True(circuit.Company.TrySelect(circuit.Company.Clear(),"reviewer",new(new("tenant","other"),["ConfiguredCompanyRole"],[])));Assert.True(circuit.Company.TrySelect(circuit.Company.Clear(),"reviewer",new(new("tenant","company"),["ConfiguredCompanyRole"],[])));}
            held.SetResult(true);await review;Refresh(page);Assert.True(state.IsBlocked("field",Well));Assert.True(state.HasAcknowledgement("field",Well));Assert.Equal(1,circuit.Handler.Writes);Assert.DoesNotContain("Original private late reason",root.ToHtmlString());Assert.DoesNotContain("Review original recorded decision",root.ToHtmlString());
        });
    }
    [Fact]public async Task LostAcknowledgementSurvivesActualPageRecreationWithoutRepeatingDecisionOrInventingRecovery()
    {
        await using var circuit=new Circuit();var state=circuit.Provider.GetRequiredService<ProductionInterventionDecisionState>();circuit.Handler.LoseAcknowledgement=true;
        await using(var renderer=circuit.Renderer())await renderer.Dispatcher.InvokeAsync(async()=>{await renderer.RenderComponentAsync<InterventionDecisions>();var page=((Components)circuit.Provider.GetRequiredService<IComponentActivator>()).Page!;await SelectRecorded(page);Field(page,"_decisionNote","Original lost acknowledgement reason");await Invoke(page,"RejectIntervention");Assert.True(state.IsBlocked("field",Well));Assert.False(state.HasAcknowledgement("field",Well));Assert.Equal(1,circuit.Handler.Writes);});
        await using(var renderer=circuit.Renderer())await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<InterventionDecisions>();var page=((Components)circuit.Provider.GetRequiredService<IComponentActivator>()).Page!;await SelectRecorded(page);Assert.Contains("Original lost acknowledgement reason",root.ToHtmlString());Assert.Contains("Review original recorded decision",root.ToHtmlString());Field(page,"_decisionNote","Another decision must be refused");await Invoke(page,"RejectIntervention");Assert.Equal(1,circuit.Handler.Writes);Assert.Equal("Original lost acknowledgement reason",state.Original("field",Well)!.Request.Note);});
    }
    [Fact]public async Task ActualPageRecreationKeepsAcknowledgedOriginalAndReviewsReadOnlyWithoutAnotherDecision()
    {
        await using var circuit=new Circuit();var state=circuit.Provider.GetRequiredService<ProductionInterventionDecisionState>();
        await using(var renderer=circuit.Renderer())await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<InterventionDecisions>();var page=((Components)circuit.Provider.GetRequiredService<IComponentActivator>()).Page!;await SelectRecorded(page);Assert.True(circuit.Provider.GetRequiredService<FieldSelectionState>().Loaded);Field(page,"_decisionNote","Original independent rejection");circuit.Handler.FailFollowup=true;await Invoke(page,"RejectIntervention");Refresh(page);Assert.Equal(1,circuit.Handler.Writes);Assert.True(state.HasAcknowledgement("field",Well));Assert.Contains("Original independent rejection",root.ToHtmlString());
        });
        circuit.Handler.FailFollowup=false;
        await using(var renderer=circuit.Renderer())await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<InterventionDecisions>();var page=((Components)circuit.Provider.GetRequiredService<IComponentActivator>()).Page!;await SelectRecorded(page);Assert.True(circuit.Provider.GetRequiredService<FieldSelectionState>().Loaded);Assert.Contains("original decision outcome is unconfirmed",root.ToHtmlString());Assert.Contains("Review original recorded decision",root.ToHtmlString());await Invoke(page,"ReviewAcknowledgedDecisionAsync");Refresh(page);Assert.False(state.IsBlocked("field",Well));Assert.Equal(1,circuit.Handler.Writes);Assert.DoesNotContain("original decision outcome is unconfirmed",root.ToHtmlString());
        });
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task FreshCircuitRecoversOriginalNativeReferenceWithoutSubmittingAnotherDecision(bool reservationOnly)
    {
        HttpFixture store;Guid id;
        await using(var original=new Circuit())
        {
            original.Handler.LoseAcknowledgement=true;original.Handler.RetainOnly=reservationOnly;
            await using var renderer=original.Renderer();
            await renderer.Dispatcher.InvokeAsync(async()=>{
                await renderer.RenderComponentAsync<InterventionDecisions>();var page=((Components)original.Provider.GetRequiredService<IComponentActivator>()).Page!;
                await SelectRecorded(page);Field(page,"_decisionNote","Original circuit-loss reason");await Invoke(page,"RejectIntervention");
                Assert.True(original.Provider.GetRequiredService<ProductionInterventionDecisionState>().IsBlocked("field",Well));Assert.Equal(1,original.Handler.Writes);
                Assert.Contains("decisionId=",original.Provider.GetRequiredService<NavigationManager>().Uri);
            });
            id=original.Handler.Recorded!.IntentId;store=original.Handler.CopyStore();
        }
        await using var fresh=new Circuit(store);await using var second=fresh.Renderer();
        await second.Dispatcher.InvokeAsync(async()=>{
            var root=await second.RenderComponentAsync<InterventionDecisions>();var page=((Components)fresh.Provider.GetRequiredService<IComponentActivator>()).Page!;
            page.WellId=Well;page.DecisionId=id;await SelectRecorded(page);Field(page,"_decisionNote","A different request must stay blocked");await Invoke(page,"RejectIntervention");Assert.Equal(1,fresh.Handler.Writes);
            Assert.Equal(id,page.GetType().GetField("_recoveryId",Private)!.GetValue(page));
            Assert.Equal(true,page.GetType().GetField("_confirmed",Private)!.GetValue(page));
            Assert.NotNull(page.GetType().GetField("_selectedIntervention",Private)!.GetValue(page));
            Assert.Equal(false,page.GetType().GetField("_deciding",Private)!.GetValue(page));
            await Invoke(page,"ReviewNativeDecisionAsync");Refresh(page);Assert.Equal(1,fresh.Handler.Writes);
            Assert.Null(page.GetType().GetField("_error",Private)!.GetValue(page));
            Assert.NotNull(page.GetType().GetField("_recoveryMessage",Private)!.GetValue(page));
            Assert.Contains(reservationOnly?"original request is retained":"Original REJECTED decision confirmed",root.ToHtmlString());
            Assert.Equal(reservationOnly,fresh.Provider.GetRequiredService<ProductionInterventionDecisionState>().IsBlocked("field",Well));
            if(reservationOnly)Assert.Equal("Original circuit-loss reason",fresh.Provider.GetRequiredService<ProductionInterventionDecisionState>().Original("field",Well)!.Request.Note);
        });
    }

    [Fact]
    public async Task MissingNativeReservationDoesNotClearReferenceOrInventAnOutcomeInFreshCircuit()
    {
        await using var fresh=new Circuit();await using var renderer=fresh.Renderer();
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<InterventionDecisions>();var page=((Components)fresh.Provider.GetRequiredService<IComponentActivator>()).Page!;
            page.WellId=Well;page.DecisionId=Guid.NewGuid();await SelectRecorded(page);await Invoke(page,"ReviewNativeDecisionAsync");
            Field(page,"_decisionNote","No replacement decision");await Invoke(page,"RejectIntervention");Refresh(page);
            await Invoke(page,"TransitionToDecommissioning");
            Assert.Equal(0,fresh.Handler.Writes);Assert.Contains("Original decision reference",root.ToHtmlString());Assert.DoesNotContain("decision confirmed",root.ToHtmlString());
        });
    }
    [Theory][InlineData("missing-review")][InlineData("changed-review")][InlineData("maker")][InlineData("closed")]
    public async Task NativeScreenRefusesUnreviewedOrClosedPreparationBeforeTransport(string fault)
    {
        await using var circuit=new Circuit();
        if(fault=="missing-review")circuit.Handler.Source.SelectionContext=null;
        if(fault=="maker")circuit.Handler.Source.SourcePreparer="reviewer";
        if(fault=="closed")circuit.Handler.Source.LatestActivityType="WORKOVER_APPROVED";
        await using var renderer=circuit.Renderer();await renderer.Dispatcher.InvokeAsync(async()=>{
            await renderer.RenderComponentAsync<InterventionDecisions>();var page=((Components)circuit.Provider.GetRequiredService<IComponentActivator>()).Page!;await SelectRecorded(page);
            if(fault=="changed-review")circuit.Fields.Setup(value=>value.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId="field",SelectionAvailable=true,SelectionContext=HttpFixture.Context with{Revision="changed-review"}});
            Field(page,"_decisionNote","No unreviewed replacement decision");await Invoke(page,"RejectIntervention");Assert.Equal(0,circuit.Handler.Writes);
            Assert.False(circuit.Provider.GetRequiredService<ProductionInterventionDecisionState>().IsBlocked("field",Well));
        });
    }
    [Theory][InlineData("allowed")][InlineData("company")][InlineData("role")][InlineData("signout")][InlineData("component")]
    public async Task ReceivingSourceShowsOnlyClosedOriginalContextAndNeverClaimsAcceptance(string change)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"reviewer",new(new("tenant","company"),["ConfiguredCompanyRole"],[])));
        var auth=new Auth();var id=Guid.NewGuid();var order=new WorkOrderSummary{InstanceId="order",FieldId="field",Source="WORK_ORDER",WoSubType=WorkOrderSubType.Corrective,Components=[new("order","component","field",Well,"equipment")]};
        var hold=new TaskCompletionSource<ProductionInterventionMaintenanceLink>(TaskCreationOptions.RunContinuationsAsynchronously);var started=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var client=new Mock<IProductionServiceClient>();client.Setup(value=>value.GetInterventionMaintenanceLinkAsync(Well,id,"order",It.IsAny<CancellationToken>())).Returns(()=>{started.TrySetResult(true);return hold.Task;});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton<IProductionServiceClient>(client.Object);services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<NavigationManager,Nav>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        services.AddSingleton(Mock.Of<ICompanyMaintenanceHandoffClient>());services.AddScoped<CompanyMaintenanceHandoffState>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var value=new ProductionInterventionMaintenanceLink(new("tenant","company"),"reviewer","field","order",Well,id,"original-engineer","Recorded <receiving scope>",DateTime.UtcNow,new string('A',64),new string('B',64),DateTimeOffset.UtcNow);
        var rendering=renderer.Dispatcher.InvokeAsync(()=>renderer.RenderComponentAsync<WorkOrderInterventionSource>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Order",order},{"DecisionId",id},{"WellId",Well}})));
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await renderer.Dispatcher.InvokeAsync(()=>{if(change=="company")company.Clear();if(change=="role")auth.OtherRole();if(change=="signout")auth.SignOut();if(change=="component")order.Components=[];});hold.SetResult(value);
        var root=await rendering;await renderer.Dispatcher.InvokeAsync(()=>{var html=root.ToHtmlString();if(change=="allowed"){Assert.Contains("original-engineer",html);Assert.Contains("Recorded &lt;receiving scope&gt;",html);Assert.Contains("does not establish task assignment",html);}else Assert.DoesNotContain("original-engineer",html);});
        client.Verify(value=>value.RecordInterventionDecisionAsync(It.IsAny<string>(),It.IsAny<ProductionInterventionDecisionRequest>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
