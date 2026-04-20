using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Integrations;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Integrations;

/// <summary>
/// Pulls monthly volume and daily allocation data from a PRODML REST endpoint
/// and upserts records into PPDM PDEN_VOL_SUMMARY and PDEN_VOL_DISPOSITION tables.
///
/// Unit conversions applied:
///   m³  → BBL  × 6.2898
///   ft³ → MCF  ÷ 1 000
/// </summary>
public class ProdmlAdapterService : IProdmlAdapter
{
    private const string AdapterName = "PRODML";

    private readonly IIntegrationHealthService      _health;
    private readonly IDMEEditor                     _editor;
    private readonly ICommonColumnHandler           _cch;
    private readonly IPPDM39DefaultsRepository      _defaults;
    private readonly IPPDMMetadataRepository        _metadata;
    private readonly string                         _connectionName;
    private readonly IHttpClientFactory             _httpFactory;
    private readonly ILogger<ProdmlAdapterService>  _logger;

    private const double CubicMetreToBbl = 6.2898;
    private const double CubicFeetToMcf  = 0.001;

    public ProdmlAdapterService(
        IIntegrationHealthService      health,
        IDMEEditor                     editor,
        ICommonColumnHandler           cch,
        IPPDM39DefaultsRepository      defaults,
        IPPDMMetadataRepository        metadata,
        string                         connectionName,
        IHttpClientFactory             httpFactory,
        ILogger<ProdmlAdapterService>  logger)
    {
        _health         = health;
        _editor         = editor;
        _cch            = cch;
        _defaults       = defaults;
        _metadata       = metadata;
        _connectionName = connectionName;
        _httpFactory    = httpFactory;
        _logger         = logger;
    }

    // ── IProdmlAdapter ────────────────────────────────────────────────────────

