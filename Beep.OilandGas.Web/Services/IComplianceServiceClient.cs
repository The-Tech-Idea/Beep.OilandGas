using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Compliance;

namespace Beep.OilandGas.Web.Services;

public interface IComplianceServiceClient
{
    Task<List<ObligationSummary>> GetAllObligationsAsync(int? year = null, CancellationToken cancellationToken = default);
    Task<List<ObligationSummary>> GetUpcomingObligationsAsync(int daysAhead = 30, CancellationToken cancellationToken = default);
    Task<List<ObligationSummary>> GetOverdueObligationsAsync(CancellationToken cancellationToken = default);
    Task<ObligationDetailModel?> GetObligationAsync(string obligationId, CancellationToken cancellationToken = default);
    Task<string?> CreateObligationAsync(CreateObligationRequest request, CancellationToken cancellationToken = default);
    Task<ComplianceScoreCard?> GetScoreAsync(int? year = null, CancellationToken cancellationToken = default);
    Task SubmitObligationAsync(string obligationId, DateTime? submitDate = null, CancellationToken cancellationToken = default);
    Task WaiveObligationAsync(string obligationId, string reason, CancellationToken cancellationToken = default);
    Task RecordPaymentAsync(string obligationId, RecordPaymentRequest request, CancellationToken cancellationToken = default);
    Task<GHGEmissionReport?> GenerateGhgReportAsync(int year, string jurisdiction, CancellationToken cancellationToken = default);
}