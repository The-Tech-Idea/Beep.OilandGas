using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using Beep.OilandGas.PPDM39.Models;
using Moq;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;
using ReviewFixture=Beep.OilandGas.Repository.Tests.CompanyMaintenanceQualifiedAssetTests.ReviewFixture;
namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CompanyMaintenanceQualifiedLicensingTests
{
    private static async Task<ReviewFixture> ReadyPolicy()
    {
        var f=await ReadyTerms();f.Order.WoSubType="CORRECTIVE";var at=DateTime.UtcNow.AddDays(-5);
        f.Native.Rows.Add(new FACILITY_STATUS{FACILITY_ID="plant",FACILITY_TYPE="gas",STATUS_ID="facility-status",STATUS_TYPE="NativeCondition",STATUS="INACTIVE",SOURCE="native-status",ACTIVE_IND="Y",EFFECTIVE_DATE=at,TIMEZONE="UTC"});f.Native.Rows.Add(new EQUIPMENT_STATUS{EQUIPMENT_ID="pump",EQUIP_STATUS_ID="equipment-status",EQUIP_STATUS_TYPE="NativeCondition",EQUIP_STATUS="IDLE",SOURCE="native-status",ACTIVE_IND="Y",EFFECTIVE_DATE=at,TIMEZONE="UTC"});
        f.Native.Rows.Add(new R_FAC_STATUS_TYPE{STATUS_TYPE="NativeCondition",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_EQUIP_STATUS_TYPE{STATUS_TYPE="NativeCondition",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_TIMEZONE{TIMEZONE="UTC",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_FACILITY_STATUS{STATUS_TYPE="NativeCondition",STATUS="INACTIVE",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_FACILITY_STATUS{STATUS_TYPE="NativeCondition",STATUS="RECORDED_TARGET",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_EQUIP_STATUS{STATUS_TYPE="NativeCondition",EQUIP_STATUS="IDLE",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_EQUIP_STATUS{STATUS_TYPE="NativeCondition",EQUIP_STATUS="RECORDED_TARGET",ACTIVE_IND="Y"});
        var condition=f.Native.Rows.OfType<FACILITY_LIC_COND>().Single();condition.FULFILLED_IND="Y";condition.FULFILLED_DATE=DateTime.UtcNow.AddDays(-2);condition.DUE_DATE=DateTime.UtcNow.AddDays(-1);condition.FULFILLED_BY_BA_ID="fulfilment-authority";f.Native.Rows.Add(new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID="fulfilment-authority",ACTIVE_IND="Y",EFFECTIVE_DATE=at.AddYears(-1)});return f;
    }
    private static CompanyMaintenanceReturnPolicyDraft ApplicablePolicy()
    {
        var utc=DateTime.UtcNow;return new("CORRECTIVE","gas","Explicit reviewed jurisdiction / Ù…Ù†Ø·Ù‚Ø©","recorded-area","RecordedRegion",["FacilityRole"],[new("FACILITY","NativeCondition",["INACTIVE"],"RECORDED_TARGET","recorded-policy-source","Reviewed transition"),new("EQUIPMENT","NativeCondition",["IDLE"],"RECORDED_TARGET","recorded-policy-source","Reviewed transition")],[new("issuer","RecordedType","REQUIRED","REQUIRED_PAID",[new("RecordedDecision",["RECORDED_APPROVAL"],"Reviewed status")],[new("RecordedCondition","RecordedCode","FULFILLED_ONLY","RecordedDue","Recorded frequency text","recorded-product",new("EXACT",0,"value-unit","Reviewed value"),new("EXACT",2.5m,"time-unit","Reviewed due term"),"EXACT_VERSION","recorded-restriction",2,"restriction-document","Reviewed restriction interpretation","Reviewed condition"){AcceptedFulfilledByBaIds=["fulfilment-authority"]}],[new("RecordedViolation",["RecordedResolution"],"Reviewed resolution")],"Reviewed licence")],utc.AddDays(-1),utc.AddDays(30),"Reviewed policy source");
    }
    private static void SourceReferences(ReviewFixture f)
    {
        foreach(var key in f.Native.Rows.Select(row=>row.GetType().GetProperty("SOURCE")?.GetValue(row) as string).Append("recorded-policy-source").Where(value=>LeaseSummaryValidator.Identifier(value)).Distinct(StringComparer.Ordinal).ToArray())f.Native.Rows.Add(new R_SOURCE{SOURCE=key!,ACTIVE_IND="Y",EFFECTIVE_DATE=DateTime.UtcNow.AddYears(-2),REMARK="Private source remark"});
    }
    private static async Task<(CompanyMaintenanceQualifiedAssets Sources,CompanyMaintenanceReturnPolicyCatalog Catalog,CompanyMaintenanceQualifiedReturnPolicy Review)> Compare(ReviewFixture f,IReadOnlyList<CompanyMaintenanceReturnPolicyDraft> policies,string[]? roles=null)
    {
        roles??=["FacilityRole"];var identity=await f.Read();var operating=await f.Owner.ReadMaintenanceQualifiedOperatingAsync(f.Context,f.Order,identity,f.Guard);var licensing=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);var sources=identity with{Operating=operating,Licensing=licensing};var context=CompanyMaintenanceQualifiedReturnPolicy.ContextFor(sources,roles);var utc=DateTime.UtcNow;var empty=CompanyMaintenanceReturnPolicyCatalog.Create(context,[],utc);
        var versions=policies.Select(policy=>{var audit=utc<policy.EffectiveToUtc?utc:policy.EffectiveToUtc.AddDays(-1);var command=new CompanyMaintenanceReturnPolicyCommand(context,empty.Revision,policy,1,Guid.NewGuid(),"Private policy rationale");return CompanyMaintenanceReturnPolicyVersion.From(command.Prepare(audit));}).ToArray();var catalog=CompanyMaintenanceReturnPolicyCatalog.Create(context,versions,DateTime.UtcNow);var nativeSources=await f.Owner.ReadMaintenanceReturnNativeSourcesAsync(f.Context,f.Order,sources,catalog,f.Guard);return(sources,catalog,CompanyMaintenanceQualifiedReturnPolicy.Create(sources,f.Order,catalog,roles,nativeSources,DateTime.UtcNow));
    }
    [Theory][InlineData("current")][InlineData("preserve")][InlineData("nonapplicable-area")][InlineData("exempt")][InlineData("resolved-violation")][InlineData("historical-license")]
    public async Task CompleteNativeSourcesCanMatchExplicitPolicyWithoutRecordingAnAssetReturn(string mode)
    {
        using var f=await ReadyPolicy();var policy=ApplicablePolicy();var policies=new List<CompanyMaintenanceReturnPolicyDraft>{policy};
        if(mode=="preserve")
        {
            f.Native.Rows.Add(new FACILITY_STATUS{FACILITY_ID="plant",FACILITY_TYPE="gas",STATUS_ID="retained-facet",STATUS_TYPE="NativeSafety",STATUS="KNOWN",SOURCE="native-status",ACTIVE_IND="Y",EFFECTIVE_DATE=DateTime.UtcNow.AddDays(-1),TIMEZONE="UTC"});f.Native.Rows.Add(new R_FAC_STATUS_TYPE{STATUS_TYPE="NativeSafety",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_FACILITY_STATUS{STATUS_TYPE="NativeSafety",STATUS="KNOWN",ACTIVE_IND="Y"});policies[0]=policy with{Transitions=[..policy.Transitions,new("FACILITY","NativeSafety",["KNOWN"],"","","Reviewed preservation"){Disposition="PRESERVE"}]};
        }
        if(mode=="nonapplicable-area"){AddArea(f,"other-area");policies.Add(policy with{AreaId="other-area",AreaApplicability="NOT_APPLICABLE",Transitions=[],Licenses=[]});}
        if(mode=="exempt"){var condition=f.Native.Rows.OfType<FACILITY_LIC_COND>().Single();condition.FULFILLED_IND="N";condition.FULFILLED_DATE=null;condition.FULFILLED_BY_BA_ID=null!;condition.EXEMPT_IND="Y";var license=policy.Licenses[0];policies[0]=policy with{Licenses=[license with{Conditions=[license.Conditions[0] with{Decision="EXEMPT_ONLY",AcceptedFulfilledByBaIds=[]}]}]};}
        if(mode=="resolved-violation")AddViolation(f,true);
        if(mode=="historical-license"){var old=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<FACILITY_LICENSE>().Single());old.LICENSE_ID="dated-history";old.EXPIRY_DATE=DateTime.UtcNow.AddDays(-1);f.Native.Rows.Add(old);}
        SourceReferences(f);var result=await Compare(f,policies);result.Review.Require(result.Sources,f.Order);Assert.Empty(result.Review.Blockers);var combined=result.Sources with{ReturnPolicy=result.Review};combined.Require(f.Order);Assert.NotEqual(result.Sources.SourceRevision(),combined.SourceRevision());Assert.Equal(result.Sources.IdentityRevision(),combined.IdentityRevision());Assert.True(combined.LatestComparisonAtUtc()>=result.Review.ComparedAtUtc);var json=JsonSerializer.Serialize(combined);var restored=JsonSerializer.Deserialize<CompanyMaintenanceQualifiedAssets>(json)!;restored.Require(f.Order);Assert.Equal(combined.SourceRevision(),restored.SourceRevision());Assert.DoesNotContain("Private policy rationale",json);Assert.DoesNotContain("Private source remark",json);Assert.Empty(f.Native.Writes);
    }
    private static void AddViolation(ReviewFixture f,bool resolved)
    {
        var at=DateTime.UtcNow.AddDays(-3);f.Native.Rows.OfType<FACILITY_LICENSE>().Single().VIOLATION_IND="Y";f.Native.Rows.Add(new FACILITY_LIC_VIOLATION{FACILITY_ID="plant",FACILITY_TYPE="gas",LICENSE_ID="licence-Kuwait",VIOLATION_ID="violation",CONDITION_ID="numeric-condition",VIOLATION_TYPE="RecordedViolation",RESOLVE_TYPE="RecordedResolution",ACTIVE_IND="Y",EFFECTIVE_DATE=at,VIOLATION_DATE=at,RESOLVE_DATE=resolved?at.AddDays(1):null,SOURCE="native-violation"});f.Native.Rows.Add(new R_FAC_LIC_VIOLATION_TYPE{VIOLATION_TYPE="RecordedViolation",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_FAC_LIC_VIOL_RESOL{RESOLVE_TYPE="RecordedResolution",ACTIVE_IND="Y"});
    }
    [Theory][InlineData("no-policy")][InlineData("expired-policy")][InlineData("future-policy")][InlineData("wrong-area")][InlineData("wrong-order")][InlineData("role")][InlineData("conflict")][InlineData("target-reference")][InlineData("target-source")][InlineData("source-value")][InlineData("unmapped-area")][InlineData("nonapplicable-only")][InlineData("fee")][InlineData("null-flag")][InlineData("status-unaccepted")][InlineData("unlisted-condition")][InlineData("value-null")][InlineData("unit")][InlineData("due-frequency")][InlineData("unfulfilled")][InlineData("missing-fulfiller")][InlineData("unaccepted-fulfiller")][InlineData("late-fulfilment")][InlineData("restriction-version")][InlineData("restriction-document")][InlineData("restriction-event")][InlineData("unresolved-violation")]
    public async Task MissingUnlistedAmbiguousOrUnconfirmedSourcesRemainPolicyFindingsWithoutWrites(string mode)
    {
        using var f=await ReadyPolicy();var policy=ApplicablePolicy();var license=policy.Licenses[0];var decision=license.Conditions[0];var condition=f.Native.Rows.OfType<FACILITY_LIC_COND>().Single();var policies=new List<CompanyMaintenanceReturnPolicyDraft>();
        if(mode=="expired-policy")policy=policy with{EffectiveFromUtc=DateTime.UtcNow.AddDays(-20),EffectiveToUtc=DateTime.UtcNow.AddDays(-5)};if(mode=="future-policy")policy=policy with{EffectiveFromUtc=DateTime.UtcNow.AddDays(1)};if(mode=="wrong-area")policy=policy with{AreaId="other-area"};if(mode=="wrong-order")policy=policy with{WorkOrderType="OTHER"};if(mode=="source-value")policy=policy with{Transitions=[policy.Transitions[0] with{AcceptedSourceValues=["OTHER"]},policy.Transitions[1]]};if(mode=="nonapplicable-only")policy=policy with{AreaApplicability="NOT_APPLICABLE",Transitions=[],Licenses=[]};
        if(mode=="target-reference")f.Native.Rows.RemoveAll(row=>row is R_FACILITY_STATUS status && status.STATUS=="RECORDED_TARGET");if(mode=="unmapped-area")AddArea(f,"unmapped-area");if(mode=="fee")f.Native.Rows.OfType<FACILITY_LICENSE>().Single().FEES_PAID_IND="N";if(mode=="null-flag")f.Native.Rows.OfType<FACILITY_LICENSE>().Single().FEES_PAID_IND=null!;
        if(mode=="status-unaccepted"){f.Native.Rows.OfType<FACILITY_LIC_STATUS>().Single().LICENSE_STATUS="REVOKED";f.Native.Rows.Add(new R_LICENSE_STATUS{LICENSE_STATUS_TYPE="RecordedDecision",LICENSE_STATUS="REVOKED",ACTIVE_IND="Y"});}
        if(mode=="unlisted-condition"){var extra=CompanyInventorySourceReview.Copy(condition);extra.CONDITION_ID="extra";extra.CONDITION_CODE="OTHER";f.Native.Rows.Add(extra);f.Native.Rows.Add(new R_FAC_LIC_COND_CODE{CONDITION_TYPE=condition.CONDITION_TYPE,CONDITION_CODE="OTHER",ACTIVE_IND="Y"});}
        if(mode=="value-null")condition.CONDITION_VALUE=null;if(mode=="unit")condition.CONDITION_VALUE_UOM="time-unit";if(mode=="due-frequency")condition.DUE_FREQUENCY="Other frequency";if(mode=="unfulfilled")condition.FULFILLED_IND="N";if(mode=="missing-fulfiller")condition.FULFILLED_BY_BA_ID=null!;
        if(mode=="unaccepted-fulfiller"){condition.FULFILLED_BY_BA_ID="other-actor";f.Native.Rows.Add(new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID="other-actor",ACTIVE_IND="Y"});}if(mode=="late-fulfilment")condition.DUE_DATE=DateTime.UtcNow.AddDays(-4);
        if(mode=="restriction-version")policy=policy with{Licenses=[license with{Conditions=[decision with{RestrictionVersion=3}]}]};if(mode=="restriction-document")policy=policy with{Licenses=[license with{Conditions=[decision with{RestrictionSourceDocumentId="other-document"}]}]};if(mode=="restriction-event")f.Native.Rows.OfType<RESTRICTION>().First().END_DATE_EVENT=DateTime.UtcNow.AddDays(10);if(mode=="unresolved-violation")AddViolation(f,false);
        if(mode!="no-policy")policies.Add(policy);if(mode=="conflict")policies.Add(policy with{Jurisdiction="Another reviewed jurisdiction",Transitions=[policy.Transitions[0] with{TargetValue="OTHER_TARGET"},policy.Transitions[1]]});
        SourceReferences(f);if(mode=="target-source")f.Native.Rows.RemoveAll(row=>row is R_SOURCE source && source.SOURCE=="recorded-policy-source");var result=await Compare(f,policies,mode=="role"?["OtherRole"]:null);Assert.NotEmpty(result.Review.Blockers);Assert.Throws<InvalidOperationException>(()=>(result.Review with{Blockers=[]}).Require(result.Sources,f.Order));if(mode=="conflict")Assert.Contains(result.Review.Blockers,value=>value.Contains("disagree",StringComparison.Ordinal));if(mode=="target-source")Assert.Contains(result.Review.Blockers,value=>value.Contains("R_SOURCE",StringComparison.Ordinal));Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("schema")][InlineData("duplicate")][InlineData("foreign")][InlineData("drift")]
    public async Task PolicySourceReviewRequiresExactPhysicalReferencesAndStableCurrentParents(string mode)
    {
        using var f=await ReadyPolicy();SourceReferences(f);
        if(mode=="schema")f.Native.Source.Setup(source=>source.CheckEntityExist(It.IsAny<string>())).Returns((string table)=>table!="R_SOURCE");if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<R_SOURCE>().First()));if(mode=="foreign")f.Native.Source.Setup(source=>source.GetEntityAsync("R_SOURCE",It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[]{new R_SOURCE{SOURCE="foreign",ACTIVE_IND="Y"}});
        var reads=0;if(mode=="drift")f.Native.AfterRead=()=>{var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");if(call.Arguments[0] as string=="R_SOURCE" && ((List<AppFilter>)call.Arguments[1]).Any(row=>row.FilterValue=="recorded-policy-source") && ++reads==2)f.Native.Rows.OfType<R_SOURCE>().Single(row=>row.SOURCE=="recorded-policy-source").ACTIVE_IND="N";return Task.CompletedTask;};
        await Assert.ThrowsAnyAsync<Exception>(()=>Compare(f,[ApplicablePolicy()]));Assert.Empty(f.Native.Writes);
    }
    [Fact]
    public async Task ScopedPolicyProjectionOmitsUnrelatedHistoryAndRejectsForgedRolesOrSourceMembership()
    {
        using var f=await ReadyPolicy();SourceReferences(f);var policy=ApplicablePolicy();var result=await Compare(f,[policy,policy with{AreaId="unrelated-area",Jurisdiction="Private unrelated jurisdiction"}]);Assert.Empty(result.Review.Blockers);Assert.Single(result.Review.Catalog.Versions);Assert.DoesNotContain("Private unrelated jurisdiction",JsonSerializer.Serialize(result.Review));Assert.Throws<InvalidOperationException>(()=>(result.Review with{ActorReturnRoles=["OtherRole"]}).Require(result.Sources,f.Order));Assert.Throws<InvalidOperationException>(()=>(result.Review with{Applications=[]}).Require(result.Sources,f.Order));Assert.Throws<InvalidOperationException>(()=>(result.Review with{NativeSources=[..result.Review.NativeSources,result.Review.NativeSources[0]]}).Require(result.Sources,f.Order));Assert.Empty(f.Native.Writes);
    }

    [Theory]
    [InlineData("identity")] [InlineData("operating")] [InlineData("licensing")] [InlineData("sources")]
    public async Task QualifiedEvidenceJoinsOnlyTheSameOwnedSerializableCommand(string reader)
    {
        using var f=await ReadyPolicy();SourceReferences(f);var baseline=await Compare(f,[ApplicablePolicy()]);
        f.Native.EnableCorrectiveActionTransactions();
        var connection=new Mock<IDataConnection>();connection.SetupGet(value=>value.ConnectionProp).Returns(f.Native.Source.Object.Dataconnection.ConnectionProp);
        connection.As<IBeepConnectionEvidence>().SetupGet(value=>value.DbConn).Returns(f.Native.LeasePhysicalConnection.Object);f.Native.Source.SetupGet(value=>value.Dataconnection).Returns(connection.Object);
        async Task<string> Read(bool owned)=>reader switch
        {
            "identity" => (await f.Owner.ReadMaintenanceQualifiedAssetsAsync(f.Context,f.Order,f.Guard,owned)).IdentityRevision(),
            "operating" => (await f.Owner.ReadMaintenanceQualifiedOperatingAsync(f.Context,f.Order,baseline.Sources,f.Guard,owned)).SourceRevision(),
            "licensing" => (await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,baseline.Sources,f.Guard,owned)).SourceRevision(),
            _ => JsonSerializer.Serialize(await f.Owner.ReadMaintenanceReturnNativeSourcesAsync(f.Context,f.Order,baseline.Sources,baseline.Catalog,f.Guard,owned))
        };
        var expected=await Read(false);var reads=f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Read(true));
        Assert.Equal(reads,f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync"));
        await PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>Read(false));
            Assert.Equal(reads,f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync"));
            Assert.Equal(expected,await Read(true));
            PPDMDataSourceTransaction.RequireOwned(f.Native.Source.Object,System.Data.IsolationLevel.Serializable);
        });
        Assert.Equal(expected,await Read(false));Assert.Empty(f.Native.Writes);
        f.Native.TransactionIsolation=System.Data.IsolationLevel.ReadCommitted;
        reads=f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync");
        await PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>await Assert.ThrowsAsync<InvalidOperationException>(()=>Read(true)));
        Assert.Equal(reads,f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync"));Assert.Empty(f.Native.Writes);
    }
}
