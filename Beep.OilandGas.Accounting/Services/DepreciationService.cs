using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// Depreciation Service - Fixed asset depreciation scheduling and calculations
    /// Supports multiple depreciation methods (Straight-line, MACRS, Units of Production)
    /// Integrates with GL for automatic depreciation posting
    /// </summary>
    public class DepreciationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly GLAccountService _glAccountService;
        private readonly ILogger<DepreciationService> _logger;
        private const string ConnectionName = "PPDM39";

        public DepreciationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            GLAccountService glAccountService,
            ILogger<DepreciationService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _glAccountService = glAccountService ?? throw new ArgumentNullException(nameof(glAccountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create fixed asset for depreciation
        /// </summary>
        public async Task<FixedAsset> CreateFixedAssetAsync(
            string assetId,
            string assetName,
            decimal purchasePrice,
            DateTime purchaseDate,
            decimal salvageValue,
            int usefulLifeYears,
            DepreciationMethod method,
            string userId)
        {
            _logger?.LogInformation("Creating fixed asset {AssetId}. Cost: {Cost:C}, Life: {Years} years",
                assetId, purchasePrice, usefulLifeYears);

            try
            {
                if (string.IsNullOrWhiteSpace(assetId))
                    throw new ArgumentNullException(nameof(assetId));

                if (purchasePrice <= 0)
                    throw new InvalidOperationException("Purchase price must be positive");

                if (usefulLifeYears <= 0)
                    throw new InvalidOperationException("Useful life must be positive");

                if (salvageValue < 0)
                    throw new InvalidOperationException("Salvage value cannot be negative");

                var asset = new FixedAsset
                {
                    AssetId = assetId,
                    AssetName = assetName,
                    PurchasePrice = purchasePrice,
                    PurchaseDate = purchaseDate,
                    SalvageValue = Math.Min(salvageValue, purchasePrice),
                    UsefulLifeYears = usefulLifeYears,
                    DepreciationMethod = method,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId,
                    Status = "ACTIVE",
                    DepreciationSchedule = new List<DepreciationEntry>()
                };

                // Calculate depreciable base
                asset.DepreciableBase = asset.PurchasePrice - asset.SalvageValue;

                // Generate depreciation schedule
                await GenerateDepreciationScheduleAsync(asset);

                _logger?.LogInformation("Fixed asset created. Depreciable Base: {Base:C}", asset.DepreciableBase);
                return asset;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating fixed asset: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generate depreciation schedule for asset
        /// </summary>
        private async Task GenerateDepreciationScheduleAsync(FixedAsset asset)
        {
            _logger?.LogInformation("Generating depreciation schedule for {Asset} using {Method}",
                asset.AssetId, asset.DepreciationMethod);

            try
            {
                decimal remainingDepreciableBase = asset.DepreciableBase;

                switch (asset.DepreciationMethod)
                {
                    case DepreciationMethod.StraightLine:
                        remainingDepreciableBase = GenerateStraightLineSchedule(asset);
                        break;

                    case DepreciationMethod.DoubleDeclining:
                        remainingDepreciableBase = GenerateDoubleDeclingSchedule(asset);
                        break;

                    case DepreciationMethod.UnitsOfProduction:
                        // Units of production requires usage data
                        asset.DepreciationSchedule = new List<DepreciationEntry>();
                        break;

                    case DepreciationMethod.MACRS:
                        remainingDepreciableBase = GenerateMACRSSchedule(asset);
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown depreciation method: {asset.DepreciationMethod}");
                }

                asset.TotalScheduledDepreciation = asset.DepreciationSchedule.Sum(x => x.AnnualDepreciation);
                _logger?.LogInformation("Depreciation schedule generated. Total Depreciation: {Total:C}",
                    asset.TotalScheduledDepreciation);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating depreciation schedule: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Calculate straight-line depreciation
        /// </summary>
        private decimal GenerateStraightLineSchedule(FixedAsset asset)
        {
            decimal annualDepreciation = asset.DepreciableBase / asset.UsefulLifeYears;
            decimal accumulatedDepreciation = 0;

            for (int year = 1; year <= asset.UsefulLifeYears; year++)
            {
                accumulatedDepreciation += annualDepreciation;
                decimal bookValue = asset.PurchasePrice - accumulatedDepreciation;

                asset.DepreciationSchedule.Add(new DepreciationEntry
                {
                    Year = year,
                    AnnualDepreciation = annualDepreciation,
                    AccumulatedDepreciation = accumulatedDepreciation,
                    BookValue = bookValue,
                    Method = "Straight-Line"
                });
            }

            return accumulatedDepreciation;
        }

        /// <summary>
        /// Calculate double declining balance depreciation
        /// </summary>
        private decimal GenerateDoubleDeclingSchedule(FixedAsset asset)
        {
            decimal rate = 2m / asset.UsefulLifeYears;
            decimal bookValue = asset.PurchasePrice;
            decimal accumulatedDepreciation = 0;

            for (int year = 1; year <= asset.UsefulLifeYears; year++)
            {
                decimal annualDepreciation = bookValue * rate;
                accumulatedDepreciation += annualDepreciation;
                bookValue -= annualDepreciation;

                asset.DepreciationSchedule.Add(new DepreciationEntry
                {
                    Year = year,
                    AnnualDepreciation = annualDepreciation,
                    AccumulatedDepreciation = accumulatedDepreciation,
                    BookValue = bookValue,
                    Method = "Double Declining"
                });
            }

            return accumulatedDepreciation;
        }

        /// <summary>
        /// Calculate MACRS depreciation (Modified Accelerated Cost Recovery System)
        /// </summary>
        private decimal GenerateMACRSSchedule(FixedAsset asset)
        {
            // MACRS rates for 5-year property (common for business equipment)
            decimal[] macrsRates = { 0.20m, 0.32m, 0.192m, 0.1152m, 0.1152m, 0.0576m };
            decimal accumulatedDepreciation = 0;

            for (int year = 0; year < macrsRates.Length; year++)
            {
                decimal annualDepreciation = asset.DepreciableBase * macrsRates[year];
                accumulatedDepreciation += annualDepreciation;
                decimal bookValue = asset.PurchasePrice - accumulatedDepreciation;

                asset.DepreciationSchedule.Add(new DepreciationEntry
                {
                    Year = year + 1,
                    AnnualDepreciation = annualDepreciation,
                    AccumulatedDepreciation = accumulatedDepreciation,
                    BookValue = bookValue,
                    Method = "MACRS"
                });
            }

            return accumulatedDepreciation;
        }

        /// <summary>
        /// Calculate depreciation for a specific year
        /// </summary>
        public async Task<DepreciationEntry> CalculateYearDepreciationAsync(
            FixedAsset asset,
            int depreciationYear)
        {
            _logger?.LogInformation("Calculating depreciation for {Asset} year {Year}",
                asset.AssetId, depreciationYear);

            try
            {
                var entry = asset.DepreciationSchedule.FirstOrDefault(x => x.Year == depreciationYear);
                if (entry == null)
                    throw new InvalidOperationException($"No depreciation entry found for year {depreciationYear}");

                return entry;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating year depreciation: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generate asset depreciation summary
        /// </summary>
        public async Task<AssetDepreciationSummary> GenerateDepreciationSummaryAsync(
            DateTime asOfDate)
        {
            _logger?.LogInformation("Generating depreciation summary as of {Date}", asOfDate.Date);

            try
            {
                var summary = new AssetDepreciationSummary
                {
                    AsOfDate = asOfDate,
                    GeneratedDate = DateTime.UtcNow,
                    Assets = new List<AssetSummaryLine>()
                };

                // Get accumulated depreciation from GL
                var accumulatedDepreciationAccount = await _glAccountService.GetAccountByNumberAsync("1210");
                if (accumulatedDepreciationAccount != null)
                {
                    var accumulatedBalance = await _glAccountService.GetAccountBalanceAsync("1210", asOfDate);
                    summary.TotalAccumulatedDepreciation = Math.Abs(accumulatedBalance);
                }

                // Get fixed assets
                var fixedAssetAccount = await _glAccountService.GetAccountByNumberAsync("1200");
                if (fixedAssetAccount != null)
                {
                    var fixedAssetBalance = await _glAccountService.GetAccountBalanceAsync("1200", asOfDate);
                    summary.TotalFixedAssetsCost = fixedAssetBalance;
                }

                summary.TotalNetBookValue = summary.TotalFixedAssetsCost - summary.TotalAccumulatedDepreciation;

                _logger?.LogInformation("Depreciation summary generated. Net Book Value: {NBV:C}",
                    summary.TotalNetBookValue);

                return summary;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating depreciation summary: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Export depreciation schedule as formatted text
        /// </summary>
        public string ExportDepreciationScheduleAsText(FixedAsset asset)
        {
            _logger?.LogInformation("Exporting depreciation schedule as text");

            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine("                    FIXED ASSET DEPRECIATION SCHEDULE");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                sb.AppendLine("ASSET INFORMATION:");
                sb.AppendLine($"  Asset ID:                    {asset.AssetId}");
                sb.AppendLine($"  Asset Name:                  {asset.AssetName}");
                sb.AppendLine($"  Acquisition Date:           {asset.PurchaseDate:MMMM dd, yyyy}");
                sb.AppendLine($"  Depreciation Method:        {asset.DepreciationMethod}");
                sb.AppendLine($"  Useful Life:                {asset.UsefulLifeYears} years");
                sb.AppendLine();

                sb.AppendLine("ASSET VALUE:");
                sb.AppendLine($"  Original Cost:              ${asset.PurchasePrice,15:N2}");
                sb.AppendLine($"  Salvage Value:              ${asset.SalvageValue,15:N2}");
                sb.AppendLine($"  Depreciable Base:           ${asset.DepreciableBase,15:N2}");
                sb.AppendLine();

                sb.AppendLine("DEPRECIATION SCHEDULE:");
                sb.AppendLine("Year | Annual Depreciation | Accumulated Depreciation | Book Value");
                sb.AppendLine("─────────────────────────────────────────────────────────────────────");

                foreach (var entry in asset.DepreciationSchedule.OrderBy(x => x.Year))
                {
                    sb.AppendLine($"{entry.Year,4} | ${entry.AnnualDepreciation,18:N2} | ${entry.AccumulatedDepreciation,22:N2} | ${entry.BookValue,10:N2}");
                }

                sb.AppendLine("═════════════════════════════════════════════════════════════════════");
                sb.AppendLine($"  {"TOTAL DEPRECIATION",-38} ${asset.TotalScheduledDepreciation,22:N2}");
                sb.AppendLine($"  {"FINAL BOOK VALUE",-38} ${(asset.PurchasePrice - asset.TotalScheduledDepreciation),22:N2}");
                sb.AppendLine("═════════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                sb.AppendLine($"Created: {asset.CreatedDate:MMMM dd, yyyy hh:mm:ss tt}");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting depreciation schedule: {Message}", ex.Message);
                throw;
            }
        }
    }
}
