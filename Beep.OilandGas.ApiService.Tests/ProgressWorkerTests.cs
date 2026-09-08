using System.Collections.Concurrent;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.DataManagement;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProgressWorkerTests
{
    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static ProgressTrackingService Create(Func<string, object?[], CancellationToken, Task> send, TimeProvider? clock = null)
    {
        var client = new Mock<IClientProxy>();
        client.Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(send);
        var hub = new Mock<IHubContext<ProgressHub>> { DefaultValue = DefaultValue.Mock };
        hub.Setup(h => h.Clients.Group(It.IsAny<string>())).Returns(client.Object);
        return new(hub.Object, NullLogger<ProgressTrackingService>.Instance, clock: clock);
    }

    [Fact]
    public async Task BroadcastsOwnedSnapshotsInOrder()
    {
        var seen = new ConcurrentQueue<int>();
        var complete = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var service = Create((_, args, _) =>
        {
            seen.Enqueue(((JsonElement)args[0]!).GetProperty("progressPercentage").GetInt32());
            if (seen.Count == 3) complete.TrySetResult();
            return Task.CompletedTask;
        });
        var id = service.StartOperation("test", "start");
        service.UpdateProgress(id, 50, "half");
        service.CompleteOperation(id, true);
        await service.StartAsync(default);
        try { await complete.Task.WaitAsync(TimeSpan.FromSeconds(10)); }
        finally { await service.StopAsync(default); }
        Assert.Equal(new[] { 0, 50, 100 }, seen);
    }

    [Fact]
    public async Task OverflowDropsOldestNotificationsButPreservesPollingState()
    {
        var count = 0;
        var complete = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var service = Create((_, _, _) =>
        {
            if (Interlocked.Increment(ref count) == 256) complete.TrySetResult();
            return Task.CompletedTask;
        });
        string? first = null;
        for (var i = 0; i < 300; i++)
        {
            var id = service.StartOperation("test", i.ToString());
            first ??= id;
        }
        Assert.NotNull(service.GetProgress(first!));
        await service.StartAsync(default);
        try { await complete.Task.WaitAsync(TimeSpan.FromSeconds(10)); }
        finally { await service.StopAsync(default); }
        Assert.Equal(256, count);
    }

    [Fact]
    public async Task SendFailureDoesNotStopWorker()
    {
        var count = 0;
        var complete = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var service = Create((_, _, _) =>
        {
            if (Interlocked.Increment(ref count) == 1) throw new InvalidOperationException("send failed");
            complete.TrySetResult();
            return Task.CompletedTask;
        });
        service.StartOperation("test", "one");
        service.StartOperation("test", "two");
        await service.StartAsync(default);
        try { await complete.Task.WaitAsync(TimeSpan.FromSeconds(10)); }
        finally { await service.StopAsync(default); }
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task ShutdownCancelsActiveSendAndDoesNotDrainPendingBroadcasts()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var count = 0;
        using var service = Create(async (_, _, token) =>
        {
            Interlocked.Increment(ref count);
            entered.TrySetResult();
            await Task.Delay(Timeout.Infinite, token);
        });
        service.StartOperation("test", "one");
        service.StartOperation("test", "two");
        await service.StartAsync(default);
        try { await entered.Task.WaitAsync(TimeSpan.FromSeconds(10)); }
        finally { await service.StopAsync(default).WaitAsync(TimeSpan.FromSeconds(10)); }
        Assert.Equal(1, count);
    }

    [Fact]
    public void PollingAndCallerMutationCannotChangeStoredOrTerminalStatus()
    {
        using var service = Create((_, _, _) => Task.CompletedTask);
        var input = new ProgressUpdate { OperationId = "id", StatusMessage = "original" };
        service.UpdateProgress(input);
        input.StatusMessage = "mutated";
        Assert.Equal("original", service.GetProgress("id")!.StatusMessage);
        service.CompleteOperation("id", true);
        var returned = service.GetProgress("id")!;
        returned.IsComplete = false;
        service.UpdateProgress("id", 1, "late");
        service.UpdateProgress(new ProgressUpdate { OperationId = "id", IsComplete = false });
        service.CancelOperation("id");
        var status = service.GetProgress("id")!;
        Assert.True(status.IsComplete);
        Assert.False(status.HasError);
        Assert.Equal(100, status.ProgressPercentage);
    }

    [Fact]
    public void RetentionDoesNotDependOnSuccessfulBroadcasts()
    {
        var clock = new Clock();
        using var service = Create((_, _, _) => throw new InvalidOperationException(), clock);
        var active = service.StartOperation("test", "active");
        var finished = service.StartOperation("test", "finished");
        var cancelled = service.StartOperation("test", "cancelled");
        service.CompleteOperation(finished, true);
        service.CancelOperation(cancelled);
        var workflow = service.StartWorkflow("test", new() { "step" });
        service.CompleteWorkflow(workflow, true);
        clock.Now += TimeSpan.FromMinutes(5);
        Assert.Null(service.GetProgress(finished));
        Assert.Null(service.GetProgress(cancelled));
        Assert.NotNull(service.GetProgress(active));
        Assert.NotNull(service.GetWorkflowProgress(workflow));
        clock.Now += TimeSpan.FromMinutes(5);
        Assert.Null(service.GetWorkflowProgress(workflow));
    }
}
