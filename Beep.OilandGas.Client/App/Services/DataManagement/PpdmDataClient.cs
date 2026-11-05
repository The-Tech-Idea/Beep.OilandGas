using Beep.OilandGas.Client.Exceptions;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Client.App.Services.DataManagement;

/// <summary>The existing PPDM CRUD API contract, using the facade's canonical company-scoped HTTP transport.</summary>
internal sealed class PpdmDataClient(ClientApiTransport transport)
{
    public async Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, GetEntitiesRequest request, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Required(await transport.PostAsync<GetEntitiesRequest, GetEntitiesResponse>(Table(tableName), request, token));
    }
    public async Task<GenericEntityResponse> GetEntityAsync(string tableName, string id, CancellationToken token = default) =>
        Required(await transport.GetAsync<GenericEntityResponse>(Entity(tableName, id), token));
    public async Task<GenericEntityResponse> InsertEntityAsync(string tableName, Dictionary<string, object> request, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Required(await transport.PostAsync<Dictionary<string, object>, GenericEntityResponse>(Table(tableName) + "/insert", request, token));
    }
    public async Task<GenericEntityResponse> UpdateEntityAsync(string tableName, string id, Dictionary<string, object> request, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Required(await transport.PutAsync<Dictionary<string, object>, GenericEntityResponse>(Entity(tableName, id), request, token));
    }
    public async Task<GenericEntityResponse> DeleteEntityAsync(string tableName, string id, CancellationToken token = default) =>
        Required(await transport.DeleteAsync<GenericEntityResponse>(Entity(tableName, id), token));

    private static T Required<T>(T? result) where T : class => result ?? throw new ApiClientException("The PPDM API returned no result.");
    private static string Table(string name)
    { ArgumentException.ThrowIfNullOrWhiteSpace(name); return "/api/ppdm39/data/" + Uri.EscapeDataString(name); }
    private static string Entity(string table, string id)
    { ArgumentException.ThrowIfNullOrWhiteSpace(id); return Table(table) + "/" + Uri.EscapeDataString(id); }
}
