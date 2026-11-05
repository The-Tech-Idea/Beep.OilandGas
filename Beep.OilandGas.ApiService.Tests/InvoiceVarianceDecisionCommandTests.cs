using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceVarianceDecisionCommandTests
{
    internal sealed class Fixture
    {
        public InvoiceVarianceCommandTests.Fixture Existing=null!;
        public APInvoiceVarianceDecisionReview Review=null!;
        public APInvoiceVarianceDecisionCommand Command=null!;
        public async Task Prepare(bool within=true)
        {
            Existing=await InvoiceVarianceDecisionReviewTests.Recorded(within);Review=APInvoiceVarianceDecisionReview.Create(await InvoiceVarianceDecisionReviewTests.Financial(Existing),Existing.Command.BindingRevision);
            Command=new(Existing.Command.Company,"reviewer","invoice",Existing.Command.IntentId,within?"ACCEPT_WITHIN_POLICY_VARIANCE":"REQUEST_CORRECTION","Explicit independently reviewed variance disposition.",Review.Current.Invoice.Revision,Review.Revision,Review.BindingRevision,Guid.NewGuid().ToString("N"));
        }
        public Task<AP_INVOICE> Record()=>Existing.Existing.ReadOwner.Native.AP.RecordVarianceDecisionAsync(Command,Review,Existing.Existing.ReadOwner.Native.Authority,()=>InvoiceVarianceDecisionReviewTests.Financial(Existing));
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task ControllerDispositionPreservesOriginalProposalAndFinancialValuesAndRetriesOnce(bool within)
    {
        var f=new Fixture();await f.Prepare(within);var proposal=f.Existing.Stored.VARIANCE_PROPOSAL_JSON;var row=await f.Record();var proof=APInvoiceVarianceDecisionProof.Require(f.Command.Company,row);
        Assert.True(proof.Command.Same(f.Command));Assert.Equal(proposal,row.VARIANCE_PROPOSAL_JSON);Assert.Equal("DRAFT",row.STATUS);Assert.Equal(2.01m,row.TOTAL_AMOUNT);
        Assert.Equal("reviewer",row.ROW_CHANGED_BY);Assert.Equal("actor",proof.OriginalInvoice.ROW_CHANGED_BY);Assert.Equal("source-maker",row.ROW_CREATED_BY);
        Assert.True(APInvoiceVarianceProof.Require(f.Command.Company,row).Command.Same(f.Existing.Command));new APInvoiceVarianceDecisionResult(f.Command,row).Require(new(f.Command,f.Review));
        var retry=await f.Record();Assert.Equal(row.VARIANCE_DECISION_JSON,retry.VARIANCE_DECISION_JSON);Assert.Equal(2,f.Existing.Existing.ReadOwner.Native.Native.Writes);
        await Assert.ThrowsAsync<InvalidOperationException>(async()=>APInvoiceVarianceDecisionReview.Create(await InvoiceVarianceDecisionReviewTests.Financial(f.Existing),f.Command.BindingRevision));
        await f.Existing.Record();Assert.Equal(2,f.Existing.Existing.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("intent")][InlineData("actor")][InlineData("proposal")][InlineData("binding")][InlineData("review")][InlineData("source")][InlineData("reason")][InlineData("disposition")]
    public async Task ChangedDisplayedIntentOrInvalidChoiceCannotOpenTransaction(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.ReadOwner.Native.Native;n.Transactions.Clear();
        f.Command=fault switch{"intent"=>f.Command with{IntentId="invalid"},"actor"=>f.Command with{ActorUserId="actor"},"proposal"=>f.Command with{ProposalIntentId=Guid.NewGuid().ToString("N")},"binding"=>f.Command with{BindingRevision=new string('C',64)},"review"=>f.Command with{ExpectedReviewRevision=new string('C',64)},"source"=>f.Command with{ExpectedInvoiceRevision=new string('C',64)},"reason"=>f.Command with{Reason="bad\nreason"},_=>f.Command with{Disposition="APPROVED"}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Empty(n.Transactions);Assert.Equal(1,n.Writes);
    }
    [Fact] public async Task OutOfPolicyProposalCannotBeAcceptedByController()
    {var f=new Fixture();await f.Prepare(false);f.Command=f.Command with{Disposition="ACCEPT_WITHIN_POLICY_VARIANCE"};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(1,f.Existing.Existing.ReadOwner.Native.Native.Writes);}
    [Theory][InlineData("header")][InlineData("line")][InlineData("purchase")][InlineData("contributor")][InlineData("policy")][InlineData("receiver")][InlineData("foreign")][InlineData("duplicate")]
    public async Task FullFreshEvidenceAndGlobalInvoiceIdentityCannotDriftBeforeWrite(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.ReadOwner.Native;
        if(fault=="header")f.Existing.Stored.TOTAL_AMOUNT=99m;if(fault=="line")n.Line.ITEM_DESCRIPTION="changed";if(fault=="purchase")n.OrderLine.UNIT_PRICE=99m;
        if(fault=="contributor")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="policy")f.Existing.Existing.Stored.DESCRIPTION="changed";if(fault=="receiver")n.Receipt.RECEIVER_DECISION_JSON=null;
        if(fault=="foreign")f.Existing.Stored.OWNER_COMPANY_BA_ID="other";if(fault=="duplicate")n.Native.Rows.Add(PurchaseReceiptDecisionProof.Copy(f.Existing.Stored));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(1,n.Native.Writes);Assert.All(n.Native.Rows.OfType<AP_INVOICE>().Where(row=>row.AP_INVOICE_ID=="invoice"),row=>Assert.Null(row.VARIANCE_DECISION_JSON));
    }
    [Theory][InlineData("missing")][InlineData("type")][InlineData("size")][InlineData("nullable")]
    public async Task NativeBeepMetadataMustContainTheCompleteBoundedDecisionColumn(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.ReadOwner.Native.Native;
        n.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(row=>row.EntityName=="AP_INVOICE"),true)).Returns(()=>
        {var schema=new ClassCreator(n.Editor.Object).ConvertToEntityStructure(typeof(AP_INVOICE));var field=schema.Fields.Single(row=>row.FieldName=="VARIANCE_DECISION_JSON");if(fault=="missing")schema.Fields.Remove(field);if(fault=="type")field.Fieldtype=typeof(decimal).FullName;if(fault=="size")field.Size=100;if(fault=="nullable")field.AllowDBNull=false;return schema;});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(1,n.Writes);
    }
    [Theory][InlineData("write")][InlineData("readback")][InlineData("late-source")][InlineData("late-authority")][InlineData("binding")]
    public async Task FailedWriteReadbackOrLateAuthorityCannotConfirmDisposition(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.ReadOwner.Native;var original=f.Existing.Stored.VARIANCE_PROPOSAL_JSON;n.Native.Transactions.Clear();
        f.Existing.FailWrite=fault=="write";f.Existing.Corrupt=fault=="readback";
        if(fault=="late-source")f.Existing.OnUpdate=()=>n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="late-authority")f.Existing.OnUpdate=()=>n.RevokeAt=n.AuthorityReads+1;if(fault=="binding")f.Existing.OnUpdate=()=>n.Native.Connection="changed";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(new[]{"begin","rollback"},n.Native.Transactions);Assert.Null(f.Existing.Stored.VARIANCE_DECISION_JSON);Assert.Equal(original,f.Existing.Stored.VARIANCE_PROPOSAL_JSON);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task InsufficientIsolationOrBorrowedTransactionCannotRecord(bool borrowed)
    {var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.ReadOwner.Native.Native;n.Borrowed=borrowed;if(!borrowed)n.Isolation=IsolationLevel.ReadCommitted;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(1,n.Writes);}
    [Theory][InlineData("valid")][InlineData("intent")][InlineData("reason")][InlineData("contributor")][InlineData("proof")]
    public async Task UnknownCommitReconcilesExactDecisionOrRefusesReplacementAndDrift(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.ReadOwner.Native.Native;n.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.NotNull(f.Existing.Stored.VARIANCE_DECISION_JSON);n.FailCommit=false;
        if(fault=="intent")f.Command=f.Command with{IntentId=Guid.NewGuid().ToString("N")};if(fault=="reason")f.Command=f.Command with{Reason="another reason"};
        if(fault=="contributor")n.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="proof")f.Existing.Stored.VARIANCE_DECISION_JSON=f.Existing.Stored.VARIANCE_DECISION_JSON!.Replace(f.Command.Reason,"altered history",StringComparison.Ordinal);
        if(fault=="valid")new APInvoiceVarianceDecisionResult(f.Command,await f.Record()).Require(new(f.Command,f.Review));else await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(2,n.Writes);
    }
    [Fact] public async Task NullDecisionHasNoEffectOnTheExistingProposalSerialization()
    {var f=new Fixture();await f.Prepare();Assert.DoesNotContain("VARIANCE_DECISION_JSON",f.Existing.Stored.VARIANCE_PROPOSAL_JSON!);Assert.DoesNotContain("VARIANCE_DECISION_JSON",JsonSerializer.Serialize(f.Existing.Stored));}
    [Fact] public async Task ProviderCommitWithoutDurableInvoiceCannotPublishConfirmation()
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.ReadOwner.Native.Native;n.Transactions.Clear();
        n.Source.Setup(source=>source.GetEntityAsync("AP_INVOICE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table,List<TheTechIdea.Beep.Report.AppFilter> filters)=>
            n.Transactions.LastOrDefault()=="commit"?Array.Empty<object>():n.Rows.Where(row=>row is AP_INVOICE && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(2,n.Writes);
    }
    [Fact] public async Task LostRecordingAuthorityCannotOpenAnotherTransaction()
    {var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.ReadOwner.Native;n.Native.Transactions.Clear();n.RevokeAt=n.AuthorityReads+1;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Record());Assert.Empty(n.Native.Transactions);Assert.Equal(1,n.Native.Writes);}
    [Fact] public async Task LostAuthorityAfterCommitRetainsExactOriginalControllerDecision()
    {
        var f=new Fixture();await f.Prepare();var native=f.Existing.Existing.ReadOwner.Native;var n=native.Native;n.Transactions.Clear();var revoked=false;
        n.Source.Setup(source=>source.GetEntityAsync("AP_INVOICE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table,List<TheTechIdea.Beep.Report.AppFilter> filters)=>
        {
            var rows=n.Rows.Where(row=>row is AP_INVOICE && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
            if(!revoked && n.Transactions.LastOrDefault()=="commit"){revoked=true;native.RevokeAt=native.AuthorityReads+1;}return rows;
        });
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Record());Assert.NotNull(f.Existing.Stored.VARIANCE_DECISION_JSON);native.RevokeAt=0;await f.Record();Assert.Equal(2,n.Writes);
    }
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("source")]
    public async Task ActualUnknownCommitRetainsOneOriginalControllerSubmissionForReconciliation(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.ReadOwner.Native;n.Native.FailCommit=true;
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});
        host.Context.Actor="reviewer";host.Context.Roles=["ConfiguredControllerRole"];host.Context.PolicyBinding=f.Command.BindingRevision;host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.ReviewInvoiceVariance","Accounting.RecordInvoiceVarianceDecision"];
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reviewer");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredControllerRole");
        using var company=InvoiceVarianceDecisionReviewTests.Selected();var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);var input=new APInvoiceVarianceDecisionSubmission(f.Command,f.Review);
        var unconfirmed=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordAPInvoiceVarianceDecisionAsync(input));Assert.Equal(HttpStatusCode.Conflict,unconfirmed.StatusCode);Assert.NotNull(f.Existing.Stored.VARIANCE_DECISION_JSON);n.Native.FailCommit=false;
        if(fault=="grant")host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.ReviewInvoiceVariance"];
        if(fault=="source")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="valid")(await client.RecordAPInvoiceVarianceDecisionAsync(input)).Require(input);
        else{var refused=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordAPInvoiceVarianceDecisionAsync(input));Assert.Equal(fault=="grant"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,refused.StatusCode);}
        Assert.Equal(2,n.Native.Writes);Assert.Equal("DRAFT",f.Existing.Stored.STATUS);
    }
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("review-grant")][InlineData("role")][InlineData("late-grant")][InlineData("binding")][InlineData("source")][InlineData("schema")]
    public async Task ActualApiClientRecordsExactControllerIntentOrRefusesCurrentAuthorityAndEvidence(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.ReadOwner.Native;
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});
        host.Context.Actor="reviewer";host.Context.Roles=["ConfiguredControllerRole"];host.Context.PolicyBinding=f.Command.BindingRevision;host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.ReviewInvoiceVariance","Accounting.RecordInvoiceVarianceDecision"];
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reviewer");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredControllerRole");
        if(fault=="grant")host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.ReviewInvoiceVariance"];
        if(fault=="review-grant")host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.RecordInvoiceVarianceDecision"];
        if(fault=="role")host.Context.Roles=["OtherRole"];if(fault=="binding")host.Context.PolicyBinding=new string('C',64);
        if(fault=="late-grant")host.Context.OnCapture=_=>{if(host.Context.Calls>2)host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.ReviewInvoiceVariance"];};
        if(fault=="source")n.OrderLine.UNIT_PRICE=99m;
        if(fault=="schema")n.Native.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(row=>row.EntityName=="AP_INVOICE"),true)).Returns(new EntityStructure{EntityName="AP_INVOICE",Fields=[]});
        using var company=InvoiceVarianceDecisionReviewTests.Selected();var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);var input=new APInvoiceVarianceDecisionSubmission(f.Command,f.Review);
        if(fault=="valid"){var result=await client.RecordAPInvoiceVarianceDecisionAsync(input);result.Require(input);var retry=await client.RecordAPInvoiceVarianceDecisionAsync(input);Assert.Equal(result.Invoice.VARIANCE_DECISION_JSON,retry.Invoice.VARIANCE_DECISION_JSON);Assert.Equal(2,n.Native.Writes);}
        else{var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordAPInvoiceVarianceDecisionAsync(input));Assert.Equal(fault is "source" or "schema"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,error.StatusCode);Assert.Equal(1,n.Native.Writes);}
    }
    [Theory][InlineData("valid")][InlineData("malformed")][InlineData("clear")]
    public async Task ActualSourcePanelShowsVerifiedControllerHistoryOrExplicitUnavailableState(string scenario)
    {
        var f=new Fixture();await f.Prepare();await f.Record();if(scenario=="malformed")f.Existing.Stored.VARIANCE_DECISION_JSON="unverified";
        using var company=InvoiceVarianceDecisionReviewTests.Selected();using var state=new APInvoiceWorkspaceState(company);
        await state.LoadDetailAsync("invoice",id=>f.Existing.Existing.ReadOwner.Native.AP.GetSourceReviewAsync(f.Command.Company,id,"reviewer"));
        Assert.True(state.HasRecordedVarianceDecisionSource);
        if(scenario=="valid"){state.RecordedVarianceDecision!.OriginalInvoice.TOTAL_AMOUNT=99m;Assert.Equal(2.01m,state.RecordedVarianceDecision!.OriginalInvoice.TOTAL_AMOUNT);}
        if(scenario=="clear"){company.Clear();Assert.False(state.HasRecordedVarianceDecisionSource);Assert.Null(state.RecordedVarianceDecision);}
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Auth.Tests.Fixtures.APInvoiceRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state}}));return WebUtility.HtmlDecode(root.ToHtmlString());});
        if(scenario=="valid"){Assert.Contains("Verified recorded controller variance disposition",html);Assert.Contains("Controller account: reviewer",html);Assert.Contains(f.Command.Reason,html);Assert.Contains(f.Command.IntentId,html);Assert.Contains("require separate evidence",html);}
        else{Assert.DoesNotContain("Verified recorded controller variance disposition",html);Assert.DoesNotContain(f.Command.Reason,html);if(scenario=="malformed")Assert.Contains("Recorded controller disposition verification: Unavailable",html);}
    }
}
#endif
