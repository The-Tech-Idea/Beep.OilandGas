#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanySalesInvoiceDraftTests
{
    internal sealed class Fixture
    {
        public readonly CompanyMatchingPolicyCommandTests.Fixture Policy=new();
        public PersonaTaskRoutingTests.Fixture Native=>Policy.ReadOwner.Native.Native;
        public InvoiceService Owner=null!;
        public InvoiceService LegacyOwner=null!;
        public CompanySalesInvoiceDraftCommand Command=null!;
        public CompanySalesInvoiceDraftReview Current=null!;
        public bool Denied,FailWrite,Corrupt;
        public Action? OnWrite;
        public Action<Beep.OilandGas.PPDM.Models.IPPDMEntity>? AlterDuringAudit;
        public int CurrentReads;
        public async Task Prepare(string mode="full",bool approve=true,string? bindingRevision=null)
        {
            var delivery=FieldRevenueSourceTests.BillingDeliveryReview(mode,bindingRevision);
            var company=delivery.Source.Source.Source.Company;
            var draftHistory=await Policy.Owner.GetCompanyBillingPoliciesAsync(company,"policy-maker",Authority);
            var draft=new CompanyBillingPolicyDraft(Guid.NewGuid().ToString("N"),company,"policy-maker","billing-policy",CompanyBillingPolicyTests.Rules,
                "Explicit company rules",draftHistory.Revision,new string('B',64));
            await Policy.Owner.PrepareCompanyBillingPolicyAsync(draft,draftHistory,Authority);
            if(approve)
            {
                var history=await Policy.Owner.GetCompanyBillingPoliciesAsync(company,"policy-reviewer",Authority);
                var row=Assert.Single(history.Records);
                var approval=new CompanyBillingPolicyApproval(Guid.NewGuid().ToString("N"),company,"policy-reviewer",row.ACCOUNTING_METHOD_ID,
                    CompanyBillingPolicyProof.Require(company,row).RowRevision,"Independent exact rules review",history.Revision,new string('B',64));
                await Policy.Owner.ApproveCompanyBillingPolicyAsync(approval,history,Authority);
            }
            var policies=await Policy.Owner.GetCompanyBillingPoliciesAsync(company,delivery.Source.Source.Source.ActorUserId,Authority);
            Current=CompanySalesInvoiceDraftReview.Create(delivery,policies,[],new string('B',64));
            Command=new(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),Current,Facts());
            var actualColumns=new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler();
            var columns=new Mock<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>();
            columns.Setup(value=>value.PrepareForInsert(It.IsAny<Beep.OilandGas.PPDM.Models.IPPDMEntity>(),It.IsAny<string>())).Callback((Beep.OilandGas.PPDM.Models.IPPDMEntity row,string actor)=>{actualColumns.PrepareForInsert(row,actor);AlterDuringAudit?.Invoke(row);});
            columns.Setup(value=>value.PrepareForUpdate(It.IsAny<Beep.OilandGas.PPDM.Models.IPPDMEntity>(),It.IsAny<string>())).Callback((Beep.OilandGas.PPDM.Models.IPPDMEntity row,string actor)=>{actualColumns.PrepareForUpdate(row,actor);AlterDuringAudit?.Invoke(row);});
            var defaults=Mock.Of<IPPDM39DefaultsRepository>();var metadata=new Mock<IPPDMMetadataRepository>();
            metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("SALES_INVOICE_INTENT")).ReturnsAsync(new List<string>{"SALES_INVOICE_INTENT_ID"});
            metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("INVOICE")).ReturnsAsync(new List<string>{"INVOICE_ID"});
            metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("SALES_INVOICE_OUTCOME")).ReturnsAsync(new List<string>{"SALES_INVOICE_OUTCOME_ID"});
            metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("SALES_INVOICE_REVIEW_INTENT")).ReturnsAsync(new List<string>{"SALES_INVOICE_REVIEW_INTENT_ID"});
            Func<Task<string>> binding=()=>Task.FromResult(Native.Connection);
            var accounts=new GLAccountService(Native.Editor.Object,columns.Object,defaults,metadata.Object,NullLogger<GLAccountService>.Instance,binding);
            var journal=new JournalEntryService(Native.Editor.Object,columns.Object,defaults,metadata.Object,accounts,NullLogger<JournalEntryService>.Instance,binding);
            Owner=new(Native.Editor.Object,columns.Object,defaults,metadata.Object,new AccountingBasisPostingService(journal),NullLogger<InvoiceService>.Instance,resolveConnection:binding);
            LegacyOwner=new(Native.Editor.Object,columns.Object,defaults,metadata.Object,new AccountingBasisPostingService(journal),NullLogger<InvoiceService>.Instance);
            Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(entity=>entity.EntityName=="SALES_INVOICE_INTENT"),true))
                .Returns(()=>new ClassCreator(Native.Editor.Object).ConvertToEntityStructure(typeof(SALES_INVOICE_INTENT)));
            Native.Source.Setup(value=>value.InsertEntity("SALES_INVOICE_INTENT",It.IsAny<object>())).Returns((string _,object value)=>{
                Native.Writes++;if(FailWrite)return new ErrorsInfo{Flag=Errors.Failed};var saved=CompanyInventorySourceReview.Copy((SALES_INVOICE_INTENT)value);
                if(Corrupt)saved.INVOICE_ID=Guid.NewGuid().ToString("N");Native.Rows.Add(saved);OnWrite?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            Native.Writes=0;Native.Transactions.Clear();
        }
        public Task Authority(){if(Denied)throw new UnauthorizedAccessException("Original authority revoked.");return Policy.ReadOwner.Native.Authority();}
        public Task<CompanySalesInvoiceDraftReview> ReadCurrent(){CurrentReads++;return Task.FromResult(CompanyInventorySourceReview.Copy(Current));}
        public Task<SALES_INVOICE_INTENT> Save()=>Owner.SaveCompanyInvoiceDraftIntentAsync(Command,Authority,ReadCurrent);
        public SALES_INVOICE_INTENT Stored=>Assert.Single(Native.Rows.OfType<SALES_INVOICE_INTENT>());
        public void AdvanceSource()=>Current=CompanySalesInvoiceDraftReview.Create(Current.Delivery,Current.Policies,[],new string('C',64));
    }
    internal static CompanySalesInvoiceFacts Facts()=>new("INV-2026-01",new DateTime(2026,6,1),new DateTime(2026,7,1),0m,0m,
        "Explicit zero tax proposal requiring invoice review","Explicit zero charge proposal requiring invoice review","Contract entitlement proposal requiring independent review","Prepare reviewed delivery invoice proposal");

    [Fact] public void BeepJournalHasOneCanonicalOwnerAndBoundedRequiredNativeSchema()
    {
        Assert.Equal(typeof(CompanySalesInvoiceDraftCommand).Assembly,typeof(SALES_INVOICE_INTENT).Assembly);
        Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.ModuleSchemaBoundary.Validate([typeof(SALES_INVOICE_INTENT)]);
        var schema=new ClassCreator(Mock.Of<TheTechIdea.Beep.Editor.IDMEEditor>()).ConvertToEntityStructure(typeof(SALES_INVOICE_INTENT));
        var key=Assert.Single(schema.Fields,value=>value.IsKey);Assert.Equal("SALES_INVOICE_INTENT_ID",key.FieldName);Assert.Equal(32,key.Size1);Assert.False(key.AllowDBNull);
        foreach(var (name,size) in new[]{("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("FIELD_ID",128),("RUN_TICKET_ID",128),("INVOICE_ID",32),("SOURCE_REVISION",64),("SCOPE_KEY",64),("RECORDED_REVISION",64),("COMMAND_JSON",SALES_INVOICE_INTENT.MaximumCommandLength)})
        {var field=Assert.Single(schema.Fields,value=>value.FieldName==name);Assert.False(field.AllowDBNull);Assert.Equal(typeof(string).FullName,field.Fieldtype);Assert.Equal(size,field.Size1);}
    }
    [Theory][InlineData(typeof(Beep.OilandGas.Models.Data.Accounting.CreateInvoiceRequest))][InlineData(typeof(Beep.OilandGas.Models.Data.Accounting.UpdateInvoiceRequest))]
    [InlineData(typeof(Beep.OilandGas.Models.Data.Accounting.CreateInvoicePaymentRequest))][InlineData(typeof(Beep.OilandGas.Models.Data.Accounting.InvoiceApprovalResult))]
    [InlineData(typeof(Beep.OilandGas.Models.Data.Accounting.InvoiceAgingSummary))][InlineData(typeof(Beep.OilandGas.Models.Data.Accounting.InvoicePaymentStatus))]
    public void ExistingInvoiceRequestAndReadContractsHaveOneCanonicalDataOwner(Type type)
    {
        Assert.Equal(typeof(CompanySalesInvoiceDraftCommand).Assembly,type.Assembly);
        var value=Activator.CreateInstance(type)!;var json=JsonSerializer.Serialize(value,type);Assert.NotNull(JsonSerializer.Deserialize(json,type));
        Assert.Throws<ArgumentException>(()=>Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.ModuleSchemaBoundary.Validate([type]));
    }

    [Theory][InlineData("full","86.415")][InlineData("partial","49.380")]
    public async Task ExactApprovedPhysicalQuantityAndPriceUseExplicitPolicyAndAdjustments(string mode,string expected)
    {
        var f=new Fixture();await f.Prepare(mode);f.Command.Require();var amounts=f.Current.ReviewAmounts(f.Command.Facts);
        Assert.Equal(decimal.Parse(expected,System.Globalization.CultureInfo.InvariantCulture),amounts.Total);
        Assert.Equal(amounts.Total,amounts.Subtotal);Assert.Equal("KWD",amounts.Rules.CurrencyCode);
        var changed=f.Command.Facts with{ChargesAmount=1.234m,TaxAmount=2.345m};
        Assert.Equal(amounts.Total+3.579m,f.Current.ReviewAmounts(changed).Total);Assert.Equal(0,f.Native.Writes);
    }
    [Theory][InlineData("opaque")][InlineData("unit")][InlineData("zero")][InlineData("legacy")][InlineData("terms-pending")][InlineData("terms-rejected")][InlineData("receiving-pending")][InlineData("receiving-rejected")]
    public async Task UnapprovedOrIncompatibleSourceCannotBecomeInvoiceProposal(string mode)
    {var f=new Fixture();await f.Prepare(mode);Assert.Throws<InvalidOperationException>(()=>f.Command.Require());Assert.Equal(0,f.Native.Writes);}
    [Fact] public async Task PreparedPolicyAloneCannotAuthorizeInvoiceProposal()
    {var f=new Fixture();await f.Prepare(approve:false);Assert.Throws<InvalidOperationException>(()=>f.Command.Require());}
    [Theory][InlineData("tax-missing")][InlineData("charges-missing")][InlineData("negative")][InlineData("precision")][InlineData("tax-notes")][InlineData("entitlement")][InlineData("date-time")][InlineData("date-kind")][InlineData("due")][InlineData("reason")][InlineData("overflow")]
    public async Task InvoiceFactsHaveNoFinancialCalendarOrReviewDefaults(string fault)
    {
        var f=new Fixture();await f.Prepare();var facts=f.Command.Facts;
        facts=fault switch{"tax-missing"=>facts with{TaxAmount=null},"charges-missing"=>facts with{ChargesAmount=null},"negative"=>facts with{TaxAmount=-1},"precision"=>facts with{ChargesAmount=.0001m},
            "tax-notes"=>facts with{TaxReviewNotes=""},"entitlement"=>facts with{EntitlementReviewNotes=""},"date-time"=>facts with{InvoiceDate=facts.InvoiceDate.AddHours(1)},"date-kind"=>facts with{InvoiceDate=DateTime.SpecifyKind(facts.InvoiceDate,DateTimeKind.Utc)},
            "due"=>facts with{DueDate=facts.InvoiceDate.AddDays(-1)},"reason"=>facts with{Reason=""},"overflow"=>facts with{ChargesAmount=decimal.MaxValue},_=>facts};
        f.Command=f.Command with{Facts=facts};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Save());Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    [Fact] public async Task ExistingOwnerRecordsExactJournalAndRetryNeverCreatesInvoiceOrLedger()
    {
        var f=new Fixture();await f.Prepare();var saved=await f.Save();Assert.Equal(JsonSerializer.Serialize(f.Command),JsonSerializer.Serialize(saved.Restore(f.Current.Company,f.Current.ActorUserId)));
        Assert.Equal(1,f.Native.Writes);Assert.Equal(new[]{"begin","commit"},f.Native.Transactions);
        var retry=await f.Save();Assert.Equal(JsonSerializer.Serialize(saved),JsonSerializer.Serialize(retry));Assert.Equal(1,f.Native.Writes);
        Assert.Equal(2,f.CurrentReads);Assert.Empty(f.Native.Rows.OfType<INVOICE>());Assert.Empty(f.Native.Rows.OfType<JOURNAL_ENTRY>());
        saved.COMMAND_JSON="{}";Assert.NotEqual(saved.COMMAND_JSON,f.Stored.COMMAND_JSON);
    }
    [Fact] public async Task UnknownCommitRecoversExactOriginalAfterSourceAdvancesWithoutNewWrite()
    {
        var f=new Fixture();await f.Prepare();f.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Save());Assert.Equal(1,f.Native.Writes);
        var recorded=JsonSerializer.Serialize(f.Stored);f.Native.FailCommit=false;f.AdvanceSource();var reads=f.CurrentReads;
        Assert.Equal(recorded,JsonSerializer.Serialize(await f.Save()));Assert.Equal(reads,f.CurrentReads);Assert.Equal(1,f.Native.Writes);
    }
    [Theory][InlineData("write")][InlineData("corrupt")][InlineData("authority")][InlineData("binding")][InlineData("source")]
    public async Task FailedWriteReadbackOrClosureRollsBackJournal(string fault)
    {
        var f=new Fixture();await f.Prepare();f.FailWrite=fault=="write";f.Corrupt=fault=="corrupt";
        f.OnWrite=()=>{if(fault=="authority")f.Denied=true;if(fault=="binding")f.Native.Connection="changed";if(fault=="source")f.AdvanceSource();};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Save());Assert.Empty(f.Native.Rows.OfType<SALES_INVOICE_INTENT>());Assert.Equal("rollback",f.Native.Transactions.Last());
    }
    [Fact] public async Task SourceChangeBeforeWriteRefusesAndRollsBack()
    {var f=new Fixture();await f.Prepare();f.AdvanceSource();await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Save());Assert.Equal(0,f.Native.Writes);Assert.Equal(new[]{"begin","rollback"},f.Native.Transactions);}
    [Theory][InlineData("borrowed")][InlineData("weak")][InlineData("revoked")]
    public async Task OnlyCurrentAuthorityAndOwnedSerializableTransactionCanInsert(string fault)
    {
        var f=new Fixture();await f.Prepare();f.Native.Borrowed=fault=="borrowed";if(fault=="weak")f.Native.Isolation=IsolationLevel.ReadCommitted;f.Denied=fault=="revoked";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Save());Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Rows.OfType<SALES_INVOICE_INTENT>());
    }
    [Theory][InlineData("missing")][InlineData("nullable")][InlineData("size")][InlineData("duplicate")]
    public async Task BeepJournalMetadataMustDeclareCompleteBoundedRequiredColumns(string fault)
    {
        var f=new Fixture();await f.Prepare();var schema=new ClassCreator(f.Native.Editor.Object).ConvertToEntityStructure(typeof(SALES_INVOICE_INTENT));var field=schema.Fields.Single(value=>value.FieldName=="COMMAND_JSON");
        if(fault=="missing")schema.Fields.Remove(field);if(fault=="nullable")field.AllowDBNull=true;if(fault=="size"){field.Size=10;field.Size1=10;}if(fault=="duplicate")schema.Fields.Add(field);
        f.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(entity=>entity.EntityName=="SALES_INVOICE_INTENT"),true)).Returns(schema);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Save());Assert.Equal(0,f.Native.Writes);
    }
    [Theory][InlineData("body")][InlineData("owner")][InlineData("actor")][InlineData("audit")][InlineData("identity")][InlineData("source")]
    public async Task NativeJournalTamperingNeverReplaysOriginalCommand(string fault)
    {
        var f=new Fixture();await f.Prepare();var row=SALES_INVOICE_INTENT.Create(f.Command,DateTime.UtcNow);
        if(fault=="body")row.COMMAND_JSON=row.COMMAND_JSON.Insert(1,"\"Unknown\":true,");if(fault=="owner")row.OWNER_COMPANY_BA_ID="other";if(fault=="actor")row.ROW_CHANGED_BY="other";
        if(fault=="audit")row.ROW_CHANGED_DATE=DateTime.SpecifyKind(row.ROW_CREATED_DATE!.Value.AddTicks(1),DateTimeKind.Unspecified);if(fault=="identity")row.INVOICE_ID=Guid.NewGuid().ToString("N");if(fault=="source")row.SCOPE_KEY=new string('C',64);
        row.RECORDED_REVISION=SALES_INVOICE_INTENT.Revision(row);Assert.Throws<InvalidOperationException>(()=>row.Restore(f.Current.Company,f.Current.ActorUserId));
    }
    [Fact] public async Task RetainedDeliveryIntentCannotBeReplacedByDifferentBodyOrIdentity()
    {
        var f=new Fixture();await f.Prepare();await f.Save();var original=JsonSerializer.Serialize(f.Stored);
        f.Command=f.Command with{IntentId=Guid.NewGuid().ToString("N"),InvoiceId=Guid.NewGuid().ToString("N")};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Save());Assert.Equal(1,f.Native.Writes);Assert.Equal(original,JsonSerializer.Serialize(f.Stored));
    }
    [Fact] public async Task PreparationAuditMustFollowTheApplicableIndependentPolicyApproval()
    {
        var f=new Fixture();await f.Prepare();var policy=CompanyBillingPolicyProof.Require(f.Current.Company,f.Current.Policies.RequireApplicable("KWD",f.Command.Facts.InvoiceDate));
        Assert.Throws<InvalidOperationException>(()=>SALES_INVOICE_INTENT.Create(f.Command,policy.Record.ApprovedAtUtc!.Value.AddTicks(-1)));
        Assert.Equal(0,f.Native.Writes);
    }
    [Fact] public async Task RepositoryAuditCannotChangeEvenAnOtherwiseValidOriginalCommandBody()
    {
        var f=new Fixture();await f.Prepare();var original=JsonSerializer.Serialize(f.Command);
        f.AlterDuringAudit=row=>((SALES_INVOICE_INTENT)row).COMMAND_JSON=JsonSerializer.Serialize(f.Command with{Facts=f.Command.Facts with{TaxAmount=1m}});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Save());Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Rows.OfType<SALES_INVOICE_INTENT>());
        Assert.Equal(original,JsonSerializer.Serialize(f.Command));Assert.Equal("rollback",f.Native.Transactions.Last());
    }
    [Fact] public async Task AnotherCompanyGlobalIntentCollisionNeverWritesIntoOriginalDelivery()
    {
        var f=new Fixture();await f.Prepare();var foreign=SALES_INVOICE_INTENT.Create(f.Command,DateTime.UtcNow);foreign.OWNER_COMPANY_BA_ID="other";foreign.SCOPE_KEY=new string('C',64);
        f.Native.Rows.Add(foreign);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Save());Assert.Equal(0,f.Native.Writes);Assert.Single(f.Native.Rows.OfType<SALES_INVOICE_INTENT>());
    }
    [Fact] public async Task AnotherOwnedDeliveryIntentBodyCannotOverwriteRetainedOriginal()
    {
        var f=new Fixture();await f.Prepare();await f.Save();f.Command=f.Command with{Facts=f.Command.Facts with{TaxAmount=1m}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Save());Assert.Equal(1,f.Native.Writes);
    }
    [Fact] public async Task JournalReaderRefusesForeignOrAmbiguousProviderRows()
    {
        var f=new Fixture();await f.Prepare();var original=await f.Save();var foreign=CompanyInventorySourceReview.Copy(original);foreign.OWNER_COMPANY_BA_ID="other";
        f.Native.Source.Setup(value=>value.GetEntityAsync("SALES_INVOICE_INTENT",It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[]{foreign});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.GetCompanyInvoiceDraftIntentAsync(f.Current.Company,f.Current.ActorUserId,f.Command.IntentId,f.Authority));
        f.Native.Source.Setup(value=>value.GetEntityAsync("SALES_INVOICE_INTENT",It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[]{original,original});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.GetCompanyInvoiceDraftIntentAsync(f.Current.Company,f.Current.ActorUserId,f.Command.IntentId,f.Authority));Assert.Equal(1,f.Native.Writes);
    }
    [Theory][InlineData("none")][InlineData("owner")][InlineData("customer")][InlineData("quantity-price")][InlineData("tax")][InlineData("status")][InlineData("paid")][InlineData("balance")][InlineData("source")][InlineData("maker")][InlineData("currency")]
    public async Task InvoiceProofBindsNativeProposalAndPreventsAnotherDraftFromSameDelivery(string fault)
    {
        var f=new Fixture();await f.Prepare();var command=f.Command;var record=new CompanySalesInvoiceDraftRecord(command,DateTime.UtcNow);var facts=command.Facts;var amounts=f.Current.ReviewAmounts(facts);
        var row=new INVOICE{INVOICE_ID=command.InvoiceId,INVOICE_NUMBER=facts.InvoiceNumber,CUSTOMER_BA_ID=f.Current.Delivery.Source.Source.Contract.BUYER_BA_ID,
            OWNER_TENANT_ID=f.Current.Company.TenantId,OWNER_COMPANY_BA_ID=f.Current.Company.BusinessAssociateId,FIELD_ID=f.Current.FieldId,SOURCE_RUN_TICKET_ID=f.Current.TicketId,BILLING_SOURCE_KEY=f.Current.SourceKey,
            INVOICE_DATE=facts.InvoiceDate,DUE_DATE=facts.DueDate,SUBTOTAL=amounts.Subtotal,TAX_AMOUNT=facts.TaxAmount!.Value,TOTAL_AMOUNT=amounts.Total,PAID_AMOUNT=0m,BALANCE_DUE=amounts.Total,
            CURRENCY_CODE=amounts.Rules.CurrencyCode,STATUS="DRAFT",DESCRIPTION=facts.Reason,ACTIVE_IND="Y",PPDM_GUID=command.IntentId,ROW_CREATED_BY=f.Current.ActorUserId,ROW_CHANGED_BY=f.Current.ActorUserId,
            ROW_CREATED_DATE=record.PreparedAtUtc,ROW_CHANGED_DATE=record.PreparedAtUtc};
        if(fault=="owner")row.OWNER_COMPANY_BA_ID="other";if(fault=="customer")row.CUSTOMER_BA_ID="another";if(fault=="quantity-price")row.SUBTOTAL=1m;if(fault=="tax")row.TAX_AMOUNT=1m;
        if(fault=="status")row.STATUS="APPROVED";if(fault=="paid")row.PAID_AMOUNT=1m;if(fault=="balance")row.BALANCE_DUE=1m;if(fault=="source")row.SOURCE_RUN_TICKET_ID="other";
        if(fault=="maker")row.ROW_CREATED_BY="other";if(fault=="currency")row.CURRENCY_CODE="USD";
        row.BILLING_WORKFLOW_JSON=CompanySalesInvoiceDraftProof.Create(record,row);
        if(fault!="none"){Assert.Throws<InvalidOperationException>(()=>CompanySalesInvoiceDraftProof.Require(f.Current.Company,row));return;}
        var proof=CompanySalesInvoiceDraftProof.Require(f.Current.Company,row);Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(proof.Record.Command));
        var next=CompanySalesInvoiceDraftReview.Create(f.Current.Delivery,f.Current.Policies,[row],f.Current.BindingRevision);Assert.Single(next.Invoices);
        var another=command with{IntentId=Guid.NewGuid().ToString("N"),InvoiceId=Guid.NewGuid().ToString("N"),Review=next};Assert.Throws<InvalidOperationException>(()=>another.Require());
        Assert.Equal(f.Current.SourceKey,next.SourceKey);Assert.Equal(0,f.Native.Writes);
    }
}
#endif
