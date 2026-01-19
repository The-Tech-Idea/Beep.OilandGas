using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// COPAS overhead allocation for joint interest billing.
    /// </summary>
    public class CopasOverheadService : ICopasOverheadService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<CopasOverheadService> _logger;
        private const string ConnectionName = "PPDM39";

        public CopasOverheadService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<CopasOverheadService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<decimal> CalculateOverheadAsync(
            string leaseId,
            decimal baseAmount,
            DateTime asOfDate,
            string cn = "PPDM39")
        {
            if (baseAmount <= 0m)
                return 0m;

            var overheadRate = await GetOverheadRateAsync(leaseId, asOfDate, cn);
            if (overheadRate <= 0m)
                return 0m;

            var overhead = baseAmount * overheadRate;
            _logger?.LogInformation(
                "COPAS overhead calculated for lease {LeaseId}: base {Base} rate {Rate} overhead {Overhead}",
                leaseId, baseAmount, overheadRate, overhead);

            return overhead;
        }

        public async Task<JIB_CHARGE?> ApplyOverheadToStatementAsync(
            JOINT_INTEREST_STATEMENT statement,
            decimal baseAmount,
            string userId,
            string cn = "PPDM39")
        {
            if (statement == null)
                throw new ArgumentNullException(nameof(statement));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var overhead = await CalculateOverheadAsync(statement.JIB_ID, baseAmount, statement.REPORT_PERIOD_END ?? DateTime.UtcNow, cn);
            if (overhead <= 0m)
                return null;

            await RecordOverheadAuditAsync(statement.JIB_ID, statement.REPORT_PERIOD_END ?? DateTime.UtcNow, userId, cn);

            var charge = new JIB_CHARGE
            {
                JIB_CHARGE_ID = Guid.NewGuid().ToString(),
                JOINT_INTEREST_STATEMENT_ID = statement.JOINT_INTEREST_STATEMENT_ID,
                DESCRIPTION = "COPAS Overhead",
                CATEGORY = "OVERHEAD",
                AMOUNT = overhead,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await CreateRepoAsync<JIB_CHARGE>("JIB_CHARGE", cn);
            await repo.InsertAsync(charge, userId);

            return charge;
        }

        private async Task<decimal> GetOverheadRateAsync(string leaseId, DateTime asOfDate, string cn)
        {
            var scheduleRate = await GetScheduleRateAsync(leaseId, asOfDate, cn);
            if (scheduleRate > 0m)
                return scheduleRate;

            var repo = await CreateRepoAsync<COST_SHARING>("COST_SHARING", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var records = results?.Cast<COST_SHARING>().ToList() ?? new List<COST_SHARING>();

            var match = records.FirstOrDefault(r => RemarkHasLease(r.REMARK, leaseId));
            var overheadPercent = match?.OPERATOR_OVERHEAD_PERCENTAGE ?? records.FirstOrDefault()?.OPERATOR_OVERHEAD_PERCENTAGE ?? 0m;
            if (overheadPercent > 1m)
                overheadPercent /= 100m;
            return overheadPercent;
        }

        private async Task<decimal> GetScheduleRateAsync(string leaseId, DateTime asOfDate, string cn)
        {
            try
            {
                var repo = await CreateRepoAsync<COPAS_OVERHEAD_SCHEDULE>("COPAS_OVERHEAD_SCHEDULE", cn);
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                if (!string.IsNullOrWhiteSpace(leaseId))
                    filters.Add(new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId });

                var results = await repo.GetAsync(filters);
                var schedules = results?.Cast<COPAS_OVERHEAD_SCHEDULE>().ToList()
                    ?? new List<COPAS_OVERHEAD_SCHEDULE>();

                var schedule = schedules.FirstOrDefault(s =>
                    (!s.EFFECTIVE_DATE.HasValue || s.EFFECTIVE_DATE.Value.Date <= asOfDate.Date) &&
                    (!s.EXPIRY_DATE.HasValue || s.EXPIRY_DATE.Value.Date >= asOfDate.Date));

                if (schedule?.OVERHEAD_RATE == null)
                    return 0m;

                var rate = schedule.OVERHEAD_RATE.Value;
                if (rate > 1m)
                    rate /= 100m;
                return rate;
            }
            catch
            {
                return 0m;
            }
        }

        private async Task RecordOverheadAuditAsync(string leaseId, DateTime asOfDate, string userId, string cn)
        {
            try
            {
                var scheduleRepo = await CreateRepoAsync<COPAS_OVERHEAD_SCHEDULE>("COPAS_OVERHEAD_SCHEDULE", cn);
                var scheduleFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
                    new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId }
                };

                var schedules = await scheduleRepo.GetAsync(scheduleFilters);
                var schedule = schedules?.Cast<COPAS_OVERHEAD_SCHEDULE>()
                    .FirstOrDefault(s =>
                        (!s.EFFECTIVE_DATE.HasValue || s.EFFECTIVE_DATE.Value.Date <= asOfDate.Date) &&
                        (!s.EXPIRY_DATE.HasValue || s.EXPIRY_DATE.Value.Date >= asOfDate.Date));

                if (schedule == null)
                    return;

                var audit = new COPAS_OVERHEAD_AUDIT
                {
                    COPAS_OVERHEAD_AUDIT_ID = Guid.NewGuid().ToString(),
                    COPAS_OVERHEAD_SCHEDULE_ID = schedule.COPAS_OVERHEAD_SCHEDULE_ID,
                    CHANGE_DATE = asOfDate,
                    OLD_RATE = null,
                    NEW_RATE = schedule.OVERHEAD_RATE,
                    CHANGE_REASON = "Applied overhead schedule",
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var auditRepo = await CreateRepoAsync<COPAS_OVERHEAD_AUDIT>("COPAS_OVERHEAD_AUDIT", cn);
                await auditRepo.InsertAsync(audit, userId);
            }
            catch
            {
                // Optional audit table not available; skip
            }
        }

        private static bool RemarkHasLease(string remark, string leaseId)
        {
            if (string.IsNullOrWhiteSpace(remark) || string.IsNullOrWhiteSpace(leaseId))
                return false;
            var tokens = remark.Split(new[] { ';', ',', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var parts = token.Split('=');
                if (parts.Length != 2)
                    continue;
                if (!parts[0].Equals("LEASE_ID", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (parts[1].Equals(leaseId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private async Task<PPDMGenericRepository> CreateRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, tableName);
        }
    }
}
