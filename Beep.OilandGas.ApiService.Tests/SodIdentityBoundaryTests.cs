using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;
using Xunit.Abstractions;

namespace Beep.OilandGas.ApiService.Tests;

public class SodIdentityBoundaryTests(ITestOutputHelper output)
{
    [LocalDbModuleFact]
    public async Task RoleChecksUseLocalDbIdentityAndOnlyReadRulesFromTheModule()
    {
        var database = $"BeepOilGas_Sod_{Guid.NewGuid():N}";
        output.WriteLine($"Retained SoD repository test database: {database}");
        await using var db = new SqlServerRepositoryDbContext(new DbContextOptionsBuilder<SqlServerRepositoryDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true").Options);
        await db.Database.MigrateAsync();
        db.Roles.AddRange(new IdentityRole { Id = "maker", Name = "Maker", NormalizedName = "MAKER" },
            new IdentityRole { Id = "checker", Name = "Checker", NormalizedName = "CHECKER" },
            new IdentityRole { Id = "empty", Name = "Empty", NormalizedName = "EMPTY" });
        db.RoleClaims.AddRange(
            new IdentityRoleClaim<string> { RoleId = "maker", ClaimType = "permission", ClaimValue = "create" },
            new IdentityRoleClaim<string> { RoleId = "maker", ClaimType = "permission", ClaimValue = "create" },
            new IdentityRoleClaim<string> { RoleId = "checker", ClaimType = "permission", ClaimValue = "approve" },
            new IdentityRoleClaim<string> { RoleId = "maker", ClaimType = "role", ClaimValue = "Administrator" });
        await db.SaveChangesAsync();
        var reader = new RepositoryRolePermissionReader(db, new UpperInvariantLookupNormalizer());
        Assert.Equal(new[] { "create" }, await reader.GetPermissionsAsync("maker"));
        Assert.Empty(await reader.GetPermissionsAsync("Empty"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.GetPermissionsAsync("missing"));

        var source = new Mock<IDataSource>(MockBehavior.Strict);
        source.Setup(x => x.GetEntityAsync("SOD_RULE", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(new List<SOD_RULE> { new()
            {
                SOD_RULE_ID = "rule", RULE_NAME = "maker-checker", CONFLICTING_PERMISSION_A = "create",
                CONFLICTING_PERMISSION_B = "approve", IS_BLOCKING = "Y"
            } });
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("selected-module")).Returns(source.Object);
        var engine = Create(editor.Object, () => Task.FromResult("selected-module"), reader);
        var result = await engine.CheckRoleCombinationAsync("Maker", "Checker");
        Assert.True(result.HasBlockingConflict);
        Assert.False(result.CanProceed);
        Assert.Single(result.Conflicts);
        source.Verify(x => x.GetEntityAsync("SOD_RULE", It.IsAny<List<AppFilter>>()), Times.Once);
        editor.Verify(x => x.GetDataSource("selected-module"), Times.Once);
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MissingReaderOrFailedIdentityLookupCannotReturnSuccessfulCheck()
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var engine = Create(editor.Object, () => Task.FromResult("selected"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => engine.CheckRoleCombinationAsync("a", "b"));
        var reader = new Mock<ISodRolePermissionReader>(MockBehavior.Strict);
        reader.Setup(x => x.GetPermissionsAsync("a")).ThrowsAsync(new InvalidOperationException("repository unavailable"));
        engine = Create(editor.Object, () => Task.FromResult("selected"), reader.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => engine.CheckRoleCombinationAsync("a", "b"));
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MissingModuleBindingRejectsRuleReadsAndSeeding()
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var engine = Create(editor.Object, null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => engine.GetAllRulesAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => engine.EvaluatePermissionsAsync(new()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => engine.SeedDefaultRulesAsync("actor"));
        editor.VerifyNoOtherCalls();
    }

    private static SodEvaluationEngine Create(IDMEEditor editor, Func<Task<string>>? connection,
        ISodRolePermissionReader? reader = null) =>
        new(editor, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            Mock.Of<IPPDMMetadataRepository>(), connection, rolePermissions: reader);
}
