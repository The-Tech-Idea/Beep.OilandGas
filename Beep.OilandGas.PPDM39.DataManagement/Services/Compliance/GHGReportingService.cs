using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Compliance;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Compliance;

/// <summary>
/// Calculates GHG emissions per EPA 40 CFR 98 (USA) and ECCC NIR (Canada).
/// Emission factors are loaded from EmissionFactors.json deployed alongside the API.
/// </summary>
public class GHGReportingService : IGHGReportingService
{
    private readonly IDMEEditor                      _editor;
    private readonly ICommonColumnHandler            _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository       _defaults;
    private readonly IPPDMMetadataRepository         _metadata;
    private readonly string                          _connectionName;
    private readonly ILogger<GHGReportingService>    _logger;
    private readonly List<EmissionFactorEntry>       _factors;

    public GHGReportingService(
        IDMEEditor                     editor,
        ICommonColumnHandler           commonColumnHandler,
        IPPDM39DefaultsRepository      defaults,
        IPPDMMetadataRepository        metadata,
        string                         connectionName,
        ILogger<GHGReportingService>   logger,
        string                         emissionFactorJsonPath = "")
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _connectionName      = connectionName;
        _logger              = logger;
        _factors             = LoadFactors(emissionFactorJsonPath);
    }

    public async Task<GHGEmissionReport> GenerateAnnualReportAsync(
        string fieldId, int year, string jurisdiction, string userId)
    {
        try
        {
            var sources = await BuildSourceLinesAsync(fieldId, year, jurisdiction);
            var total   = sources.Sum(s => s.EmissionsCO2e);

            return new GHGEmissionReport(
                fieldId, year, jurisdiction,
                total, "t_CO2e",
                sources,
                DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GHG report generation failed for {FieldId} {Year}", fieldId, year);
            throw;
        }
    }

    public async Task<List<EmissionSourceLine>> GetEmissionSourcesAsync(string fieldId, int year)
    {
        // Default to USA; the caller can use GenerateAnnualReportAsync with explicit jurisdiction
        return await BuildSourceLinesAsync(fieldId, year, "USA");
    }

    public async Task<double> GetTotalEmissionsAsync(
        string fieldId, int year, string? jurisdiction = null)
    {
        var jur     = jurisdiction ?? "USA";
        var sources = await BuildSourceLinesAsync(fieldId, year, jur);
        return sources.Sum(s => s.EmissionsCO2e);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<List<EmissionSourceLine>> BuildSourceLinesAsync(
        string fieldId, int year, string jurisdiction)
    {
        var sources = new List<EmissionSourceLine>();

        double flareVol  = await GetDispositionVolumeAsync(fieldId, year, "FLARE");
        double ventVol   = await GetDispositionVolumeAsync(fieldId, year, "VENT");
        double dieselLit = await GetEquipmentFuelAsync(fieldId, year, "COMBUSTION_ENGINE");

        // FLARE_GAS
        var flareFactor = FindFactor("FLARE_GAS", jurisdiction);
        if (flareFactor is not null && flareVol > 0)
            sources.Add(new EmissionSourceLine(
                "FLARE_GAS", flareFactor.Regulation,
                flareVol, flareFactor.VolumeUnit,
                flareFactor.Factor, flareFactor.Unit,
                flareVol * flareFactor.Factor));

        // VENTED_GAS
        var ventFactor = FindFactor("VENTED_GAS", jurisdiction);
        if (ventFactor is not null && ventVol > 0)
            sources.Add(new EmissionSourceLine(
                "VENTED_GAS", ventFactor.Regulation,
                ventVol, ventFactor.VolumeUnit,
                ventFactor.Factor, ventFactor.Unit,
                ventVol * ventFactor.Factor));

        // DIESEL_COMBUSTION
        var dieselFactor = FindFactor("DIESEL_COMBUSTION", jurisdiction);
        if (dieselFactor is not null && dieselLit > 0)
            sources.Add(new EmissionSourceLine(
                "DIESEL_COMBUSTION", dieselFactor.Regulation,
                dieselLit, "litres",
                dieselFactor.Factor, dieselFactor.Unit,
                dieselLit * dieselFactor.Factor));

        return sources;
    }

    private async Task<double> GetDispositionVolumeAsync(
        string fieldId, int year, string dispType)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PDEN_VOL_DISPOSITION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}") ?? typeof(object);
            var repo       = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "PDEN_VOL_DISPOSITION");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "FIELD_ID",   Operator = "=", FilterValue = fieldId },
                new() { FieldName = "DISP_TYPE",  Operator = "=", FilterValue = dispType },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var rows = (await repo.GetAsync(filters)).ToList()
                .Where(r =>
                {
                    var prop = r.GetType().GetProperty("PERIOD_YEAR",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.IgnoreCase);
                    var val = prop?.GetValue(r)?.ToString();
                    return val == year.ToString();
                });

            return rows.Sum(r =>
            {
                var prop = r.GetType().GetProperty("DISP_VOL",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.IgnoreCase);
                var val = prop?.GetValue(r);
                return val is double d ? d : (val is decimal dc ? (double)dc : 0.0);
            });
        }
        catch { return 0.0; }
    }

    private async Task<double> GetEquipmentFuelAsync(
        string fieldId, int year, string equipType)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("EQUIPMENT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}") ?? typeof(object);
            var repo       = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "EQUIPMENT");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "FIELD_ID",   Operator = "=", FilterValue = fieldId },
                new() { FieldName = "EQUIP_TYPE", Operator = "=", FilterValue = equipType },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var rows = (await repo.GetAsync(filters)).ToList();
            return rows.Sum(r =>
            {
                var prop = r.GetType().GetProperty("FUEL_CONSUMPTION",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.IgnoreCase);
                var val = prop?.GetValue(r);
                return val is double d ? d : (val is decimal dc ? (double)dc : 0.0);
            });
        }
        catch { return 0.0; }
    }

    private EmissionFactorEntry? FindFactor(string sourceCategory, string jurisdiction)
        => _factors.FirstOrDefault(f =>
               f.SourceCategory.Equals(sourceCategory, StringComparison.OrdinalIgnoreCase) &&
               f.Jurisdiction.Equals(jurisdiction, StringComparison.OrdinalIgnoreCase));

    private static List<EmissionFactorEntry> LoadFactors(string jsonPath)
    {
        // Try to load from file; fall back to built-in defaults
        if (!string.IsNullOrWhiteSpace(jsonPath) && File.Exists(jsonPath))
        {
            try
            {
                var json = File.ReadAllText(jsonPath);
                var doc  = JsonDocument.Parse(json);
                var arr  = doc.RootElement.GetProperty("factors");
                return JsonSerializer.Deserialize<List<EmissionFactorEntry>>(arr.GetRawText())
                       ?? BuiltInFactors();
            }
            catch { /* fall through */ }
        }
        return BuiltInFactors();
    }

    private static List<EmissionFactorEntry> BuiltInFactors() =>
    [
        new("FLARE_GAS",        "USA",           "40CFR98_W", 1.9959,   "t_CO2e_per_MSCF",      "MSCF"),
        new("VENTED_GAS",       "USA",           "40CFR98_W", 0.05306,  "t_CO2e_per_MSCF",      "MSCF"),
        new("DIESEL_COMBUSTION","USA",           "40CFR98_C", 0.002785, "t_CO2e_per_litre",     "litres"),
        new("FLARE_GAS",        "CANADA",        "ECCC_NIR",  2.18,     "t_CO2e_per_1000m3",    "1000m3"),
        new("VENTED_GAS",       "CANADA",        "ECCC_NIR",  1.22,     "t_CO2e_per_1000m3",    "1000m3"),
        new("DIESEL_COMBUSTION","CANADA",        "ECCC_NIR",  0.002690, "t_CO2e_per_litre",     "litres"),
        new("FLARE_GAS",        "INTERNATIONAL", "IOGP_GHG",  2.00,     "t_CO2e_per_MSCF",      "MSCF"),
        new("VENTED_GAS",       "INTERNATIONAL", "IOGP_GHG",  0.060,    "t_CO2e_per_MSCF",      "MSCF"),
    ];

    private sealed record EmissionFactorEntry(
        string SourceCategory,
        string Jurisdiction,
        string Regulation,
        double Factor,
        string Unit,
        string VolumeUnit);
}
