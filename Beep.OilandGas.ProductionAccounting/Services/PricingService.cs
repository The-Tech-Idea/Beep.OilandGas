using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Pricing Service - Manages product pricing and revenue calculation.
    /// Provides price indices and calculates revenue from volumes and prices.
    /// </summary>
    public class PricingService : IPricingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PricingService> _logger;
        private const string ConnectionName = "PPDM39";

        public PricingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PricingService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Gets the price for a product on a specific date.
        /// </summary>
        public async Task<decimal> GetPriceAsync(
            string productId,
            DateTime date,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentNullException(nameof(productId));

            _logger?.LogInformation("Getting price for product {ProductId} on {Date}",
                productId, date.ToShortDateString());

            var metadata = await _metadata.GetTableMetadataAsync("PRICE_INDEX");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(PRICE_INDEX);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "PRICE_INDEX");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COMMODITY_TYPE", Operator = "=", FilterValue = productId },
                new AppFilter { FieldName = "PRICE_DATE", Operator = "<=", FilterValue = date.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var prices = await repo.GetAsync(filters);
            var priceList = prices?.Cast<PRICE_INDEX>().OrderByDescending(p => p.PRICE_DATE).ToList() 
                ?? new List<PRICE_INDEX>();

            if (!priceList.Any())
                throw new AccountingException($"No pricing found for {productId} as of {date}");

            return priceList.First().PRICE_VALUE ?? 0;
        }

        /// <summary>
        /// Gets price history for a product in a date range.
        /// </summary>
        public async Task<List<PRICE_INDEX>> GetHistoryAsync(
            string productId,
            DateTime start,
            DateTime end,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentNullException(nameof(productId));
            if (start > end)
                throw new ArgumentException("start must be <= end", nameof(start));

            var metadata = await _metadata.GetTableMetadataAsync("PRICE_INDEX");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(PRICE_INDEX);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "PRICE_INDEX");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COMMODITY_TYPE", Operator = "=", FilterValue = productId },
                new AppFilter { FieldName = "PRICE_DATE", Operator = ">=", FilterValue = start.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "PRICE_DATE", Operator = "<=", FilterValue = end.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var prices = await repo.GetAsync(filters);
            return prices?.Cast<PRICE_INDEX>().OrderByDescending(p => p.PRICE_DATE).ToList() 
                ?? new List<PRICE_INDEX>();
        }

        /// <summary>
        /// Calculates revenue: volume × price.
        /// </summary>
        public async Task<decimal> CalculateRevenueAsync(
            string productId,
            decimal volume,
            DateTime date,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentNullException(nameof(productId));
            if (volume <= 0)
                throw new AccountingException($"Volume must be positive: {volume}");

            var price = await GetPriceAsync(productId, date, cn);
            var revenue = volume * price;

            _logger?.LogInformation("Revenue calculated for {ProductId}: Volume={Volume} × Price={Price} = {Revenue}",
                productId, volume, price, revenue);

            return revenue;
        }

        /// <summary>
        /// Gets average price for a product over a date range.
        /// </summary>
        public async Task<decimal> GetAveragePriceAsync(
            string productId,
            DateTime start,
            DateTime end,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentNullException(nameof(productId));

            var history = await GetHistoryAsync(productId, start, end, cn);
            if (!history.Any())
                return 0;

            var average = history.Average(p => p.PRICE_VALUE ?? 0);

            _logger?.LogInformation("Average price for {ProductId}: {AveragePrice}",
                productId, average);

            return average;
        }
    }
}
