using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.Models.FlashCalculations;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.FlashCalculations.Services
{
    /// <summary>
    /// Service for flash calculation operations.
    /// Uses PPDMGenericRepository for data persistence following LifeCycle patterns.
    /// </summary>
    public class FlashCalculationService : IFlashCalculationService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<FlashCalculationService>? _logger;

        public FlashCalculationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<FlashCalculationService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public FlashResult PerformIsothermalFlash(FlashConditions conditions)
        {
            _logger?.LogInformation("Performing isothermal flash calculation");
            var result = FlashCalculator.PerformIsothermalFlash(conditions);
            _logger?.LogInformation("Flash calculation completed: VaporFraction={VaporFraction}, Converged={Converged}", 
                result.VaporFraction, result.Converged);
            return result;
        }

        public List<FlashResult> PerformMultiStageFlash(FlashConditions conditions, int stages)
        {
            _logger?.LogInformation("Performing multi-stage flash calculation with {Stages} stages", stages);
            var results = new List<FlashResult>();
            
            // Perform first stage
            var firstStageResult = FlashCalculator.PerformIsothermalFlash(conditions);
            results.Add(firstStageResult);

            // For subsequent stages, use liquid from previous stage as feed
            var currentFeed = conditions;
            for (int i = 1; i < stages; i++)
            {
                // Create new conditions with liquid composition from previous stage
                var nextStageConditions = new FlashConditions
                {
                    Pressure = currentFeed.Pressure,
                    Temperature = currentFeed.Temperature,
                    FeedComposition = firstStageResult.LiquidComposition.Select(kvp => 
                        new FlashComponent
                        {
                            Name = kvp.Key,
                            MoleFraction = kvp.Value
                        }).ToList()
                };

                var stageResult = FlashCalculator.PerformIsothermalFlash(nextStageConditions);
                results.Add(stageResult);
                currentFeed = nextStageConditions;
            }

            _logger?.LogInformation("Multi-stage flash calculation completed with {Stages} stages", stages);
            return results;
        }

        public async Task SaveFlashResultAsync(FlashResult result, string userId)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving flash calculation result");

            // Create repository for FLASH_CALCULATION_RESULT
            var flashRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(FLASH_CALCULATION_RESULT), _connectionName, "FLASH_CALCULATION_RESULT", null);

            var calculationId = _defaults.FormatIdForTable("FLASH_CALCULATION", Guid.NewGuid().ToString());
            
            var newEntity = new FLASH_CALCULATION_RESULT
            {
                CALCULATION_ID = calculationId,
                CALCULATION_DATE = DateTime.UtcNow,
                VAPOR_FRACTION = result.VaporFraction,
                LIQUID_FRACTION = result.LiquidFraction,
                ITERATIONS = result.Iterations,
                CONVERGED = result.Converged ? "Y" : "N",
                CONVERGENCE_ERROR = result.ConvergenceError,
                ACTIVE_IND = "Y"
            };

            // Prepare for insert (sets common columns)
            if (newEntity is IPPDMEntity ppdmNewEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmNewEntity, userId);
            }
            await flashRepo.InsertAsync(newEntity, userId);

            _logger?.LogInformation("Successfully saved flash calculation result {CalculationId}", calculationId);
        }

        public async Task<List<FlashResult>> GetFlashHistoryAsync(string? componentId = null)
        {
            _logger?.LogInformation("Getting flash calculation history for component: {ComponentId}", componentId ?? "all");

            // Create repository for FLASH_CALCULATION_RESULT
            var flashRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(FLASH_CALCULATION_RESULT), _connectionName, "FLASH_CALCULATION_RESULT", null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            if (!string.IsNullOrWhiteSpace(componentId))
            {
                filters.Add(new AppFilter { FieldName = "COMPONENT_ID", Operator = "=", FilterValue = componentId });
            }

            var entities = await flashRepo.GetAsync(filters);
            var results = entities.Cast<FLASH_CALCULATION_RESULT>().Select(entity => new FlashResult
            {
                VaporFraction = entity.VAPOR_FRACTION ?? 0,
                LiquidFraction = entity.LIQUID_FRACTION ?? 0,
                Iterations = entity.ITERATIONS ?? 0,
                Converged = entity.CONVERGED == "Y",
                ConvergenceError = entity.CONVERGENCE_ERROR ?? 0,
                VaporComposition = new Dictionary<string, decimal>(),
                LiquidComposition = new Dictionary<string, decimal>(),
                KValues = new Dictionary<string, decimal>()
            }).ToList();

            _logger?.LogInformation("Retrieved {Count} flash calculation results", results.Count);
            return results;
        }
    }
}
