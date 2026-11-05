#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
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
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyLedgerAccountScreenTests
{
    private sealed class Authentication:AuthenticationStateProvider
    {
        public string Actor="actor";public bool Role=true;
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(
            new[]{new Claim(ClaimTypes.NameIdentifier,Actor)}.Concat(Role?[new Claim(ClaimTypes.Role,"ConfiguredCompanyRole")]:[]),"test"))));
        public void Change(){Actor="other";NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private sealed class Components:IComponentActivator
    {
        public Beep.OilandGas.Web.Components.Workflow.CompanyLedgerAccounts? Workbench;
        public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.CompanyLedgerAccounts workbench)Workbench=workbench;return component;}
    }
    [Theory][InlineData("valid")][InlineData("empty")][InlineData("reason")][InlineData("overlap")][InlineData("date")][InlineData("grant")][InlineData("uncertain")][InlineData("recreated")][InlineData("recovery")][InlineData("auth-change")]
    public async Task CompanyPeriodFormUsesRecordedCalendarAndRetainsExactOriginalRequest(string mode)
    {
        var native=new CompanyLedgerPeriodCreationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Reader.Service);services.AddSingleton(native.Reader.Native.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var authentication=new Authentication();
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);services.AddScoped<CompanyLedgerAccountState>();
        services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider>(authentication);services.AddSingleton<IAccountingServiceClient>(CompanyGLAccountHttpTests.Client(host.Client,company));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var state=provider.GetRequiredService<CompanyLedgerAccountState>();
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.LedgerAccounts>();var components=(Components)provider.GetRequiredService<IComponentActivator>();
            void Invoke(string method,params object?[] values)=>components.Workbench!.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,values);
            async Task Action(string method){await (Task)components.Workbench!.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null)!;Invoke("StateHasChanged");}
            void Field(string name,object? value)=>components.Workbench!.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(components.Workbench,value);
            Invoke("BookChanged","statutory");Invoke("CurrencyChanged","KWD");Invoke("CalendarChanged","company-gregorian");Invoke("StateHasChanged");await Action("ReloadPeriods");Assert.Single(state.Periods!.Periods);Assert.Contains("FY26-M10",root.ToHtmlString());
            await Action("ReviewPeriodConfiguration");Assert.NotNull(state.PeriodConfigurationReview);
            if(mode!="empty"){Field("_periodCode","FY26-M11");Field("_periodReason",mode=="reason"?"":"Reviewed company calendar period");Field("_periodFrom",mode=="overlap"?new DateTime(2026,10,31):mode=="date"?new DateTime(2026,11,1,1,0,0):new DateTime(2026,11,1));Field("_periodThrough",new DateTime(2026,11,30));}
            if(mode=="grant")host.Context.Permissions=["Accounting.View"];if(mode is "uncertain" or "recreated" or "auth-change")native.Reader.Native.Native.FailCommit=true;Invoke("StateHasChanged");await Action("CreatePeriod");
            if(mode is "uncertain" or "recreated" or "auth-change")
            {
                Assert.True(state.HasPendingPeriod);Assert.Contains("original period request is retained",root.ToHtmlString());var saved=System.Text.Json.JsonSerializer.Serialize(state.PendingPeriod);
                await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadConfigurationReviewAsync("statutory","KWD",provider.GetRequiredService<IAccountingServiceClient>()));
                if(mode=="auth-change"){authentication.Change();await Action("ReconcilePeriod");Assert.True(state.HasPendingPeriod);Assert.Null(state.PeriodResult);Assert.Equal(1,native.Reader.Native.Native.Writes);return;}
                if(mode=="recreated"){components.Workbench!.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.LedgerAccounts>();}
                Assert.Equal(saved,System.Text.Json.JsonSerializer.Serialize(state.PendingPeriod));native.Reader.Native.Native.FailCommit=false;await Action("ReconcilePeriod");
            }
            if(mode is "valid" or "uncertain" or "recreated" or "recovery")
            {
                Assert.False(state.HasPendingPeriod);Assert.NotNull(state.PeriodResult);Assert.Equal(1,native.Reader.Native.Native.Writes);Assert.Equal(2,state.Periods!.Periods.Count);Assert.Contains("recorded and confirmed in complete company/book/calendar history",root.ToHtmlString());
                if(mode=="recovery"){var id=state.PeriodResult!.COMPANY_LEDGER_PERIOD_ID;Invoke("BookChanged","statutory");Invoke("CurrencyChanged","KWD");Invoke("CalendarChanged","company-gregorian");Field("_periodRecoveryId",id);Invoke("StateHasChanged");await Action("RecoverPeriod");Assert.True(state.HasPendingPeriod);await Action("ReconcilePeriod");Assert.False(state.HasPendingPeriod);Assert.Equal(1,native.Reader.Native.Native.Writes);}
            }
            else{Assert.Equal(0,native.Reader.Native.Native.Writes);Assert.Null(state.PeriodResult);}
        });
    }
    [Theory][InlineData("valid")][InlineData("empty")][InlineData("reason")][InlineData("account")][InlineData("expiry")][InlineData("overlap")][InlineData("date")][InlineData("grant")][InlineData("uncertain")][InlineData("recreated")][InlineData("recovery")][InlineData("auth-change")]
    public async Task AdministratorMappingFormUsesReviewedAccountDatesAndExactOriginalRequest(string mode)
    {
        var native=new CompanyGLMappingConfigurationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Reader.Service);services.AddSingleton(native.Reader.Accounts.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var authentication=new Authentication();
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);services.AddScoped<CompanyLedgerAccountState>();
        services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider>(authentication);services.AddSingleton<IAccountingServiceClient>(CompanyGLAccountHttpTests.Client(host.Client,company));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var state=provider.GetRequiredService<CompanyLedgerAccountState>();
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.LedgerAccounts>();var components=(Components)provider.GetRequiredService<IComponentActivator>();
            void Invoke(string method,params object?[] values)=>components.Workbench!.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,values);
            void Refresh()=>Invoke("StateHasChanged");
            async Task Action(string method){await (Task)components.Workbench!.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null)!;Refresh();}
            void Field(string name,object? value)=>components.Workbench!.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(components.Workbench,value);
            Invoke("BookChanged","statutory");Invoke("CurrencyChanged","KWD");Invoke("MappingDateChanged",CompanyGLMappingReviewTests.Date);Refresh();await Action("ReviewMappingConfiguration");Assert.NotNull(state.MappingConfigurationReview);
            if(mode!="empty")
            {
                Field("_mappingKey",mode=="overlap"?"AccountsReceivable":"Cash");Field("_mappingAccountId",mode=="account"?"foreign":"actual-account-id");Field("_mappingReason",mode=="reason"?"":"Reviewed effective mapping");
                Field("_mappingFrom",mode=="date"?new DateTime(2026,1,1,1,0,0):new DateTime(2026,1,1));if(mode=="expiry")Field("_mappingThrough",new DateTime(2025,12,31));Refresh();
            }
            if(mode=="grant")host.Context.Permissions=["Accounting.View"];if(mode is "uncertain" or "recreated")native.Reader.Accounts.Native.FailCommit=true;await Action("ConfigureMapping");
            if(mode is "uncertain" or "recreated")
            {
                Assert.True(state.HasPendingMappingConfiguration);Assert.Contains("original mapping configuration request is retained",root.ToHtmlString());var saved=System.Text.Json.JsonSerializer.Serialize(state.PendingMappingConfiguration);native.Reader.Accounts.Native.FailCommit=false;
                if(mode=="recreated"){components.Workbench!.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.LedgerAccounts>();Assert.Equal(saved,System.Text.Json.JsonSerializer.Serialize(state.PendingMappingConfiguration));Invoke("BookChanged","statutory");Invoke("CurrencyChanged","KWD");Invoke("MappingDateChanged",CompanyGLMappingReviewTests.Date);Refresh();await Action("ReloadMappings");}
                await Action("ReconcileMappingConfiguration");
            }
            if(mode=="recovery")
            {
                Assert.NotNull(state.MappingConfigurationResult);var id=state.MappingConfigurationResult!.GL_ACCOUNT_MAPPING_ID;components.Workbench!.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.LedgerAccounts>();
                Invoke("BookChanged","statutory");Invoke("CurrencyChanged","KWD");Field("_mappingRecoveryId",id);Refresh();await Action("RecoverMappingConfiguration");Assert.True(state.HasPendingMappingConfiguration);Assert.Null(state.PendingMappingConfiguration!.Review);await Action("ReconcileMappingConfiguration");
            }
            if(mode=="auth-change"){Assert.NotNull(state.MappingConfigurationResult);authentication.Change();Refresh();Assert.Null(state.MappingConfigurationResult);}
            return root.ToHtmlString();
        });
        if(mode is "valid" or "uncertain" or "recreated" or "recovery"){Assert.NotNull(state.MappingConfigurationResult);Assert.False(state.HasPendingLedgerRequest);Assert.Contains("configuration is recorded and confirmed in complete account/mapping history",html);Assert.Equal(1,native.Reader.Accounts.Native.Writes);}
        else if(mode=="auth-change"){Assert.Equal(1,native.Reader.Accounts.Native.Writes);Assert.DoesNotContain("configuration is recorded and confirmed in complete account/mapping history",html);}
        else{Assert.Null(state.MappingConfigurationResult);Assert.Equal(0,native.Reader.Accounts.Native.Writes);}
        if(mode is "empty" or "reason" or "account" or "expiry" or "overlap" or "date")Assert.False(state.HasPendingLedgerRequest);if(mode=="grant")Assert.True(state.HasPendingMappingConfiguration);
    }
    [Theory][InlineData("valid")][InlineData("empty")][InlineData("reason")][InlineData("type")][InlineData("parent")][InlineData("grant")][InlineData("uncertain")][InlineData("recreated")][InlineData("recovery")][InlineData("auth-change")]
    public async Task AdministratorFormRecordsExplicitNativeConfigurationOrRetainsItsExactOriginalRequest(string mode)
    {
        var native=new CompanyGLAccountConfigurationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var authentication=new Authentication();
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);services.AddScoped<CompanyLedgerAccountState>();
        services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider>(authentication);services.AddSingleton<IAccountingServiceClient>(CompanyGLAccountHttpTests.Client(host.Client,company));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var state=provider.GetRequiredService<CompanyLedgerAccountState>();
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.LedgerAccounts>();var components=(Components)provider.GetRequiredService<IComponentActivator>();
            void Invoke(string method,params object?[] values)=>components.Workbench!.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,values);
            void Refresh()=>Invoke("StateHasChanged");
            async Task Action(string method){await (Task)components.Workbench!.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null)!;Refresh();}
            void Field(string name,object? value)=>components.Workbench!.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(components.Workbench,value);
            Invoke("BookChanged","statutory");Invoke("CurrencyChanged","KWD");Refresh();await Action("ReviewConfiguration");Assert.NotNull(state.ConfigurationReview);
            if(mode!="empty"){Field("_number","4110");Field("_name","Explicit revenue account");Field("_type",mode=="type"?"UNKNOWN":"REVENUE");Field("_normal","CREDIT");Field("_reason",mode=="reason"?"":"Reviewed company account configuration");if(mode=="parent")Field("_parent","foreign");Refresh();}
            if(mode=="grant")host.Context.Permissions=["Accounting.View"];if(mode is "uncertain" or "recreated")native.Reader.Native.FailCommit=true;
            await Action("ConfigureAccount");
            if(mode is "uncertain" or "recreated")
            {
                Assert.True(state.HasPendingConfiguration);Assert.Contains("original account configuration request is retained",root.ToHtmlString());var saved=System.Text.Json.JsonSerializer.Serialize(state.PendingConfiguration);native.Reader.Native.FailCommit=false;
                if(mode=="recreated")
                {
                    components.Workbench!.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.LedgerAccounts>();
                    Assert.Equal(saved,System.Text.Json.JsonSerializer.Serialize(state.PendingConfiguration));Invoke("BookChanged","statutory");Invoke("CurrencyChanged","KWD");Refresh();await Action("Reload");
                }
                await Action("ReconcileConfiguration");
            }
            if(mode=="recovery")
            {
                Assert.NotNull(state.ConfigurationResult);var id=state.ConfigurationResult!.GL_ACCOUNT_ID;components.Workbench!.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.LedgerAccounts>();
                Invoke("BookChanged","statutory");Invoke("CurrencyChanged","KWD");Field("_recoveryId",id);Refresh();await Action("RecoverConfiguration");Assert.True(state.HasPendingConfiguration);Assert.Null(state.PendingConfiguration!.Review);await Action("ReconcileConfiguration");
            }
            if(mode=="auth-change"){Assert.NotNull(state.ConfigurationResult);authentication.Change();Refresh();Assert.Null(state.ConfigurationResult);Assert.DoesNotContain("configuration is recorded and confirmed",root.ToHtmlString());}
            return root.ToHtmlString();
        });
        if(mode is "valid" or "uncertain" or "recreated" or "recovery"){Assert.NotNull(state.ConfigurationResult);Assert.False(state.HasPendingConfiguration);Assert.Contains("configuration is recorded and confirmed",html);Assert.Equal(1,native.Reader.Native.Writes);}
        else if(mode=="auth-change")Assert.Equal(1,native.Reader.Native.Writes);
        else{Assert.Null(state.ConfigurationResult);Assert.Equal(0,native.Reader.Native.Writes);Assert.DoesNotContain("configuration is recorded and confirmed",html);}
        if(mode is "empty" or "reason" or "type" or "parent")Assert.False(state.HasPendingConfiguration);if(mode=="grant")Assert.True(state.HasPendingConfiguration);
    }
    [Theory][InlineData("valid")][InlineData("empty")][InlineData("future")][InlineData("date")][InlineData("book")][InlineData("currency")][InlineData("role")][InlineData("actor")][InlineData("permission")]
    [InlineData("unavailable")][InlineData("auth-change")][InlineData("company-change")][InlineData("book-change")][InlineData("currency-change")][InlineData("date-change")][InlineData("ar-SA")]
    public async Task ActualScreenReviewsEffectiveMappingsWithoutInferringPostingOrRetainingStaleEvidence(string mode)
    {
        var native=new CompanyGLMappingReviewTests.Fixture();if(mode=="empty")native.Accounts.Native.Rows.Remove(native.Mapping);if(mode=="unavailable")native.NullRows=true;
        if(mode=="future")native.Mapping.EFFECTIVE_DATE=CompanyGLMappingReviewTests.Date.AddDays(1);
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,context)=>{services.AddSingleton(native.Service);services.AddSingleton(native.Accounts.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:mode=="permission"?[]:null);var authentication=new Authentication{Role=mode!="role",Actor=mode=="actor"?"other":"actor"};
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);services.AddScoped<CompanyLedgerAccountState>();
        services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider>(authentication);
        services.AddSingleton<IAccountingServiceClient>(CompanyGLAccountHttpTests.Client(host.Client,company));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var previousCulture=System.Globalization.CultureInfo.CurrentCulture;var previousUICulture=System.Globalization.CultureInfo.CurrentUICulture;string html;
        try
        {
            if(mode=="ar-SA")System.Globalization.CultureInfo.CurrentCulture=System.Globalization.CultureInfo.CurrentUICulture=new System.Globalization.CultureInfo("ar-SA");
            html=await renderer.Dispatcher.InvokeAsync(async()=>
            {
                var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.LedgerAccounts>();var workbench=(Components)provider.GetRequiredService<IComponentActivator>();
                void Invoke(string method,params object?[] values)=>workbench.Workbench!.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(workbench.Workbench,values);
                void Refresh()=>Invoke("StateHasChanged");
                Invoke("BookChanged",mode=="book"?"":"statutory");Invoke("CurrencyChanged",mode=="currency"?"":"KWD");Invoke("MappingDateChanged",mode=="date"?null:(object)CompanyGLMappingReviewTests.Date);Refresh();
                await (Task)workbench.Workbench!.GetType().GetMethod("ReloadMappings",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(workbench.Workbench,null)!;Refresh();
                if(mode is "valid" or "ar-SA" or "future" or "auth-change" or "company-change" or "book-change" or "currency-change" or "date-change")
                {
                    Assert.Contains("Mapping ID: mapping-id",root.ToHtmlString());Assert.Contains("Company calendar date: 2026-10-12",root.ToHtmlString());Assert.Contains("Currency: KWD",root.ToHtmlString());
                    Assert.Contains("Posted balances are unavailable here",root.ToHtmlString());
                }
                if(mode=="auth-change")authentication.Change();if(mode=="company-change")company.Clear();if(mode=="book-change")Invoke("BookChanged","other");if(mode=="currency-change")Invoke("CurrencyChanged","USD");if(mode=="date-change")Invoke("MappingDateChanged",CompanyGLMappingReviewTests.Date.AddDays(1));Refresh();
                return root.ToHtmlString();
            });
        }
        finally{System.Globalization.CultureInfo.CurrentCulture=previousCulture;System.Globalization.CultureInfo.CurrentUICulture=previousUICulture;}
        if(mode is "valid" or "ar-SA" or "future")Assert.Contains("Mapping ID: mapping-id",html);else Assert.DoesNotContain("Mapping ID: mapping-id",html);
        if(mode=="future"){Assert.Contains("Outside selected date",html);Assert.DoesNotContain("Recorded account: Recorded receivable",html);}
        if(mode=="empty")Assert.Contains("No recorded mappings are configured",html);if(mode=="unavailable")Assert.Contains("Company mapping evidence could not be confirmed",html);
        if(mode is "date" or "book" or "currency" or "role" or "actor" or "permission"){Assert.Equal(0,native.MappingReads);Assert.Equal(0,native.Accounts.Reads);}native.Accounts.NoWrites();
    }
    [Theory][InlineData("company")][InlineData("account")][InlineData("permission")][InlineData("date")][InlineData("dispose")]
    public async Task MappingStateDiscardsLatePayloadAfterContextOrDatePresentationChanges(string fault)
    {
        using var company=CompanyGLAccountHttpTests.Selected();using var state=new CompanyLedgerAccountState(company);var completion=new TaskCompletionSource<CompanyGLMappingReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        var client=new Mock<IAccountingServiceClient>();client.Setup(value=>value.GetCompanyGLMappingsAsync("statutory","KWD",CompanyGLMappingReviewTests.Date)).Returns(completion.Task);
        var loading=state.LoadMappingsAsync("statutory","KWD",CompanyGLMappingReviewTests.Date,client.Object);Assert.True(state.Busy);
        if(fault=="date")state.Invalidate();else if(fault=="dispose")state.Dispose();else company.TrySelect(company.Clear(),fault=="account"?"other":"actor",new(new("tenant",fault=="company"?"other":"company"),["ConfiguredCompanyRole"],fault=="permission"?[]:["Accounting.View"]));
        var fixture=new CompanyGLMappingReviewTests.Fixture();completion.SetResult(CompanyGLMappingReview.Create(CompanyGLAccountReview.Create(new("tenant","company"),"actor","statutory","KWD",[fixture.Accounts.Row]),CompanyGLMappingReviewTests.Date,[fixture.Mapping]));
        await loading;Assert.Null(state.Mappings);Assert.Null(state.Review);Assert.False(state.Busy);
    }
    [Theory][InlineData("valid")][InlineData("empty")][InlineData("book")][InlineData("currency")][InlineData("role")][InlineData("actor")][InlineData("permission")]
    [InlineData("unavailable")][InlineData("auth-change")][InlineData("company-change")][InlineData("book-change")][InlineData("currency-change")]
    public async Task ActualAccountantScreenReviewsRecordedAccountsAndClearsPrivateEvidence(string mode)
    {
        var native=new CompanyGLAccountReviewTests.Fixture();if(mode=="empty")native.Native.Rows.Clear();if(mode=="unavailable")native.NullRows=true;
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,context)=>{services.AddSingleton(native.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:mode=="permission"?[]:null);var authentication=new Authentication{Role=mode!="role",Actor=mode=="actor"?"other":"actor"};
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);services.AddScoped<CompanyLedgerAccountState>();
        services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider>(authentication);
        services.AddSingleton<IAccountingServiceClient>(CompanyGLAccountHttpTests.Client(host.Client,company));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.LedgerAccounts>();var workbench=provider.GetRequiredService<IComponentActivator>() as Components;
            void Invoke(string method,params object?[] arguments)=>workbench!.Workbench!.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(workbench.Workbench,arguments);
            void Refresh()=>Invoke("StateHasChanged");
            Invoke("BookChanged",mode=="book"?"":"statutory");Invoke("CurrencyChanged",mode=="currency"?"":"KWD");Refresh();
            await (Task)workbench!.Workbench!.GetType().GetMethod("Reload",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(workbench.Workbench,null)!;Refresh();
            if(mode is "valid" or "auth-change" or "company-change" or "book-change" or "currency-change")
            {
                Assert.Contains("Native account ID: actual-account-id",root.ToHtmlString());Assert.Contains("Currency: KWD",root.ToHtmlString());
                Assert.Contains("Posted balances are unavailable here",root.ToHtmlString());
            }
            if(mode=="auth-change")authentication.Change();if(mode=="company-change")company.Clear();if(mode=="book-change")Invoke("BookChanged","other-book");if(mode=="currency-change")Invoke("CurrencyChanged","USD");Refresh();
            return root.ToHtmlString();
        });
        if(mode=="valid")Assert.Contains("Recorded receivable",html);else Assert.DoesNotContain("Native account ID: actual-account-id",html);
        if(mode=="empty")Assert.Contains("No recorded accounts are configured",html);
        if(mode=="unavailable")Assert.Contains("Company ledger configuration could not be confirmed",html);
        if(mode is "book" or "currency" or "role" or "actor" or "permission")Assert.Equal(0,native.Reads);
        native.NoWrites();
    }
    [Theory][InlineData("company")][InlineData("account")][InlineData("permission")][InlineData("presentation")][InlineData("dispose")]
    public async Task StateRejectsLatePrivatePayloadAfterContextOrPresentationChanges(string fault)
    {
        using var company=CompanyGLAccountHttpTests.Selected();using var state=new CompanyLedgerAccountState(company);var completion=new TaskCompletionSource<CompanyGLAccountReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        var client=new Mock<IAccountingServiceClient>();client.Setup(value=>value.GetCompanyGLAccountsAsync("statutory","KWD")).Returns(completion.Task);
        var loading=state.LoadAsync("statutory","KWD",client.Object);Assert.True(state.Busy);
        if(fault=="presentation")state.Invalidate();else if(fault=="dispose")state.Dispose();else company.TrySelect(company.Clear(),fault=="account"?"other":"actor",
            new(new("tenant",fault=="company"?"other":"company"),["ConfiguredCompanyRole"],fault=="permission"?[]:["Accounting.View"]));
        completion.SetResult(CompanyGLAccountReview.Create(new("tenant","company"),"actor","statutory","KWD",[new CompanyGLAccountReviewTests.Fixture().Row]));
        await loading;Assert.False(state.Busy);Assert.Null(state.Review);
    }
    [Fact] public async Task StateExposesOnlyDetachedValidatedReviewAndInvalidatesOldEvidenceBeforeFailure()
    {
        using var company=CompanyGLAccountHttpTests.Selected();using var state=new CompanyLedgerAccountState(company);var review=CompanyGLAccountReview.Create(new("tenant","company"),"actor","statutory","KWD",[new CompanyGLAccountReviewTests.Fixture().Row]);
        var client=new Mock<IAccountingServiceClient>();client.Setup(value=>value.GetCompanyGLAccountsAsync("statutory","KWD")).ReturnsAsync(review);
        await state.LoadAsync("statutory","KWD",client.Object);state.Review!.Accounts[0].ACCOUNT_NAME="caller changed";Assert.Equal("Recorded receivable",state.Review!.Accounts[0].ACCOUNT_NAME);
        client.Setup(value=>value.GetCompanyGLAccountsAsync("statutory","KWD")).ThrowsAsync(new InvalidOperationException());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync("statutory","KWD",client.Object));Assert.Null(state.Review);Assert.False(state.Busy);
    }
}
#endif
