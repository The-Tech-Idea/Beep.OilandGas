using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyGLAccountConfigurationTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    internal sealed class Fixture
    {
        public readonly CompanyGLAccountReviewTests.Fixture Reader=new();
        public GLAccountService Service;
        public CompanyGLAccountConfigurationSubmission Submission=null!;
        public Action? OnInsert;
        public bool CorruptInsert,FailInsert;
        public Fixture()
        {
            var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("GL_ACCOUNT")).ReturnsAsync(new List<string>{"GL_ACCOUNT_ID"});
            Service=new(Reader.Native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,NullLogger<GLAccountService>.Instance,()=>Task.FromResult(Reader.Native.Connection));
            Reader.Native.Source.Setup(source=>source.InsertEntity("GL_ACCOUNT",It.IsAny<object>())).Returns((string table,object value)=>{
                Reader.Native.Writes++;if(FailInsert)return new ErrorsInfo{Flag=Errors.Failed};var row=JsonSerializer.Deserialize<GL_ACCOUNT>(JsonSerializer.Serialize(value))!;
                if(CorruptInsert)row.ACCOUNT_NAME="corrupted";Reader.Native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
        }
        public async Task Prepare()
        {
            var review=await Service.GetCompanyAccountsAsync(Company,"administrator","statutory","KWD",Reader.Authority);
            var command=new CompanyGLAccountConfigurationCommand(Guid.NewGuid().ToString("N"),Company,"administrator","statutory","KWD",Guid.NewGuid().ToString("N"),"4110","Recorded sales revenue","REVENUE","CREDIT",null,"Explicit native account configuration","Reviewed company account configuration",review.Revision,new string('B',64));
            Submission=new(command,review);
        }
        public Task<GL_ACCOUNT> Record()=>Service.ConfigureCompanyAccountAsync(Submission,Reader.Authority);
    }
    [Fact] public async Task ExactNativeConfigurationCommitsOnceAndRetryReadsTheSameUnavailableBalancesAndOriginalReceipt()
    {
        var fixture=new Fixture();await fixture.Prepare();var row=await fixture.Record();Assert.Equal(fixture.Submission.Command,CompanyGLAccountConfigurationProof.Require(row).Command);
        Assert.Null(row.OPENING_BALANCE);Assert.Null(row.CURRENT_BALANCE);Assert.Equal("administrator",row.ROW_CREATED_BY);Assert.Equal(1,fixture.Reader.Native.Writes);
        var retry=await fixture.Record();Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(retry));Assert.Equal(1,fixture.Reader.Native.Writes);
        Assert.Equal(new[]{"begin","commit","begin","commit"},fixture.Reader.Native.Transactions);
        var publicReview=await fixture.Service.GetCompanyAccountsAsync(Company,"accountant","statutory","KWD",fixture.Reader.Authority);
        Assert.All(publicReview.Accounts,account=>Assert.Null(account.CONFIGURATION_COMMAND_JSON));Assert.DoesNotContain("BindingRevision",JsonSerializer.Serialize(publicReview));
    }
    [Theory][InlineData("changed")][InlineData("number")][InlineData("foreign-parent")][InlineData("cycle")][InlineData("reason")][InlineData("type")]
    public async Task ChangedSourceDuplicateNumberOrInvalidExplicitConfigurationNeverWrites(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();var command=fixture.Submission.Command;
        if(fault=="changed")fixture.Reader.Row.ACCOUNT_NAME="changed";if(fault=="number")command=command with{AccountNumber="1110"};if(fault=="foreign-parent")command=command with{ParentAccountId="foreign"};
        if(fault=="cycle")command=command with{ParentAccountId=command.AccountId};if(fault=="reason")command=command with{Reason=""};if(fault=="type")command=command with{AccountType="UNKNOWN"};
        fixture.Submission=fixture.Submission with{Command=command};await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Record());Assert.Equal(0,fixture.Reader.Native.Writes);
    }
    [Theory][InlineData("insert")][InlineData("readback")][InlineData("authority")][InlineData("binding")][InlineData("other-account")]
    public async Task ProviderOrClosingEvidenceFailureRollsBackTheConfiguredAccount(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.FailInsert=fault=="insert";fixture.CorruptInsert=fault=="readback";
        fixture.OnInsert=()=>{if(fault=="authority")fixture.Reader.RevokeAt=fixture.Reader.AuthorityCalls+1;if(fault=="binding")fixture.Reader.Native.Connection="changed";if(fault=="other-account")fixture.Reader.Row.ACCOUNT_NAME="changed";};
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Record());Assert.Single(fixture.Reader.Native.Rows.OfType<GL_ACCOUNT>());Assert.Equal("rollback",fixture.Reader.Native.Transactions.Last());
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task LostCommitAcknowledgementReconcilesExactOriginalCommandEvenAfterLaterCompanyConfiguration(bool later)
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.Reader.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(1,fixture.Reader.Native.Writes);
        fixture.Reader.Native.FailCommit=false;if(later)fixture.Reader.Row.ACCOUNT_NAME="Later recorded source change";
        // Durable command-only recovery confirms an existing outcome and cannot authorize a new insertion.
        fixture.Submission=fixture.Submission with{Review=null};var result=await fixture.Record();Assert.Equal(fixture.Submission.Command,CompanyGLAccountConfigurationProof.Require(result).Command);Assert.Equal(1,fixture.Reader.Native.Writes);
    }
    [Theory][InlineData("borrowed")][InlineData("isolation")][InlineData("schema")]
    public async Task MissingOwnedTransactionOrNativeReceiptCapacityNeverInserts(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();if(fault=="borrowed")fixture.Reader.Native.Borrowed=true;if(fault=="isolation")fixture.Reader.Native.Isolation=IsolationLevel.ReadCommitted;
        if(fault=="schema")fixture.Reader.ChangeSchema=schema=>schema.Fields.Remove(schema.Fields.Single(field=>field.FieldName=="CONFIGURATION_COMMAND_JSON"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(0,fixture.Reader.Native.Writes);if(fault=="borrowed")Assert.Empty(fixture.Reader.Native.Transactions);
    }
    [Fact] public async Task OriginalIntentCannotBeReusedForAnotherAccountOrAlteredConfiguration()
    {
        var fixture=new Fixture();await fixture.Prepare();await fixture.Record();var first=fixture.Submission;
        fixture.Submission=first with{Command=first.Command with{AccountName="Different native account"}};await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());
        fixture.Submission=first with{Command=first.Command with{AccountId=Guid.NewGuid().ToString("N")}};await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(1,fixture.Reader.Native.Writes);
    }
    [Fact] public async Task OriginalProducerRecoversTheExactPrivateCommandAndAnotherActorCannotBorrowIt()
    {
        var fixture=new Fixture();await fixture.Prepare();await fixture.Record();var command=fixture.Submission.Command;
        var recovered=await fixture.Service.GetCompanyAccountConfigurationAsync(Company,"administrator","statutory","KWD",command.AccountId,fixture.Reader.Authority);Assert.Equal(command,recovered);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Service.GetCompanyAccountConfigurationAsync(Company,"accountant","statutory","KWD",command.AccountId,fixture.Reader.Authority));
        Assert.Null(await fixture.Service.GetCompanyAccountConfigurationAsync(Company,"administrator","statutory","KWD",Guid.NewGuid().ToString("N"),fixture.Reader.Authority));Assert.Equal(1,fixture.Reader.Native.Writes);
    }
}
#endif
