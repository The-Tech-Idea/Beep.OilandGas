using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.EconomicAnalysis;
using Beep.OilandGas.EconomicAnalysis.Calculations;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis.Calculations;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.WellTestAnalysis;
using Beep.OilandGas.FlashCalculations;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.ChokeAnalysis;
using Beep.OilandGas.ChokeAnalysis.Calculations;
using Beep.OilandGas.GasLift;
using Beep.OilandGas.GasLift.Calculations;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.PumpPerformance;
using Beep.OilandGas.PumpPerformance.Calculations;
using Beep.OilandGas.SuckerRodPumping;
using Beep.OilandGas.SuckerRodPumping.Calculations;
using Beep.OilandGas.CompressorAnalysis;
using Beep.OilandGas.CompressorAnalysis.Calculations;
using Beep.OilandGas.PipelineAnalysis;
using Beep.OilandGas.PipelineAnalysis.Calculations;
using Beep.OilandGas.PlungerLift;
using Beep.OilandGas.PlungerLift.Calculations;
using Beep.OilandGas.HydraulicPumps;
using Beep.OilandGas.HydraulicPumps.Calculations;
using Beep.OilandGas.LifeCycle.Services.DataMapping;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.Models.Data.Pumps;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Beep.OilandGas.Models.Data.HydraulicPumps;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.Models.Data.Calculations;
using EconomicAnalysisResult = Beep.OilandGas.Models.Data.Calculations.EconomicAnalysisResult;
using Beep.OilandGas.ProductionForecasting.Calculations;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    /// <summary>
    /// Service for performing PPDM-based calculations (DCA, Economic, Nodal, Well Test, Flash, etc.)
    /// Refactored into partial classes for maintainability.
    /// </summary>
    public partial class PPDMCalculationService : ICalculationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IFieldMappingService _fieldMappingService;
        private readonly string _connectionName;
        private readonly ILogger<PPDMCalculationService>? _logger;

        public PPDMCalculationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            IFieldMappingService fieldMappingService,
            string connectionName = "PPDM39",
            ILogger<PPDMCalculationService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _fieldMappingService = fieldMappingService ?? throw new ArgumentNullException(nameof(fieldMappingService));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        // The following partial class files contain the method implementations:
        // - PPDMCalculationService.DCA.cs
        // - PPDMCalculationService.Economics.cs
        // - PPDMCalculationService.Nodal.cs
        // - PPDMCalculationService.WellTest.cs
        // - PPDMCalculationService.Flash.cs
        // - PPDMCalculationService.ArtificialLift.cs
        // - PPDMCalculationService.Facilities.cs
        // - PPDMCalculationService.Results.cs
        // - PPDMCalculationService.Common.cs
        // - PPDMCalculationService.DataRetrieval.cs
        // - PPDMCalculationService.CRUD.cs
        // - PPDMCalculationService.PropertyHelpers.*
    }
}
