
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.ProductionAccounting;
using System.Text.Json;

namespace Beep.OilandGas.ProductionAccounting.Pricing
{
    /// <summary>
    /// Manages pricing operations.
    /// Uses Entity classes directly with IDataSource - no dictionary conversions.
    /// </summary>
    public class PricingManager
    {
        private readonly PriceIndexManager _indexManager;
        private readonly RegulatedPricingManager _regulatedPricingManager;
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PricingManager>? _logger;
        private readonly string _connectionName;
        private const string RUN_TICKET_VALUATION_TABLE = "RUN_TICKET_VALUATION";

        public PricingManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PricingManager>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";

            _indexManager = new PriceIndexManager(editor, commonColumnHandler, defaults, metadata, null, connectionName);
            _indexManager.InitializeStandardIndexes();
            _regulatedPricingManager = new RegulatedPricingManager(editor, commonColumnHandler, defaults, metadata, null, connectionName);
        }

        /// <summary>
        /// Values a run ticket.
        /// </summary>
        public async Task<RUN_TICKET_VALUATION> ValueRunTicketAsync(
            RunTicket runTicket,
            PricingMethod method,
            decimal? fixedPrice = null,
            string? indexName = null,
            decimal? differential = null,
            string? regulatoryAuthority = null,
            string userId = "system",
            string? connectionName = null)
        {
            if (runTicket == null)
                throw new ArgumentNullException(nameof(runTicket));

            RUN_TICKET_VALUATION valuation;

            switch (method)
            {
                case PricingMethod.Fixed:
                    if (!fixedPrice.HasValue)
                        throw new ArgumentException("Fixed price is required for fixed pricing method.", nameof(fixedPrice));
                    valuation = RUN_TICKET_VALUATIONEngine.ValueWithFixedPrice(runTicket, fixedPrice.Value);
                    break;

                case PricingMethod.IndexBased:
                    if (string.IsNullOrEmpty(indexName))
                        throw new ArgumentException("Index name is required for index-based pricing.", nameof(indexName));
                    var index = await _indexManager.GetLatestPriceAsync(indexName, connectionName);
                    if (index == null)
                        throw new InvalidOperationException($"Price index {indexName} not found.");
                    valuation = RUN_TICKET_VALUATIONEngine.ValueWithIndex(
                        runTicket, 
                        index, 
                        differential ?? 0m);
                    break;

                case PricingMethod.PostedPrice:
                    if (!fixedPrice.HasValue)
                        throw new ArgumentException("Posted price is required for posted price method.", nameof(fixedPrice));
                    valuation = RUN_TICKET_VALUATIONEngine.ValueWithPostedPrice(
                        runTicket, 
                        fixedPrice.Value, 
                        differential ?? 0m);
                    break;

                case PricingMethod.Regulated:
                    if (string.IsNullOrEmpty(regulatoryAuthority))
                        throw new ArgumentException("Regulatory authority is required for regulated pricing.", nameof(regulatoryAuthority));
                    var regulatedPrice = _regulatedPricingManager.GetApplicablePrice(
                        regulatoryAuthority, 
                        runTicket.TicketDateTime);
                    if (regulatedPrice == null)
                        throw new InvalidOperationException($"No regulated price found for {regulatoryAuthority} on {runTicket.TicketDateTime}.");
                    valuation = RUN_TICKET_VALUATIONEngine.ValueWithRegulatedPrice(runTicket, regulatedPrice);
                    break;

                default:
                    throw new ArgumentException($"Unsupported pricing method: {method}", nameof(method));
            }

            // Convert RUN_TICKET_VALUATION model to RUN_TICKET_VALUATION Entity
            var valuationEntity = new RUN_TICKET_VALUATION
            {
                VALUATION_ID = valuation.ValuationId,
                RUN_TICKET_NUMBER = valuation.RunTicketNumber,
                VALUATION_DATE = valuation.ValuationDate,
                BASE_PRICE = valuation.BasePrice,
                ADJUSTED_PRICE = valuation.AdjustedPrice,
                NET_VOLUME = valuation.NetVolume,
                TOTAL_VALUE = valuation.TotalValue,
                PRICING_METHOD = valuation.PricingMethod.ToString(),
                QUALITY_ADJUSTMENTS_JSON = valuation.QualityAdjustments != null ? JsonSerializer.Serialize(valuation.QualityAdjustments) : null,
                LOCATION_ADJUSTMENTS_JSON = valuation.LocationAdjustments != null ? JsonSerializer.Serialize(valuation.LocationAdjustments) : null,
                TIME_ADJUSTMENTS_JSON = valuation.TimeAdjustments != null ? JsonSerializer.Serialize(valuation.TimeAdjustments) : null,
                TOTAL_ADJUSTMENTS = valuation.TotalAdjustments
            };

            // Prepare for insert and save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(valuationEntity, userId);
            var result = dataSource.InsertEntity(RUN_TICKET_VALUATION_TABLE, valuationEntity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to save valuation {ValuationId}: {Error}", valuationEntity.VALUATION_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save valuation: {errorMessage}");
            }

            _logger?.LogDebug("Created valuation {ValuationId} for run ticket {TicketNumber} in database", valuationEntity.VALUATION_ID, valuation.RunTicketNumber);
            return valuationEntity;
        }

        /// <summary>
        /// Values a run ticket (synchronous wrapper).
        /// </summary>
        public RUN_TICKET_VALUATION ValueRunTicket(
            RunTicket runTicket,
            PricingMethod method,
            decimal? fixedPrice = null,
            string? indexName = null,
            decimal? differential = null,
            string? regulatoryAuthority = null)
        {
            return ValueRunTicketAsync(runTicket, method, fixedPrice, indexName, differential, regulatoryAuthority).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a valuation by ID.
        /// </summary>
        public async Task<RUN_TICKET_VALUATION?> GetValuationAsync(string valuationId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(valuationId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "VALUATION_ID", Operator = "=", FilterValue = valuationId }
            };

            var results = await dataSource.GetEntityAsync(RUN_TICKET_VALUATION_TABLE, filters);
            return results?.FirstOrDefault() as RUN_TICKET_VALUATION;
        }

        /// <summary>
        /// Gets a valuation by ID (synchronous wrapper).
        /// </summary>
        public RUN_TICKET_VALUATION? GetValuation(string valuationId)
        {
            return GetValuationAsync(valuationId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets the price index manager.
        /// </summary>
        public PriceIndexManager GetIndexManager() => _indexManager;

        /// <summary>
        /// Gets the regulated pricing manager.
        /// </summary>
        public RegulatedPricingManager GetRegulatedPricingManager() => _regulatedPricingManager;
    }
}
