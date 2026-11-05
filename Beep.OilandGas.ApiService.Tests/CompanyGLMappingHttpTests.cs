#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Globalization;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyGLMappingHttpTests
{
    [Theory][InlineData("valid")][InlineData("empty")][InlineData("ar-SA")][InlineData("role")][InlineData("permission")][InlineData("auth")][InlineData("revoked")]
    [InlineData("binding")][InlineData("actor")][InlineData("roles")][InlineData("foreign")][InlineData("date")][InlineData("account")]
    public async Task ActualMappingApiAndTypedClientRequireExplicitDateAndCurrentWholeSource(string mode)
    {
        var native=new CompanyGLMappingReviewTests.Fixture();if(mode=="empty")native.Accounts.Native.Rows.Remove(native.Mapping);
        if(mode=="foreign"){native.Mapping.OWNER_COMPANY_BA_ID="foreign";native.OverrideRows=[native.Mapping];}if(mode=="date")native.Mapping.EFFECTIVE_DATE=null;
        await using var host=await AccountingSourceHttpTests.Start(authenticate:mode!="auth",role:mode!="role",permissions:mode=="permission"?[]:["Accounting.View"],
            configureAccounting:(services,context)=>{services.AddSingleton(native.Service);services.AddSingleton(native.Accounts.Service);context.PolicyBinding=new string('B',64);
                native.OnMappingRead=()=>{if(native.MappingReads!=1)return;switch(mode){case "revoked":context.Permissions=[];break;case "binding":context.PolicyBinding=new string('C',64);break;
                    case "actor":context.Actor="other";break;case "roles":context.Roles=["changed"];break;case "account":native.Accounts.Row.ACCOUNT_NAME="changed";break;}};});
        using var company=CompanyGLAccountHttpTests.Selected();var client=CompanyGLAccountHttpTests.Client(host.Client,company);
        var oldCulture=CultureInfo.CurrentCulture;var oldUICulture=CultureInfo.CurrentUICulture;
        try
        {
            if(mode=="ar-SA")CultureInfo.CurrentCulture=CultureInfo.CurrentUICulture=new CultureInfo("ar-SA");
            if(mode is "valid" or "empty" or "ar-SA")
            {var review=await client.GetCompanyGLMappingsAsync("statutory","KWD",CompanyGLMappingReviewTests.Date);review.Require(new("tenant","company"),"actor","statutory","KWD",CompanyGLMappingReviewTests.Date);
                if(mode=="empty")Assert.Empty(review.Records);else Assert.Equal("actual-account-id",review.Resolve("AccountsReceivable","ASSET","DEBIT").GL_ACCOUNT_ID);}
            else{var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanyGLMappingsAsync("statutory","KWD",CompanyGLMappingReviewTests.Date));Assert.Equal(mode=="auth"?HttpStatusCode.Unauthorized:
                mode is "role" or "permission" or "revoked" or "binding" or "actor" or "roles"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);}
        }
        finally{CultureInfo.CurrentCulture=oldCulture;CultureInfo.CurrentUICulture=oldUICulture;}
        if(mode is "role" or "permission" or "auth"){Assert.Equal(0,native.MappingReads);Assert.Equal(0,native.Accounts.Reads);}native.Accounts.NoWrites();
    }
    [Theory][InlineData("timestamp")][InlineData("utc")][InlineData("missing")]
    public async Task InvalidClientDateStopsBeforeApiAndRawTimestampNeverBecomesACompanyDate(string mode)
    {
        var native=new CompanyGLMappingReviewTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,context)=>{services.AddSingleton(native.Service);services.AddSingleton(native.Accounts.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected();var client=CompanyGLAccountHttpTests.Client(host.Client,company);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetCompanyGLMappingsAsync("statutory","KWD",mode=="missing"?default:mode=="utc"?DateTime.SpecifyKind(CompanyGLMappingReviewTests.Date,DateTimeKind.Utc):CompanyGLMappingReviewTests.Date.AddHours(1)));
        Assert.Equal(0,host.Context.Calls);using var response=await host.Client.GetAsync("api/accounting/traditional/gl/company-accounts/mappings?book=statutory&currency=KWD&date=2026-10-12T01:00:00Z");
        Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);Assert.Equal(0,native.MappingReads);native.Accounts.NoWrites();
    }
}
#endif
