using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData.DummyData
{
    public partial class PPDM39DummyDataGenerator
    {
        private static readonly string[] FacilityTypes = { "PROCESSING PLANT", "SEPARATOR", "PIPELINE", "STORAGE TANK" };

        /// <summary>Seeds one FACILITY per seeded field. Returns count inserted.</summary>
        private async Task<int> GenerateFacilitiesAsync(string userId)
        {
            var repo  = MakeRepo<FACILITY>();
            int count = 0;

            for (int i = 0; i < SeededFieldIds.Count; i++)
            {
                var fieldId    = SeededFieldIds[i];
                var facilityId = $"DEMO_FAC_{i + 1:D3}";
                var typeIndex  = i % FacilityTypes.Length;

                var facility = new FACILITY
                {
                    FACILITY_ID         = facilityId,
                    FACILITY_TYPE       = FacilityTypes[typeIndex],
                    FACILITY_LONG_NAME  = $"Demo {FacilityTypes[typeIndex].ToTitleCase()} {i + 1}",
                    FACILITY_SHORT_NAME = $"FAC-{i + 1:D3}",
                    PRIMARY_FIELD_ID    = fieldId,
                    DESCRIPTION         = $"Auto-generated demo facility for field {fieldId}",
                    REMARK              = "Demo data"
                };

                if (facility is IPPDMEntity ppdm)
                    _commonColumnHandler.PrepareForInsert(ppdm, userId);

                try
                {
                    await repo.InsertAsync(facility, userId);
                    count++;
                    _logger?.LogDebug("[Facilities] Seeded {FacilityId}", facilityId);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "[Facilities] Could not insert {FacilityId}", facilityId);
                }
            }

            return count;
        }
    }

    // Local extension to avoid dependency on external helpers
    internal static class StringExtensions
    {
        internal static string ToTitleCase(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var words = s.Split(' ');
            for (int i = 0; i < words.Length; i++)
                if (words[i].Length > 0)
                    words[i] = char.ToUpperInvariant(words[i][0]) + words[i][1..].ToLowerInvariant();
            return string.Join(' ', words);
        }
    }
}
