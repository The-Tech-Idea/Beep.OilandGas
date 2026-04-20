using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData.DummyData
{
    public partial class PPDM39DummyDataGenerator
    {
        private static readonly string[] TestTypes = { "DST", "PRODUCTION", "BUILD-UP", "INJECTIVITY" };

        /// <summary>Inserts one WELL_TEST per seeded well. Returns count inserted.</summary>
        private async Task<int> GenerateWellTestsAsync(string userId)
        {
            var repo  = MakeRepo<WELL_TEST>();
            var rng   = new Random(7);
            int count = 0;

            foreach (var uwi in SeededUwis)
            {
                var testType = TestTypes[rng.Next(TestTypes.Length)];
                var test = new WELL_TEST
                {
                    UWI       = uwi,
                    SOURCE    = "DEMO",
                    TEST_TYPE = testType,
                    RUN_NUM   = "1",
                    TEST_NUM  = "1"
                };

                if (test is IPPDMEntity ppdm)
                    _commonColumnHandler.PrepareForInsert(ppdm, userId);

                try
                {
                    await repo.InsertAsync(test, userId);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "[WellTests] Could not insert test for {UWI}", uwi);
                }
            }

            _logger?.LogDebug("[WellTests] Seeded {Count} well tests", count);
            return count;
        }
    }
}
