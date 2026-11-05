using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyGLAccountHttpTests
{
    internal static CompanyClientContext Selected(string actor="actor",string[]? grants=null)
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(new("tenant","company"),["ConfiguredCompanyRole"],grants ??["Accounting.View"]));return company;}
    internal static AccountingServiceClient Client(HttpClient http,CompanyClientContext company)=>new(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
    [Theory][InlineData("valid")][InlineData("empty")][InlineData("encoded-book")][InlineData("auth")][InlineData("role")][InlineData("permission")]
    [InlineData("revoked")][InlineData("binding")][InlineData("invalid-binding")][InlineData("actor")][InlineData("roles")][InlineData("foreign")][InlineData("changed")][InlineData("schema")]
    public async Task ActualCompanyApiClientReturnsOnlyCompleteRecordedNativeAccounts(string mode)
    {
        var native=new CompanyGLAccountReviewTests.Fixture();var book=mode=="encoded-book"?"دفتر %2F/x":"statutory";native.Row.BOOK_ID=book;
        if(mode=="empty")native.Native.Rows.Clear();if(mode=="foreign"){native.Row.OWNER_COMPANY_BA_ID="foreign";native.OverrideRows=[native.Row];}
        if(mode=="schema")native.ChangeSchema=schema=>schema.Fields.Remove(schema.Fields.Single(field=>field.FieldName=="BOOK_ID"));
        await using var host=await AccountingSourceHttpTests.Start(authenticate:mode!="auth",role:mode!="role",permissions:mode=="permission"?[]:["Accounting.View"],
            configureAccounting:(services,context)=>{services.AddSingleton(native.Service);context.PolicyBinding=mode=="invalid-binding"?"unconfirmed":new string('B',64);
                native.OnRead=()=>{if(native.Reads!=1)return;switch(mode){case "revoked":context.Permissions=[];break;case "binding":context.PolicyBinding=new string('C',64);break;
                    case "actor":context.Actor="changed";break;case "roles":context.Roles=["changed"];break;}};
                if(mode=="changed")native.OnRead=()=>{if(native.Reads==2)native.Row.ACCOUNT_NAME="changed";};});
        using var company=Selected();var client=Client(host.Client,company);
        if(mode is "valid" or "empty" or "encoded-book")
        {var review=await client.GetCompanyGLAccountsAsync(book,"KWD");review.Require(new("tenant","company"),"actor",book,"KWD");Assert.Equal(mode=="empty"?0:1,review.Accounts.Count);}
        else
        {var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanyGLAccountsAsync(book,"KWD"));Assert.Equal(mode=="auth"?HttpStatusCode.Unauthorized:
            mode is "role" or "permission" or "revoked" or "binding" or "actor" or "roles"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);}
        if(mode is "auth" or "role" or "permission" or "invalid-binding" or "schema")Assert.Equal(0,native.Reads);native.NoWrites();
    }
    [Theory][InlineData("company")][InlineData("account")][InlineData("grant")]
    public async Task ClientDiscardsLateReviewAfterCompanyAccountOrAuthorityChange(string fault)
    {
        var native=new CompanyGLAccountReviewTests.Fixture();using var company=Selected();native.OnRead=()=>{if(native.Reads!=1)return;
            company.TrySelect(company.Clear(),fault=="account"?"other":"actor",new(new("tenant",fault=="company"?"other":"company"),["ConfiguredCompanyRole"],fault=="grant"?[]:["Accounting.View"]));};
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,context)=>{services.AddSingleton(native.Service);context.PolicyBinding=new string('B',64);});
        await Assert.ThrowsAnyAsync<Exception>(()=>Client(host.Client,company).GetCompanyGLAccountsAsync("statutory","KWD"));native.NoWrites();
    }
    [Theory][InlineData("book")][InlineData("currency")][InlineData("long-book")][InlineData("long-currency")]
    public async Task InvalidExplicitSelectionStopsBeforePrivateReads(string fault)
    {
        var native=new CompanyGLAccountReviewTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,context)=>{services.AddSingleton(native.Service);context.PolicyBinding=new string('B',64);});
        using var company=Selected();await Assert.ThrowsAnyAsync<Exception>(()=>Client(host.Client,company).GetCompanyGLAccountsAsync(fault=="book"?"":fault=="long-book"?new string('x',129):"statutory",fault=="currency"?"":fault=="long-currency"?new string('x',17):"KWD"));
        Assert.Equal(0,host.Context.Calls);Assert.Equal(0,native.Reads);native.NoWrites();
    }
}
#endif
