using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using Xunit;
using Xunit.Abstractions;

namespace Beep.OilandGas.ApiService.Tests;

public class AssetApplicationAuthorizationTests(ITestOutputHelper output)
{
    [LocalDbModuleFact]
    public async Task AssetServiceUsesIdentityMembershipAndClaimsWithoutDomainQueries()
    {
        var database = $"BeepOilGas_AssetRoles_{Guid.NewGuid():N}";
        output.WriteLine($"Retained authorization test database: {database}");
        await using var db = new SqlServerRepositoryDbContext(new DbContextOptionsBuilder<SqlServerRepositoryDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true").Options);
        await db.Database.MigrateAsync();
        var user = new OilGasUser { Id = "user", UserName = "user", NormalizedUserName = "USER", IsActive = true };
        db.Users.Add(user);
        db.Roles.Add(new IdentityRole { Id = "reader", Name = "Reader", NormalizedName = "READER" });
        var membership = new IdentityUserRole<string> { UserId = user.Id, RoleId = "reader" };
        db.UserRoles.Add(membership);
        db.RoleClaims.AddRange(
            new IdentityRoleClaim<string> { RoleId = "reader", ClaimType = "permission", ClaimValue = "asset.read" },
            new IdentityRoleClaim<string> { RoleId = "reader", ClaimType = "role", ClaimValue = "asset.write" });
        await db.SaveChangesAsync();
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = Create(editor.Object, new RepositoryApplicationAuthorizationReader(db));
        Assert.Equal(new[] { "Reader" }, await service.GetUserRolesAsync(user.Id));
        Assert.True(await service.HasPermissionAsync(user.Id, "asset.read"));
        Assert.False(await service.HasPermissionAsync(user.Id, "ASSET.READ"));
        Assert.False(await service.HasPermissionAsync(user.Id, "asset.write"));
        Assert.Empty(await service.GetUserRolesAsync("missing"));
        Assert.False(await service.HasPermissionAsync("missing", "asset.read"));

        user.IsActive = false;
        await db.SaveChangesAsync();
        Assert.Empty(await service.GetUserRolesAsync(user.Id));
        Assert.False(await service.HasPermissionAsync(user.Id, "asset.read"));
        user.IsActive = true;
        db.UserRoles.Remove(membership);
        await db.SaveChangesAsync();
        Assert.Empty(await service.GetUserRolesAsync(user.Id));
        Assert.False(await service.HasPermissionAsync(user.Id, "asset.read"));
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task OrganizationScopeIsNotSilentlyReplacedWithGlobalGrants()
    {
        var reader = new Mock<IApplicationAuthorizationReader>(MockBehavior.Strict);
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = Create(editor.Object, reader.Object);
        await Assert.ThrowsAsync<NotSupportedException>(() => service.GetUserRolesAsync("user", "organization"));
        await Assert.ThrowsAsync<NotSupportedException>(() => service.HasPermissionAsync("user", "asset.read", "organization"));
        reader.VerifyNoOtherCalls();
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RepositoryFailureCannotFallBackToLegacyRoleTables()
    {
        var reader = new Mock<IApplicationAuthorizationReader>(MockBehavior.Strict);
        reader.Setup(x => x.GetRolesAsync("user")).ThrowsAsync(new InvalidOperationException("repository unavailable"));
        reader.Setup(x => x.HasPermissionAsync("user", "read")).ThrowsAsync(new InvalidOperationException("repository unavailable"));
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = Create(editor.Object, reader.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetUserRolesAsync("user"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.HasPermissionAsync("user", "read"));
        editor.VerifyNoOtherCalls();
    }

    private static UserAssetAccessService Create(IDMEEditor editor, IApplicationAuthorizationReader reader) => new(editor,
        Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
        new PPDMMappingService(), reader, Mock.Of<IApplicationRolePermissionStore>(),
        Mock.Of<TheTechIdea.Data.OilGas.IUserAssetAccessStore>(), () => throw new InvalidOperationException("Application roles must not resolve a module database."));
}
