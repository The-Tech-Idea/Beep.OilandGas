using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Accounting.Operational.Pricing
{
    /// <summary>
    /// Manages price indexes.
    /// </summary>
    public class PriceIndexManager
    {
        private readonly Dictionary<string, List<PriceIndex>> indexes = new();

        /// <summary>
        /// Registers a price index.
        /// </summary>
        public void RegisterIndex(PriceIndex index)
        {
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            if (string.IsNullOrEmpty(index.IndexName))
                throw new ArgumentException("Index name cannot be null or empty.", nameof(index));

            if (!indexes.ContainsKey(index.IndexName))
                indexes[index.IndexName] = new List<PriceIndex>();

            indexes[index.IndexName].Add(index);
        }

        /// <summary>
        /// Gets the latest price for an index.
        /// </summary>
        public PriceIndex? GetLatestPrice(string indexName)
        {
            if (!indexes.ContainsKey(indexName))
                return null;

            return indexes[indexName]
                .OrderByDescending(i => i.Date)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets price for an index on a specific date.
        /// </summary>
        public PriceIndex? GetPrice(string indexName, DateTime date)
        {
            if (!indexes.ContainsKey(indexName))
                return null;

            return indexes[indexName]
                .Where(i => i.Date.Date == date.Date)
                .OrderByDescending(i => i.Date)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets prices for an index in a date range.
        /// </summary>
        public IEnumerable<PriceIndex> GetPrices(string indexName, DateTime startDate, DateTime endDate)
        {
            if (!indexes.ContainsKey(indexName))
                return Enumerable.Empty<PriceIndex>();

            return indexes[indexName]
                .Where(i => i.Date >= startDate && i.Date <= endDate)
                .OrderBy(i => i.Date);
        }

        /// <summary>
        /// Gets average price for an index in a date range.
        /// </summary>
        public decimal? GetAveragePrice(string indexName, DateTime startDate, DateTime endDate)
        {
            var prices = GetPrices(indexName, startDate, endDate).ToList();
            if (prices.Count == 0)
                return null;

            return prices.Average(p => p.Price);
        }

        /// <summary>
        /// Gets all available index names.
        /// </summary>
        public IEnumerable<string> GetAvailableIndexes()
        {
            return indexes.Keys;
        }

        /// <summary>
        /// Initializes with standard price indexes.
        /// </summary>
        public void InitializeStandardIndexes()
        {
            // WTI (West Texas Intermediate)
            RegisterIndex(new PriceIndex
            {
                IndexId = Guid.NewGuid().ToString(),
                IndexName = "WTI",
                PricingPoint = "Cushing, Oklahoma",
                Date = DateTime.Now,
                Price = 0m, // Will be updated with actual prices
                Source = "NYMEX"
            });

            // Brent
            RegisterIndex(new PriceIndex
            {
                IndexId = Guid.NewGuid().ToString(),
                IndexName = "Brent",
                PricingPoint = "North Sea",
                Date = DateTime.Now,
                Price = 0m,
                Source = "ICE"
            });

            // LLS (Light Louisiana Sweet)
            RegisterIndex(new PriceIndex
            {
                IndexId = Guid.NewGuid().ToString(),
                IndexName = "LLS",
                PricingPoint = "Louisiana",
                Date = DateTime.Now,
                Price = 0m,
                Source = "Platts"
            });

            // WCS (Western Canadian Select)
            RegisterIndex(new PriceIndex
            {
                IndexId = Guid.NewGuid().ToString(),
                IndexName = "WCS",
                PricingPoint = "Hardisty, Alberta",
                Date = DateTime.Now,
                Price = 0m,
                Source = "Platts"
            });
        }
    }
}

