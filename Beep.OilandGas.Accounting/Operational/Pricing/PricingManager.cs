using System;
using System.Collections.Generic;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Accounting.Operational.Production;

namespace Beep.OilandGas.Accounting.Operational.Pricing
{
    /// <summary>
    /// Manages pricing operations.
    /// </summary>
    public class PricingManager
    {
        private readonly PriceIndexManager indexManager;
        private readonly RegulatedPricingManager regulatedPricingManager;
        private readonly Dictionary<string, RunTicketValuation> valuations = new();

        public PricingManager()
        {
            indexManager = new PriceIndexManager();
            indexManager.InitializeStandardIndexes();
            regulatedPricingManager = new RegulatedPricingManager();
        }

        /// <summary>
        /// Values a run ticket.
        /// </summary>
        public RunTicketValuation ValueRunTicket(
            RunTicket runTicket,
            PricingMethod method,
            decimal? fixedPrice = null,
            string? indexName = null,
            decimal? differential = null,
            string? regulatoryAuthority = null)
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
                    var index = indexManager.GetLatestPrice(indexName);
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
                    var regulatedPrice = regulatedPricingManager.GetApplicablePrice(
                        regulatoryAuthority, 
                        runTicket.TicketDateTime);
                    if (regulatedPrice == null)
                        throw new InvalidOperationException($"No regulated price found for {regulatoryAuthority} on {runTicket.TicketDateTime}.");
                    valuation = RunTicketValuationEngine.ValueWithRegulatedPrice(runTicket, regulatedPrice);
                    break;

                default:
                    throw new ArgumentException($"Unsupported pricing method: {method}", nameof(method));
            }

            valuations[valuation.ValuationId] = valuation;
            return valuation;
        }

        /// <summary>
        /// Gets a valuation by ID.
        /// </summary>
        public RunTicketValuation? GetValuation(string valuationId)
        {
            return valuations.TryGetValue(valuationId, out var valuation) ? valuation : null;
        }

        /// <summary>
        /// Gets the price index manager.
        /// </summary>
        public PriceIndexManager GetIndexManager() => indexManager;

        /// <summary>
        /// Gets the regulated pricing manager.
        /// </summary>
        public RegulatedPricingManager GetRegulatedPricingManager() => regulatedPricingManager;
    }
}

