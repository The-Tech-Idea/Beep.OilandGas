using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LeaseAcquisition.Services
{
    public partial class LeaseAcquisitionService
    {
        // Explicit implementations of Models.Core.Interfaces.ILeaseAcquisitionService

        async Task<LeaseSummary> Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService.EvaluateLeaseAsync(
            string leaseId)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));

            var storedLease = await GetLeaseSummaryAsync(leaseId);
            var evaluation = await EvaluateLeaseOpportunityAsync(
                leaseId,
                new LeaseEvaluationRequest { LeaseId = leaseId, IncludeFinancialAnalysis = true });

            return new LeaseSummary
            {
                LeaseId     = leaseId,
                LeaseNumber = storedLease?.LeaseNumber ?? leaseId,
                LeaseName   = storedLease?.LeaseName ?? evaluation.EvaluationSummary,
                Status      = evaluation.RecommendationStatus,
                LeaseType   = storedLease?.LeaseType ?? string.Empty,
                FieldId     = storedLease?.FieldId ?? string.Empty,
                EffectiveDate = storedLease?.EffectiveDate,
                ExpirationDate = storedLease?.ExpirationDate,
                PrimaryTermMonths = storedLease?.PrimaryTermMonths,
                WorkingInterest = storedLease?.WorkingInterest ?? 0m
            };
        }

        async Task<List<LeaseSummary>> Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService.GetAvailableLeasesAsync(
            Dictionary<string, string>? filters)
        {
            return await GetLeaseSummariesAsync(filters);
        }

        async Task<string> Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService.CreateLeaseAcquisitionAsync(
            CreateLeaseAcquisition leaseRequest, string userId)
        {
            if (leaseRequest == null)
                throw new ArgumentNullException(nameof(leaseRequest));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var leaseId = _defaults.FormatIdForTable("LEASE", Guid.NewGuid().ToString().Substring(0, 12));
            var effectiveDate = ResolveLeaseEffectiveDate(leaseRequest);
            var leaseName = ResolveLeaseName(leaseRequest, leaseId);
            var leaseNumber = ResolveLeaseNumber(leaseRequest, leaseId);
            var leaseType = NormalizeLeaseType(leaseRequest.LeaseType);
            var locationId = leaseRequest.PropertyId?.Trim() ?? string.Empty;

            var landRightRepo = CreateLandRightRepository();
            var landAgreementRepo = CreateLandAgreementRepository();
            var landStatusRepo = CreateLandStatusRepository();

            var landRight = new LAND_RIGHT
            {
                LAND_RIGHT_SUBTYPE = LeaseRecordSubtype,
                LAND_RIGHT_ID = leaseId,
                ACTIVE_IND = ActiveIndicator,
                ACQTN_DATE = effectiveDate,
                AREA_ID = locationId,
                AREA_TYPE = string.IsNullOrWhiteSpace(locationId) ? string.Empty : FieldAreaType,
                EFFECTIVE_DATE = effectiveDate,
                EXPIRY_DATE = leaseRequest.ExpirationDate,
                CASE_SERIAL_NUM = leaseNumber,
                CALCULATED_INTEREST = leaseRequest.WorkingInterest,
                REMARK = leaseName
            };

            var landAgreement = new LAND_AGREEMENT
            {
                LAND_RIGHT_SUBTYPE = LeaseRecordSubtype,
                LAND_RIGHT_ID = leaseId,
                ACTIVE_IND = ActiveIndicator,
                EFFECTIVE_DATE = effectiveDate,
                EXPIRY_DATE = leaseRequest.ExpirationDate,
                LAND_AGREE_TYPE = leaseType,
                REMARK = leaseName
            };

            var landStatus = new LAND_STATUS
            {
                LAND_RIGHT_SUBTYPE = LeaseRecordSubtype,
                LAND_RIGHT_ID = leaseId,
                STATUS_TYPE = OperationalStatusType,
                LAND_RIGHT_STATUS = ActiveStatus,
                STATUS_SEQ_NO = 1m,
                ACTIVE_IND = ActiveIndicator,
                EFFECTIVE_DATE = effectiveDate,
                EFFECTIVE_TERM = leaseRequest.PrimaryTermMonths?.ToString() ?? string.Empty,
                EFFECTIVE_TERM_OUOM = leaseRequest.PrimaryTermMonths.HasValue ? MonthsOuom : string.Empty,
                EXPIRY_DATE = leaseRequest.ExpirationDate,
                REMARK = leaseName
            };

            await landRightRepo.InsertAsync(landRight, userId);
            await landAgreementRepo.InsertAsync(landAgreement, userId);
            await landStatusRepo.InsertAsync(landStatus, userId);

            return leaseId;
        }

        async Task Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService.UpdateLeaseStatusAsync(
            string leaseId, string status, string userId)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentNullException(nameof(status));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var normalizedStatus = NormalizeLeaseStatus(status);
            var activeIndicator = normalizedStatus == ActiveStatus ? ActiveIndicator : InactiveIndicator;
            var landRightRepo = CreateLandRightRepository();
            var landAgreementRepo = CreateLandAgreementRepository();
            var landStatusRepo = CreateLandStatusRepository();

            var landRight = await landRightRepo.GetByIdAsync(leaseId) as LAND_RIGHT;
            if (landRight == null)
                throw new KeyNotFoundException($"Lease with ID {leaseId} was not found.");

            landRight.ACTIVE_IND = activeIndicator;
            await landRightRepo.UpdateAsync(landRight, userId);

            var agreementEntities = await landAgreementRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "LAND_RIGHT_ID", Operator = "=", FilterValue = leaseId },
                new AppFilter { FieldName = "LAND_RIGHT_SUBTYPE", Operator = "=", FilterValue = LeaseRecordSubtype }
            });

            var agreements = agreementEntities.OfType<LAND_AGREEMENT>().ToList();
            foreach (var agreement in agreements)
            {
                agreement.ACTIVE_IND = activeIndicator;
                await landAgreementRepo.UpdateAsync(agreement, userId);
            }

            var statusEntities = await landStatusRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "LAND_RIGHT_ID", Operator = "=", FilterValue = leaseId },
                new AppFilter { FieldName = "LAND_RIGHT_SUBTYPE", Operator = "=", FilterValue = LeaseRecordSubtype },
                new AppFilter { FieldName = "STATUS_TYPE", Operator = "=", FilterValue = OperationalStatusType }
            });

            var existingStatuses = statusEntities.OfType<LAND_STATUS>().ToList();
            var latestStatusWithTerm = existingStatuses
                .OrderByDescending(item => item.EFFECTIVE_DATE ?? DateTime.MinValue)
                .ThenByDescending(item => item.STATUS_SEQ_NO)
                .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item.EFFECTIVE_TERM));
            var statusRecord = new LAND_STATUS
            {
                LAND_RIGHT_ID = leaseId,
                LAND_RIGHT_SUBTYPE = LeaseRecordSubtype,
                STATUS_TYPE = OperationalStatusType,
                LAND_RIGHT_STATUS = normalizedStatus,
                STATUS_SEQ_NO = existingStatuses.Count == 0 ? 1m : existingStatuses.Max(item => item.STATUS_SEQ_NO) + 1m,
                EFFECTIVE_DATE = DateTime.UtcNow,
                EFFECTIVE_TERM = latestStatusWithTerm?.EFFECTIVE_TERM ?? string.Empty,
                EFFECTIVE_TERM_OUOM = latestStatusWithTerm?.EFFECTIVE_TERM_OUOM ?? string.Empty,
                EXPIRY_DATE = landRight.EXPIRY_DATE,
                ACTIVE_IND = ActiveIndicator,
                REMARK = agreements.FirstOrDefault()?.REMARK ?? landRight.REMARK ?? string.Empty
            };

            await landStatusRepo.InsertAsync(statusRecord, userId);
        }

        private const string LeaseRecordSubtype = "LEASE";
        private const string OperationalStatusType = "OPERATIONAL";
        private const string ActiveIndicator = "Y";
        private const string InactiveIndicator = "N";
        private const string ActiveStatus = "ACTIVE";
        private const string FieldAreaType = "FIELD";
        private const string MonthsOuom = "MONTH";

        private PPDMGenericRepository CreateLandRightRepository()
            => new(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LAND_RIGHT), _connectionName, "LAND_RIGHT", null);

        private PPDMGenericRepository CreateLandAgreementRepository()
            => new(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LAND_AGREEMENT), _connectionName, "LAND_AGREEMENT", null);

        private PPDMGenericRepository CreateLandStatusRepository()
            => new(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LAND_STATUS), _connectionName, "LAND_STATUS", null);

        private async Task<List<LeaseSummary>> GetLeaseSummariesAsync(Dictionary<string, string>? filters)
        {
            var landRightRepo = CreateLandRightRepository();
            var landAgreementRepo = CreateLandAgreementRepository();
            var landStatusRepo = CreateLandStatusRepository();
            var landRightFilters = BuildLandRightFilters(filters);
            var landRightEntities = await landRightRepo.GetAsync(landRightFilters);
            var landRights = landRightEntities.OfType<LAND_RIGHT>().ToList();

            if (landRights.Count == 0)
                return new List<LeaseSummary>();

            var leaseIds = landRights
                .Select(item => item.LAND_RIGHT_ID)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var agreementEntities = await landAgreementRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "LAND_RIGHT_SUBTYPE", Operator = "=", FilterValue = LeaseRecordSubtype }
            });
            var agreementsByLeaseId = agreementEntities
                .OfType<LAND_AGREEMENT>()
                .Where(item => !string.IsNullOrWhiteSpace(item.LAND_RIGHT_ID) && leaseIds.Contains(item.LAND_RIGHT_ID))
                .GroupBy(item => item.LAND_RIGHT_ID, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(item => item.EFFECTIVE_DATE ?? DateTime.MinValue)
                        .ThenByDescending(item => item.ROW_CHANGED_DATE ?? DateTime.MinValue)
                        .First(),
                    StringComparer.OrdinalIgnoreCase);

            var statusEntities = await landStatusRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "LAND_RIGHT_SUBTYPE", Operator = "=", FilterValue = LeaseRecordSubtype },
                new AppFilter { FieldName = "STATUS_TYPE", Operator = "=", FilterValue = OperationalStatusType }
            });
            var statusHistoryByLeaseId = statusEntities
                .OfType<LAND_STATUS>()
                .Where(item => !string.IsNullOrWhiteSpace(item.LAND_RIGHT_ID) && leaseIds.Contains(item.LAND_RIGHT_ID))
                .GroupBy(item => item.LAND_RIGHT_ID, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(item => item.EFFECTIVE_DATE ?? DateTime.MinValue)
                        .ThenByDescending(item => item.STATUS_SEQ_NO)
                        .ToList(),
                    StringComparer.OrdinalIgnoreCase);

            var summaries = landRights
                .Select(landRight =>
                {
                    agreementsByLeaseId.TryGetValue(landRight.LAND_RIGHT_ID ?? string.Empty, out var agreement);
                    statusHistoryByLeaseId.TryGetValue(landRight.LAND_RIGHT_ID ?? string.Empty, out var statusHistory);
                    var leaseStatus = statusHistory?.FirstOrDefault();

                    return new LeaseSummary
                    {
                        LeaseId = landRight.LAND_RIGHT_ID ?? string.Empty,
                        LeaseNumber = !string.IsNullOrWhiteSpace(landRight.CASE_SERIAL_NUM)
                            ? landRight.CASE_SERIAL_NUM
                            : landRight.LAND_RIGHT_ID ?? string.Empty,
                        LeaseName = ResolveLeaseSummaryName(landRight, agreement),
                        Status = ResolveLeaseSummaryStatus(landRight, agreement, leaseStatus),
                        LeaseType = agreement?.LAND_AGREE_TYPE ?? string.Empty,
                        FieldId = landRight.AREA_ID ?? string.Empty,
                        EffectiveDate = agreement?.EFFECTIVE_DATE ?? landRight.EFFECTIVE_DATE ?? landRight.ACQTN_DATE,
                        ExpirationDate = agreement?.EXPIRY_DATE ?? landRight.EXPIRY_DATE,
                        PrimaryTermMonths = ResolvePrimaryTermMonths(statusHistory),
                        WorkingInterest = landRight.CALCULATED_INTEREST
                    };
                })
                .ToList();

            return ApplySummaryFilters(summaries, filters)
                .OrderBy(item => item.LeaseNumber, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.LeaseName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<LeaseSummary?> GetLeaseSummaryAsync(string leaseId)
        {
            var summaries = await GetLeaseSummariesAsync(new Dictionary<string, string>
            {
                ["LeaseId"] = leaseId
            });

            return summaries.FirstOrDefault();
        }

        private static List<AppFilter> BuildLandRightFilters(Dictionary<string, string>? filters)
        {
            var appFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LAND_RIGHT_SUBTYPE", Operator = "=", FilterValue = LeaseRecordSubtype }
            };

            if (filters == null)
                return appFilters;

            foreach (var filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Value))
                    continue;

                var key = filter.Key.Trim();
                if (key.Equals("LeaseName", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("Status", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var fieldName = key switch
                {
                    var value when value.Equals("LeaseId", StringComparison.OrdinalIgnoreCase) => "LAND_RIGHT_ID",
                    var value when value.Equals("LeaseNumber", StringComparison.OrdinalIgnoreCase) => "CASE_SERIAL_NUM",
                    var value when value.Equals("PropertyId", StringComparison.OrdinalIgnoreCase) => "AREA_ID",
                    var value when value.Equals("FieldId", StringComparison.OrdinalIgnoreCase) => "AREA_ID",
                    _ => key
                };

                appFilters.Add(new AppFilter { FieldName = fieldName, Operator = "=", FilterValue = filter.Value });
            }

            return appFilters;
        }

        private static List<LeaseSummary> ApplySummaryFilters(List<LeaseSummary> summaries, Dictionary<string, string>? filters)
        {
            if (filters == null || filters.Count == 0)
                return summaries;

            IEnumerable<LeaseSummary> filtered = summaries;

            foreach (var filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Value))
                    continue;

                var key = filter.Key.Trim();
                var value = filter.Value.Trim();

                if (key.Equals("LeaseName", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(item => item.LeaseName.Contains(value, StringComparison.OrdinalIgnoreCase));
                }
                else if (key.Equals("Status", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(item => string.Equals(item.Status, value, StringComparison.OrdinalIgnoreCase));
                }
            }

            return filtered.ToList();
        }

        private static string ResolveLeaseSummaryName(LAND_RIGHT landRight, LAND_AGREEMENT? agreement)
        {
            if (!string.IsNullOrWhiteSpace(agreement?.REMARK))
                return agreement.REMARK;

            if (!string.IsNullOrWhiteSpace(landRight.REMARK))
                return landRight.REMARK;

            if (!string.IsNullOrWhiteSpace(agreement?.LAND_AGREE_TYPE))
                return agreement.LAND_AGREE_TYPE;

            if (!string.IsNullOrWhiteSpace(landRight.CASE_SERIAL_NUM))
                return landRight.CASE_SERIAL_NUM;

            return landRight.LAND_RIGHT_ID ?? string.Empty;
        }

        private static string ResolveLeaseSummaryStatus(LAND_RIGHT landRight, LAND_AGREEMENT? agreement, LAND_STATUS? leaseStatus)
        {
            if (!string.IsNullOrWhiteSpace(leaseStatus?.LAND_RIGHT_STATUS))
                return leaseStatus.LAND_RIGHT_STATUS;

            if (string.Equals(landRight.ACTIVE_IND, ActiveIndicator, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(agreement?.ACTIVE_IND, ActiveIndicator, StringComparison.OrdinalIgnoreCase))
            {
                return ActiveStatus;
            }

            return "INACTIVE";
        }

        private static int? ResolvePrimaryTermMonths(IEnumerable<LAND_STATUS>? statusHistory)
        {
            if (statusHistory == null)
                return null;

            var statusWithTerm = statusHistory.FirstOrDefault(item =>
                !string.IsNullOrWhiteSpace(item.EFFECTIVE_TERM) &&
                (string.IsNullOrWhiteSpace(item.EFFECTIVE_TERM_OUOM) ||
                 string.Equals(item.EFFECTIVE_TERM_OUOM, MonthsOuom, StringComparison.OrdinalIgnoreCase)));

            if (statusWithTerm == null)
                return null;

            return int.TryParse(statusWithTerm.EFFECTIVE_TERM, out var months)
                ? months
                : null;
        }

        private static DateTime ResolveLeaseEffectiveDate(CreateLeaseAcquisition leaseRequest)
            => leaseRequest.EffectiveDate == default ? DateTime.UtcNow : leaseRequest.EffectiveDate;

        private static string ResolveLeaseName(CreateLeaseAcquisition leaseRequest, string leaseId)
        {
            if (!string.IsNullOrWhiteSpace(leaseRequest.LeaseName))
                return leaseRequest.LeaseName.Trim();

            if (!string.IsNullOrWhiteSpace(leaseRequest.LeaseNumber))
                return leaseRequest.LeaseNumber.Trim();

            return leaseId;
        }

        private static string ResolveLeaseNumber(CreateLeaseAcquisition leaseRequest, string leaseId)
            => !string.IsNullOrWhiteSpace(leaseRequest.LeaseNumber)
                ? leaseRequest.LeaseNumber.Trim()
                : leaseId;

        private static string NormalizeLeaseType(string? leaseType)
            => string.IsNullOrWhiteSpace(leaseType) ? LeaseRecordSubtype : leaseType.Trim();

        private static string NormalizeLeaseStatus(string status)
            => status.Trim().ToUpperInvariant();
    }
}
