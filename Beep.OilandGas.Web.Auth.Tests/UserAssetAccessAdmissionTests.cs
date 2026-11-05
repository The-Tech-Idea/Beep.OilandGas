using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using System.Reflection;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class UserAssetAccessAdmissionTests
{
    public class BoundaryProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Call { get; set; } =
            (method, _) => throw new InvalidOperationException($"Unexpected boundary: {method.Name}");
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Call(method!, args);
    }
    private static T Boundary<T>(Func<MethodInfo, object?[]?, object?>? call = null) where T : class
    {
        var result = DispatchProxy.Create<T, BoundaryProxy>();
        if (call is not null) ((BoundaryProxy)(object)result).Call = call;
        return result;
    }

    [Theory]
    [InlineData("missing", 0)]
    [InlineData("inherited-field", 0)]
    [InlineData("inactive-order", 0)]
    [InlineData("direct-order-without-row-authority", 1)]
    public async Task OrderAccessRequiresActiveDirectGrantAndIndependentRowAuthority(string scenario, int expectedPolicyReads)
    {
        var scope = new AssetDatabaseScope("reviewed-company-core", new string('A', 64));
        var grants = scenario == "missing" ? new List<AppUserAssetAccess>() :
            [new() { Id = "grant", UserId = "actor", DatabaseScope = scope.Fingerprint,
                AssetType = scenario == "inherited-field" ? "FIELD" : "WORK_ORDER",
                AssetId = scenario == "inherited-field" ? "field" : "order", AccessLevel = "WRITE",
                Inherit = true, IsActive = scenario != "inactive-order" }];
        var storeReads = 0;
        var store = Boundary<IUserAssetAccessStore>((method, args) =>
        {
            Assert.Equal(nameof(IUserAssetAccessStore.ReadAsync), method.Name);
            Assert.Equal("actor", args![0]); Assert.Equal(scope.Fingerprint, args[1]);
            storeReads++; return Task.FromResult(grants);
        });
        var policyReads = 0;
        var policy = Boundary<IPPDMRowAccessPolicy>((method, args) =>
        {
            Assert.Equal(nameof(IPPDMRowAccessPolicy.GetReadConjunctionsAsync), method.Name);
            Assert.Equal(typeof(Beep.OilandGas.PPDM39.Models.WORK_ORDER), args![0]);
            Assert.Equal("WORK_ORDER", args[1]); policyReads++;
            return Task.FromResult<IReadOnlyList<IReadOnlyList<AppFilter>>>([]);
        });
        // Every other boundary throws, including editor/datasource and app-permission calls.
        var service = new UserAssetAccessService(Boundary<IDMEEditor>(), Boundary<ICommonColumnHandler>(),
            Boundary<IPPDM39DefaultsRepository>(), Boundary<IPPDMMetadataRepository>(),
            Boundary<IApplicationAuthorizationReader>(), Boundary<IApplicationRolePermissionStore>(), store,
            () => Task.FromResult(scope), user => { Assert.Equal("actor", user); return policy; },
            Boundary<ICompanyFieldInterestReview>());
        var result = await service.CheckAssetAccessAsync("actor", "order", "WORK_ORDER", "WRITE");
        Assert.False(result.HasAccess); Assert.Equal(expectedPolicyReads, policyReads); Assert.Equal(2, storeReads);
    }
}
