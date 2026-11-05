using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;
using System.Security.Claims;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Orchestrates source checks through the existing resource owner; request company/FIELD admission remains mandatory.</summary>
public sealed class CompanyProcessReadService(IAccessControlService resources)
{
    /// <summary>Company accounting process reads require explicit stored ownership and current native source evidence.</summary>
    public async Task<ProcessInstance?> ReadAccountingAsync(IProcessService service,ClaimsPrincipal user,string actor,CompanyScope company,
        string binding,string id,Func<Task<(RepositoryCompanyAccess Access,string Binding,string Actor)>> current,
        Func<CompanyScope,string,string,string,Task<string>> readSourceRevision,Func<ProcessInstance,Task<string>>? readBoundSourceRevision=null)
    {
        APInvoiceOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(binding))
            throw new UnauthorizedAccessException("Original company, actor and accounting process binding are required.");
        async Task Guard(string? type=null)
        {
            var authority=await current();
            if(user.Identity?.IsAuthenticated!=true || authority.Access is null || authority.Access.Scope!=company ||
                authority.Actor!=actor || authority.Binding!=binding || authority.Access.Roles is null || !authority.Access.Roles.Any(user.IsInRole) ||
                authority.Access.Permissions is null || (type is "AP_INVOICE" or "SALES_CONTRACT" or "RUN_TICKET" or "INVOICE" or "AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT") && !authority.Access.Permissions.Contains("Accounting.View",StringComparer.Ordinal) ||
                type=="PURCHASE_ORDER" && !authority.Access.Permissions.Contains("Procurement.View",StringComparer.Ordinal) ||
                type is null && !authority.Access.Permissions.Any(permission=>permission is "Accounting.View" or "Procurement.View"))
                throw new UnauthorizedAccessException("Current company accounting source authority or binding changed.");
        }
        void RequireSource(ProcessInstance source)
        {
            if(source.InstanceId!=id || !string.IsNullOrEmpty(source.FieldId) || string.IsNullOrWhiteSpace(source.ProcessId) ||
                source.ProcessData?.InstanceId!=id || source.ProcessData.Data is null ||
                !source.ProcessData.Data.TryGetValue(CompanyAccountingProcessBinding.PayloadKey,out var owner) || owner is null)
                throw new InvalidOperationException("The company accounting process has no explicit confirmed source scope.");
            CompanyAccountingProcessBinding.FromJson(JsonSerializer.Serialize(owner),company,id,source.EntityType,source.EntityId);
        }
        async Task<(ProcessInstance Source,string Revision,string DomainRevision)?> Capture()
        {
            await Guard();var source=await service.GetProcessInstanceAsync(id);if(source is null)return null;
            RequireSource(source);await Guard(source.EntityType);
            var frozen=JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(source))!;
            var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(frozen));
            var domainRevision=readBoundSourceRevision is null?await readSourceRevision(company,actor,frozen.EntityType,frozen.EntityId):await readBoundSourceRevision(frozen);
            if(!ProcessDefinitionRevision.IsValid(domainRevision))throw new InvalidOperationException("The owned accounting process source is unavailable.");
            await Guard(frozen.EntityType);return(frozen,revision,domainRevision);
        }
        var initial=await Capture();if(initial is null)return null;
        var closing=await Capture();
        if(closing is null || closing.Value.Revision!=initial.Value.Revision || closing.Value.DomainRevision!=initial.Value.DomainRevision)
            throw new InvalidOperationException("The company accounting process or its owned source changed during review.");
        return closing.Value.Source;
    }
    private async Task RequireReadAsync(string actor, ProcessInstance source, string id, string field,CompanyScope? company)
    {
        if(source.EntityType=="DRILLING_DAILY_REPORT" || id.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal) || source.ProcessData?.Data?.ContainsKey(DrillingCompletionReviewStart.PayloadKey)==true)
            throw new UnauthorizedAccessException("Completion review requires its exact reviewed company source reader.");
        if(source.EntityType is "AFE" or "DRILLING_PROGRAM" || id.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal) || source.ProcessData?.Data?.ContainsKey(DrillingProgramReviewStart.PayloadKey)==true || id.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal) || source.ProcessData?.Data?.ContainsKey(DevelopmentInvestmentHandoff.PayloadKey)==true)
            throw new UnauthorizedAccessException("Investment assignments require their dedicated financial receiving-source reader.");
        if(id.StartsWith(CompanyMaintenanceProcessStart.InstancePrefix,StringComparison.Ordinal) || CompanyMaintenanceProcessStart.HasPayload(JsonSerializer.Serialize(source.ProcessData)))
        {
            var original=CompanyMaintenanceProcessStart.FromPayload(JsonSerializer.Serialize(source.ProcessData),id,field,source.EntityId);
            if(company is null || original.Context.Company!=company)throw new UnauthorizedAccessException("The maintenance process belongs to another selected company.");
        }
        if (string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(field) || source.InstanceId!=id || source.FieldId!=field ||
            string.IsNullOrWhiteSpace(source.EntityId) || string.IsNullOrWhiteSpace(source.EntityType) ||
            !(await resources.CheckAssetAccessAsync(actor,field,"FIELD","READ")).HasAccess ||
            !(await resources.CheckAssetAccessAsync(actor,source.EntityId,source.EntityType,"READ")).HasAccess)
            throw new UnauthorizedAccessException("Current company FIELD and source resource READ access is required.");
    }
    private async Task<ProcessInstance> CloseAsync(IProcessService service,string actor,string field,string id,string entity,string type,CompanyScope? company)
    {
        var source=await service.GetProcessInstanceAsync(id);
        if(source is null || source.InstanceId!=id || source.FieldId!=field || source.EntityId!=entity || source.EntityType!=type)
            throw new InvalidOperationException("The process source changed during the read. Reload before continuing.");
        await RequireReadAsync(actor,source,id,field,company);return source;
    }
    public async Task<ProcessInstance?> ReadAsync(IProcessService service,string actor,string field,string id,CompanyScope? company=null)
    {
        var source=await service.GetProcessInstanceAsync(id);if(source is null)return null;
        var entity=source.EntityId;var type=source.EntityType;
        await RequireReadAsync(actor,source,id,field,company);
        return await CloseAsync(service,actor,field,id,entity,type,company);
    }
    public async Task<List<ProcessHistoryEntry>?> HistoryAsync(IProcessService service,string actor,string field,string id,CompanyScope? company=null)
    {
        var source=await service.GetProcessInstanceAsync(id);if(source is null)return null;
        var entity=source.EntityId;var type=source.EntityType;await RequireReadAsync(actor,source,id,field,company);
        var rows=await service.GetProcessHistoryAsync(id);
        if(rows is null || rows.Any(row=>row is null || row.InstanceId!=id || string.IsNullOrWhiteSpace(row.HistoryId)) || rows.Select(row=>row.HistoryId).Distinct().Count()!=rows.Count)
            throw new InvalidOperationException("The process history could not be confirmed for this source.");
        await CloseAsync(service,actor,field,id,entity,type,company);return rows;
    }
}
