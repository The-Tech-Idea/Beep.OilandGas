using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class SetupWizardJobTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task RealExecutorForwardsConnectionAndActorAndRejectsPartialFailure(bool success, bool hasErrors)
    {
        var editor = new Mock<IDMEEditor>();
        editor.Setup(e => e.OpenDataSource("selected")).Returns(System.Data.ConnectionState.Open);
        var module = new Mock<Beep.OilandGas.PPDM39.Core.Interfaces.IModuleSetup>();
        module.Setup(m => m.SeedAsync("selected", "actor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Interfaces.ModuleSetupResult
            { Success = success, Errors = hasErrors ? new() { "row failed" } : new() });
        var executor = new SetupWizardExecutor(editor.Object, new[] { module.Object }, NullLogger<PpdmModuleSeedingService>.Instance);
        var run = executor.ExecuteAsync(new("id", "selected", "actor"), Mock.Of<IProgress<WizardProgress>>(), default);
        if (success && !hasErrors) await run;
        else await Assert.ThrowsAsync<InvalidOperationException>(() => run);
        module.Verify(m => m.SeedAsync("selected", "actor", It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class Executor : ISetupWizardExecutor, IDisposable
    {
        public Func<SetupWizardJob, IProgress<WizardProgress>, CancellationToken, Task> Work = (_, _, _) => Task.CompletedTask;
        public bool Disposed;
        public bool FailDisposal;
        public int Calls;
        public Task ExecuteAsync(SetupWizardJob job, IProgress<WizardProgress> progress, CancellationToken token)
        { Calls++; return Work(job, progress, token); }
        public void Dispose()
        {
            Disposed = true;
            if (FailDisposal) throw new InvalidOperationException("private disposal detail");
        }
    }

    private sealed class Harness : IAsyncDisposable
    {
        public readonly Executor Executor = new();
        public readonly ServiceProvider Services;
        public readonly ScopedBackgroundOperationQueue Queue;
        public readonly SetupWizardCoordinator Coordinator;
        public Harness()
        {
            SetupWizardCoordinator? coordinator = null;
            Services = new ServiceCollection()
                .AddSingleton(_ => coordinator!)
                .AddScoped<ISetupWizardExecutor>(_ => Executor)
                .AddScoped<SetupWizardJobRunner>().BuildServiceProvider(validateScopes: true);
            Queue = new(Services.GetRequiredService<IServiceScopeFactory>(), NullLogger<ScopedBackgroundOperationQueue>.Instance);
            Coordinator = coordinator = new(Queue);
        }
        public async Task WaitFor(BackgroundOperationState state)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (Queue.GetStatus("setup-wizard")?.State != state) await Task.Delay(10, timeout.Token);
        }
        public async ValueTask DisposeAsync()
        { await Queue.StopAsync(default); Queue.Dispose(); await Services.DisposeAsync(); Coordinator.Dispose(); }
    }

    [Fact]
    public async Task ControllerForwardsActorAndConnectionAndRejectsDuplicate()
    {
        await using var h = new Harness();
        SetupWizardJob? received = null;
        h.Executor.Work = (job, progress, _) => { received = job; progress.Report(new(2, 2, "done")); return Task.CompletedTask; };
        var controller = new SetupWizardController(Mock.Of<IDMEEditor>(), h.Coordinator)
        {
            ControllerContext = new() { HttpContext = new DefaultHttpContext
            { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "operator-7") }, "test")) } }
        };
        Assert.IsType<AcceptedResult>(controller.StartWizard("Field-West"));
        Assert.IsType<ConflictObjectResult>(controller.StartWizard("Other"));
        await h.Queue.StartAsync(default);
        await h.WaitFor(BackgroundOperationState.Succeeded);
        Assert.Equal("Field-West", received!.ConnectionName);
        Assert.Equal("operator-7", received.UserId);
        Assert.Equal("Completed", h.Coordinator.GetStatus().State);
        Assert.True(h.Executor.Disposed);
        Assert.False(h.Coordinator.Cancel());
    }

    [Fact]
    public async Task QueuedCancellationDoesNotExecuteModules()
    {
        await using var h = new Harness();
        Assert.True(h.Coordinator.TryStart("db", "user"));
        Assert.True(h.Coordinator.Cancel());
        await h.Queue.StartAsync(default);
        await h.WaitFor(BackgroundOperationState.Cancelled);
        Assert.Equal(0, h.Executor.Calls);
        Assert.Equal("Cancelled", h.Coordinator.GetStatus().State);
    }

    [Fact]
    public async Task RunningCancellationReachesWorkerAndAllowsRestartAfterDisposal()
    {
        await using var h = new Harness();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        h.Executor.Work = async (_, _, token) => { entered.SetResult(); await Task.Delay(Timeout.Infinite, token); };
        Assert.True(h.Coordinator.TryStart("db", "user"));
        await h.Queue.StartAsync(default);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(h.Coordinator.Cancel());
        await h.WaitFor(BackgroundOperationState.Cancelled);
        Assert.True(h.Executor.Disposed);
        Assert.Equal("Cancelled", h.Coordinator.GetStatus().State);
        h.Executor.Work = (_, _, _) => Task.CompletedTask;
        Assert.True(h.Coordinator.TryStart("next", "user"));
        await h.WaitFor(BackgroundOperationState.Succeeded);
    }

    [Fact]
    public async Task FailureIsSanitizedAndLateProgressCannotChangeTerminalState()
    {
        await using var h = new Harness();
        IProgress<WizardProgress>? retained = null;
        h.Executor.Work = (_, progress, _) => { retained = progress; throw new InvalidOperationException("private connection detail"); };
        Assert.True(h.Coordinator.TryStart("db", "user"));
        await h.Queue.StartAsync(default);
        await h.WaitFor(BackgroundOperationState.Failed);
        var status = h.Coordinator.GetStatus();
        Assert.Equal("Failed", status.State);
        Assert.DoesNotContain("private", status.Message);
        retained!.Report(new(1, 1, "late"));
        Assert.Equal(status, h.Coordinator.GetStatus());
    }

    [Fact]
    public async Task ScopeDisposalFailureOverridesCompletedWorkerStatus()
    {
        await using var h = new Harness();
        h.Executor.FailDisposal = true;
        Assert.True(h.Coordinator.TryStart("db", "user"));
        await h.Queue.StartAsync(default);
        await h.WaitFor(BackgroundOperationState.Failed);
        Assert.Equal("Failed", h.Coordinator.GetStatus().State);
        Assert.DoesNotContain("private", h.Coordinator.GetStatus().Message);
    }

    [Fact]
    public async Task RejectedAdmissionAndInvalidRequestsLeaveStatusIdle()
    {
        await using var h = new Harness();
        var controller = new SetupWizardController(Mock.Of<IDMEEditor>(), h.Coordinator)
        { ControllerContext = new() { HttpContext = new DefaultHttpContext() } };
        Assert.IsType<BadRequestObjectResult>(controller.StartWizard(" "));
        Assert.IsType<UnauthorizedResult>(controller.StartWizard("db"));
        await h.Queue.StopAsync(default);
        Assert.False(h.Coordinator.TryStart("db", "user"));
        Assert.Equal("Idle", h.Coordinator.GetStatus().State);
    }
}
