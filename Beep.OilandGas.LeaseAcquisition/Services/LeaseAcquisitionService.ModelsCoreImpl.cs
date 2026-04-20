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

            var evaluation = await EvaluateLeaseOpportunityAsync(
                leaseId,
                new LeaseEvaluationRequest { LeaseId = leaseId, IncludeFinancialAnalysis = true });

            return new LeaseSummary
            {
                LeaseId   = leaseId,
                LeaseName = evaluation.EvaluationSummary,
                Status    = evaluation.RecommendationStatus
            };
        }

        async Task<List<LeaseSummary>> Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService.GetAvailableLeasesAsync(
            Dictionary<string, string>? filters)
        {
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LAND_AGREEMENT), _connectionName, "LAND_AGREEMENT", null);

            var appFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (filters != null)
            {
                foreach (var kvp in filters)
                    appFilters.Add(new AppFilter { FieldName = kvp.Key, Operator = "=", FilterValue = kvp.Value });
            }

            var entities = await repo.GetAsync(appFilters);
            return entities.OfType<LAND_AGREEMENT>().Select(e => new LeaseSummary
            {
                LeaseId     = e.LAND_RIGHT_ID ?? string.Empty,
                LeaseNumber = e.LAND_RIGHT_ID ?? string.Empty,
                LeaseName   = e.LAND_AGREE_TYPE ?? string.Empty,
                Status      = e.ACTIVE_IND == "Y" ? "ACTIVE" : "INACTIVE"
            }).ToList();
        }

        async Task<string> Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService.CreateLeaseAcquisitionAsync(
            CreateLeaseAcquisition leaseRequest, string userId)
        {
            if (leaseRequest == null)
                throw new ArgumentNullException(nameof(leaseRequest));

            var request = new LeaseAcquisitionRequest
            {
                LeaseName = leaseRequest.LeaseName,
                LocationId = leaseRequest.PropertyId ?? string.Empty
            };

            var result = await InitiateLeaseAcquisitionAsync(request, userId);
            return result.LeaseId;
        }

        async Task Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService.UpdateLeaseStatusAsync(
            string leaseId, string status, string userId)
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentNullException(nameof(status));

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LAND_STATUS), _connectionName, "LAND_STATUS", null);

            var statusRecord = new LAND_STATUS
            {
                LAND_RIGHT_ID      = leaseId,
                LAND_RIGHT_SUBTYPE = "LEASE",
                STATUS_TYPE        = "OPERATIONAL",
                LAND_RIGHT_STATUS  = status,
                STATUS_SEQ_NO      = 1m,
                EFFECTIVE_DATE     = DateTime.UtcNow,
                ACTIVE_IND         = "Y"
            };

            await repo.InsertAsync(statusRecord, userId);
        }
    }
}
