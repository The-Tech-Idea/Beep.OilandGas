using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.Editor;
namespace Beep.OilandGas.ApiService.Services;
internal static class CompanyWorkOrderFieldReview
{
    public static async Task<string?> LevelAsync(CompanyModuleConnection company,IDMEEditor editor,ICommonColumnHandler columns,IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,ICompanyFieldEligibility eligibility,string order,string grant)
        => (await ReviewAsync(company,editor,columns,defaults,metadata,eligibility,order,grant)).Level;
    public static async Task<List<WorkOrderComponentReference>> ComponentsAsync(CompanyModuleConnection company,IDMEEditor editor,ICommonColumnHandler columns,IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,ICompanyFieldEligibility eligibility,string order)
    {
        var result=await ReviewAsync(company,editor,columns,defaults,metadata,eligibility,order,"READ");
        if(result.Level is null)throw RefusalException.Forbidden("Current company entitlement to every recorded order FIELD is required.");
        return result.Components;
    }
    private static async Task<(string? Level,List<WorkOrderComponentReference> Components)> ReviewAsync(CompanyModuleConnection company,IDMEEditor editor,ICommonColumnHandler columns,IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,ICompanyFieldEligibility eligibility,string order,string grant)
    {
        var database=await company.ResolveAsync();var scope=company.Company.Scope;var actor=company.Company.UserId;
        var reader=new CompanyPPDMEvidenceReader(company,editor,columns,defaults,metadata,database,scope,actor);await reader.OpenAsync();
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata,typeof(WORK_ORDER_COMPONENT),["WORK_ORDER_ID","COMPONENT_ID"],("WORK_ORDER_ID","WORK_ORDER","WORK_ORDER_ID"),("FIELD_ID","FIELD","FIELD_ID"));
        async Task<object[]> ReadAsync()=>await reader.ReadAsync(typeof(WORK_ORDER_COMPONENT),[CompanyPPDMEvidenceReader.Equal("WORK_ORDER_ID",order),CompanyPPDMEvidenceReader.Equal("ACTIVE_IND","Y")],["WORK_ORDER_ID","COMPONENT_ID","FIELD_ID","ACTIVE_IND"]);
        var rows=await ReadAsync();var fingerprint=CompanyPPDMEvidenceReader.Fingerprint(rows);
        var components=rows.Cast<WORK_ORDER_COMPONENT>().Select(c=>new WorkOrderComponentReference(c.WORK_ORDER_ID,c.COMPONENT_ID,c.FIELD_ID,c.UWI ?? "",c.EQUIPMENT_ID ?? "")).ToList();
        string[] fields;
        try { fields=WorkOrderParentAccess.Fields(order,components); }
        catch(InvalidOperationException) { throw RefusalException.Conflict("Recorded work-order components have incomplete or ambiguous FIELD scope."); }
        var levels=new List<string?>();foreach(var field in fields)levels.Add(await eligibility.GetMaximumLevelAsync(field));
        var current=await ReadAsync();if(fingerprint!=CompanyPPDMEvidenceReader.Fingerprint(current))throw RefusalException.Conflict("Recorded work-order FIELD components changed during resource entitlement review.");
        var closed=new List<string?>();foreach(var field in fields)closed.Add(await eligibility.GetMaximumLevelAsync(field));
        await reader.RequireCurrentAsync();if(fingerprint!=CompanyPPDMEvidenceReader.Fingerprint(await ReadAsync()))throw RefusalException.Conflict("Work-order FIELD scope changed before the resource decision.");
        return (WorkOrderParentAccess.Intersect(grant,levels.Concat(closed)),components);
    }
}
