#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanySalesInvoiceHttpTests
{
    private const string Route="api/accounting/traditional/company-invoices";
    internal sealed class Fixture
    {
        public readonly CompanySalesInvoiceDraftTests.Fixture Data=new();
        public ProductionAccountingService Production=null!;
        public readonly Mock<IAccessControlService> Assets=new();
        public bool AssetDenied,FailInvoiceWrite,FailReceiptWrite,UncertainInvoiceCommit;
        public Action? OnInvoiceWrite,OnReceiptWrite;
        public int InvoiceWrites,ReceiptWrites,RevenueReads;
        public string Actor=>Data.Current.ActorUserId;
        public string Ticket=>Data.Current.TicketId;
        public async Task Prepare(string mode="full",bool approvePolicy=true)
        {
            await Data.Prepare(mode,approvePolicy,bindingRevision:new string('B',64));var native=Data.Native;var delivery=Data.Current.Delivery;
            native.Rows.RemoveAll(row=>row is not ACCOUNTING_METHOD);
            native.Rows.AddRange(delivery.Source.Source.Source.Rows.Select(CompanyInventorySourceReview.Copy));
            native.Rows.Add(CompanyInventorySourceReview.Copy(delivery.Source.Source.Contract));native.Rows.Add(CompanyInventorySourceReview.Copy(delivery.Source.Allocation));native.Rows.Add(CompanyInventorySourceReview.Copy(delivery.Source.Ticket));
            var types=native.Rows.Select(row=>row.GetType()).Concat([typeof(INVOICE),typeof(SALES_INVOICE_INTENT),typeof(SALES_INVOICE_OUTCOME),typeof(SALES_INVOICE_REVIEW_INTENT)]).Distinct().ToDictionary(type=>type.Name,type=>type,StringComparer.Ordinal);
            native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>new ClassCreator(native.Editor.Object).ConvertToEntityStructure(types[request.EntityName]));
            native.Source.SetupGet(value=>value.ErrorObject).Returns(new ErrorsInfo{Flag=Errors.Ok});
            native.Source.Setup(value=>value.GetEntityAsync("REVENUE_TRANSACTION",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table,List<TheTechIdea.Beep.Report.AppFilter> filters)=>{
                RevenueReads++;var field=filters.Single(filter=>filter.FieldName is "FIELD_ID" or "PROPERTY_ID");
                return native.Rows.Where(row=>row.GetType().Name==table && row.GetType().GetProperty(field.FieldName)?.GetValue(row)?.ToString()==field.FilterValue).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();});
            native.Source.Setup(value=>value.InsertEntity("INVOICE",It.IsAny<object>())).Returns((string _,object value)=>{
                native.Writes++;InvoiceWrites++;if(FailInvoiceWrite)return new ErrorsInfo{Flag=Errors.Failed};native.Rows.Add(CompanyInventorySourceReview.Copy((INVOICE)value));OnInvoiceWrite?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            native.Source.Setup(value=>value.InsertEntity("SALES_INVOICE_OUTCOME",It.IsAny<object>())).Returns((string _,object value)=>{
                native.Writes++;ReceiptWrites++;if(FailReceiptWrite)return new ErrorsInfo{Flag=Errors.Failed};native.Rows.Add(CompanyInventorySourceReview.Copy((SALES_INVOICE_OUTCOME)value));
                if(UncertainInvoiceCommit)native.FailCommit=true;OnReceiptWrite?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(It.IsAny<string>())).ReturnsAsync((string table)=>new List<string>{table+"_ID"});
            Production=new(native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
            Assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),delivery.Source.Source.Source.FieldId,"FIELD","READ",null)).ReturnsAsync(()=>new AccessCheckResponse{HasAccess=!AssetDenied});
        }
        public async Task<AccountingSourceHttpTests.Host> Start(bool authenticated=true,bool role=true,string[]? permissions=null,Action<IServiceCollection,AccountingSourceHttpTests.ReadContext>? configure=null)
        {
            var host=await AccountingSourceHttpTests.Start(authenticate:authenticated,role:role,permissions:permissions ??["Accounting.View","Accounting.PrepareSalesInvoice"],configureAccounting:(services,context)=>{
                context.Actor=Actor;context.PolicyBinding=new string('B',64);services.AddSingleton<IInvoiceService>(Data.Owner);services.AddSingleton(Data.Policy.Owner);services.AddSingleton(Assets.Object);
                services.AddSingleton(new CompanyFieldRevenueReads(Production,context,Assets.Object,()=>Task.FromResult(Data.Native.Connection)));services.AddScoped<CompanySalesInvoiceReads>();configure?.Invoke(services,context);});
            host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",Actor);return host;
        }
        public CompanyClientContext Selected(params string[] grants)
        {
            var selected=new CompanyClientContext();selected.TrySelect(selected.Clear(),Actor,new(Data.Current.Company,["ConfiguredCompanyRole"],new[]{"Accounting.View"}.Concat(grants).ToArray()));return selected;
        }
        public static AccountingServiceClient Client(HttpClient http,CompanyClientContext selected)=>new(new ApiClient(http,Mock.Of<IFailureReporter>(),selected),NullLogger<AccountingServiceClient>.Instance);
        public CompanySalesInvoiceDraftCommand Command(CompanySalesInvoiceWorkspaceReview review)=>new(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),review.Source,CompanySalesInvoiceDraftTests.Facts());
    }
    [Theory][InlineData("full",86.415)][InlineData("partial",49.38)]
    public async Task ActualCompanyApiClientAndSharedProductionOwnerPrepareAndRecoverOneDraft(string mode,double expected)
    {
        var f=new Fixture();await f.Prepare(mode);await using var host=await f.Start();using var selected=f.Selected("Accounting.PrepareSalesInvoice");var client=Fixture.Client(host.Client,selected);
        var review=await client.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket);review.Require(f.Data.Current.Company,f.Actor,f.Ticket);Assert.Null(review.Retained);Assert.Null(review.Outcome);
        Assert.Empty(review.Source.Invoices);Assert.True(review.Source.Delivery.ReviewBillingBasis().HasCompatiblePriceBasis);
        var command=f.Command(review);var result=await client.PrepareCompanySalesInvoiceAsync(command);result.Require(command);Assert.Equal((decimal)expected,result.Invoice.TOTAL_AMOUNT);
        Assert.Equal("DRAFT",result.Invoice.STATUS);Assert.Equal(3,f.Data.Native.Writes);Assert.Equal(1,f.InvoiceWrites);Assert.Equal(1,f.ReceiptWrites);
        var reopened=await client.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket);Assert.Equal(command.IntentId,reopened.Retained!.SALES_INVOICE_INTENT_ID);reopened.Outcome!.Require(command);
        var recovered=await client.GetCompanySalesInvoiceIntentAsync(command.IntentId);recovered.Outcome!.Require(command);
        (await client.PrepareCompanySalesInvoiceAsync(command)).Require(command);Assert.Equal(3,f.Data.Native.Writes);
        Assert.Empty(f.Data.Native.Rows.OfType<JOURNAL_ENTRY>());Assert.DoesNotContain(f.Data.Native.Rows,row=>row.GetType().Name is "AR_INVOICE" or "INVOICE_PAYMENT");
    }
    [Theory][InlineData("anonymous")][InlineData("role")][InlineData("view")][InlineData("asset")][InlineData("binding")][InlineData("company")]
    public async Task InvoiceWorkspaceRequiresCurrentCompanyRoleReadGrantFieldAndBinding(string fault)
    {
        var f=new Fixture();await f.Prepare();f.AssetDenied=fault=="asset";await using var host=await f.Start(authenticated:fault!="anonymous",role:fault!="role",permissions:fault=="view"?[]:["Accounting.View"]);
        if(fault=="binding")host.Context.PolicyBinding="invalid";if(fault=="company")host.Client.DefaultRequestHeaders.Add("X-Test-Company","other");
        var response=await host.Client.GetAsync(Route+"/delivery/"+Uri.EscapeDataString(f.Ticket));
        Assert.Equal(fault=="anonymous"?HttpStatusCode.Unauthorized:fault is "binding" or "company"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,response.StatusCode);Assert.Equal(0,f.Data.Native.Writes);
    }
    [Theory][InlineData("missing")][InlineData("policy-grant")][InlineData("actor")][InlineData("company")][InlineData("binding")][InlineData("source")][InlineData("facts")]
    public async Task InvoiceCommandCannotBorrowPolicyGrantOrRebindTransportedSource(string fault)
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Start(permissions:fault=="missing"?["Accounting.View"]:fault=="policy-grant"?["Accounting.View","Accounting.PrepareBillingPolicy"]:["Accounting.View","Accounting.PrepareSalesInvoice"]);
        using var selected=f.Selected();var client=Fixture.Client(host.Client,selected);var command=f.Command(await client.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket));
        if(fault=="actor")host.Context.Actor="other";if(fault=="company")host.Client.DefaultRequestHeaders.Add("X-Test-Company","other");
        if(fault=="binding")command=command with{Review=command.Review with{BindingRevision=new string('C',64)}};
        if(fault=="source")command=command with{Review=command.Review with{Revision=new string('C',64)}};
        if(fault=="facts")command=command with{Facts=command.Facts with{TaxAmount=null}};
        var response=await host.Client.PostAsJsonAsync(Route+"/drafts",command);
        Assert.Equal(fault is "binding" or "source" or "facts"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,response.StatusCode);Assert.Equal(0,f.Data.Native.Writes);
    }
    [Fact] public async Task RetainedCommandReopensByDeliveryAfterFailedInvoiceWithoutNewIntent()
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Start();using var selected=f.Selected();var client=Fixture.Client(host.Client,selected);
        var command=f.Command(await client.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket));f.FailInvoiceWrite=true;
        await Assert.ThrowsAsync<OilGasApiException>(()=>client.PrepareCompanySalesInvoiceAsync(command));Assert.Single(f.Data.Native.Rows.OfType<SALES_INVOICE_INTENT>());Assert.Empty(f.Data.Native.Rows.OfType<INVOICE>());
        var reopened=await client.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket);Assert.Null(reopened.Outcome);var original=reopened.Retained!.Restore(f.Data.Current.Company,f.Actor);
        Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(original));f.FailInvoiceWrite=false;(await client.PrepareCompanySalesInvoiceAsync(original)).Require(command);Assert.Equal(2,f.InvoiceWrites);
    }
    [Fact] public async Task UncertainInvoiceCommitRecoversCompleteNativeReceiptWithoutSecondWrite()
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Start();using var selected=f.Selected();var client=Fixture.Client(host.Client,selected);
        var command=f.Command(await client.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket));f.UncertainInvoiceCommit=true;
        await Assert.ThrowsAsync<OilGasApiException>(()=>client.PrepareCompanySalesInvoiceAsync(command));Assert.Equal(3,f.Data.Native.Writes);
        f.Data.Native.FailCommit=false;f.UncertainInvoiceCommit=false;var recovered=await client.GetCompanySalesInvoiceIntentAsync(command.IntentId);recovered.Outcome!.Require(command);
        (await client.PrepareCompanySalesInvoiceAsync(command)).Require(command);Assert.Equal(3,f.Data.Native.Writes);
    }
    [Fact] public async Task BillingAccountConsumesOtherAccountsApprovedReceivingWithoutChangingMakerEvidence()
    {
        var f=new Fixture();await f.Prepare();var receivingMaker=f.Actor;await using var host=await f.Start();const string billing="billing-operator";
        host.Context.Actor=billing;host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",billing);
        using var selected=new CompanyClientContext();selected.TrySelect(selected.Clear(),billing,new(f.Data.Current.Company,["ConfiguredCompanyRole"],["Accounting.View","Accounting.PrepareSalesInvoice"]));
        var client=Fixture.Client(host.Client,selected);var workspace=await client.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket);
        Assert.Equal(billing,workspace.Source.ActorUserId);Assert.Equal(receivingMaker,workspace.Source.Delivery.Recorded!.Preparation.ActorUserId);
        var command=f.Command(workspace);var outcome=await client.PrepareCompanySalesInvoiceAsync(command);outcome.Require(command);
        Assert.Equal(billing,outcome.Invoice.ROW_CREATED_BY);Assert.Equal(receivingMaker,command.Review.Delivery.Recorded!.Preparation.ActorUserId);Assert.Equal(3,f.Data.Native.Writes);
    }
    [Theory][InlineData("grant")][InlineData("field")][InlineData("binding")]
    public async Task AuthorityChangeDuringInvoiceWriteRollsBackInvoiceAndReceiptButKeepsOriginalRequest(string mode)
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Start();using var selected=f.Selected();var client=Fixture.Client(host.Client,selected);
        var command=f.Command(await client.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket));f.OnInvoiceWrite=()=>{
            if(mode=="grant")host.Context.Permissions=["Accounting.View"];else if(mode=="field")f.AssetDenied=true;else host.Context.PolicyBinding=new string('C',64);};
        await Assert.ThrowsAsync<OilGasApiException>(()=>client.PrepareCompanySalesInvoiceAsync(command));Assert.Single(f.Data.Native.Rows.OfType<SALES_INVOICE_INTENT>());
        Assert.Empty(f.Data.Native.Rows.OfType<INVOICE>());Assert.Empty(f.Data.Native.Rows.OfType<SALES_INVOICE_OUTCOME>());
        f.OnInvoiceWrite=null;f.AssetDenied=false;host.Context.PolicyBinding=new string('B',64);host.Context.Permissions=["Accounting.View","Accounting.PrepareSalesInvoice"];
        var recovered=await client.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket);Assert.Equal(command.IntentId,recovered.Retained!.SALES_INVOICE_INTENT_ID);
        (await client.PrepareCompanySalesInvoiceAsync(recovered.Retained.Restore(command.Review.Company,command.Review.ActorUserId))).Require(command);Assert.Single(f.Data.Native.Rows.OfType<SALES_INVOICE_INTENT>());
    }
    [Fact] public async Task LateCompanySelectionRejectsTransportOutcomeAndOriginalCompanyCanRecoverWithoutNewWrite()
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Start();using var selected=f.Selected();var client=Fixture.Client(host.Client,selected);
        var command=f.Command(await client.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket));var committed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Data.Native.AfterCommit=()=>{if(f.Data.Native.Rows.OfType<SALES_INVOICE_OUTCOME>().Any())committed.TrySetResult();};f.OnInvoiceWrite=()=>selected.Clear();
        var refused=await Record.ExceptionAsync(()=>client.PrepareCompanySalesInvoiceAsync(command));Assert.True(refused is OperationCanceledException or InvalidOperationException);
        await committed.Task.WaitAsync(TimeSpan.FromSeconds(30));Assert.Equal(3,f.Data.Native.Writes);
        f.OnInvoiceWrite=null;selected.TrySelect(selected.Clear(),f.Actor,new(command.Review.Company,["ConfiguredCompanyRole"],["Accounting.View"]));
        var recovered=await client.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket);recovered.Outcome!.Require(command);Assert.Equal(3,f.Data.Native.Writes);
    }
    [Theory][InlineData("pending")][InlineData("confirmed")][InlineData("missing-receipt")]
    public async Task AnotherBillingAccountCannotTreatRetainedOriginalAsEmptyOrReadItsPrivateRequest(string mode)
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Start();using var originalSelection=f.Selected();var originalClient=Fixture.Client(host.Client,originalSelection);
        var command=f.Command(await originalClient.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket));
        if(mode=="pending"){f.FailInvoiceWrite=true;await Assert.ThrowsAsync<OilGasApiException>(()=>originalClient.PrepareCompanySalesInvoiceAsync(command));}
        else(await originalClient.PrepareCompanySalesInvoiceAsync(command)).Require(command);
        if(mode=="missing-receipt")f.Data.Native.Rows.RemoveAll(row=>row is SALES_INVOICE_OUTCOME);var writes=f.Data.Native.Writes;
        const string next="another-billing-account";host.Context.Actor=next;host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",next);
        using var selected=new CompanyClientContext();selected.TrySelect(selected.Clear(),next,new(command.Review.Company,["ConfiguredCompanyRole"],["Accounting.View","Accounting.PrepareSalesInvoice"]));var client=Fixture.Client(host.Client,selected);
        if(mode=="confirmed")
        {
            var shared=await client.GetCompanySalesInvoiceWorkspaceAsync(f.Ticket);shared.Require(command.Review.Company,next,f.Ticket);Assert.Null(shared.Retained);Assert.Null(shared.Outcome);
            var invoice=Assert.Single(shared.Source.Invoices);Assert.Equal(command.InvoiceId,invoice.InvoiceId);Assert.Equal(command.Review.ActorUserId,invoice.PreparedBy);
        }
        else
        {
            var response=await host.Client.GetAsync(Route+"/delivery/"+Uri.EscapeDataString(f.Ticket));Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);
            Assert.DoesNotContain(command.Facts.EntitlementReviewNotes,await response.Content.ReadAsStringAsync());
        }
        var privateRequest=await host.Client.GetAsync(Route+"/intents/"+command.IntentId);Assert.Equal(HttpStatusCode.NotFound,privateRequest.StatusCode);Assert.Equal(writes,f.Data.Native.Writes);
    }
}
#endif
