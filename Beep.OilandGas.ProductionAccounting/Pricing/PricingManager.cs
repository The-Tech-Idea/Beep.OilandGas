using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Pricing
{
    /// <summary>
    /// Manages pricing operations.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
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
            ILoggerFactory loggerFactory,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = loggerFactory?.CreateLogger<PricingManager>();
            _connectionName = connectionName ?? "PPDM39";

            _indexManager = new PriceIndexManager(editor, commonColumnHandler, defaults, metadata, loggerFactory, connectionName);
            _indexManager.InitializeStandardIndexes();
            _regulatedPricingManager = new RegulatedPricingManager(editor, commonColumnHandler, defaults, metadata, loggerFactory, connectionName);
        }

        /// <summary>
        /// Values a run ticket.
        /// </summary>
        public async Task<RunTicketValuation> ValueRunTicketAsync(
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

            RunTicketValuation valuation;

            switch (method)
            {
                case PricingMethod.Fixed:
                    if (!fixedPrice.HasValue)
                        throw new ArgumentException("Fixed price is required for fixed pricing method.", nameof(fixedPrice));
                    valuation = RunTicketValuationEngine.ValueWithFixedPrice(runTicket, fixedPrice.Value);
                    break;

                case PricingMethod.IndexBased:
                    if (string.IsNullOrEmpty(indexName))
                        throw new ArgumentException("Index name is required for index-based pricing.", nameof(indexName));
                    var index = await _indexManager.GetLatestPriceAsync(indexName, connectionName);
                    if (index == null)
                        throw new InvalidOperationException($"Price index {indexName} not found.");
                    valuation = RunTicketValuationEngine.ValueWithIndex(
                        runTicket, 
                        index, 
                        differential ?? 0m);
                    break;

                case PricingMethod.PostedPrice:
                    if (!fixedPrice.HasValue)
                        throw new ArgumentException("Posted price is required for posted price method.", nameof(fixedPrice));
                    valuation = RunTicketValuationEngine.ValueWithPostedPrice(
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
                    valuation = RunTicketValuationEngine.ValueWithRegulatedPrice(runTicket, regulatedPrice);
                    break;

                default:
                    throw new ArgumentException($"Unsupported pricing method: {method}", nameof(method));
            }

            // Save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var valuationData = ConvertValuationToDictionary(valuation);
            var result = dataSource.InsertEntity(RUN_TICKET_VALUATION_TABLE, valuationData);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to save valuation {ValuationId}: {Error}", valuation.ValuationId, errorMessage);
                throw new InvalidOperationException($"Failed to save valuation: {errorMessage}");
            }

            _logger?.LogDebug("Created valuation {ValuationId} for run ticket {TicketNumber} in database", valuation.ValuationId, runTicket.RunTicketNumber);
            return valuation;
        }

        /// <summary>
        /// Values a run ticket (synchronous wrapper).
        /// </summary>
        public RunTicketValuation ValueRunTicket(
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
        public async Task<RunTicketValuation?> GetValuationAsync(string valuationId, string? connectionName = null)
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
            var valuationData = results?.OfType<Dictionary<string, object>>().FirstOrDefault();
            
            if (valuationData == null)
                return null;

            return ConvertDictionaryToValuation(valuationData);
        }

        /// <summary>
        /// Gets a valuation by ID (synchronous wrapper).
        /// </summary>
        public RunTicketValuation? GetValuation(string valuationId)
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

        #region Helper Methods - Model to Dictionary Conversion

        /// <summary>
        /// Converts RunTicketValuation to dictionary for database storage.
        /// </summary>
        private Dictionary<string, object> ConvertValuationToDictionary(RunTicketValuation valuation)
        {
            return new Dictionary<string, object>
            {
                { "VALUATION_ID", valuation.ValuationId },
                { "RUN_TICKET_NUMBER", valuation.RunTicketNumber },
                { "VALUATION_DATE", valuation.ValuationDate },
                { "BASE_PRICE", valuation.BasePrice },
                { "ADJUSTED_PRICE", valuation.AdjustedPrice },
                { "NET_VOLUME", valuation.NetVolume },
                { "TOTAL_VALUE", valuation.TotalValue },
                { "PRICING_METHOD", valuation.PricingMethod.ToString() },
                { "API_GRAVITY_ADJUSTMENT", valuation.QualityAdjustments?.ApiGravityAdjustment ?? 0m },
                { "SULFUR_ADJUSTMENT", valuation.QualityAdjustments?.SulfurAdjustment ?? 0m },
                { "BSW_ADJUSTMENT", valuation.QualityAdjustments?.BSWAdjustment ?? 0m },
                { "OTHER_QUALITY_ADJUSTMENTS", valuation.QualityAdjustments?.OtherAdjustments ?? 0m },
                { "LOCATION_DIFFERENTIAL", valuation.LocationAdjustments?.LocationDifferential ?? 0m },
                { "TRANSPORTATION_ADJUSTMENT", valuation.LocationAdjustments?.TransportationAdjustment ?? 0m },
                { "TIME_DIFFERENTIAL", valuation.TimeAdjustments?.TimeDifferential ?? 0m },
                { "INTEREST_ADJUSTMENT", valuation.TimeAdjustments?.InterestAdjustment ?? 0m }
            };
        }

        /// <summary>
        /// Converts dictionary to RunTicketValuation.
        /// </summary>
        private RunTicketValuation? ConvertDictionaryToValuation(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("VALUATION_ID"))
                return null;

            var valuation = new RunTicketValuation
            {
                ValuationId = dict["VALUATION_ID"]?.ToString() ?? string.Empty,
                RunTicketNumber = dict.ContainsKey("RUN_TICKET_NUMBER") ? dict["RUN_TICKET_NUMBER"]?.ToString() ?? string.Empty : string.Empty,
                ValuationDate = dict.ContainsKey("VALUATION_DATE") && dict["VALUATION_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["VALUATION_DATE"])
                    : DateTime.MinValue,
                BasePrice = dict.ContainsKey("BASE_PRICE") ? Convert.ToDecimal(dict["BASE_PRICE"]) : 0m,
                NetVolume = dict.ContainsKey("NET_VOLUME") ? Convert.ToDecimal(dict["NET_VOLUME"]) : 0m
            };

            if (dict.ContainsKey("PRICING_METHOD") && Enum.TryParse<PricingMethod>(dict["PRICING_METHOD"]?.ToString(), out var method))
                valuation.PricingMethod = method;

            valuation.QualityAdjustments = new QualityAdjustments
            {
                ApiGravityAdjustment = dict.ContainsKey("API_GRAVITY_ADJUSTMENT") ? Convert.ToDecimal(dict["API_GRAVITY_ADJUSTMENT"]) : 0m,
                SulfurAdjustment = dict.ContainsKey("SULFUR_ADJUSTMENT") ? Convert.ToDecimal(dict["SULFUR_ADJUSTMENT"]) : 0m,
                BSWAdjustment = dict.ContainsKey("BSW_ADJUSTMENT") ? Convert.ToDecimal(dict["BSW_ADJUSTMENT"]) : 0m,
                OtherAdjustments = dict.ContainsKey("OTHER_QUALITY_ADJUSTMENTS") ? Convert.ToDecimal(dict["OTHER_QUALITY_ADJUSTMENTS"]) : 0m
            };

            valuation.LocationAdjustments = new LocationAdjustments
            {
                LocationDifferential = dict.ContainsKey("LOCATION_DIFFERENTIAL") ? Convert.ToDecimal(dict["LOCATION_DIFFERENTIAL"]) : 0m,
                TransportationAdjustment = dict.ContainsKey("TRANSPORTATION_ADJUSTMENT") ? Convert.ToDecimal(dict["TRANSPORTATION_ADJUSTMENT"]) : 0m
            };

            valuation.TimeAdjustments = new TimeAdjustments
            {
                TimeDifferential = dict.ContainsKey("TIME_DIFFERENTIAL") ? Convert.ToDecimal(dict["TIME_DIFFERENTIAL"]) : 0m,
                InterestAdjustment = dict.ContainsKey("INTEREST_ADJUSTMENT") ? Convert.ToDecimal(dict["INTEREST_ADJUSTMENT"]) : 0m
            };

            return valuation;
        }

        #endregion
    }
}

