using Beep.OilandGas.Models.Data.Unitization;
using Beep.OilandGas.Models.DTOs.Unitization;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Unitization
{
    /// <summary>
    /// Service for managing unit agreements, participating areas, and tract participations.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class UnitizationService : IUnitizationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<UnitizationService>? _logger;
        private readonly string _connectionName;
        private const string UNIT_AGREEMENT_TABLE = "UNIT_AGREEMENT";
        private const string PARTICIPATING_AREA_TABLE = "PARTICIPATING_AREA";
        private const string TRACT_PARTICIPATION_TABLE = "TRACT_PARTICIPATION";

        public UnitizationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<UnitizationService>? logger = null,
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
        /// Creates a unit agreement.
        /// </summary>
        public async Task<UNIT_AGREEMENT> CreateUnitAgreementAsync(CreateUnitAgreementRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.UnitName))
                throw new ArgumentException("Unit name is required.", nameof(request));
            if (string.IsNullOrEmpty(request.UnitOperatorBaId))
                throw new ArgumentException("Unit operator is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = await GetUnitAgreementRepositoryAsync(connName);

            var agreement = new UNIT_AGREEMENT
            {
                UNIT_AGREEMENT_ID = Guid.NewGuid().ToString(),
                UNIT_NAME = request.UnitName,
                UNIT_NUMBER = request.UnitNumber,
                UNIT_OPERATOR_BA_ID = request.UnitOperatorBaId,
                EFFECTIVE_DATE = request.EffectiveDate,
                EXPIRY_DATE = request.ExpiryDate,
                STATUS = "Pending",
                TERMS_AND_CONDITIONS = request.TermsAndConditions,
                ACTIVE_IND = "Y"
            };

            if (agreement is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(agreement);
            _logger?.LogDebug("Created unit agreement {UnitName}", request.UnitName);

            return agreement;
        }

        /// <summary>
        /// Gets a unit agreement by ID.
        /// </summary>
        public async Task<UNIT_AGREEMENT?> GetUnitAgreementAsync(string agreementId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(agreementId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = await GetUnitAgreementRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UNIT_AGREEMENT_ID", Operator = "=", FilterValue = agreementId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<UNIT_AGREEMENT>().FirstOrDefault();
        }

        /// <summary>
        /// Gets all unit agreements.
        /// </summary>
        public async Task<List<UNIT_AGREEMENT>> GetUnitAgreementsAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = await GetUnitAgreementRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<UNIT_AGREEMENT>().OrderByDescending(u => u.EFFECTIVE_DATE).ToList();
        }

        /// <summary>
        /// Creates a participating area.
        /// </summary>
        public async Task<PARTICIPATING_AREA> CreateParticipatingAreaAsync(CreateParticipatingAreaRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.UnitAgreementId))
                throw new ArgumentException("Unit agreement ID is required.", nameof(request));
            if (string.IsNullOrEmpty(request.AreaName))
                throw new ArgumentException("Area name is required.", nameof(request));

            // Verify unit agreement exists
            var unitAgreement = await GetUnitAgreementAsync(request.UnitAgreementId, connectionName);
            if (unitAgreement == null)
                throw new InvalidOperationException($"Unit agreement {request.UnitAgreementId} not found.");

            var connName = connectionName ?? _connectionName;
            var repo = await GetParticipatingAreaRepositoryAsync(connName);

            var area = new PARTICIPATING_AREA
            {
                PARTICIPATING_AREA_ID = Guid.NewGuid().ToString(),
                UNIT_AGREEMENT_ID = request.UnitAgreementId,
                AREA_NAME = request.AreaName,
                EFFECTIVE_DATE = request.EffectiveDate,
                EXPIRY_DATE = request.ExpiryDate,
                ACTIVE_IND = "Y"
            };

            if (area is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(area);
            _logger?.LogDebug("Created participating area {AreaName} for unit {UnitId}", request.AreaName, request.UnitAgreementId);

            return area;
        }

        /// <summary>
        /// Gets participating areas by unit.
        /// </summary>
        public async Task<List<PARTICIPATING_AREA>> GetParticipatingAreasByUnitAsync(string unitId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(unitId))
                return new List<PARTICIPATING_AREA>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetParticipatingAreaRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UNIT_AGREEMENT_ID", Operator = "=", FilterValue = unitId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<PARTICIPATING_AREA>().OrderBy(a => a.EFFECTIVE_DATE).ToList();
        }

        /// <summary>
        /// Registers tract participation.
        /// </summary>
        public async Task<TRACT_PARTICIPATION> RegisterTractParticipationAsync(CreateTractParticipationRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.ParticipationPercentage < 0 || request.ParticipationPercentage > 100)
                throw new ArgumentException("Participation percentage must be between 0 and 100.", nameof(request));
            if (request.WorkingInterest < 0 || request.WorkingInterest > 1)
                throw new ArgumentException("Working interest must be between 0 and 1.", nameof(request));
            if (request.NetRevenueInterest < 0 || request.NetRevenueInterest > 1)
                throw new ArgumentException("Net revenue interest must be between 0 and 1.", nameof(request));

            var connName = connectionName ?? _connectionName;

            // Verify participating area exists
            var areaRepo = await GetParticipatingAreaRepositoryAsync(connName);
            var areaFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PARTICIPATING_AREA_ID", Operator = "=", FilterValue = request.ParticipatingAreaId }
            };

            var areaResults = await areaRepo.GetAsync(areaFilters);
            if (!areaResults.Any())
                throw new InvalidOperationException($"Participating area {request.ParticipatingAreaId} not found.");

            // Validate total participation doesn't exceed 100%
            var participationRepo = await GetTractParticipationRepositoryAsync(connName);
            var participationFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PARTICIPATING_AREA_ID", Operator = "=", FilterValue = request.ParticipatingAreaId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var existingParticipations = await participationRepo.GetAsync(participationFilters);
            var totalParticipation = existingParticipations.Cast<TRACT_PARTICIPATION>()
                .Sum(p => p.PARTICIPATION_PERCENTAGE ?? 0m) + request.ParticipationPercentage;

            if (totalParticipation > 100.01m)
                throw new InvalidOperationException($"Total participation would exceed 100%: {totalParticipation}%");

            var participation = new TRACT_PARTICIPATION
            {
                TRACT_PARTICIPATION_ID = Guid.NewGuid().ToString(),
                PARTICIPATING_AREA_ID = request.ParticipatingAreaId,
                UNIT_AGREEMENT_ID = request.UnitAgreementId,
                TRACT_ID = request.TractId,
                PARTICIPATION_PERCENTAGE = request.ParticipationPercentage,
                WORKING_INTEREST = request.WorkingInterest,
                NET_REVENUE_INTEREST = request.NetRevenueInterest,
                TRACT_ACREAGE = request.TractAcreage,
                ACTIVE_IND = "Y"
            };

            if (participation is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await participationRepo.InsertAsync(participation);
            _logger?.LogDebug("Registered tract participation for tract {TractId} in area {AreaId}", request.TractId, request.ParticipatingAreaId);

            return participation;
        }

        /// <summary>
        /// Gets tract participations by area.
        /// </summary>
        public async Task<List<TRACT_PARTICIPATION>> GetTractParticipationsByAreaAsync(string areaId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(areaId))
                return new List<TRACT_PARTICIPATION>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetTractParticipationRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PARTICIPATING_AREA_ID", Operator = "=", FilterValue = areaId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<TRACT_PARTICIPATION>().OrderBy(p => p.PARTICIPATION_PERCENTAGE).ToList();
        }

        /// <summary>
        /// Approves a unit agreement.
        /// </summary>
        public async Task<UnitApprovalResult> ApproveUnitAgreementAsync(string agreementId, string approverId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(agreementId))
                throw new ArgumentException("Agreement ID is required.", nameof(agreementId));
            if (string.IsNullOrEmpty(approverId))
                throw new ArgumentException("Approver ID is required.", nameof(approverId));

            var connName = connectionName ?? _connectionName;
            var agreement = await GetUnitAgreementAsync(agreementId, connName);

            if (agreement == null)
                throw new InvalidOperationException($"Unit agreement {agreementId} not found.");

            agreement.STATUS = "Approved";
            agreement.APPROVAL_DATE = DateTime.UtcNow;
            agreement.APPROVED_BY = approverId;

            if (agreement is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, approverId, connName);
            }

            var repo = await GetUnitAgreementRepositoryAsync(connName);
            await repo.UpdateAsync(agreement);

            _logger?.LogDebug("Approved unit agreement {AgreementId} by {ApproverId}", agreementId, approverId);

            return new UnitApprovalResult
            {
                AgreementId = agreementId,
                IsApproved = true,
                ApproverId = approverId,
                ApprovalDate = DateTime.UtcNow,
                Status = "Approved"
            };
        }

        /// <summary>
        /// Gets unit operations summary.
        /// </summary>
        public async Task<UnitOperationsSummary> GetUnitOperationsSummaryAsync(string unitId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(unitId))
                throw new ArgumentException("Unit ID is required.", nameof(unitId));

            var connName = connectionName ?? _connectionName;
            var agreement = await GetUnitAgreementAsync(unitId, connName);

            if (agreement == null)
                throw new InvalidOperationException($"Unit agreement {unitId} not found.");

            var areas = await GetParticipatingAreasByUnitAsync(unitId, connName);
            var allTracts = new List<TRACT_PARTICIPATION>();

            foreach (var area in areas)
            {
                var tracts = await GetTractParticipationsByAreaAsync(area.PARTICIPATING_AREA_ID ?? string.Empty, connName);
                allTracts.AddRange(tracts);
            }

            var summary = new UnitOperationsSummary
            {
                UnitAgreementId = unitId,
                UnitName = agreement.UNIT_NAME ?? string.Empty,
                ParticipatingAreaCount = areas.Count,
                TractCount = allTracts.Count,
                TotalParticipationPercentage = allTracts.Sum(t => t.PARTICIPATION_PERCENTAGE ?? 0m),
                TotalWorkingInterest = allTracts.Sum(t => t.WORKING_INTEREST ?? 0m),
                TotalNetRevenueInterest = allTracts.Sum(t => t.NET_REVENUE_INTEREST ?? 0m)
            };

            return summary;
        }

        /// <summary>
        /// Gets agreements requiring approval.
        /// </summary>
        public async Task<List<UNIT_AGREEMENT>> GetAgreementsRequiringApprovalAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = await GetUnitAgreementRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = "Pending" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<UNIT_AGREEMENT>().OrderByDescending(u => u.EFFECTIVE_DATE).ToList();
        }

        // Repository helper methods
        private async Task<PPDMGenericRepository> GetUnitAgreementRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(UNIT_AGREEMENT_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Unitization.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(UNIT_AGREEMENT);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, UNIT_AGREEMENT_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetParticipatingAreaRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(PARTICIPATING_AREA_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Unitization.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(PARTICIPATING_AREA);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, PARTICIPATING_AREA_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetTractParticipationRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(TRACT_PARTICIPATION_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Unitization.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(TRACT_PARTICIPATION);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, TRACT_PARTICIPATION_TABLE,
                null);
        }
    }
}