    public async Task<List<ProdmlWellSummary>> GetAvailableWellsAsync(string prodmlEndpoint)
    {
        try
        {
            CheckCircuitBreaker();
            using var client = MakeClient(prodmlEndpoint);
            var json = await client.GetStringAsync("wells");
            var doc  = JsonDocument.Parse(json);
            var list = new List<ProdmlWellSummary>();
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                list.Add(new ProdmlWellSummary(
                    Uid:      GetStr(el, "uid"),
                    WellName: GetStr(el, "name"),
                    Status:   GetStr(el, "status")));
            }
            _health.RecordSuccess(AdapterName);
            return list;
        }
        catch (IntegrationUnavailableException) { return new(); }
        catch (Exception ex)
        {
            _health.RecordFailure(AdapterName, ex);
            _logger.LogError(ex, "[PRODML] GetAvailableWells failed");
            return new();
        }
    }

    public async Task<ProdmlSyncResult> SyncMonthlyVolumesAsync(
        string prodmlEndpoint, string fieldId, int year, int month, string userId)
    {
        try
        {
            CheckCircuitBreaker();

            using var client = MakeClient(prodmlEndpoint);
            var json = await client.GetStringAsync(
                $"volumes/monthly?fieldId={Uri.EscapeDataString(fieldId)}&year={year}&month={month}");

            var doc           = JsonDocument.Parse(json);
            var volRepo       = await MakeRepoAsync("PDEN_VOL_SUMMARY");
            int volRows       = 0;

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var row = MapMonthlyVolumeElement(el, fieldId, year, month);
                await volRepo.InsertAsync(row, userId);
                volRows++;
            }

            var period = new DateRange(
                new DateTime(year, month, 1),
                new DateTime(year, month, DateTime.DaysInMonth(year, month)));

            _health.RecordSuccess(AdapterName);
            _health.AppendHistory(new IntegrationSyncHistoryEntry(
                AdapterName, DateTime.UtcNow, true, volRows, null));
            return new ProdmlSyncResult(true, volRows, 0, period, null);
        }
        catch (IntegrationUnavailableException ex)
        {
            return Fail(year, month, ex.Message);
        }
        catch (Exception ex)
        {
            _health.RecordFailure(AdapterName, ex);
            _logger.LogError(ex, "[PRODML] SyncMonthlyVolumes {Year}/{Month} failed", year, month);
            return Fail(year, month, ex.Message);
        }
    }

    public async Task<ProdmlSyncResult> SyncDailyAllocationsAsync(
        string prodmlEndpoint, string fieldId, DateTime date, string userId)
    {
        try
        {
            CheckCircuitBreaker();

            using var client = MakeClient(prodmlEndpoint);
            var json = await client.GetStringAsync(
                $"volumes/daily?fieldId={Uri.EscapeDataString(fieldId)}&date={date:yyyy-MM-dd}");

            var doc      = JsonDocument.Parse(json);
            var dispRepo = await MakeRepoAsync("PDEN_VOL_DISPOSITION");
            int rows     = 0;

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var row = MapDailyAllocationElement(el, fieldId, date);
                await dispRepo.InsertAsync(row, userId);
                rows++;
            }

            var period = new DateRange(date.Date, date.Date);

            _health.RecordSuccess(AdapterName);
            _health.AppendHistory(new IntegrationSyncHistoryEntry(
                AdapterName, DateTime.UtcNow, true, rows, null));
            return new ProdmlSyncResult(true, 0, rows, period, null);
        }
        catch (IntegrationUnavailableException ex)
        {
            return Fail(date.Year, date.Month, ex.Message);
        }
        catch (Exception ex)
        {
            _health.RecordFailure(AdapterName, ex);
            _logger.LogError(ex, "[PRODML] SyncDailyAllocations {Date} failed", date.ToString("yyyy-MM-dd"));
            return Fail(date.Year, date.Month, ex.Message);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void CheckCircuitBreaker()
    {
        var s = _health.GetStatusAsync(AdapterName).GetAwaiter().GetResult();
        if (s.State == CircuitBreakerState.Open)
            throw new IntegrationUnavailableException($"{AdapterName} circuit breaker is OPEN");
    }

    private HttpClient MakeClient(string baseUrl)
    {
        var client = _httpFactory.CreateClient("prodml");
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        return client;
    }

    private static object MapMonthlyVolumeElement(JsonElement el, string fieldId, int year, int month)
    {
        dynamic row = new System.Dynamic.ExpandoObject();
        var d = (IDictionary<string, object?>)row;
        d["PDEN_SOURCE"]    = "PRODML";
        d["PDEN_ID"]        = GetStr(el, "productionEntityId");
        d["PDEN_TYPE"]      = GetStr(el, "entityType");
        d["PROD_PERIOD"]    = new DateTime(year, month, 1);
        d["PERIOD_TYPE"]    = "M";
        d["FIELD_ID"]       = fieldId;

        // Oil (m³ → BBL)
        if (el.TryGetProperty("oilVolume", out var oilEl) &&
            oilEl.TryGetDouble(out var oilM3))
        {
            var unit = oilEl.TryGetProperty("uom", out var u) ? u.GetString() : "m3";
            d["OIL_GAS_RATIO"] = unit == "m3" ? oilM3 * CubicMetreToBbl : oilM3;
        }

        // Gas (ft³ → MCF)
        if (el.TryGetProperty("gasVolume", out var gasEl) &&
            gasEl.TryGetDouble(out var gasV))
        {
            var unit = gasEl.TryGetProperty("uom", out var u) ? u.GetString() : "ft3";
            d["GAS_PROD_VOL"] = unit == "ft3" ? gasV * CubicFeetToMcf : gasV;
        }

        // Water (m³ → BBL)
        if (el.TryGetProperty("waterVolume", out var watEl) &&
            watEl.TryGetDouble(out var watM3))
        {
            var unit = watEl.TryGetProperty("uom", out var u) ? u.GetString() : "m3";
            d["WATER_PROD_VOL"] = unit == "m3" ? watM3 * CubicMetreToBbl : watM3;
        }

        return row;
    }

    private static object MapDailyAllocationElement(JsonElement el, string fieldId, DateTime date)
    {
        dynamic row = new System.Dynamic.ExpandoObject();
        var d = (IDictionary<string, object?>)row;
        d["PDEN_SOURCE"]   = "PRODML";
        d["PDEN_ID"]       = GetStr(el, "productionEntityId");
        d["PROD_PERIOD"]   = date.Date;
        d["PERIOD_TYPE"]   = "D";
        d["FIELD_ID"]      = fieldId;
        d["DISP_TYPE"]     = GetStr(el, "dispositionType");

        if (el.TryGetProperty("volume", out var volEl) &&
            volEl.TryGetDouble(out var vol))
        {
            var unit = volEl.TryGetProperty("uom", out var u) ? u.GetString() : "m3";
            d["VOLUME"] = unit == "m3" ? vol * CubicMetreToBbl : vol;
        }

        return row;
    }

    private async Task<PPDMGenericRepository> MakeRepoAsync(string table)
    {
        var meta       = await _metadata.GetTableMetadataAsync(table);
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}") ?? typeof(object);
        return new PPDMGenericRepository(
            _editor, _cch, _defaults, _metadata, entityType, _connectionName, table);
    }

    private static string GetStr(JsonElement el, string key)
        => el.TryGetProperty(key, out var p) ? p.GetString() ?? string.Empty : string.Empty;

    private static ProdmlSyncResult Fail(int y, int m, string msg)
        => new(false, 0, 0, new DateRange(new DateTime(y, m, 1),
               new DateTime(y, m, DateTime.DaysInMonth(y, m))), msg);
}
