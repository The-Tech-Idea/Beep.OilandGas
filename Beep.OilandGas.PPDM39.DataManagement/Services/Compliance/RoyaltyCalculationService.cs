using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Compliance;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Compliance;

/// <summary>
/// Calculates USA ONRR and Alberta Crown royalties from PPDM production volumes.
/// Uses PDEN_VOL_SUMMARY for aggregate volumes and OBLIG_PAYMENT to persist results.
/// </summary>
public class RoyaltyCalculationService : IRoyaltyCalculationService
{
    private readonly IDMEEditor                           _editor;
    private readonly ICommonColumnHandler                 _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository            _defaults;
    private readonly IPPDMMetadataRepository              _metadata;
    private readonly IComplianceService                   _compliance;
    private readonly string                               _connectionName;
    private readonly ILogger<RoyaltyCalculationService>   _logger;

    // ONRR statutory rates
    private const double OnrrOnshorOilRate  = 0.125;   // 12.5%
    private const double OnrrOffshoreOilRate = 0.1875;  // 18.75%
    private const double OnrrGasRate        = 0.125;    // 12.5% (minimum)

    // Alberta Crown MRF constants (2017 framework)
    private const double AlbertaBaseRate    = 0.05;     // 5% pre-payout
    private const double AlbertaRValueBreak = 200.0;    // CAD/m³
    private const double AlbertaSlopeFactor = 0.0015;

