
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using OilSalesAgreementDto = Beep.OilandGas.Models.DTOs.ProductionAccounting.OilSalesAgreementDto;
using TransportationAgreementDto = Beep.OilandGas.Models.DTOs.ProductionAccounting.TransportationAgreementDto;
using PricingTermsDto = Beep.OilandGas.Models.DTOs.ProductionAccounting.PricingTermsDto;
using DeliveryTermsDto = Beep.OilandGas.Models.DTOs.ProductionAccounting.DeliveryTermsDto;
using Beep.OilandGas.ProductionAccounting.Validation;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.Agreement;

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
            ILogger<LeaseManager>? logger = null,
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
using Beep.OilandGas.Models.Data.ProductionAccounting;

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
        public async Task RegisterSalesAgreementAsync(OilSalesAgreementDto agreement, string userId = "system", string? connectionName = null)
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
        public async Task<OilSalesAgreementDto?> GetSalesAgreementAsync(string agreementId, string? connectionName = null)
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
            return results?.FirstOrDefault() as SALES_AGREEMENT;
        }

        /// <summary>
        /// Gets a sales agreement by ID (synchronous wrapper).
        /// </summary>
        public SALES_AGREEMENT? GetSalesAgreement(string agreementId)
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
        public void RegisterTransportationAgreement(TransportationAgreementDto agreement)
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
            return results?.FirstOrDefault() as TRANSPORTATION_AGREEMENT;
        }

        /// <summary>
        /// Gets a transportation agreement by ID (synchronous wrapper).
        /// </summary>
        public TRANSPORTATION_AGREEMENT? GetTransportationAgreement(string agreementId)
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

    }
}
