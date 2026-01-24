using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PermitsAndApplications.Services
{
    /// <summary>
    /// Compliance tracking and reporting for permit applications.
    /// </summary>
    public class PermitComplianceReportService : PermitsServiceBase, IPermitComplianceReportService
    {
        private readonly ILogger<PermitComplianceReportService> _logger;

        public PermitComplianceReportService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PermitComplianceReportService> logger = null,
            string connectionName = "PPDM39")
            : base(editor, commonColumnHandler, defaults, metadata, logger, connectionName)
        {
            _logger = logger;
        }

        public async Task<IReadOnlyList<ExpiringPermitRecord>> GetExpiringPermitsAsync(
            DateTime asOfDate,
            int daysUntilExpiry)
        {
            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var endDate = asOfDate.Date.AddDays(daysUntilExpiry);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
                new AppFilter { FieldName = "EXPIRY_DATE", Operator = ">=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "EXPIRY_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") }
            };

            var results = await repo.GetAsync(filters);
            return results
                .Select(r => r as PERMIT_APPLICATION)
                .Where(r => r != null)
                .Select(r => new ExpiringPermitRecord
                {
                    PermitApplicationId = r.PERMIT_APPLICATION_ID ?? string.Empty,
                    ApplicationType = r.APPLICATION_TYPE,
                    Status = r.STATUS,
                    ExpiryDate = r.EXPIRY_DATE,
                    RegulatoryAuthority = r.REGULATORY_AUTHORITY
                })
                .ToList();
        }

        public async Task<ComplianceReport> GenerateComplianceReportAsync(DateTime asOfDate, int daysUntilExpiry)
        {
            var report = new ComplianceReport
            {
                GeneratedOn = DateTime.UtcNow,
                AsOfDate = asOfDate,
                ExpiringWithinDays = daysUntilExpiry
            };

            var expiring = await GetExpiringPermitsAsync(asOfDate, daysUntilExpiry);
            report.ExpiringPermits = expiring.ToList();

            var statusCounts = await GetStatusCountsAsync();
            report.StatusCounts = statusCounts.ToList();

            if (report.ExpiringPermits.Count == 0)
                report.Warnings.Add("No permits are expiring within the specified window.");

            _logger?.LogInformation(
                "Compliance report generated: {ExpiringCount} expiring permits",
                report.ExpiringPermits.Count);

            return report;
        }

        private async Task<IReadOnlyList<ComplianceStatusCount>> GetStatusCountsAsync()
        {
            var repo = await CreateRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var applications = results.Select(r => r as PERMIT_APPLICATION).Where(r => r != null).ToList();

            return applications
                .GroupBy(a => a.STATUS ?? "UNKNOWN", StringComparer.OrdinalIgnoreCase)
                .Select(g => new ComplianceStatusCount
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();
        }

    }
}
