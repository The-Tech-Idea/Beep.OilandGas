using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Pricing;
using Beep.OilandGas.Models.DTOs.Pricing;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using CrudeOilPropertiesDto = Beep.OilandGas.Models.DTOs.ProductionAccounting.CrudeOilPropertiesDto;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionAccounting.Pricing
{
    /// <summary>
    /// Service for managing pricing operations.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class PricingService : IPricingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PricingService>? _logger;
        private readonly string _connectionName;
        private readonly PriceIndexManager _indexManager;
        private readonly RegulatedPricingManager _regulatedPricingManager;
        private const string RUN_TICKET_VALUATION_TABLE = "RUN_TICKET_VALUATION";
        private const string PRICE_INDEX_TABLE = "PRICE_INDEX";
        private const string REGULATED_PRICE_TABLE = "REGULATED_PRICE";
        private const string RUN_TICKET_TABLE = "RUN_TICKET";

        public PricingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PricingService>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";
            _indexManager = new PriceIndexManager(editor, commonColumnHandler, defaults, metadata, null, connectionName);
            _regulatedPricingManager = new RegulatedPricingManager(editor, commonColumnHandler, defaults, metadata, null, connectionName);
        }

        /// <summary>
        /// Values a run ticket.
        /// </summary>
        public async Task<RUN_TICKET_VALUATION> ValueRunTicketAsync(
            Beep.OilandGas.Models.DTOs.Pricing.ValueRunTicketRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;

            // Get run ticket entity
            var runTicketRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RUN_TICKET), connName, RUN_TICKET_TABLE, null);

            var runTicketFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "RUN_TICKET_NUMBER", Operator = "=", FilterValue = request.RunTicketNumber }
            };
            var runTicketResults = await runTicketRepo.GetAsync(runTicketFilters);
            var runTicketEntity = runTicketResults.Cast<RUN_TICKET>().FirstOrDefault();

            if (runTicketEntity == null)
                throw new InvalidOperationException($"Run ticket {request.RunTicketNumber} not found.");

            // Convert to RunTicket model for valuation engine
            var runTicket = ConvertToRunTicketModel(runTicketEntity);

            // Calculate valuation using existing engine
            RUN_TICKET_VALUATION valuation;
            PricingMethod method = Enum.TryParse<PricingMethod>(request.PricingMethod, out var parsedMethod) ? parsedMethod : PricingMethod.Fixed;

            switch (method)
            {
                case PricingMethod.Fixed:
                    if (!request.FixedPrice.HasValue)
                        throw new ArgumentException("Fixed price is required for fixed pricing method.", nameof(request));

                    valuation = RUN_TICKET_VALUATIONEngine.ValueWithFixedPrice(runTicket, request.FixedPrice.Value);
                    break;

                case PricingMethod.IndexBased:
                    if (string.IsNullOrEmpty(request.IndexName))
                        throw new ArgumentException("Index name is required for index-based pricing.", nameof(request));

                    var indexModel = await _indexManager.GetLatestPriceAsync(request.IndexName, connName);
                    if (indexModel == null)
                        throw new InvalidOperationException($"Price index {request.IndexName} not found.");

                    valuation = RUN_TICKET_VALUATIONEngine.ValueWithIndex(runTicket, indexModel, request.Differential ?? 0m);
                    break;

                case PricingMethod.PostedPrice:
                    if (!request.FixedPrice.HasValue)
                        throw new ArgumentException("Posted price is required for posted price method.", nameof(request));

                    valuation = RUN_TICKET_VALUATIONEngine.ValueWithPostedPrice(runTicket, request.FixedPrice.Value, request.Differential ?? 0m);
                    break;

                case PricingMethod.Regulated:
                    if (string.IsNullOrEmpty(request.RegulatoryAuthority))
                        throw new ArgumentException("Regulatory authority is required for regulated pricing.", nameof(request));

                    var regulatedPrice = _regulatedPricingManager.GetApplicablePrice(request.RegulatoryAuthority, runTicket.TICKET_DATE_TIME);
                    if (regulatedPrice == null)
                        throw new InvalidOperationException($"No regulated price found for {request.RegulatoryAuthority} on {runTicket.TICKET_DATE_TIME}.");

                    valuation = RUN_TICKET_VALUATIONEngine.ValueWithRegulatedPrice(runTicket, regulatedPrice);
                    break;

                default:
                    throw new ArgumentException($"Unsupported pricing method: {method}", nameof(request));
            }

            // Convert to entity and save
            var valuationEntity = ConvertToValuationEntity(valuation);

            if (valuationEntity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RUN_TICKET_VALUATION), connName, RUN_TICKET_VALUATION_TABLE, null);

            await repo.InsertAsync(valuationEntity);

            _logger?.LogDebug("Created valuation {ValuationId} for run ticket {TicketNumber}",
                valuation.ValuationId, request.RunTicketNumber);

            return valuationEntity;
        }

        /// <summary>
        /// Gets a valuation by ID.
        /// </summary>
        public async Task<RUN_TICKET_VALUATION?> GetValuationAsync(string valuationId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(valuationId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RUN_TICKET_VALUATION), connName, RUN_TICKET_VALUATION_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "VALUATION_ID", Operator = "=", FilterValue = valuationId }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<RUN_TICKET_VALUATION>().FirstOrDefault();
        }

        /// <summary>
        /// Gets valuations by run ticket.
        /// </summary>
        public async Task<List<RUN_TICKET_VALUATION>> GetValuationsByRunTicketAsync(
            string runTicketNumber,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(runTicketNumber))
                return new List<RUN_TICKET_VALUATION>();

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RUN_TICKET_VALUATION), connName, RUN_TICKET_VALUATION_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "RUN_TICKET_NUMBER", Operator = "=", FilterValue = runTicketNumber },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<RUN_TICKET_VALUATION>().OrderByDescending(v => v.VALUATION_DATE).ToList();
        }

        /// <summary>
        /// Creates a price index.
        /// </summary>
        public async Task<PRICE_INDEX> CreatePriceIndexAsync(
            CreatePriceIndexRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PRICE_INDEX), connName, PRICE_INDEX_TABLE, null);

            var index = new PRICE_INDEX
            {
                PRICE_INDEX_ID = Guid.NewGuid().ToString(),
                INDEX_NAME = request.IndexName,
                COMMODITY_TYPE = request.CommodityType,
                PRICE_DATE = request.PriceDate,
                PRICE_VALUE = request.PriceValue,
                CURRENCY_CODE = request.CurrencyCode,
                PRICING_POINT = request.PricingPoint,
                UNIT = request.Unit ?? "USD/Barrel",
                SOURCE = request.Source,
                ACTIVE_IND = "Y"
            };

            if (index is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(index);

            _logger?.LogDebug("Created price index {IndexName} for date {Date}", request.IndexName, request.PriceDate);
            return index;
        }

        /// <summary>
        /// Gets the latest price for an index.
        /// </summary>
        public async Task<PRICE_INDEX?> GetLatestPriceAsync(string indexName, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(indexName))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PRICE_INDEX), connName, PRICE_INDEX_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INDEX_NAME", Operator = "=", FilterValue = indexName },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<PRICE_INDEX>().OrderByDescending(i => i.PRICE_DATE).FirstOrDefault();
        }

        /// <summary>
        /// Gets price history for an index.
        /// </summary>
        public async Task<List<PRICE_INDEX>> GetPriceHistoryAsync(
            string indexName,
            DateTime? startDate,
            DateTime? endDate,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(indexName))
                return new List<PRICE_INDEX>();

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PRICE_INDEX), connName, PRICE_INDEX_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INDEX_NAME", Operator = "=", FilterValue = indexName },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (startDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "PRICE_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
            }

            if (endDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "PRICE_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });
            }

            var results = await repo.GetAsync(filters);
            return results.Cast<PRICE_INDEX>().OrderByDescending(i => i.PRICE_DATE).ToList();
        }

        /// <summary>
        /// Reconciles pricing.
        /// </summary>
        public async Task<PricingReconciliationResult> ReconcilePricingAsync(
            PricingReconciliationRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var valuations = await GetValuationsByRunTicketAsync(request.RunTicketNumber, connName);

            var periodValuations = valuations.Where(v =>
                v.VALUATION_DATE >= request.PeriodStart &&
                v.VALUATION_DATE <= request.PeriodEnd).ToList();

            var expectedValue = periodValuations.Sum(v => v.TOTAL_VALUE ?? 0m);

            // In a full implementation, actualValue would come from sales/invoices
            var actualValue = expectedValue; // Placeholder
            var variance = actualValue - expectedValue;

            return new PricingReconciliationResult
            {
                ReconciliationId = Guid.NewGuid().ToString(),
                RunTicketNumber = request.RunTicketNumber,
                ExpectedValue = expectedValue,
                ActualValue = actualValue,
                Variance = variance,
                IsReconciled = Math.Abs(variance) < 0.01m,
                ReconciliationDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Gets pricing approvals.
        /// </summary>
        public async Task<List<PricingApproval>> GetPricingApprovalsAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RUN_TICKET_VALUATION), connName, RUN_TICKET_VALUATION_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var results = await repo.GetAsync(filters);
            var valuations = results.Cast<RUN_TICKET_VALUATION>().ToList();

            return valuations.Select(v => new PricingApproval
            {
                ValuationId = v.VALUATION_ID ?? string.Empty,
                RunTicketNumber = v.RUN_TICKET_NUMBER ?? string.Empty,
                TotalValue = v.TOTAL_VALUE ?? 0m,
                PricingMethod = v.PRICING_METHOD ?? string.Empty,
                Status = "Pending", // Would be stored in entity in full implementation
                ValuationDate = v.VALUATION_DATE ?? DateTime.MinValue
            }).ToList();
        }

        /// <summary>
        /// Approves pricing.
        /// </summary>
        public async Task<PricingApprovalResult> ApprovePricingAsync(
            string valuationId,
            string approverId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(valuationId))
                throw new ArgumentException("Valuation ID is required.", nameof(valuationId));

            var connName = connectionName ?? _connectionName;
            var valuation = await GetValuationAsync(valuationId, connName);

            if (valuation == null)
                throw new InvalidOperationException($"Valuation {valuationId} not found.");

            // In a full implementation, would update approval status in entity
            // For now, just return approval result
            return new PricingApprovalResult
            {
                ValuationId = valuationId,
                IsApproved = true,
                ApproverId = approverId,
                ApprovalDate = DateTime.UtcNow,
                Status = "Approved"
            };
        }

        // Helper methods for conversion
     

     
    }
}
