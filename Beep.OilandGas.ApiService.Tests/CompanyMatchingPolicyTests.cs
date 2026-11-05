using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyMatchingPolicyTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    internal sealed class Fixture
    {
        public APReceiptAllocationCommandTests.Fixture Native=new();
        public ACCOUNTING_METHOD Row=new(){ACCOUNTING_METHOD_ID="policy",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",METHOD_TYPE="AP_MATCHING",MATCHING_POLICY_STATUS="DRAFT",
            EFFECTIVE_DATE=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc),ACTIVE_IND="Y",MATCHING_POLICY_JSON=JsonSerializer.Serialize(new CompanyMatchingPolicyRules("v1","KWD",0,1,"EXACT_RECORDED_UNITS","EXACT_RECORDED_CURRENCY","SEPARATE_REVIEW_REQUIRED","EXACT_SOURCE_AMOUNTS","SEPARATE_REVIEW_REQUIRED"))};
        public AccountingPolicyService Owner;
        public Fixture()
        {
            Native.Native.Rows.Add(Row);Native.Native.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(entity=>entity.EntityName=="ACCOUNTING_METHOD"),true))
                .Returns(()=>new ClassCreator(Native.Native.Editor.Object).ConvertToEntityStructure(typeof(ACCOUNTING_METHOD)));
            Owner=new(Native.Native.Editor.Object,new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),
                NullLogger<AccountingPolicyService>.Instance,()=>Task.FromResult(Native.Native.Connection));
        }
        public Task<CompanyMatchingPolicyReview> Read()=>Owner.GetCompanyMatchingPoliciesAsync(Company,"accountant");
    }
    [Fact] public async Task ActualPolicyOwnerReturnsFrozenCompleteCompanyHistoryWithoutChoosingAnApproval()
    {
        var f=new Fixture();var retired=PurchaseReceiptDecisionProof.Copy(f.Row);retired.ACCOUNTING_METHOD_ID="old";retired.ACTIVE_IND="N";retired.MATCHING_POLICY_STATUS="RETIRED";f.Native.Native.Rows.Add(retired);
        var other=PurchaseReceiptDecisionProof.Copy(f.Row);other.OWNER_COMPANY_BA_ID="other";other.ACCOUNTING_METHOD_ID="other";f.Native.Native.Rows.Add(other);
        var review=await f.Read();Assert.Equal(2,review.Records.Count);review.Require(Company,"accountant");Assert.Equal(1m,CompanyMatchingPolicyReview.Rules(review.Records.Single(row=>row.ACCOUNTING_METHOD_ID=="policy")).PriceTolerancePercent);
        Assert.Equal("DRAFT",review.Records.Single(row=>row.ACCOUNTING_METHOD_ID=="policy").MATCHING_POLICY_STATUS);
        f.Row.MATCHING_POLICY_JSON="{}";Assert.Equal(1m,CompanyMatchingPolicyReview.Rules(review.Records.Single(row=>row.ACCOUNTING_METHOD_ID=="policy")).PriceTolerancePercent);
        Assert.Equal(0,f.Native.Native.Writes);Assert.Empty(f.Native.Native.Transactions);
    }
    [Theory][InlineData("foreign")][InlineData("duplicate")][InlineData("json")][InlineData("status")][InlineData("field")][InlineData("date")][InlineData("expiry")][InlineData("activity")]
    public async Task UnconfirmedRecordedPoliciesRefuseInsteadOfSelectingOrDefaulting(string fault)
    {
        var f=new Fixture();if(fault=="duplicate")f.Native.Native.Rows.Add(PurchaseReceiptDecisionProof.Copy(f.Row));if(fault=="json")f.Row.MATCHING_POLICY_JSON="{}";
        if(fault=="status")f.Row.MATCHING_POLICY_STATUS="?";if(fault=="field")f.Row.FIELD_ID="field";if(fault=="date")f.Row.EFFECTIVE_DATE=null;
        if(fault=="expiry")f.Row.EXPIRY_DATE=f.Row.EFFECTIVE_DATE!.Value.AddDays(-1);if(fault=="activity")f.Row.ACTIVE_IND="?";
        if(fault=="foreign")f.Native.Native.Source.Setup(source=>source.GetEntityAsync("ACCOUNTING_METHOD",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new object[]{
            new ACCOUNTING_METHOD{ACCOUNTING_METHOD_ID="foreign",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="other",METHOD_TYPE="AP_MATCHING"}});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Read());Assert.Equal(0,f.Native.Native.Writes);
    }
    [Theory][InlineData("missing")][InlineData("duplicate")][InlineData("size")][InlineData("nullable")][InlineData("key")]
    public async Task MissingOrAmbiguousBeepSchemaRefusesBeforeBusinessRead(string fault)
    {
        var f=new Fixture();var schema=new ClassCreator(f.Native.Native.Editor.Object).ConvertToEntityStructure(typeof(ACCOUNTING_METHOD));
        var field=schema.Fields.Single(item=>item.FieldName=="MATCHING_POLICY_JSON");if(fault=="missing")schema.Fields.Remove(field);if(fault=="duplicate")schema.Fields.Add(field);
        if(fault=="size")field.Size=100;if(fault=="nullable")field.AllowDBNull=false;if(fault=="key")schema.Fields.Single(item=>item.IsKey).IsKey=false;
        f.Native.Native.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(entity=>entity.EntityName=="ACCOUNTING_METHOD"),true)).Returns(schema);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Read());Assert.DoesNotContain(f.Native.Native.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && (string)call.Arguments[0]=="ACCOUNTING_METHOD");
    }
    [Theory][InlineData("quantity")][InlineData("price")][InlineData("currency")][InlineData("unit")][InlineData("tax")][InlineData("rounding")][InlineData("non-po")]
    public void RulesRequireEveryExplicitSupportedValueWithoutZeroDefaults(string fault)
    {
        var rules=CompanyMatchingPolicyReview.Rules(new Fixture().Row);rules=fault switch{"quantity"=>rules with{QuantityTolerancePercent=null},"price"=>rules with{PriceTolerancePercent=101},
            "currency"=>rules with{CurrencyCode=""},"unit"=>rules with{UnitRule="CONVERT"},"tax"=>rules with{TaxTreatment="DEFAULT"},"rounding"=>rules with{RoundingRule="DEFAULT"},_=>rules with{NonPurchaseInvoiceRule="ALLOW"}};
        Assert.Throws<InvalidOperationException>(()=>rules.Require());
    }
    [Fact] public async Task EmptyCompanyHistoryIsExplicitAndNoLatestRecordIsInvented()
    {
        var f=new Fixture();f.Native.Native.Rows.Remove(f.Row);Assert.Empty((await f.Read()).Records);
    }
    [Fact] public async Task UnknownRuleFieldsCannotBeSilentlyIgnored()
    {
        var f=new Fixture();f.Row.MATCHING_POLICY_JSON=f.Row.MATCHING_POLICY_JSON![..^1]+",\"ImplicitConversion\":true}";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Read());Assert.Equal(0,f.Native.Native.Writes);
    }
    [Fact] public async Task AccountAndContentAreValidatedIndependentlyAcrossReaders()
    {
        var f=new Fixture();var first=await f.Read();var second=await f.Owner.GetCompanyMatchingPoliciesAsync(Company,"controller");Assert.Equal(first.Revision,second.Revision);
        Assert.Throws<InvalidOperationException>(()=>first.Require(Company,"controller"));first.Records[0].MATCHING_POLICY_STATUS="APPROVED";
        Assert.Throws<InvalidOperationException>(()=>first.Require(Company,"accountant"));
    }
    [Fact] public async Task LegacyCompanylessPolicyMethodsCannotWriteOrReadMatchingRules()
    {
        var f=new Fixture();await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.RecordPolicyChangeAsync("AP_MATCHING",DateTime.UtcNow,"Rules","Reason","actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.GetActivePolicyAsync("AP_MATCHING"));Assert.Equal(0,f.Native.Native.Writes);
    }
    [Fact] public async Task MissingBindingCannotFallBackToGlobalConnection()
    {
        var f=new Fixture();var unbound=new AccountingPolicyService(f.Native.Native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),NullLogger<AccountingPolicyService>.Instance);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>unbound.GetCompanyMatchingPoliciesAsync(Company,"accountant"));
    }
    [Theory][InlineData("before")][InlineData("after")]
    public async Task ChangedBindingCannotPublishOriginalCompanyPolicy(string stage)
    {
        var f=new Fixture();var captures=0;
        var owner=new AccountingPolicyService(f.Native.Native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),NullLogger<AccountingPolicyService>.Instance,
            ()=>Task.FromResult(++captures> (stage=="before"?1:2)?"changed":f.Native.Native.Connection));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.GetCompanyMatchingPoliciesAsync(Company,"accountant"));Assert.Equal(0,f.Native.Native.Writes);
        if(stage=="before")Assert.DoesNotContain(f.Native.Native.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && (string)call.Arguments[0]=="ACCOUNTING_METHOD");
    }
}
#endif
