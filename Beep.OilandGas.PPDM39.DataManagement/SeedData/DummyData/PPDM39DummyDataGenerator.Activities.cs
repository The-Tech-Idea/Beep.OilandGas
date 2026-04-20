using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData.DummyData
{
    public partial class PPDM39DummyDataGenerator
    {
        private static readonly string[] ActivityTypeIds =
            { "DRILL", "COMPLETE", "WORKOVER", "WORKOVER_PUMP", "SHUT_IN", "REOPEN" };

        /// <summary>Inserts two WELL_ACTIVITY rows per well (historical milestones). Returns count inserted.</summary>
        private async Task<int> GenerateActivitiesAsync(string userId)
        {
            var repo  = MakeRepo<WELL_ACTIVITY>();
            var rng   = new Random(13);
            int count = 0;

            foreach (var uwi in SeededUwis)
            {
                // Spud / completion activity
                for (int act = 0; act < 2; act++)
                {
                    var typeId = ActivityTypeIds[(rng.Next(ActivityTypeIds.Length / 2)) + act];
                    var activity = new WELL_ACTIVITY
                    {
                        UWI              = uwi,
                        SOURCE           = "DEMO",
                        ACTIVITY_OBS_NO  = act + 1,
                        ACTIVITY_TYPE_ID = typeId
                    };

                    if (activity is IPPDMEntity ppdm)
                        _commonColumnHandler.PrepareForInsert(ppdm, userId);

                    try
                    {
                        await repo.InsertAsync(activity, userId);
                        count++;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "[Activities] Could not insert activity for {UWI}", uwi);
                    }
                }
            }

            _logger?.LogDebug("[Activities] Seeded {Count} well activities", count);
            return count;
        }
    }
}
