using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Xunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyProcessStartHttpTests
{
    internal static Task<AccountingSourceHttpTests.Host> Host(CompanyProcessStartTests.Fixture native,bool authenticated=true,bool standardRole=true,string[]? permissions=null)
        =>AccountingSourceHttpTests.Start(authenticate:authenticated,role:standardRole,permissions:permissions??["Accounting.View","Procurement.View","Workflow.Start"],configure:(_,context)=>
        {
            context.FixedProcess=native.Service;context.StartBinding=new string('B',64);context.Preparer="invoice-preparer";
            context.PreparedAt=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc);
        });
    private static readonly CompanyAccountingProcessStartQuery Query=new("definition","AP_INVOICE","invoice");
    [Theory][InlineData("AP_INVOICE","invoice")][InlineData("PURCHASE_ORDER","order")]
    public async Task TypedCatalogListsOnlyRecordedSourceMatchedChoicesAndValidatesEmptyState(string type,string id)
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");native.Definition.ENTITY_TYPE=type;
        await using var host=await Host(native);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View","Workflow.Start"]));
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var review=await client.ReviewCompanyAccountingStartAsync(new("definition",type,id));
        var query=new CompanyAccountingProcessStartCatalogQuery(type,id,review.SourceRevision);
        var catalog=await client.GetCompanyAccountingStartCatalogAsync(query);catalog.Require(new("tenant","company"),"actor",query);
        Assert.Equal("definition",Assert.Single(catalog.Choices).ProcessId);Assert.Equal(review,catalog.Choices[0]);Assert.Equal(0,native.Native.Writes);
        native.Definition.ACTIVE_IND="N";
        var empty=await client.GetCompanyAccountingStartCatalogAsync(query);Assert.Empty(empty.Choices);Assert.Equal(0,native.Native.Writes);
    }
    [Theory][InlineData("stale")][InlineData("permission")][InlineData("provider")]
    public async Task UnconfirmedCatalogNeverReturnsSuccessfulChoices(string fault)
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");await using var host=await Host(native);
        var review=await Review(host);var query=new CompanyAccountingProcessStartCatalogQuery("AP_INVOICE","invoice",review.SourceRevision);
        if(fault=="stale")host.Context.Preparer="changed";
        if(fault=="permission")host.Context.Permissions=["Accounting.View"];
        if(fault=="provider")host.Context.FailProvider=true;
        var response=await host.Client.PostAsJsonAsync("/api/accounting/processes/start-catalog",query);
        Assert.Equal(fault=="permission"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,response.StatusCode);
        var body=await response.Content.ReadAsStringAsync();Assert.DoesNotContain("Choices",body);Assert.DoesNotContain("private provider",body);Assert.Equal(0,native.Native.Writes);
    }
    [Fact] public async Task TypedCompanyClientReviewsSubmitsAndReadsTheRecordedNativeHandoff()
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");await using var host=await Host(native);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Workflow.Start"])));
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var review=await client.ReviewCompanyAccountingStartAsync(Query);
        var intent=review.CreateIntent(Guid.NewGuid().ToString("N"));var receipt=await client.StartCompanyAccountingAsync(intent);receipt.Require(intent);
        var detail=await client.GetCompanyAccountingInstanceAsync(receipt.InstanceId);Assert.NotNull(detail);Assert.Single(detail.History);
        Assert.Equal(receipt.RecordedRevision,(await client.StartCompanyAccountingAsync(intent)).RecordedRevision);Assert.Equal(6,native.Native.Writes);
        company.Clear();await Assert.ThrowsAsync<InvalidOperationException>(()=>client.StartCompanyAccountingAsync(intent));Assert.Equal(6,native.Native.Writes);
    }
    [Fact] public async Task DisplayedSourceRevisionMustStillMatchBeforeReview()
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");await using var host=await Host(native);
        var review=await Review(host);
        host.Context.Preparer="changed-preparer";
        var response=await host.Client.PostAsJsonAsync("/api/accounting/processes/start-review",Query with{ExpectedSourceRevision=review.SourceRevision});
        Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(0,native.Native.Writes);
    }
    [Fact] public async Task CompanyChangeAfterCommitCannotDeliverASuccessfulTypedReceipt()
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");await using var host=await Host(native);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Workflow.Start"])));
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var intent=(await client.ReviewCompanyAccountingStartAsync(Query)).CreateIntent(Guid.NewGuid().ToString("N"));
        var cleared=false;host.Context.OnCapture=_=>{if(!cleared && native.Native.Transactions.LastOrDefault()=="commit"){cleared=true;company.Clear();}};
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>client.StartCompanyAccountingAsync(intent));
        Assert.True(cleared);Assert.Equal(6,native.Native.Writes);Assert.Single(native.Native.Rows.OfType<PROCESS_INSTANCE>());
    }
    [Theory][InlineData("AP_INVOICE","Procurement.View")][InlineData("PURCHASE_ORDER","Accounting.View")]
    public async Task WorkflowStartDoesNotSubstituteForSourceReadPermission(string type,string unrelatedPermission)
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");native.Definition.ENTITY_TYPE=type;
        await using var host=await Host(native,permissions:["Workflow.Start",unrelatedPermission]);
        var response=await host.Client.PostAsJsonAsync("/api/accounting/processes/start-review",new CompanyAccountingProcessStartQuery("definition",type,type=="AP_INVOICE"?"invoice":"order"));
        Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);Assert.Equal(0,host.Context.Reads);Assert.Equal(0,native.Native.Writes);
    }
    private static async Task<CompanyAccountingProcessStartReview> Review(AccountingSourceHttpTests.Host host,CompanyAccountingProcessStartQuery? query=null)
    {
        var response=await host.Client.PostAsJsonAsync("/api/accounting/processes/start-review",query??Query);
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);return (await response.Content.ReadFromJsonAsync<CompanyAccountingProcessStartReview>())!;
    }
    [Theory][InlineData("AP_INVOICE","invoice")][InlineData("PURCHASE_ORDER","order")]
    public async Task ReviewAndSubmissionUseActualOwnedSourceAuditAndConfirmUnchangedRetry(string type,string source)
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");native.Definition.ENTITY_TYPE=type;
        await using var host=await Host(native);var review=await Review(host,new("definition",type,source));review.Require();
        Assert.Equal("invoice-preparer",review.SourcePreparerUserId);Assert.Equal("ConfiguredCompanyRole",review.InitialRole);Assert.Equal(0,native.Native.Writes);
        var intent=review.CreateIntent(Guid.NewGuid().ToString("N"));
        var response=await host.Client.PostAsJsonAsync("/api/accounting/processes/start",intent);Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var receipt=(await response.Content.ReadFromJsonAsync<CompanyAccountingProcessStartReceipt>())!;receipt.Require(intent);
        var retry=await host.Client.PostAsJsonAsync("/api/accounting/processes/start",intent);Assert.Equal(HttpStatusCode.OK,retry.StatusCode);
        Assert.Equal(receipt.RecordedRevision,(await retry.Content.ReadFromJsonAsync<CompanyAccountingProcessStartReceipt>())!.RecordedRevision);Assert.Equal(6,native.Native.Writes);
        var detail=await host.Client.GetAsync("/api/accounting/processes/"+Uri.EscapeDataString(receipt.InstanceId));Assert.Equal(HttpStatusCode.OK,detail.StatusCode);
        host.Context.Resources.VerifyNoOtherCalls();
        Assert.DoesNotContain(host.Context.Source!.Invocations,call=>new[]{"Insert","Update","Delete","BeginTransaction","Commit"}.Any(prefix=>call.Method.Name.StartsWith(prefix,StringComparison.Ordinal)));
    }
    [Theory][InlineData("anonymous",401)][InlineData("standardRole",403)][InlineData("permission",403)]
    public async Task ReviewRequiresAuthenticatedStandardRoleAndAuthoritativeWorkflowStart(string fault,int status)
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");
        await using var host=await Host(native,fault!="anonymous",fault!="standardRole",fault=="permission"?["Accounting.View","Procurement.View"]:null);
        var response=await host.Client.PostAsJsonAsync("/api/accounting/processes/start-review",Query);
        Assert.Equal(status,(int)response.StatusCode);Assert.Equal(0,native.Native.Writes);Assert.Equal(0,host.Context.Reads);
    }
    [Theory][InlineData("preparer")][InlineData("audit")][InlineData("foreign")][InlineData("definition")][InlineData("edited")][InlineData("lineMaker")][InlineData("systemMaker")]
    public async Task UnconfirmedSourceOrDefinitionCannotProduceAStartReview(string fault)
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");await using var host=await Host(native);
        if(fault=="preparer")host.Context.Preparer=null;if(fault=="audit")host.Context.PreparedAt=null;
        if(fault=="foreign")host.Context.ForeignNative=true;if(fault=="definition")native.Definition.ENTITY_TYPE="PURCHASE_ORDER";
        if(fault=="edited")host.Context.SourceEdited=true;if(fault=="lineMaker")host.Context.LineMakerMissing=true;
        if(fault=="systemMaker")host.Context.Preparer="SYSTEM";
        var response=await host.Client.PostAsJsonAsync("/api/accounting/processes/start-review",Query);
        Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(0,native.Native.Writes);
    }
    [Theory][InlineData("company",403)][InlineData("actor",403)][InlineData("binding",403)][InlineData("preparer",409)][InlineData("source",409)][InlineData("definition",409)]
    public async Task SubmittedAssertionsMustMatchTheApiReviewBeforeMutation(string fault,int status)
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");await using var host=await Host(native);
        var intent=(await Review(host)).CreateIntent(Guid.NewGuid().ToString("N"));
        intent=fault switch {"company"=>intent with{Company=new("tenant","foreign")},"actor"=>intent with{ActorUserId="other"},"binding"=>intent with{BindingRevision=new string('C',64)},
            "preparer"=>intent with{SourcePreparerUserId="other"},"source"=>intent with{SourceRevision=new string('C',64)},_=>intent with{DefinitionRevision=new string('C',64)}};
        var response=await host.Client.PostAsJsonAsync("/api/accounting/processes/start",intent);
        Assert.Equal(status,(int)response.StatusCode);Assert.Equal(0,native.Native.Writes);
    }
    [Fact] public async Task RevokedAuthorityInsideTheNativeCommandRollsBackTheHandoff()
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");await using var host=await Host(native);
        var intent=(await Review(host)).CreateIntent(Guid.NewGuid().ToString("N"));
        host.Context.OnCapture=_=>{if(native.Native.Writes>0)host.Context.Permissions=["Accounting.View","Procurement.View"];};
        var response=await host.Client.PostAsJsonAsync("/api/accounting/processes/start",intent);
        Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);Assert.Single(native.Native.Rows);Assert.Contains("rollback",native.Native.Transactions);
    }
    [Fact] public async Task SourceChangeAfterCommitReturnsUnconfirmedAndRetainedIntentCanLaterReconcile()
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");await using var host=await Host(native);
        var intent=(await Review(host)).CreateIntent(Guid.NewGuid().ToString("N"));
        host.Context.OnCapture=_=>{if(native.Native.Transactions.LastOrDefault()=="commit")host.Context.Preparer="changed-preparer";};
        var response=await host.Client.PostAsJsonAsync("/api/accounting/processes/start",intent);
        Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(6,native.Native.Writes);Assert.Single(native.Native.Rows.OfType<PROCESS_INSTANCE>());
        Assert.DoesNotContain("TaskIds",await response.Content.ReadAsStringAsync());
        host.Context.OnCapture=null;host.Context.Preparer="invoice-preparer";
        var retry=await host.Client.PostAsJsonAsync("/api/accounting/processes/start",intent);
        Assert.Equal(HttpStatusCode.OK,retry.StatusCode);Assert.Equal(6,native.Native.Writes);
    }
}
#endif
