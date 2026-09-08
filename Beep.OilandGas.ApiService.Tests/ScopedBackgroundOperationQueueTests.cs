using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ScopedBackgroundOperationQueueTests
{
    public sealed class ScopedProbe : IAsyncDisposable
    {
        public bool Disposed { get; private set; }
        public ValueTask DisposeAsync() { Disposed = true; return ValueTask.CompletedTask; }
    }

    private static ScopedBackgroundOperationQueue Create(ServiceProvider services) => new(
        services.GetRequiredService<IServiceScopeFactory>(), NullLogger<ScopedBackgroundOperationQueue>.Instance);

    private static TaskCompletionSource<T> Signal<T>() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static async Task WaitFor(ScopedBackgroundOperationQueue queue, string key, BackgroundOperationState state)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (queue.GetStatus(key)?.State != state) await Task.Delay(10, timeout.Token);
    }

    [Fact]
    public async Task JobUsesFreshScopeAfterRequestScopeIsDisposed()
    {
        await using var services = new ServiceCollection().AddScoped<ScopedProbe>().BuildServiceProvider(validateScopes: true);
        using var queue = Create(services);
        ScopedProbe requestProbe;
        var seen = Signal<ScopedProbe>();
        await using (var request = services.CreateAsyncScope())
        {
            requestProbe = request.ServiceProvider.GetRequiredService<ScopedProbe>();
            Assert.True(queue.TryEnqueue<ScopedProbe, TaskCompletionSource<ScopedProbe>>("job", seen,
                static (probe, completion, token) => { completion.SetResult(probe); return Task.CompletedTask; }));
        }
        Assert.True(requestProbe.Disposed);
        await queue.StartAsync(default);
        try
        {
            var jobProbe = await seen.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await WaitFor(queue, "job", BackgroundOperationState.Succeeded);
            Assert.NotSame(requestProbe, jobProbe);
            Assert.True(jobProbe.Disposed);
        }
        finally { await queue.StopAsync(default); }
    }

    [Fact]
    public async Task RejectsDuplicateActiveJobAndRecordsFailure()
    {
        await using var services = new ServiceCollection().AddScoped<ScopedProbe>().BuildServiceProvider();
        using var queue = Create(services);
        Assert.True(queue.TryEnqueue<ScopedProbe, int>("job", 0,
            static (_, _, _) => throw new InvalidOperationException("private provider detail")));
        Assert.False(queue.TryEnqueue<ScopedProbe, int>("job", 0, static (_, _, _) => Task.CompletedTask));
        await queue.StartAsync(default);
        try
        {
            await WaitFor(queue, "job", BackgroundOperationState.Failed);
            Assert.DoesNotContain("private provider detail", queue.GetStatus("job")!.Error);
            Assert.True(queue.TryEnqueue<ScopedProbe, int>("job", 0, static (_, _, _) => Task.CompletedTask));
            await WaitFor(queue, "job", BackgroundOperationState.Succeeded);
        }
        finally { await queue.StopAsync(default); }
    }

    [Fact]
    public async Task ShutdownWaitsForActiveWorkAndCancelsQueuedWork()
    {
        await using var services = new ServiceCollection().AddScoped<ScopedProbe>().BuildServiceProvider();
        using var queue = Create(services);
        var started = Signal<ScopedProbe>();
        var release = Signal<bool>();
        Assert.True(queue.TryEnqueue<ScopedProbe, (TaskCompletionSource<ScopedProbe>, TaskCompletionSource<bool>)>(
            "active", (started, release), static async (probe, state, token) =>
            { state.Item1.SetResult(probe); await state.Item2.Task; }));
        Assert.True(queue.TryEnqueue<ScopedProbe, int>("pending", 0, static (_, _, _) => Task.CompletedTask));
        await queue.StartAsync(default);
        var probe = await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var stopping = queue.StopAsync(new CancellationToken(true));
        try
        {
            Assert.False(stopping.IsCompleted);
            Assert.False(probe.Disposed);
            Assert.False(queue.TryEnqueue<ScopedProbe, int>("late", 0, static (_, _, _) => Task.CompletedTask));
        }
        finally { release.TrySetResult(true); await stopping.WaitAsync(TimeSpan.FromSeconds(10)); }
        Assert.True(probe.Disposed);
        Assert.Equal(BackgroundOperationState.Succeeded, queue.GetStatus("active")!.State);
        Assert.Equal(BackgroundOperationState.Cancelled, queue.GetStatus("pending")!.State);
    }

    [Fact]
    public async Task AdmissionIsBoundedAndStopBeforeStartCancelsPendingWork()
    {
        await using var services = new ServiceCollection().BuildServiceProvider();
        using var queue = Create(services);
        for (var i = 0; i < 64; i++)
            Assert.True(queue.TryEnqueue<ScopedProbe, int>(i.ToString(), 0, static (_, _, _) => Task.CompletedTask));
        Assert.False(queue.TryEnqueue<ScopedProbe, int>("overflow", 0, static (_, _, _) => Task.CompletedTask));
        await queue.StopAsync(default);
        Assert.Equal(BackgroundOperationState.Cancelled, queue.GetStatus("0")!.State);
    }

    [Fact]
    public async Task ServiceResolutionFailureIsVisibleAndWorkerContinues()
    {
        await using var services = new ServiceCollection().BuildServiceProvider(validateScopes: true);
        using var queue = Create(services);
        Assert.True(queue.TryEnqueue<ScopedProbe, int>("missing", 0, static (_, _, _) => Task.CompletedTask));
        Assert.True(queue.TryEnqueue<IServiceProvider, int>("next", 0, static (_, _, _) => Task.CompletedTask));
        await queue.StartAsync(default);
        try
        {
            await WaitFor(queue, "missing", BackgroundOperationState.Failed);
            await WaitFor(queue, "next", BackgroundOperationState.Succeeded);
        }
        finally { await queue.StopAsync(default); }
    }
}
