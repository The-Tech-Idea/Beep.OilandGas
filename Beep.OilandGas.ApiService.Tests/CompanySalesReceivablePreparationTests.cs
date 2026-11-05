#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Text.Json;
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanySalesReceivablePreparationTests
{
    internal sealed class Fixture
    {
        public readonly CompanySalesInvoiceDecisionTests.Fixture Invoice=new();
        public ARService Owner=null!,Legacy=null!;
        public CompanySalesReceivableCommand Command=null!;
        public bool Denied,Advanced,FailIntent,FailDraft,FailReceipt,CorruptDraft,CorruptReceipt,UncertainDraft;
        public Action<object>? Audit;
        public Action? OnDraft,OnReceipt;
        public int Reads;
        public async Task Prepare()
        {
            await Invoice.Prepare();await Invoice.Execute();var native=Invoice.Data.Data.Native;
            var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(It.IsAny<string>())).ReturnsAsync((string table)=>new List<string>{table+"_ID"});
            var actual=new CommonColumnHandler();var columns=new Mock<ICommonColumnHandler>();
            columns.Setup(value=>value.PrepareForInsert(It.IsAny<Beep.OilandGas.PPDM.Models.IPPDMEntity>(),It.IsAny<string>())).Callback((Beep.OilandGas.PPDM.Models.IPPDMEntity row,string actor)=>{actual.PrepareForInsert(row,actor);Audit?.Invoke(row);});
            var basis=(AccountingBasisPostingService)typeof(InvoiceService).GetField("_basisPosting",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(Invoice.Data.Data.Owner)!;
            Owner=new(native.Editor.Object,columns.Object,Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,basis,Mock.Of<IFailureReporter>(),NullLogger<ARService>.Instance,resolveConnection:()=>Task.FromResult(native.Connection));
            Legacy=new(native.Editor.Object,columns.Object,Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,basis,Mock.Of<IFailureReporter>(),NullLogger<ARService>.Instance);
            var types=native.Rows.Select(row=>row.GetType()).Concat([typeof(AR_INVOICE),typeof(SALES_RECEIVABLE_INTENT),typeof(SALES_RECEIVABLE_OUTCOME)]).Distinct().ToDictionary(type=>type.Name,type=>type);
            native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure schema,bool refresh)=>new ClassCreator(native.Editor.Object).ConvertToEntityStructure(types[schema.EntityName]));
            native.Source.Setup(value=>value.InsertEntity("SALES_RECEIVABLE_INTENT",It.IsAny<object>())).Returns((string _,object value)=>{
                native.Writes++;if(FailIntent)return new ErrorsInfo{Flag=Errors.Failed};native.Rows.Add(CompanyInventorySourceReview.Copy((SALES_RECEIVABLE_INTENT)value));return new ErrorsInfo{Flag=Errors.Ok};});
            native.Source.Setup(value=>value.InsertEntity("AR_INVOICE",It.IsAny<object>())).Returns((string _,object value)=>{
                native.Writes++;if(FailDraft)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((AR_INVOICE)value);if(CorruptDraft)row.TOTAL_AMOUNT+=1m;native.Rows.Add(row);OnDraft?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            native.Source.Setup(value=>value.InsertEntity("SALES_RECEIVABLE_OUTCOME",It.IsAny<object>())).Returns((string _,object value)=>{
                native.Writes++;if(FailReceipt)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((SALES_RECEIVABLE_OUTCOME)value);if(CorruptReceipt)row.AR_INVOICE_ID=Guid.NewGuid().ToString("N");native.Rows.Add(row);if(UncertainDraft)native.FailCommit=true;OnReceipt?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            var source=await Current();Command=new(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),source,"Explicit AR document","Prepare the reviewed approved invoice receivable proposal");
            native.Writes=0;native.Transactions.Clear();
        }
        public Task Guard(){if(Denied)throw new UnauthorizedAccessException("Current source authority revoked.");return Invoice.Authority();}
        public async Task<CompanySalesReceivableSource> Current()
        {
            Reads++;await Guard();var invoice=await Invoice.Current();var rows=await Owner.GetCompanySalesReceivablesAsync(invoice.Current.Company,invoice.Current.FieldId,invoice.InvoiceId,Guard);
            var source=CompanySalesReceivableSource.Create(invoice,rows);if(Advanced)source=source with{BindingRevision=new string('C',64),Revision="changed"};return source;
        }
        public Task<CompanySalesReceivableOutcome> Execute()=>Owner.PrepareCompanySalesReceivableAsync(Command,Guard,Current);
    }
    [Fact]public async Task CanonicalSourceAndNativeDraftKeepApprovedInvoiceMoneyAndCreateNoPosting()
    {
        using var operation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();var original=JsonSerializer.Serialize(f.Command.Source.Invoice);
        var outcome=await f.Execute();outcome.Require(f.Command);Assert.Equal("DRAFT",outcome.Receivable.STATUS);Assert.Equal(f.Command.Source.Invoice.TOTAL_AMOUNT,outcome.Receivable.TOTAL_AMOUNT);
        Assert.Equal(0m,outcome.Receivable.PAID_AMOUNT);Assert.Equal(f.Command.Source.Invoice.TOTAL_AMOUNT,outcome.Receivable.BALANCE_DUE);Assert.Equal(f.Command.Source.Invoice.CURRENCY_CODE,outcome.Receivable.CURRENCY_CODE);
        Assert.Equal(original,JsonSerializer.Serialize(f.Command.Source.Invoice));Assert.Equal(3,f.Invoice.Data.Data.Native.Writes);Assert.Equal(new[]{"begin","commit","begin","commit"},f.Invoice.Data.Data.Native.Transactions);
        var reads=f.Reads;f.Advanced=true;var retry=await f.Execute();Assert.Equal(JsonSerializer.Serialize(outcome),JsonSerializer.Serialize(retry));Assert.Equal(3,f.Invoice.Data.Data.Native.Writes);Assert.Equal(reads,f.Reads);
        Assert.DoesNotContain(f.Invoice.Data.Data.Native.Rows,row=>row.GetType().Name is "AR_PAYMENT" or "GL_ENTRY" or "JOURNAL_ENTRY");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.PrepareCompanySalesReceivableAsync(f.Command with{Reason="Different retained body"},f.Guard,f.Current));Assert.Equal(3,f.Invoice.Data.Data.Native.Writes);
    }
    [Theory][InlineData("intent")][InlineData("draft")][InlineData("receipt")][InlineData("draft-readback")][InlineData("receipt-readback")]
    public async Task FailedWritesAndReadbackRollBackOwnedDraftButPreserveOriginalIntent(string fault)
    {
        using var operation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();f.FailIntent=fault=="intent";f.FailDraft=fault=="draft";f.FailReceipt=fault=="receipt";f.CorruptDraft=fault=="draft-readback";f.CorruptReceipt=fault=="receipt-readback";
        await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);Assert.Empty(f.Invoice.Data.Data.Native.Rows.OfType<AR_INVOICE>());Assert.Empty(f.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_OUTCOME>());
        Assert.Equal(fault=="intent"?0:1,f.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_INTENT>().Count());
        f.FailIntent=f.FailDraft=f.FailReceipt=f.CorruptDraft=f.CorruptReceipt=false;var result=await f.Execute();result.Require(f.Command);Assert.Single(f.Invoice.Data.Data.Native.Rows.OfType<AR_INVOICE>());
    }
    [Theory][InlineData("draft")][InlineData("receipt")][InlineData("intent")]
    public async Task MissingCommittedNativeEvidenceRefusesRetryInsteadOfRecreating(string missing)
    {
        using var operation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();await f.Execute();var native=f.Invoice.Data.Data.Native;var writes=native.Writes;
        native.Rows.RemoveAll(row=>missing=="draft"?row is AR_INVOICE:missing=="receipt"?row is SALES_RECEIVABLE_OUTCOME:row is SALES_RECEIVABLE_INTENT);
        await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);Assert.Equal(writes,native.Writes);
    }
    [Fact]public async Task UnknownNativeCommitReconcilesExactlyWithoutAnotherWrite()
    {
        using var operation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();f.UncertainDraft=true;await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);Assert.Single(f.Invoice.Data.Data.Native.Rows.OfType<AR_INVOICE>());var writes=f.Invoice.Data.Data.Native.Writes;
        f.UncertainDraft=false;f.Invoice.Data.Data.Native.FailCommit=false;var outcome=await f.Execute();outcome.Require(f.Command);Assert.Equal(writes,f.Invoice.Data.Data.Native.Writes);
    }
    [Theory][InlineData("money")][InlineData("number")][InlineData("proof")][InlineData("actor")]
    public async Task RepositoryAuditCannotReplaceFrozenReceivableProposal(string mutation)
    {
        using var operation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();f.Audit=row=>{
            if(row is not AR_INVOICE invoice)return;if(mutation=="money")invoice.TOTAL_AMOUNT+=1;else if(mutation=="number")invoice.INVOICE_NUMBER="Other proposal";else if(mutation=="proof")invoice.PREPARATION_WORKFLOW_JSON="{}";else invoice.ROW_CHANGED_BY="other";};
        await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);Assert.Empty(f.Invoice.Data.Data.Native.Rows.OfType<AR_INVOICE>());Assert.Single(f.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_INTENT>());
    }
    [Theory][InlineData("source")][InlineData("authority")][InlineData("global")][InlineData("source-reservation")]
    public async Task ChangedSourceAuthorityAndCompetingOriginalIdentitiesRefuse(string fault)
    {
        using var operation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();var native=f.Invoice.Data.Data.Native;
        if(fault=="source")f.Advanced=true;else if(fault=="authority")f.Denied=true;
        else if(fault=="global")native.Rows.Add(new AR_INVOICE{AR_INVOICE_ID=f.Command.ReceivableId,OWNER_TENANT_ID="foreign"});
        else native.Rows.Add(SALES_RECEIVABLE_INTENT.Create(f.Command with{IntentId=Guid.NewGuid().ToString("N")},DateTime.UtcNow));
        if(fault=="authority")await Assert.ThrowsAsync<UnauthorizedAccessException>(f.Execute);else await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);
        Assert.DoesNotContain(native.Rows.OfType<AR_INVOICE>(),row=>row.OWNER_TENANT_ID==f.Command.Source.Company.TenantId);
    }
    [Theory][InlineData("total")][InlineData("paid")][InlineData("balance")][InlineData("currency")][InlineData("source")][InlineData("status")]
    public async Task NativeProofRefusesFinancialAndSourceTampering(string mutation)
    {
        using var operation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();var row=CompanySalesReceivableProof.Prepare(f.Command,DateTime.UtcNow);
        if(mutation=="total")row.TOTAL_AMOUNT+=1;else if(mutation=="paid")row.PAID_AMOUNT=1;else if(mutation=="balance")row.BALANCE_DUE=0;else if(mutation=="currency")row.CURRENCY_CODE="USD";else if(mutation=="source")row.SOURCE_INVOICE_ID="other";else row.STATUS="PAID";
        Assert.Throws<InvalidOperationException>(()=>CompanySalesReceivableProof.Require(f.Command.Source.Company,row));
    }
    [Fact]public async Task LegacyCompanylessMethodsRefuseSelectedOwnerAndPartiallyOwnedRows()
    {
        using var operation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();var row=new AR_INVOICE{AR_INVOICE_ID="legacy",CUSTOMER_BA_ID="buyer",ACTIVE_IND="Y",FIELD_ID="protected"};f.Invoice.Data.Data.Native.Rows.Add(row);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.GetInvoiceAsync("legacy"));await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.CreateInvoiceAsync(new(){CustomerBaId="buyer",InvoiceNumber="explicit",InvoiceDate=new(2026,10,1),DueDate=new(2026,10,2),TotalAmount=1},"actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.GetInvoicesByCustomerAsync("buyer",null,null));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.UpdateInvoiceAsync(new(){ArInvoiceId="legacy"},"actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.DeleteInvoiceAsync("legacy","actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.CreatePaymentAsync(new(){CustomerBaId="buyer",PaymentAmount=1},"actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.ApplyPaymentAsync("payment","legacy",1,"actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.GetPaymentsByInvoiceAsync("legacy"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.CreateCreditMemoAsync(new(){ArInvoiceId="legacy",CreditAmount=1},"actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.ApproveInvoiceAsync("legacy","actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.GetARAgingAsync(null));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.GetCustomerSummaryAsync("buyer"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.ApplyPaymentToInvoicesAsync(new(){PaymentId="payment",Applications=[new(){InvoiceId="legacy",Amount=1}]},"actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Legacy.GetInvoiceAsync("legacy",f.Invoice.Data.Data.Native.Connection));await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Legacy.GetPaymentsByInvoiceAsync("legacy",f.Invoice.Data.Data.Native.Connection));Assert.Equal(0,f.Invoice.Data.Data.Native.Writes);
    }
    [Theory][InlineData("source")][InlineData("authority")][InlineData("financial")]
    public async Task PostInsertClosureRefusesChangedEvidenceAndRollsBackDraftAndReceipt(string change)
    {
        using var operation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();
        f.OnReceipt=()=>{if(change=="source")f.Advanced=true;else if(change=="authority")f.Denied=true;else Assert.Single(f.Invoice.Data.Data.Native.Rows.OfType<AR_INVOICE>()).BALANCE_DUE=0;};
        if(change=="authority")await Assert.ThrowsAsync<UnauthorizedAccessException>(f.Execute);else await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);
        Assert.Empty(f.Invoice.Data.Data.Native.Rows.OfType<AR_INVOICE>());Assert.Empty(f.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_OUTCOME>());Assert.Single(f.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_INTENT>());
    }
    [Theory][InlineData("receiving-source")][InlineData("billing-policy")]
    public async Task ChangedCurrentPhysicalOrPolicyEvidenceCannotPrepareReceivable(string change)
    {
        using var operation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();
        if(change=="receiving-source")f.Invoice.Data.Data.Current.Delivery.Source.Source.Source.Rows[0].ROW_CHANGED_BY="changed source audit";
        else f.Invoice.Data.Data.Current.Policies.Records[0].ROW_CHANGED_BY="changed policy audit";
        await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);Assert.Equal(0,f.Invoice.Data.Data.Native.Writes);Assert.Empty(f.Invoice.Data.Data.Native.Rows.OfType<SALES_RECEIVABLE_INTENT>());
    }
    [Theory][InlineData("key")][InlineData("owner")][InlineData("proof")][InlineData("money")][InlineData("receipt")]
    public async Task NativePreparationRefusesInsufficientBeepSchemaBeforeIntentWrite(string missing)
    {
        using var operation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();var native=f.Invoice.Data.Data.Native;
        native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(schema=>schema.EntityName=="AR_INVOICE" || schema.EntityName=="SALES_RECEIVABLE_OUTCOME"),true)).Returns((EntityStructure requested,bool refresh)=>{
            var type=requested.EntityName=="AR_INVOICE"?typeof(AR_INVOICE):typeof(SALES_RECEIVABLE_OUTCOME);var schema=new ClassCreator(native.Editor.Object).ConvertToEntityStructure(type);
            if(requested.EntityName=="AR_INVOICE"){
                if(missing=="key")schema.Fields.Single(field=>field.IsKey).IsKey=false;
                if(missing=="owner")schema.Fields.RemoveAll(field=>field.FieldName=="OWNER_TENANT_ID");
                if(missing=="proof"){var field=schema.Fields.Single(field=>field.FieldName=="PREPARATION_WORKFLOW_JSON");field.Size=field.Size1=100;}
                if(missing=="money")schema.Fields.Single(field=>field.FieldName=="TOTAL_AMOUNT").Fieldtype=typeof(double).FullName;
            }else if(missing=="receipt")schema.Fields.RemoveAll(field=>field.FieldName=="RECEIVABLE_JSON");return schema;});
        await Assert.ThrowsAsync<InvalidOperationException>(f.Execute);Assert.Equal(0,native.Writes);Assert.Empty(native.Rows.OfType<SALES_RECEIVABLE_INTENT>());
    }
    [Fact]public void ReceivableBusinessModelsAndSharedContractsHaveCanonicalOwnershipAndNullableMigrationMetadata()
    {
        Assert.Equal(typeof(CompanySalesReceivableSource).Assembly,typeof(AR_INVOICE).Assembly);Assert.Equal(typeof(AR_INVOICE).Assembly,typeof(CreateARInvoiceRequest).Assembly);
        ModuleSchemaBoundary.Validate([typeof(AR_INVOICE),typeof(SALES_RECEIVABLE_INTENT),typeof(SALES_RECEIVABLE_OUTCOME)]);
        var schema=new ClassCreator(Mock.Of<TheTechIdea.Beep.Editor.IDMEEditor>()).ConvertToEntityStructure(typeof(AR_INVOICE));Assert.Equal("AR_INVOICE_ID",Assert.Single(schema.Fields,field=>field.IsKey).FieldName);
        var proof=Assert.Single(schema.Fields,field=>field.FieldName=="PREPARATION_WORKFLOW_JSON");Assert.True(proof.AllowDBNull);Assert.Equal(CompanySalesReceivableProof.MaximumLength,proof.Size1);
        var json=JsonSerializer.Serialize(new AR_INVOICE());Assert.DoesNotContain("OWNER_TENANT_ID",json);Assert.DoesNotContain("PREPARATION_WORKFLOW_JSON",json);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task RetiredArHttpPathsChallengeAnonymousAndRefuseAuthenticatedCompanylessAccess(bool authenticated)
    {
        await using var host=await AccountingSourceHttpTests.Start(authenticate:authenticated);
        var response=await host.Client.GetAsync("api/accounting/traditional/ar/legacy?connectionName=foreign");Assert.Equal(authenticated?HttpStatusCode.Conflict:HttpStatusCode.Unauthorized,response.StatusCode);
        response=await host.Client.PostAsJsonAsync("api/accounting/traditional/ar",new CreateARInvoiceRequest{InvoiceNumber="explicit",CustomerBaId="buyer",InvoiceDate=new(2026,10,1),DueDate=new(2026,10,2),TotalAmount=1});Assert.Equal(authenticated?HttpStatusCode.Conflict:HttpStatusCode.Unauthorized,response.StatusCode);
    }
}
#endif
