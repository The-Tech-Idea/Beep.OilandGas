#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyGLAccountConfigurationHttpTests
{
    internal static CompanyGLAccountConfigurationSubmission Submission(CompanyGLAccountConfigurationReview review)=>new(new(Guid.NewGuid().ToString("N"),review.Accounts.Company,review.Accounts.ActorUserId,review.Accounts.BookId,review.Accounts.CurrencyCode,
        Guid.NewGuid().ToString("N"),"4110","Explicit recorded revenue","REVENUE","CREDIT",null,"Explicit configured description","Reviewed account configuration",review.Accounts.Revision,review.BindingRevision),review.Accounts);
    [Theory][InlineData("valid")][InlineData("uncertain")][InlineData("recovery")][InlineData("grant")][InlineData("scope")][InlineData("actor")][InlineData("binding")][InlineData("source")]
    public async Task NativeConfigurationUsesActualApiClientAndExactOriginalRequest(string mode)
    {
        var native=new CompanyGLAccountConfigurationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=CompanyGLAccountHttpTests.Client(host.Client,company);
        var submission=Submission(await client.GetCompanyGLAccountConfigurationReviewAsync("statutory","KWD"));
        if(mode=="grant")host.Context.Permissions=["Accounting.View"];if(mode=="scope")submission=submission with{Command=submission.Command with{Company=new("tenant","other")}};
        if(mode=="actor")submission=submission with{Command=submission.Command with{ActorUserId="other"}};if(mode=="binding")submission=submission with{Command=submission.Command with{BindingRevision=new string('C',64)}};
        if(mode=="source")native.Reader.Row.ACCOUNT_NAME="changed";
        if(mode is "grant" or "scope" or "actor" or "binding" or "source")
        {using var response=await host.Client.PostAsJsonAsync("api/accounting/traditional/gl/company-accounts/configuration",submission);Assert.Equal(mode=="source"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,response.StatusCode);Assert.Equal(0,native.Reader.Native.Writes);return;}
        if(mode is "uncertain" or "recovery"){native.Reader.Native.FailCommit=true;await Assert.ThrowsAsync<OilGasApiException>(()=>client.ConfigureCompanyGLAccountAsync(submission));native.Reader.Native.FailCommit=false;}
        if(mode=="recovery"){var recovered=await client.GetCompanyGLAccountConfigurationRequestAsync("statutory","KWD",submission.Command.AccountId);Assert.Equal(submission.Command,recovered);submission=new(recovered!,null);}
        var outcome=await client.ConfigureCompanyGLAccountAsync(submission);Assert.Equal(submission.Command,CompanyGLAccountConfigurationProof.Require(outcome).Command);
        await client.ConfigureCompanyGLAccountAsync(submission);Assert.Equal(1,native.Reader.Native.Writes);Assert.Null(outcome.OPENING_BALANCE);Assert.Null(outcome.CURRENT_BALANCE);
        var publicSource=await client.GetCompanyGLAccountsAsync("statutory","KWD");Assert.All(publicSource.Accounts,row=>Assert.Null(row.CONFIGURATION_COMMAND_JSON));
    }
    [Theory][InlineData("roles")][InlineData("grant")][InlineData("actor")][InlineData("binding")]
    public async Task AuthorityLossAtProviderWriteRollsBackAndKeepsTheOriginalBody(string mode)
    {
        var native=new CompanyGLAccountConfigurationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=CompanyGLAccountHttpTests.Client(host.Client,company);var submission=Submission(await client.GetCompanyGLAccountConfigurationReviewAsync("statutory","KWD"));var original=JsonSerializer.Serialize(submission);
        native.OnInsert=()=>{if(mode=="roles")host.Context.Roles=["changed"];if(mode=="grant")host.Context.Permissions=["Accounting.View"];if(mode=="actor")host.Context.Actor="other";if(mode=="binding")host.Context.PolicyBinding=new string('C',64);};
        var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.ConfigureCompanyGLAccountAsync(submission));Assert.Equal(HttpStatusCode.Forbidden,error.StatusCode);Assert.Equal("rollback",native.Reader.Native.Transactions.Last());Assert.Single(native.Reader.Native.Rows.OfType<GL_ACCOUNT>());Assert.Equal(original,JsonSerializer.Serialize(submission));
    }
    [Theory][InlineData("role")][InlineData("permission")][InlineData("schema")]
    public async Task ConfigurationReviewRequiresManagementAuthorityAndReceiptSchemaBeforePrivateReads(string mode)
    {
        var native=new CompanyGLAccountConfigurationTests.Fixture();if(mode=="schema")native.Reader.ChangeSchema=schema=>schema.Fields.Remove(schema.Fields.Single(field=>field.FieldName=="CONFIGURATION_COMMAND_JSON"));
        await using var host=await AccountingSourceHttpTests.Start(role:mode!="role",permissions:mode=="permission"?["Accounting.View"]:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected();var error=await Assert.ThrowsAsync<OilGasApiException>(()=>CompanyGLAccountHttpTests.Client(host.Client,company).GetCompanyGLAccountConfigurationReviewAsync("statutory","KWD"));Assert.Equal(mode=="schema"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,error.StatusCode);Assert.Equal(0,native.Reader.Reads);Assert.Equal(0,native.Reader.Native.Writes);
    }
    [Fact] public async Task AnotherCompanyAccountCanReadPublicFactsButCannotRecoverThePrivateRequest()
    {
        var native=new CompanyGLAccountConfigurationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected();var client=CompanyGLAccountHttpTests.Client(host.Client,company);var submission=Submission(await client.GetCompanyGLAccountConfigurationReviewAsync("statutory","KWD"));await client.ConfigureCompanyGLAccountAsync(submission);
        host.Context.Actor="other";host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","other");company.TrySelect(company.Clear(),"other",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Accounting.ConfigureLedger"]));
        Assert.All((await client.GetCompanyGLAccountsAsync("statutory","KWD")).Accounts,row=>Assert.Null(row.CONFIGURATION_COMMAND_JSON));var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanyGLAccountConfigurationRequestAsync("statutory","KWD",submission.Command.AccountId));Assert.Equal(HttpStatusCode.Forbidden,error.StatusCode);Assert.Equal(1,native.Reader.Native.Writes);
    }
    [Fact] public async Task RetainedStateSurvivesDetachAndActorChangesThenReconcilesThroughActualApi()
    {
        var native=new CompanyGLAccountConfigurationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=CompanyGLAccountHttpTests.Client(host.Client,company);using var state=new CompanyLedgerAccountState(company);var owner=new object();state.AttachOwner(owner);
        await state.LoadConfigurationReviewAsync("statutory","KWD",client);native.Reader.Native.FailCommit=true;
        await Assert.ThrowsAsync<OilGasApiException>(()=>state.ConfigureAsync("4110","Explicit recorded revenue","REVENUE","CREDIT",null,null,"Reviewed configuration",client));var original=JsonSerializer.Serialize(state.PendingConfiguration);Assert.True(state.HasPendingConfiguration);
        state.DetachOwner(owner);state.AttachOwner(new object());Assert.Equal(original,JsonSerializer.Serialize(state.PendingConfiguration));company.Clear();Assert.Null(state.PendingConfiguration);Assert.True(state.HasPendingConfiguration);
        company.TrySelect(company.Version,"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Accounting.ConfigureLedger"]));native.Reader.Native.FailCommit=false;
        await state.SubmitConfigurationAsync(client);Assert.False(state.HasPendingConfiguration);Assert.NotNull(state.ConfigurationResult);Assert.Equal(1,native.Reader.Native.Writes);
    }
    [Fact] public async Task AValidWriteAcknowledgementWithFailedPublicReadKeepsTheExactOriginalRequest()
    {
        var native=new CompanyGLAccountConfigurationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var actual=CompanyGLAccountHttpTests.Client(host.Client,company);var wrapper=new Mock<IAccountingServiceClient>();
        wrapper.Setup(client=>client.GetCompanyGLAccountConfigurationReviewAsync("statutory","KWD")).Returns(()=>actual.GetCompanyGLAccountConfigurationReviewAsync("statutory","KWD"));
        wrapper.Setup(client=>client.ConfigureCompanyGLAccountAsync(It.IsAny<CompanyGLAccountConfigurationSubmission>())).Returns((CompanyGLAccountConfigurationSubmission submission)=>actual.ConfigureCompanyGLAccountAsync(submission));
        wrapper.Setup(client=>client.GetCompanyGLAccountsAsync("statutory","KWD")).ThrowsAsync(new InvalidOperationException("Public account source unavailable"));
        using var state=new CompanyLedgerAccountState(company);state.AttachOwner(new object());await state.LoadConfigurationReviewAsync("statutory","KWD",wrapper.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ConfigureAsync("4110","Explicit recorded revenue","REVENUE","CREDIT",null,null,"Reviewed configuration",wrapper.Object));
        Assert.True(state.HasPendingConfiguration);Assert.Null(state.ConfigurationResult);Assert.Equal(1,native.Reader.Native.Writes);var original=JsonSerializer.Serialize(state.PendingConfiguration);
        wrapper.Setup(client=>client.GetCompanyGLAccountsAsync("statutory","KWD")).Returns(()=>actual.GetCompanyGLAccountsAsync("statutory","KWD"));await state.SubmitConfigurationAsync(wrapper.Object);
        Assert.False(state.HasPendingConfiguration);Assert.NotNull(state.ConfigurationResult);Assert.Equal(1,native.Reader.Native.Writes);Assert.NotEmpty(original);
    }
}
#endif
