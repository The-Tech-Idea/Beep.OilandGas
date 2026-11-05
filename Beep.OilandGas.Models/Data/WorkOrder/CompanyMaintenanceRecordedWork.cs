using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Models.Data.WorkOrder;

public sealed record CompanyMaintenanceRecordedWorkWindow(string FieldId,DateTime FromUtc,DateTime ToUtc,string? WorkOrderId=null)
{
    public const int MaximumRecords=256,MaximumOrders=128;
    public void Require()
    {
        if(!LeaseSummaryValidator.Identifier(FieldId) || FromUtc==default || FromUtc.Kind!=DateTimeKind.Utc || ToUtc.Kind!=DateTimeKind.Utc || ToUtc<=FromUtc || ToUtc-FromUtc>TimeSpan.FromDays(31) || WorkOrderId is not null && !LeaseSummaryValidator.Identifier(WorkOrderId))throw new ArgumentException("Select the original FIELD, an explicit UTC recorded period of at most 31 days and an optional exact work-order identity.");
    }
    public bool Includes(CompanyWorkOrderSubmissionFacts facts)=>facts.Query.Task.FieldId==FieldId && facts.RecordedAtUtc>=FromUtc && facts.RecordedAtUtc<ToUtc && (WorkOrderId is null || facts.Query.Task.WorkOrderId==WorkOrderId);
}
/// <summary>Published work-report summary; no private submission/closure/return intent or command body.</summary>
public sealed record CompanyMaintenanceRecordedWorkEntry(CompanyWorkOrderCompletionQuery Query,string ExecutorUserId,string ExecutionRole,string WorkPerformed,int OutstandingDefects,DateTime CompletedAtUtc,DateTime RecordedAtUtc,string ReportRevision)
{
    public static CompanyMaintenanceRecordedWorkEntry From(CompanyWorkOrderSubmissionFacts facts)
    {facts.Require();return new(facts.Query,facts.ExecutorUserId,facts.ExecutionRole,facts.WorkPerformed,facts.OutstandingDefects.Count,facts.CompletedAtUtc,facts.RecordedAtUtc,facts.Revision);}
    public void Require(CompanyMaintenanceRecordedWorkWindow window)
    {
        Query.Require();if(Query.Task.FieldId!=window.FieldId || window.WorkOrderId is not null && Query.Task.WorkOrderId!=window.WorkOrderId || !LeaseSummaryValidator.Identifier(ExecutorUserId) || !LeaseSummaryValidator.Identifier(ExecutionRole) || string.IsNullOrWhiteSpace(WorkPerformed) || WorkPerformed.Length>16384 || OutstandingDefects is <0 or >64 || CompletedAtUtc.Kind!=DateTimeKind.Utc || CompletedAtUtc==default || RecordedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc<CompletedAtUtc || RecordedAtUtc<window.FromUtc || RecordedAtUtc>=window.ToUtc || !ProcessDefinitionRevision.IsValid(ReportRevision))throw new InvalidOperationException("Original authorized report identity, work/defect source and recorded UTC period are required.");
    }
    public string DetailRoute(CompanyScope company){PurchaseOrderOwnership.RequireScope(company);return "/ppdm39/workorder/"+Uri.EscapeDataString(Query.Task.WorkOrderId)+"?closureProcess="+Uri.EscapeDataString(Query.Task.InstanceId)+"&closureSubmission="+Uri.EscapeDataString(Query.SubmissionId)+"&sourceTenant="+Uri.EscapeDataString(company.TenantId)+"&sourceCompany="+Uri.EscapeDataString(company.BusinessAssociateId)+"&sourceField="+Uri.EscapeDataString(Query.Task.FieldId);}
}
public sealed record CompanyMaintenanceRecordedWorkPage(FieldSelectionReview Context,CompanyMaintenanceRecordedWorkWindow Window,int PageNumber,int PageSize,int TotalRecords,IReadOnlyList<CompanyMaintenanceRecordedWorkEntry> Rows,DateTime ComparedAtUtc,string SourceRevision)
{
    public int TotalPages=>Math.Max(1,(TotalRecords+PageSize-1)/PageSize);
    public bool HasPrevious=>PageNumber>1;
    public bool HasNext=>PageNumber<TotalPages;
    public static void RequirePage(int page,int size){if(page is <1 or >CompanyMaintenanceRecordedWorkWindow.MaximumRecords || size is <1 or >50)throw new ArgumentException("Select a recorded-work page and between 1 and 50 reports per page.");}
    public static CompanyMaintenanceRecordedWorkPage Create(FieldSelectionReview context,CompanyMaintenanceRecordedWorkWindow window,int page,int size,IReadOnlyList<CompanyWorkOrderSubmissionFacts> reports,DateTime utc)
    {
        window.Require();RequirePage(page,size);FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        if(context.FieldId!=window.FieldId || reports.Count>CompanyMaintenanceRecordedWorkWindow.MaximumRecords || reports.Any(report=>report.Company!=context.Company || report.Database!=context.Database || !window.Includes(report)) || reports.Select(report=>report.Query.SubmissionId).Distinct(StringComparer.Ordinal).Count()!=reports.Count)throw new InvalidOperationException("Complete unique authorized report membership and original company/CORE/period are required.");
        var all=reports.Select(CompanyMaintenanceRecordedWorkEntry.From).OrderByDescending(row=>row.RecordedAtUtc).ThenBy(row=>row.Query.SubmissionId,StringComparer.Ordinal).ToArray();
        var result=new CompanyMaintenanceRecordedWorkPage(context,window,page,size,all.Length,all.Skip((page-1)*size).Take(size).ToArray(),utc,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context=context,Window=window,Rows=all})));result.Require(context.Company,context.ActorUserId,window,page,size);return result;
    }
    public void Require(CompanyScope company,string actor,CompanyMaintenanceRecordedWorkWindow window,int page,int size)
    {
        window.Require();RequirePage(page,size);FieldSelectionReviewValidator.RequireReview(Context,company,actor);
        if(Context.FieldId!=window.FieldId || Window!=window || PageNumber!=page || PageSize!=size || TotalRecords is <0 or >CompanyMaintenanceRecordedWorkWindow.MaximumRecords || PageNumber>TotalPages || Rows is null || Rows.Count!=Math.Min(PageSize,Math.Max(0,TotalRecords-(PageNumber-1)*PageSize)) || Rows.Select(row=>row.Query.SubmissionId).Distinct(StringComparer.Ordinal).Count()!=Rows.Count || ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc==default || !ProcessDefinitionRevision.IsValid(SourceRevision))throw new InvalidOperationException("Complete recorded-work page membership, source and original company/account/period are required.");
        foreach(var row in Rows)row.Require(window);
        if(!Rows.SequenceEqual(Rows.OrderByDescending(row=>row.RecordedAtUtc).ThenBy(row=>row.Query.SubmissionId,StringComparer.Ordinal)))throw new InvalidOperationException("Original recorded-work page order changed.");
    }
}
