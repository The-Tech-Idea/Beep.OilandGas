using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using Xunit;
using Xunit.Abstractions;

namespace Beep.OilandGas.ApiService.Tests;

public class AssetRolePermissionStoreTests(ITestOutputHelper output)
{
    [LocalDbModuleFact]
    public async Task PermissionMutationsUseIdentityClaimsAndPreserveExtensionHistory()
    {
        var database = $"BeepOilGas_PermissionStore_{Guid.NewGuid():N}";
        output.WriteLine($"Retained permission store database: {database}");
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Repository:Provider"] = "SqlServer",
            ["Repository:ConnectionString"] = $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true"
        }).Build();
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "actor"), new Claim(ClaimTypes.Role, "Administrator")
            }, "test"))
        } };
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOilGasRepository(configuration);
        services.AddSingleton<IHttpContextAccessor>(accessor);
        services.AddScoped<RepositoryRoleAssignmentService>();
        services.AddScoped<RepositoryApplicationRolePermissionStore>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RepositoryDbContext>();
        await db.Database.MigrateAsync();
        db.Users.Add(new OilGasUser { Id = "member", UserName = "member", NormalizedUserName = "MEMBER", IsActive = true });
        db.Roles.Add(new IdentityRole { Id = "reader", Name = "Reader", NormalizedName = "READER" });
        db.UserRoles.Add(new IdentityUserRole<string> { UserId = "member", RoleId = "reader" });
        db.Add(new AppPermissionExtension { PermissionId = "permission-id", PermissionKey = "asset.read" });
        await db.SaveChangesAsync();
        var store = scope.ServiceProvider.GetRequiredService<RepositoryApplicationRolePermissionStore>();
        var reader = new RepositoryApplicationAuthorizationReader(db);
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = new UserAssetAccessService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), new PPDMMappingService(),
            reader, store, Mock.Of<IUserAssetAccessStore>(), () => throw new InvalidOperationException("Application roles must not resolve a module database."));
        Assert.True(await service.AssignPermissionToRoleAsync("reader", "permission-id"));
        Assert.Equal(new[] { "permission-id" }, await service.GetRolePermissionsAsync("reader"));
        Assert.True(await service.HasPermissionAsync("member", "asset.read"));
        var history = await db.Set<AppRolePermissionExtension>().SingleAsync();
        Assert.Equal("actor", history.ApprovedByUserId);
        Assert.NotNull(history.RoleClaimId);
        Assert.True(await service.RemovePermissionFromRoleAsync("reader", "asset.read"));
        Assert.False(await service.HasPermissionAsync("member", "asset.read"));
        Assert.Empty(await db.RoleClaims.ToListAsync());
        Assert.NotNull(history.EffectiveToUtc);
        Assert.Null(history.RoleClaimId);
        Assert.False(await service.RemovePermissionFromRoleAsync("reader", "permission-id"));

        db.RoleClaims.AddRange(new IdentityRoleClaim<string> { RoleId = "reader", ClaimType = "permission", ClaimValue = "asset.read" },
            new IdentityRoleClaim<string> { RoleId = "reader", ClaimType = "permission", ClaimValue = "asset.read" });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemovePermissionFromRoleAsync("reader", "permission-id"));
        Assert.Equal(2, await db.RoleClaims.CountAsync());
        accessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "member") }, "test"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.AssignPermissionToRoleAsync("reader", "permission-id"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RemovePermissionFromRoleAsync("reader", "permission-id"));
        editor.VerifyNoOtherCalls();
    }
}
