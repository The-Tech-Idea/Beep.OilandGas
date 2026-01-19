using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Lease economic interest service for ownership and royalty validation.
    /// </summary>
    public class LeaseEconomicInterestService : ILeaseEconomicInterestService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<LeaseEconomicInterestService> _logger;
        private const string ConnectionName = "PPDM39";

        public LeaseEconomicInterestService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<LeaseEconomicInterestService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<List<OWNERSHIP_INTEREST>> GetOwnershipInterestsAsync(
            string propertyOrLeaseId,
            DateTime? asOfDate = null,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyOrLeaseId))
                throw new ArgumentNullException(nameof(propertyOrLeaseId));

            var metadata = await _metadata.GetTableMetadataAsync("OWNERSHIP_INTEREST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(OWNERSHIP_INTEREST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "OWNERSHIP_INTEREST");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = propertyOrLeaseId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var interests = results?.Cast<OWNERSHIP_INTEREST>().ToList() ?? new List<OWNERSHIP_INTEREST>();

            return ApplyEffectiveDateFilter(interests, asOfDate);
        }

        public async Task<List<ROYALTY_INTEREST>> GetRoyaltyInterestsAsync(
            string propertyOrLeaseId,
            DateTime? asOfDate = null,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyOrLeaseId))
                throw new ArgumentNullException(nameof(propertyOrLeaseId));

            var metadata = await _metadata.GetTableMetadataAsync("ROYALTY_INTEREST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ROYALTY_INTEREST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ROYALTY_INTEREST");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = propertyOrLeaseId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var interests = results?.Cast<ROYALTY_INTEREST>().ToList() ?? new List<ROYALTY_INTEREST>();

            return ApplyEffectiveDateFilter(interests, asOfDate);
        }

        public async Task<bool> ValidateEconomicInterestsAsync(
            string propertyOrLeaseId,
            DateTime? asOfDate = null,
            string cn = "PPDM39")
        {
            var ownership = await GetOwnershipInterestsAsync(propertyOrLeaseId, asOfDate, cn);
            var royalty = await GetRoyaltyInterestsAsync(propertyOrLeaseId, asOfDate, cn);
            var divisionOrders = await GetDivisionOrdersAsync(propertyOrLeaseId, asOfDate, cn);

            decimal workingTotal = ownership.Sum(o => NormalizeFraction(o.WORKING_INTEREST));
            decimal royaltyTotal = ownership.Sum(o => NormalizeFraction(o.ROYALTY_INTEREST) + NormalizeFraction(o.OVERRIDING_ROYALTY_INTEREST));
            decimal directRoyaltyTotal = royalty.Sum(r => NormalizeFraction(r.INTEREST_PERCENTAGE));

            decimal nriTotal = ownership.Sum(o => CalculateNetRevenueInterest(o, divisionOrders));

            var unapprovedDivisionOrders = ownership
                .Where(o => !string.IsNullOrWhiteSpace(o.DIVISION_ORDER_ID))
                .Where(o => divisionOrders.All(d =>
                    !string.Equals(d.DIVISION_ORDER_ID, o.DIVISION_ORDER_ID, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(d.STATUS, "APPROVED", StringComparison.OrdinalIgnoreCase)))
                .Select(o => o.DIVISION_ORDER_ID)
                .Distinct()
                .ToList();

            if (unapprovedDivisionOrders.Count > 0)
            {
                _logger?.LogWarning(
                    "Economic interest validation failed for {PropertyOrLeaseId}: unapproved division orders {DivisionOrders}",
                    propertyOrLeaseId, string.Join(", ", unapprovedDivisionOrders));
                return false;
            }

            bool workingOk = workingTotal <= 1.0m + 0.0001m;
            bool royaltyOk = (royaltyTotal + directRoyaltyTotal) <= 1.0m + 0.0001m;
            bool nriOk = nriTotal <= 1.0m + 0.0001m;

            if (!workingOk || !royaltyOk || !nriOk)
            {
                _logger?.LogWarning(
                    "Economic interest validation failed for {PropertyOrLeaseId}: working={WorkingTotal}, royalty={RoyaltyTotal}, nri={NriTotal}",
                    propertyOrLeaseId, workingTotal, royaltyTotal + directRoyaltyTotal, nriTotal);
                return false;
            }

            return true;
        }

        private static List<OWNERSHIP_INTEREST> ApplyEffectiveDateFilter(
            List<OWNERSHIP_INTEREST> interests,
            DateTime? asOfDate)
        {
            if (!asOfDate.HasValue)
                return interests;

            DateTime date = asOfDate.Value.Date;
            return interests
                .Where(i =>
                    (!i.EFFECTIVE_START_DATE.HasValue || i.EFFECTIVE_START_DATE.Value.Date <= date) &&
                    (!i.EFFECTIVE_END_DATE.HasValue || i.EFFECTIVE_END_DATE.Value.Date >= date))
                .ToList();
        }

        private static List<ROYALTY_INTEREST> ApplyEffectiveDateFilter(
            List<ROYALTY_INTEREST> interests,
            DateTime? asOfDate)
        {
            if (!asOfDate.HasValue)
                return interests;

            DateTime date = asOfDate.Value.Date;
            return interests
                .Where(i =>
                    (!i.EFFECTIVE_START_DATE.HasValue || i.EFFECTIVE_START_DATE.Value.Date <= date) &&
                    (!i.EFFECTIVE_END_DATE.HasValue || i.EFFECTIVE_END_DATE.Value.Date >= date) &&
                    (!i.EFFECTIVE_DATE.HasValue || i.EFFECTIVE_DATE.Value.Date <= date) &&
                    (!i.EXPIRATION_DATE.HasValue || i.EXPIRATION_DATE.Value.Date >= date))
                .ToList();
        }

        private async Task<List<DIVISION_ORDER>> GetDivisionOrdersAsync(
            string propertyOrLeaseId,
            DateTime? asOfDate,
            string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("DIVISION_ORDER");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(DIVISION_ORDER);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "DIVISION_ORDER");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = propertyOrLeaseId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var divisionOrders = results?.Cast<DIVISION_ORDER>().ToList() ?? new List<DIVISION_ORDER>();

            if (!asOfDate.HasValue)
                return divisionOrders;

            var date = asOfDate.Value.Date;
            return divisionOrders
                .Where(d =>
                    string.Equals(d.STATUS, "APPROVED", StringComparison.OrdinalIgnoreCase) &&
                    (!d.EFFECTIVE_DATE.HasValue || d.EFFECTIVE_DATE.Value.Date <= date) &&
                    (!d.EXPIRATION_DATE.HasValue || d.EXPIRATION_DATE.Value.Date >= date))
                .ToList();
        }

        private static decimal CalculateNetRevenueInterest(
            OWNERSHIP_INTEREST ownership,
            List<DIVISION_ORDER> divisionOrders)
        {
            if (ownership == null)
                return 0m;

            var nri = NormalizeFraction(ownership.NET_REVENUE_INTEREST);
            if (nri > 0m)
                return nri;

            if (!string.IsNullOrWhiteSpace(ownership.DIVISION_ORDER_ID))
            {
                var order = divisionOrders.FirstOrDefault(d =>
                    string.Equals(d.DIVISION_ORDER_ID, ownership.DIVISION_ORDER_ID, StringComparison.OrdinalIgnoreCase));
                if (order != null)
                {
                    var orderNri = NormalizeFraction(order.NET_REVENUE_INTEREST);
                    if (orderNri > 0m)
                        return orderNri;
                }
            }

            var working = NormalizeFraction(ownership.WORKING_INTEREST);
            var royalty = NormalizeFraction(ownership.ROYALTY_INTEREST);
            var overriding = NormalizeFraction(ownership.OVERRIDING_ROYALTY_INTEREST);
            var computed = working - (royalty + overriding);
            return computed < 0m ? 0m : computed;
        }

        private static decimal NormalizeFraction(decimal? value)
        {
            if (!value.HasValue)
                return 0m;
            if (value.Value <= 0m)
                return 0m;
            return value.Value > 1m ? value.Value / 100m : value.Value;
        }
    }
}
