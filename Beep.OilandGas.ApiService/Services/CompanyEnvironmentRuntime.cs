using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using TheTechIdea.OilGas.Core;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Runs OilandGasTools against one confirmed company revision without changing process-wide defaults.</summary>
public sealed class CompanyEnvironmentRuntime(CompanyEnvironmentEffectiveResolver effective)
{
    public async Task<CompanyEnvironmentVerification> VerifyAsync(CompanyScope company, Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(authority);
        var resolved = await effective.ResolveRuntimeAsync(company, authority);
        if (resolved.Review.ReviewStatus != "Current")
            throw new InvalidOperationException("Shared or country settings changed. Review and save the company environment before verification.");
        var environment = resolved.Environment;
        if (environment.Verification?.Cases is not { } cases || cases.Count > 64)
            throw new InvalidOperationException("The saved verification configuration is invalid or too large.");

        // Use the constructor rather than FromEnvironment: the latter reads resource files from the server filesystem.
        OilGasServices services;
        try { services = new OilGasServices(environment); }
        catch (Exception invalid) when (invalid is ArgumentException or InvalidOperationException or NullReferenceException)
        { throw new InvalidOperationException("The saved environment has invalid required settings.", invalid); }
        using var scope = OilGasServices.Scope(services);
        List<string> issues;
        try { issues = environment.Validate(); }
        catch (Exception invalid) when (invalid is ArgumentException or InvalidOperationException or NullReferenceException)
        { throw new InvalidOperationException("The saved environment has invalid required settings.", invalid); }
        var results = new List<CompanyEnvironmentReferenceResult>();
        foreach (var test in cases)
        {
            if (test is null || string.IsNullOrWhiteSpace(test.Name) || test.Name.Length > 256 ||
                test.Kind is null || test.Kind.Length > 64 || !double.IsFinite(test.Tolerance) || test.Tolerance < 0 ||
                test.Survey is null || test.Survey.Count > 2048 || test.Survey.Any(s => s is null || s.Length < 3 || s.Any(v => !double.IsFinite(v))))
                throw new InvalidOperationException("A saved reference case is invalid or too large.");
            if (test.Kind is not ("unit" or "trajectory" or "north" or "uncertainty" or "dipCorrection"))
            { results.Add(new(test.Name, test.Kind, "Unavailable", "This case requires a source or tool extension that was not checked.")); continue; }
            try
            {
                var result = ReferenceCases.Run([test]).Single();
                results.Add(new(test.Name, test.Kind, result.Passed && result.Values.Count > 0 ? "Passed" : "Failed", result.Error));
            }
            catch (Exception invalid) when (invalid is ArgumentException or InvalidOperationException or IndexOutOfRangeException or NullReferenceException)
            { results.Add(new(test.Name, test.Kind, "Failed", "The reference case could not be evaluated.")); }
        }
        await authority();
        var closing = await effective.ResolveAsync(company, authority);
        await authority();
        if (closing.ReviewStatus != "Current" || closing.EffectiveRevision != resolved.Review.EffectiveRevision)
            throw new InvalidOperationException("The effective environment changed during verification.");
        return new(company, resolved.Review.CompanyRevision, CompanyEnvironmentPreviewer.ToolsVersion, issues.ToArray(), results.ToArray(), false, resolved.Review.EffectiveRevision);
    }
}
