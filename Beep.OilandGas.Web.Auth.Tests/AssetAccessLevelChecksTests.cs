using Beep.OilandGas.Models.Data.AccessControl;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class AssetAccessLevelChecksTests
{
    [Theory][InlineData("READ","READ",true)][InlineData("READ","WRITE",false)][InlineData("READ","DELETE",false)][InlineData("WRITE","READ",true)][InlineData("WRITE","WRITE",true)][InlineData("WRITE","DELETE",false)][InlineData("DELETE","READ",true)][InlineData("DELETE","WRITE",true)][InlineData("DELETE","DELETE",true)][InlineData("read","WRITE",false)][InlineData("write","read",true)]
    public void ResourceLevelMustMeetRequestedOperation(string actual,string requested,bool allowed)=>Assert.Equal(allowed,AssetAccessLevelChecks.Meets(actual,requested));
    [Theory][InlineData(null)][InlineData("")][InlineData("ADMIN")][InlineData("READ ")]
    public void MissingOrUnrecognizedGrantNeverAuthorizes(string? actual)=>Assert.False(AssetAccessLevelChecks.Meets(actual,"READ"));
    [Theory][InlineData("HSE.View")][InlineData("Production.Update")][InlineData(null)][InlineData("")]
    public void SeparateAppPermissionNamesDoNotBecomeInventedResourceLevels(string? permission){Assert.True(AssetAccessLevelChecks.Meets("READ",permission));Assert.False(AssetAccessLevelChecks.Meets(null,permission));}
    [Fact] public void WriteLevelRevokedDuringAppPermissionReviewCannotRetainOldAuthority(){Assert.True(AssetAccessLevelChecks.Meets("WRITE","WRITE"));Assert.False(AssetAccessLevelChecks.Meets("READ","WRITE"));}
    [Theory][InlineData("READ",false)][InlineData("WRITE",false)][InlineData("DELETE",false)][InlineData("write",false)][InlineData("HSE.View",true)][InlineData("Facilities.Edit",true)][InlineData("WRITE ",true)][InlineData("asset.read",true)][InlineData(null,false)][InlineData("",false)]
    public void ResourceOperationsDoNotRequireInventedApplicationPermissionNames(string? request,bool required)=>Assert.Equal(required,AssetAccessLevelChecks.RequiresApplicationPermission(request));
}
