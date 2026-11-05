#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyFxRateHttpTests
{
    [Theory][InlineData("valid")][InlineData("binding")][InlineData("grant")][InlineData("role")][InlineData("source")]
    public async Task ActualRateApiAndTypedClientEnforceOriginalCompanyRolesPurposeAndNativeSource(string fault)
    {
        var f=new CompanyFxRateTests.Fixture();await f.Prepare();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(f.Service);context.PolicyBinding=new string('B',64);});using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=CompanyGLAccountHttpTests.Client(host.Client,company);var review=await client.GetCompanyFxRateConfigurationReviewAsync("statutory","USD",f.Date);var command=CompanyFxRateTests.Command(review.Source);if(fault=="binding")command=command with{BindingRevision=new string('C',64)};if(fault=="source")command=command with{SourceRevision=new string('C',64)};if(fault=="grant")host.Context.Permissions=["Accounting.View"];if(fault=="role")host.Client.DefaultRequestHeaders.Remove("X-Test-Role");var submission=new CompanyFxRateConfigurationSubmission(command,review.Source);
        if(fault!="valid"){var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.ConfigureCompanyFxRateAsync(submission));Assert.True(error.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Conflict);Assert.Equal(0,f.Books.Reader.Native.Writes);return;}
        var row=await client.ConfigureCompanyFxRateAsync(submission);Assert.Equal(command,CompanyFxRateProof.Require(row).Command);Assert.Equal(command.Rate,(await client.GetCompanyFxRatesAsync("statutory","USD",f.Date)).RequireApplicable().Rate);host.Context.Permissions=["Accounting.View"];Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(await client.GetCompanyFxRateConfigurationAsync(command.Id)));host.Context.Actor="other";host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","other");Assert.True(company.TrySelect(company.Clear(),"other",new(command.Company,["ConfiguredCompanyRole"],["Accounting.View"])));Assert.Single((await client.GetCompanyFxRatesAsync("statutory","USD",f.Date)).Rates);var privateError=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanyFxRateConfigurationAsync(command.Id));Assert.Equal(HttpStatusCode.Forbidden,privateError.StatusCode);Assert.Equal(1,f.Books.Reader.Native.Writes);
    }
    [Fact] public async Task CircuitRetainsExactUncertainRateBlocksBookRequestsAndRecoversUnderReadOnlyAuthority()
    {
        var f=new CompanyFxRateTests.Fixture();await f.Prepare();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(f.Service);context.PolicyBinding=new string('B',64);});using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=CompanyGLAccountHttpTests.Client(host.Client,company);using var state=new CompanyLedgerAccountState(company);var owner=new object();state.AttachOwner(owner);await state.LoadFxRatesAsync("statutory","USD",f.Date,client,true);f.Books.Reader.Native.FailCommit=true;var c=CompanyFxRateTests.Command(state.FxConfigurationReview!.Source);
        await Assert.ThrowsAsync<OilGasApiException>(()=>state.ConfigureFxAsync(c.Version,c.Rate,c.RateDecimals,c.Direction,c.QuoteDate,c.EffectiveFrom,c.EffectiveThrough,c.ObservedAtUtc,c.Provider,c.Provenance,c.AuthorityReference,c.AllocationPolicy,c.Reason,client));Assert.True(state.HasPendingFx);var original=state.PendingFx!.Command;await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadBookConfigurationReviewAsync(client));state.DetachOwner(owner);state.AttachOwner(new object());Assert.Equal(original,state.PendingFx!.Command);Assert.True(company.TrySelect(company.Clear(),"actor",new(original.Company,["ConfiguredCompanyRole"],["Accounting.View"])));host.Context.Permissions=["Accounting.View"];f.Books.Reader.Native.FailCommit=false;await state.RecoverFxAsync(original.Id,client);Assert.False(state.HasPendingFx);Assert.Equal(original,CompanyFxRateProof.Require(state.FxResult!).Command);Assert.Equal(1,f.Books.Reader.Native.Writes);
    }
}
#endif
