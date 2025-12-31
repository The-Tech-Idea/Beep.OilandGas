using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.EconomicAnalysis.Calculations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.EconomicAnalysis;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.EconomicAnalysis.Services
{
    /// <summary>
    /// Service for economic analysis operations.
    /// Uses PPDMGenericRepository for data persistence following LifeCycle patterns.
    /// </summary>
    public class EconomicAnalysisService : IEconomicAnalysisService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<EconomicAnalysisService>? _logger;


        public EconomicAnalysisService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<EconomicAnalysisService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public double CalculateNPV(CashFlow[] cashFlows, double discountRate)
        {
            _logger?.LogDebug("Calculating NPV for {Count} cash flows with discount rate {Rate}", 
                cashFlows?.Length ?? 0, discountRate);
            return EconomicCalculator.CalculateNPV(cashFlows, discountRate);
        }

        public double CalculateIRR(CashFlow[] cashFlows, double initialGuess = 0.1)
        {
            _logger?.LogDebug("Calculating IRR for {Count} cash flows with initial guess {Guess}", 
                cashFlows?.Length ?? 0, initialGuess);
            return EconomicCalculator.CalculateIRR(cashFlows, initialGuess);
        }

        public EconomicResult Analyze(CashFlow[] cashFlows, double discountRate, double financeRate = 0.1, double reinvestRate = 0.1)
        {
            _logger?.LogInformation("Performing comprehensive economic analysis for {Count} cash flows", 
                cashFlows?.Length ?? 0);
            var result = EconomicCalculator.Analyze(cashFlows, discountRate, financeRate, reinvestRate);
            _logger?.LogInformation("Economic analysis completed: NPV={NPV}, IRR={IRR}", result.NPV, result.IRR);
            return result;
        }

        public List<NPVProfilePoint> GenerateNPVProfile(CashFlow[] cashFlows, double minRate = 0.0, double maxRate = 1.0, int points = 50)
        {
            _logger?.LogDebug("Generating NPV profile with {Points} points from {MinRate} to {MaxRate}", 
                points, minRate, maxRate);
            return EconomicCalculator.GenerateNPVProfile(cashFlows, minRate, maxRate, points);
        }

        public async Task SaveAnalysisResultAsync(string analysisId, EconomicResult result, string userId)
        {
            if (string.IsNullOrWhiteSpace(analysisId))
                throw new ArgumentException("Analysis ID cannot be null or empty", nameof(analysisId));
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving economic analysis result {AnalysisId}", analysisId);

            // Create repository for economic analysis result
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ECONOMIC_ANALYSIS_RESULT), _connectionName, "ECONOMIC_ANALYSIS_RESULT", null);

            // Create entity
            var entity = new ECONOMIC_ANALYSIS_RESULT
            {
                ANALYSIS_ID = analysisId,
                ANALYSIS_DATE = DateTime.UtcNow,
                NPV = (decimal)result.NPV,
                IRR = (decimal)result.IRR,
                PAYBACK_PERIOD = (decimal)result.PaybackPeriod,
                DISCOUNT_RATE = (decimal)result.DiscountRate,
                ACTIVE_IND = "Y"
            };

            // Prepare for insert (sets common columns)
            if (entity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }
            await repo.InsertAsync(entity, userId);

            _logger?.LogInformation("Successfully saved economic analysis result {AnalysisId}", analysisId);
        }

        public async Task<EconomicResult?> GetAnalysisResultAsync(string analysisId)
        {
            if (string.IsNullOrWhiteSpace(analysisId))
            {
                _logger?.LogWarning("GetAnalysisResultAsync called with null or empty analysisId");
                return null;
            }

            _logger?.LogInformation("Getting economic analysis result {AnalysisId}", analysisId);

            // Create repository for economic analysis result
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ECONOMIC_ANALYSIS_RESULT), _connectionName, "ECONOMIC_ANALYSIS_RESULT", null);

            // Get entity using filters
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ANALYSIS_ID", Operator = "=", FilterValue = analysisId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var entities = await repo.GetAsync(filters);
            var entity = entities.Cast<ECONOMIC_ANALYSIS_RESULT>().FirstOrDefault();

            if (entity == null)
            {
                _logger?.LogWarning("Economic analysis result {AnalysisId} not found", analysisId);
                return null;
            }

            // Map entity to DTO
            var result = new EconomicResult
            {
                NPV = (double)(entity.NPV ?? 0),
                IRR = (double)(entity.IRR ?? 0),
                PaybackPeriod = (double)(entity.PAYBACK_PERIOD ?? 0),
                DiscountRate = (double)(entity.DISCOUNT_RATE ?? 0)
            };

            _logger?.LogInformation("Successfully retrieved economic analysis result {AnalysisId}", analysisId);
            return result;
        }
    }
}

