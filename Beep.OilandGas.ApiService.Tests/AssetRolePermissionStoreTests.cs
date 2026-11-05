using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
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
using Xunit;
using Xunit.Abstractions;
using Beep.OilandGas.ApiService.Tests.Infrastructure;

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
                new Claim("party_id", "actor"), new Claim(ClaimTypes.Role, "Administrator")
            }, "test"))
        } };
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOilGasRepository(configuration);
        services.AddSingleton<IHttpContextAccessor>(accessor);
        services.AddSingleton<Func<ClaimsPrincipal, string?>>(_ => principal => principal.FindActingUserId());
        services.AddScoped<RepositoryRoleAssignmentService>();
        services.AddScoped<RepositoryRequestAccount>();
        services.AddScoped<RepositoryApplicationRolePermissionStore>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RepositoryDbContext>();
        await db.Database.MigrateAsync();
        db.Users.AddRange(new OilGasUser { Id = "member", UserName = "member", NormalizedUserName = "MEMBER", IsActive = true },
            new OilGasUser { Id = "actor", UserName = "actor", NormalizedUserName = "ACTOR", IsActive = true });
        db.Roles.AddRange(new IdentityRole { Id = "reader", Name = "Reader", NormalizedName = "READER" },
            new IdentityRole { Id = "admin", Name = "Administrator", NormalizedName = "ADMINISTRATOR" });
        db.UserRoles.AddRange(new IdentityUserRole<string> { UserId = "member", RoleId = "reader" },
            new IdentityUserRole<string> { UserId = "actor", RoleId = "admin" });
        db.Add(new AppUserRoleExtension { UserId = "actor", RoleId = "admin", GrantedByUserId = "actor",
            AssignmentReason = "Repository administration fixture; company selection is separate." });
        db.Add(new AppPermissionExtension { PermissionId = "permission-id", PermissionKey = "asset.read" });
        await db.SaveChangesAsync();
        var store = scope.ServiceProvider.GetRequiredService<RepositoryApplicationRolePermissionStore>();
        var reader = new RepositoryApplicationAuthorizationReader(await TestCompanyModules.CreateAsync(db, "actor"));
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = new UserAssetAccessService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
            reader, store, Mock.Of<IUserAssetAccessStore>(), () => throw new InvalidOperationException("Application roles must not resolve a module database."),
            _ => throw new InvalidOperationException("Application roles must not query asset rows."), Mock.Of<ICompanyFieldInterestReview>());
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
        accessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("party_id", "member") }, "test"));
        await Refusals.ForbiddenAsync(() => service.AssignPermissionToRoleAsync("reader", "permission-id"));
        await Refusals.ForbiddenAsync(() => service.RemovePermissionFromRoleAsync("reader", "permission-id"));
        editor.VerifyNoOtherCalls();
    }
}
