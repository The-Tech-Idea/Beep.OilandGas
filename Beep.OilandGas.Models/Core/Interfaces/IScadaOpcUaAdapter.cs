using Beep.OilandGas.Models.Data.Integrations;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IScadaOpcUaAdapter : IAsyncDisposable
{
    Task ConnectAsync(string endpointUrl, string? username = null, string? password = null);

    Task SubscribeTagsAsync(IEnumerable<string> nodeIds,
        int samplingIntervalMs, Func<OpcTagValue, Task> onValueChanged);

    Task<OpcTagValue> ReadTagOnceAsync(string nodeId);

    void Disconnect();

    Task UpsertInstrumentReadingAsync(OpcTagValue tag, string userId);
}
