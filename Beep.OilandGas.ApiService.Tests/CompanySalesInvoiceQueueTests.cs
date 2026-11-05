using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Security;
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

public sealed class CompanySalesInvoiceQueueTests
{
    private static readonly CompanyScope Scope=new("tenant","company");
    private static CompanySalesInvoiceQueueReview Queue(string actor="actor",string field="field",CompanyScope? scope=null)
    {
        var rows=new[]{new CompanySalesInvoiceQueueRow(new string('1',32),"Sales 1","buyer","ticket/one","KWD","DRAFT",new(2026,10,1),new(2026,10,31),10.123m,"maker",null,null,new string('A',64),new string('B',64)),
            new CompanySalesInvoiceQueueRow(new string('2',32),"Sales 2","buyer","ticket/two","USD","DRAFT",new(2026,10,1),new(2026,10,31),20.12m,"maker",null,null,new string('A',64),new string('B',64)),
            new CompanySalesInvoiceQueueRow(new string('3',32),"Sales 3","buyer","ticket/three","KWD","APPROVED",new(2026,10,1),new(2026,10,31),30.123m,"maker","reviewer",new DateTime(2026,10,2,0,0,0,DateTimeKind.Utc),new string('A',64),new string('B',64))};
        return Seal(new(scope ?? Scope,actor,field,new string('B',64),rows,string.Empty));
    }
    private static CompanySalesInvoiceQueueReview Seal(CompanySalesInvoiceQueueReview review)=>review with{Revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(review with{Revision=string.Empty}))};
    private static CompanyClientContext Selected(string actor="actor",CompanyScope? scope=null,params string[] grants)
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(scope ?? Scope,["ConfiguredCompanyRole"],new[]{"Accounting.View"}.Concat(grants).ToArray()));return company;}

    [Fact]public void CanonicalQueueKeepsCurrencyAndStatusTotalsSeparateAndRejectsChangedPayload()
    {
        var queue=Queue();queue.Require(Scope,"actor","field");Assert.Equal(3,queue.Totals.Count);
        Assert.Equal(10.123m,queue.Totals.Single(row=>row.Currency=="KWD" && row.Status=="DRAFT").ProposedTotal);
        Assert.Equal(30.123m,queue.Totals.Single(row=>row.Currency=="KWD" && row.Status=="APPROVED").ProposedTotal);
        var altered=queue with{Rows=queue.Rows.Select(row=>row with{ProposedTotal=row.ProposedTotal+1}).ToArray()};Assert.Throws<InvalidOperationException>(()=>altered.Require(Scope,"actor","field"));
    }
    [Theory][InlineData("duplicate")][InlineData("reviewer")][InlineData("calendar")][InlineData("foreign")][InlineData("bound")][InlineData("overflow")]
    public void CanonicalQueueRefusesInvalidCompleteSourceFacts(string fault)
    {
        var queue=Queue();queue=fault switch{
            "duplicate"=>queue with{Rows=[queue.Rows[0],queue.Rows[0]]},
            "reviewer"=>queue with{Rows=[queue.Rows[2] with{ReviewerId="maker"}]},
            "calendar"=>queue with{Rows=[queue.Rows[0] with{InvoiceDate=DateTime.SpecifyKind(queue.Rows[0].InvoiceDate,DateTimeKind.Utc)}]},
            "foreign"=>queue with{Company=new("tenant","other")},
            "bound"=>queue with{Rows=Enumerable.Range(1,129).Select(i=>queue.Rows[0] with{InvoiceId=i.ToString("x32")}).ToArray()},
            _=>queue with{Rows=[queue.Rows[0] with{ProposedTotal=decimal.MaxValue},queue.Rows[1] with{Currency="KWD",ProposedTotal=1m}]}};
        queue=Seal(queue);Assert.Throws<InvalidOperationException>(()=>queue.Require(Scope,"actor","field"));
    }
    [Theory][InlineData("company")][InlineData("account")][InlineData("field")][InlineData("dispose")]
    public async Task StateNeverPublishesAnOldQueueAfterContextChanges(string change)
    {
        var company=Selected();using var state=new CompanySalesInvoiceQueueState(company);var pending=new TaskCompletionSource<CompanySalesInvoiceQueueReview>();
        var load=state.LoadAsync(()=>Task.FromResult<string?>("field"),_=>pending.Task);Assert.True(state.Busy);
        if(change=="company")company.TrySelect(company.Clear(),"actor",new(new("tenant","other"),["ConfiguredCompanyRole"],["Accounting.View"]));
        else if(change=="account")company.TrySelect(company.Clear(),"other",new(Scope,["ConfiguredCompanyRole"],["Accounting.View"]));
        else if(change=="dispose")state.Dispose();else state.Clear();pending.SetResult(Queue());await load;Assert.Null(state.Review);Assert.False(state.Busy);
    }
    [Fact]public async Task StateCopiesRecordsAndClearsOldEvidenceBeforeErrorsAndMissingField()
    {
        var company=Selected();using var state=new CompanySalesInvoiceQueueState(company);await state.LoadAsync(()=>Task.FromResult<string?>("field"),_=>Task.FromResult(Queue()));Assert.NotNull(state.Review);
        var copy=state.Review!;((IList<CompanySalesInvoiceQueueRow>)copy.Rows)[0]=copy.Rows[0] with{Number="altered"};Assert.Equal("Sales 1",state.Review!.Rows[0].Number);
        await state.LoadAsync(()=>Task.FromResult<string?>(null),_=>throw new Exception("Must not read"));Assert.True(state.NeedsField);Assert.Null(state.Review);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync(()=>Task.FromResult<string?>("field"),_=>throw new InvalidOperationException("Unavailable")));Assert.Null(state.Review);Assert.False(state.Busy);
    }

    [Fact]public async Task ActualNativeQueueChecksOriginalReceiptAndCreatesNoWrites()
    {
        var f=new CompanySalesInvoiceHttpTests.Fixture();await f.Prepare();var command=f.Data.Command;var at=DateTime.UtcNow.AddSeconds(1);
        var intent=SALES_INVOICE_INTENT.Create(command,at);var invoice=CompanySalesInvoiceDraftProof.Prepare(command,at.AddSeconds(1));var receipt=SALES_INVOICE_OUTCOME.Create(intent,invoice,at.AddSeconds(2));
        f.Data.Native.Rows.AddRange([intent,invoice,receipt]);var writes=f.Data.Native.Writes;
        var records=await f.Data.Owner.GetCompanySalesInvoiceQueueAsync(command.Review.Company,command.Review.FieldId,f.Data.Authority);
        Assert.Equal(command.InvoiceId,Assert.Single(records).Invoice.INVOICE_ID);Assert.Equal(writes,f.Data.Native.Writes);
        await using var host=await f.Start();var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,f.Selected());
        var queue=await client.GetCompanySalesInvoiceQueueAsync(command.Review.FieldId);Assert.Equal(command.InvoiceId,Assert.Single(queue.Rows).InvoiceId);Assert.Equal(invoice.TOTAL_AMOUNT,Assert.Single(queue.Totals).ProposedTotal);
        var publicJson=JsonSerializer.Serialize(queue);Assert.DoesNotContain("BILLING_WORKFLOW_JSON",publicJson);Assert.DoesNotContain("COMMAND_JSON",publicJson);Assert.Equal(writes,f.Data.Native.Writes);
        f.Data.Native.Rows.Remove(receipt);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Owner.GetCompanySalesInvoiceQueueAsync(command.Review.Company,command.Review.FieldId,f.Data.Authority));Assert.Equal(writes,f.Data.Native.Writes);
    }
    [Theory][InlineData("foreign")][InlineData("duplicate")][InlineData("bound")]
    public async Task NativeQueueRefusesProviderScopeAndCompletenessFaults(string fault)
    {
        var f=new CompanySalesInvoiceHttpTests.Fixture();await f.Prepare();var rows=fault=="bound"?129:fault=="duplicate"?2:1;
        f.Data.Native.Source.Setup(source=>source.GetEntityAsync("INVOICE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(Enumerable.Range(0,rows).Select(i=>new INVOICE{INVOICE_ID=fault=="duplicate"?"same":i.ToString(),OWNER_TENANT_ID=f.Data.Current.Company.TenantId,OWNER_COMPANY_BA_ID=fault=="foreign"?"other":f.Data.Current.Company.BusinessAssociateId,FIELD_ID=f.Data.Current.FieldId}).ToArray());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Owner.GetCompanySalesInvoiceQueueAsync(f.Data.Current.Company,f.Data.Current.FieldId,f.Data.Authority));Assert.Equal(0,f.Data.Native.Writes);
    }

    [Theory][InlineData("ok",200)][InlineData("anonymous",401)][InlineData("role",403)][InlineData("permission",403)][InlineData("field",403)][InlineData("binding",403)]
    public async Task ActualQueueApiRequiresCurrentCompanyRoleAndFieldBeforeNativeRead(string fault,int status)
    {
        var owner=new Mock<IInvoiceService>();var assets=new Mock<IAccessControlService>();
        assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=fault!="field"});
        owner.Setup(value=>value.GetCompanySalesInvoiceQueueAsync(It.IsAny<CompanyScope>(),"field",It.IsAny<Func<Task>>())).ReturnsAsync(Array.Empty<CompanySalesInvoiceDraftOutcome>());
        await using var host=await AccountingSourceHttpTests.Start(authenticate:fault!="anonymous",role:fault!="role",permissions:fault=="permission"?[]:["Accounting.View"],configureAccounting:(services,context)=>{
            context.PolicyBinding=new string('B',64);context.ChangeBinding=fault=="binding";
            services.AddScoped(provider=>new Beep.OilandGas.ApiService.Services.CompanySalesInvoiceReads(owner.Object,null!,null!,provider.GetRequiredService<Beep.OilandGas.ApiService.Services.ICompanyAccountingReadContext>(),assets.Object));});
        var response=await host.Client.GetAsync("api/accounting/traditional/company-invoices/queue/field");Assert.Equal(status,(int)response.StatusCode);
        owner.Verify(value=>value.GetCompanySalesInvoiceQueueAsync(It.IsAny<CompanyScope>(),"field",It.IsAny<Func<Task>>()),status==200?Times.Exactly(2):Times.Never());
    }
    private sealed class Authentication:AuthenticationStateProvider
    {
        public bool Role=true;
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,"actor")}.Concat(Role?[new Claim(ClaimTypes.Role,"ConfiguredCompanyRole")]:[]),"test"))));
        public void Revoke(){Role=false;NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    [Theory][InlineData("role")][InlineData("permission")][InlineData("account")]
    public async Task QueueApiDiscardsReadWhenAuthorityChangesBeforePublication(string change)
    {
        var owner=new Mock<IInvoiceService>();var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        owner.Setup(value=>value.GetCompanySalesInvoiceQueueAsync(It.IsAny<CompanyScope>(),"field",It.IsAny<Func<Task>>())).ReturnsAsync(Array.Empty<CompanySalesInvoiceDraftOutcome>());
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View"],configureAccounting:(services,context)=>{
            context.PolicyBinding=new string('B',64);context.OnCapture=count=>{if(count==4){if(change=="role")context.Roles=[];else if(change=="permission")context.Permissions=[];else context.Actor="other";}};
            services.AddScoped(provider=>new Beep.OilandGas.ApiService.Services.CompanySalesInvoiceReads(owner.Object,null!,null!,provider.GetRequiredService<Beep.OilandGas.ApiService.Services.ICompanyAccountingReadContext>(),assets.Object));});
        var response=await host.Client.GetAsync("api/accounting/traditional/company-invoices/queue/field");Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);
        owner.Verify(value=>value.GetCompanySalesInvoiceQueueAsync(It.IsAny<CompanyScope>(),"field",It.IsAny<Func<Task>>()),Times.Once());
    }
    private sealed class Handler(Func<HttpRequestMessage,Task<HttpResponseMessage>> send):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)=>send(request);}
    [Theory][InlineData("ok")][InlineData("company")][InlineData("account")][InlineData("foreign")][InlineData("changed")]
    public async Task TypedQueueClientEscapesFieldAndRefusesLateOrAlteredEvidence(string mode)
    {
        var company=Selected();const string field="field /?";
        using var http=new HttpClient(new Handler(request=>{
            Assert.Contains("field%20%2F%3F",request.RequestUri!.OriginalString);
            var review=Queue(field:field);if(mode=="foreign")review=Queue(field:field,scope:new("tenant","other"));
            if(mode=="changed")review=review with{Rows=[review.Rows[0] with{Number="changed"}]};
            if(mode is "company" or "account")company.TrySelect(company.Clear(),mode=="account"?"other":"actor",new(mode=="company"?new("tenant","other"):Scope,["ConfiguredCompanyRole"],["Accounting.View"]));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=System.Net.Http.Json.JsonContent.Create(review)});
        })){BaseAddress=new("http://localhost/")};var client=CompanySalesInvoiceHttpTests.Fixture.Client(http,company);
        if(mode=="ok")Assert.Equal(field,(await client.GetCompanySalesInvoiceQueueAsync(field)).FieldId);
        else if(mode is "company" or "account")await Assert.ThrowsAsync<OperationCanceledException>(()=>client.GetCompanySalesInvoiceQueueAsync(field));
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetCompanySalesInvoiceQueueAsync(field));
    }
    private sealed class Components:IComponentActivator
    {
        public Beep.OilandGas.Web.Components.Workflow.SalesInvoiceQueue? Queue;
        public IComponent CreateInstance(Type type)
        {var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.SalesInvoiceQueue queue)Queue=queue;return component;}
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ActualMudQueueShowsCurrencyStatusAndOwnedLinksThenRedactsOnRoleChange(bool reviewGrant)
    {
        var company=Selected(grants:reviewGrant?["Accounting.ReviewSalesInvoice"]:[]);var accounting=new Mock<IAccountingServiceClient>();accounting.Setup(value=>value.GetCompanySalesInvoiceQueueAsync("field")).ReturnsAsync(Queue());
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("field");var auth=new Authentication();
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();
        services.AddSingleton(company);services.AddSingleton(accounting.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.SalesInvoiceQueue>(ParameterView.Empty);var html=root.ToHtmlString();
            Assert.Contains("10.123",html);Assert.Contains("30.123",html);Assert.Contains("20.12",html);Assert.Contains("KWD",html);Assert.Contains("USD",html);Assert.Contains("APPROVED",html);
            Assert.Contains("ticket%2Fone",html);Assert.Equal(reviewGrant,html.Contains("sales-invoice-review?invoice="));Assert.Contains("settlement are unavailable",html);
            Assert.Contains("sales-receivables?invoice="+new string('1',32),html);
            var component=((Components)provider.GetRequiredService<IComponentActivator>()).Queue!;
            var select=component.GetType().GetMethod("Select",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!;
            var render=typeof(ComponentBase).GetMethod("StateHasChanged",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!;
            select.Invoke(component,["USD","DRAFT"]);render.Invoke(component,null);await root.QuiescenceTask;html=root.ToHtmlString();
            Assert.Contains("Sales 2",html);Assert.DoesNotContain("Sales 1",html);Assert.DoesNotContain("Sales 3",html);
            select.Invoke(component,[null,null]);render.Invoke(component,null);await root.QuiescenceTask;Assert.Contains("Sales 1",root.ToHtmlString());
            auth.Revoke();await Task.Yield();html=root.ToHtmlString();Assert.DoesNotContain("Sales 1",html);Assert.Contains("could not be confirmed",html);
        });
    }
}
#endif
