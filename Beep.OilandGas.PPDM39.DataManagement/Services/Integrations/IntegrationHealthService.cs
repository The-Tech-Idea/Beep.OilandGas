using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Integrations;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Integrations;

/// <summary>
/// Thread-safe in-memory circuit-breaker and health monitor for all integration adapters.
/// Registered as Singleton so state persists for the lifetime of the API process.
/// </summary>
public class IntegrationHealthService : IIntegrationHealthService
{
    private static readonly TimeSpan HalfOpenCooldown    = TimeSpan.FromMinutes(5);
    private const           int      DefaultThreshold    = 3;

    private readonly ILogger<IntegrationHealthService> _logger;

    // Per-adapter state
    private sealed class AdapterState
    {
        public CircuitBreakerState State            { get; set; } = CircuitBreakerState.Closed;
        public int  ConsecutiveFailures             { get; set; }
        public DateTime? LastSuccessAt              { get; set; }
        public DateTime? LastFailureAt              { get; set; }
        public DateTime? OpenedAt                   { get; set; }
        public string?   LastErrorMessage           { get; set; }
        public readonly object Lock = new();
    }

    private readonly ConcurrentDictionary<string, AdapterState> _states = new();
    private readonly ConcurrentQueue<IntegrationSyncHistoryEntry> _history = new();
    private const int HistoryCap = 500;

    public IntegrationHealthService(ILogger<IntegrationHealthService> logger)
    {
        _logger = logger;
        foreach (var name in new[] { "WITSML", "PRODML", "OPC-UA", "SharePoint", "SAP", "OSDU" })
            _states[name] = new AdapterState();
    }

    public Task<List<AdapterHealthStatus>> GetAllStatusAsync()
        => Task.FromResult(_states.Select(kv => ToStatus(kv.Key, kv.Value)).ToList());

    public Task<AdapterHealthStatus> GetStatusAsync(string adapterName)
    {
        var state = _states.GetOrAdd(adapterName, _ => new AdapterState());
        return Task.FromResult(ToStatus(adapterName, state));
    }

    public void RecordSuccess(string adapterName)
    {
        var state = _states.GetOrAdd(adapterName, _ => new AdapterState());
        lock (state.Lock)
        {
            state.ConsecutiveFailures = 0;
            state.LastSuccessAt       = DateTime.UtcNow;
            if (state.State != CircuitBreakerState.Closed)
            {
                _logger.LogInformation("[{Adapter}] Circuit breaker closed (success after open/half-open)", adapterName);
                state.State    = CircuitBreakerState.Closed;
                state.OpenedAt = null;
            }
        }
    }

    public void RecordFailure(string adapterName, Exception ex)
    {
        var state = _states.GetOrAdd(adapterName, _ => new AdapterState());
        lock (state.Lock)
        {
            state.ConsecutiveFailures++;
            state.LastFailureAt     = DateTime.UtcNow;
            state.LastErrorMessage  = ex.Message;

            if (state.State == CircuitBreakerState.Closed &&
                state.ConsecutiveFailures >= DefaultThreshold)
            {
                state.State    = CircuitBreakerState.Open;
                state.OpenedAt = DateTime.UtcNow;
                _logger.LogWarning("[{Adapter}] Circuit breaker OPENED after {N} consecutive failures. Last error: {Err}",
                    adapterName, state.ConsecutiveFailures, ex.Message);

                // Emit a structured log entry so external monitors can create alerts
                _logger.LogError("INTEGRATION_CIRCUIT_OPEN adapter={Adapter} failures={N} error={Err}",
                    adapterName, state.ConsecutiveFailures, ex.Message);
            }
            else if (state.State == CircuitBreakerState.HalfOpen)
            {
                // Trial call failed — re-open
                state.State    = CircuitBreakerState.Open;
                state.OpenedAt = DateTime.UtcNow;
                _logger.LogWarning("[{Adapter}] Circuit breaker re-opened (half-open trial failed)", adapterName);
            }
        }
    }

    public void ResetCircuitBreaker(string adapterName)
    {
        var state = _states.GetOrAdd(adapterName, _ => new AdapterState());
        lock (state.Lock)
        {
            state.State               = CircuitBreakerState.Closed;
            state.ConsecutiveFailures = 0;
            state.OpenedAt            = null;
            _logger.LogInformation("[{Adapter}] Circuit breaker manually reset", adapterName);
        }
    }

    public List<IntegrationSyncHistoryEntry> GetRecentHistory(int count = 50)
        => _history.TakeLast(count).Reverse().ToList();

    public void AppendHistory(IntegrationSyncHistoryEntry entry)
    {
        _history.Enqueue(entry);
        // Trim: dequeue oldest items when over cap
        while (_history.Count > HistoryCap)
            _history.TryDequeue(out _);
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private AdapterHealthStatus ToStatus(string name, AdapterState s)
    {
        lock (s.Lock)
        {
            TimeSpan? cooldown = null;
            if (s.State == CircuitBreakerState.Open && s.OpenedAt.HasValue)
            {
                var elapsed   = DateTime.UtcNow - s.OpenedAt.Value;
                var remaining = HalfOpenCooldown - elapsed;
                cooldown = remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;

                // Auto-transition to HalfOpen if cooldown expired
                if (remaining <= TimeSpan.Zero)
                    s.State = CircuitBreakerState.HalfOpen;
            }

            return new AdapterHealthStatus(
                name, s.State, s.ConsecutiveFailures,
                s.LastSuccessAt, s.LastFailureAt,
                s.LastErrorMessage, cooldown);
        }
    }
}
