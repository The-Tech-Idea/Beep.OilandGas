using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData.DummyData
{
    public partial class PPDM39DummyDataGenerator
    {
        /// <summary>
        /// Seeds monthly PDEN_VOL_SUMMARY rows for every seeded well.
        /// Returns total rows inserted.
        /// </summary>
        private async Task<int> GenerateProductionAsync(string userId)
        {
            var repo   = MakeRepo<PDEN_VOL_SUMMARY>();
            var rng    = new Random(99);
            int count  = 0;
            var origin = DateTime.UtcNow.AddMonths(-_productionMonths);

            for (int uwi_i = 0; uwi_i < SeededUwis.Count; uwi_i++)
            {
                var uwi      = SeededUwis[uwi_i];
                var pdenId   = $"PDEN_{uwi}";
                // Randomise base rates per well (bbls/month and mmscf/month)
                decimal baseOil = rng.Next(200, 2500);
                decimal baseGas = rng.Next(50, 800);
                decimal baseWater = rng.Next(10, 300);
                // Simple exponential decline factor (5–15 % / year)
                double declinePerMonth = (rng.NextDouble() * 0.10 + 0.05) / 12.0;

                for (int m = 0; m < _productionMonths; m++)
                {
                    var periodDate = origin.AddMonths(m);
                    var periodId   = periodDate.ToString("yyyyMM");

                    double factor = Math.Pow(1.0 - declinePerMonth, m);
                    decimal oil   = Math.Max(0m, baseOil   * (decimal)factor);
                    decimal gas   = Math.Max(0m, baseGas   * (decimal)factor);
                    decimal water = Math.Max(0m, baseWater * (decimal)factor);

                    var summary = new PDEN_VOL_SUMMARY
                    {
                        PDEN_SUBTYPE       = "WELL",
                        PDEN_ID            = pdenId,
                        PERIOD_ID          = periodId,
                        PDEN_SOURCE        = "DEMO",
                        VOLUME_METHOD      = "MEASURED",
                        AMENDMENT_SEQ_NO   = 0,
                        PERIOD_TYPE        = "MONTH",
                        ACTIVITY_TYPE      = "PRODUCTION",
                        OIL_VOLUME         = oil,
                        BOE_VOLUME         = oil + gas * 6m // rough conversion
                    };

                    if (summary is IPPDMEntity ppdm)
                        _commonColumnHandler.PrepareForInsert(ppdm, userId);

                    try
                    {
                        await repo.InsertAsync(summary, userId);
                        count++;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "[Production] Skipped row {PdenId}/{Period}", pdenId, periodId);
                    }
                }

                _logger?.LogDebug("[Production] {Count} months seeded for {UWI}", _productionMonths, uwi);
            }

            return count;
        }
    }
}
