using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProductionForecasting.Constants;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionForecasting.Services
{
    /// <summary>
    /// Loads aligned oil (and optional water) time series from PPDM for decline fitting.
    /// </summary>
    internal static class ProductionHistoryLoader
    {
        /// <summary>
        /// Daily-equivalent oil rate from period volume (STB/month approximation).
        /// </summary>
        private static double ToDailyOilRate(decimal periodOilVolume)
        {
            if (periodOilVolume <= 0) return 0;
            return (double)(periodOilVolume / ForecastAlgorithmConstants.DaysPerMonth);
        }

        public static async Task<(List<double> OilRates, List<DateTime> Dates)?> TryLoadOilHistoryAsync(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName,
            string wellUwi)
        {
            if (string.IsNullOrWhiteSpace(wellUwi))
                return null;

            var pdenWellRepo = new PPDMGenericRepository(editor, commonColumnHandler, defaults, metadata,
                typeof(PDEN_WELL), connectionName, "PDEN_WELL", null);

            var pdenLinks = await pdenWellRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "PRIMARY_UWI", Operator = "=", FilterValue = wellUwi },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = defaults.GetActiveIndicatorYes() }
            });

            var pdenIds = pdenLinks.OfType<PDEN_WELL>()
                .Select(x => x.PDEN_ID)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (pdenIds.Count == 0)
                return null;

            var volRepo = new PPDMGenericRepository(editor, commonColumnHandler, defaults, metadata,
                typeof(PDEN_VOL_SUMMARY), connectionName, "PDEN_VOL_SUMMARY", null);

            var rows = new List<PDEN_VOL_SUMMARY>();
            foreach (var pdenId in pdenIds)
            {
                var formatted = defaults.FormatIdForTable("PDEN_VOL_SUMMARY", pdenId);
                var batch = await volRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "PDEN_ID", Operator = "=", FilterValue = formatted },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = defaults.GetActiveIndicatorYes() }
                });
                rows.AddRange(batch.OfType<PDEN_VOL_SUMMARY>());
            }

            var ordered = rows
                .Select(r => new
                {
                    Row = r,
                    Date = r.EFFECTIVE_DATE ?? r.EXPIRY_DATE
                })
                .Where(x => x.Date.HasValue)
                .OrderBy(x => x.Date!.Value)
                .ToList();

            if (ordered.Count < ForecastAlgorithmConstants.MinHistoryPointsForFit)
                return null;

            var rates = new List<double>();
            var dates = new List<DateTime>();
            foreach (var x in ordered)
            {
                var q = ToDailyOilRate(x.Row.OIL_VOLUME);
                if (q <= 0) continue;
                dates.Add(x.Date!.Value);
                rates.Add(q);
            }

            if (rates.Count < ForecastAlgorithmConstants.MinHistoryPointsForFit)
                return null;

            return (rates, dates);
        }
    }
}
