using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using Xunit;
using Xunit.Abstractions;
using Moq;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Tests;

public class RepositoryAssetAccessStoreTests(ITestOutputHelper output)
{
    [LocalDbModuleFact]
    public async Task GrantsAreIdentityExtensionsScopedToOneDatabaseAndRevokedWithoutDeletion()
    {
        var database = $"BeepOilGas_AssetStore_{Guid.NewGuid():N}";
        output.WriteLine($"Retained asset store database: {database}");
        var options = new DbContextOptionsBuilder<SqlServerRepositoryDbContext>().UseSqlServer(
            $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true").Options;
        await using var db = new SqlServerRepositoryDbContext(options);
        await db.Database.MigrateAsync();
        db.Users.AddRange(new OilGasUser { Id = "actor", UserName = "actor", NormalizedUserName = "ACTOR", IsActive = true },
            new OilGasUser { Id = "member", UserName = "member", NormalizedUserName = "MEMBER", IsActive = true },
            new OilGasUser { Id = "inactive", UserName = "inactive", NormalizedUserName = "INACTIVE", IsActive = false });
        db.Roles.Add(new IdentityRole { Id = "admin", Name = "Administrator", NormalizedName = "ADMINISTRATOR" });
        db.UserRoles.Add(new IdentityUserRole<string> { UserId = "actor", RoleId = "admin" });
        await db.SaveChangesAsync();
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = Principal("actor") } };
        var store = new RepositoryAssetAccessStore(db, accessor);
        var first = new string('A', 64);
        var second = new string('B', 64);
        var currentScope = new AssetDatabaseScope("selected-module", first);
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = new UserAssetAccessService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), new PPDMMappingService(),
            new RepositoryApplicationAuthorizationReader(db), Mock.Of<IApplicationRolePermissionStore>(),
            store, () => Task.FromResult(currentScope));
        Assert.True(await service.GrantAssetAccessAsync("member", "same-id", "well", "write", false));
        Assert.True(await store.GrantAsync("member", second, "WELL", "same-id", "READ", false));
        Assert.True(await store.GrantAsync("member", first, "WELL", "same-id", "READ", false, "organization"));
        var initial = Assert.Single(await store.ReadAsync("member", first));
        Assert.Equal("WRITE", initial.AccessLevel);
        Assert.Equal("actor", initial.CreatedBy);
        Assert.Equal("actor", initial.ChangedBy);
        Assert.Equal("READ", Assert.Single(await store.ReadAsync("member", second)).AccessLevel);
        Assert.True((await service.CheckAssetAccessAsync("member", "same-id", "WELL")).HasAccess);
        currentScope = currentScope with { Fingerprint = second };
        Assert.Equal("READ", Assert.Single(await service.GetUserAccessibleAssetsAsync("member")).AccessLevel);
        currentScope = currentScope with { Fingerprint = new string('C', 64) };
        Assert.False((await service.CheckAssetAccessAsync("member", "same-id", "WELL")).HasAccess);
        currentScope = currentScope with { Fingerprint = first };
        Assert.Single(await store.ReadAsync("member", first, "organization"));
        Assert.Empty(await store.ReadAsync("member", first, "other-organization"));
        Assert.Empty(await store.ReadAsync("member", first, "ORGANIZATION"));
        Assert.False(await store.RevokeAsync("member", first, "WELL", "SAME-ID"));
        Assert.False(await store.GrantAsync("inactive", first, "WELL", "same-id", "READ", false));
        Assert.False(await store.GrantAsync("missing", first, "WELL", "same-id", "READ", false));
        await Assert.ThrowsAsync<ArgumentException>(() => store.GrantAsync("member", "connection-name", "WELL", "same-id", "READ", false));
        await Assert.ThrowsAsync<ArgumentException>(() => store.GrantAsync("member", first, "UNKNOWN", "same-id", "READ", false));
        await Assert.ThrowsAsync<ArgumentException>(() => store.GrantAsync("member", first, "WELL", "same-id", "OWNER", false));
        Assert.Equal(3, await db.Set<AppUserAssetAccess>().CountAsync());

        await using (var reopened = new SqlServerRepositoryDbContext(options))
        {
            var persisted = Assert.Single(await new RepositoryAssetAccessStore(reopened, accessor).ReadAsync("member", first));
            Assert.Equal(initial.Id, persisted.Id);
            Assert.Equal(initial.CreatedUtc, persisted.CreatedUtc);
        }
        Assert.True(await service.RevokeAssetAccessAsync("member", "same-id", "WELL"));
        Assert.Empty(await store.ReadAsync("member", first));
        Assert.Empty(await store.ReadAsync("member", first, "organization"));
        Assert.Single(await store.ReadAsync("member", second));
        Assert.Equal(3, await db.Set<AppUserAssetAccess>().CountAsync());
        Assert.False(await store.RevokeAsync("member", first, "WELL", "same-id"));
        Assert.True(await store.GrantAsync("member", first, "WELL", "same-id", "DELETE", true));
        var renewed = Assert.Single(await store.ReadAsync("member", first));
        Assert.Equal(initial.Id, renewed.Id);
        Assert.Equal(initial.CreatedUtc, renewed.CreatedUtc);
        Assert.NotEqual(initial.ConcurrencyStamp, renewed.ConcurrencyStamp);

        var member = await db.Users.SingleAsync(x => x.Id == "member");
        member.IsActive = false;
        await db.SaveChangesAsync();
        Assert.Empty(await store.ReadAsync("member", first));
        db.UserRoles.Remove(await db.UserRoles.SingleAsync());
        await db.SaveChangesAsync();
        await Refusals.ForbiddenAsync(() => store.RevokeAsync("member", first, "WELL", "same-id"));
        await Refusals.ForbiddenAsync(() => store.GrantAsync("member", first, "WELL", "same-id", "READ", false));
        accessor.HttpContext.User = Principal("member");
        await Refusals.ForbiddenAsync(() => store.RevokeAsync("member", first, "WELL", "same-id"));
        editor.VerifyNoOtherCalls();
    }

    private static ClaimsPrincipal Principal(string id) => new(new ClaimsIdentity(new[]
    {
        new Claim("party_id", id), new Claim(ClaimTypes.Role, "Administrator")
    }, "test"));
}
