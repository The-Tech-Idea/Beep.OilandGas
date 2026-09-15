using System.Reflection;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class WorkflowBackgroundStartupTests
{
    [Theory]
    [InlineData(RepositoryReadiness.BootstrapRequired)]
    [InlineData(RepositoryReadiness.MigrationRequired)]
    [InlineData(RepositoryReadiness.RecoveryRequired)]
    [InlineData(RepositoryReadiness.Unavailable)]
    public async Task RepositoryNotReadyCannotAccessModuleDatasource(RepositoryReadiness status)
    {
        var readiness = new Mock<IRepositoryReadinessService>();
        readiness.Setup(x => x.CheckAsync(It.IsAny<CancellationToken>())).ReturnsAsync(status);
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var gate = new WorkflowBackgroundConnection(readiness.Object, null!, editor.Object,
            Array.Empty<IModuleSetup>(), NullLogger<WorkflowBackgroundConnection>.Instance);
        Assert.Null(await gate.ResolveAsync(default));
        editor.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SlaMonitorWithoutApprovedTargetDoesNotResolveBusinessServices(bool hasResolver)
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var calls = 0;
        using var monitor = new SlaMonitorService(services.GetRequiredService<IServiceScopeFactory>(),
            resolveConnection: hasResolver ? (_, _) => { calls++; return Task.FromResult<string?>(null); } : null);
        var check = typeof(SlaMonitorService).GetMethod("CheckSlaBreachesAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
        await (Task)check.Invoke(monitor, [CancellationToken.None])!;
        Assert.Equal(hasResolver ? 1 : 0, calls);
    }
}
