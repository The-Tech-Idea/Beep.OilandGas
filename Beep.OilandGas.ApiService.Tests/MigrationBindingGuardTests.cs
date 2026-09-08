using System.Reflection;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Migration;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class MigrationBindingGuardTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StaleOrUnavailableBindingBlocksApprovalExecutionAndQueueing(bool unavailable)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
            Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
            migrationBindingFingerprint: (_, _) => unavailable
                ? Task.FromException<string>(new InvalidOperationException("Repository unavailable")) : Task.FromResult("new-version"));
        var id = AddSession("old-version");
        var approval = await service.ApproveSchemaMigrationPlanAsync(new() { PlanId = id, ApprovedBy = "actor" });
        Assert.False(approval.Success);
        Assert.Contains("binding", approval.Message);
        var request = new SchemaMigrationExecuteRequest { PlanId = id, ExpectedPlanHash = "plan-hash", ExpectedManifestHash = "manifest-hash" };
        var execution = await service.ExecuteSchemaMigrationPlanAsync(request);
        Assert.False(execution.Success);
        Assert.Contains("binding", execution.Message);
        var queued = await service.StartSchemaMigrationExecutionAsync(request);
        Assert.False(queued.Success);
        Assert.Contains("binding", queued.Message);
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MatchingServerBindingAllowsApproval()
    {
        var service = new PPDM39SetupService(Mock.Of<IDMEEditor>(), NullLogger<PPDM39SetupService>.Instance,
            Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
            migrationBindingFingerprint: (ids, connection) =>
            {
                Assert.Equal("PRODUCTION", Assert.Single(ids));
                Assert.Equal("module-db", connection);
                return Task.FromResult("current-version");
            });
        var result = await service.ApproveSchemaMigrationPlanAsync(new() { PlanId = AddSession("current-version"), ApprovedBy = "actor" });
        Assert.True(result.Success);
    }

    // Seed only the in-process plan cache to test gates without opening a module database.
    private static string AddSession(string fingerprint)
    {
        var type = typeof(PPDM39SetupService).GetNestedType("SchemaMigrationPlanSession", BindingFlags.NonPublic)!;
        var session = Activator.CreateInstance(type)!;
        var id = Guid.NewGuid().ToString();
        type.GetProperty("ConnectionName")!.SetValue(session, "module-db");
        type.GetProperty("ModuleIds")!.SetValue(session, new[] { "PRODUCTION" });
        type.GetProperty("BindingFingerprint")!.SetValue(session, fingerprint);
        type.GetProperty("ManifestHash")!.SetValue(session, "manifest-hash");
        type.GetProperty("IsApproved")!.SetValue(session, true);
        type.GetProperty("Plan")!.SetValue(session, new MigrationPlanArtifact { PlanId = id, PlanHash = "plan-hash" });
        var cache = typeof(PPDM39SetupService).GetField("_schemaMigrationPlans", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        Assert.True((bool)cache.GetType().GetMethod("TryAdd")!.Invoke(cache, [id, session])!);
        return id;
    }
}
