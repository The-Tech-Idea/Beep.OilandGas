using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Models.Data.Accounting;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanySalesInvoiceExecutionTests
{
    private sealed class Fixture
    {
        public readonly CompanySalesInvoiceDraftTests.Fixture Source=new();
        public bool FailInvoiceWrite,CorruptInvoice,FailReceiptWrite,CorruptReceipt;
        public Action? OnInvoiceWrite;
        public Action? OnReceiptWrite;
        public int InvoiceWrites,ReceiptWrites,SourceReads;
        public async Task Prepare(string mode="full")
        {
            await Source.Prepare(mode);var native=Source.Native;
            native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(entity=>entity.EntityName=="INVOICE"),true))
                .Returns(()=>new ClassCreator(native.Editor.Object).ConvertToEntityStructure(typeof(INVOICE)));
            native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(entity=>entity.EntityName=="SALES_INVOICE_OUTCOME"),true))
                .Returns(()=>new ClassCreator(native.Editor.Object).ConvertToEntityStructure(typeof(SALES_INVOICE_OUTCOME)));
            native.Source.Setup(value=>value.InsertEntity("INVOICE",It.IsAny<object>())).Returns((string _,object value)=>{
                native.Writes++;InvoiceWrites++;if(FailInvoiceWrite)return new ErrorsInfo{Flag=Errors.Failed};
                var row=CompanyInventorySourceReview.Copy((INVOICE)value);if(CorruptInvoice)row.TOTAL_AMOUNT=1m;native.Rows.Add(row);OnInvoiceWrite?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            native.Source.Setup(value=>value.InsertEntity("SALES_INVOICE_OUTCOME",It.IsAny<object>())).Returns((string _,object value)=>{
                native.Writes++;ReceiptWrites++;if(FailReceiptWrite)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((SALES_INVOICE_OUTCOME)value);
                if(CorruptReceipt)row.INVOICE_REVISION=new string('C',64);native.Rows.Add(row);OnReceiptWrite?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
        }
        public async Task<CompanySalesInvoiceDraftReview> Current()
        {
            SourceReads++;var displayed=Source.Current;
            var rows=await Source.Owner.GetCompanySalesInvoicesForDeliveryAsync(displayed.Company,displayed.FieldId,displayed.TicketId,Source.Authority);
            return CompanySalesInvoiceDraftReview.Create(displayed.Delivery,displayed.Policies,rows,displayed.BindingRevision);
        }
        public Task<CompanySalesInvoiceDraftOutcome> Execute()=>Source.Owner.ExecuteCompanyInvoiceDraftAsync(Source.Command,Source.Authority,Current);
        public INVOICE Stored=>Assert.Single(Source.Native.Rows.OfType<INVOICE>());
    }
    [Theory][InlineData("full",86.415)][InlineData("partial",49.38)]
    public async Task OriginalJournalExecutesOneOwnedNativeDraftWithoutApprovalReceivableOrPosting(string mode,double total)
    {
        var f=new Fixture();await f.Prepare(mode);var result=await f.Execute();result.Require(f.Source.Command);
        Assert.Equal((decimal)total,result.Invoice.TOTAL_AMOUNT);Assert.Equal("DRAFT",result.Invoice.STATUS);Assert.Equal("KWD",result.Invoice.CURRENCY_CODE);
        Assert.Equal(0m,result.Invoice.PAID_AMOUNT);Assert.Equal((decimal)total,result.Invoice.BALANCE_DUE);Assert.Equal(3,f.Source.Native.Writes);Assert.Equal(1,f.InvoiceWrites);Assert.Equal(1,f.ReceiptWrites);
        Assert.Equal(new[]{"begin","commit","begin","commit"},f.Source.Native.Transactions);
        Assert.Empty(f.Source.Native.Rows.OfType<JOURNAL_ENTRY>());Assert.DoesNotContain(f.Source.Native.Rows,row=>row.GetType().Name is "AR_INVOICE" or "INVOICE_PAYMENT");
        var original=JsonSerializer.Serialize(result);Assert.Equal(original,JsonSerializer.Serialize(await f.Execute()));Assert.Equal(3,f.Source.Native.Writes);Assert.Equal(1,f.InvoiceWrites);
        result.Invoice.TOTAL_AMOUNT=999m;Assert.NotEqual(999m,f.Stored.TOTAL_AMOUNT);
    }
    [Fact] public async Task UncertainNativeInvoiceCommitRecoversOriginalOutcomeAfterSourceAdvances()
    {
        var f=new Fixture();await f.Prepare();f.OnInvoiceWrite=()=>f.Source.Native.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());Assert.Equal(1,f.InvoiceWrites);Assert.Equal(3,f.Source.Native.Writes);
        var stored=JsonSerializer.Serialize(f.Stored);f.Source.Native.FailCommit=false;f.OnInvoiceWrite=null;f.Source.AdvanceSource();var reads=f.SourceReads;
        var result=await f.Execute();result.Require(f.Source.Command);Assert.Equal(stored,JsonSerializer.Serialize(result.Invoice));Assert.Equal(reads,f.SourceReads);Assert.Equal(1,f.InvoiceWrites);
    }
    [Theory][InlineData("write")][InlineData("corrupt")][InlineData("authority")][InlineData("binding")][InlineData("source")]
    public async Task InvoiceFailureRollsBackDraftButRetainsDurableOriginalCommand(string fault)
    {
        var f=new Fixture();await f.Prepare();f.FailInvoiceWrite=fault=="write";f.CorruptInvoice=fault=="corrupt";
        f.OnInvoiceWrite=()=>{if(fault=="authority")f.Source.Denied=true;if(fault=="binding")f.Source.Native.Connection="changed";if(fault=="source")f.Source.AdvanceSource();};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Execute());Assert.Empty(f.Source.Native.Rows.OfType<INVOICE>());Assert.Single(f.Source.Native.Rows.OfType<SALES_INVOICE_INTENT>());
        Assert.Equal(JsonSerializer.Serialize(f.Source.Command),JsonSerializer.Serialize(f.Source.Stored.Restore(f.Source.Command.Review.Company,f.Source.Command.Review.ActorUserId)));
        Assert.Equal("rollback",f.Source.Native.Transactions.Last());
    }
    [Fact] public async Task HistoricalRetainedCommandCannotExecuteNewInvoiceAfterSourceChanges()
    {
        var f=new Fixture();await f.Prepare();await f.Source.Save();f.Source.AdvanceSource();
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());Assert.Equal(0,f.InvoiceWrites);Assert.Single(f.Source.Native.Rows.OfType<SALES_INVOICE_INTENT>());
    }
    [Fact] public async Task RepositoryAuditCannotAlterInvoiceFinancialProposal()
    {
        var f=new Fixture();await f.Prepare();f.Source.AlterDuringAudit=row=>{if(row is INVOICE invoice)invoice.TAX_AMOUNT=1m;};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());Assert.Equal(0,f.InvoiceWrites);Assert.Empty(f.Source.Native.Rows.OfType<INVOICE>());Assert.Single(f.Source.Native.Rows.OfType<SALES_INVOICE_INTENT>());
    }
    [Theory][InlineData("foreign-id")][InlineData("intent")][InlineData("number")]
    public async Task NativeInvoiceIdentityIntentAndCompanyNumberCollisionsRefuseBeforeInsertion(string fault)
    {
        var f=new Fixture();await f.Prepare();var command=f.Source.Command;
        var legacy=new INVOICE{INVOICE_ID=fault=="foreign-id"?command.InvoiceId:Guid.NewGuid().ToString("N"),PPDM_GUID=fault=="intent"?command.IntentId:Guid.NewGuid().ToString("N"),
            OWNER_TENANT_ID=f.Source.Current.Company.TenantId,OWNER_COMPANY_BA_ID=fault=="foreign-id"?"another":f.Source.Current.Company.BusinessAssociateId,INVOICE_NUMBER=fault=="number"?command.Facts.InvoiceNumber:"other"};
        f.Source.Native.Rows.Add(legacy);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());Assert.Equal(0,f.InvoiceWrites);Assert.Single(f.Source.Native.Rows.OfType<INVOICE>());
    }
    [Theory][InlineData("missing")][InlineData("size")][InlineData("nullable")][InlineData("business-type")][InlineData("business-size")]
    public async Task InvoiceBeepSchemaMustConfirmSourceProofAndFinancialColumns(string fault)
    {
        var f=new Fixture();await f.Prepare();var schema=new ClassCreator(f.Source.Native.Editor.Object).ConvertToEntityStructure(typeof(INVOICE));
        var proof=schema.Fields.Single(value=>value.FieldName=="BILLING_WORKFLOW_JSON");if(fault=="missing")schema.Fields.Remove(proof);if(fault=="size"){proof.Size=10;proof.Size1=10;}if(fault=="nullable")proof.AllowDBNull=false;
        if(fault=="business-type")schema.Fields.Single(value=>value.FieldName=="TOTAL_AMOUNT").Fieldtype=typeof(double).FullName;
        if(fault=="business-size"){var number=schema.Fields.Single(value=>value.FieldName=="INVOICE_NUMBER");number.Size=10;number.Size1=10;}
        f.Source.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(entity=>entity.EntityName=="INVOICE"),true)).Returns(schema);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());Assert.Equal(0,f.InvoiceWrites);Assert.Empty(f.Source.Native.Rows.OfType<INVOICE>());
    }
    [Theory][InlineData("get")][InlineData("customer")][InlineData("create")][InlineData("update")][InlineData("delete")][InlineData("payment")][InlineData("payments")][InlineData("lines")][InlineData("approve")][InlineData("aging")][InlineData("status")]
    public async Task SelectedCompanyServiceCannotUseAnyCompanylessInvoiceEntryPoint(string operation)
    {
        var f=new Fixture();await f.Prepare();var owner=f.Source.Owner;var calls=f.Source.Native.Source.Invocations.Count;
        Task Action()=>operation switch{"get"=>owner.GetInvoiceAsync("id"),"customer"=>owner.GetInvoicesByCustomerAsync("customer",null,null),"create"=>owner.CreateInvoiceAsync(new(),"actor"),
            "update"=>owner.UpdateInvoiceAsync(new(),"actor"),"delete"=>owner.DeleteInvoiceAsync("id","actor"),"payment"=>owner.RecordPaymentAsync(new(),"actor"),"payments"=>owner.GetInvoicePaymentsAsync("id"),
            "lines"=>owner.GetInvoiceLineItemsAsync("id"),"approve"=>owner.ApproveInvoiceAsync("id","actor"),"aging"=>owner.GetInvoiceAgingAsync(null),_=>owner.GetInvoicePaymentStatusAsync("id")};
        await Assert.ThrowsAsync<InvalidOperationException>(Action);Assert.Equal(calls,f.Source.Native.Source.Invocations.Count);Assert.Equal(0,f.Source.Native.Writes);
    }
    [Theory][InlineData("list",true)][InlineData("id",true)][InlineData("create",true)][InlineData("list",false)][InlineData("id",false)][InlineData("create",false)]
    public async Task LegacyInvoiceHttpRoutesCannotReadWriteOrImmediatelyPost(string operation,bool authenticated)
    {
        await using var host=await AccountingSourceHttpTests.Start(authenticate:authenticated);
        var response=operation=="create"?await host.Client.PostAsJsonAsync("api/accounting/traditional/invoice",new CreateInvoiceRequest{CustomerBaId="buyer",Subtotal=10m}):
            await host.Client.GetAsync(operation=="list"?"api/accounting/traditional/invoice?connectionName=foreign":"api/accounting/traditional/invoice/foreign-id");
        Assert.Equal(authenticated?HttpStatusCode.Conflict:HttpStatusCode.Unauthorized,response.StatusCode);
        if(authenticated)Assert.Contains("company_invoice_workflow_required",await response.Content.ReadAsStringAsync());
    }
    [Theory][InlineData("invoice")][InlineData("receipt")]
    public async Task MissingCommittedInvoiceOrReceiptRefusesRetryInsteadOfRecreatingFinancialDraft(string missing)
    {
        var f=new Fixture();await f.Prepare();f.Source.Native.AfterCommit=()=>{
            if(f.Source.Native.Rows.OfType<INVOICE>().Any() && f.Source.Native.Rows.OfType<SALES_INVOICE_OUTCOME>().Any())
                f.Source.Native.Rows.RemoveAll(row=>missing=="invoice"?row is INVOICE:row is SALES_INVOICE_OUTCOME);
        };
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());f.Source.Native.AfterCommit=null;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());Assert.Equal(1,f.InvoiceWrites);Assert.Equal(1,f.ReceiptWrites);Assert.Equal(3,f.Source.Native.Writes);
    }
    [Theory][InlineData("write")][InlineData("corrupt")][InlineData("authority")][InlineData("binding")][InlineData("source")]
    public async Task OutcomeReceiptFailureRollsBackBothInvoiceAndReceipt(string fault)
    {
        var f=new Fixture();await f.Prepare();f.FailReceiptWrite=fault=="write";f.CorruptReceipt=fault=="corrupt";
        f.OnReceiptWrite=()=>{if(fault=="authority")f.Source.Denied=true;if(fault=="binding")f.Source.Native.Connection="changed";if(fault=="source")f.Source.AdvanceSource();};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Execute());Assert.Empty(f.Source.Native.Rows.OfType<INVOICE>());Assert.Empty(f.Source.Native.Rows.OfType<SALES_INVOICE_OUTCOME>());
        Assert.Single(f.Source.Native.Rows.OfType<SALES_INVOICE_INTENT>());Assert.Equal("rollback",f.Source.Native.Transactions.Last());
    }
    [Fact] public async Task RepositoryAuditCannotChangeOutcomeReceiptSnapshot()
    {
        var f=new Fixture();await f.Prepare();f.Source.AlterDuringAudit=row=>{if(row is SALES_INVOICE_OUTCOME receipt)receipt.INVOICE_JSON="{}";};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());Assert.Equal(1,f.InvoiceWrites);Assert.Equal(0,f.ReceiptWrites);
        Assert.Empty(f.Source.Native.Rows.OfType<INVOICE>());Assert.Empty(f.Source.Native.Rows.OfType<SALES_INVOICE_OUTCOME>());
    }
    [Theory][InlineData("revision")][InlineData("intent")][InlineData("body")][InlineData("owner")][InlineData("maker")][InlineData("invoice")][InlineData("audit")]
    public async Task NativeOutcomeReceiptTamperingCannotAcknowledgeOrExecuteAgain(string fault)
    {
        var f=new Fixture();await f.Prepare();await f.Execute();var receipt=Assert.Single(f.Source.Native.Rows.OfType<SALES_INVOICE_OUTCOME>());
        if(fault=="revision")receipt.INVOICE_REVISION=new string('C',64);if(fault=="intent")receipt.INTENT_REVISION=new string('C',64);
        if(fault=="body")receipt.INVOICE_JSON=receipt.INVOICE_JSON.Insert(1,"\"Unknown\":true,");if(fault=="owner")receipt.OWNER_COMPANY_BA_ID="other";
        if(fault=="maker")receipt.ROW_CREATED_BY="other";if(fault=="invoice")receipt.INVOICE_ID=Guid.NewGuid().ToString("N");
        if(fault=="audit")receipt.ROW_CHANGED_DATE=DateTime.SpecifyKind(receipt.ROW_CREATED_DATE!.Value.AddTicks(1),DateTimeKind.Unspecified);
        receipt.RECORDED_REVISION=SALES_INVOICE_OUTCOME.Revision(receipt);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Execute());Assert.Equal(3,f.Source.Native.Writes);
    }
    [Fact] public void NativeOutcomeReceiptHasCanonicalRequiredBoundedBeepMetadata()
    {
        Assert.Equal(typeof(CompanySalesInvoiceDraftOutcome).Assembly,typeof(SALES_INVOICE_OUTCOME).Assembly);
        Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.ModuleSchemaBoundary.Validate([typeof(SALES_INVOICE_OUTCOME)]);
        var schema=new ClassCreator(Mock.Of<TheTechIdea.Beep.Editor.IDMEEditor>()).ConvertToEntityStructure(typeof(SALES_INVOICE_OUTCOME));
        var key=Assert.Single(schema.Fields,value=>value.IsKey);Assert.Equal("SALES_INVOICE_OUTCOME_ID",key.FieldName);Assert.Equal(32,key.Size1);Assert.False(key.AllowDBNull);
        foreach(var (name,size) in new[]{("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("INVOICE_ID",32),("INTENT_REVISION",64),("INVOICE_REVISION",64),("RECORDED_REVISION",64),("INVOICE_JSON",SALES_INVOICE_OUTCOME.MaximumInvoiceLength)})
        {var field=Assert.Single(schema.Fields,value=>value.FieldName==name);Assert.False(field.AllowDBNull);Assert.Equal(typeof(string).FullName,field.Fieldtype);Assert.Equal(size,field.Size1);}
    }
    [Theory][InlineData("tenant")][InlineData("company")][InlineData("field")][InlineData("ticket")][InlineData("scope")][InlineData("proof")]
    public async Task StandaloneLegacyOwnerCannotReadEvenPartiallyMarkedCompanyInvoice(string marker)
    {
        var f=new Fixture();await f.Prepare();var row=new INVOICE{INVOICE_ID="marked",CUSTOMER_BA_ID="buyer",ACTIVE_IND="Y"};
        if(marker=="tenant")row.OWNER_TENANT_ID="";if(marker=="company")row.OWNER_COMPANY_BA_ID="company";if(marker=="field")row.FIELD_ID="field";
        if(marker=="ticket")row.SOURCE_RUN_TICKET_ID="ticket";if(marker=="scope")row.BILLING_SOURCE_KEY=new string('B',64);if(marker=="proof")row.BILLING_WORKFLOW_JSON="{}";
        f.Source.Native.Rows.Add(row);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Source.LegacyOwner.GetInvoiceAsync("marked","production"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Source.LegacyOwner.GetInvoicesByCustomerAsync("buyer",null,null,"production"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Source.LegacyOwner.GetInvoiceAgingAsync("buyer","production"));Assert.Equal(0,f.Source.Native.Writes);
    }
}
#endif
