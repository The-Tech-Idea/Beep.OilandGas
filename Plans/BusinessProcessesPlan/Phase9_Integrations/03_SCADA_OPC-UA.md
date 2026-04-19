# Phase 9 — SCADA / OPC-UA Adapter
## OPC-UA Subscription → INSTRUMENT and PROD_STRING Real-Time Data

---

## IScadaOpcUaAdapter Interface

```csharp
public interface IScadaOpcUaAdapter : IAsyncDisposable
{
    Task ConnectAsync(string endpointUrl, string? username = null, string? password = null);

    Task SubscribeTagsAsync(IEnumerable<string> nodeIds,
        int samplingIntervalMs, Func<OpcTagValue, Task> onValueChanged);

    Task<OpcTagValue> ReadTagOnceAsync(string nodeId);

    void Disconnect();
}

public record OpcTagValue(
    string NodeId, string TagName, object? Value,
    string DataType, DateTime SourceTimestamp, bool IsGood);
```

---

## OPC-UA → PPDM INSTRUMENT Mapping

Each OPC-UA tag subscription writes real-time readings into the `INSTRUMENT` table (or `INSTRUMENT_READING` extension):

| OPC-UA Field | PPDM Column | Notes |
|---|---|---|
| `nodeId` | `INSTRUMENT.INSTRUMENT_ID` | Map via configuration dictionary |
| `TagName` | `INSTRUMENT.INSTRUMENT_NAME` | |
| `Value` (numeric) | `INSTRUMENT.CURRENT_READING` | Cast to DECIMAL |
| `DataType` | `INSTRUMENT.READING_UOM` | Derive UOM from OPC data type hint |
| `SourceTimestamp` | `INSTRUMENT.READING_DATE` | UTC stored |
| `IsGood` | `INSTRUMENT.QUALITY_CODE` | `GOOD` / `BAD` / `UNCERTAIN` |

---

## PROD_STRING Real-Time Telemetry

For wellhead and production string gauges, readings also update `PROD_STRING`:

| OPC-UA Tag Category | PPDM Column | Example Tag |
|---|---|---|
| Wellhead pressure | `PROD_STRING.WH_PRESSURE` | `ns=2;s=WELL01.WHPRESSURE` |
| Flowing temperature | `PROD_STRING.FLOWING_TEMP` | `ns=2;s=WELL01.FTEMP` |
| Choke position | `PROD_STRING.CHOKE_SIZE` | `ns=2;s=WELL01.CHOKE_PCT` |
| Gas rate (MSCFD) | `PROD_STRING.GAS_RATE_DAILY` | `ns=2;s=WELL01.GASRATE` |

---

## Tag-to-PPDM Configuration (appsettings.json)

```json
"Integrations": {
    "SCADA": {
        "EndpointUrl": "opc.tcp://scada-server.company.com:4840",
        "Username": "{{env:SCADA_USER}}",
        "Password": "{{env:SCADA_PASS}}",
        "SamplingIntervalMs": 5000,
        "TagMappings": [
            {
                "nodeId": "ns=2;s=WELL01.WHPRESSURE",
                "tagName": "WH_PRESSURE_WELL01",
                "instrument_id": "INS-WELL01-WHP",
                "prod_string_column": "WH_PRESSURE"
            }
        ]
    }
}
```

---

## Buffered Write Strategy

Real-time OPC-UA callbacks fire every `SamplingIntervalMs`. Writing every reading to the DB would be too frequent. Instead:

1. Buffer readings in `ConcurrentQueue<OpcTagValue>`
2. Background `IHostedService` flushes buffer every 60 seconds
3. For each tag, take **last value in window** (not all samples) → single upsert per `INSTRUMENT` per minute

```csharp
// OpcUaFlushService (IHostedService)
protected override async Task ExecuteAsync(CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        await Task.Delay(60_000, ct);
        var snapshot = _buffer.Drain();      // Take latest per NodeId
        foreach (var tag in snapshot)
        {
            await _adapter.UpsertInstrumentReadingAsync(tag, _userId);
        }
    }
}
```

---

## Error Handling

| Scenario | Behavior |
|---|---|
| OPC-UA server unreachable at startup | Log warning; circuit breaker opens; NOTIFICATION created |
| Bad quality reading (`IsGood=false`) | Write with `QUALITY_CODE='BAD'`; do NOT update `PROD_STRING` |
| DB write failure | Log error; retry once; if still fails, add to dead-letter queue JSON file |
