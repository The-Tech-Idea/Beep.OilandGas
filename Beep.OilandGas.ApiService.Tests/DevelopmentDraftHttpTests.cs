#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.ApiService.Middleware;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.DevelopmentPlanning.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentDraftHttpTests
{
    private sealed class Authentication(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder,DevelopmentDraftAuthorityTests.Fixture fixture)
        :AuthenticationHandler<AuthenticationSchemeOptions>(options,logger,encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()=>Task.FromResult(Request.Headers["X-Test-Auth"]=="yes"?
            AuthenticateResult.Success(new AuthenticationTicket(fixture.User,Scheme.Name)):AuthenticateResult.NoResult());
    }
    internal sealed class Host(WebApplication app,HttpClient http) : IAsyncDisposable
    {
        internal HttpClient Http=>http;
        public async ValueTask DisposeAsync(){http.Dispose();await app.DisposeAsync();}
    }
    internal static async Task<Host> Start(DevelopmentDraftAuthorityTests.Fixture fixture,bool authenticate=true,DevelopmentReviewCatalogService? catalog=null,DevelopmentReviewHandoffService? handoffs=null,
        Func<Microsoft.AspNetCore.Http.IHttpContextAccessor,IDevelopmentReviewProgressReader>? progress=null)
    {
        var builder=WebApplication.CreateBuilder(new WebApplicationOptions{EnvironmentName="Production"});builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");builder.Services.AddSingleton(fixture);
        builder.Services.AddAuthentication("fixture").AddScheme<AuthenticationSchemeOptions,Authentication>("fixture",_=>{});builder.Services.AddAuthorization();
        builder.Services.AddRefusalAnswers();builder.Services.AddControllers().AddApplicationPart(typeof(DevelopmentPlanningController).Assembly);
        if(catalog is not null)builder.Services.AddSingleton(catalog);
        if(handoffs is not null)builder.Services.AddSingleton(handoffs);
        if(progress is not null)
        {
            builder.Services.AddHttpContextAccessor();builder.Services.AddScoped<IDevelopmentReviewProgressReader>(provider=>progress(provider.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));
            builder.Services.AddSingleton(Mock.Of<IFailureReporter>());
        }
        builder.Services.AddSingleton<IDevelopmentPlanService>(fixture.Drafts.Service);builder.Services.AddSingleton<IDevelopmentPlanRequestAuthority>(fixture.Authority);
        var app=builder.Build();app.UseExceptionHandler();app.UseAuthentication();
        app.Use(async(context,next)=>{fixture.Http.HttpContext=context;await next(context);});app.UseAuthorization();app.MapControllers();await app.StartAsync();
        var http=new HttpClient{BaseAddress=new(app.Urls.Single())};if(authenticate)http.DefaultRequestHeaders.Add("X-Test-Auth","yes");return new(app,http);
    }
    private static CompanyClientContext Selected()
    {
        var company=new CompanyClientContext();company.TrySelect(company.Clear(),"planner",new(new("tenant","company"),["ConfiguredPlanner"],["DevelopmentPlanning.View","DevelopmentPlanning.Create"]));return company;
    }
    [Fact] public async Task ActualTypedClientControllerAuthorityAndNativeDraftCompleteReviewSaveListRecovery()
    {
        using var fixture=new DevelopmentDraftAuthorityTests.Fixture();await using var host=await Start(fixture);using var company=Selected();
        var client=new DevelopmentServiceClient(new ApiClient(host.Http,Mock.Of<IFailureReporter>(),company),NullLogger<DevelopmentServiceClient>.Instance);
        var options=await client.GetDraftOptionsAsync();Assert.Equal(new[]{"KWD"},options.Currencies);Assert.Empty((await client.GetDraftPlansAsync()).Plans);
        var saved=await client.SaveDraftPlanAsync(fixture.Drafts.Request);DevelopmentPlanDraftReview.RequireRecorded(fixture.Drafts.Request,saved);
        Assert.Single((await client.GetDraftPlansAsync()).Plans);
        var detail=await client.GetDraftPlanAsync(saved.FDP_ID);detail.Require(fixture.Context,saved.FDP_ID);Assert.Equal(saved.FDP_ID,detail.Plan!.FDP_ID);
        Assert.Null((await client.GetDraftPlanAsync("missing")).Plan);Assert.NotNull((await client.RecoverDraftPlanAsync(fixture.Drafts.Request)).Plan);Assert.Equal(1,fixture.Drafts.Native.Writes);
        Assert.Equal(saved.FDP_ID,(await client.SaveDraftPlanAsync(fixture.Drafts.Request)).FDP_ID);Assert.Equal(1,fixture.Drafts.Native.Writes);
    }
    [Theory][InlineData("anonymous")][InlineData("role")][InlineData("permission")][InlineData("binding")][InlineData("missing-intent")][InlineData("timestamp")]
    public async Task HttpRejectsUnqualifiedCompanySavesBeforeBusinessWrite(string fault)
    {
        using var fixture=new DevelopmentDraftAuthorityTests.Fixture();
        if(fault=="role")fixture.Roles=[];if(fault=="permission")fixture.Permissions=[];
        var input=fixture.Drafts.Request;if(fault=="binding")input=input with{PlanningDatabase=new("planning","changed")};
        var json=System.Text.Json.JsonSerializer.SerializeToNode(input)!.AsObject();if(fault=="missing-intent")json.Remove("IntentId");if(fault=="timestamp")json["PlannedFirstProductionDate"]="2030-04-01T20:00:00Z";
        await using var host=await Start(fixture,fault!="anonymous");var result=await host.Http.PostAsJsonAsync("/api/field/current/development-planning/plans",json);
        Assert.Equal(fault switch{"anonymous"=>HttpStatusCode.Unauthorized,"role" or "permission"=>HttpStatusCode.Forbidden,"binding"=>HttpStatusCode.Conflict,_=>HttpStatusCode.BadRequest},result.StatusCode);
        Assert.Equal(0,fixture.Drafts.Native.Writes);Assert.Empty(fixture.Drafts.Native.Transactions);
    }
    [Theory][InlineData("well-activities")][InlineData("maintenance")][InlineData("service-jobs")]
    public async Task RetiredUnqualifiedRoutesHaveNoControllerAction(string retired)
    {
        using var fixture=new DevelopmentDraftAuthorityTests.Fixture();await using var host=await Start(fixture);
        Assert.Equal(HttpStatusCode.NotFound,(await host.Http.GetAsync("/api/field/current/development-planning/"+retired)).StatusCode);Assert.Equal(0,fixture.Drafts.Native.Writes);
    }
}
#endif
