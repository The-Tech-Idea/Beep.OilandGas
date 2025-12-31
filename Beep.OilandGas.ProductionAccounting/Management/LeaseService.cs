using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Agreement;
using Beep.OilandGas.Models.DTOs.Lease;
using Beep.OilandGas.Models.DTOs.Agreement;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Management
{
    /// <summary>
    /// Service for managing lease agreements and operations.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class LeaseService : ILeaseService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<LeaseService>? _logger;
        private readonly string _connectionName;
        private const string FEE_MINERAL_LEASE_TABLE = "FEE_MINERAL_LEASE";
        private const string GOVERNMENT_LEASE_TABLE = "GOVERNMENT_LEASE";
        private const string SALES_AGREEMENT_TABLE = "SALES_AGREEMENT";
        private const string TRANSPORTATION_AGREEMENT_TABLE = "TRANSPORTATION_AGREEMENT";

        public LeaseService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<LeaseService>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Registers a fee mineral lease.
        /// </summary>
        public async Task<FEE_MINERAL_LEASE> RegisterFeeMineralLeaseAsync(CreateFeeMineralLeaseRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = await GetFeeMineralLeaseRepositoryAsync(connName);

            var lease = new FEE_MINERAL_LEASE
            {
                LEASE_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = request.PropertyId,
                LEASE_NUMBER = request.LeaseNumber,
                LEASE_NAME = request.LeaseName,
                MINERAL_OWNER_BA_ID = request.MineralOwnerBaId,
                SURFACE_OWNER_BA_ID = request.SurfaceOwnerBaId,
                EFFECTIVE_DATE = request.EffectiveDate,
                EXPIRATION_DATE = request.ExpirationDate,
                PRIMARY_TERM_MONTHS = request.PrimaryTermMonths,
                WORKING_INTEREST = request.WorkingInterest,
                NET_REVENUE_INTEREST = request.NetRevenueInterest,
                ROYALTY_RATE = request.RoyaltyRate,
                IS_HELD_BY_PRODUCTION = "N",
                ACTIVE_IND = "Y"
            };

            if (lease is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(lease);

            _logger?.LogDebug("Registered fee mineral lease {LeaseId} for property {PropertyId}", lease.LEASE_ID, request.PropertyId);
            return lease;
        }

        /// <summary>
        /// Registers a government lease.
        /// </summary>
        public async Task<GOVERNMENT_LEASE> RegisterGovernmentLeaseAsync(CreateGovernmentLeaseRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = await GetGovernmentLeaseRepositoryAsync(connName);

            var lease = new GOVERNMENT_LEASE
            {
                LEASE_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = request.PropertyId,
                GOVERNMENT_AGENCY = request.GovernmentAgency,
                LEASE_NUMBER = request.LeaseNumber,
                IS_FEDERAL = request.IsFederal ? "Y" : "N",
                IS_INDIAN = request.IsIndian ? "Y" : "N",
                EFFECTIVE_DATE = request.EffectiveDate,
                EXPIRATION_DATE = request.ExpirationDate,
                WORKING_INTEREST = request.WorkingInterest,
                NET_REVENUE_INTEREST = request.NetRevenueInterest,
                ROYALTY_RATE = request.RoyaltyRate,
                ACTIVE_IND = "Y"
            };

            if (lease is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(lease);

            _logger?.LogDebug("Registered government lease {LeaseId} for property {PropertyId}", lease.LEASE_ID, request.PropertyId);
            return lease;
        }

        /// <summary>
        /// Gets a fee mineral lease by ID.
        /// </summary>
        public async Task<FEE_MINERAL_LEASE?> GetFeeMineralLeaseAsync(string leaseId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(leaseId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = await GetFeeMineralLeaseRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<FEE_MINERAL_LEASE>().FirstOrDefault();
        }

        /// <summary>
        /// Gets leases by property.
        /// </summary>
        public async Task<List<FEE_MINERAL_LEASE>> GetLeasesByPropertyAsync(string propertyId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                return new List<FEE_MINERAL_LEASE>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetFeeMineralLeaseRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<FEE_MINERAL_LEASE>().OrderByDescending(l => l.EFFECTIVE_DATE).ToList();
        }

        /// <summary>
        /// Registers a sales agreement.
        /// </summary>
        public async Task<SALES_AGREEMENT> RegisterSalesAgreementAsync(CreateSalesAgreementRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = await GetSalesAgreementRepositoryAsync(connName);

            var agreement = new SALES_AGREEMENT
            {
                AGREEMENT_ID = Guid.NewGuid().ToString(),
                AGREEMENT_NAME = request.AgreementName,
                SELLER_BA_ID = request.SellerBaId,
                PURCHASER_BA_ID = request.PurchaserBaId,
                EFFECTIVE_DATE = request.EffectiveDate,
                EXPIRATION_DATE = request.ExpirationDate,
                PRICING_METHOD = request.PricingMethod,
                BASE_PRICE = request.BasePrice,
                PRICE_INDEX = request.PriceIndex,
                DIFFERENTIAL = request.Differential,
                PAYMENT_TERMS_DAYS = request.PaymentTermsDays,
                ACTIVE_IND = "Y"
            };

            if (agreement is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(agreement);

            _logger?.LogDebug("Registered sales agreement {AgreementId}", agreement.AGREEMENT_ID);
            return agreement;
        }

        /// <summary>
        /// Registers a transportation agreement.
        /// </summary>
        public async Task<TRANSPORTATION_AGREEMENT> RegisterTransportationAgreementAsync(CreateTransportationAgreementRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = await GetTransportationAgreementRepositoryAsync(connName);

            var agreement = new TRANSPORTATION_AGREEMENT
            {
                AGREEMENT_ID = Guid.NewGuid().ToString(),
                CARRIER_BA_ID = request.CarrierBaId,
                ORIGIN_POINT = request.OriginPoint,
                DESTINATION_POINT = request.DestinationPoint,
                EFFECTIVE_DATE = request.EffectiveDate,
                EXPIRATION_DATE = request.ExpirationDate,
                TARIFF_RATE = request.TariffRate,
                MINIMUM_VOLUME_COMMITMENT = request.MinimumVolumeCommitment,
                MAXIMUM_CAPACITY = request.MaximumCapacity,
                ACTIVE_IND = "Y"
            };

            if (agreement is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(agreement);

            _logger?.LogDebug("Registered transportation agreement {AgreementId}", agreement.AGREEMENT_ID);
            return agreement;
        }

        /// <summary>
        /// Renews a lease.
        /// </summary>
        public async Task<LeaseRenewalResult> RenewLeaseAsync(string leaseId, LeaseRenewalRequest request, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(leaseId))
                throw new ArgumentException("Lease ID is required.", nameof(leaseId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var lease = await GetFeeMineralLeaseAsync(leaseId, connName);

            if (lease == null)
                throw new InvalidOperationException($"Lease {leaseId} not found.");

            lease.EXPIRATION_DATE = request.NewExpirationDate;

            if (request.NewRoyaltyRate.HasValue)
            {
                lease.ROYALTY_RATE = request.NewRoyaltyRate.Value;
            }

            if (lease is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, userId, connName);
            }

            var repo = await GetFeeMineralLeaseRepositoryAsync(connName);
            await repo.UpdateAsync(lease);

            _logger?.LogDebug("Renewed lease {LeaseId} until {ExpirationDate}", leaseId, request.NewExpirationDate);

            return new LeaseRenewalResult
            {
                LeaseId = leaseId,
                IsRenewed = true,
                NewExpirationDate = request.NewExpirationDate,
                RenewalDate = DateTime.UtcNow,
                RenewedBy = userId
            };
        }

        /// <summary>
        /// Gets leases expiring within a date range.
        /// </summary>
        public async Task<List<LeaseExpirationAlert>> GetLeasesExpiringAsync(DateTime? expirationDate, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = await GetFeeMineralLeaseRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (expirationDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "EXPIRATION_DATE", Operator = "<=", FilterValue = expirationDate.Value });
            }

            var results = await repo.GetAsync(filters);
            var leases = results.Cast<FEE_MINERAL_LEASE>()
                .Where(l => l.EXPIRATION_DATE.HasValue)
                .ToList();

            var alerts = new List<LeaseExpirationAlert>();
            var today = DateTime.UtcNow.Date;

            foreach (var lease in leases)
            {
                if (lease.EXPIRATION_DATE.HasValue)
                {
                    var daysUntilExpiration = (lease.EXPIRATION_DATE.Value.Date - today).Days;
                    var alertLevel = daysUntilExpiration <= 30 ? "Critical" : daysUntilExpiration <= 90 ? "Warning" : "Info";

                    alerts.Add(new LeaseExpirationAlert
                    {
                        LeaseId = lease.LEASE_ID ?? string.Empty,
                        LeaseNumber = lease.LEASE_NUMBER ?? string.Empty,
                        LeaseName = lease.LEASE_NAME ?? string.Empty,
                        ExpirationDate = lease.EXPIRATION_DATE.Value,
                        DaysUntilExpiration = daysUntilExpiration,
                        AlertLevel = alertLevel
                    });
                }
            }

            return alerts.OrderBy(a => a.DaysUntilExpiration).ToList();
        }

        /// <summary>
        /// Gets lease summary.
        /// </summary>
        public async Task<LeaseSummary> GetLeaseSummaryAsync(string leaseId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(leaseId))
                throw new ArgumentException("Lease ID is required.", nameof(leaseId));

            var connName = connectionName ?? _connectionName;
            var lease = await GetFeeMineralLeaseAsync(leaseId, connName);

            if (lease == null)
                throw new InvalidOperationException($"Lease {leaseId} not found.");

            var today = DateTime.UtcNow.Date;
            var isActive = lease.EFFECTIVE_DATE.HasValue && lease.EFFECTIVE_DATE.Value.Date <= today &&
                          (!lease.EXPIRATION_DATE.HasValue || lease.EXPIRATION_DATE.Value.Date >= today);

            return new LeaseSummary
            {
                LeaseId = lease.LEASE_ID ?? string.Empty,
                LeaseNumber = lease.LEASE_NUMBER ?? string.Empty,
                LeaseName = lease.LEASE_NAME ?? string.Empty,
                LeaseType = "FeeMineral",
                EffectiveDate = lease.EFFECTIVE_DATE ?? DateTime.MinValue,
                ExpirationDate = lease.EXPIRATION_DATE,
                IsActive = isActive,
                WorkingInterest = lease.WORKING_INTEREST ?? 0m,
                NetRevenueInterest = lease.NET_REVENUE_INTEREST ?? 0m,
                RoyaltyRate = lease.ROYALTY_RATE ?? 0m,
                IsHeldByProduction = lease.IS_HELD_BY_PRODUCTION == "Y"
            };
        }

        // Repository helper methods
        private async Task<PPDMGenericRepository> GetFeeMineralLeaseRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(FEE_MINERAL_LEASE_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Lease.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(FEE_MINERAL_LEASE);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, FEE_MINERAL_LEASE_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetGovernmentLeaseRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(GOVERNMENT_LEASE_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Lease.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(GOVERNMENT_LEASE);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, GOVERNMENT_LEASE_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetSalesAgreementRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(SALES_AGREEMENT_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Agreement.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(SALES_AGREEMENT);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, SALES_AGREEMENT_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetTransportationAgreementRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(TRANSPORTATION_AGREEMENT_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Agreement.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(TRANSPORTATION_AGREEMENT);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, TRANSPORTATION_AGREEMENT_TABLE,
                null);
        }
    }
}
