using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.DTOs.Lease;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.DataBase;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LeaseAcquisition.Services
{
    /// <summary>
    /// Service for lease acquisition operations.
    /// Uses PPDMGenericRepository for data persistence following LifeCycle patterns.
    /// </summary>
    public class LeaseAcquisitionService : ILeaseAcquisitionService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<LeaseAcquisitionService>? _logger;

        private PPDMGenericRepository? _leaseRepository;

        public LeaseAcquisitionService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<LeaseAcquisitionService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        private async Task<PPDMGenericRepository> GetLeaseRepositoryAsync()
        {
            if (_leaseRepository == null)
            {
                // Use LAND_RIGHT table from PPDM for leases
                _leaseRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    typeof(LAND_RIGHT),
                    _connectionName,
                    "LAND_RIGHT",
                    null);
            }
            return _leaseRepository;
        }

        public async Task<LeaseSummary> EvaluateLeaseAsync(string leaseId)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentException("Lease ID cannot be null or empty", nameof(leaseId));

            _logger?.LogInformation("Evaluating lease {LeaseId}", leaseId);

            var leaseRepo = await GetLeaseRepositoryAsync();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LAND_RIGHT_ID", FilterValue = leaseId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            var leaseEntities = await leaseRepo.GetAsync(filters);
            var leases = leaseEntities.Cast<LAND_RIGHT>().ToList();
            var lease = leases.FirstOrDefault();

            if (lease == null)
                throw new KeyNotFoundException($"Lease with ID {leaseId} not found.");

            // TODO: Implement comprehensive lease evaluation
            var evaluation = new LeaseSummary
            {
                LeaseId = lease.LAND_RIGHT_ID ?? string.Empty,
                LeaseNumber = lease.LAND_RIGHT_ID ?? string.Empty,
                LeaseName = lease.REMARK ?? string.Empty,
                IsActive = lease.ACTIVE_IND == "Y",
                EffectiveDate = lease.ROW_EFFECTIVE_DATE,
                ExpirationDate = lease.ROW_EXPIRY_DATE
            };

            _logger?.LogInformation("Lease evaluation completed for {LeaseId}", leaseId);
            return evaluation;
        }

        public async Task<List<LeaseSummary>> GetAvailableLeasesAsync(Dictionary<string, string>? filters = null)
        {
            _logger?.LogInformation("Getting available leases");

            var leaseRepo = await GetLeaseRepositoryAsync();
            var appFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    appFilters.Add(new AppFilter { FieldName = filter.Key, FilterValue = filter.Value, Operator = "=" });
                }
            }

            var leaseEntities = await leaseRepo.GetAsync(appFilters);
            var leases = leaseEntities.Cast<LAND_RIGHT>().ToList();

            var result = leases.Select(l => new LeaseSummary
            {
                LeaseId = l.LAND_RIGHT_ID ?? string.Empty,
                LeaseNumber = l.LAND_RIGHT_ID ?? string.Empty,
                LeaseName = l.REMARK ?? string.Empty,
                IsActive = l.ACTIVE_IND == "Y",
                EffectiveDate = l.ROW_EFFECTIVE_DATE,
                ExpirationDate = l.ROW_EXPIRY_DATE
            }).ToList();

            _logger?.LogInformation("Retrieved {Count} available leases", result.Count);
            return result;
        }

        public async Task<string> CreateLeaseAcquisitionAsync(CreateLeaseAcquisitionDto leaseRequest, string userId)
        {
            if (leaseRequest == null)
                throw new ArgumentNullException(nameof(leaseRequest));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Creating lease acquisition: Type={LeaseType}, Number={LeaseNumber}", leaseRequest.LeaseType, leaseRequest.LeaseNumber);

            var leaseRepo = await GetLeaseRepositoryAsync();
            var lease = new LAND_RIGHT
            {
                LAND_RIGHT_ID = _defaults.FormatIdForTable("LAND_RIGHT", Guid.NewGuid().ToString()),
                ACTIVE_IND = "Y",
                REMARK = leaseRequest.LeaseName,
                ROW_EFFECTIVE_DATE = leaseRequest.EffectiveDate,
                ROW_EXPIRY_DATE = leaseRequest.ExpirationDate ?? DateTime.MaxValue
            };

            await leaseRepo.InsertAsync(lease, userId);

            _logger?.LogInformation("Lease acquisition created: {LeaseId}", lease.LAND_RIGHT_ID);
            return lease.LAND_RIGHT_ID ?? string.Empty;
        }

        public async Task UpdateLeaseStatusAsync(string leaseId, string status, string userId)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentException("Lease ID cannot be null or empty", nameof(leaseId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Updating lease status for {LeaseId} to {Status}", leaseId, status);

            var leaseRepo = await GetLeaseRepositoryAsync();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LAND_RIGHT_ID", FilterValue = leaseId, Operator = "=" }
            };

            var leaseEntities = await leaseRepo.GetAsync(filters);
            var leases = leaseEntities.Cast<LAND_RIGHT>().ToList();
            var lease = leases.FirstOrDefault();

            if (lease == null)
                throw new KeyNotFoundException($"Lease with ID {leaseId} not found.");

            // Update status based on status string
            if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                lease.ACTIVE_IND = "Y";
            else if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                lease.ACTIVE_IND = "N";

            await leaseRepo.UpdateAsync(lease, userId);

            _logger?.LogInformation("Lease status updated for {LeaseId}", leaseId);
        }
    }
}

