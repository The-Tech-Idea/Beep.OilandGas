using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;

/// <summary>
/// A gateway that refuses every write, for a preview that must not touch a database. Kept beside the importer so
/// the "preview writes nothing" rule can be exercised without a connection.
/// </summary>
public sealed class OgCsvPlanOnlyGateway : IOgPpdmTableGateway
{
    public Task<OgResult> InsertAsync(string table, OgRow row, CancellationToken cancellationToken = default) =>
        Task.FromResult(OgResult.Failed("A preview does not write.", "preview-only"));

    public Task<OgResult> UpdateAsync(string table, OgRow row, OgRow original, CancellationToken cancellationToken = default) =>
        Task.FromResult(OgResult.Failed("A preview does not write.", "preview-only"));

    public Task<OgResult> DeleteAsync(string table, OgRow row, CancellationToken cancellationToken = default) =>
        Task.FromResult(OgResult.Failed("A preview does not write.", "preview-only"));

    public Task<OgResult<OgPage<OgRow>>> QueryAsync(string table, OgQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult(OgResult<OgPage<OgRow>>.Failed("A preview does not read.", "preview-only"));

    public Task<OgResult<IReadOnlyList<string>>> KeyColumnsAsync(string table, CancellationToken cancellationToken = default) =>
        Task.FromResult(OgResult<IReadOnlyList<string>>.Ok([]));

    public Task<OgResult<byte[]>> ExportAsync(string table, OgQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult(OgResult<byte[]>.Failed("A preview does not export.", "preview-only"));

    public Task<OgResult<OgFormDefinition>> FormAsync(string table, CancellationToken cancellationToken = default) =>
        Task.FromResult(OgResult<OgFormDefinition>.Failed("A preview does not read.", "preview-only"));
}
