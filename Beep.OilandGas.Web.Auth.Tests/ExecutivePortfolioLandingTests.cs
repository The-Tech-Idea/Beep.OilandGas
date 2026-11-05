using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages.PPDM39;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class ExecutivePortfolioLandingTests
{
    private static CompanyClientContext Company(bool roles=true)
    {var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"manager",new(new("tenant","company"),roles?["PortfolioRole"]:[],[])));return company;}
    private static ExecutivePortfolioReview Review()=>new(new("tenant","company"),"manager","FIELD",DateTimeOffset.UtcNow,[new("field","Recorded executive field")]);
    private static HttpResponseMessage Response(ExecutivePortfolioReview? review)=>new(HttpStatusCode.OK){Content=JsonContent.Create(review)};
    private sealed class Transport:HttpMessageHandler
    {
        internal int Reads;
        internal Func<Task<HttpResponseMessage>> Next=()=>Task.FromResult(Response(Review()));
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {Reads++;Assert.Equal(HttpMethod.Get,request.Method);Assert.Equal("/api/executive/portfolio",request.RequestUri!.AbsolutePath);return Next();}
    }
    [Theory]
    [InlineData("current")][InlineData("empty")][InlineData("company")][InlineData("actor")][InlineData("source")][InlineData("duplicate")]
    [InlineData("null")][InlineData("nullrows")][InlineData("negative")][InlineData("no-company")][InlineData("no-role")]
    [InlineData("late-company")][InlineData("late-account")][InlineData("forbidden")][InlineData("unavailable")]
    public async Task ActualTypedClientRequiresCurrentAppScopeAndValidatesItsCanonicalPortfolioEnvelope(string mode)
    {
        using var company=Company(mode!="no-role");if(mode=="no-company")company.Clear();var transport=new Transport();
        transport.Next=()=>
        {
            if(mode=="late-company")company.Clear();
            if(mode=="late-account")Assert.True(company.TrySelect(company.Clear(),"other",new(new("tenant","company"),["PortfolioRole"],[])));
            if(mode=="unavailable")throw new HttpRequestException("Source unavailable");
            if(mode=="forbidden")return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden){Content=JsonContent.Create(new{error="Source denied"})});
            var review=Review();review=mode switch{"empty"=>review with{Assets=[]},"company"=>review with{Company=new("tenant","other")},"actor"=>review with{Actor="other"},"source"=>review with{Source="UNCONFIRMED"},"duplicate"=>review with{Assets=[review.Assets[0],review.Assets[0]]},"null"=>null!,"nullrows"=>review with{Assets=null!},"negative"=>review with{Assets=[new("field","Invalid count",null,-1)]},_=>review};return Task.FromResult(Response(review));
        };
        using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};var client=new ExecutiveServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company));
        var error=await Record.ExceptionAsync(()=>client.GetPortfolioAsync());if(mode is "current" or "empty")Assert.Null(error);else Assert.NotNull(error);
        Assert.Equal(mode is "no-company" or "no-role"?0:1,transport.Reads);
    }
    private sealed class Auth:AuthenticationStateProvider
    {
        internal ClaimsPrincipal User=Principal();internal Task<AuthenticationState>? Pending;
        internal static ClaimsPrincipal Principal(string actor="manager",string role="PortfolioRole",bool authenticated=true)=>new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,role)],authenticated?"fixture":null));
        public override Task<AuthenticationState> GetAuthenticationStateAsync(){if(Pending is {} pending){Pending=null;return pending;}return Task.FromResult(new AuthenticationState(User));}
        internal void Emit()=>NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(User)));
    }
    private sealed class Components:IComponentActivator
    {internal ExecutiveDashboard Page=null!;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is ExecutiveDashboard page)Page=page;return component;}}
    private sealed class NoBrowser:IJSRuntime
    {public ValueTask<T> InvokeAsync<T>(string name,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string name,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory]
    [InlineData("current")][InlineData("asset-manager")][InlineData("empty")][InlineData("failed")][InlineData("foreign-company")][InlineData("foreign-account")]
    [InlineData("logout")][InlineData("no-role")][InlineData("wrong-role")][InlineData("wrong-account")]
    [InlineData("company-change")][InlineData("auth-change")][InlineData("late-company")][InlineData("late-auth")][InlineData("late-auth-failure")][InlineData("late-authentication")][InlineData("newer-load")]
    public async Task ActualExecutiveLandingAndInboxClearAccountRoleAndLateEvidenceWithoutInventingAnEmptySuccess(string mode)
    {
        using var company=Company(mode!="no-role");var auth=new Auth();if(mode=="logout")auth.User=Auth.Principal(authenticated:false);if(mode=="wrong-role")auth.User=Auth.Principal(role:"OtherRole");if(mode=="wrong-account")auth.User=Auth.Principal(actor:"other");
        var pendingAuth=new TaskCompletionSource<AuthenticationState>(TaskCreationOptions.RunContinuationsAsynchronously);if(mode=="late-authentication")auth.Pending=pendingAuth.Task;
        var transport=new Transport();transport.Next=()=>
        {
            if(mode=="failed")throw new HttpRequestException("Portfolio unavailable");
            var review=Review();if(mode=="empty")review=review with{Assets=[]};if(mode=="foreign-company")review=review with{Company=new("tenant","other")};if(mode=="foreign-account")review=review with{Actor="other"};return Task.FromResult(Response(review));
        };
        var persona=new AppPersona{Code=mode=="asset-manager"?"ASSET_MANAGER":"EXECUTIVE",Name=mode=="asset-manager"?"Asset manager":"Executive",DefaultRoute=mode=="asset-manager"?"/lifecycle/asset/dashboard":"/executive/dashboard"};Assert.Contains(typeof(ExecutiveDashboard).GetCustomAttributes(typeof(RouteAttribute),true).Cast<RouteAttribute>(),route=>route.Template==persona.DefaultRoute);var personas=new Mock<IPersonaContextService>();personas.SetupGet(value=>value.CurrentPersona).Returns(persona);personas.SetupGet(value=>value.CurrentProfile).Returns(new AppUserPersona{UserId="manager",PersonaCode=persona.Code});personas.SetupGet(value=>value.Catalog).Returns([persona]);personas.Setup(value=>value.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var inbox=new Mock<IUnifiedTaskInboxService>();inbox.Setup(value=>value.GetInboxAsync(persona.Code)).ReturnsAsync(new UnifiedInbox{Company=new("tenant","company"),ActorUserId="manager",PersonaCode=persona.Code});
        using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton<IExecutiveServiceClient>(new ExecutiveServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company)));services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton(personas.Object);services.AddSingleton(inbox.Object);services.AddSingleton<IComponentActivator,Components>();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var render=renderer.RenderComponentAsync<ExecutiveDashboard>();var page=((Components)provider.GetRequiredService<IComponentActivator>()).Page;
            if(mode=="late-authentication"){auth.User=Auth.Principal(authenticated:false);auth.Emit();pendingAuth.SetResult(new AuthenticationState(Auth.Principal()));}
            var root=await render;
            void Refresh()=>typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null);
            Task Load()=>(Task)typeof(ExecutiveDashboard).GetMethod("LoadAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null)!;
            var state=(ExecutivePortfolioState)typeof(ExecutiveDashboard).GetField("_state",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(page)!;
            if(state.Current?.Assets.Count>0){state.Select("field");Refresh();Assert.Contains("Recorded FIELD field",root.ToHtmlString());}
            if(mode is "company-change" or "auth-change"){if(mode=="company-change")company.Clear();else{auth.User=Auth.Principal(authenticated:false);auth.Emit();}await Task.Yield();}
            if(mode.StartsWith("late-",StringComparison.Ordinal) && mode!="late-authentication" || mode=="newer-load")
            {
                Assert.Contains("Recorded executive field",root.ToHtmlString());var pending=new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);transport.Next=()=>pending.Task;var load=Load();
                if(mode=="late-company")company.Clear();else{auth.User=mode=="newer-load"?Auth.Principal():Auth.Principal(authenticated:false);auth.Emit();}
                if(mode=="newer-load"){transport.Next=()=>Task.FromResult(Response(Review() with{Assets=[new("fresh-field","Fresh recorded field")]}));await Load();}
                if(mode=="late-auth-failure")pending.SetException(new HttpRequestException("Old source unavailable"));else pending.SetResult(Response(Review()));await load;Refresh();await Task.Yield();
            }
            var html=System.Net.WebUtility.HtmlDecode(root.ToHtmlString());
            if(mode is "current" or "asset-manager"){Assert.Contains("Recorded executive field",html);Assert.Contains("1 authorized FIELD records returned",html);Assert.Contains("Assigned work",html);Assert.DoesNotContain("mapped to another landing page",html);}
            else if(mode=="empty"){Assert.Contains("No authorized FIELD records were returned",html);Assert.Contains("0 authorized FIELD records returned",html);}
            else if(mode=="newer-load"){Assert.Contains("Fresh recorded field",html);Assert.DoesNotContain("Recorded executive field",html);}
            else{Assert.DoesNotContain("Recorded executive field",html);Assert.DoesNotContain("No authorized FIELD records were returned",html);Assert.DoesNotContain("0 authorized FIELD records returned",html);}
            if(mode is "company-change" or "auth-change" or "late-company" or "late-auth" or "late-auth-failure" or "late-authentication" or "newer-load")Assert.Null(state.Selected);
            if(mode is "logout" or "no-role" or "wrong-role" or "wrong-account" or "late-authentication"){Assert.Equal(0,transport.Reads);inbox.Verify(value=>value.GetInboxAsync(It.IsAny<string?>()),Times.Never);}
            if(mode is "company-change" or "auth-change" or "late-company" or "late-auth" or "late-auth-failure" or "late-authentication")Assert.DoesNotContain("Assigned work",html);
            if(mode=="late-auth-failure")Assert.DoesNotContain("Executive evidence could not be confirmed",html);
            Assert.Contains("KPIs are unavailable",html);Assert.DoesNotContain("Approved performance total: 0",html);
        });
    }
}
