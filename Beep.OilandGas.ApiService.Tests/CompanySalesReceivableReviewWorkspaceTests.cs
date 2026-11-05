using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Web.Services;
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

public sealed class CompanySalesReceivableReviewWorkspaceTests
{
    private const string Review="Accounting.ReviewSalesReceivable",Record="Accounting.RecordSalesReceivableDecision",Route="api/accounting/traditional/company-receivables";
    private static ClaimsPrincipal User(string actor="controller",bool role=true)=>new(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,actor)}.Concat(role?[new Claim(ClaimTypes.Role,"ConfiguredCompanyRole")]:[]),"test"));
    private static CompanyClientContext Selected(CompanyScope scope,string actor="controller",string[]? grants=null)
    {var selected=new CompanyClientContext();selected.TrySelect(selected.Clear(),actor,new(scope,["ConfiguredCompanyRole"],grants??["Accounting.View",Review,Record]));return selected;}
    private static async Task<AccountingSourceHttpTests.Host> Start(CompanySalesReceivableDecisionTests.Fixture f,bool auth=true,bool role=true,string[]? grants=null,string actor="controller")
    {
        var host=await f.Data.Invoice.Data.Start(authenticated:auth,role:role,permissions:grants??["Accounting.View",Review,Record],configure:(services,context)=>{context.Actor=actor;services.AddSingleton<IARService>(f.Data.Owner);services.AddScoped<CompanySalesReceivableReads>();});
        host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);return host;
    }
    private static CompanySalesReceivableWorkspaceState State(CompanyClientContext company)
    {var state=new CompanySalesReceivableWorkspaceState(company);state.AttachOwner(new object());state.Admit(User());return state;}
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task ActualApiClientRecordsExactNativeDispositionAndRecoversOnlyItsReviewer(string decision)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();await using var host=await Start(f);using var company=Selected(f.Command.Company);var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);
        var source=await client.GetCompanySalesReceivableReviewAsync(f.Command.ReceivableId);Assert.True(source.CanReview);
        var command=f.Command with{Review=source,Decision=decision};var outcome=await client.RecordCompanySalesReceivableDecisionAsync(command);AccountingServiceClient.RequireReceivableDecision(outcome,command);
        var recovered=await client.GetCompanySalesReceivableReviewRecoveryAsync(command.ReceivableId);recovered.Require(command.Company,command.ActorUserId,command.ReceivableId);Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(recovered.Retained!.Restore(command.Company,command.ActorUserId)));Assert.Equal(JsonSerializer.Serialize(outcome),JsonSerializer.Serialize(recovered.Outcome));
        var status=await client.GetCompanySalesReceivableStatusAsync(command.Review.InvoiceId);Assert.Equal("controller",Assert.Single(status.Receivables).ReviewerId);Assert.Equal(decision=="APPROVE"?"APPROVED":"REJECTED",Assert.Single(status.Receivables).Status);
        Assert.Equal(JsonSerializer.Serialize(f.Original.Receipt),JsonSerializer.Serialize(outcome.Receipt));Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);Assert.DoesNotContain(f.Data.Invoice.Data.Data.Native.Rows,row=>row.GetType().Name is "AR_PAYMENT" or "JOURNAL_ENTRY");
    }
    [Theory][InlineData("anonymous",401)][InlineData("role",403)][InlineData("view",403)][InlineData("field",403)][InlineData("binding",403)][InlineData("company",409)]
    public async Task ReviewSourceRequiresCurrentCompanyRoleFieldAndBinding(string fault,int expected)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();f.Data.Invoice.Data.AssetDenied=fault=="field";
        await using var host=await Start(f,auth:fault!="anonymous",role:fault!="role",grants:fault=="view"?[Review,Record]:null);
        if(fault=="binding")host.Context.ChangeBinding=true;if(fault=="company")host.Client.DefaultRequestHeaders.Add("X-Test-Company","other");
        Assert.Equal((HttpStatusCode)expected,(await host.Client.GetAsync(Route+"/review/"+f.Command.ReceivableId)).StatusCode);Assert.Equal(0,f.Data.Invoice.Data.Data.Native.Writes);
    }
    [Theory][InlineData("Accounting.PrepareSalesReceivable")][InlineData("Accounting.ReviewSalesInvoice")][InlineData("Accounting.RecordSalesInvoiceDecision")][InlineData("Accounting.PrepareBillingPolicy")][InlineData("Accounting.RecordSalesReceivableDecision")]
    public async Task OtherGrantsCannotBorrowIndependentReceivableReview(string grant)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();await using var host=await Start(f,grants:["Accounting.View",grant]);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(Route+"/review/"+f.Command.ReceivableId)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync(Route+"/decisions",f.Command)).StatusCode);Assert.Equal(0,f.Data.Invoice.Data.Data.Native.Writes);
    }
    [Theory][InlineData("field-write")][InlineData("changed-role")][InlineData("changed-grant")]
    public async Task DecisionCannotPublishAfterOriginalWriteAuthorityChanges(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();await using var host=await Start(f);using var company=Selected(f.Command.Company);var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);
        var command=f.Command with{Review=await client.GetCompanySalesReceivableReviewAsync(f.Command.ReceivableId)};
        if(fault=="field-write")f.Data.Invoice.Data.Assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),command.Review.FieldId,"FIELD","WRITE",null)).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=false});
        else f.OnUpdate=()=>{if(fault=="changed-role")host.Context.Roles=[];else host.Context.Permissions=["Accounting.View",Review];};
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync(Route+"/decisions",command)).StatusCode);Assert.Null(Assert.Single(f.Data.Invoice.Data.Data.Native.Rows.OfType<AR_INVOICE>()).REVIEW_WORKFLOW_JSON);
        Assert.Equal(fault=="field-write"?0:1,f.Data.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_REVIEW_INTENT>().Count());
    }
    [Fact]public async Task OriginalReviewerRecoverySurvivesSourceAdvanceAndLossOfWriteGrants()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();var result=await f.Execute();var physical=f.Data.Invoice.Data.Data.Native.Rows.First(row=>row.GetType().Name=="REVENUE_TRANSACTION");physical.GetType().GetProperty("ROW_CHANGED_BY")!.SetValue(physical,"advanced receiving source");
        await using var host=await Start(f,grants:["Accounting.View"]);using var company=Selected(f.Command.Company,grants:["Accounting.View"]);var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);
        var recovered=await client.GetCompanySalesReceivableReviewRecoveryAsync(f.Command.ReceivableId);Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(recovered.Outcome));Assert.Equal(JsonSerializer.Serialize(f.Command),JsonSerializer.Serialize(recovered.Retained!.Restore(f.Command.Company,"controller")));
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(Route+"/review/"+f.Command.ReceivableId)).StatusCode);Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);
    }
    [Fact]public async Task AnotherAccountingReaderCannotRecoverPrivateReviewerCommand()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();await f.Execute();await using var host=await Start(f,grants:["Accounting.View"],actor:"reader");using var company=Selected(f.Command.Company,"reader",["Accounting.View"]);var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);
        var recovered=await client.GetCompanySalesReceivableReviewRecoveryAsync(f.Command.ReceivableId);Assert.Null(recovered.Retained);Assert.Null(recovered.Outcome);Assert.DoesNotContain(f.Command.Reason,JsonSerializer.Serialize(recovered));
        Assert.Equal("controller",Assert.Single((await client.GetCompanySalesReceivableStatusAsync(f.Command.Review.InvoiceId)).Receivables).ReviewerId);Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);
    }
    [Fact]public async Task MakerSeesExplicitSeparationOfDutiesAndCannotRecordReview()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();await using var host=await Start(f,actor:"reviewer");using var company=Selected(f.Command.Company,"reviewer");var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);
        using var state=new CompanySalesReceivableWorkspaceState(company);state.AttachOwner(new object());state.Admit(User("reviewer"));await state.LoadReviewAsync(f.Command.ReceivableId,client);Assert.False(state.ReviewSource!.IsIndependent);Assert.False(state.CanRecordReview);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordReviewAsync("APPROVE","CONFIRMED","Explicit finance","CONFIRMED","Explicit source","Explicit reason",client));Assert.Equal(0,f.Data.Invoice.Data.Data.Native.Writes);
    }
    [Fact]public async Task FreshCircuitRecoversExactRetainedDecisionAndRequiresExplicitRetry()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();await using var host=await Start(f);using var company=Selected(f.Command.Company);var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);CompanySalesReceivableDecisionCommand original;
        using(var first=State(company)){
            await first.LoadReviewAsync(f.Command.ReceivableId,client);f.FailUpdate=true;await Assert.ThrowsAsync<OilGasApiException>(()=>first.RecordReviewAsync("APPROVE","CONFIRMED","Original finance evidence","CONFIRMED","Original source evidence","Original decision reason",client));original=first.ReviewPending!;Assert.True(first.HasPendingReview);Assert.False(first.CanRecordReview);}
        var writes=f.Data.Invoice.Data.Data.Native.Writes;using var reopened=State(company);await reopened.RecoverReviewAsync(f.Command.ReceivableId,client);Assert.Equal(JsonSerializer.Serialize(original),JsonSerializer.Serialize(reopened.ReviewPending));Assert.Equal(writes,f.Data.Invoice.Data.Data.Native.Writes);
        f.FailUpdate=false;await reopened.SubmitReviewAsync(client);Assert.False(reopened.HasPendingReview);AccountingServiceClient.RequireReceivableDecision(reopened.ReviewRecovery!.Outcome!,original);Assert.Single(f.Data.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_REVIEW_INTENT>());Assert.Equal(2,f.Updates);
    }
    [Fact]public async Task UnknownCommitRetainsOriginalDecisionAndRecoversWithoutAnotherUpdate()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();await using var host=await Start(f);using var company=Selected(f.Command.Company);var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);using var state=State(company);
        await state.LoadReviewAsync(f.Command.ReceivableId,client);f.Uncertain=true;await Assert.ThrowsAsync<OilGasApiException>(()=>state.RecordReviewAsync("APPROVE","CONFIRMED","Original financial evidence","CONFIRMED","Original source evidence","Original reviewer reason",client));var original=state.ReviewPending!;Assert.True(state.HasPendingReview);Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);
        await state.RecoverReviewAsync(f.Command.ReceivableId,client);Assert.False(state.HasPendingReview);AccountingServiceClient.RequireReceivableDecision(state.ReviewRecovery!.Outcome!,original);Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);
    }
    [Theory][InlineData("auth")][InlineData("detached")][InlineData("company")]
    public async Task LateDecisionRemainsPrivateUntilOriginalContextRecovery(string change)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();await using var host=await Start(f);using var company=Selected(f.Command.Company);var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);using var state=State(company);var owner=new object();state.AttachOwner(owner);state.Admit(User());await state.LoadReviewAsync(f.Command.ReceivableId,client);
        var committed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var native=f.Data.Invoice.Data.Data.Native;native.AfterCommit=()=>{if(native.Rows.OfType<AR_INVOICE>().Any(row=>row.REVIEW_WORKFLOW_JSON is not null))committed.TrySetResult();};
        f.OnUpdate=()=>{if(change=="auth")state.Invalidate();else if(change=="company")company.Clear();else state.DetachOwner(owner);};
        if(change=="company"){var error=await Xunit.Record.ExceptionAsync(()=>state.RecordReviewAsync("APPROVE","CONFIRMED","Explicit finance","CONFIRMED","Explicit source","Explicit reason",client));Assert.True(error is OperationCanceledException or InvalidOperationException);}
        else await state.RecordReviewAsync("APPROVE","CONFIRMED","Explicit finance","CONFIRMED","Explicit source","Explicit reason",client);
        await committed.Task.WaitAsync(TimeSpan.FromSeconds(60));Assert.True(state.HasPendingReview);Assert.Null(state.ReviewPending);Assert.Null(state.ReviewSource);Assert.Null(state.ReviewRecovery);Assert.Equal(2,native.Writes);
        company.TrySelect(company.Clear(),"controller",new(f.Command.Company,["ConfiguredCompanyRole"],["Accounting.View"]));host.Context.Permissions=["Accounting.View"];state.AttachOwner(new object());state.Admit(User());await state.RecoverReviewAsync(f.Command.ReceivableId,client);Assert.False(state.HasPendingReview);Assert.Equal(2,native.Writes);
    }
    [Fact]public async Task MissingFreshRecoveryConfirmationCannotDiscardOriginalDecision()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();await using var host=await Start(f);using var company=Selected(f.Command.Company);var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);using var state=State(company);await state.LoadReviewAsync(f.Command.ReceivableId,client);
        var damaged=new Mock<IAccountingServiceClient>();damaged.Setup(value=>value.RecordCompanySalesReceivableDecisionAsync(It.IsAny<CompanySalesReceivableDecisionCommand>())).Returns((CompanySalesReceivableDecisionCommand command)=>client.RecordCompanySalesReceivableDecisionAsync(command));damaged.Setup(value=>value.GetCompanySalesReceivableReviewRecoveryAsync(f.Command.ReceivableId)).Returns(async()=>{var known=await client.GetCompanySalesReceivableReviewRecoveryAsync(f.Command.ReceivableId);return known with{Outcome=null};});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordReviewAsync("APPROVE","CONFIRMED","Explicit finance","CONFIRMED","Explicit source","Explicit reason",damaged.Object));Assert.True(state.HasPendingReview);Assert.Null(state.ReviewRecovery);var original=state.ReviewPending!;
        await state.RecoverReviewAsync(f.Command.ReceivableId,client);Assert.False(state.HasPendingReview);AccountingServiceClient.RequireReceivableDecision(state.ReviewRecovery!.Outcome!,original);Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);
    }
    [Theory][InlineData("company")][InlineData("account")][InlineData("actor")][InlineData("amount")][InlineData("revision")]
    public async Task TypedClientRefusesLateOrAlteredCompleteReview(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();var review=f.Command.Review;using var company=Selected(f.Command.Company);
        using var http=new HttpClient(new Handler(request=>{
            if(fault=="company")company.TrySelect(company.Clear(),"controller",new(new("tenant","other"),["ConfiguredCompanyRole"],["Accounting.View",Review]));if(fault=="account")company.TrySelect(company.Clear(),"other",new(f.Command.Company,["ConfiguredCompanyRole"],["Accounting.View",Review]));
            if(fault=="actor")review=review with{CurrentInvoice=review.CurrentInvoice.AsReader("other")};if(fault=="amount")review.Original.Receivable.TOTAL_AMOUNT+=1;if(fault=="revision")review=review with{Revision=new string('C',64)};
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(review)});
        })){BaseAddress=new("http://localhost/")};var client=CompanySalesInvoiceHttpTests.Fixture.Client(http,company);
        if(fault is "company" or "account")await Assert.ThrowsAsync<OperationCanceledException>(()=>client.GetCompanySalesReceivableReviewAsync(f.Command.ReceivableId));else await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetCompanySalesReceivableReviewAsync(f.Command.ReceivableId));
    }
    private sealed class Handler(Func<HttpRequestMessage,Task<HttpResponseMessage>> send):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)=>send(request);}
    private sealed class Authentication:AuthenticationStateProvider
    {public bool Role=true;public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(User(role:Role)));public void Revoke(){Role=false;NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}}
    private sealed class Components:IComponentActivator
    {public Beep.OilandGas.Web.Components.Workflow.SalesReceivableReviewWorkbench? Review;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.SalesReceivableReviewWorkbench review)Review=review;return component;}}
    [Fact]public async Task RetainedReviewerRecoveryStaysAvailableWhenAnotherInvoiceStatusCannotLoad()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();await using var host=await Start(f);using var company=Selected(f.Command.Company);var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);using var state=State(company);await state.LoadReviewAsync(f.Command.ReceivableId,client);f.FailUpdate=true;
        await Assert.ThrowsAsync<OilGasApiException>(()=>state.RecordReviewAsync("APPROVE","CONFIRMED","Explicit finance","CONFIRMED","Explicit source","Original saved reviewer reason",client));var original=state.ReviewPending!;var writes=f.Data.Invoice.Data.Data.Native.Writes;
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton(company);services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton(state);services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.SalesReceivables>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Invoice",Guid.NewGuid().ToString("N")}}));var html=root.ToHtmlString();Assert.Contains("Open original receivable reviewer request",html);Assert.Contains("/accounting/sales-receivables?invoice="+original.Review.InvoiceId,html);Assert.Contains("Original saved reviewer reason",html);Assert.Contains("Recover my original reviewer decision",html);
            var component=((Components)provider.GetRequiredService<IComponentActivator>()).Review!;await (Task)component.GetType().GetMethod("Recover",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(component,null)!;
            Assert.Equal(JsonSerializer.Serialize(original),JsonSerializer.Serialize(state.ReviewPending));Assert.Equal(writes,f.Data.Invoice.Data.Data.Native.Writes);
        });
    }
    [Theory][InlineData("APPROVE","en-US")][InlineData("REJECT","ar-SA")]
    public async Task ActualRoutedMudFormRequiresExplicitEvidenceRecordsDispositionAndRedactsRevokedRole(string decision,string culture)
    {
        var originalCulture=System.Globalization.CultureInfo.CurrentCulture;System.Globalization.CultureInfo.CurrentCulture=System.Globalization.CultureInfo.GetCultureInfo(culture);
        try{
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesReceivableDecisionTests.Fixture();await f.Prepare();await using var host=await Start(f);using var company=Selected(f.Command.Company);var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);var auth=new Authentication();var services=new ServiceCollection();services.AddLogging();services.AddMudServices();
        services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton(company);services.AddSingleton<IAccountingServiceClient>(client);services.AddScoped<CompanySalesReceivableWorkspaceState>();services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var amount=f.Original.Receivable.TOTAL_AMOUNT!.Value.ToString(System.Globalization.CultureInfo.CurrentCulture);
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.SalesReceivables>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Invoice",f.Command.Review.InvoiceId}}));var html=root.ToHtmlString();Assert.Contains("Independent receivable financial review",html);Assert.Contains(System.Text.Encodings.Web.HtmlEncoder.Default.Encode(amount),html);
            var component=((Components)provider.GetRequiredService<IComponentActivator>()).Review!;var flags=BindingFlags.NonPublic|BindingFlags.Instance;
            await (Task)component.GetType().GetMethod("Load",flags)!.Invoke(component,null)!;typeof(ComponentBase).GetMethod("StateHasChanged",flags)!.Invoke(component,null);await root.QuiescenceTask;html=root.ToHtmlString();Assert.Contains("Financial review evidence",html);Assert.Contains("Source review evidence",html);Assert.Contains("Reviewer decision reason",html);
            var calendar=f.Original.Receivable.INVOICE_DATE!.Value.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture);Assert.Contains(calendar,html);
            if(culture=="ar-SA"){var converted=f.Original.Receivable.INVOICE_DATE.Value.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.CurrentCulture);Assert.NotEqual(calendar,converted);Assert.DoesNotContain(converted,html);}
            await (Task)component.GetType().GetMethod("Record",flags)!.Invoke(component,[decision])!;Assert.Equal(0,f.Data.Invoice.Data.Data.Native.Writes);
            foreach(var (name,value) in new[]{("_financial",decision=="REJECT"?"ISSUE":"CONFIRMED"),("_financialEvidence","Explicit screen finance evidence"),("_source","CONFIRMED"),("_sourceEvidence","Explicit screen source evidence"),("_reason","Explicit screen reviewer reason")})component.GetType().GetField(name,flags)!.SetValue(component,value);
            typeof(ComponentBase).GetMethod("StateHasChanged",flags)!.Invoke(component,null);await root.QuiescenceTask;await (Task)component.GetType().GetMethod("Record",flags)!.Invoke(component,[decision])!;typeof(ComponentBase).GetMethod("StateHasChanged",flags)!.Invoke(component,null);await root.QuiescenceTask;html=root.ToHtmlString();
            Assert.Contains("original reviewer decision and native disposition are confirmed",html);Assert.Contains(decision=="APPROVE"?"APPROVED":"REJECTED",html);Assert.Contains("Independent reviewer controller",html);Assert.Equal(2,f.Data.Invoice.Data.Data.Native.Writes);
            auth.Revoke();await Task.Yield();html=root.ToHtmlString();Assert.DoesNotContain(System.Text.Encodings.Web.HtmlEncoder.Default.Encode(amount),html);Assert.DoesNotContain("Explicit screen reviewer reason",html);Assert.DoesNotContain("Financial review evidence",html);
        });
        }finally{System.Globalization.CultureInfo.CurrentCulture=originalCulture;}
    }
}
#endif
