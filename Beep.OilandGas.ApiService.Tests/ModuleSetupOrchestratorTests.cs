using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// Unit tests for <see cref="ModuleSetupOrchestrator"/>.
///
/// All tests use in-memory stub modules — no database or DI container required.
/// Evidence checklist (Phase 5 verification):
///   [x] Modules run in Order ASC then ModuleId ASC
///   [x] Entity types deduplicated across modules
///   [x] SeedAsync isolates per-module exceptions (continues to next module)
///   [x] ModuleSetupAbortException halts the entire run
///   [x] CancellationToken is respected between modules
///   [x] Aggregate counters (ModulesRun / ModulesSucceeded / TotalRecordsInserted) are correct
///   [x] AllSucceeded=true only when every module reports Success
///   [x] Idempotency: two successive runs produce identical aggregate shapes
///   [x] RunSeedForModulesAsync filters to the requested module IDs
///   [x] RunSeedForModulesAsync on an empty list falls back to RunSeedAsync
///   [x] GetAllEntityTypes returns empty list when no modules registered
/// </summary>
public class ModuleSetupOrchestratorTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static ModuleSetupOrchestrator Build(params IModuleSetup[] modules)
        => new(modules, NullLogger<ModuleSetupOrchestrator>.Instance);

    /// <summary>Stub module that always succeeds and inserts <paramref name="records"/> records.</summary>
    private sealed class OkModule : IModuleSetup
    {
        public string ModuleId   { get; }
        public string ModuleName { get; }
        public int    Order      { get; }
        public IReadOnlyList<Type> EntityTypes { get; }

        private readonly int _records;

        public OkModule(string id, int order, int records = 1, params Type[] types)
        {
            ModuleId   = id;
            ModuleName = id;
            Order      = order;
            _records   = records;
            EntityTypes = types.Length > 0 ? types : Array.Empty<Type>();
        }

        public Task<ModuleSetupResult> SeedAsync(string connectionName, string userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new ModuleSetupResult
            {
                ModuleId         = ModuleId,
                ModuleName       = ModuleName,
                Success          = true,
                RecordsInserted  = _records,
                TablesSeeded     = 1
            });
    }

    /// <summary>Stub module that always throws a plain exception (non-abort).</summary>
    private sealed class FaultModule : IModuleSetup
    {
        public string ModuleId   { get; }
        public string ModuleName { get; }
        public int    Order      { get; }
        public IReadOnlyList<Type> EntityTypes => Array.Empty<Type>();

        public FaultModule(string id, int order) { ModuleId = id; ModuleName = id; Order = order; }

        public Task<ModuleSetupResult> SeedAsync(string connectionName, string userId,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException($"Simulated failure in {ModuleId}");
    }

    /// <summary>Stub module that throws <see cref="ModuleSetupAbortException"/>.</summary>
    private sealed class AbortModule : IModuleSetup
    {
        public string ModuleId   { get; }
        public string ModuleName { get; }
        public int    Order      { get; }
        public IReadOnlyList<Type> EntityTypes => Array.Empty<Type>();

        public AbortModule(string id, int order) { ModuleId = id; ModuleName = id; Order = order; }

        public Task<ModuleSetupResult> SeedAsync(string connectionName, string userId,
            CancellationToken cancellationToken = default)
            => throw new ModuleSetupAbortException(ModuleId, "Unrecoverable DB connection lost");
    }

    /// <summary>Stub module that records call order into a shared list.</summary>
    private sealed class OrderRecorder : IModuleSetup
    {
        public string ModuleId   { get; }
        public string ModuleName { get; }
        public int    Order      { get; }
        public IReadOnlyList<Type> EntityTypes => Array.Empty<Type>();

        private readonly List<string> _log;

        public OrderRecorder(string id, int order, List<string> log)
        {
            ModuleId   = id;
            ModuleName = id;
            Order      = order;
            _log       = log;
        }

        public Task<ModuleSetupResult> SeedAsync(string connectionName, string userId,
            CancellationToken cancellationToken = default)
        {
            _log.Add(ModuleId);
            return Task.FromResult(new ModuleSetupResult { ModuleId = ModuleId, ModuleName = ModuleName, Success = true });
        }
    }

    // Sentinel types for entity-type deduplication tests
    private class TypeA { }
    private class TypeB { }
    private class TypeC { }

    // ──────────────────────────────────────────────────────────────────────────
    // GetAllEntityTypes
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAllEntityTypes_NoModules_ReturnsEmpty()
    {
        var sut = Build();
        Assert.Empty(sut.GetAllEntityTypes());
    }

    [Fact]
    public void GetAllEntityTypes_DeduplicatesAcrossModules()
    {
        // ModuleA owns TypeA and TypeB; ModuleB also declares TypeB (duplicate) plus TypeC.
        var sut = Build(
            new OkModule("A", 10, 0, typeof(TypeA), typeof(TypeB)),
            new OkModule("B", 20, 0, typeof(TypeB), typeof(TypeC)));

        var types = sut.GetAllEntityTypes();

        Assert.Equal(3, types.Count);
        Assert.Contains(typeof(TypeA), types);
        Assert.Contains(typeof(TypeB), types);
        Assert.Contains(typeof(TypeC), types);
        // TypeB must appear exactly once
        Assert.Single(types, t => t == typeof(TypeB));
    }

    [Fact]
    public void GetAllEntityTypes_PreservesLowerOrderFirst()
    {
        var sut = Build(
            new OkModule("Z", 50, 0, typeof(TypeC)),   // higher order → last
            new OkModule("A", 10, 0, typeof(TypeA)));  // lower order  → first

        var types = sut.GetAllEntityTypes();

        Assert.Equal(typeof(TypeA), types[0]);
        Assert.Equal(typeof(TypeC), types[1]);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Execution order
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunSeedAsync_ExecutesModulesInOrderAsc()
    {
        var callLog = new List<string>();

        var sut = Build(
            new OrderRecorder("C", 30, callLog),
            new OrderRecorder("A", 10, callLog),
            new OrderRecorder("B", 20, callLog));

        await sut.RunSeedAsync("TEST", "user");

        Assert.Equal(new[] { "A", "B", "C" }, callLog);
    }

    [Fact]
    public async Task RunSeedAsync_TieBreaksOrderByModuleId()
    {
        var callLog = new List<string>();

        var sut = Build(
            new OrderRecorder("Z", 10, callLog),
            new OrderRecorder("A", 10, callLog),
            new OrderRecorder("M", 10, callLog));

        await sut.RunSeedAsync("TEST", "user");

        Assert.Equal(new[] { "A", "M", "Z" }, callLog);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Success / failure isolation
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunSeedAsync_AllSucceed_AllSucceededIsTrue()
    {
        var sut = Build(
            new OkModule("A", 10, records: 5),
            new OkModule("B", 20, records: 3));

        var result = await sut.RunSeedAsync("TEST", "user");

        Assert.True(result.AllSucceeded);
        Assert.Equal(2, result.ModulesRun);
        Assert.Equal(2, result.ModulesSucceeded);
        Assert.Equal(8, result.TotalRecordsInserted);
    }

    [Fact]
    public async Task RunSeedAsync_FaultModule_ContinuesToNextModule()
    {
        var callLog = new List<string>();

        // Fault in the middle — B should still run
        var sut = Build(
            new OrderRecorder("A", 10, callLog),
            new FaultModule("FAULT", 20),
            new OrderRecorder("B", 30, callLog));

        var result = await sut.RunSeedAsync("TEST", "user");

        // A and B executed
        Assert.Contains("A", callLog);
        Assert.Contains("B", callLog);

        // 3 modules attempted; 2 succeeded; FAULT failed
        Assert.Equal(3, result.ModulesRun);
        Assert.Equal(2, result.ModulesSucceeded);
        Assert.False(result.AllSucceeded);

        var faultResult = result.ModuleResults.Single(r => r.ModuleId == "FAULT");
        Assert.False(faultResult.Success);
        Assert.NotEmpty(faultResult.Errors);
    }

    [Fact]
    public async Task RunSeedAsync_AllSucceeded_FalseWhenAnyModuleFails()
    {
        var sut = Build(
            new OkModule("A", 10),
            new FaultModule("FAULT", 20));

        var result = await sut.RunSeedAsync("TEST", "user");

        Assert.False(result.AllSucceeded);
    }

    [Fact]
    public async Task RunSeedAsync_AbortModule_ThrowsAndStopsRun()
    {
        var callLog = new List<string>();

        var sut = Build(
            new OrderRecorder("A", 10, callLog),
            new AbortModule("ABORT", 20),
            new OrderRecorder("B", 30, callLog));  // must NOT run

        await Assert.ThrowsAsync<ModuleSetupAbortException>(
            () => sut.RunSeedAsync("TEST", "user"));

        Assert.Contains("A", callLog);
        Assert.DoesNotContain("B", callLog);  // halted before B
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Cancellation
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunSeedAsync_CancelledToken_ThrowsOperationCancelledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();  // pre-cancelled

        var sut = Build(new OkModule("A", 10));

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => sut.RunSeedAsync("TEST", "user", cts.Token));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Idempotency (second run produces same shape)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunSeedAsync_TwiceInARow_ProducesSameAggregateShape()
    {
        var sut = Build(
            new OkModule("A", 10, records: 2),
            new OkModule("B", 20, records: 3));

        var first  = await sut.RunSeedAsync("TEST", "user");
        var second = await sut.RunSeedAsync("TEST", "user");

        Assert.Equal(first.ModulesRun,          second.ModulesRun);
        Assert.Equal(first.ModulesSucceeded,     second.ModulesSucceeded);
        Assert.Equal(first.TotalRecordsInserted, second.TotalRecordsInserted);
        Assert.Equal(first.AllSucceeded,         second.AllSucceeded);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // RunSeedForModulesAsync
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunSeedForModulesAsync_FiltersToRequestedIds()
    {
        var callLog = new List<string>();

        var sut = Build(
            new OrderRecorder("A", 10, callLog),
            new OrderRecorder("B", 20, callLog),
            new OrderRecorder("C", 30, callLog));

        var result = await sut.RunSeedForModulesAsync(
            new[] { "A", "C" }, "TEST", "user");

        Assert.Contains("A", callLog);
        Assert.DoesNotContain("B", callLog);
        Assert.Contains("C", callLog);

        Assert.Equal(2, result.ModulesRun);
    }

    [Fact]
    public async Task RunSeedForModulesAsync_EmptyList_FallsBackToRunAll()
    {
        var callLog = new List<string>();

        var sut = Build(
            new OrderRecorder("A", 10, callLog),
            new OrderRecorder("B", 20, callLog));

        var result = await sut.RunSeedForModulesAsync(
            Array.Empty<string>(), "TEST", "user");

        Assert.Contains("A", callLog);
        Assert.Contains("B", callLog);
        Assert.Equal(2, result.ModulesRun);
    }

    [Fact]
    public async Task RunSeedForModulesAsync_NoMatchingIds_ReturnsEmptyResult()
    {
        var sut = Build(new OkModule("A", 10));

        var result = await sut.RunSeedForModulesAsync(
            new[] { "DOES_NOT_EXIST" }, "TEST", "user");

        Assert.Equal(0, result.ModulesRun);
        Assert.Empty(result.ModuleResults);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Aggregate counter correctness
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunSeedAsync_TotalRecordsInserted_IsSumOfAllModules()
    {
        var sut = Build(
            new OkModule("A", 10, records: 10),
            new OkModule("B", 20, records: 25),
            new OkModule("C", 30, records: 7));

        var result = await sut.RunSeedAsync("TEST", "user");

        Assert.Equal(42, result.TotalRecordsInserted);
    }

    [Fact]
    public async Task RunSeedAsync_FaultedModuleContributesZeroRecords()
    {
        var sut = Build(
            new OkModule("A", 10, records: 10),
            new FaultModule("FAULT", 20));

        var result = await sut.RunSeedAsync("TEST", "user");

        Assert.Equal(10, result.TotalRecordsInserted);
    }
}
