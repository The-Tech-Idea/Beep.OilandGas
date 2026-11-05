using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Client.App.Services.DataManagement;

internal partial class DataManagementService
{
    public Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, GetEntitiesRequest request, CancellationToken cancellationToken = default) =>
        DataClient.GetEntitiesAsync(tableName, request, cancellationToken);

    public Task<GenericEntityResponse> GetEntityAsync(string tableName, string id, CancellationToken cancellationToken = default) =>
        DataClient.GetEntityAsync(tableName, id, cancellationToken);

    public Task<GenericEntityResponse> InsertEntityAsync(string tableName, Dictionary<string, object> request, CancellationToken cancellationToken = default) =>
        DataClient.InsertEntityAsync(tableName, request, cancellationToken);

    public Task<GenericEntityResponse> UpdateEntityAsync(string tableName, string id, Dictionary<string, object> request, CancellationToken cancellationToken = default) =>
        DataClient.UpdateEntityAsync(tableName, id, request, cancellationToken);

    public Task<GenericEntityResponse> DeleteEntityAsync(string tableName, string id, CancellationToken cancellationToken = default) =>
        DataClient.DeleteEntityAsync(tableName, id, cancellationToken);
}
