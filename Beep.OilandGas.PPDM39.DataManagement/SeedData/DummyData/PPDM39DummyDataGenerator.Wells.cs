using System;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData.DummyData
{
    public partial class PPDM39DummyDataGenerator
    {
        private static readonly string[] WellSuffixes =
            { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10",
              "ST1", "ST2", "H1", "H2", "D1" };

        private static readonly string[] WellTypes   = { "Oil", "Gas", "Injection", "Water Disposal" };
        private static readonly string[] Formations  = { "Cretaceous", "Jurassic", "Triassic", "Permian", "Devonian" };

        /// <summary>
        /// Seeds WELL records via <see cref="WellServices"/> (which calls <c>initializeDefaultStatuses: true</c>
        /// so all 13 PPDM WSC v3 STATUS_TYPE facets are seeded per well).
        /// UWIs are recorded in <see cref="SeededUwis"/>.
        /// </summary>
        private async Task GenerateWellsAsync(string userId)
        {
            var rng = new Random(42); // deterministic seed for reproducibility

            for (int fi = 0; fi < SeededFieldIds.Count; fi++)
            {
                var fieldId = SeededFieldIds[fi];

                for (int wi = 0; wi < _wellsPerField; wi++)
                {
                    var suffix = WellSuffixes[wi % WellSuffixes.Length];
                    var uwi    = $"DEMO_{fi + 1:D3}_{wi + 1:D3}";
                    var name   = $"Demo-{fi + 1}-{suffix}";

                    var well = new WELL
                    {
                        UWI            = uwi,
                        WELL_NAME      = name,
                        ASSIGNED_FIELD = fieldId,
                        FINAL_TD       = (decimal)(rng.Next(1200, 4500)),
                        REMARK         = $"Auto-generated demo well — {Formations[wi % Formations.Length]}"
                    };

                    try
                    {
                        await _wellServices.CreateAsync(well, userId, initializeDefaultStatuses: true);
                        SeededUwis.Add(uwi);
                        _logger?.LogDebug("[Wells] Seeded {UWI}", uwi);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "[Wells] Could not insert {UWI} — may already exist, skipping", uwi);
                    }
                }
            }
        }
    }
}
