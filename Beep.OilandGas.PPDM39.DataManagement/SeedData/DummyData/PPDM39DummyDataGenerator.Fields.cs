using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData.DummyData
{
    public partial class PPDM39DummyDataGenerator
    {
        /// <summary>
        /// Seeds FIELD records. Field IDs are recorded in <see cref="SeededFieldIds"/>
        /// so subsequent partials (wells, facilities, production) can reference them.
        /// </summary>
        private async Task GenerateFieldsAsync(string userId)
        {
            var repo = MakeRepo<FIELD>();

            var basins = new[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" };
            var types  = new[] { "OIL", "GAS", "OIL/GAS" };

            for (int i = 0; i < _fieldCount; i++)
            {
                var fieldId = $"DEMO_FLD_{i + 1:D3}";
                var name    = $"Demo {basins[i % basins.Length]} Field";

                var field = new FIELD
                {
                    FIELD_ID   = fieldId,
                    FIELD_NAME = name,
                    REMARK     = $"Auto-generated demo field — {types[i % types.Length]} producing"
                };

                if (field is IPPDMEntity ppdm)
                    _commonColumnHandler.PrepareForInsert(ppdm, userId);

                try
                {
                    await repo.InsertAsync(field, userId);
                    SeededFieldIds.Add(fieldId);
                    _logger?.LogDebug("[Fields] Seeded {FieldId}", fieldId);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "[Fields] Could not insert {FieldId} — may already exist, skipping", fieldId);
                }
            }
        }
    }
}
