using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Pricing
{
    /// <summary>
    /// Manages regulated pricing.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class RegulatedPricingManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<RegulatedPricingManager>? _logger;
        private readonly string _connectionName;
        private const string REGULATED_PRICE_TABLE = "REGULATED_PRICE";

        public RegulatedPricingManager(
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
            _logger = loggerFactory?.CreateLogger<RegulatedPricingManager>();
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Registers a regulated price.
        /// </summary>
        public void RegisterRegulatedPrice(RegulatedPrice regulatedPrice, string? connectionName = null)
        {
            if (regulatedPrice == null)
                throw new ArgumentNullException(nameof(regulatedPrice));

            if (string.IsNullOrEmpty(regulatedPrice.RegulatoryAuthority))
                throw new ArgumentException("Regulatory authority cannot be null or empty.", nameof(regulatedPrice));

            if (string.IsNullOrEmpty(regulatedPrice.RegulatedPriceId))
                regulatedPrice.RegulatedPriceId = Guid.NewGuid().ToString();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var entity = ConvertRegulatedPriceToEntity(regulatedPrice);
            if (entity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, "SYSTEM");
            var result = dataSource.InsertEntity(REGULATED_PRICE_TABLE, entity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to register regulated price {RegulatedPriceId}: {Error}", regulatedPrice.RegulatedPriceId, errorMessage);
                throw new InvalidOperationException($"Failed to save regulated price: {errorMessage}");
            }

            _logger?.LogDebug("Registered regulated price {RegulatedPriceId} for authority {Authority} in database", 
                regulatedPrice.RegulatedPriceId, regulatedPrice.RegulatoryAuthority);
        }

        /// <summary>
        /// Gets the applicable regulated price for a date.
        /// </summary>
        public RegulatedPrice? GetApplicablePrice(string regulatoryAuthority, DateTime date, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(regulatoryAuthority))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "REGULATORY_AUTHORITY", Operator = "=", FilterValue = regulatoryAuthority },
                new AppFilter { FieldName = "EFFECTIVE_START_DATE", Operator = "<=", FilterValue = date.ToString("yyyy-MM-dd") }
            };

            var results = dataSource.GetEntityAsync(REGULATED_PRICE_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return null;

            var prices = results
                .OfType<REGULATED_PRICE>()
                .Select(ConvertEntityToRegulatedPrice)
                .Where(p => p != null && 
                           p.EffectiveStartDate <= date &&
                           (p.EffectiveEndDate == null || p.EffectiveEndDate >= date))
                .OrderByDescending(p => p.EffectiveStartDate)
                .ToList();

            return prices.FirstOrDefault();
        }

        /// <summary>
        /// Calculates price using regulated pricing formula.
        /// </summary>
        public decimal CalculateRegulatedPrice(
            RegulatedPrice regulatedPrice,
            Dictionary<string, decimal> variables)
        {
            if (regulatedPrice == null)
                throw new ArgumentNullException(nameof(regulatedPrice));

            // Start with base price
            decimal price = regulatedPrice.BasePrice;

            // Apply adjustment factors
            foreach (var factor in regulatedPrice.AdjustmentFactors)
            {
                if (variables.ContainsKey(factor.Key))
                {
                    price += factor.Value * variables[factor.Key];
                }
                else
                {
                    price += factor.Value;
                }
            }

            // Apply price cap/floor
            if (regulatedPrice.PriceCap.HasValue && price > regulatedPrice.PriceCap.Value)
                price = regulatedPrice.PriceCap.Value;

            if (regulatedPrice.PriceFloor.HasValue && price < regulatedPrice.PriceFloor.Value)
                price = regulatedPrice.PriceFloor.Value;

            return price;
        }

        /// <summary>
        /// Gets all regulated prices for an authority.
        /// </summary>
        public IEnumerable<RegulatedPrice> GetRegulatedPrices(string regulatoryAuthority, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(regulatoryAuthority))
                return Enumerable.Empty<RegulatedPrice>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "REGULATORY_AUTHORITY", Operator = "=", FilterValue = regulatoryAuthority }
            };

            var results = dataSource.GetEntityAsync(REGULATED_PRICE_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return Enumerable.Empty<RegulatedPrice>();

            return results.OfType<REGULATED_PRICE>().Select(ConvertEntityToRegulatedPrice).Where(p => p != null)!;
        }

        /// <summary>
        /// Gets all regulatory authorities.
        /// </summary>
        public IEnumerable<string> GetRegulatoryAuthorities(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var results = dataSource.GetEntityAsync(REGULATED_PRICE_TABLE, null).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return Enumerable.Empty<string>();

            return results
                .OfType<REGULATED_PRICE>()
                .Where(p => !string.IsNullOrEmpty(p.REGULATORY_AUTHORITY))
                .Select(p => p.REGULATORY_AUTHORITY!)
                .Distinct();
        }

        private REGULATED_PRICE ConvertRegulatedPriceToEntity(RegulatedPrice price)
        {
            var entity = new REGULATED_PRICE
            {
                REGULATED_PRICE_ID = price.RegulatedPriceId,
                REGULATORY_AUTHORITY = price.RegulatoryAuthority,
                PRICE_FORMULA = price.PriceFormula,
                EFFECTIVE_START_DATE = price.EffectiveStartDate,
                EFFECTIVE_END_DATE = price.EffectiveEndDate,
                PRICE_CAP = price.PriceCap,
                PRICE_FLOOR = price.PriceFloor,
                BASE_PRICE = price.BasePrice
            };
            
            // Store AdjustmentFactors as JSON string
            if (price.AdjustmentFactors != null && price.AdjustmentFactors.Any())
            {
                entity.ADJUSTMENT_FACTORS = JsonSerializer.Serialize(price.AdjustmentFactors);
            }
            
            return entity;
        }

        private RegulatedPrice ConvertEntityToRegulatedPrice(REGULATED_PRICE entity)
        {
            var price = new RegulatedPrice
            {
                RegulatedPriceId = entity.REGULATED_PRICE_ID ?? string.Empty,
                RegulatoryAuthority = entity.REGULATORY_AUTHORITY ?? string.Empty,
                PriceFormula = entity.PRICE_FORMULA ?? string.Empty,
                EffectiveStartDate = entity.EFFECTIVE_START_DATE ?? DateTime.MinValue,
                EffectiveEndDate = entity.EFFECTIVE_END_DATE,
                PriceCap = entity.PRICE_CAP,
                PriceFloor = entity.PRICE_FLOOR,
                BasePrice = entity.BASE_PRICE ?? 0m
            };
            
            if (!string.IsNullOrEmpty(entity.ADJUSTMENT_FACTORS))
            {
                try
                {
                    price.AdjustmentFactors = JsonSerializer.Deserialize<Dictionary<string, decimal>>(entity.ADJUSTMENT_FACTORS) ?? new Dictionary<string, decimal>();
                }
                catch
                {
                    price.AdjustmentFactors = new Dictionary<string, decimal>();
                }
            }
            else
            {
                price.AdjustmentFactors = new Dictionary<string, decimal>();
            }
            
            return price;
        }
    }
}

