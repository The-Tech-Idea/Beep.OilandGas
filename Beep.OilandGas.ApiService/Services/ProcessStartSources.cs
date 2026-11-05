using System.Text.Json;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Projects existing company-bound journal and FIELD-bound maintenance owners. No generic source inference.</summary>
public static class ProcessStartSources
{
    public static ProcessStartSourceReview Journal(CompanyScope company,string actor,ProcessInstanceRequest request,
        JOURNAL_ENTRY? journal,IReadOnlyList<JOURNAL_ENTRY_LINE>? lines)
    {
        if(request.EntityType!="JOURNAL_ENTRY" || journal is null || journal.JOURNAL_ENTRY_ID!=request.EntityId ||
            lines is null || lines.Any(row=>row is null || string.IsNullOrWhiteSpace(row.JOURNAL_ENTRY_LINE_ID) ||
                row.JOURNAL_ENTRY_ID!=request.EntityId || string.IsNullOrWhiteSpace(row.GL_ACCOUNT_ID)) ||
            lines.Select(row=>row.JOURNAL_ENTRY_LINE_ID).Distinct(StringComparer.Ordinal).Count()!=lines.Count)
            throw new InvalidOperationException("The recorded journal header and lines cannot be confirmed.");
        return Create(company,actor,request,journal.ENTRY_NUMBER,journal.DESCRIPTION,journal.STATUS,journal.ROW_CREATED_BY,
            new {Journal=journal,Lines=lines.OrderBy(row=>row.JOURNAL_ENTRY_LINE_ID,StringComparer.Ordinal)});
    }
    public static ProcessStartSourceReview WorkOrder(CompanyScope company,string actor,ProcessInstanceRequest request,WorkOrderDetailModel? order)
    {
        if(request.EntityType!="WORK_ORDER" || order is null || order.InstanceId!=request.EntityId || order.FieldId!=request.ExpectedFieldId || order.Source!="WORK_ORDER")
            throw new InvalidOperationException("The recorded maintenance header does not match the original FIELD and source.");
        RecordedWorkOrderComponentChecks.Require(order);
        if(!WorkOrderParentAccess.Fields(request.EntityId,order.Components).Contains(request.ExpectedFieldId,StringComparer.Ordinal))
            throw new InvalidOperationException("The recorded components do not establish the original FIELD relationship.");
        return Create(company,actor,request,order.InstanceName,order.Instructions,null,null,
            new {order.InstanceId,order.FieldId,order.InstanceName,order.WoSubType,order.RequestDate,order.DueDate,order.Instructions,
                Components=order.Components.OrderBy(row=>row.ComponentId,StringComparer.Ordinal)});
    }
    private static ProcessStartSourceReview Create(CompanyScope company,string actor,ProcessInstanceRequest request,
        string? reference,string? description,string? status,string? recordedBy,object evidence)
    {
        var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new {company,actor,request.ExpectedFieldId,
            request.ProcessId,request.ExpectedDefinitionRevision,request.EntityId,request.EntityType,Source=evidence}));
        var review=new ProcessStartSourceReview(company,actor,request.ExpectedFieldId,request.ProcessId,request.ExpectedDefinitionRevision,
            request.EntityId,request.EntityType,reference,description,status,recordedBy,revision,DateTimeOffset.UtcNow);
        review.Require(company,actor,request);return review;
    }
}
