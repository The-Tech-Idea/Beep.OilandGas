#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyBillingPolicyTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    internal static CompanyBillingPolicyRules Rules=>new("sales-v1","KWD",3,"TO_EVEN",new DateTime(2026,1,1),new DateTime(2026,12,31),
        "EXPLICIT_INVOICE_TAX_REVIEW","EXPLICIT_INVOICE_CHARGES_REVIEW","CONTRACT_ENTITLEMENT_REVIEW");
    private sealed class Fixture
    {
        public CompanyMatchingPolicyCommandTests.Fixture Native=new();
        public AccountingPolicyService Owner=>Native.Owner;
        public CompanyBillingPolicyReview Displayed=null!;
        public CompanyBillingPolicyDraft Draft=null!;
        public CompanyBillingPolicyApproval Approval=null!;
        public async Task Prepare(string id="sales-policy",CompanyScope? company=null)
        {
            company ??=Company;Displayed=await Owner.GetCompanyBillingPoliciesAsync(company,"maker",Native.ReadOwner.Native.Authority);
            Draft=new(Guid.NewGuid().ToString("N"),company,"maker",id,Rules,"Explicit currency precision and independent invoice review rules.",Displayed.Revision,new string('B',64));
        }
        public Task<ACCOUNTING_METHOD> RecordDraft()=>Owner.PrepareCompanyBillingPolicyAsync(Draft,Displayed,Native.ReadOwner.Native.Authority);
        public async Task Review(string actor="reviewer",string id="sales-policy",CompanyScope? company=null)
        {
            company ??=Company;Displayed=await Owner.GetCompanyBillingPoliciesAsync(company,actor,Native.ReadOwner.Native.Authority);
            var row=Displayed.Records.Single(value=>value.ACCOUNTING_METHOD_ID==id);
            Approval=new(Guid.NewGuid().ToString("N"),company,actor,id,CompanyBillingPolicyProof.Require(company,row).RowRevision,"Independently reviewed exact policy, currency and calendar window.",Displayed.Revision,new string('B',64));
        }
        public Task<ACCOUNTING_METHOD> RecordApproval()=>Owner.ApproveCompanyBillingPolicyAsync(Approval,Displayed,Native.ReadOwner.Native.Authority);
        public ACCOUNTING_METHOD Stored(string id="sales-policy")=>Native.ReadOwner.Native.Native.Rows.OfType<ACCOUNTING_METHOD>().Single(row=>row.ACCOUNTING_METHOD_ID==id);
    }
    [Fact] public async Task ExistingBeepOwnerPreparesApprovesAndRetriesExactPolicyWithoutDuplicatingWrites()
    {
        var f=new Fixture();await f.Prepare();var draft=await f.RecordDraft();Assert.Equal(JsonSerializer.Serialize(draft),JsonSerializer.Serialize(await f.RecordDraft()));
        var prep=CompanyBillingPolicyProof.Require(Company,draft);Assert.Equal(f.Draft,prep.Draft);Assert.Null(prep.Approval);Assert.Null(prep.Original);
        Assert.Equal("maker",draft.ROW_CREATED_BY);Assert.Equal("DRAFT",draft.MATCHING_POLICY_STATUS);Assert.Equal(1,f.Native.ReadOwner.Native.Native.Writes);
        await f.Review();var approved=await f.RecordApproval();var proof=CompanyBillingPolicyProof.Require(Company,approved);
        Assert.Equal(f.Approval,proof.Approval);Assert.Equal(JsonSerializer.Serialize(draft),JsonSerializer.Serialize(proof.Original));Assert.Equal(prep.Record.PreparedAtUtc,proof.Record.PreparedAtUtc);
        Assert.Equal("maker",approved.ROW_CREATED_BY);Assert.Equal("reviewer",approved.ROW_CHANGED_BY);Assert.Equal("APPROVED",approved.MATCHING_POLICY_STATUS);
        Assert.Equal(JsonSerializer.Serialize(approved),JsonSerializer.Serialize(await f.RecordApproval()));Assert.Equal(2,f.Native.ReadOwner.Native.Native.Writes);
        var history=await f.Owner.GetCompanyBillingPoliciesAsync(Company,"billing",f.Native.ReadOwner.Native.Authority);
        Assert.Equal(approved.ACCOUNTING_METHOD_ID,history.RequireApplicable("KWD",new DateTime(2026,6,1)).ACCOUNTING_METHOD_ID);
        Assert.Throws<InvalidOperationException>(()=>history.RequireApplicable("USD",new DateTime(2026,6,1)));
        Assert.Throws<InvalidOperationException>(()=>history.RequireApplicable("KWD",new DateTime(2027,1,1)));
        approved.DESCRIPTION="changed caller copy";Assert.Equal("sales-v1",f.Stored().DESCRIPTION);
    }
    [Theory][InlineData("draft")][InlineData("approve")]
    public async Task CommitUncertaintyRetainsExactOriginalIntentAndRetryPerformsNoSecondWrite(string action)
    {
        var f=new Fixture();await f.Prepare();if(action=="approve"){await f.RecordDraft();await f.Review();}
        f.Native.ReadOwner.Native.Native.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>action=="draft"?f.RecordDraft():f.RecordApproval());var writes=f.Native.ReadOwner.Native.Native.Writes;
        f.Native.ReadOwner.Native.Native.FailCommit=false;var row=await (action=="draft"?f.RecordDraft():f.RecordApproval());
        Assert.Equal(writes,f.Native.ReadOwner.Native.Native.Writes);var proof=CompanyBillingPolicyProof.Require(Company,row);
        if(action=="draft")Assert.Equal(f.Draft,proof.Draft);else Assert.Equal(f.Approval,proof.Approval);
    }
    [Theory][InlineData("maker")][InlineData("reason")][InlineData("intent")][InlineData("preparation-intent")][InlineData("row-revision")][InlineData("history")]
    public async Task NonIndependentOrUnreviewedApprovalRefusesBeforeTransaction(string fault)
    {
        var f=new Fixture();await f.Prepare();await f.RecordDraft();await f.Review(fault=="maker"?"maker":"reviewer");
        f.Approval=fault switch{"reason"=>f.Approval with{Reason=""},"intent"=>f.Approval with{IntentId=""},"preparation-intent"=>f.Approval with{IntentId=f.Draft.IntentId},
            "row-revision"=>f.Approval with{OriginalRowRevision=new string('C',64)},"history"=>f.Approval with{HistoryRevision=new string('C',64)},_=>f.Approval};
        var writes=f.Native.ReadOwner.Native.Native.Writes;var transactions=f.Native.ReadOwner.Native.Native.Transactions.Count;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.RecordApproval());Assert.Equal(writes,f.Native.ReadOwner.Native.Native.Writes);Assert.Equal(transactions,f.Native.ReadOwner.Native.Native.Transactions.Count);
    }
    [Theory][InlineData("draft","write")][InlineData("draft","corrupt")][InlineData("draft","authority")][InlineData("draft","binding")]
    [InlineData("approve","write")][InlineData("approve","corrupt")][InlineData("approve","authority")][InlineData("approve","binding")]
    public async Task WriteOrAuthorityFailureRollsBackExactPolicyHistory(string action,string fault)
    {
        var f=new Fixture();await f.Prepare();if(action=="approve"){await f.RecordDraft();await f.Review();}
        var original=JsonSerializer.Serialize(f.Native.ReadOwner.Native.Native.Rows.OfType<ACCOUNTING_METHOD>().ToArray());
        if(fault=="write")f.Native.FailWrite=true;if(fault=="corrupt")f.Native.Corrupt=true;
        if(fault=="authority")f.Native.OnWrite=()=>f.Native.ReadOwner.Native.RevokeAt=f.Native.ReadOwner.Native.AuthorityReads+1;
        if(fault=="binding")f.Native.OnWrite=()=>f.Native.ReadOwner.Native.Native.Connection="changed";
        await Assert.ThrowsAnyAsync<Exception>(()=>action=="draft"?f.RecordDraft():f.RecordApproval());
        Assert.Equal(original,JsonSerializer.Serialize(f.Native.ReadOwner.Native.Native.Rows.OfType<ACCOUNTING_METHOD>().ToArray()));Assert.Equal("rollback",f.Native.ReadOwner.Native.Native.Transactions.Last());
    }
    [Theory][InlineData("missing")][InlineData("size")][InlineData("nullable")][InlineData("duplicate")][InlineData("key")]
    public async Task UnconfirmedExistingBeepSchemaRefusesBeforeBusinessReadOrWrite(string fault)
    {
        var f=new Fixture();await f.Prepare();var schema=new ClassCreator(f.Native.ReadOwner.Native.Native.Editor.Object).ConvertToEntityStructure(typeof(ACCOUNTING_METHOD));
        var field=schema.Fields.Single(value=>value.FieldName=="MATCHING_POLICY_COMMAND_JSON");
        if(fault=="missing")schema.Fields.Remove(field);if(fault=="size")field.Size=100;if(fault=="nullable")field.AllowDBNull=false;if(fault=="duplicate")schema.Fields.Add(field);
        if(fault=="key")schema.Fields.Single(value=>value.IsKey).Size=10;
        f.Native.ReadOwner.Native.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(entity=>entity.EntityName=="ACCOUNTING_METHOD"),true)).Returns(schema);
        var reads=f.Native.ReadOwner.Native.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.GetCompanyBillingPoliciesAsync(Company,"maker",f.Native.ReadOwner.Native.Authority));
        Assert.Equal(reads,f.Native.ReadOwner.Native.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.RecordDraft());Assert.Equal(0,f.Native.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("foreign")][InlineData("AP-kind")][InlineData("duplicate")][InlineData("tampered")][InlineData("command")]
    public async Task AnotherOwnerOrAlteredOriginalIntentCannotBeOverwritten(string fault)
    {
        var f=new Fixture();await f.Prepare();await f.RecordDraft();await f.Review();
        if(fault=="foreign")f.Stored().OWNER_COMPANY_BA_ID="other";if(fault=="AP-kind")f.Stored().METHOD_TYPE="AP_MATCHING";
        if(fault=="duplicate")f.Native.ReadOwner.Native.Native.Rows.Add(PurchaseReceiptDecisionProof.Copy(f.Stored()));if(fault=="tampered")f.Stored().DESCRIPTION="changed";
        if(fault=="command")f.Approval=f.Approval with{Reason="Different reviewed body"};
        if(fault=="command"){await f.RecordApproval();f.Approval=f.Approval with{Reason="Original body again"};}
        var writes=f.Native.ReadOwner.Native.Native.Writes;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.RecordApproval());Assert.Equal(writes,f.Native.ReadOwner.Native.Native.Writes);
    }
    [Fact] public async Task SameCurrencyOverlappingApprovalRefusesBeforeSecondApprovalWrite()
    {
        var f=new Fixture();await f.Prepare();await f.RecordDraft();await f.Review();await f.RecordApproval();
        await f.Prepare("second-policy");await f.RecordDraft();await f.Review(id:"second-policy");var writes=f.Native.ReadOwner.Native.Native.Writes;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.RecordApproval());Assert.Equal(writes,f.Native.ReadOwner.Native.Native.Writes);Assert.Equal("DRAFT",f.Stored("second-policy").MATCHING_POLICY_STATUS);
    }
    [Fact] public async Task ExplicitDifferentCurrencyOrNonOverlappingStoredDatesAllowDistinctPolicies()
    {
        var f=new Fixture();await f.Prepare();await f.RecordDraft();await f.Review();await f.RecordApproval();
        await f.Prepare("usd");f.Draft=f.Draft with{Rules=Rules with{CurrencyCode="USD",CurrencyDecimals=2}};await f.RecordDraft();await f.Review(id:"usd");await f.RecordApproval();
        await f.Prepare("future-kwd");f.Draft=f.Draft with{Rules=Rules with{EffectiveFrom=new DateTime(2027,1,1),EffectiveThrough=null}};await f.RecordDraft();await f.Review(id:"future-kwd");await f.RecordApproval();
        var history=await f.Owner.GetCompanyBillingPoliciesAsync(Company,"billing",f.Native.ReadOwner.Native.Authority);
        Assert.Equal("usd",history.RequireApplicable("USD",new DateTime(2026,12,31)).ACCOUNTING_METHOD_ID);Assert.Equal("future-kwd",history.RequireApplicable("KWD",new DateTime(2027,1,1)).ACCOUNTING_METHOD_ID);
    }
    [Fact] public async Task TwoCompaniesAndApPolicyRemainSeparateWithNoBorrowedApprovalOrLatestSelection()
    {
        var f=new Fixture();f.Native.ReadOwner.Native.Native.Rows.Add(PurchaseReceiptDecisionProof.Copy(f.Native.ReadOwner.Row));
        Assert.Empty((await f.Owner.GetCompanyBillingPoliciesAsync(Company,"maker",f.Native.ReadOwner.Native.Authority)).Records);
        await f.Prepare();await f.RecordDraft();await f.Review();await f.RecordApproval();
        var other=new CompanyScope("tenant","other");await f.Prepare("other-sales",other);await f.RecordDraft();await f.Review(id:"other-sales",company:other);await f.RecordApproval();
        Assert.Single((await f.Owner.GetCompanyBillingPoliciesAsync(Company,"billing",f.Native.ReadOwner.Native.Authority)).Records);
        Assert.Single((await f.Owner.GetCompanyBillingPoliciesAsync(other,"billing",f.Native.ReadOwner.Native.Authority)).Records);
        Assert.Single((await f.Owner.GetCompanyMatchingPoliciesAsync(Company,"accountant")).Records);
        await f.Prepare("other-sales");await Assert.ThrowsAsync<InvalidOperationException>(()=>f.RecordDraft());
    }
    [Theory][InlineData("borrowed")][InlineData("isolation")][InlineData("denied")]
    public async Task NativeWritesRequireOwnedSerializableTransactionAndCurrentIndependentAuthority(string fault)
    {
        var f=new Fixture();await f.Prepare();if(fault=="borrowed")f.Native.ReadOwner.Native.Native.Borrowed=true;
        if(fault=="isolation")f.Native.ReadOwner.Native.Native.Isolation=IsolationLevel.ReadCommitted;if(fault=="denied")f.Native.ReadOwner.Native.RevokeAt=1;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.RecordDraft());Assert.Equal(0,f.Native.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("unknown")][InlineData("null")][InlineData("AP-proof")][InlineData("nested-approval")]
    public async Task UnknownMissingOrBorrowedProofFailsClosed(string fault)
    {
        var f=new Fixture();await f.Prepare();await f.RecordDraft();await f.Review();await f.RecordApproval();var row=f.Stored();
        row.MATCHING_POLICY_COMMAND_JSON=fault switch{"unknown"=>row.MATCHING_POLICY_COMMAND_JSON![..^1]+",\"ImplicitApproval\":true}","null"=>"null","AP-proof"=>"{\"Command\":null}",_=>JsonSerializer.Serialize(CompanyBillingPolicyProof.Require(Company,row) with{Original=PurchaseReceiptDecisionProof.Copy(row)})};
        Assert.Throws<InvalidOperationException>(()=>CompanyBillingPolicyProof.Require(Company,row));
    }
    [Theory][InlineData("precision")][InlineData("rounding")][InlineData("currency")][InlineData("tax")][InlineData("charges")][InlineData("rights")][InlineData("date-kind")][InlineData("date-time")][InlineData("expiry")]
    public void PolicyRequiresExplicitCurrencyRoundingStoredCalendarAndAllIndependentReviewRules(string fault)
    {
        var rules=fault switch{"precision"=>Rules with{CurrencyDecimals=9},"rounding"=>Rules with{RoundingMode="DEFAULT"},"currency"=>Rules with{CurrencyCode=""},
            "tax"=>Rules with{TaxRule="ZERO"},"charges"=>Rules with{ChargesRule="NONE"},"rights"=>Rules with{RightsRule="ALL"},"date-kind"=>Rules with{EffectiveFrom=DateTime.SpecifyKind(Rules.EffectiveFrom,DateTimeKind.Utc)},
            "date-time"=>Rules with{EffectiveFrom=Rules.EffectiveFrom.AddHours(1)},_=>Rules with{EffectiveThrough=Rules.EffectiveFrom.AddDays(-1)}};
        Assert.Throws<InvalidOperationException>(()=>rules.Require());
    }
    [Fact] public void CurrencyRoundingRequiresExplicitPrecisionAndModeWithoutImplicitDefaults()
    {
        var rules=Rules with{CurrencyDecimals=2};Assert.Equal(86.42m,rules.RoundPriceExtension(86.415m));Assert.Equal(49.38m,rules.RoundPriceExtension(49.38m));
        Assert.Equal(1.22m,rules.RoundPriceExtension(1.225m));Assert.Equal(1.23m,(rules with{RoundingMode="AWAY_FROM_ZERO"}).RoundPriceExtension(1.225m));
        Assert.Throws<InvalidOperationException>(()=>(rules with{RoundingMode="EXACT_ONLY"}).RoundPriceExtension(86.415m));
        Assert.Throws<InvalidOperationException>(()=>rules.RoundPriceExtension(0.001m));Assert.Throws<InvalidOperationException>(()=>rules.RequireExactAmount(1.001m));
        Assert.Throws<InvalidOperationException>(()=>rules.RequireExactAmount(-1));rules.RequireExactAmount(0);rules.RequireExactAmount(1.22m);
    }
    [Fact] public async Task LegacyCompanylessPolicyMethodsCannotReadOrOverwriteSalesBillingRules()
    {
        var f=new Fixture();await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.RecordPolicyChangeAsync("SALES_BILLING",DateTime.UtcNow,"Rules","Reason","actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.GetActivePolicyAsync("SALES_BILLING"));Assert.Equal(0,f.Native.ReadOwner.Native.Native.Writes);
    }
    [Fact] public async Task ChangedCompleteHistoryAndDuplicatePreparationIntentCannotStartAnotherWrite()
    {
        var f=new Fixture();await f.Prepare();var original=f.Draft;var shown=f.Displayed;
        await f.Prepare("intervening-policy");await f.RecordDraft();f.Draft=original;f.Displayed=shown;
        var writes=f.Native.ReadOwner.Native.Native.Writes;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.RecordDraft());Assert.Equal(writes,f.Native.ReadOwner.Native.Native.Writes);
        await f.Prepare();await f.RecordDraft();f.Draft=f.Draft with{Reason="Changed original preparation body"};
        writes=f.Native.ReadOwner.Native.Native.Writes;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.RecordDraft());Assert.Equal(writes,f.Native.ReadOwner.Native.Native.Writes);
    }
    [Fact] public async Task FrozenReadEvidenceBindsActorAndCompleteContentWithoutSharingMutableRows()
    {
        var f=new Fixture();await f.Prepare();await f.RecordDraft();var first=await f.Owner.GetCompanyBillingPoliciesAsync(Company,"reader",f.Native.ReadOwner.Native.Authority);
        var second=await f.Owner.GetCompanyBillingPoliciesAsync(Company,"reviewer",f.Native.ReadOwner.Native.Authority);Assert.Equal(first.Revision,second.Revision);
        Assert.Throws<InvalidOperationException>(()=>first.Require(Company,"reviewer"));first.Records[0].DESCRIPTION="changed copy";
        Assert.Equal("sales-v1",f.Stored().DESCRIPTION);Assert.Throws<InvalidOperationException>(()=>first.Require(Company,"reader"));
    }
    [Theory][InlineData("draft")][InlineData("approve")]
    public async Task ExactRecordedIntentCanReconcileAfterLaterCompanyPolicyHistoryAdvances(string action)
    {
        var f=new Fixture();await f.Prepare();if(action=="approve"){await f.RecordDraft();await f.Review();}
        f.Native.ReadOwner.Native.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>action=="draft"?f.RecordDraft():f.RecordApproval());
        f.Native.ReadOwner.Native.Native.FailCommit=false;var originalDraft=f.Draft;var originalApproval=f.Approval;var originalDisplayed=f.Displayed;
        await f.Prepare("later-policy");await f.RecordDraft();var writes=f.Native.ReadOwner.Native.Native.Writes;
        f.Draft=originalDraft;f.Approval=originalApproval;f.Displayed=originalDisplayed;var reconciled=await (action=="draft"?f.RecordDraft():f.RecordApproval());
        Assert.Equal(writes,f.Native.ReadOwner.Native.Native.Writes);var proof=CompanyBillingPolicyProof.Require(Company,reconciled);
        if(action=="draft")Assert.Equal(originalDraft,proof.Draft);else Assert.Equal(originalApproval,proof.Approval);
        Assert.Equal(2,(await f.Owner.GetCompanyBillingPoliciesAsync(Company,"reader",f.Native.ReadOwner.Native.Authority)).Records.Count);
    }
    [Fact] public async Task NewWriteStillRollsBackIfWholeHistoryChangesInsideItsOwnedTransaction()
    {
        var other=new Fixture();await other.Prepare("unreviewed-provider-row");var unexpected=await other.RecordDraft();
        var f=new Fixture();await f.Prepare();f.Native.OnWrite=()=>f.Native.ReadOwner.Native.Native.Rows.Add(PurchaseReceiptDecisionProof.Copy(unexpected));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.RecordDraft());Assert.Empty(f.Native.ReadOwner.Native.Native.Rows.OfType<ACCOUNTING_METHOD>());
        Assert.Equal("rollback",f.Native.ReadOwner.Native.Native.Transactions.Last());
    }
    [Fact] public void CanonicalInvoiceRetainsLegacyFieldsAndNullOmittedOwnershipWithBoundedBeepMetadata()
    {
        Assert.Equal(typeof(CompanyBillingPolicyReview).Assembly,typeof(INVOICE).Assembly);ModuleSchemaBoundary.Validate([typeof(INVOICE)]);
        var schema=new ClassCreator(Mock.Of<TheTechIdea.Beep.Editor.IDMEEditor>()).ConvertToEntityStructure(typeof(INVOICE));
        var key=Assert.Single(schema.Fields,field=>field.IsKey);Assert.Equal("INVOICE_ID",key.FieldName);Assert.Equal(128,key.Size1);Assert.False(key.AllowDBNull);
        foreach(var (name,size) in new[]{("OWNER_TENANT_ID",128),("OWNER_COMPANY_BA_ID",128),("FIELD_ID",128),("SOURCE_RUN_TICKET_ID",128),("BILLING_SOURCE_KEY",64),("BILLING_WORKFLOW_JSON",4194304)})
        {var field=Assert.Single(schema.Fields,value=>value.FieldName==name);Assert.True(field.AllowDBNull);Assert.Equal(size,field.Size1);}
        var legacy=new INVOICE{INVOICE_ID="original",CURRENCY_CODE="KWD",TOTAL_AMOUNT=12};var json=JsonSerializer.Serialize(legacy);
        Assert.DoesNotContain("OWNER_TENANT_ID",json);Assert.DoesNotContain("OWNER_COMPANY_BA_ID",json);Assert.DoesNotContain("FIELD_ID",json);Assert.DoesNotContain("BILLING_WORKFLOW_JSON",json);
        Assert.DoesNotContain("SOURCE_RUN_TICKET_ID",json);Assert.DoesNotContain("BILLING_SOURCE_KEY",json);
        var restored=JsonSerializer.Deserialize<INVOICE>(json)!;Assert.Equal("original",restored.INVOICE_ID);Assert.Equal(12,restored.TOTAL_AMOUNT);Assert.Equal("KWD",restored.CURRENCY_CODE);
    }
}
#endif
