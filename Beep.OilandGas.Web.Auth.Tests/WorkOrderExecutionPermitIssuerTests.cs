#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components.BusinessProcess;
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
public sealed class WorkOrderExecutionPermitIssuerTests
{
    private sealed class Auth:AuthenticationStateProvider
    {
        private string actor="issuer";
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"ConfiguredRole")],"fixture"))));
        internal void Switch(){actor="another-actor";NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private sealed class Nav:NavigationManager
    {
        internal Nav()=>Initialize("http://fixture/","http://fixture/ppdm39/process/instances/original");
        protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();
    }
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);
    }
    private sealed class Components:IComponentActivator
    {
        internal WorkOrderExecutionPermitIssuer Panel=null!;
        public IComponent CreateInstance(Type type){var value=(IComponent)Activator.CreateInstance(type)!;if(value is WorkOrderExecutionPermitIssuer panel)Panel=panel;return value;}
    }
    private static Task Invoke(object panel,string method)=>(Task)panel.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!;
    private static void Set(object panel,string field,string value)=>panel.GetType().GetField(field,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,value);
    [Theory][InlineData("allowed")][InlineData("unknown")][InlineData("invalid-utc")][InlineData("fresh")][InlineData("field")][InlineData("company")][InlineData("revoked-api")][InlineData("actor")][InlineData("versioned")]
    public async Task ActualIssuerRetainsUrlBeforeTransportAndConfirmsOriginalReadOnly(string fault)
    {
        var review=fault=="versioned"?WorkOrderExecutionPermitStateTests.RequirementReview():WorkOrderExecutionPermitStateTests.Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(review.Context.Company,["ConfiguredRole"],["HSE.IssuePermit"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("field");var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(c=>c.ReviewWorkOrderExecutionPermitIssueAsync(review.Source.Stage.Task,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var nav=new Nav();var writes=0;var recoveries=0;CompanyWorkOrderExecutionPermitIntent? original=fault=="fresh"?WorkOrderExecutionPermitStateTests.Proposal(review) with{IntentId=Guid.NewGuid()}:null;
        client.Setup(c=>c.IssueWorkOrderExecutionPermitAsync(It.IsAny<CompanyWorkOrderExecutionPermitIntent>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitIntent body,CancellationToken _)=>{original=body;writes++;Assert.Equal(review.RequirementEvidenceVersion,body.RequirementEvidenceVersion);body.RequireSource(review.Source);Assert.Contains("permitIssueId="+body.IntentId.ToString("D"),nav.Uri);return fault=="unknown"?Task.FromException<CompanyWorkOrderExecutionPermitReceipt>(new HttpRequestException("Unknown commit")):Task.FromResult(WorkOrderExecutionPermitStateTests.Receipt(body));});
        client.Setup(c=>c.RecoverWorkOrderExecutionPermitReferenceAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitReference query,CancellationToken _)=>{recoveries++;Assert.Equal(original!.IntentId,query.IntentId);Assert.Equal(original.Task,query.Task);return Task.FromResult(new CompanyWorkOrderExecutionPermitRecovery(original,WorkOrderExecutionPermitStateTests.Receipt(original)));});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderExecutionPermitState>();services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider,Auth>();services.AddSingleton<NavigationManager>(nav);services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderExecutionPermitIssuer>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Task",review.Source.Stage.Task},{"SourceRevision","source"},{"OriginalReference",original?.IntentId}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            if(fault=="fresh"){Assert.Equal(1,recoveries);Assert.Equal(0,writes);Assert.Contains(original!.PermitId(),root.ToHtmlString());return;}
            await Invoke(panel,"ReviewAsync");Assert.Contains(review.Source.SnapshotRevision,root.ToHtmlString());if(fault=="versioned"){Assert.Contains("Original inspection requirements",root.ToHtmlString());Assert.Contains("Original native requirement",root.ToHtmlString());Assert.Contains("requirement-author",root.ToHtmlString());}else Assert.Contains("No original inspection requirements",root.ToHtmlString());
            foreach(var pair in new Dictionary<string,string>{{"_type","ElectricalIsolation"},{"_jurisdiction","Reported jurisdiction"},{"_scope","Original scope"},{"_isolation","isolation"},{"_risk","risk"},{"_personnel","personnel"},{"_materials","materials"},{"_from",fault=="invalid-utc"?"2026-10-13T10:00:00":"2026-10-13T10:00:00Z"},{"_to","2026-10-13T18:00:00Z"}})Set(panel,pair.Key,pair.Value);
            await Invoke(panel,"IssueAsync");if(fault=="invalid-utc"){Assert.Equal(0,writes);Assert.DoesNotContain("Recorded pending permit:",root.ToHtmlString());return;}Assert.Equal(1,writes);
            if(fault=="unknown"){Assert.Contains("unconfirmed",root.ToHtmlString());await Invoke(panel,"IssueAsync");Assert.Equal(1,writes);await Invoke(panel,"RecoverAsync");Assert.Equal(1,recoveries);}
            Assert.Contains(original!.PermitId(),root.ToHtmlString());
            Assert.Contains("permitReviewId="+original.IntentId.ToString("D"),root.ToHtmlString());Assert.Contains("Independent review link",root.ToHtmlString());
            Assert.Contains("Original scope",root.ToHtmlString());Assert.Contains("2026-10-13T10:00:00Z",root.ToHtmlString());
            if(fault=="revoked-api")
            {
                client.Setup(c=>c.RecoverWorkOrderExecutionPermitReferenceAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).ThrowsAsync(new OilGasApiException(System.Net.HttpStatusCode.Forbidden,"Authority revoked",null));await Invoke(panel,"RecoverAsync");Assert.DoesNotContain(original.PermitId(),root.ToHtmlString());Assert.DoesNotContain("Original scope",root.ToHtmlString());
            }
            if(fault=="field"){fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("foreign");fields.Raise(f=>f.CurrentFieldChanged+=null!,"foreign");}if(fault=="company")company.Clear();
            if(fault=="actor"){((Auth)provider.GetRequiredService<AuthenticationStateProvider>()).Switch();Set(panel,"_reference",original.IntentId.ToString("D"));await Invoke(panel,"RecoverAsync");Assert.Equal(0,recoveries);}
            if(fault is "field" or "company" or "actor"){await Task.Yield();Assert.DoesNotContain(original.PermitId(),root.ToHtmlString());Assert.DoesNotContain("Original scope",root.ToHtmlString());}
            Assert.Equal(1,writes);
        });
    }
}
#endif
