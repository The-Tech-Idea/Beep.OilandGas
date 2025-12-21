using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Accounting.Operational.Pricing
{
    /// <summary>
    /// Manages regulated pricing.
    /// </summary>
    public class RegulatedPricingManager
    {
        private readonly Dictionary<string, List<RegulatedPrice>> regulatedPrices = new();

        /// <summary>
        /// Registers a regulated price.
        /// </summary>
        public void RegisterRegulatedPrice(RegulatedPrice regulatedPrice)
        {
            if (regulatedPrice == null)
                throw new ArgumentNullException(nameof(regulatedPrice));

            if (string.IsNullOrEmpty(regulatedPrice.RegulatoryAuthority))
                throw new ArgumentException("Regulatory authority cannot be null or empty.", nameof(regulatedPrice));

            if (!regulatedPrices.ContainsKey(regulatedPrice.RegulatoryAuthority))
                regulatedPrices[regulatedPrice.RegulatoryAuthority] = new List<RegulatedPrice>();

            regulatedPrices[regulatedPrice.RegulatoryAuthority].Add(regulatedPrice);
        }

        /// <summary>
        /// Gets the applicable regulated price for a date.
        /// </summary>
        public RegulatedPrice? GetApplicablePrice(string regulatoryAuthority, DateTime date)
        {
            if (!regulatedPrices.ContainsKey(regulatoryAuthority))
                return null;

            return regulatedPrices[regulatoryAuthority]
                .Where(p => p.EffectiveStartDate <= date &&
                           (p.EffectiveEndDate == null || p.EffectiveEndDate >= date))
                .OrderByDescending(p => p.EffectiveStartDate)
                .FirstOrDefault();
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
        public IEnumerable<RegulatedPrice> GetRegulatedPrices(string regulatoryAuthority)
        {
            if (!regulatedPrices.ContainsKey(regulatoryAuthority))
                return Enumerable.Empty<RegulatedPrice>();

            return regulatedPrices[regulatoryAuthority];
        }

        /// <summary>
        /// Gets all regulatory authorities.
        /// </summary>
        public IEnumerable<string> GetRegulatoryAuthorities()
        {
            return regulatedPrices.Keys;
        }
    }
}

