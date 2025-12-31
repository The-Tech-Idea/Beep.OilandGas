using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.GasLift.Calculations;
using Beep.OilandGas.Models.GasLift;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.GasLift.Services
{
    /// <summary>
    /// Service for gas lift operations.
    /// Uses PPDMGenericRepository for data persistence following LifeCycle patterns.
    /// </summary>
    public class GasLiftService : IGasLiftService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<GasLiftService>? _logger;

        public GasLiftService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<GasLiftService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public GasLiftPotentialResult AnalyzeGasLiftPotential(
            GasLiftWellProperties wellProperties,
            decimal minGasInjectionRate,
            decimal maxGasInjectionRate,
            int numberOfPoints = 50)
        {
            _logger?.LogInformation("Analyzing gas lift potential for well");
            var result = GasLiftPotentialCalculator.AnalyzeGasLiftPotential(
                wellProperties, minGasInjectionRate, maxGasInjectionRate, numberOfPoints);
            _logger?.LogInformation("Gas lift potential analysis completed: OptimalGasInjectionRate={Rate}, MaximumProductionRate={Production}", 
                result.OptimalGasInjectionRate, result.MaximumProductionRate);
            return result;
        }

        public GasLiftValveDesignResult DesignValves(
            GasLiftWellProperties wellProperties,
            decimal gasInjectionPressure,
            int numberOfValves,
            bool useSIUnits = false)
        {
            _logger?.LogInformation("Designing gas lift valves: {ValveCount} valves, SIUnits={SIUnits}", numberOfValves, useSIUnits);
            
            GasLiftValveDesignResult result;
            if (useSIUnits)
            {
                result = GasLiftValveDesignCalculator.DesignValvesSI(wellProperties, gasInjectionPressure, numberOfValves);
            }
            else
            {
                result = GasLiftValveDesignCalculator.DesignValvesUS(wellProperties, gasInjectionPressure, numberOfValves);
            }

            _logger?.LogInformation("Gas lift valve design completed: {ValveCount} valves designed", result.Valves.Count);
            return result;
        }

        public async Task SaveGasLiftDesignAsync(GasLiftDesignDto design, string userId)
        {
            if (design == null)
                throw new ArgumentNullException(nameof(design));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving gas lift design {DesignId} for well {WellUWI}", design.DesignId, design.WellUWI);

            if (string.IsNullOrWhiteSpace(design.DesignId))
            {
                design.DesignId = _defaults.FormatIdForTable("GAS_LIFT_DESIGN", Guid.NewGuid().ToString());
            }

            // Create repository for GAS_LIFT_DESIGN
            var designRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GAS_LIFT_DESIGN), _connectionName, "GAS_LIFT_DESIGN", null);

            var newEntity = new GAS_LIFT_DESIGN
            {
                DESIGN_ID = design.DesignId,
                WELL_UWI = design.WellUWI ?? string.Empty,
                DESIGN_DATE = design.DesignDate,
                NUMBER_OF_VALVES = design.NumberOfValves,
                TOTAL_GAS_INJECTION_RATE = design.TotalGasInjectionRate,
                EXPECTED_PRODUCTION_RATE = design.ExpectedProductionRate,
                ACTIVE_IND = "Y"
            };

            // Prepare for insert (sets common columns)
            if (newEntity is IPPDMEntity ppdmNewEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmNewEntity, userId);
            }
            await designRepo.InsertAsync(newEntity, userId);

            _logger?.LogInformation("Successfully saved gas lift design {DesignId}", design.DesignId);
        }

        public async Task<GasLiftPerformanceDto> GetGasLiftPerformanceAsync(string wellUWI)
        {
            if (string.IsNullOrWhiteSpace(wellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(wellUWI));

            _logger?.LogInformation("Getting gas lift performance for well {WellUWI}", wellUWI);

            // Create repository for GAS_LIFT_PERFORMANCE
            var performanceRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(GAS_LIFT_PERFORMANCE), _connectionName, "GAS_LIFT_PERFORMANCE", null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_UWI", Operator = "=", FilterValue = wellUWI },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var entities = await performanceRepo.GetAsync(filters);
            var entity = entities.Cast<GAS_LIFT_PERFORMANCE>().FirstOrDefault();

            if (entity == null)
            {
                _logger?.LogWarning("No gas lift performance data found for well {WellUWI}", wellUWI);
                return new GasLiftPerformanceDto
                {
                    WellUWI = wellUWI,
                    PerformanceDate = DateTime.UtcNow
                };
            }

            var performance = new GasLiftPerformanceDto
            {
                WellUWI = entity.WELL_UWI ?? wellUWI,
                PerformanceDate = entity.PERFORMANCE_DATE ?? DateTime.UtcNow,
                GasInjectionRate = entity.GAS_INJECTION_RATE ?? 0,
                ProductionRate = entity.PRODUCTION_RATE ?? 0,
                GasLiquidRatio = entity.GAS_LIQUID_RATIO ?? 0,
                Efficiency = entity.EFFICIENCY ?? 0
            };

            _logger?.LogInformation("Retrieved gas lift performance for well {WellUWI}", wellUWI);
            return performance;
        }
    }
}
