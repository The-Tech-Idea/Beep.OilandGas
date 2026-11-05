#if WORKFLOW_INBOX_FOCUSED
using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentFacilityReadAuthorityTests
{
    private static DevelopmentFacilityReadAuthority Authority(DevelopmentDraftAuthorityTests.Fixture f)=>new(f.Account,f.Company,f.Fields.Object,f.Preference.Object,f.Assets.Object,f.Grants.Object);
    [Fact]
    public async Task InTransactionCheckUsesAppStateAndClosingConfirmationRechecksField()
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();var scope=await Authority(f).CaptureAsync(f.User);var reads=f.FieldReads;
        await scope.CheckCurrentAsync();Assert.Equal(reads,f.FieldReads);
        f.Context=f.Context with{FieldId="different"};await Assert.ThrowsAnyAsync<Exception>(()=>scope.ConfirmAsync());
    }
    [Fact]
    public async Task OriginalRecoveryUsesReadAccessWithoutEditPermission()
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();f.Permissions=[Beep.OilandGas.Models.Data.Security.PermissionConstants.DevelopmentPlanning.View];
        f.Tenant.SelectCompany((await new Beep.OilandGas.Repository.RepositoryCompanyContextResolver(f.Accounts.Object).ResolveAsync("planner",f.Context.Company))!);
        f.Assets.Setup(a=>a.CheckAssetAccessAsync("planner","id","FACILITY","READ","PROCESSING")).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        var scope=await Authority(f).CaptureRecoveryAsync(f.User,"id","PROCESSING");await scope.ConfirmAsync();
        f.Assets.Verify(a=>a.CheckAssetAccessAsync("planner","id","FACILITY","READ","PROCESSING"),Moq.Times.AtLeast(2));
    }
    [Fact]
    public async Task CurrentCompanyRolesFieldAndReadGrantAreCheckedBeforeAndAfterTheSourceRead()
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();var scope=await Authority(f).CaptureAsync(f.User);
        Assert.Equal(f.Context,scope.Context);var prior=f.FieldReads;await scope.ConfirmAsync();Assert.True(f.FieldReads>prior);Assert.True(f.AssetReads>=2);
        f.Assets.Verify(a=>a.CheckAssetAccessAsync("planner","field","FIELD","READ",null),Moq.Times.AtLeast(2));
    }
    [Theory][InlineData("account")][InlineData("company")][InlineData("role")][InlineData("claim")][InlineData("permission")][InlineData("field")][InlineData("stored")]
    public async Task MissingCurrentAuthorityRefusesFacilityRead(string fault)
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();switch(fault){case "account":f.Active=false;break;case "company":f.Available=false;break;case "role":f.Roles=[];break;
            case "claim":f.User=new(new ClaimsIdentity([new Claim("party_id","planner")],"fixture"));f.Http.HttpContext!.User=f.User;break;
            case "permission":f.Permissions=[];break;case "field":f.FieldAccess=false;break;case "stored":f.Stored=false;break;}
        await Assert.ThrowsAnyAsync<Exception>(()=>Authority(f).CaptureAsync(f.User));
    }
    [Theory][InlineData("account")][InlineData("company")][InlineData("role")][InlineData("permission")][InlineData("field")][InlineData("stored")][InlineData("grant")][InlineData("selection")]
    public async Task ChangedAuthorityRefusesPublishingReadRows(string fault)
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();var scope=await Authority(f).CaptureAsync(f.User);
        switch(fault){case "account":f.Active=false;break;case "company":f.Available=false;break;case "role":f.Roles=["AnotherRole"];break;
            case "permission":f.Permissions=[];break;case "field":f.FieldAccess=false;break;case "stored":f.Stored=false;break;
            case "grant":f.GrantChanged=true;break;case "selection":f.Context=f.Context with{FieldId="different"};break;}
        await Assert.ThrowsAnyAsync<Exception>(()=>scope.ConfirmAsync());
    }
}
#endif
