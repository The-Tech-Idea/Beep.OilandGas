using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Compliance;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Compliance;

public class ComplianceService : IComplianceService
{
    private readonly IDMEEditor                  _editor;
    private readonly ICommonColumnHandler        _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository   _defaults;
    private readonly IPPDMMetadataRepository     _metadata;
    private readonly string                      _connectionName;
    private readonly ILogger<ComplianceService>  _logger;

    public ComplianceService(
        IDMEEditor                 editor,
        ICommonColumnHandler       commonColumnHandler,
        IPPDM39DefaultsRepository  defaults,
        IPPDMMetadataRepository    metadata,
        string                     connectionName,
        ILogger<ComplianceService> logger)
    {
        _editor             = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults           = defaults;
        _metadata           = metadata;
        _connectionName     = connectionName;
        _logger             = logger;
    }

    // ── Repository factory ────────────────────────────────────────────────────

    private async Task<PPDMGenericRepository> MakeRepoAsync(string table)
    {
        var meta       = await _metadata.GetTableMetadataAsync(table);
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}")
                         ?? typeof(object);
        return new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, table);
    }

    // ── IComplianceService ────────────────────────────────────────────────────

    public async Task<string> CreateObligationAsync(CreateObligationRequest request, string userId)
    {
        try
        {
            var repo = await MakeRepoAsync("OBLIGATION");

            // Build sequential ID
            var existingFilters = new List<AppFilter>
            {
                new() { FieldName = "FIELD_ID",     Operator = "=", FilterValue = request.FieldId },
                new() { FieldName = "ACTIVE_IND",   Operator = "=", FilterValue = "Y" }
            };
            var existing = (await repo.GetAsync(existingFilters)).ToList();
            var seq      = existing.Count + 1;
            var id       = $"OBL-{request.FieldId}-{request.DueDate:yyyyMMdd}-{seq:D3}";

            var detail = new ObligationDetailModel
            {
                ObligationId         = id,
                FieldId              = request.FieldId,
                ObligType            = request.ObligType,
                ObligStatus          = ObligationStatus.Pending,
                DueDate              = request.DueDate,
                JurisdictionCode     = request.Jurisdiction,
                Description          = request.Description,
                ReportingPeriodStart = request.ReportingPeriodStart,
                ReportingPeriodEnd   = request.ReportingPeriodEnd,
                CreatedByProcess     = request.CreatedByProcess,
            };

            await repo.InsertAsync(detail, userId);
            _logger.LogInformation("Created obligation {Id} for field {FieldId}", id, request.FieldId);
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create obligation for field {FieldId}", request.FieldId);
            throw;
        }
    }

    public async Task MarkSubmittedAsync(string obligationId, DateTime submitDate, string userId)
    {
        try
        {
            var repo   = await MakeRepoAsync("OBLIGATION");
            var entity = await repo.GetByIdAsync(obligationId);
            if (entity is null) return;

            SetProperty(entity, "OBLIG_STATUS", ObligationStatus.Submitted);
            SetProperty(entity, "FULFILL_DATE", submitDate);
            await repo.UpdateAsync(entity, userId);
            _logger.LogInformation("Marked obligation {Id} as submitted", obligationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark obligation {Id} submitted", obligationId);
            throw;
        }
    }

    public async Task WaiveObligationAsync(string obligationId, string reason, string userId)
    {
        try
        {
            var repo   = await MakeRepoAsync("OBLIGATION");
            var entity = await repo.GetByIdAsync(obligationId);
            if (entity is null) return;

            SetProperty(entity, "OBLIG_STATUS", ObligationStatus.Waived);
            SetProperty(entity, "DESCRIPTION", reason);
            await repo.UpdateAsync(entity, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to waive obligation {Id}", obligationId);
            throw;
        }
    }

    public async Task RecordPaymentAsync(string obligationId, RecordPaymentRequest request, string userId)
    {
        try
        {
            var repo     = await MakeRepoAsync("OBLIG_PAYMENT");
            var payId    = $"PAY-{obligationId}-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var variance = request.GrossAmt - request.ActualAmt;

            var payment = new ObligationPayment(
                payId, obligationId, request.PaymentType,
                request.PaymentDate, request.PaymentCurrency,
                request.GrossAmt, request.ActualAmt, variance,
                request.PaymentNotes);

            await repo.InsertAsync(payment, userId);

            // Flag variance if significant
            if (Math.Abs(variance) > 500m || (request.GrossAmt > 0 && Math.Abs(variance / request.GrossAmt) > 0.05m))
            {
                var obligRepo = await MakeRepoAsync("OBLIGATION");
                var entity    = await obligRepo.GetByIdAsync(obligationId);
                if (entity is not null)
                {
                    SetProperty(entity, "OBLIG_STATUS", ObligationStatus.VarianceFlagged);
                    await obligRepo.UpdateAsync(entity, userId);
                    _logger.LogWarning("Variance flagged on obligation {Id}: {Variance}", obligationId, variance);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record payment for obligation {Id}", obligationId);
            throw;
        }
    }

    public async Task<ObligationDetailModel?> GetByIdAsync(string obligationId)
    {
        try
        {
            var repo   = await MakeRepoAsync("OBLIGATION");
            var entity = await repo.GetByIdAsync(obligationId);
            if (entity is null) return null;

            var detail = MapToDetail(entity);

            // Load payments
            var payRepo = await MakeRepoAsync("OBLIG_PAYMENT");
            var payFilters = new List<AppFilter>
            {
                new() { FieldName = "OBLIGATION_ID", Operator = "=", FilterValue = obligationId }
            };
            var payments = (await payRepo.GetAsync(payFilters)).ToList();
            detail.Payments = payments.Select(MapToPayment).ToList();

            return detail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get obligation {Id}", obligationId);
            throw;
        }
    }

    public async Task<List<ObligationSummary>> GetUpcomingObligationsAsync(string fieldId, int daysAhead = 30)
    {
        try
        {
            var repo    = await MakeRepoAsync("OBLIGATION");
            var cutoff  = DateTime.UtcNow.AddDays(daysAhead);
            var filters = new List<AppFilter>
            {
                new() { FieldName = "FIELD_ID",     Operator = "=",  FilterValue = fieldId },
                new() { FieldName = "OBLIG_STATUS",  Operator = "=",  FilterValue = ObligationStatus.Pending },
                new() { FieldName = "DUE_DATE",      Operator = "<=", FilterValue = cutoff.ToString("yyyy-MM-dd") },
                new() { FieldName = "ACTIVE_IND",    Operator = "=",  FilterValue = "Y" }
            };
            return (await repo.GetAsync(filters)).ToList().Select(ToSummary).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get upcoming obligations for {FieldId}", fieldId);
            return new();
        }
    }

    public async Task<List<ObligationSummary>> GetOverdueObligationsAsync(string fieldId)
    {
        try
        {
            var repo    = await MakeRepoAsync("OBLIGATION");
            var filters = new List<AppFilter>
            {
                new() { FieldName = "FIELD_ID",    Operator = "=", FilterValue = fieldId },
                new() { FieldName = "OBLIG_STATUS", Operator = "=", FilterValue = ObligationStatus.Overdue },
                new() { FieldName = "ACTIVE_IND",   Operator = "=", FilterValue = "Y" }
            };
            return (await repo.GetAsync(filters)).ToList().Select(ToSummary).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get overdue obligations for {FieldId}", fieldId);
            return new();
        }
    }

    public async Task<List<ObligationSummary>> GetAllObligationsAsync(string fieldId, int year)
    {
        try
        {
            var repo    = await MakeRepoAsync("OBLIGATION");
            var filters = new List<AppFilter>
            {
                new() { FieldName = "FIELD_ID",  Operator = "=", FilterValue = fieldId },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            return (await repo.GetAsync(filters)).ToList()
                .Select(ToSummary)
                .Where(s => s.DueDate.Year == year)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all obligations for {FieldId}", fieldId);
            return new();
        }
    }

    public async Task<ComplianceScoreCard> GetComplianceScoreAsync(string fieldId, int year)
    {
        try
        {
            var all = await GetAllObligationsAsync(fieldId, year);

            var total         = all.Count;
            var submitted     = all.Where(s => s.Status == ObligationStatus.Submitted).ToList();
            var overdue       = all.Count(s => s.Status == ObligationStatus.Overdue);
            var waived        = all.Count(s => s.Status == ObligationStatus.Waived);
            var onTime        = submitted.Count(s => !s.IsOverdue);
            var late          = submitted.Count - onTime;
            var onTimeRate    = total > 0 ? (double)onTime / total * 100.0 : 100.0;

            return new ComplianceScoreCard(year, total, onTime, late, overdue, waived, onTimeRate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute compliance score for {FieldId}", fieldId);
            return new ComplianceScoreCard(year, 0, 0, 0, 0, 0, 100.0);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static ObligationSummary ToSummary(object entity)
    {
        var id          = GetStr(entity, "OBLIGATION_ID");
        var obligType   = GetStr(entity, "OBLIG_TYPE");
        var status      = GetStr(entity, "OBLIG_STATUS");
        var dueDate     = GetDate(entity, "DUE_DATE");
        var fulfillDate = GetNullableDate(entity, "FULFILL_DATE");
        var jurisdiction = GetStr(entity, "JURISDICTION_CODE");
        var today       = DateTime.UtcNow.Date;
        var isOverdue   = status == ObligationStatus.Pending && dueDate < today;
        var days        = (int)(dueDate - today).TotalDays;

        return new ObligationSummary(
            id, obligType, status, dueDate,
            isOverdue, days, jurisdiction,
            fulfillDate?.ToString("yyyy-MM-dd"));
    }

    private static ObligationDetailModel MapToDetail(object entity) => new()
    {
        ObligationId         = GetStr(entity, "OBLIGATION_ID"),
        FieldId              = GetStr(entity, "FIELD_ID"),
        ObligType            = GetStr(entity, "OBLIG_TYPE"),
        ObligStatus          = GetStr(entity, "OBLIG_STATUS"),
        DueDate              = GetDate(entity, "DUE_DATE"),
        FulfillDate          = GetNullableDate(entity, "FULFILL_DATE"),
        JurisdictionCode     = GetStr(entity, "JURISDICTION_CODE"),
        Description          = GetStr(entity, "DESCRIPTION"),
        ReportingPeriodStart = GetNullableDate(entity, "REPORTING_PERIOD_START"),
        ReportingPeriodEnd   = GetNullableDate(entity, "REPORTING_PERIOD_END"),
        RegulatorBaId        = GetStr(entity, "REGULATOR_BA_ID"),
        CreatedByProcess     = GetStr(entity, "CREATED_BY_PROCESS"),
    };

    private static ObligationPayment MapToPayment(object entity) => new(
        GetStr(entity, "PAYMENT_ID"),
        GetStr(entity, "OBLIGATION_ID"),
        GetStr(entity, "PAYMENT_TYPE"),
        GetDate(entity, "PAYMENT_DATE"),
        GetStr(entity, "PAYMENT_CURRENCY"),
        GetDecimal(entity, "GROSS_AMT"),
        GetDecimal(entity, "ACTUAL_AMT"),
        GetDecimal(entity, "VARIANCE_AMT"),
        GetStr(entity, "PAYMENT_NOTES"));

    private static void SetProperty(object entity, string name, object? value)
    {
        var prop = entity.GetType().GetProperty(name,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.IgnoreCase);
        prop?.SetValue(entity, value);
    }

    private static string GetStr(object e, string name)
        => e.GetType().GetProperty(name,
               System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
               System.Reflection.BindingFlags.IgnoreCase)
           ?.GetValue(e)?.ToString() ?? string.Empty;

    private static DateTime GetDate(object e, string name)
    {
        var val = e.GetType().GetProperty(name,
                      System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
                      System.Reflection.BindingFlags.IgnoreCase)
                  ?.GetValue(e);
        return val is DateTime dt ? dt : DateTime.MinValue;
    }

    private static DateTime? GetNullableDate(object e, string name)
    {
        var val = e.GetType().GetProperty(name,
                      System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
                      System.Reflection.BindingFlags.IgnoreCase)
                  ?.GetValue(e);
        return val is DateTime dt ? dt : null;
    }

    private static decimal GetDecimal(object e, string name)
    {
        var val = e.GetType().GetProperty(name,
                      System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
                      System.Reflection.BindingFlags.IgnoreCase)
                  ?.GetValue(e);
        return val is decimal d ? d : 0m;
    }
}
