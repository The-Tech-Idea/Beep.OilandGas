using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalSourceHttpTests
{
    internal static readonly string[] Grants=["Accounting.View","Accounting.ReviewSalesInvoice","Accounting.ReviewSalesReceivable","Accounting.ReviewSalesJournalSource"];
    private sealed class Authentication:AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"controller"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));}
    private sealed class Components:IComponentActivator
    {public Beep.OilandGas.Web.Components.Workflow.SalesReceivableReviewWorkbench? Workbench;public IComponent CreateInstance(Type type){var value=(IComponent)Activator.CreateInstance(type)!;if(value is Beep.OilandGas.Web.Components.Workflow.SalesReceivableReviewWorkbench workbench)Workbench=workbench;return value;}}
    internal static async Task<AccountingSourceHttpTests.Host> Start(CompanySalesJournalSourceTests.Fixture fixture)
    {
        var host=await fixture.Receivable.Data.Invoice.Data.Start(permissions:Grants,configure:(services,context)=>{
            context.Actor=fixture.Current.ActorUserId;services.AddSingleton<IARService>(fixture.Receivable.Data.Owner);services.AddScoped<CompanySalesReceivableReads>();services.AddSingleton(fixture.Journal);services.AddSingleton(fixture.Mappings);services.AddSingleton(fixture.Periods);});
        host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",fixture.Current.ActorUserId);return host;
    }
    [Theory][InlineData("anonymous")][InlineData("role")][InlineData("view")][InlineData("source-grant")]
    public async Task MissingStandardRoleOrPurposeGrantCannotReadNativeJournalSource(string fault)
    {
        var native=new CompanyLedgerPeriodReadTests.Fixture();var columns=new CommonColumnHandler();var defaults=Mock.Of<IPPDM39DefaultsRepository>();var metadata=Mock.Of<IPPDMMetadataRepository>();
        var journal=new JournalEntryService(native.Native.Native.Editor.Object,columns,defaults,metadata,native.Native.Service,NullLogger<JournalEntryService>.Instance,()=>Task.FromResult(native.Native.Native.Connection));
        var mappings=new AccountMappingService(native.Native.Native.Editor.Object,columns,defaults,metadata,NullLogger<AccountMappingService>.Instance,()=>Task.FromResult(native.Native.Native.Connection),native.Native.Service);
        await using var host=await AccountingSourceHttpTests.Start(authenticate:fault!="anonymous",role:fault!="role",permissions:fault=="view"?["Accounting.ReviewSalesJournalSource"]:fault=="source-grant"?["Accounting.View"]:Grants,
            configureAccounting:(services,context)=>{context.PolicyBinding=new string('B',64);services.AddSingleton(journal);services.AddSingleton(mappings);services.AddSingleton(native.Service);services.AddSingleton(new CompanySalesReceivableReads(Mock.Of<IARService>(),Mock.Of<IInvoiceService>(),null!,context,Mock.Of<IAccessControlService>()));});
        using var company=CompanyGLAccountHttpTests.Selected(grants:Grants);var error=await Assert.ThrowsAsync<OilGasApiException>(()=>CompanyGLAccountHttpTests.Client(host.Client,company).GetCompanySalesJournalSourceAsync(Guid.NewGuid().ToString("N"),"statutory","KWD","company-calendar",new DateTime(2026,1,1),new("company-ar","company-sales",null,null)));
        Assert.Equal(fault=="anonymous"?HttpStatusCode.Unauthorized:HttpStatusCode.Forbidden,error.StatusCode);Assert.Equal(0,native.Native.Reads);native.Native.NoWrites();
    }
    [Fact] public async Task ActualApiAndRenderedAccountantFormShowExactApprovedPurposeLinesWithoutPrivateCommandHistories()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new CompanySalesJournalSourceTests.Fixture();await fixture.Prepare(taxAndCharges:true);await using var host=await Start(fixture);
        using var company=CompanyGLAccountHttpTests.Selected(actor:fixture.Current.ActorUserId,grants:Grants);var client=CompanyGLAccountHttpTests.Client(host.Client,company);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();
        services.AddSingleton(company);services.AddScoped<CompanySalesReceivableWorkspaceState>();services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();var state=provider.GetRequiredService<CompanySalesReceivableWorkspaceState>();state.AttachOwner(new object());await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.SalesReceivableReviewWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"ReceivableId",fixture.Current.ReceivableId}}));var component=((Components)provider.GetRequiredService<IComponentActivator>()).Workbench!;
            void Invoke(string name,params object?[] values)=>component.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(component,values);
            async Task Action(){await (Task)component.GetType().GetMethod("LoadJournalSource",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(component,null)!;Invoke("StateHasChanged");}
            await Action();Assert.Null(state.JournalSource);fixture.NoWrites();
            Invoke("JournalSelectionChanged","book","statutory");Invoke("JournalSelectionChanged","currency","KWD");Invoke("JournalSelectionChanged","calendar","company-calendar");Invoke("JournalDateChanged",fixture.Date);
            Invoke("JournalSelectionChanged","receivable",fixture.Selection.ReceivableKey);Invoke("JournalSelectionChanged","revenue",fixture.Selection.RevenueKey);Invoke("JournalSelectionChanged","charges",fixture.Selection.ChargesKey);Invoke("JournalSelectionChanged","tax",fixture.Selection.TaxKey);Invoke("StateHasChanged");await Action();
            var source=state.JournalSource;Assert.NotNull(source);Assert.Equal(4,source.Lines.Count);var json=JsonSerializer.Serialize(source);Assert.DoesNotContain("COMMAND_JSON",json);Assert.DoesNotContain("WORKFLOW_JSON",json);Assert.DoesNotContain("AUTHORITY_JSON",json);
            Assert.Contains("SALES_TAX",root.ToHtmlString());Assert.Contains("Native ID tax-account",root.ToHtmlString());Assert.Contains("creates no journal",root.ToHtmlString());
            Invoke("JournalSelectionChanged","book","changed");Assert.Null(state.JournalSource);fixture.NoWrites();
        });
    }
    [Theory][InlineData("grant")][InlineData("binding")][InlineData("client-account")]
    public async Task ClosingServerAuthorityOrClientAccountChangeCannotPublishJournalSource(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new CompanySalesJournalSourceTests.Fixture();await fixture.Prepare();await using var host=await Start(fixture);using var company=CompanyGLAccountHttpTests.Selected(actor:fixture.Current.ActorUserId,grants:Grants);
        var changed=false;fixture.OnRead=table=>{if(changed)return;changed=true;if(fault=="grant")host.Context.Permissions=["Accounting.View"];if(fault=="binding")host.Context.PolicyBinding=new string('C',64);if(fault=="client-account")company.TrySelect(company.Clear(),"other",new(fixture.Current.Company,["ConfiguredCompanyRole"],Grants));};
        var client=CompanyGLAccountHttpTests.Client(host.Client,company);
        if(fault=="client-account"){await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>client.GetCompanySalesJournalSourceAsync(fixture.Current.ReceivableId,"statutory","KWD","company-calendar",fixture.Date,fixture.Selection));Assert.Equal("other",company.Selection!.UserId);}
        else{var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanySalesJournalSourceAsync(fixture.Current.ReceivableId,"statutory","KWD","company-calendar",fixture.Date,fixture.Selection));Assert.Equal(HttpStatusCode.Forbidden,error.StatusCode);}
        Assert.True(changed);fixture.NoWrites();
    }
}
#endif
