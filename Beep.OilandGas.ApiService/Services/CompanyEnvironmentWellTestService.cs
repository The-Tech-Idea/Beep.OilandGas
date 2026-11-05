using System.Globalization;
using TheTechIdea.Data.OilGas;
using TheTechIdea.OilGas.Core;
using TheTechIdea.OilGas.Loaders;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Reads one explicitly requested WELL through the existing company/asset row policy.</summary>
public interface ICompanyEnvironmentWellReader
{
    Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ReadAsync(CompanyScope company, string wellId,
        IReadOnlyDictionary<string, string> filters, Func<Task> authority, CancellationToken token);
}

public sealed class CompanyEnvironmentWellTestService(CompanyEnvironmentEffectiveResolver effective,
    ICompanyEnvironmentBindingReader bindings, ICompanyEnvironmentWellReader reader)
{
    public async Task<CompanyEnvironmentWellTest> TestAsync(CompanyScope company, CompanyEnvironmentWellTestRequest request,
        Func<Task> authority, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request); ArgumentNullException.ThrowIfNull(authority);
        if (string.IsNullOrWhiteSpace(request.SourceName) || request.SourceName.Length > 64 ||
            string.IsNullOrWhiteSpace(request.WellId) || request.WellId.Length > 128 || request.WellId.Any(char.IsControl) ||
            !CompanyEnvironmentIdentity.IsRevision(request.ExpectedRevision) || !CompanyEnvironmentIdentity.IsRevision(request.ExpectedEffectiveRevision))
            throw new ArgumentException("Select a saved source and an explicit well identifier.");
        var resolved = await effective.ResolveRuntimeAsync(company, authority);
        if (resolved.Review.ReviewStatus != "Current" || resolved.Review.CompanyRevision != request.ExpectedRevision || resolved.Review.EffectiveRevision != request.ExpectedEffectiveRevision)
            throw new InvalidOperationException("The saved or effective environment changed.");
        var environment = resolved.Environment;
        if (environment.DataSources is null || !environment.DataSources.TryGetValue(request.SourceName, out var definition) ||
            definition is null || definition.Kind != "beep" || definition.Path != "PPDM_CORE" || definition.Profile != "ppdm39" ||
            definition.Codes is null || definition.Where is null)
            throw new InvalidOperationException("The selected source is not a PPDM core Beep source.");
        if (CompanyEnvironmentSources.ReviewMapping(definition).Length != 0)
            throw new InvalidOperationException("Review the source mappings before testing the well.");
        var originalBinding = await bindings.ResolveAsync(definition.Path, token);
        if (!CompanyEnvironmentIdentity.IsRevision(originalBinding)) throw new InvalidOperationException("The source binding is unavailable.");

        var profile = MappingProfile.LoadBuiltIn("ppdm39");
        foreach (var (entity, fields) in definition.Codes)
        {
            if (fields is null || !profile.Entities.TryGetValue(entity, out var map)) throw new InvalidOperationException("The source code mappings are invalid.");
            foreach (var (field, codes) in fields)
            {
                if (codes is null) throw new InvalidOperationException("The source code mappings are invalid.");
                if (!map.Codes.TryGetValue(field, out var target)) map.Codes[field] = target = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var (code, value) in codes) target[code] = value;
            }
        }
        foreach (var (entity, filters) in definition.Where)
        {
            if (filters is null || !profile.Entities.TryGetValue(entity, out var map)) throw new InvalidOperationException("The source filters are invalid.");
            foreach (var (column, value) in filters) map.Where[column] = value;
        }

        var source = new SingleWellSource(reader, company, request.WellId, authority);
        using var scope = OilGasServices.Scope(new OilGasServices(environment));
        var provider = new MappedWellDataProvider(source, profile);
        var wells = await provider.FindWellsAsync(cancellationToken: token);
        if (wells.Count > 1 || wells.Any(x => x.Id != request.WellId || x.Uwi != request.WellId))
            throw new InvalidOperationException("The selected well source returned an ambiguous or foreign record.");
        var closingWells = await provider.FindWellsAsync(cancellationToken: token);
        if (closingWells.Count != wells.Count || closingWells.Zip(wells).Any(pair => pair.First.Id != pair.Second.Id ||
            pair.First.Uwi != pair.Second.Uwi || pair.First.Name != pair.Second.Name || pair.First.Field != pair.Second.Field))
            throw new InvalidOperationException("The selected well changed during the source check.");
        await authority();
        var closing = await effective.ResolveAsync(company, authority);
        if (await bindings.ResolveAsync(definition.Path, token) != originalBinding || closing.ReviewStatus != "Current" ||
            closing.EffectiveRevision != resolved.Review.EffectiveRevision)
            throw new InvalidOperationException("The company environment or source binding changed during the test.");
        await authority();
        var well = wells.SingleOrDefault();
        return new(company, resolved.Review.CompanyRevision, request.SourceName, request.WellId, well is null ? "NotVisible" : "Found",
            well?.Name, well?.Field, false, typeof(MappingProfile).Assembly.ManifestModule.ModuleVersionId.ToString("N"), resolved.Review.EffectiveRevision);
    }

    private sealed class SingleWellSource(ICompanyEnvironmentWellReader reader, CompanyScope company, string wellId, Func<Task> authority) : ITabularSource
    {
        public async Task<IReadOnlyList<DataRow>> QueryAsync(string table, IReadOnlyDictionary<string, object?> equals, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(table, "WELL", StringComparison.Ordinal) || equals.Count > 32)
                throw new InvalidOperationException("Only one reviewed WELL header can be tested.");
            var filters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, value) in equals)
            {
                var text = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(key) || key.Length > 128 || key.Any(c => !(char.IsAsciiLetterOrDigit(c) || c == '_')) ||
                    string.IsNullOrEmpty(text) || text.Length > 512 || text.Any(char.IsControl) || !filters.TryAdd(key, text))
                    throw new InvalidOperationException("The WELL profile filter is invalid.");
            }
            if (filters.TryGetValue("UWI", out var requested) && requested != wellId)
                throw new InvalidOperationException("The source filter conflicts with the selected well.");
            filters["UWI"] = wellId;
            var rows = await reader.ReadAsync(company, wellId, filters, authority, cancellationToken);
            if (rows is null || rows.Count > 1) throw new InvalidOperationException("The WELL source is unavailable or ambiguous.");
            return rows.Select(row =>
            {
                if (!row.TryGetValue("UWI", out var value) || Convert.ToString(value, CultureInfo.InvariantCulture) != wellId)
                    throw new InvalidOperationException("The source returned a different well.");
                return new DataRow(row.ToDictionary(x => x.Key, x => x.Value is DBNull ? null : x.Value, StringComparer.OrdinalIgnoreCase));
            }).ToArray();
        }
    }
}
