#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using TheTechIdea.Beep.ConfigUtil;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyGLMappingConfigurationTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    internal sealed class Fixture
    {
        public readonly CompanyGLMappingReviewTests.Fixture Reader=new();
        public CompanyGLMappingConfigurationSubmission Submission=null!;
        public Action? OnInsert;
        public bool CorruptInsert,FailInsert;
        public Fixture()
        {
            Reader.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("GL_ACCOUNT_MAPPING")).ReturnsAsync(new List<string>{"GL_ACCOUNT_MAPPING_ID"});
            Reader.Accounts.Native.Source.Setup(source=>source.InsertEntity("GL_ACCOUNT_MAPPING",It.IsAny<object>())).Returns((string table,object value)=>{
                Reader.Accounts.Native.Writes++;if(FailInsert)return new ErrorsInfo{Flag=Errors.Failed};var row=JsonSerializer.Deserialize<GLAccountMapping>(JsonSerializer.Serialize(value))!;
                if(CorruptInsert)row.GL_ACCOUNT_ID="corrupted";Reader.Accounts.Native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
        }
        public async Task Prepare()
        {
            var review=await Reader.Service.GetCompanyMappingConfigurationReviewAsync(Company,"administrator","statutory","KWD",CompanyGLMappingReviewTests.Date,Reader.Accounts.Authority);
            Submission=new(new(Guid.NewGuid().ToString("N"),Company,"administrator","statutory","KWD",Guid.NewGuid().ToString("N"),"Cash","actual-account-id","1110","ASSET","DEBIT",new DateTime(2026,1,1),null,review.AsOfDate,"Reviewed effective mapping",review.Revision,new string('B',64)),review);
        }
        public Task<GLAccountMapping> Record()=>Reader.Service.ConfigureCompanyMappingAsync(Submission,Reader.Accounts.Authority);
    }
    [Fact] public async Task ExplicitNativeMappingCommitsOnceAndPublicHistoryOmitsPrivateCommandReceipts()
    {
        var fixture=new Fixture();await fixture.Prepare();var row=await fixture.Record();Assert.Equal(fixture.Submission.Command,CompanyGLMappingConfigurationProof.Require(row).Command);Assert.Equal("actual-account-id",row.GL_ACCOUNT_ID);Assert.Equal("1110",row.GL_ACCOUNT_NUMBER);
        Assert.Equal(1,fixture.Reader.Accounts.Native.Writes);Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(await fixture.Record()));Assert.Equal(1,fixture.Reader.Accounts.Native.Writes);
        var publicSource=await fixture.Reader.Service.GetCompanyMappingsAsync(Company,"accountant","statutory","KWD",CompanyGLMappingReviewTests.Date,fixture.Reader.Accounts.Authority);
        Assert.Equal(2,publicSource.Records.Count);Assert.All(publicSource.Records,mapping=>Assert.Null(mapping.CONFIGURATION_COMMAND_JSON));Assert.Equal("actual-account-id",publicSource.Resolve("Cash","ASSET","DEBIT").GL_ACCOUNT_ID);
    }
    [Theory][InlineData("overlap")][InlineData("id-as-number")][InlineData("foreign-account")][InlineData("number")][InlineData("type")][InlineData("normal")]
    [InlineData("date")][InlineData("expiry")][InlineData("utc")][InlineData("reason")][InlineData("changed-account")][InlineData("changed-mapping")]
    public async Task ChangedForeignUnresolvedOverlappingOrMalformedMappingNeverWrites(string mode)
    {
        var fixture=new Fixture();await fixture.Prepare();var command=fixture.Submission.Command;switch(mode){case "overlap":command=command with{MappingKey="AccountsReceivable"};break;
            case "id-as-number":command=command with{AccountId="1110"};break;case "foreign-account":command=command with{AccountId="foreign"};break;case "number":command=command with{AccountNumber="other"};break;
            case "type":command=command with{AccountType="REVENUE"};break;case "normal":command=command with{NormalBalance="CREDIT"};break;case "date":command=command with{EffectiveFrom=command.EffectiveFrom.AddHours(1)};break;
            case "expiry":command=command with{EffectiveThrough=new DateTime(2025,12,31)};break;case "utc":command=command with{EffectiveFrom=DateTime.SpecifyKind(command.EffectiveFrom,DateTimeKind.Utc)};break;
            case "reason":command=command with{Reason=""};break;case "changed-account":fixture.Reader.Accounts.Row.ACCOUNT_NAME="changed";break;case "changed-mapping":fixture.Reader.Mapping.DESCRIPTION="changed";break;}
        fixture.Submission=fixture.Submission with{Command=command};await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Record());Assert.Equal(0,fixture.Reader.Accounts.Native.Writes);
    }
    [Theory][InlineData("insert")][InlineData("readback")][InlineData("authority")][InlineData("binding")][InlineData("account")][InlineData("mapping")]
    public async Task ProviderOrClosingSourceFailureRollsBack(string mode)
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.FailInsert=mode=="insert";fixture.CorruptInsert=mode=="readback";
        fixture.OnInsert=()=>{if(mode=="authority")fixture.Reader.Accounts.RevokeAt=fixture.Reader.Accounts.AuthorityCalls+1;if(mode=="binding")fixture.Reader.Accounts.Native.Connection="changed";if(mode=="account")fixture.Reader.Accounts.Row.ACCOUNT_NAME="changed";if(mode=="mapping")fixture.Reader.Mapping.DESCRIPTION="changed";};
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Record());Assert.Single(fixture.Reader.Accounts.Native.Rows.OfType<GLAccountMapping>());Assert.Equal("rollback",fixture.Reader.Accounts.Native.Transactions.Last());
    }
    [Fact] public async Task UncertainCommitAndProducerRecoveryReconcileTheOriginalCommandWithoutAnotherMapping()
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.Reader.Accounts.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());fixture.Reader.Accounts.Native.FailCommit=false;
        var command=fixture.Submission.Command;var recovered=await fixture.Reader.Service.GetCompanyMappingConfigurationAsync(Company,"administrator","statutory","KWD",command.MappingId,fixture.Reader.Accounts.Authority);Assert.Equal(command,recovered);
        fixture.Submission=new(recovered!,null);var row=await fixture.Record();Assert.Equal(command,CompanyGLMappingConfigurationProof.Require(row).Command);Assert.Equal(1,fixture.Reader.Accounts.Native.Writes);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Reader.Service.GetCompanyMappingConfigurationAsync(Company,"accountant","statutory","KWD",command.MappingId,fixture.Reader.Accounts.Authority));
    }
    [Theory][InlineData("schema")][InlineData("borrowed")][InlineData("isolation")]
    public async Task NativeReceiptSchemaAndOwnedSerializableTransactionAreRequired(string mode)
    {
        var fixture=new Fixture();await fixture.Prepare();if(mode=="schema")fixture.Reader.ChangeSchema=schema=>schema.Fields.Remove(schema.Fields.Single(field=>field.FieldName=="CONFIGURATION_COMMAND_JSON"));
        if(mode=="borrowed")fixture.Reader.Accounts.Native.Borrowed=true;if(mode=="isolation")fixture.Reader.Accounts.Native.Isolation=IsolationLevel.ReadCommitted;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(0,fixture.Reader.Accounts.Native.Writes);
    }
    [Fact] public async Task OriginalIntentCannotBeReusedForAnotherMapping()
    {
        var fixture=new Fixture();await fixture.Prepare();await fixture.Record();fixture.Submission=fixture.Submission with{Command=fixture.Submission.Command with{MappingId=Guid.NewGuid().ToString("N")}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(1,fixture.Reader.Accounts.Native.Writes);
    }
}
#endif
