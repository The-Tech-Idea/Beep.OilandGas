using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Validation;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionAccounting.Management
{
    /// <summary>
    /// Manages lease agreements and operations.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class LeaseManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<LeaseManager>? _logger;
        private readonly string _connectionName;
        private const string LEASE_TABLE = "LEASE";
        private const string SALES_AGREEMENT_TABLE = "SALES_AGREEMENT";
        private const string TRANSPORTATION_AGREEMENT_TABLE = "TRANSPORTATION_AGREEMENT";

        public LeaseManager(
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
            _logger = loggerFactory?.CreateLogger<LeaseManager>();
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Registers a new lease agreement.
        /// </summary>
        public async Task RegisterLeaseAsync(LeaseAgreement lease, string userId = "system", string? connectionName = null)
        {
            if (lease == null)
                throw new ArgumentNullException(nameof(lease));

            if (string.IsNullOrEmpty(lease.LeaseId))
                throw new InvalidLeaseDataException(nameof(lease.LeaseId), "Lease ID cannot be null or empty.");

            LeaseValidator.Validate(lease);

            // Save to database using IDataSource
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Save lease to database
            var result = dataSource.InsertEntity(LEASE_TABLE, lease);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to register lease {LeaseId}: {Error}", lease.LeaseId, errorMessage);
                throw new InvalidLeaseDataException(nameof(lease), $"Failed to save lease: {errorMessage}");
            }

            _logger?.LogDebug("Registered lease {LeaseId} to database", lease.LeaseId);
        }

        /// <summary>
        /// Gets a lease by ID.
        /// </summary>
        public async Task<LeaseAgreement?> GetLeaseAsync(string leaseId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(leaseId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Query database using IDataSource.GetEntityAsync
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId }
            };

            var results = await dataSource.GetEntityAsync(LEASE_TABLE, filters);
            var leaseData = results?.FirstOrDefault();
            
            if (leaseData == null)
                return null;

            return leaseData as LeaseAgreement;
        }

        /// <summary>
        /// Gets a lease by ID (synchronous wrapper for backward compatibility).
        /// </summary>
        public LeaseAgreement? GetLease(string leaseId)
        {
            return GetLeaseAsync(leaseId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets all leases.
        /// </summary>
        public async Task<IEnumerable<LeaseAgreement>> GetAllLeasesAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var results = await dataSource.GetEntityAsync(LEASE_TABLE, null);
            if (results == null || !results.Any())
                return Enumerable.Empty<LeaseAgreement>();

            return results.Cast<LeaseAgreement>().Where(l => l != null)!;
        }

        /// <summary>
        /// Gets all leases (synchronous wrapper).
        /// </summary>
        public IEnumerable<LeaseAgreement> GetAllLeases()
        {
            return GetAllLeasesAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets leases by type.
        /// </summary>
        public async Task<IEnumerable<LeaseAgreement>> GetLeasesByTypeAsync(LeaseType leaseType, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LEASE_TYPE", Operator = "=", FilterValue = leaseType.ToString() }
            };

            var results = await dataSource.GetEntityAsync(LEASE_TABLE, filters);
            if (results == null || !results.Any())
                return Enumerable.Empty<LeaseAgreement>();

            return results.Cast<LeaseAgreement>().Where(l => l != null)!;
        }

        /// <summary>
        /// Gets leases by type (synchronous wrapper).
        /// </summary>
        public IEnumerable<LeaseAgreement> GetLeasesByType(LeaseType leaseType)
        {
            return GetLeasesByTypeAsync(leaseType).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets active leases (not expired).
        /// </summary>
        public async Task<IEnumerable<LeaseAgreement>> GetActiveLeasesAsync(DateTime asOfDate, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var paramDelimiter = dataSource.ParameterDelimiter;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "EFFECTIVE_DATE", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "EXPIRATION_DATE", Operator = ">=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "EXPIRATION_DATE", Operator = "IS NULL", FilterValue = null }
            };

            var results = await dataSource.GetEntityAsync(LEASE_TABLE, filters);
            if (results == null || !results.Any())
                return Enumerable.Empty<LeaseAgreement>();

            return results.Cast<LeaseAgreement>()
                .Where(l => l != null && l.EffectiveDate <= asOfDate && (l.ExpirationDate == null || l.ExpirationDate >= asOfDate))!;
        }

        /// <summary>
        /// Gets active leases (synchronous wrapper).
        /// </summary>
        public IEnumerable<LeaseAgreement> GetActiveLeases(DateTime asOfDate)
        {
            return GetActiveLeasesAsync(asOfDate).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks if a lease is held by production.
        /// </summary>
        public bool IsHeldByProduction(string leaseId)
        {
            var lease = GetLease(leaseId);
            return lease?.Provisions.IsHeldByProduction ?? false;
        }

        /// <summary>
        /// Registers a sales agreement.
        /// </summary>
        public async Task RegisterSalesAgreementAsync(OilSalesAgreement agreement, string userId = "system", string? connectionName = null)
        {
            if (agreement == null)
                throw new ArgumentNullException(nameof(agreement));

            if (string.IsNullOrEmpty(agreement.AgreementId))
                throw new InvalidLeaseDataException(nameof(agreement.AgreementId), "Agreement ID cannot be null or empty.");

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(SALES_AGREEMENT_TABLE, agreement);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to register sales agreement {AgreementId}: {Error}", agreement.AgreementId, errorMessage);
                throw new InvalidLeaseDataException(nameof(agreement), $"Failed to save sales agreement: {errorMessage}");
            }

            _logger?.LogDebug("Registered sales agreement {AgreementId} to database", agreement.AgreementId);
        }

        /// <summary>
        /// Registers a sales agreement (synchronous wrapper).
        /// </summary>
        public void RegisterSalesAgreement(OilSalesAgreement agreement)
        {
            RegisterSalesAgreementAsync(agreement).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a sales agreement by ID.
        /// </summary>
        public async Task<OilSalesAgreement?> GetSalesAgreementAsync(string agreementId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(agreementId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AGREEMENT_ID", Operator = "=", FilterValue = agreementId }
            };

            var results = await dataSource.GetEntityAsync(SALES_AGREEMENT_TABLE, filters);
            var agreementData = results?.FirstOrDefault();
            
            if (agreementData == null)
                return null;

            return agreementData as OilSalesAgreement;
        }

        /// <summary>
        /// Gets a sales agreement by ID (synchronous wrapper).
        /// </summary>
        public OilSalesAgreement? GetSalesAgreement(string agreementId)
        {
            return GetSalesAgreementAsync(agreementId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Registers a transportation agreement.
        /// </summary>
        public async Task RegisterTransportationAgreementAsync(TransportationAgreement agreement, string userId = "system", string? connectionName = null)
        {
            if (agreement == null)
                throw new ArgumentNullException(nameof(agreement));

            if (string.IsNullOrEmpty(agreement.AgreementId))
                throw new InvalidLeaseDataException(nameof(agreement.AgreementId), "Agreement ID cannot be null or empty.");

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(TRANSPORTATION_AGREEMENT_TABLE, agreement);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to register transportation agreement {AgreementId}: {Error}", agreement.AgreementId, errorMessage);
                throw new InvalidLeaseDataException(nameof(agreement), $"Failed to save transportation agreement: {errorMessage}");
            }

            _logger?.LogDebug("Registered transportation agreement {AgreementId} to database", agreement.AgreementId);
        }

        /// <summary>
        /// Registers a transportation agreement (synchronous wrapper).
        /// </summary>
        public void RegisterTransportationAgreement(TransportationAgreement agreement)
        {
            RegisterTransportationAgreementAsync(agreement).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a transportation agreement by ID.
        /// </summary>
        public async Task<TransportationAgreement?> GetTransportationAgreementAsync(string agreementId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(agreementId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AGREEMENT_ID", Operator = "=", FilterValue = agreementId }
            };

            var results = await dataSource.GetEntityAsync(TRANSPORTATION_AGREEMENT_TABLE, filters);
            var agreementData = results?.FirstOrDefault();
            
            if (agreementData == null)
                return null;

            return agreementData as TransportationAgreement;
        }

        /// <summary>
        /// Gets a transportation agreement by ID (synchronous wrapper).
        /// </summary>
        public TransportationAgreement? GetTransportationAgreement(string agreementId)
        {
            return GetTransportationAgreementAsync(agreementId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Calculates total working interest for a lease.
        /// </summary>
        public decimal CalculateTotalWorkingInterest(string leaseId)
        {
            var lease = GetLease(leaseId);
            if (lease == null)
                return 0;

            if (lease is JointInterestLease jointLease)
            {
                return jointLease.Participants.Sum(p => p.WorkingInterest);
            }

            return lease.WorkingInterest;
        }

        /// <summary>
        /// Calculates total net revenue interest for a lease.
        /// </summary>
        public decimal CalculateTotalNetRevenueInterest(string leaseId)
        {
            var lease = GetLease(leaseId);
            if (lease == null)
                return 0;

            if (lease is JointInterestLease jointLease)
            {
                return jointLease.Participants.Sum(p => p.NetRevenueInterest);
            }

            return lease.NetRevenueInterest;
        }

        #region Helper Methods - Model to Dictionary Conversion

        /// <summary>
        /// Converts LeaseAgreement to dictionary for database storage.
        /// </summary>
        private Dictionary<string, object> ConvertLeaseToDictionary(LeaseAgreement lease)
        {
            var dict = new Dictionary<string, object>
            {
                { "LEASE_ID", lease.LeaseId },
                { "LEASE_NAME", lease.LeaseName ?? string.Empty },
                { "LEASE_TYPE", lease.LeaseType.ToString() },
                { "EFFECTIVE_DATE", lease.EffectiveDate },
                { "EXPIRATION_DATE", lease.ExpirationDate ?? (object)DBNull.Value },
                { "PRIMARY_TERM_MONTHS", lease.PrimaryTermMonths },
                { "WORKING_INTEREST", lease.WorkingInterest },
                { "NET_REVENUE_INTEREST", lease.NetRevenueInterest },
                { "ROYALTY_RATE", lease.RoyaltyRate },
                { "IS_HELD_BY_PRODUCTION", lease.Provisions?.IsHeldByProduction ?? false },
                { "SHUT_IN_ROYALTY_IND", lease.Provisions?.IsShutInRoyalty ?? false },
                { "SHUT_IN_ROYALTY_RATE", lease.Provisions?.ShutInRoyaltyRate ?? (object)DBNull.Value },
                { "STATE", lease.Location?.State ?? string.Empty },
                { "COUNTY", lease.Location?.County ?? string.Empty },
                { "TOWNSHIP", lease.Location?.Township ?? string.Empty },
                { "RANGE", lease.Location?.Range ?? string.Empty },
                { "SECTION", lease.Location?.Section ?? string.Empty }
            };

            // Handle lease type-specific properties
            if (lease is JointInterestLease jointLease)
            {
                // Store participants as JSON or in separate table (simplified here)
                dict["IS_JOINT_INTEREST"] = true;
            }
            else if (lease is FeeMineralLease feeLease)
            {
                dict["MINERAL_OWNER"] = feeLease.MineralOwner ?? string.Empty;
            }
            else if (lease is GovernmentLease govLease)
            {
                dict["GOVERNMENT_AGENCY"] = govLease.GovernmentAgency ?? string.Empty;
                dict["LEASE_NUMBER"] = govLease.LeaseNumber ?? string.Empty;
            }

            return dict;
        }

        /// <summary>
        /// Converts dictionary to LeaseAgreement.
        /// </summary>
        private LeaseAgreement? ConvertDictionaryToLease(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("LEASE_ID"))
                return null;

            var leaseTypeStr = dict.ContainsKey("LEASE_TYPE") ? dict["LEASE_TYPE"]?.ToString() : "Fee";
            if (!Enum.TryParse<LeaseType>(leaseTypeStr, out var leaseType))
                leaseType = LeaseType.Fee;

            LeaseAgreement lease = leaseType switch
            {
                LeaseType.JointInterest => new JointInterestLease(),
                LeaseType.Government => new GovernmentLease
                {
                    GovernmentAgency = dict.ContainsKey("GOVERNMENT_AGENCY") ? dict["GOVERNMENT_AGENCY"]?.ToString() : string.Empty,
                    LeaseNumber = dict.ContainsKey("LEASE_NUMBER") ? dict["LEASE_NUMBER"]?.ToString() : string.Empty
                },
                LeaseType.NetProfit => new NetProfitLease(),
                _ => new FeeMineralLease
                {
                    MineralOwner = dict.ContainsKey("MINERAL_OWNER") ? dict["MINERAL_OWNER"]?.ToString() : string.Empty
                }
            };

            lease.LeaseId = dict["LEASE_ID"]?.ToString() ?? string.Empty;
            lease.LeaseName = dict.ContainsKey("LEASE_NAME") ? dict["LEASE_NAME"]?.ToString() ?? string.Empty : string.Empty;
            lease.LeaseType = leaseType;
            lease.EffectiveDate = dict.ContainsKey("EFFECTIVE_DATE") && dict["EFFECTIVE_DATE"] != DBNull.Value 
                ? Convert.ToDateTime(dict["EFFECTIVE_DATE"]) 
                : DateTime.MinValue;
            lease.ExpirationDate = dict.ContainsKey("EXPIRATION_DATE") && dict["EXPIRATION_DATE"] != DBNull.Value
                ? Convert.ToDateTime(dict["EXPIRATION_DATE"])
                : null;
            lease.PrimaryTermMonths = dict.ContainsKey("PRIMARY_TERM_MONTHS") ? Convert.ToInt32(dict["PRIMARY_TERM_MONTHS"]) : 0;
            lease.WorkingInterest = dict.ContainsKey("WORKING_INTEREST") ? Convert.ToDecimal(dict["WORKING_INTEREST"]) : 0m;
            lease.NetRevenueInterest = dict.ContainsKey("NET_REVENUE_INTEREST") ? Convert.ToDecimal(dict["NET_REVENUE_INTEREST"]) : 0m;
            lease.RoyaltyRate = dict.ContainsKey("ROYALTY_RATE") ? Convert.ToDecimal(dict["ROYALTY_RATE"]) : 0m;

            lease.Provisions = new LeaseProvisions
            {
                IsHeldByProduction = dict.ContainsKey("IS_HELD_BY_PRODUCTION") && Convert.ToBoolean(dict["IS_HELD_BY_PRODUCTION"]),
                IsShutInRoyalty = dict.ContainsKey("SHUT_IN_ROYALTY_IND") && Convert.ToBoolean(dict["SHUT_IN_ROYALTY_IND"]),
                ShutInRoyaltyRate = dict.ContainsKey("SHUT_IN_ROYALTY_RATE") && dict["SHUT_IN_ROYALTY_RATE"] != DBNull.Value
                    ? Convert.ToDecimal(dict["SHUT_IN_ROYALTY_RATE"])
                    : null
            };

            lease.Location = new LeaseLocation
            {
                State = dict.ContainsKey("STATE") ? dict["STATE"]?.ToString() ?? string.Empty : string.Empty,
                County = dict.ContainsKey("COUNTY") ? dict["COUNTY"]?.ToString() ?? string.Empty : string.Empty,
                Township = dict.ContainsKey("TOWNSHIP") ? dict["TOWNSHIP"]?.ToString() ?? string.Empty : string.Empty,
                Range = dict.ContainsKey("RANGE") ? dict["RANGE"]?.ToString() ?? string.Empty : string.Empty,
                Section = dict.ContainsKey("SECTION") ? dict["SECTION"]?.ToString() ?? string.Empty : string.Empty
            };

            return lease;
        }

        /// <summary>
        /// Converts OilSalesAgreement to dictionary.
        /// </summary>
        private Dictionary<string, object> ConvertSalesAgreementToDictionary(OilSalesAgreement agreement)
        {
            return new Dictionary<string, object>
            {
                { "AGREEMENT_ID", agreement.AgreementId },
                { "AGREEMENT_NAME", agreement.AgreementName ?? string.Empty },
                { "SELLER", agreement.Seller ?? string.Empty },
                { "PURCHASER", agreement.Purchaser ?? string.Empty },
                { "EFFECTIVE_DATE", agreement.EffectiveDate },
                { "EXPIRATION_DATE", agreement.ExpirationDate ?? (object)DBNull.Value },
                { "PRICING_METHOD", agreement.PricingTerms?.PricingMethod.ToString() ?? string.Empty },
                { "BASE_PRICE", agreement.PricingTerms?.BasePrice ?? (object)DBNull.Value },
                { "PRICE_INDEX", agreement.PricingTerms?.PriceIndex ?? string.Empty },
                { "DIFFERENTIAL", agreement.PricingTerms?.Differential ?? 0m },
                { "DELIVERY_POINT", agreement.DeliveryTerms?.DeliveryPoint ?? string.Empty },
                { "DELIVERY_METHOD", agreement.DeliveryTerms?.DeliveryMethod ?? string.Empty },
                { "MINIMUM_VOLUME_COMMITMENT", agreement.MinimumVolumeCommitment ?? (object)DBNull.Value },
                { "MAXIMUM_VOLUME_COMMITMENT", agreement.MaximumVolumeCommitment ?? (object)DBNull.Value },
                { "PAYMENT_TERMS_DAYS", agreement.PaymentTermsDays }
            };
        }

        /// <summary>
        /// Converts dictionary to OilSalesAgreement.
        /// </summary>
        private OilSalesAgreement ConvertDictionaryToSalesAgreement(Dictionary<string, object> dict)
        {
            var agreement = new OilSalesAgreement
            {
                AgreementId = dict["AGREEMENT_ID"]?.ToString() ?? string.Empty,
                AgreementName = dict.ContainsKey("AGREEMENT_NAME") ? dict["AGREEMENT_NAME"]?.ToString() ?? string.Empty : string.Empty,
                Seller = dict.ContainsKey("SELLER") ? dict["SELLER"]?.ToString() ?? string.Empty : string.Empty,
                Purchaser = dict.ContainsKey("PURCHASER") ? dict["PURCHASER"]?.ToString() ?? string.Empty : string.Empty,
                EffectiveDate = dict.ContainsKey("EFFECTIVE_DATE") && dict["EFFECTIVE_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["EFFECTIVE_DATE"])
                    : DateTime.MinValue,
                ExpirationDate = dict.ContainsKey("EXPIRATION_DATE") && dict["EXPIRATION_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["EXPIRATION_DATE"])
                    : null,
                PaymentTermsDays = dict.ContainsKey("PAYMENT_TERMS_DAYS") ? Convert.ToInt32(dict["PAYMENT_TERMS_DAYS"]) : 30
            };

            if (dict.ContainsKey("MINIMUM_VOLUME_COMMITMENT") && dict["MINIMUM_VOLUME_COMMITMENT"] != DBNull.Value)
                agreement.MinimumVolumeCommitment = Convert.ToDecimal(dict["MINIMUM_VOLUME_COMMITMENT"]);
            if (dict.ContainsKey("MAXIMUM_VOLUME_COMMITMENT") && dict["MAXIMUM_VOLUME_COMMITMENT"] != DBNull.Value)
                agreement.MaximumVolumeCommitment = Convert.ToDecimal(dict["MAXIMUM_VOLUME_COMMITMENT"]);

            agreement.PricingTerms = new PricingTerms();
            if (dict.ContainsKey("PRICING_METHOD") && Enum.TryParse<PricingMethod>(dict["PRICING_METHOD"]?.ToString(), out var method))
                agreement.PricingTerms.PricingMethod = method;
            if (dict.ContainsKey("BASE_PRICE") && dict["BASE_PRICE"] != DBNull.Value)
                agreement.PricingTerms.BasePrice = Convert.ToDecimal(dict["BASE_PRICE"]);
            if (dict.ContainsKey("PRICE_INDEX"))
                agreement.PricingTerms.PriceIndex = dict["PRICE_INDEX"]?.ToString();
            if (dict.ContainsKey("DIFFERENTIAL"))
                agreement.PricingTerms.Differential = Convert.ToDecimal(dict["DIFFERENTIAL"]);

            agreement.DeliveryTerms = new DeliveryTerms
            {
                DeliveryPoint = dict.ContainsKey("DELIVERY_POINT") ? dict["DELIVERY_POINT"]?.ToString() ?? string.Empty : string.Empty,
                DeliveryMethod = dict.ContainsKey("DELIVERY_METHOD") ? dict["DELIVERY_METHOD"]?.ToString() ?? string.Empty : string.Empty
            };

            return agreement;
        }

        /// <summary>
        /// Converts TransportationAgreement to dictionary.
        /// </summary>
        private Dictionary<string, object> ConvertTransportationAgreementToDictionary(TransportationAgreement agreement)
        {
            return new Dictionary<string, object>
            {
                { "AGREEMENT_ID", agreement.AgreementId },
                { "CARRIER", agreement.Carrier ?? string.Empty },
                { "ORIGIN_POINT", agreement.OriginPoint ?? string.Empty },
                { "DESTINATION_POINT", agreement.DestinationPoint ?? string.Empty },
                { "EFFECTIVE_DATE", agreement.EffectiveDate },
                { "EXPIRATION_DATE", agreement.ExpirationDate ?? (object)DBNull.Value },
                { "TARIFF_RATE", agreement.TariffRate },
                { "MINIMUM_VOLUME_COMMITMENT", agreement.MinimumVolumeCommitment ?? (object)DBNull.Value },
                { "MAXIMUM_CAPACITY", agreement.MaximumCapacity ?? (object)DBNull.Value }
            };
        }

        /// <summary>
        /// Converts dictionary to TransportationAgreement.
        /// </summary>
        private TransportationAgreement ConvertDictionaryToTransportationAgreement(Dictionary<string, object> dict)
        {
            var agreement = new TransportationAgreement
            {
                AgreementId = dict["AGREEMENT_ID"]?.ToString() ?? string.Empty,
                Carrier = dict.ContainsKey("CARRIER") ? dict["CARRIER"]?.ToString() ?? string.Empty : string.Empty,
                OriginPoint = dict.ContainsKey("ORIGIN_POINT") ? dict["ORIGIN_POINT"]?.ToString() ?? string.Empty : string.Empty,
                DestinationPoint = dict.ContainsKey("DESTINATION_POINT") ? dict["DESTINATION_POINT"]?.ToString() ?? string.Empty : string.Empty,
                EffectiveDate = dict.ContainsKey("EFFECTIVE_DATE") && dict["EFFECTIVE_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["EFFECTIVE_DATE"])
                    : DateTime.MinValue,
                ExpirationDate = dict.ContainsKey("EXPIRATION_DATE") && dict["EXPIRATION_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["EXPIRATION_DATE"])
                    : null,
                TariffRate = dict.ContainsKey("TARIFF_RATE") ? Convert.ToDecimal(dict["TARIFF_RATE"]) : 0m
            };

            if (dict.ContainsKey("MINIMUM_VOLUME_COMMITMENT") && dict["MINIMUM_VOLUME_COMMITMENT"] != DBNull.Value)
                agreement.MinimumVolumeCommitment = Convert.ToDecimal(dict["MINIMUM_VOLUME_COMMITMENT"]);
            if (dict.ContainsKey("MAXIMUM_CAPACITY") && dict["MAXIMUM_CAPACITY"] != DBNull.Value)
                agreement.MaximumCapacity = Convert.ToDecimal(dict["MAXIMUM_CAPACITY"]);

            return agreement;
        }

        #endregion
    }
}

