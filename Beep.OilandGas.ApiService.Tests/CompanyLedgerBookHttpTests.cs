using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyLedgerBookHttpTests
{
    [Theory][InlineData("valid")][InlineData("scope")][InlineData("actor")][InlineData("binding")][InlineData("grant")][InlineData("role")]
    public async Task ActualApiAndTypedClientEnforceCurrentCompanyRolesAndConfigurationGrant(string fault)
    {
        var f=new CompanyLedgerBookTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(f.Service);context.PolicyBinding=new string('B',64);});using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=CompanyGLAccountHttpTests.Client(host.Client,company);var review=await client.GetCompanyLedgerBookConfigurationReviewAsync();var command=CompanyLedgerBookTests.Command(review.History);
        if(fault=="scope")command=command with{Company=new("tenant","foreign")};if(fault=="actor")command=command with{ActorUserId="other"};if(fault=="binding")command=command with{BindingRevision=new string('C',64)};if(fault=="grant")host.Context.Permissions=["Accounting.View"];if(fault=="role"){host.Client.DefaultRequestHeaders.Remove("X-Test-Role");host.Client.DefaultRequestHeaders.Add("X-Test-Role","UnassignedRole");}
        var submission=new CompanyLedgerBookConfigurationSubmission(command,review.History);
        if(fault is "scope" or "actor"){await Assert.ThrowsAsync<InvalidOperationException>(()=>client.ConfigureCompanyLedgerBookAsync(submission));Assert.Equal(0,f.Reader.Native.Writes);return;}
        if(fault!="valid"){var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.ConfigureCompanyLedgerBookAsync(submission));Assert.True(error.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Conflict);Assert.Equal(0,f.Reader.Native.Writes);return;}
        var row=await client.ConfigureCompanyLedgerBookAsync(submission);Assert.Equal(command,CompanyLedgerBookConfigurationProof.Require(row).Command);Assert.Single((await client.GetCompanyLedgerBooksAsync()).Books);host.Context.Permissions=["Accounting.View"];var recovered=await client.GetCompanyLedgerBookConfigurationAsync(command.Id);Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(recovered));Assert.Equal(1,f.Reader.Native.Writes);
    }
    [Fact] public async Task RetainedUnknownCommitBlocksOtherLedgerRequestsAndReadOnlyRecoveryConfirmsOriginal()
    {
        var f=new CompanyLedgerBookTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(f.Service);context.PolicyBinding=new string('B',64);});using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=CompanyGLAccountHttpTests.Client(host.Client,company);using var state=new CompanyLedgerAccountState(company);var owner=new object();state.AttachOwner(owner);await state.LoadBookConfigurationReviewAsync(client);f.Reader.Native.FailCommit=true;
        await Assert.ThrowsAsync<OilGasApiException>(()=>state.ConfigureBookAsync("statutory","Recorded book","KWD",3,"TO_EVEN","calendar-2026","Asia/Kuwait","Reviewed configuration",client));Assert.True(state.HasPendingLedgerRequest);var original=state.PendingBook!.Command;await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadConfigurationReviewAsync("statutory","KWD",client));state.DetachOwner(owner);state.AttachOwner(new object());Assert.Equal(original,state.PendingBook!.Command);company.Clear();Assert.True(state.HasPendingBook);Assert.Null(state.PendingBook);company.TrySelect(company.Version,"actor",new(original.Company,["ConfiguredCompanyRole"],["Accounting.View"]));host.Context.Permissions=["Accounting.View"];f.Reader.Native.FailCommit=false;await state.RecoverBookAsync(original.Id,client);Assert.False(state.HasPendingBook);Assert.Equal(original,CompanyLedgerBookConfigurationProof.Require(state.BookResult!).Command);Assert.Equal(1,f.Reader.Native.Writes);
    }
}
#endif
