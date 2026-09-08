using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Seeding;

/// <summary>
/// Seeds PPDM BA reference codes for accounting workflows.
/// Uses strongly-typed constants from <see cref="AccountingReferenceCodes"/>.
/// Populates R_BA_CATEGORY, R_BA_TYPE, R_BA_PREF_TYPE, and R_BA_STATUS.
/// </summary>
public class AccountingReferenceSeedService
{
    private readonly IDMEEditor _editor;
    private readonly string _connectionName;
    private readonly ILogger<AccountingReferenceSeedService>? _logger;

    public AccountingReferenceSeedService(IDMEEditor editor, string connectionName = "PPDM39", ILogger<AccountingReferenceSeedService>? logger = null)
    {
        _editor = editor;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task SeedAllAsync(string userId, CancellationToken ct = default)
    {
        await SeedFromConstantsAsync<R_BA_CATEGORY>("R_BA_CATEGORY", AccountingReferenceCodes.VendorLOVCodes.All, userId, ct);
        await SeedFromConstantsAsync<R_BA_TYPE>("R_BA_TYPE", AccountingReferenceCodes.VendorTypeCodes.All, userId, ct);
        await SeedFromConstantsAsync<R_BA_PREF_TYPE>("R_BA_PREF_TYPE", AccountingReferenceCodes.PreferenceTypeCodes.All, userId, ct);
        await SeedFromConstantsAsync<R_BA_STATUS>("R_BA_STATUS", AccountingReferenceCodes.BAStatusCodes.All, userId, ct);
    }

    private async Task SeedFromConstantsAsync<T>(string tableName, string[] codes, string userId, CancellationToken ct) where T : class, new()
    {
        var repo = GetRepo<T>(tableName);
        var existing = (await repo.GetAsync(new List<AppFilter>())).OfType<dynamic>().ToList();
        var existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in existing)
        {
            var key = (tableName switch
            {
                "R_BA_CATEGORY" => ((R_BA_CATEGORY)row).BA_CATEGORY,
                "R_BA_TYPE"     => ((R_BA_TYPE)row).BA_TYPE,
                "R_BA_PREF_TYPE" => ((R_BA_PREF_TYPE)row).PREFERENCE_TYPE,
                "R_BA_STATUS"   => ((R_BA_STATUS)row).BA_STATUS,
                _ => null
            }) ?? "";
            if (!string.IsNullOrEmpty(key)) existingKeys.Add(key);
        }

        foreach (var code in codes)
        {
            ct.ThrowIfCancellationRequested();
            if (existingKeys.Contains(code)) continue;

            dynamic entity = new
            {
                Code = code,
                Abbreviation = code.Length > 10 ? code[..10] : code,
                ShortName = code.Replace("_", " ").ToTitleCase(),
                LongName = $"{code.Replace("_", " ").ToTitleCase()} (auto-seeded)",
                ActiveInd = "Y",
                EffectiveDate = DateTime.UtcNow,
                PpdmGuid = Guid.NewGuid().ToString(),
                Source = "ACCOUNTING_SEED"
            };

            object record = tableName switch
            {
                "R_BA_CATEGORY" => new R_BA_CATEGORY { BA_CATEGORY = code, ABBREVIATION = code[..Math.Min(code.Length, 10)], SHORT_NAME = code.Replace("_", " "), LONG_NAME = $"{code.Replace("_", " ")} (seeded)", ACTIVE_IND = "Y", EFFECTIVE_DATE = DateTime.UtcNow, PPDM_GUID = Guid.NewGuid().ToString(), SOURCE = "ACCOUNTING_SEED" },
                "R_BA_TYPE"     => new R_BA_TYPE     { BA_TYPE     = code, ABBREVIATION = code[..Math.Min(code.Length, 10)], SHORT_NAME = code.Replace("_", " "), LONG_NAME = $"{code.Replace("_", " ")} (seeded)", ACTIVE_IND = "Y", EFFECTIVE_DATE = DateTime.UtcNow, PPDM_GUID = Guid.NewGuid().ToString(), SOURCE = "ACCOUNTING_SEED" },
                "R_BA_PREF_TYPE" => new R_BA_PREF_TYPE { PREFERENCE_TYPE = code, ABBREVIATION = code[..Math.Min(code.Length, 10)], SHORT_NAME = code.Replace("_", " "), LONG_NAME = $"{code.Replace("_", " ")} (seeded)", ACTIVE_IND = "Y", EFFECTIVE_DATE = DateTime.UtcNow, PPDM_GUID = Guid.NewGuid().ToString(), SOURCE = "ACCOUNTING_SEED" },
                "R_BA_STATUS"   => new R_BA_STATUS   { BA_STATUS   = code, ABBREVIATION = code[..Math.Min(code.Length, 10)], SHORT_NAME = code.Replace("_", " "), LONG_NAME = $"{code.Replace("_", " ")} (seeded)", ACTIVE_IND = "Y", EFFECTIVE_DATE = DateTime.UtcNow, PPDM_GUID = Guid.NewGuid().ToString(), SOURCE = "ACCOUNTING_SEED" },
                _ => throw new ArgumentException($"Unknown table: {tableName}")
            };

            await repo.InsertAsync(record, userId);
            _logger?.LogInformation("Seeded {Table}: {Code}", tableName, code);
        }
    }

    private PPDMGenericRepository GetRepo<T>(string tableName) =>
        new(_editor, null!, null!, null!, typeof(T), _connectionName, tableName, null);
}

/// <summary>Extension to convert SCREAMING_CASE to Title Case.</summary>
file static class StringExtensions
{
    public static string ToTitleCase(this string input) =>
        string.Join(" ", input.Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Length > 0 ? char.ToUpper(w[0]) + w[1..].ToLower() : ""));
}
