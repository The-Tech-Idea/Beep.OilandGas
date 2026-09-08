using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Modules;

/// <summary>
/// Module order 51 — seeds default accounting BA reference codes into PPDM standard R_* tables.
/// Follows the same ModuleSetupBase pattern as ProductionAccountingModuleSetup (order 70).
///
/// Seeds the standard reference values from <see cref="AccountingReferenceCodes"/> into R_* tables.
/// Code always uses typed constants (e.g., AccountingReferenceCodes.VendorLOVCodes.CategoryVendor) —
/// never magic strings. Users may extend R_* tables with additional values via the setup UI.
///
/// Tables seeded: R_BA_CATEGORY, R_BA_TYPE, R_BA_PREF_TYPE, R_BA_STATUS
/// Seed defaults from: <see cref="AccountingReferenceCodes"/>
/// </summary>
public sealed class AccountingModuleSetup : ModuleSetupBase
{
    private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
    {
        // PPDM standard BA reference tables — seeded with accounting-specific codes
        typeof(R_BA_CATEGORY),
        typeof(R_BA_TYPE),
        typeof(R_BA_PREF_TYPE),
        typeof(R_BA_STATUS),
    };

    public AccountingModuleSetup(ModuleSetupContext context) : base(context) { }

    public override string ModuleId => "ACCOUNTING";
    public override string ModuleName => "Accounting BA Reference Codes";
    /// <summary>After LIFECYCLE (50), before domain calculation modules (70+).</summary>
    public override int Order => 51;
    public override IReadOnlyList<Type> EntityTypes => _entityTypes;

    public override async Task<ModuleSetupResult> SeedAsync(
        string connectionName,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var result = NewResult();
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await SeedBACategoriesAsync(connectionName, userId, result, cancellationToken);
            await SeedBATypesAsync(connectionName, userId, result, cancellationToken);
            await SeedBAPreferenceTypesAsync(connectionName, userId, result, cancellationToken);
            await SeedBAStatusCodesAsync(connectionName, userId, result, cancellationToken);

            result.Success = result.Errors.Count == 0;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            result.Success = false;
            result.Errors.Add($"Accounting reference code seeding failed: {ex.Message}");
        }

        return result;
    }

    private async Task SeedBACategoriesAsync(string connectionName, string userId, ModuleSetupResult result, CancellationToken ct)
    {
        var repo = GetRepo<R_BA_CATEGORY>("R_BA_CATEGORY", connectionName);
        var existing = (await repo.GetAsync(new List<AppFilter>()))
            .OfType<R_BA_CATEGORY>().Select(r => r.BA_CATEGORY).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var code in AccountingReferenceCodes.VendorLOVCodes.All)
        {
            ct.ThrowIfCancellationRequested();
            if (existing.Contains(code)) continue;

            await TryInsertAsync(repo, new R_BA_CATEGORY
            {
                BA_CATEGORY = code, ABBREVIATION = code[..Math.Min(code.Length, 10)],
                SHORT_NAME = code.Replace("_", " "), LONG_NAME = $"{code.Replace("_", " ")} (seeded)",
                ACTIVE_IND = "Y", EFFECTIVE_DATE = DateTime.UtcNow,
                PPDM_GUID = Guid.NewGuid().ToString(), SOURCE = "ACCOUNTING_MODULE"
            }, userId, result, $"R_BA_CATEGORY/{code}");
        }
    }

    private async Task SeedBATypesAsync(string connectionName, string userId, ModuleSetupResult result, CancellationToken ct)
    {
        var repo = GetRepo<R_BA_TYPE>("R_BA_TYPE", connectionName);
        var existing = (await repo.GetAsync(new List<AppFilter>()))
            .OfType<R_BA_TYPE>().Select(r => r.BA_TYPE).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var code in AccountingReferenceCodes.VendorTypeCodes.All)
        {
            ct.ThrowIfCancellationRequested();
            if (existing.Contains(code)) continue;

            await TryInsertAsync(repo, new R_BA_TYPE
            {
                BA_TYPE = code, ABBREVIATION = code[..Math.Min(code.Length, 10)],
                SHORT_NAME = code.Replace("_", " "), LONG_NAME = $"{code.Replace("_", " ")} (seeded)",
                ACTIVE_IND = "Y", EFFECTIVE_DATE = DateTime.UtcNow,
                PPDM_GUID = Guid.NewGuid().ToString(), SOURCE = "ACCOUNTING_MODULE"
            }, userId, result, $"R_BA_TYPE/{code}");
        }
    }

    private async Task SeedBAPreferenceTypesAsync(string connectionName, string userId, ModuleSetupResult result, CancellationToken ct)
    {
        var repo = GetRepo<R_BA_PREF_TYPE>("R_BA_PREF_TYPE", connectionName);
        var existing = (await repo.GetAsync(new List<AppFilter>()))
            .OfType<R_BA_PREF_TYPE>().Select(r => r.PREFERENCE_TYPE).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var code in AccountingReferenceCodes.PreferenceTypeCodes.All)
        {
            ct.ThrowIfCancellationRequested();
            if (existing.Contains(code)) continue;

            await TryInsertAsync(repo, new R_BA_PREF_TYPE
            {
                PREFERENCE_TYPE = code, ABBREVIATION = code[..Math.Min(code.Length, 10)],
                SHORT_NAME = code.Replace("_", " "), LONG_NAME = $"{code.Replace("_", " ")} (seeded)",
                ACTIVE_IND = "Y", EFFECTIVE_DATE = DateTime.UtcNow,
                PPDM_GUID = Guid.NewGuid().ToString(), SOURCE = "ACCOUNTING_MODULE"
            }, userId, result, $"R_BA_PREF_TYPE/{code}");
        }
    }

    private async Task SeedBAStatusCodesAsync(string connectionName, string userId, ModuleSetupResult result, CancellationToken ct)
    {
        var repo = GetRepo<R_BA_STATUS>("R_BA_STATUS", connectionName);
        var existing = (await repo.GetAsync(new List<AppFilter>()))
            .OfType<R_BA_STATUS>().Select(r => r.BA_STATUS).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var code in AccountingReferenceCodes.BAStatusCodes.All)
        {
            ct.ThrowIfCancellationRequested();
            if (existing.Contains(code)) continue;

            await TryInsertAsync(repo, new R_BA_STATUS
            {
                BA_STATUS = code, ABBREVIATION = code[..Math.Min(code.Length, 10)],
                SHORT_NAME = code.Replace("_", " "), LONG_NAME = $"{code.Replace("_", " ")} (seeded)",
                ACTIVE_IND = "Y", EFFECTIVE_DATE = DateTime.UtcNow,
                PPDM_GUID = Guid.NewGuid().ToString(), SOURCE = "ACCOUNTING_MODULE"
            }, userId, result, $"R_BA_STATUS/{code}");
        }
    }
}
