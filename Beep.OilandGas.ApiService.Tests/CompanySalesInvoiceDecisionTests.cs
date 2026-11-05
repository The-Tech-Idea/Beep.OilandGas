#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text.Json;
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanySalesInvoiceDecisionTests
{
    private const string ReadGrant="Accounting.ReviewSalesInvoice",WriteGrant="Accounting.RecordSalesInvoiceDecision";
    internal sealed class Fixture
    {
        public readonly CompanySalesInvoiceHttpTests.Fixture Data=new();
        public CompanySalesInvoiceDraftOutcome Original=null!;public CompanySalesInvoiceDecisionCommand Command=null!;
        public bool FailWrite,Corrupt,Denied,Advanced;public Action? OnUpdate;public int Updates,Reads;
        public decimal? ExplicitTax,ExplicitCharges;
        public async Task Prepare(string decision="APPROVE")
        {
            await Data.Prepare("partial");var native=Data.Data.Native;
            if(ExplicitTax is not null || ExplicitCharges is not null)Data.Data.Command=Data.Data.Command with{Facts=Data.Data.Command.Facts with{TaxAmount=ExplicitTax??Data.Data.Command.Facts.TaxAmount,ChargesAmount=ExplicitCharges??Data.Data.Command.Facts.ChargesAmount}};
            async Task<CompanySalesInvoiceDraftReview> Maker()=>CompanySalesInvoiceDraftReview.Create(Data.Data.Current.Delivery,Data.Data.Current.Policies,await Data.Data.Owner.GetCompanySalesInvoicesForDeliveryAsync(Data.Data.Current.Company,Data.Data.Current.FieldId,Data.Ticket,Data.Data.Authority),Data.Data.Current.BindingRevision);
            Original=await Data.Data.Owner.ExecuteCompanyInvoiceDraftAsync(Data.Data.Command,Data.Data.Authority,Maker);
            native.Source.Setup(value=>value.InsertEntity("SALES_INVOICE_REVIEW_INTENT",It.IsAny<object>())).Returns((string _,object row)=>{native.Writes++;native.Rows.Add(CompanyInventorySourceReview.Copy((SALES_INVOICE_REVIEW_INTENT)row));return new ErrorsInfo{Flag=Errors.Ok};});
            native.Source.Setup(value=>value.UpdateEntity("INVOICE",It.IsAny<object>())).Returns((string _,object value)=>{native.Writes++;Updates++;if(FailWrite)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((INVOICE)value);if(Corrupt)row.TOTAL_AMOUNT+=1m;native.Rows.RemoveAll(item=>item is INVOICE invoice && invoice.INVOICE_ID==row.INVOICE_ID);native.Rows.Add(row);OnUpdate?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            Data.Assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),Data.Data.Current.FieldId,"FIELD","WRITE",null)).ReturnsAsync(()=>new AccessCheckResponse{HasAccess=!Data.AssetDenied});
            Command=new(Guid.NewGuid().ToString("N"),await Current(),decision,"CONFIRMED","Independent tax evidence","CONFIRMED","Independent charges evidence","CONFIRMED","Independent entitlement evidence","Record explicit independent decision");
        }
        public Task Authority(){if(Denied)throw new UnauthorizedAccessException();return Data.Data.Authority();}
        public async Task<CompanySalesInvoiceReviewSource> Current()
        {
            Reads++;var original=Data.Data.Current;var makerField=original.Delivery.Source.Source.Source;
            var field=FieldRevenueSourceReview.Create(makerField.Company,"reviewer",makerField.FieldId,makerField.From,makerField.Through,makerField.BindingRevision,makerField.Rows);
            var contract=RevenueSalesContractReview.Create(field,original.Delivery.Source.Source.TransactionId,original.Delivery.Source.Source.Contract);
            var delivery=new RevenueDeliveryAcceptanceReadReview(RevenueDeliverySourceReview.Create(contract,original.Delivery.Source.Allocation,original.Delivery.Source.Ticket),original.Delivery.Recorded);
            var native=await Data.Data.Owner.GetCompanyInvoiceDraftForReviewAsync(original.Company,Original.Invoice.INVOICE_ID,Authority,_=>Authority());
            return CompanySalesInvoiceReviewSource.Create(native,CompanySalesInvoiceDraftReview.Create(delivery,CompanyBillingPolicyReview.Create(original.Company,"reviewer",original.Policies.Records),[native.Invoice],Advanced?new string('C',64):original.BindingRevision));
        }
        public Task<CompanySalesInvoiceDraftOutcome> Execute()=>Data.Data.Owner.RecordCompanyInvoiceDecisionAsync(Command,Authority,Current);
        public INVOICE Stored=>Assert.Single(Data.Data.Native.Rows.OfType<INVOICE>());
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task IndependentNativeDecisionPreservesOriginalDraftAndReceiptAndExactReplay(string decision)
    {
        var f=new Fixture();await f.Prepare(decision);var originalReceipt=JsonSerializer.Serialize(f.Original.Receipt);var result=await f.Execute();
        Assert.Equal(decision=="APPROVE"?"APPROVED":"REJECTED",result.Invoice.STATUS);Assert.Equal(f.Original.Invoice.ROW_CREATED_BY,result.Invoice.ROW_CREATED_BY);Assert.Equal("reviewer",result.Invoice.ROW_CHANGED_BY);
        Assert.Equal(f.Original.Invoice.BILLING_WORKFLOW_JSON,result.Invoice.BILLING_WORKFLOW_JSON);Assert.Equal(originalReceipt,JsonSerializer.Serialize(result.Receipt));Assert.Equal(f.Original.Invoice.TOTAL_AMOUNT,result.Invoice.TOTAL_AMOUNT);
        result.Require(f.Data.Data.Command);Assert.Equal(JsonSerializer.Serialize(f.Command),JsonSerializer.Serialize(CompanySalesInvoiceDecisionProof.Require(f.Command.Company,result.Invoice).Intent.Restore(f.Command.Company,"reviewer")));
        var calls=f.Reads;var writes=f.Data.Data.Native.Writes;f.Advanced=true;var recovered=await f.Execute();Assert.Equal(calls,f.Reads);Assert.Equal(writes,f.Data.Data.Native.Writes);Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(recovered));
        var draftReplay=await f.Data.Data.Owner.ExecuteCompanyInvoiceDraftAsync(f.Data.Data.Command,f.Authority,()=>throw new InvalidOperationException("Known original outcome cannot reprepare."));Assert.Equal(result.Invoice.STATUS,draftReplay.Invoice.STATUS);Assert.Equal(1,f.Updates);
    }
    [Theory][InlineData("write")][InlineData("corrupt")][InlineData("authority")][InlineData("binding")][InlineData("source")]
    public async Task FailedDispositionRollsBackNativeInvoiceButRetainsOriginalReviewerBody(string fault)
    {
        var f=new Fixture();await f.Prepare();f.FailWrite=fault=="write";f.Corrupt=fault=="corrupt";f.OnUpdate=()=>{if(fault=="authority")f.Denied=true;if(fault=="binding")f.Data.Data.Native.Connection="changed";if(fault=="source")f.Advanced=true;};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Execute());Assert.Equal("DRAFT",f.Stored.STATUS);Assert.Null(f.Stored.REVIEW_WORKFLOW_JSON);Assert.Equal(JsonSerializer.Serialize(f.Original.Invoice),JsonSerializer.Serialize(f.Stored));
        var intent=Assert.Single(f.Data.Data.Native.Rows.OfType<SALES_INVOICE_REVIEW_INTENT>());Assert.Equal(JsonSerializer.Serialize(f.Command),JsonSerializer.Serialize(intent.Restore(f.Command.Company,"reviewer")));Assert.Equal("rollback",f.Data.Data.Native.Transactions.Last());
    }
    [Fact]public async Task NewCircuitCanRecoverFailedDecisionAndOtherReviewerCannotAdoptReservation()
    {
        var f=new Fixture();await f.Prepare();f.FailWrite=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());f.FailWrite=false;
        var retained=await f.Data.Data.Owner.GetCompanyInvoiceReviewIntentAsync(f.Command.Company,"reviewer",f.Command.InvoiceId,f.Authority,_=>f.Authority());Assert.NotNull(retained);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Data.Owner.GetCompanyInvoiceReviewIntentAsync(f.Command.Company,"another",f.Command.InvoiceId,f.Authority,_=>f.Authority()));
        var changed=f.Command with{IntentId=Guid.NewGuid().ToString("N"),Reason="Different decision"};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Data.Owner.RecordCompanyInvoiceDecisionAsync(changed,f.Authority,f.Current));
        f.Command=retained!.Restore(f.Command.Company,"reviewer");var result=await f.Execute();Assert.Equal("APPROVED",result.Invoice.STATUS);Assert.Single(f.Data.Data.Native.Rows.OfType<SALES_INVOICE_REVIEW_INTENT>());
    }
    [Fact]public async Task UncertainDecisionCommitRecoversWithoutSourceSubstitutionOrAnotherWrite()
    {
        var f=new Fixture();await f.Prepare();f.OnUpdate=()=>f.Data.Data.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());Assert.Equal("APPROVED",f.Stored.STATUS);
        f.Data.Data.Native.FailCommit=false;f.OnUpdate=null;f.Advanced=true;var writes=f.Data.Data.Native.Writes;var result=await f.Execute();Assert.Equal("APPROVED",result.Invoice.STATUS);Assert.Equal(writes,f.Data.Data.Native.Writes);Assert.Equal(1,f.Updates);
    }
    [Theory][InlineData("total")][InlineData("creator")][InlineData("buyer")][InlineData("receipt")][InlineData("proof")][InlineData("status")]
    public async Task ReviewedInvoiceCannotLaunderChangedOriginalMoneyOwnershipOrOutcome(string fault)
    {
        var f=new Fixture();await f.Prepare();var result=await f.Execute();var row=CompanyInventorySourceReview.Copy(result.Invoice);
        if(fault=="total")row.TOTAL_AMOUNT+=1m;if(fault=="creator")row.ROW_CREATED_BY="other";if(fault=="buyer")row.CUSTOMER_BA_ID="other";if(fault=="proof")row.REVIEW_WORKFLOW_JSON="{}";if(fault=="status")row.STATUS="PAID";
        var receipt=CompanyInventorySourceReview.Copy(result.Receipt);if(fault=="receipt")receipt.INVOICE_JSON="{}";Assert.Throws<InvalidOperationException>(()=>(result with{Invoice=row,Receipt=receipt}).Require(f.Data.Data.Command));
    }
    [Theory][InlineData("issue")][InlineData("empty")][InlineData("changed")]
    public async Task ExplicitEvidenceAndExactDisplayedReviewAreRequiredBeforeNativeJournal(string fault)
    {
        var f=new Fixture();await f.Prepare();if(fault=="issue")f.Command=f.Command with{TaxDisposition="ISSUE"};if(fault=="empty")f.Command=f.Command with{EntitlementEvidence=""};if(fault=="changed")f.Advanced=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());Assert.Empty(f.Data.Data.Native.Rows.OfType<SALES_INVOICE_REVIEW_INTENT>());Assert.Equal(0,f.Updates);
    }
    [Fact]public async Task RepositoryAuditCannotChangeFinancialFactsBeforeIndependentDisposition()
    {
        var f=new Fixture();await f.Prepare();f.Data.Data.AlterDuringAudit=row=>{if(row is INVOICE invoice)invoice.TOTAL_AMOUNT+=1m;};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());Assert.Equal(0,f.Updates);Assert.Equal("DRAFT",f.Stored.STATUS);Assert.Single(f.Data.Data.Native.Rows.OfType<SALES_INVOICE_REVIEW_INTENT>());
    }
    [Fact]public async Task OperationValidationReuseStillRejectsChangedMutableEvidenceAndAnotherCompany()
    {
        var f=new Fixture();await f.Prepare();
        using(CompanySalesInvoiceValidation.Enter())
        {
            f.Command.Require();var row=CompanyInventorySourceReview.Copy(f.Original.Invoice);_=CompanySalesInvoiceDraftProof.Require(f.Command.Company,row);
            Assert.True(f.Command.Review.HasCurrentOriginalSource);f.Advanced=true;Assert.False((await f.Current()).HasCurrentOriginalSource);f.Advanced=false;Assert.True((await f.Current()).HasCurrentOriginalSource);
            Assert.Throws<InvalidOperationException>(()=>CompanySalesInvoiceDraftProof.Require(new(f.Command.Company.TenantId,"another"),row));
            f.Command.Review.Original.Invoice.TOTAL_AMOUNT+=1m;Assert.Throws<InvalidOperationException>(f.Command.Require);
        }
        Assert.Throws<InvalidOperationException>(f.Command.Require);
    }
    [Fact]public async Task MissingCommittedReviewerJournalRefusesRecoveryWithoutRecreatingDecision()
    {
        var f=new Fixture();await f.Prepare();await f.Execute();f.Data.Data.Native.Rows.RemoveAll(row=>row is SALES_INVOICE_REVIEW_INTENT);var writes=f.Data.Data.Native.Writes;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());Assert.Equal(writes,f.Data.Data.Native.Writes);Assert.Equal(1,f.Updates);
    }
    [Theory][InlineData("missing")][InlineData("nullable")][InlineData("size")]
    public async Task DispositionSchemaMustBeConfirmedBeforeRetainingOrUpdatingNativeDecision(string fault)
    {
        var f=new Fixture();await f.Prepare();var schema=new ClassCreator(f.Data.Data.Native.Editor.Object).ConvertToEntityStructure(typeof(INVOICE));var field=schema.Fields.Single(value=>value.FieldName=="REVIEW_WORKFLOW_JSON");
        if(fault=="missing")schema.Fields.Remove(field);if(fault=="nullable")field.AllowDBNull=false;if(fault=="size"){field.Size=10;field.Size1=10;}
        f.Data.Data.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(row=>row.EntityName=="INVOICE"),true)).Returns(schema);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());Assert.Empty(f.Data.Data.Native.Rows.OfType<SALES_INVOICE_REVIEW_INTENT>());Assert.Equal(0,f.Updates);
    }
    [Theory][InlineData("read-only")][InlineData("prepare")][InlineData("role")][InlineData("field")]
    public async Task ActualApiRequiresDistinctDispositionRoleGrantAndFieldWrite(string fault)
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Data.Start(role:fault!="role",permissions:["Accounting.View",ReadGrant,fault=="prepare"?"Accounting.PrepareSalesInvoice":fault=="read-only"?ReadGrant:WriteGrant]);
        host.Context.Actor="reviewer";host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reviewer");f.Data.AssetDenied=fault=="field";
        var response=await host.Client.PostAsync("api/accounting/traditional/company-invoices/decisions",new StringContent(JsonSerializer.Serialize(f.Command),System.Text.Encoding.UTF8,"application/json"));Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);Assert.Equal(0,f.Updates);
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task ActualApiAndTypedClientRecordAndReadOriginalNativeDecision(string decision)
    {
        var f=new Fixture();await f.Prepare(decision);await using var host=await f.Data.Start(permissions:["Accounting.View",ReadGrant,WriteGrant]);host.Context.Actor="reviewer";host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reviewer");
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"reviewer",new(f.Command.Company,["ConfiguredCompanyRole"],["Accounting.View",ReadGrant,WriteGrant]));var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);
        var source=await client.GetCompanySalesInvoiceReviewAsync(f.Command.InvoiceId);f.Command=f.Command with{Review=source};var result=await client.RecordCompanySalesInvoiceDecisionAsync(f.Command);Assert.Equal(decision=="APPROVE"?"APPROVED":"REJECTED",result.Invoice.STATUS);
        var retained=await client.GetCompanySalesInvoiceReviewIntentAsync(f.Command.InvoiceId);Assert.Equal(JsonSerializer.Serialize(f.Command),JsonSerializer.Serialize(retained!.Restore(f.Command.Company,"reviewer")));
        var current=await client.GetCompanySalesInvoiceReviewAsync(f.Command.InvoiceId);Assert.False(current.CanReview);Assert.Equal(result.Invoice.STATUS,current.Original.Invoice.STATUS);Assert.Equal(1,f.Updates);Assert.Equal(5,f.Data.Data.Native.Writes);
    }
    private sealed class Authentication:AuthenticationStateProvider
    {
        public string Actor="reviewer";
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,Actor),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));
        public void Change(string actor){Actor=actor;NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private sealed class Components:IComponentActivator
    {
        public Beep.OilandGas.Web.Components.Workflow.SalesInvoiceReviewWorkbench? Workbench;
        public IComponent CreateInstance(Type type){var value=(IComponent)Activator.CreateInstance(type)!;if(value is Beep.OilandGas.Web.Components.Workflow.SalesInvoiceReviewWorkbench workbench)Workbench=workbench;return value;}
    }
    [Theory][InlineData("success")][InlineData("reject")][InlineData("missing")][InlineData("issue")][InlineData("unknown")][InlineData("recreated")][InlineData("failed-write")][InlineData("auth")]
    public async Task ActualReviewerFormRequiresExplicitIndependentEvidenceAndRetainsExactDecisionThroughRecovery(string mode)
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Data.Start(permissions:["Accounting.View",ReadGrant,WriteGrant]);host.Context.Actor="reviewer";host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reviewer");
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"reviewer",new(f.Command.Company,["ConfiguredCompanyRole"],["Accounting.View",ReadGrant,WriteGrant]));var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);var auth=new Authentication();
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);services.AddScoped<CompanySalesInvoiceReviewWorkspaceState>();
        services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var state=provider.GetRequiredService<CompanySalesInvoiceReviewWorkspaceState>();string? retainedBody=null;
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"Invoice",f.Command.InvoiceId}});var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.SalesInvoiceReview>(parameters);
            var workbench=((Components)provider.GetRequiredService<IComponentActivator>()).Workbench!;
            void Field(string name,object? value)=>workbench.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(workbench,value);
            void Refresh()=>workbench.GetType().GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(workbench,null);
            async Task Invoke(string name){await (Task)workbench.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(workbench,null)!;Refresh();}
            Assert.NotNull(state.Source);Assert.True(state.CanDecide);Assert.Contains("Record independent invoice decision",root.ToHtmlString());Assert.Null(workbench.GetType().GetField("_decision",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(workbench));
            Field("_decision",mode=="reject"?"REJECT":"APPROVE");Field("_taxDisposition",mode=="issue"?"ISSUE":"CONFIRMED");Field("_taxEvidence",mode=="missing"?"":"Independent tax evidence");Field("_chargesDisposition","CONFIRMED");Field("_chargesEvidence","Independent charges evidence");Field("_entitlementDisposition","CONFIRMED");Field("_entitlementEvidence","Independent entitlement evidence");Field("_reason","Original explicit reviewer reason");Refresh();
            if(mode is "unknown" or "recreated")f.OnUpdate=()=>f.Data.Data.Native.FailCommit=true;if(mode=="failed-write")f.FailWrite=true;if(mode=="auth")f.OnUpdate=()=>auth.Change("other-account");
            await Invoke("Record");
            if(mode is "missing" or "issue"){Assert.Null(state.Pending);Assert.Equal("DRAFT",f.Stored.STATUS);Assert.Equal(0,f.Updates);return;}
            if(mode=="auth")
            {
                Assert.Null(state.Source);Assert.Null(state.Pending);Assert.DoesNotContain(f.Original.Invoice.INVOICE_NUMBER,root.ToHtmlString());auth.Change("reviewer");f.OnUpdate=null;await Invoke("Reload");
            }
            else if(mode is "unknown" or "recreated" or "failed-write")
            {
                Assert.NotNull(state.Pending);Assert.Null(state.Source);retainedBody=JsonSerializer.Serialize(state.Pending);Assert.Contains("original reviewer command remains unconfirmed",root.ToHtmlString());
                f.Data.Data.Native.FailCommit=false;f.OnUpdate=null;f.FailWrite=false;
                if(mode=="recreated")
                {
                    workbench.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.SalesInvoiceReview>(parameters);workbench=((Components)provider.GetRequiredService<IComponentActivator>()).Workbench!;
                }
                else{Field("_reason","Changed form must not replace original intent");await Invoke("Retry");}
            }
            Assert.Null(state.Pending);Assert.NotNull(state.Source);Assert.Contains("Recorded independent invoice disposition",root.ToHtmlString());Assert.False(state.Source!.CanReview);
        });
        if(mode is "missing" or "issue"){Assert.Empty(f.Data.Data.Native.Rows.OfType<SALES_INVOICE_REVIEW_INTENT>());return;}
        Assert.Equal(mode=="reject"?"REJECTED":"APPROVED",f.Stored.STATUS);var command=Assert.Single(f.Data.Data.Native.Rows.OfType<SALES_INVOICE_REVIEW_INTENT>()).Restore(f.Command.Company,"reviewer");Assert.Equal("Original explicit reviewer reason",command.Reason);if(retainedBody is not null)Assert.Equal(retainedBody,JsonSerializer.Serialize(command));Assert.Equal(mode=="failed-write"?2:1,f.Updates);
        Assert.Equal(JsonSerializer.Serialize(f.Original.Receipt),JsonSerializer.Serialize(Assert.Single(f.Data.Data.Native.Rows.OfType<SALES_INVOICE_OUTCOME>())));
        // A fresh circuit can observe a known decision using read authority alone, without submitting another command.
        using var readonlyCompany=new CompanyClientContext();readonlyCompany.TrySelect(readonlyCompany.Clear(),"reviewer",new(f.Command.Company,["ConfiguredCompanyRole"],["Accounting.View",ReadGrant]));
        using var readonlyState=new CompanySalesInvoiceReviewWorkspaceState(readonlyCompany);readonlyState.AttachOwner(new object());readonlyState.Admit((await auth.GetAuthenticationStateAsync()).User);host.Context.Permissions=["Accounting.View",ReadGrant];
        await readonlyState.LoadAsync(f.Command.InvoiceId,CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,readonlyCompany));Assert.NotNull(readonlyState.Source);Assert.Null(readonlyState.Pending);Assert.False(readonlyState.CanDecide);
    }
}
#endif
