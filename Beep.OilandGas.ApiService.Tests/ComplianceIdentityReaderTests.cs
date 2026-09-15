using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ComplianceIdentityReaderTests
{
    [Fact]
    public async Task ReportsReadIdentityMembershipAndOnlyPermissionClaimsWithoutAppExtensions()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var db = new TestContext(new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        db.Users.AddRange(new OilGasUser { Id = "u1", UserName = "operator" },
            new OilGasUser { Id = "u2", UserName = "unassigned" });
        db.Roles.AddRange(new IdentityRole { Id = "r1", Name = "Operator", NormalizedName = "OPERATOR" },
            new IdentityRole { Id = "r2", Name = "Reviewer", NormalizedName = "REVIEWER" });
        db.UserRoles.AddRange(new IdentityUserRole<string> { UserId = "u1", RoleId = "r1" },
            new IdentityUserRole<string> { UserId = "u1", RoleId = "r2" });
        db.RoleClaims.AddRange(
            new IdentityRoleClaim<string> { RoleId = "r1", ClaimType = "permission", ClaimValue = "read" },
            new IdentityRoleClaim<string> { RoleId = "r2", ClaimType = "permission", ClaimValue = "read" },
            new IdentityRoleClaim<string> { RoleId = "r1", ClaimType = "role", ClaimValue = "Administrator" });
        await db.SaveChangesAsync();
        var reader = new ComplianceIdentityReader(db);
        Assert.Equal((2, 2, 1), await reader.GetCountsAsync());
        var users = await reader.GetUsersAsync();
        Assert.Equal(new[] { "Operator", "Reviewer" }, users[0].Roles);
        Assert.Equal(new[] { "read" }, users[0].Permissions);
        Assert.Empty(users[1].Roles);
        Assert.Empty(users[1].Permissions);
        using var matrix = JsonDocument.Parse(await reader.GetRolePermissionMatrixJsonAsync());
        Assert.Equal(2, matrix.RootElement.EnumerateObject().Count());
        Assert.Equal("read", matrix.RootElement.GetProperty("Operator").GetProperty("permissions")[0].GetString());
        Assert.Equal(1, matrix.RootElement.GetProperty("Operator").GetProperty("permissionCount").GetInt32());
    }

    [Fact]
    public async Task MatrixDelegatesToIdentityWithoutOpeningModuleDatabase()
    {
        var identity = new Mock<IComplianceIdentityReader>();
        identity.Setup(x => x.GetRolePermissionMatrixJsonAsync()).ReturnsAsync("{\"identity\":true}");
        var service = new ComplianceReportService(null!, null!, null!, null!, identity: identity.Object,
            resolveConnection: () => throw new InvalidOperationException("Module must not be read"));
        Assert.Equal("{\"identity\":true}", await service.GenerateRolePermissionMatrixJsonAsync());
        identity.Verify(x => x.GetRolePermissionMatrixJsonAsync(), Times.Once);
    }

    [Fact]
    public async Task IdentityFailuresAreNotReportedAsEmptySuccess()
    {
        var identity = new Mock<IComplianceIdentityReader>();
        identity.Setup(x => x.GetUsersAsync()).ThrowsAsync(new InvalidOperationException("unavailable"));
        identity.Setup(x => x.GetRolePermissionMatrixJsonAsync()).ThrowsAsync(new InvalidOperationException("unavailable"));
        var service = new ComplianceReportService(null!, null!, null!, null!, identity: identity.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateUserAccessReportAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateRolePermissionMatrixJsonAsync());
    }

    [Fact]
    public async Task MissingWorkflowBindingCannotFallBackToGlobalDatabase()
    {
        var service = new ComplianceReportService(null!, null!, null!, null!);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetSodSummaryReportAsync());
        Assert.Contains("bound LIFECYCLE", error.Message);
    }

    private sealed class TestContext(DbContextOptions<TestContext> options) : RepositoryDbContext(options);
}
