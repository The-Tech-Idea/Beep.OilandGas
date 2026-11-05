using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyMatchingPolicyCommandTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    internal sealed class Fixture
    {
        public CompanyMatchingPolicyTests.Fixture ReadOwner=new();
        public AccountingPolicyService Owner;
        public CompanyMatchingPolicyReview Displayed=null!;
        public CompanyMatchingPolicyCommand Command=null!;
        public bool FailWrite,Corrupt;
        public Action? OnWrite;
        public Fixture()
        {
            var native=ReadOwner.Native.Native;native.Rows.Remove(ReadOwner.Row);
            var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("ACCOUNTING_METHOD")).ReturnsAsync(new List<string>{"ACCOUNTING_METHOD_ID"});
            Owner=new(native.Editor.Object,new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,
                NullLogger<AccountingPolicyService>.Instance,()=>Task.FromResult(native.Connection));
            IErrorsInfo Write(object value,bool insert)
            {
                native.Writes++;if(FailWrite)return new ErrorsInfo{Flag=Errors.Failed};var row=PurchaseReceiptDecisionProof.Copy((ACCOUNTING_METHOD)value);
                if(Corrupt)row.DESCRIPTION="changed";
                if(!insert)native.Rows.RemoveAll(item=>item is ACCOUNTING_METHOD record && record.ACCOUNTING_METHOD_ID==row.ACCOUNTING_METHOD_ID);
                native.Rows.Add(row);OnWrite?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};
            }
            native.Source.Setup(value=>value.InsertEntity("ACCOUNTING_METHOD",It.IsAny<object>())).Returns((string _,object value)=>Write(value,true));
            native.Source.Setup(value=>value.UpdateEntity("ACCOUNTING_METHOD",It.IsAny<object>())).Returns((string _,object value)=>Write(value,false));
        }
        public async Task PrepareDraft(string actor="maker")
        {
            Displayed=await Owner.GetCompanyMatchingPoliciesAsync(Company,actor);
            Command=new(Company,actor,"new-policy","DRAFT",CompanyMatchingPolicyReview.Rules(ReadOwner.Row),ReadOwner.Row.EFFECTIVE_DATE!.Value,null,
                Displayed.Revision,new string('B',64),Guid.NewGuid().ToString("N"),"Explicit company matching policy prepared for independent review.");
        }
        public async Task PrepareApproval(string actor="controller")
        {
            Displayed=await Owner.GetCompanyMatchingPoliciesAsync(Company,actor);var row=Displayed.Records.Single(item=>item.ACCOUNTING_METHOD_ID==(Command?.PolicyId ?? ReadOwner.Row.ACCOUNTING_METHOD_ID));
            Command=new(Company,actor,row.ACCOUNTING_METHOD_ID,"APPROVE",CompanyMatchingPolicyReview.Rules(row),row.EFFECTIVE_DATE!.Value,row.EXPIRY_DATE,
                Displayed.Revision,new string('B',64),Guid.NewGuid().ToString("N"),"Reviewed exact company matching policy and effective window.");
        }
        public Task<ACCOUNTING_METHOD> Record()=>Owner.RecordCompanyMatchingPolicyAsync(Command,Displayed,ReadOwner.Native.Authority);
        public ACCOUNTING_METHOD Stored=>Assert.Single(ReadOwner.Native.Native.Rows.OfType<ACCOUNTING_METHOD>());
    }
    [Fact] public async Task NativeDraftAndIndependentApprovalPreservePolicyAndExactIntentRetries()
    {
        var f=new Fixture();await f.PrepareDraft();var draft=await f.Record();var draftRetry=await f.Record();Assert.Equal(JsonSerializer.Serialize(draft),JsonSerializer.Serialize(draftRetry));
        Assert.Equal("DRAFT",draft.MATCHING_POLICY_STATUS);Assert.Equal("maker",draft.ROW_CREATED_BY);Assert.Equal(1,f.ReadOwner.Native.Native.Writes);
        await f.PrepareApproval();var approved=await f.Record();var retry=await f.Record();
        Assert.Equal("APPROVED",approved.MATCHING_POLICY_STATUS);Assert.Equal("maker",approved.ROW_CREATED_BY);Assert.Equal("controller",approved.ROW_CHANGED_BY);
        Assert.Equal(JsonSerializer.Serialize(approved),JsonSerializer.Serialize(retry));Assert.Equal(2,f.ReadOwner.Native.Native.Writes);
        var proof=CompanyMatchingPolicyProof.Require(Company,approved);Assert.Equal(f.Command,proof.Command);Assert.Equal("DRAFT",proof.Original!.MATCHING_POLICY_STATUS);
        Assert.Equal("maker",CompanyMatchingPolicyProof.Require(Company,proof.Original).Command.ActorUserId);
        approved.DESCRIPTION="changed";Assert.NotEqual(approved.DESCRIPTION,f.Stored.DESCRIPTION);
    }
    [Theory][InlineData("same-actor")][InlineData("rules")][InlineData("date")][InlineData("reason")][InlineData("intent")][InlineData("binding")][InlineData("revision")]
    public async Task UnreviewedOrNonIndependentApprovalNeverWrites(string fault)
    {
        var f=new Fixture();await f.PrepareDraft();await f.Record();await f.PrepareApproval(fault=="same-actor"?"maker":"controller");
        f.Command=fault switch{"rules"=>f.Command with{Rules=f.Command.Rules with{PriceTolerancePercent=2}},"date"=>f.Command with{EffectiveAtUtc=f.Command.EffectiveAtUtc.AddDays(1)},
            "reason"=>f.Command with{Reason=""},"intent"=>f.Command with{IntentId=""},"binding"=>f.Command with{BindingRevision=""},"revision"=>f.Command with{ExpectedHistoryRevision=new string('C',64)},_=>f.Command};
        var writes=f.ReadOwner.Native.Native.Writes;var transactions=f.ReadOwner.Native.Native.Transactions.Count;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(writes,f.ReadOwner.Native.Native.Writes);Assert.Equal(transactions,f.ReadOwner.Native.Native.Transactions.Count);
    }
    [Theory][InlineData("write")][InlineData("corrupt")][InlineData("authority")][InlineData("binding")]
    public async Task DraftWriteFailureOrChangedAuthorityRollsBackHistory(string fault)
    {
        var f=new Fixture();await f.PrepareDraft();if(fault=="write")f.FailWrite=true;if(fault=="corrupt")f.Corrupt=true;
        if(fault=="authority")f.OnWrite=()=>f.ReadOwner.Native.RevokeAt=f.ReadOwner.Native.AuthorityReads+1;
        if(fault=="binding")f.OnWrite=()=>f.ReadOwner.Native.Native.Connection="changed";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Empty(f.ReadOwner.Native.Native.Rows.OfType<ACCOUNTING_METHOD>());Assert.Equal("rollback",f.ReadOwner.Native.Native.Transactions.Last());
    }
    [Theory][InlineData("write")][InlineData("corrupt")][InlineData("authority")]
    public async Task ApprovalFailureRollsBackExactPreparedPolicy(string fault)
    {
        var f=new Fixture();await f.PrepareDraft();await f.Record();var original=JsonSerializer.Serialize(f.Stored);await f.PrepareApproval();
        if(fault=="write")f.FailWrite=true;if(fault=="corrupt")f.Corrupt=true;if(fault=="authority")f.OnWrite=()=>f.ReadOwner.Native.RevokeAt=f.ReadOwner.Native.AuthorityReads+1;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(original,JsonSerializer.Serialize(f.Stored));Assert.Equal("rollback",f.ReadOwner.Native.Native.Transactions.Last());
    }
    [Theory][InlineData("draft")][InlineData("approve")]
    public async Task CommitUncertaintyRetainsExactIntentAndRetryDoesNotWriteAgain(string action)
    {
        var f=new Fixture();await f.PrepareDraft();if(action=="approve"){await f.Record();await f.PrepareApproval();}
        f.ReadOwner.Native.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());var writes=f.ReadOwner.Native.Native.Writes;
        f.ReadOwner.Native.Native.FailCommit=false;var row=await f.Record();Assert.Equal(writes,f.ReadOwner.Native.Native.Writes);Assert.Equal(f.Command,CompanyMatchingPolicyProof.Require(Company,row).Command);
    }
    [Theory][InlineData("missing")][InlineData("size")][InlineData("nullable")][InlineData("key-size")]
    public async Task MissingCommandSchemaRefusesBeforeMutation(string fault)
    {
        var f=new Fixture();await f.PrepareDraft();var schema=new ClassCreator(f.ReadOwner.Native.Native.Editor.Object).ConvertToEntityStructure(typeof(ACCOUNTING_METHOD));
        var field=schema.Fields.Single(item=>item.FieldName=="MATCHING_POLICY_COMMAND_JSON");if(fault=="missing")schema.Fields.Remove(field);if(fault=="size")field.Size=100;if(fault=="nullable")field.AllowDBNull=false;
        if(fault=="key-size")schema.Fields.Single(item=>item.FieldName=="ACCOUNTING_METHOD_ID").Size=10;
        f.ReadOwner.Native.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(entity=>entity.EntityName=="ACCOUNTING_METHOD"),true)).Returns(schema);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,f.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("tampered")][InlineData("foreign")][InlineData("duplicate")][InlineData("changed-history")]
    public async Task ChangedOrAmbiguousPolicyEvidenceCannotBeOverwritten(string fault)
    {
        var f=new Fixture();await f.PrepareDraft();await f.Record();await f.PrepareApproval();
        if(fault=="tampered")f.Stored.DESCRIPTION="changed";if(fault=="foreign")f.Stored.OWNER_COMPANY_BA_ID="other";
        if(fault=="duplicate")f.ReadOwner.Native.Native.Rows.Add(PurchaseReceiptDecisionProof.Copy(f.Stored));
        if(fault=="changed-history"){var row=PurchaseReceiptDecisionProof.Copy(f.ReadOwner.Row);row.ACCOUNTING_METHOD_ID="unreviewed";f.ReadOwner.Native.Native.Rows.Add(row);}
        var writes=f.ReadOwner.Native.Native.Writes;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(writes,f.ReadOwner.Native.Native.Writes);
    }
    [Fact] public async Task OverlappingRecordedApprovalDoesNotAllowAnotherPolicyApproval()
    {
        var f=new Fixture();await f.PrepareDraft();await f.Record();var other=PurchaseReceiptDecisionProof.Copy(f.ReadOwner.Row);other.ACCOUNTING_METHOD_ID="other-policy";other.MATCHING_POLICY_STATUS="APPROVED";
        f.ReadOwner.Native.Native.Rows.Add(other);await f.PrepareApproval();var writes=f.ReadOwner.Native.Native.Writes;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(writes,f.ReadOwner.Native.Native.Writes);
    }
    [Fact] public async Task LegacyDraftWithoutVerifiedPreparationCannotBeApproved()
    {
        var f=new Fixture();f.ReadOwner.Native.Native.Rows.Add(PurchaseReceiptDecisionProof.Copy(f.ReadOwner.Row));await f.PrepareApproval();
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,f.ReadOwner.Native.Native.Writes);Assert.Empty(f.ReadOwner.Native.Native.Transactions);
    }
    [Theory][InlineData("borrowed")][InlineData("isolation")][InlineData("denied")]
    public async Task MissingIndependentWriteAuthorityOrOwnedIsolationNeverWrites(string fault)
    {
        var f=new Fixture();await f.PrepareDraft();if(fault=="borrowed")f.ReadOwner.Native.Native.Borrowed=true;
        if(fault=="isolation")f.ReadOwner.Native.Native.Isolation=IsolationLevel.ReadCommitted;if(fault=="denied")f.ReadOwner.Native.RevokeAt=1;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(0,f.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("unknown")][InlineData("null-command")][InlineData("null-proof")]
    public async Task UnsupportedOrMissingProofCannotEstablishPreparedPolicy(string fault)
    {
        var f=new Fixture();await f.PrepareDraft();await f.Record();var row=f.Stored;
        row.MATCHING_POLICY_COMMAND_JSON=fault switch{"unknown"=>row.MATCHING_POLICY_COMMAND_JSON![..^1]+",\"ImplicitApproval\":true}","null-command"=>"{\"Command\":null}",_=>"null"};
        Assert.Throws<InvalidOperationException>(()=>CompanyMatchingPolicyProof.Require(Company,row));
    }
}
#endif
