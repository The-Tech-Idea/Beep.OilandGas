using TheTechIdea.Data.OilGas;
using TheTechIdea.OilGas.Core;
using TheTechIdea.OilGas.Loaders;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Returns only a stable tenant-owned binding fingerprint, never connection details or source rows.</summary>
public interface ICompanyEnvironmentBindingReader
{
    Task<string?> ResolveAsync(string module, CancellationToken cancellationToken);
}

public sealed class CompanyEnvironmentSources(CompanyEnvironmentEffectiveResolver effective, ICompanyEnvironmentBindingReader bindings)
{
    /// <summary>Checks profile structure only, not provider columns or the meaning of company codes.</summary>
    public static string[] ReviewMapping(DataSourceDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (definition.Profile != "ppdm39" || definition.Codes is null || definition.Where is null)
            return ["The built-in PPDM profile or mapping configuration is unavailable."];
        var profile = MappingProfile.LoadBuiltIn("ppdm39");
        var issues = new List<string>();
        foreach (var (entity, fields) in definition.Codes)
        {
            if (!profile.Entities.TryGetValue(entity, out var map) || fields is null)
            { issues.Add($"Code mapping entity '{entity}' is not available in the profile."); continue; }
            foreach (var (field, values) in fields)
                if (!map.Fields.ContainsKey(field) || values is null || values.Any(v => InvalidCode(v.Key) || InvalidCode(v.Value)))
                    issues.Add($"Code mapping field '{entity}.{field}' is invalid for the profile.");
        }
        foreach (var (entity, fields) in definition.Where)
        {
            if (!profile.Entities.ContainsKey(entity) || fields is null)
            { issues.Add($"Filter entity '{entity}' is not available in the profile."); continue; }
            if (fields.Any(f => string.IsNullOrWhiteSpace(f.Key) || f.Key.Length > 128 || f.Key.Any(c => !(char.IsAsciiLetterOrDigit(c) || c == '_')) || string.IsNullOrWhiteSpace(f.Value) || f.Value.Length > 512 || f.Value.Any(char.IsControl)))
                issues.Add($"A filter for '{entity}' is invalid.");
        }
        return issues.Take(64).ToArray();
    }

    private static bool InvalidCode(string? value) => string.IsNullOrWhiteSpace(value) || value.Length > 512 || value.Any(char.IsControl);

    public static string[] ReviewEnvironment(OilGasEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        if (environment.DataSources is null || environment.DataSources.Count > 32)
            return ["The source configuration is invalid or too large."];
        var issues = new List<string>();
        foreach (var (name, definition) in environment.DataSources)
        {
            if (definition is null) { issues.Add($"Source '{name}' is unavailable."); continue; }
            issues.AddRange(ReviewMapping(definition).Select(issue => $"Source '{name}': {issue}"));
        }
        return issues.Take(64).ToArray();
    }

    public async Task<CompanyEnvironmentSourceReview> ReviewAsync(CompanyScope company, Func<Task> authority, CancellationToken cancellationToken)
    {
        var resolved = await effective.ResolveRuntimeAsync(company, authority);
        if (resolved.Review.ReviewStatus != "Current")
            throw new InvalidOperationException("Shared or country settings changed. Review and save the company environment before reviewing sources.");
        var environment = resolved.Environment;
        if (environment.DataSources is null || environment.DataSources.Count > 32)
            throw new InvalidOperationException("The source configuration is invalid.");
        var output = new List<CompanyEnvironmentSource>();
        var capturedBindings = new List<(string Module, string? Fingerprint)>();
        foreach (var (name, definition) in environment.DataSources.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested(); await authority();
            if (definition is null || definition.Kind != "beep" || definition.Codes is null || definition.Where is null ||
                definition.Codes.Values.Any(fields => fields is null || fields.Values.Any(values => values is null)) ||
                definition.Where.Values.Any(fields => fields is null))
                throw new InvalidOperationException("The source configuration is invalid.");
            var first = await bindings.ResolveAsync(definition.Path, cancellationToken);
            if (first is not null && !CompanyEnvironmentIdentity.IsRevision(first))
                throw new InvalidOperationException("The source binding evidence is invalid.");
            capturedBindings.Add((definition.Path, first));
            var codes = definition.Codes.Values.Sum(fields => fields.Values.Sum(values => values.Count));
            var filters = definition.Where.Values.Sum(fields => fields.Count);
            var issues = ReviewMapping(definition);
            var mappings = definition.Codes.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .SelectMany(entity => entity.Value.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                    .SelectMany(field => field.Value.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                        .Select(value => Mapping(name, entity.Key, field.Key, value.Key, value.Value, resolved.MappingOrigins))))
                .ToArray();
            output.Add(new CompanyEnvironmentSource(name, definition.Path, definition.Profile, codes, filters, first is null ? "Unavailable" : "Bound")
                { MappingStatus = issues.Length == 0 ? "ProfileChecked" : "ReviewRequired", MappingIssues = issues, CodeMappings = mappings });
        }
        await authority();
        var closing = await effective.ResolveAsync(company, authority);
        if (closing.ReviewStatus != "Current" || closing.EffectiveRevision != resolved.Review.EffectiveRevision)
            throw new InvalidOperationException("The effective company environment changed during source review.");
        foreach (var (module, fingerprint) in capturedBindings)
        {
            cancellationToken.ThrowIfCancellationRequested(); await authority();
            if (await bindings.ResolveAsync(module, cancellationToken) != fingerprint)
                throw new InvalidOperationException("A source binding changed during review.");
        }
        await authority();
        return new(company, resolved.Review.CompanyRevision, output.ToArray(), false, resolved.Review.EffectiveRevision);
    }

    private static CompanyEnvironmentCodeMapping Mapping(string source, string entity, string field, string code, string value,
        IReadOnlyDictionary<string, SetupMappingOrigin> origins)
    {
        if (!origins.TryGetValue(CompanyEnvironmentEffectiveResolver.MappingKey(source, entity, field, code), out var origin) ||
            !string.Equals(origin.Value, value, StringComparison.Ordinal))
            throw new InvalidOperationException("The effective code mapping origin could not be confirmed.");
        return new(entity, field, code, value, origin.Layer);
    }
}
