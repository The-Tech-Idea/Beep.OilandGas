using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class AccountingPeriodRoleChecksTests
{
    [Theory]
    [InlineData("Accountant","Accountant",true)][InlineData("Administrator","Administrator",true)]
    [InlineData("Reader","Reader",false)][InlineData("Administrator","Reader",false)]
    [InlineData("Accountant","Administrator",false)][InlineData("Reader","Accountant",false)]
    public void StandardRoleMustMatchAuthoritativeCompanyRole(string claim,string assigned,bool expected)
    {
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,claim)],"test"));
        Assert.Equal(expected,AccountingPeriodRoleChecks.CanAct(user,new("tenant","A"),new(new("tenant","A"),[assigned],[])));
    }
    [Fact] public void CompanyCollisionAndRevokedOrMissingAssignmentCannotAuthorize()
    {
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,"Administrator")],"test"));var expected=new CompanyScope("tenant","A");
        Assert.False(AccountingPeriodRoleChecks.CanAct(user,expected,new(new("tenant","B"),["Administrator"],[])));
        Assert.False(AccountingPeriodRoleChecks.CanAct(user,expected,new(expected,[],[])));
        Assert.False(AccountingPeriodRoleChecks.CanAct(user,expected,null));
    }
    [Fact] public void RoleClaimWithoutAuthenticationCannotAuthorize()
    {
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,"Accountant")]));
        Assert.False(AccountingPeriodRoleChecks.CanAct(user,new("tenant","A"),new(new("tenant","A"),["Accountant"],[])));
    }
}
