#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Accounting.Models;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyGLMappingTransitionTests
{
    internal sealed class Fixture
    {
        internal readonly CompanyGLMappingConfigurationTests.Fixture Creation=new();
        internal CompanyGLMappingTransitionSubmission Submission=null!;
        internal bool FailUpdate,CorruptUpdate;
        internal Action? AfterUpdate;
        internal async Task Prepare(bool replace=false)
        {
            await Creation.Prepare();await Creation.Record();var reader=Creation.Reader;var native=reader.Accounts.Native;
            native.Source.Setup(source=>source.UpdateEntity("GL_ACCOUNT_MAPPING",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;if(FailUpdate)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((GLAccountMapping)value);if(CorruptUpdate)row.GL_ACCOUNT_NUMBER="corrupted";native.Rows.RemoveAll(item=>item is GLAccountMapping before && before.GL_ACCOUNT_MAPPING_ID==row.GL_ACCOUNT_MAPPING_ID);native.Rows.Add(row);AfterUpdate?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            var current=await reader.Service.GetCompanyMappingConfigurationReviewAsync(Creation.Submission.Command.Company,"administrator","statutory","KWD",CompanyGLMappingReviewTests.Date,reader.Accounts.Authority);var end=CompanyGLMappingReviewTests.Date.AddDays(5);
            var next=replace?Creation.Submission.Command with{IntentId=Guid.NewGuid().ToString("N"),MappingId=Guid.NewGuid().ToString("N"),EffectiveFrom=end.AddDays(1),SourceRevision=current.Revision,Reason="Explicit adjacent replacement"}:null;
            Submission=new(new(Guid.NewGuid().ToString("N"),current.Accounts.Company,"administrator","statutory","KWD",Creation.Submission.Command.MappingId,end,current.AsOfDate,"Explicit reviewed retirement",current.Revision,new string('B',64),next),current);native.Writes=0;native.Transactions.Clear();
        }
        internal Task<CompanyGLMappingTransitionOutcome> Record()=>Creation.Reader.Service.TransitionCompanyMappingAsync(Submission,Creation.Reader.Accounts.Authority);
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task RetirementAndAdjacentReplacementPreserveHistoryReceiptsAndExactNoWriteRetry(bool replace)
    {
        var f=new Fixture();await f.Prepare(replace);var result=await f.Record();result.Require(f.Submission.Command);Assert.Equal(replace?2:1,f.Creation.Reader.Accounts.Native.Writes);Assert.Equal(f.Creation.Submission.Command,CompanyGLMappingConfigurationProof.Require(result.Retired).Command);
        var writes=f.Creation.Reader.Accounts.Native.Writes;Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(await f.Record()));Assert.Equal(writes,f.Creation.Reader.Accounts.Native.Writes);
        var recovered=await f.Creation.Reader.Service.GetCompanyMappingTransitionAsync(result.Command.Company,"administrator","statutory","KWD",result.Command.MappingId,f.Creation.Reader.Accounts.Authority);Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(recovered));
        var history=await f.Creation.Reader.Service.GetCompanyMappingsAsync(result.Command.Company,"accountant","statutory","KWD",result.Command.ReviewDate,f.Creation.Reader.Accounts.Authority);Assert.All(history.Records,row=>{Assert.Null(row.CONFIGURATION_COMMAND_JSON);Assert.Null(row.CONFIGURATION_TRANSITION_JSON);});Assert.Equal("actual-account-id",history.Resolve("Cash","ASSET","DEBIT").GL_ACCOUNT_ID);
        var after=await f.Creation.Reader.Service.GetCompanyMappingsAsync(result.Command.Company,"accountant","statutory","KWD",result.Command.EffectiveThrough.AddDays(1),f.Creation.Reader.Accounts.Authority);if(replace)Assert.Equal("actual-account-id",after.Resolve("Cash","ASSET","DEBIT").GL_ACCOUNT_ID);else Assert.Throws<InvalidOperationException>(()=>after.Resolve("Cash","ASSET","DEBIT"));
    }
    [Theory][InlineData("past")][InlineData("source")][InlineData("reason")][InlineData("overlap")][InlineData("update")][InlineData("corrupt")][InlineData("insert")]
    public async Task InvalidOrFailedTransitionCannotPartiallyExpireOrReplaceMapping(string fault)
    {
        var f=new Fixture();await f.Prepare(true);var before=JsonSerializer.Serialize(f.Creation.Reader.Accounts.Native.Rows);var command=f.Submission.Command;
        if(fault=="past")command=command with{EffectiveThrough=command.ReviewDate.AddDays(-1)};if(fault=="source")command=command with{SourceRevision=new string('C',64)};if(fault=="reason")command=command with{Reason=""};if(fault=="overlap")command=command with{Replacement=command.Replacement! with{EffectiveFrom=command.EffectiveThrough}};
        f.Submission=f.Submission with{Command=command};f.FailUpdate=fault=="update";f.CorruptUpdate=fault=="corrupt";f.Creation.FailInsert=fault=="insert";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(before,JsonSerializer.Serialize(f.Creation.Reader.Accounts.Native.Rows));
    }
    [Fact] public async Task UncertainCommitReconcilesOnlyOriginalBodyAndRejectsPrivateRecoveryByAnotherProducer()
    {
        var f=new Fixture();await f.Prepare(true);var native=f.Creation.Reader.Accounts.Native;native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(2,native.Writes);native.FailCommit=false;
        var result=await f.Record();Assert.Equal(2,native.Writes);result.Require(f.Submission.Command);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Creation.Reader.Service.GetCompanyMappingTransitionAsync(result.Command.Company,"another","statutory","KWD",result.Command.MappingId,f.Creation.Reader.Accounts.Authority));
        f.Submission=f.Submission with{Command=f.Submission.Command with{Reason="Changed original body"}};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(2,native.Writes);
    }
    [Fact] public async Task LaterReplacementRetirementPreservesEarlierImmutableOutcomeAndProducerPrivacy()
    {
        var f=new Fixture();await f.Prepare(true);var first=await f.Record();var next=first.Replacement!;var date=next.EFFECTIVE_DATE!.Value.AddDays(2);
        var source=await f.Creation.Reader.Service.GetCompanyMappingConfigurationReviewAsync(first.Command.Company,"another-administrator","statutory","KWD",date,f.Creation.Reader.Accounts.Authority);
        var command=new CompanyGLMappingTransitionCommand(Guid.NewGuid().ToString("N"),first.Command.Company,"another-administrator","statutory","KWD",next.GL_ACCOUNT_MAPPING_ID,date.AddDays(3),date,"Later controlled retirement",source.Revision,first.Command.BindingRevision);
        await f.Creation.Reader.Service.TransitionCompanyMappingAsync(new(command,source),f.Creation.Reader.Accounts.Authority);
        var earlier=await f.Creation.Reader.Service.GetCompanyMappingTransitionAsync(first.Command.Company,"administrator","statutory","KWD",first.Command.MappingId,f.Creation.Reader.Accounts.Authority);
        Assert.Equal(JsonSerializer.Serialize(first),JsonSerializer.Serialize(earlier));Assert.Null(earlier!.Replacement!.CONFIGURATION_TRANSITION_JSON);Assert.Equal(JsonSerializer.Serialize(first),JsonSerializer.Serialize(await f.Record()));
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task AuthorityOrAccountChangeAfterExpiryWriteRollsBackBothVersions(bool authority)
    {
        var f=new Fixture();await f.Prepare(true);var native=f.Creation.Reader.Accounts.Native;var before=JsonSerializer.Serialize(native.Rows);
        f.AfterUpdate=()=>{if(authority)f.Creation.Reader.Accounts.RevokeAt=f.Creation.Reader.Accounts.AuthorityCalls+1;else native.Rows.OfType<GL_ACCOUNT>().Single().ACCOUNT_NAME="Changed in transition";};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(before,JsonSerializer.Serialize(native.Rows));
    }
}
#endif
