using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyFxRateScreenTests
{
    private sealed class Authentication:AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"administrator"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));}
    private sealed class Components:IComponentActivator{internal Beep.OilandGas.Web.Components.Workflow.CompanyFxWorkbench? Workbench;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.CompanyFxWorkbench workbench)Workbench=workbench;return component;}}
    [Fact] public async Task ConversionEvidenceShowsOnlyTheMatchingReviewedCompanyTimeZone()
    {
        var f=new CompanyFxRateTests.Fixture();await f.Prepare();await f.Record();var conversion=CompanyFxConversion.Create(await f.Read(),2,10,8,1,1);
        using var company=CompanyGLAccountHttpTests.Selected(actor:"administrator",grants:["Accounting.View"]);
        var scope=company.Selection!.Scope;
        var settings=new CompanyEnvironmentPreview(scope,"KW","Kuwait","ar-KW","metric","m","bar","mm",[],[],[]){TimeZoneId="Asia/Kuwait",DisplayCulture="en-GB",DateTimeFormat="dd/MM/yyyy HH:mm",Calendar="Gregorian"};
        var effective=new CompanyEnvironmentEffectiveReview(scope,new string('A',64),new string('B',64),null,new string('C',64),settings){ReviewStatus="Current"};
        var display=new CompanyEnvironmentDisplay(company.Version,effective);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddLogging();ui.Services.AddMudServices();ui.Services.AddSingleton(company);
        var panel=ui.RenderComponent<Beep.OilandGas.Web.Components.Workflow.CompanyFxEvidencePanel>(p=>p.Add(x=>x.Conversion,conversion).AddCascadingValue(display));
        Assert.Contains("Observed UTC",panel.Markup);Assert.Contains("12/10/2026 03:00 +03:00 (Asia/Kuwait)",panel.Markup);
        company.Clear();await panel.InvokeAsync(()=>panel.Render());Assert.DoesNotContain("Observed in company time zone",panel.Markup);Assert.Contains("Observed UTC",panel.Markup);
        Assert.True(company.TrySelect(company.Version,"administrator",new(new("tenant","other"),["Reader"],["Accounting.View"])));
        var other=new CompanyEnvironmentDisplay(company.Version,effective with{Company=company.Selection!.Scope,Settings=settings with{Company=company.Selection.Scope}});
        var foreign=ui.RenderComponent<Beep.OilandGas.Web.Components.Workflow.CompanyFxEvidencePanel>(p=>p.Add(x=>x.Conversion,conversion).AddCascadingValue(other));
        Assert.DoesNotContain("Observed in company time zone",foreign.Markup);
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task RateFormRequiresQuoteEvidenceRetainsExactNativeCommandAndAllowsReadOnlyRecovery(bool ranged)
    {
        var f=new CompanyFxRateTests.Fixture();await f.Prepare();if(ranged)CompanyFxRateWindowTests.Seed(f,150);var end=ranged?f.Date.AddDays(2):f.Date;using var company=CompanyGLAccountHttpTests.Selected(actor:"administrator",grants:["Accounting.View","Accounting.ConfigureLedger"]);using var state=new CompanyLedgerAccountState(company);state.AttachOwner(new object());var client=new Mock<IAccountingServiceClient>();var lost=true;var bodies=new List<string>();
        client.Setup(value=>value.GetCompanyFxRateConfigurationReviewAsync("statutory","USD",f.Date,end)).Returns(async()=>new CompanyFxRateConfigurationReview(await f.Service.GetCompanyFxRateWindowAsync(CompanyLedgerBookTests.Company,"administrator","statutory","USD",f.Date,end,f.Books.Reader.Authority),new string('B',64)));
        client.Setup(value=>value.ConfigureCompanyFxRateAsync(It.IsAny<CompanyFxRateConfigurationSubmission>())).Returns((CompanyFxRateConfigurationSubmission submission)=>{bodies.Add(JsonSerializer.Serialize(submission.Command));f.Submission=submission;return f.Record();});
        client.Setup(value=>value.GetCompanyFxRatesAsync("statutory","USD",f.Date)).Returns(()=>lost?Task.FromException<CompanyFxRateReview>(new InvalidOperationException("Lost followup")):f.Read());
        client.Setup(value=>value.GetCompanyFxRateConfigurationAsync(It.IsAny<string>())).Returns((string id)=>f.Service.GetCompanyFxRateConfigurationAsync(CompanyLedgerBookTests.Company,"administrator",id,f.Books.Reader.Authority));
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton(company);services.AddSingleton<IAccountingServiceClient>(client.Object);services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IComponentActivator,Components>();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state},{"Book","statutory"}});var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.CompanyFxWorkbench>(parameters);var components=(Components)provider.GetRequiredService<IComponentActivator>();
            void Field(string name,object? value)=>components.Workbench!.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(components.Workbench,value);
            void Render()=>components.Workbench!.GetType().GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null);
            async Task Action(string name){await(Task)components.Workbench!.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null)!;Render();}
            Field("_source","USD");Field("_date",f.Date);Field("_reviewThrough",end);Render();await Action("Review");Assert.NotNull(state.FxConfigurationReview);components.Workbench!.GetType().GetMethod("ThroughChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,[end.AddDays(1)]);Assert.Null(state.FxConfigurationReview);Field("_reviewThrough",end);Render();await Action("Review");await Action("Record");Assert.Equal(0,f.Books.Reader.Native.Writes);Assert.False(state.HasPendingFx);
            var c=f.Submission.Command;Field("_version",c.Version);Field("_rate",c.Rate);Field("_precision",c.RateDecimals);Field("_direction",c.Direction);Field("_quote",c.QuoteDate);Field("_observed",c.ObservedAtUtc.ToString("O").Replace("Z","+00:00"));Field("_provider",c.Provider);Field("_provenance",c.Provenance);Field("_authority",c.AuthorityReference);Field("_allocation",c.AllocationPolicy);Field("_reason",c.Reason);Render();await Action("Record");Assert.True(state.HasPendingFx);Assert.Equal(1,f.Books.Reader.Native.Writes);var original=JsonSerializer.Serialize(state.PendingFx!.Command);Assert.Contains("exact original FX request is retained",root.ToHtmlString());
            components.Workbench!.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.CompanyFxWorkbench>(parameters);Assert.Equal(original,JsonSerializer.Serialize(state.PendingFx!.Command));lost=false;await Action("Retry");Assert.False(state.HasPendingFx);Assert.Equal(bodies[0],bodies[1]);Assert.Equal(1,f.Books.Reader.Native.Writes);Assert.Equal(end,state.FxResult!.EFFECTIVE_THROUGH);Assert.Contains("company FX rate is confirmed",root.ToHtmlString());
            Assert.True(company.TrySelect(company.Clear(),"administrator",new(CompanyLedgerBookTests.Company,["ConfiguredCompanyRole"],["Accounting.View"])));Field("_recoveryId",f.Submission.Command.Id);Render();await Action("RecoverSelected");Assert.NotNull(state.FxResult);Field("_source","USD");Field("_date",f.Date);Render();await Action("ReadPublic");Assert.Single(state.FxRates!.Rates);Assert.DoesNotContain("Review FX configuration authority",root.ToHtmlString());Assert.Equal(1,f.Books.Reader.Native.Writes);
            var conversion=CompanyFxConversion.Create(state.FxRates!,2,10,8,1,1);var evidence=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.CompanyFxEvidencePanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Conversion",conversion}}));Assert.Contains("Recorded functional currency conversion",evidence.ToHtmlString());Assert.Contains(c.Provenance,evidence.ToHtmlString());Assert.Contains(c.AuthorityReference,evidence.ToHtmlString());Assert.Contains("Functional",evidence.ToHtmlString());
        });
    }
}
#endif
