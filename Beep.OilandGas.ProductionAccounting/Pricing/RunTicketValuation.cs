
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.ProductionAccounting.Calculations;
using Beep.OilandGas.ProductionAccounting.Trading;

namespace Beep.OilandGas.ProductionAccounting.Pricing
{
    /// <summary>
    /// Provides run ticket valuation functionality.
    /// </summary>
    public static class RUN_TICKET_VALUATIONEngine
    {
        /// <summary>
        /// Values a run ticket using fixed price.
        /// </summary>
        public static RUN_TICKET_VALUATION ValueWithFixedPrice(
            RUN_TICKET runTicket,
            decimal fixedPrice)
        {
            if (runTicket == null)
                throw new ArgumentNullException(nameof(runTicket));

            if (fixedPrice < 0)
                throw new ArgumentException("Fixed price cannot be negative.", nameof(fixedPrice));

            var valuation = new RUN_TICKET_VALUATION
            {
                ValuationId = Guid.NewGuid().ToString(),
                RunTicketNumber = runTicket.RunTicketNumber,
                ValuationDate = runTicket.TicketDateTime,
                BasePrice = fixedPrice,
                NetVolume = runTicket.NetVolume,
                PricingMethod = PricingMethod.Fixed
            };

            // Apply quality adjustments if properties available
            if (runTicket.Properties != null)
            {
                valuation.QualityAdjustments = CalculateQualityAdjustments(runTicket.Properties);
            }

            return valuation;
        }

        /// <summary>
        /// Values a run ticket using index-based pricing.
        /// </summary>
        public static RUN_TICKET_VALUATION ValueWithIndex(
            RUN_TICKET runTicket,
            PRICE_INDEX priceIndex,
            decimal differential = 0m,
            decimal? apiGravityDifferential = null,
            decimal? sulfurDifferential = null)
        {
            if (runTicket == null)
                throw new ArgumentNullException(nameof(runTicket));

            if (priceIndex == null)
                throw new ArgumentNullException(nameof(priceIndex));

            var valuation = new RUN_TICKET_VALUATION
            {
                ValuationId = Guid.NewGuid().ToString(),
                RunTicketNumber = runTicket.RunTicketNumber,
                ValuationDate = runTicket.TicketDateTime,
                BasePrice = priceIndex.Price,
                NetVolume = runTicket.NetVolume,
                PricingMethod = PricingMethod.IndexBased
            };

            // Apply location differential
            valuation..LocationDifferential = differential;

            // Apply quality adjustments
            if (runTicket.Properties != null && (apiGravityDifferential.HasValue || sulfurDifferential.HasValue))
            {
                if (apiGravityDifferential.HasValue)
                {
                    decimal referenceApi = 40m; // Standard reference
                    valuation.QualityAdjustments.ApiGravityAdjustment = 
                        DifferentialCalculator.CalculateQualityDifferential(
                            runTicket.Properties.ApiGravity,
                            referenceApi,
                            apiGravityDifferential.Value);
                }

                if (sulfurDifferential.HasValue)
                {
                    decimal referenceSulfur = 0.5m; // Standard reference
                    valuation.QualityAdjustments.SulfurAdjustment = 
                        DifferentialCalculator.CalculateSulfurDifferential(
                            runTicket.Properties.SulfurContent,
                            referenceSulfur,
                            sulfurDifferential.Value);
                }
            }

            return valuation;
        }

        /// <summary>
        /// Values a run ticket using posted price.
        /// </summary>
        public static RUN_TICKET_VALUATION ValueWithPostedPrice(
            RUN_TICKET runTicket,
            decimal postedPrice,
            decimal locationDifferential = 0m)
        {
            if (runTicket == null)
                throw new ArgumentNullException(nameof(runTicket));

            var valuation = new RUN_TICKET_VALUATION
            {
                VALUATION_ID = Guid.NewGuid().ToString(),
                RUN_TICKET_NUMBER = runTicket.RUN_TICKET_NUMBER,
                VALUATION_DATE = runTicket.TICKET_DATE_TIME,
                BASE_PRICE = postedPrice,
                NET_VOLUME = runTicket.NET_VOLUME,
                PRICING_METHOD = PricingMethod.PostedPrice.ToString()
            };

            valuation.LocationAdjustments.LocationDifferential = locationDifferential;

            // Apply quality adjustments
            if (runTicket.Properties != null)
            {
                valuation.QualityAdjustments = CalculateQualityAdjustments(runTicket.Properties);
            }

            return valuation;
        }

        /// <summary>
        /// Values a run ticket using regulated pricing.
        /// </summary>
        public static RUN_TICKET_VALUATION ValueWithRegulatedPrice(
            RunTicket runTicket,
            RegulatedPrice regulatedPrice)
        {
            if (runTicket == null)
                throw new ArgumentNullException(nameof(runTicket));

            if (regulatedPrice == null)
                throw new ArgumentNullException(nameof(regulatedPrice));

            // Calculate price using formula (simplified - full implementation would parse formula)
            decimal calculatedPrice = regulatedPrice.BasePrice;

            // Apply adjustment factors
            foreach (var factor in regulatedPrice.AdjustmentFactors)
            {
                // Simplified - full implementation would apply factors based on formula
                calculatedPrice += factor.Value;
            }

            // Apply price cap/floor
            if (regulatedPrice.PriceCap.HasValue && calculatedPrice > regulatedPrice.PriceCap.Value)
                calculatedPrice = regulatedPrice.PriceCap.Value;

            if (regulatedPrice.PriceFloor.HasValue && calculatedPrice < regulatedPrice.PriceFloor.Value)
                calculatedPrice = regulatedPrice.PriceFloor.Value;

            var valuation = new RUN_TICKET_VALUATION
            {
                ValuationId = Guid.NewGuid().ToString(),
                RunTicketNumber = runTicket.RunTicketNumber,
                ValuationDate = runTicket.TicketDateTime,
                BasePrice = calculatedPrice,
                NetVolume = runTicket.NetVolume,
                PricingMethod = PricingMethod.Regulated
            };

            return valuation;
        }

        /// <summary>
        /// Calculates quality adjustments from crude oil properties.
        /// </summary>
        private static QualityAdjustments CalculateQualityAdjustments(CrudeOilProperties properties)
        {
            var adjustments = new QualityAdjustments();

            // BS&W penalty (simplified - typically $X per 0.1% above threshold)
            if (properties.BSW > 0.5m)
            {
                decimal excessBSW = properties.BSW - 0.5m;
                adjustments.BSWAdjustment = -excessBSW * 0.10m; // $0.10 per 0.1% excess
            }

            return adjustments;
        }
    }
}