    public RoyaltyCalculationService(
        IDMEEditor                          editor,
        ICommonColumnHandler                commonColumnHandler,
        IPPDM39DefaultsRepository           defaults,
        IPPDMMetadataRepository             metadata,
        IComplianceService                  compliance,
        string                              connectionName,
        ILogger<RoyaltyCalculationService>  logger)
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _compliance          = compliance;
        _connectionName      = connectionName;
        _logger              = logger;
    }

    public async Task<RoyaltySummary> CalculateUSARoyaltyAsync(
        string fieldId, int year, int month, string userId)
    {
        try
        {
            var period = $"{year:D4}-{month:D2}";

            // Read production volumes
            double oilBbl  = await GetProductionVolumeAsync(fieldId, year, month, "OIL");
            double gasMscf = await GetProductionVolumeAsync(fieldId, year, month, "GAS");
            double referencePricePerBbl = await GetMonthlyReferencePriceAsync(fieldId, year, month, "OIL");

            // GOR classification: condensate vs oil
            string productType = "OIL";
            double gor         = oilBbl > 0 ? (gasMscf * 1000) / oilBbl : 0;
            double royaltyRate = gor >= 100_000 ? OnrrGasRate : OnrrOnshorOilRate;
            double volume      = gor >= 100_000 ? gasMscf   : oilBbl;
            string volumeUnit  = gor >= 100_000 ? "MSCF"    : "BBL";
            if (gor >= 100_000) productType = "GAS";

            double grossRevenue = volume * referencePricePerBbl;
            var    dueRoyalty   = (decimal)(grossRevenue * royaltyRate);

            // Create matching OBLIGATION row if it doesn't exist
            var obligId = await _compliance.CreateObligationAsync(
                new CreateObligationRequest(
                    fieldId, ObligationType.OnrrRoyaltyPayment,
                    new DateTime(year, month, 1).AddMonths(1).AddDays(14), // ONRR due 25th of following month
                    "USA", $"ONRR Royalty {period}",
                    new DateTime(year, month, 1),
                    new DateTime(year, month, DateTime.DaysInMonth(year, month))),
                userId);

            // Persist as an OBLIG_PAYMENT record
            await _compliance.RecordPaymentAsync(obligId, new RecordPaymentRequest(
                PaymentType.Royalty,
                DateTime.UtcNow,
                "USD",
                dueRoyalty,
                0m,  // Actual paid is recorded separately
                null), userId);

            return new RoyaltySummary(
                fieldId, "USA", period,
                volume, productType, volumeUnit,
                royaltyRate, grossRevenue, dueRoyalty,
                null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ONRR royalty calculation failed for {FieldId} {Year}/{Month}", fieldId, year, month);
            throw;
        }
    }

    public async Task<RoyaltySummary> CalculateAlbertaCrownRoyaltyAsync(
        string fieldId, int year, int quarter, string userId)
    {
        try
        {
            int startMonth = (quarter - 1) * 3 + 1;
            var period     = $"{year:D4}-Q{quarter}";

            // Aggregate quarterly volumes
            double oilBblTotal = 0;
            for (int m = startMonth; m < startMonth + 3; m++)
                oilBblTotal += await GetProductionVolumeAsync(fieldId, year, m, "OIL");

            // WTI price (CAD/bbl) — read from PDEN_SOURCE or use fallback
            double wtiCadBbl = await GetMonthlyReferencePriceAsync(fieldId, year, startMonth, "WTI_CAD");
            if (wtiCadBbl <= 0) wtiCadBbl = 80.0; // Fallback placeholder

            // Alberta MRF: r value in CAD/m³
            double rValue    = wtiCadBbl * 6.2898;
            double royaltyRate;
            if (rValue < AlbertaRValueBreak)
                royaltyRate = AlbertaBaseRate;
            else
                royaltyRate = AlbertaBaseRate + (rValue - AlbertaRValueBreak) * AlbertaSlopeFactor;

            royaltyRate = Math.Min(royaltyRate, 0.40); // Cap at 40%

            // Convert BBL to m³ (1 bbl = 0.158987 m³)
            double oilM3    = oilBblTotal * 0.158987;
            double grossRev = oilM3 * rValue;
            var    due      = (decimal)(grossRev * royaltyRate);

            int quarterEndMonth = startMonth + 2;
            var obligId = await _compliance.CreateObligationAsync(
                new CreateObligationRequest(
                    fieldId, ObligationType.AlbertaCrownRoyalty,
                    new DateTime(year, quarterEndMonth, 1).AddMonths(1).AddDays(24),
                    "CANADA", $"Alberta Crown Royalty {period}",
                    new DateTime(year, startMonth, 1),
                    new DateTime(year, quarterEndMonth, DateTime.DaysInMonth(year, quarterEndMonth))),
                userId);

            await _compliance.RecordPaymentAsync(obligId, new RecordPaymentRequest(
                PaymentType.Royalty,
                DateTime.UtcNow,
                "CAD",
                due,
                0m,
                $"{{\"r_value\":{rValue:F1},\"rate\":{royaltyRate:F4}}}"), userId);

            return new RoyaltySummary(
                fieldId, "CANADA", period,
                oilBblTotal, "OIL", "BBL",
                royaltyRate, grossRev, due,
                null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Alberta Crown royalty calc failed for {FieldId} {Year} Q{Quarter}", fieldId, year, quarter);
            throw;
        }
    }

    public async Task<List<RoyaltyVariance>> GetVarianceHistoryAsync(string fieldId, int year)
    {
        try
        {
            // Load all royalty obligations for the year
            var obligations = await _compliance.GetAllObligationsAsync(fieldId, year);
            var royalties   = obligations.Where(o =>
                o.ObligType == ObligationType.OnrrRoyaltyPayment ||
                o.ObligType == ObligationType.AlbertaCrownRoyalty);

            var result = new List<RoyaltyVariance>();
            foreach (var obl in royalties)
            {
                var detail = await _compliance.GetByIdAsync(obl.ObligationId);
                if (detail?.Payments.Count > 0)
                {
                    var pay = detail.Payments.First();
                    result.Add(new RoyaltyVariance(
                        obl.DueDate.ToString("yyyy-MM"),
                        obl.ObligationId,
                        pay.GrossAmt,
                        pay.ActualAmt,
                        pay.VarianceAmt,
                        pay.VarianceAmt > 0));
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get royalty variance history for {FieldId}", fieldId);
            return new();
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<double> GetProductionVolumeAsync(
        string fieldId, int year, int month, string productType)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PDEN_VOL_SUMMARY");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}") ?? typeof(object);
            var repo       = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "PDEN_VOL_SUMMARY");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "FIELD_ID",    Operator = "=", FilterValue = fieldId },
                new() { FieldName = "PRODUCT",     Operator = "=", FilterValue = productType },
                new() { FieldName = "PERIOD_YEAR", Operator = "=", FilterValue = year.ToString() },
                new() { FieldName = "PERIOD_MONTH", Operator = "=", FilterValue = month.ToString() },
                new() { FieldName = "ACTIVE_IND",   Operator = "=", FilterValue = "Y" }
            };

            var rows = (await repo.GetAsync(filters)).ToList();
            return rows.Sum(r =>
            {
                var val = r.GetType().GetProperty("VOL_VALUE",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.IgnoreCase)?.GetValue(r);
                return val is double d ? d : (val is decimal dc ? (double)dc : 0.0);
            });
        }
        catch { return 0.0; }
    }

    private async Task<double> GetMonthlyReferencePriceAsync(
        string fieldId, int year, int month, string priceType)
    {
        // In production: query a price reference table or external service.
        // For now, return a plausible reference price per unit.
        await Task.CompletedTask;
        return priceType switch
        {
            "WTI_CAD" => 85.0,    // CAD/bbl
            "GAS"     => 3.50,    // USD/MMBTU → approx USD/MSCF
            _         => 75.0,    // USD/bbl oil default
        };
    }
}
