using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyGLMappingConfigurationHttpTests
{
    internal static CompanyGLMappingConfigurationSubmission Submission(CompanyGLMappingConfigurationReview review)
    {
        var history=review.History;var account=history.Accounts.Accounts.Single();return new(new(Guid.NewGuid().ToString("N"),history.Accounts.Company,history.Accounts.ActorUserId,history.Accounts.BookId,history.Accounts.CurrencyCode,
            Guid.NewGuid().ToString("N"),"Cash",account.GL_ACCOUNT_ID,account.ACCOUNT_NUMBER,account.ACCOUNT_TYPE,account.NORMAL_BALANCE,new DateTime(2026,1,1),null,history.AsOfDate,"Reviewed effective mapping",history.Revision,review.BindingRevision),history);
    }
    [Theory][InlineData("valid")][InlineData("uncertain")][InlineData("recovery")][InlineData("grant")][InlineData("scope")][InlineData("actor")][InlineData("binding")][InlineData("source")][InlineData("overlap")]
    public async Task ActualApiClientRecordsOrReconcilesTheOriginalExplicitMapping(string mode)
    {
        var native=new CompanyGLMappingConfigurationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Reader.Service);services.AddSingleton(native.Reader.Accounts.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=CompanyGLAccountHttpTests.Client(host.Client,company);var submission=Submission(await client.GetCompanyGLMappingConfigurationReviewAsync("statutory","KWD",CompanyGLMappingReviewTests.Date));
        if(mode=="grant")host.Context.Permissions=["Accounting.View"];if(mode=="scope")submission=submission with{Command=submission.Command with{Company=new("tenant","other")}};if(mode=="actor")submission=submission with{Command=submission.Command with{ActorUserId="other"}};
        if(mode=="binding")submission=submission with{Command=submission.Command with{BindingRevision=new string('C',64)}};if(mode=="source")native.Reader.Accounts.Row.ACCOUNT_NAME="changed";if(mode=="overlap")submission=submission with{Command=submission.Command with{MappingKey="AccountsReceivable"}};
        if(mode is "grant" or "scope" or "actor" or "binding" or "source" or "overlap")
        {using var response=await host.Client.PostAsJsonAsync("api/accounting/traditional/gl/company-accounts/mappings/configuration",submission);Assert.Equal(mode is "source" or "overlap"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,response.StatusCode);Assert.Equal(0,native.Reader.Accounts.Native.Writes);return;}
        if(mode is "uncertain" or "recovery"){native.Reader.Accounts.Native.FailCommit=true;await Assert.ThrowsAsync<OilGasApiException>(()=>client.ConfigureCompanyGLMappingAsync(submission));native.Reader.Accounts.Native.FailCommit=false;}
        if(mode=="recovery"){var recovered=await client.GetCompanyGLMappingConfigurationRequestAsync("statutory","KWD",submission.Command.MappingId);Assert.Equal(submission.Command,recovered);submission=new(recovered!,null);}
        var result=await client.ConfigureCompanyGLMappingAsync(submission);Assert.Equal(submission.Command,CompanyGLMappingConfigurationProof.Require(result).Command);await client.ConfigureCompanyGLMappingAsync(submission);Assert.Equal(1,native.Reader.Accounts.Native.Writes);
        var publicHistory=await client.GetCompanyGLMappingsAsync("statutory","KWD",CompanyGLMappingReviewTests.Date);Assert.All(publicHistory.Records,row=>Assert.Null(row.CONFIGURATION_COMMAND_JSON));Assert.Equal("actual-account-id",publicHistory.Resolve("Cash","ASSET","DEBIT").GL_ACCOUNT_ID);
    }
    [Theory][InlineData("role")][InlineData("permission")][InlineData("schema")]
    public async Task ManagementAuthorityAndNativeReceiptCapacityPrecedeMappingConfigurationReads(string mode)
    {
        var native=new CompanyGLMappingConfigurationTests.Fixture();if(mode=="schema")native.Reader.ChangeSchema=schema=>schema.Fields.Remove(schema.Fields.Single(field=>field.FieldName=="CONFIGURATION_COMMAND_JSON"));
        await using var host=await AccountingSourceHttpTests.Start(role:mode!="role",permissions:mode=="permission"?["Accounting.View"]:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Reader.Service);services.AddSingleton(native.Reader.Accounts.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected();var error=await Assert.ThrowsAsync<OilGasApiException>(()=>CompanyGLAccountHttpTests.Client(host.Client,company).GetCompanyGLMappingConfigurationReviewAsync("statutory","KWD",CompanyGLMappingReviewTests.Date));Assert.Equal(mode=="schema"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,error.StatusCode);Assert.Equal(0,native.Reader.MappingReads);Assert.Equal(0,native.Reader.Accounts.Reads);
    }
    [Theory][InlineData("roles")][InlineData("grant")][InlineData("actor")][InlineData("binding")]
    public async Task CurrentAuthorityLossAtNativeInsertRollsBackTheMapping(string mode)
    {
        var native=new CompanyGLMappingConfigurationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Reader.Service);services.AddSingleton(native.Reader.Accounts.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected();var client=CompanyGLAccountHttpTests.Client(host.Client,company);var submission=Submission(await client.GetCompanyGLMappingConfigurationReviewAsync("statutory","KWD",CompanyGLMappingReviewTests.Date));
        native.OnInsert=()=>{if(mode=="roles")host.Context.Roles=["changed"];if(mode=="grant")host.Context.Permissions=["Accounting.View"];if(mode=="actor")host.Context.Actor="other";if(mode=="binding")host.Context.PolicyBinding=new string('C',64);};
        var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.ConfigureCompanyGLMappingAsync(submission));Assert.Equal(HttpStatusCode.Forbidden,error.StatusCode);Assert.Single(native.Reader.Accounts.Native.Rows.OfType<GLAccountMapping>());Assert.Equal("rollback",native.Reader.Accounts.Native.Transactions.Last());
    }
    [Fact] public async Task CircuitStateRetainsTheExactMappingRequestAcrossDetachAndReconcilesWithoutDuplicateWrites()
    {
        var native=new CompanyGLMappingConfigurationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Reader.Service);services.AddSingleton(native.Reader.Accounts.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=CompanyGLAccountHttpTests.Client(host.Client,company);using var state=new CompanyLedgerAccountState(company);var owner=new object();state.AttachOwner(owner);
        await state.LoadMappingConfigurationReviewAsync("statutory","KWD",CompanyGLMappingReviewTests.Date,client);native.Reader.Accounts.Native.FailCommit=true;
        await Assert.ThrowsAsync<OilGasApiException>(()=>state.ConfigureMappingAsync("Cash","actual-account-id",new DateTime(2026,1,1),null,"Reviewed mapping",client));var original=JsonSerializer.Serialize(state.PendingMappingConfiguration);
        state.DetachOwner(owner);state.AttachOwner(new object());Assert.Equal(original,JsonSerializer.Serialize(state.PendingMappingConfiguration));await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadConfigurationReviewAsync("statutory","KWD",client));
        company.Clear();Assert.True(state.HasPendingMappingConfiguration);Assert.Null(state.PendingMappingConfiguration);company.TrySelect(company.Version,"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Accounting.ConfigureLedger"]));
        native.Reader.Accounts.Native.FailCommit=false;await state.SubmitMappingConfigurationAsync(client);Assert.False(state.HasPendingLedgerRequest);Assert.NotNull(state.MappingConfigurationResult);Assert.Equal(1,native.Reader.Accounts.Native.Writes);
    }
    [Fact] public async Task ValidNativeAcknowledgementWithoutCompletePublicMappingReadKeepsTheOriginalRequest()
    {
        var native=new CompanyGLMappingConfigurationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Reader.Service);services.AddSingleton(native.Reader.Accounts.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var actual=CompanyGLAccountHttpTests.Client(host.Client,company);var client=new Mock<IAccountingServiceClient>();
        client.Setup(value=>value.GetCompanyGLMappingConfigurationReviewAsync("statutory","KWD",CompanyGLMappingReviewTests.Date)).Returns(()=>actual.GetCompanyGLMappingConfigurationReviewAsync("statutory","KWD",CompanyGLMappingReviewTests.Date));
        client.Setup(value=>value.ConfigureCompanyGLMappingAsync(It.IsAny<CompanyGLMappingConfigurationSubmission>())).Returns((CompanyGLMappingConfigurationSubmission submission)=>actual.ConfigureCompanyGLMappingAsync(submission));
        client.Setup(value=>value.GetCompanyGLMappingsAsync("statutory","KWD",CompanyGLMappingReviewTests.Date)).ThrowsAsync(new InvalidOperationException("Public mapping source unavailable"));
        using var state=new CompanyLedgerAccountState(company);state.AttachOwner(new object());await state.LoadMappingConfigurationReviewAsync("statutory","KWD",CompanyGLMappingReviewTests.Date,client.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ConfigureMappingAsync("Cash","actual-account-id",new DateTime(2026,1,1),null,"Reviewed mapping",client.Object));Assert.True(state.HasPendingMappingConfiguration);Assert.Null(state.MappingConfigurationResult);Assert.Equal(1,native.Reader.Accounts.Native.Writes);
        client.Setup(value=>value.GetCompanyGLMappingsAsync("statutory","KWD",CompanyGLMappingReviewTests.Date)).Returns(()=>actual.GetCompanyGLMappingsAsync("statutory","KWD",CompanyGLMappingReviewTests.Date));await state.SubmitMappingConfigurationAsync(client.Object);
        Assert.False(state.HasPendingLedgerRequest);Assert.NotNull(state.MappingConfigurationResult);Assert.Equal(1,native.Reader.Accounts.Native.Writes);
    }
}
#endif
