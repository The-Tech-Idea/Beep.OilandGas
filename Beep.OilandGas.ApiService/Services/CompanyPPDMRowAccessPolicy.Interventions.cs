using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM39.Core;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyPPDMRowAccessPolicy
{
    public async Task<ProductionInterventionWorkOrderReview> ReviewInterventionWorkOrderAsync(FieldSelectionReview context,string order,string well)
    {
        if(context.Company!=company.Company.Scope || context.ActorUserId!=company.Company.UserId || context.Database!=await company.ResolveAsync())
            throw RefusalException.Forbidden("The recorded maintenance review must use the original company/account/database.");
        var reader=new CompanyPPDMEvidenceReader(company,editor,columns,defaults,metadata,context.Database,context.Company,context.ActorUserId);await reader.OpenAsync();
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata,typeof(WORK_ORDER),["WORK_ORDER_ID"]);
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata,typeof(WORK_ORDER_COMPONENT),["WORK_ORDER_ID","COMPONENT_ID"],("WORK_ORDER_ID","WORK_ORDER","WORK_ORDER_ID"),("FIELD_ID","FIELD","FIELD_ID"),("UWI","WELL","UWI"));
        var headers=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER),context.Database.ConnectionName,"WORK_ORDER",rowAccess:this);
        async Task<WORK_ORDER> Header()
        {
            var rows=(await headers.GetAsync([CompanyPPDMEvidenceReader.Equal("WORK_ORDER_ID",order),CompanyPPDMEvidenceReader.Equal("ACTIVE_IND","Y")])).ToArray();
            if(rows is not [WORK_ORDER header])throw RefusalException.Forbidden("The original order requires current READ rights and all recorded FIELD entitlements.");
            return CompanyInventorySourceReview.Copy(header);
        }
        async Task<WORK_ORDER_COMPONENT[]> Components()
        {
            _=await CompanyWorkOrderFieldReview.ComponentsAsync(company,editor,columns,defaults,metadata,fieldEligibility,order);
            return (await reader.ReadAsync(typeof(WORK_ORDER_COMPONENT),[CompanyPPDMEvidenceReader.Equal("WORK_ORDER_ID",order),CompanyPPDMEvidenceReader.Equal("ACTIVE_IND","Y")])).Cast<WORK_ORDER_COMPONENT>().Select(CompanyInventorySourceReview.Copy).ToArray();
        }
        var initial=ProductionInterventionWorkOrderReview.Create(await Header(),await Components());initial.Require(order,context.FieldId!,well);
        var closing=ProductionInterventionWorkOrderReview.Create(await Header(),await Components());closing.Require(order,context.FieldId!,well);
        await reader.RequireCurrentAsync();
        if(System.Text.Json.JsonSerializer.Serialize(initial)!=System.Text.Json.JsonSerializer.Serialize(closing))throw RefusalException.Conflict("The complete original order/components or authority changed during intervention review.");
        return closing;
    }
    private static bool WellActivityTable(Type type)=>type==typeof(WELL_ACTIVITY);
    private static void ValidateWellActivityRows(IReadOnlyList<object> rows)
    {
        var keys=new Dictionary<(string Well,string Source,decimal Observation),string>();
        foreach(var row in rows.Cast<WELL_ACTIVITY>())
        {
            if(!CompanyPPDMEvidenceReader.Identifier(row.UWI) || !CompanyPPDMEvidenceReader.Identifier(row.SOURCE) || !CompanyPPDMEvidenceReader.Observation(row.ACTIVITY_OBS_NO))throw RefusalException.Conflict("The complete recorded intervention activity identity is unavailable.");
            var key=(row.UWI,row.SOURCE,row.ACTIVITY_OBS_NO);var revision=CompanyPPDMEvidenceReader.CanonicalRecord(row);
            if(keys.TryGetValue(key,out var previous) && previous!=revision)throw RefusalException.Conflict("One recorded intervention activity key has conflicting native values.");keys[key]=revision;
        }
    }
    private async Task WellActivityDefinitionAsync()
    {
        var definition=await metadata.GetTableMetadataAsync("WELL_ACTIVITY");
        if(definition?.EntityTypeName!="WELL_ACTIVITY" || !new[]{"ACTIVITY_OBS_NO","SOURCE","UWI"}.SequenceEqual((definition.PrimaryKeyColumn??"").Split(',').Select(value=>value.Trim()).Order(StringComparer.Ordinal)) ||
            definition.ForeignKeys.Count(key=>key.ForeignKeyColumn=="UWI" && key.ReferencedTable=="WELL" && key.ReferencedPrimaryKey=="UWI")!=1 ||
            typeof(WELL_ACTIVITY).GetProperty("UWI")?.PropertyType!=typeof(string) || typeof(WELL_ACTIVITY).GetProperty("SOURCE")?.PropertyType!=typeof(string) || typeof(WELL_ACTIVITY).GetProperty("ACTIVITY_OBS_NO")?.PropertyType!=typeof(decimal) || typeof(WELL_ACTIVITY).GetProperty("ACTIVE_IND")?.PropertyType!=typeof(string))
            throw RefusalException.Conflict("Recorded intervention activity metadata does not match its complete canonical WELL relationship.");
        await PathsAsync(typeof(WELL),"WELL");
    }
    private async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> WellActivityPredicatesAsync(List<AppUserAssetAccess> grants)
    {
        var parentPaths=await PathsAsync(typeof(WELL),"WELL");var database=await company.ResolveAsync();
        var reader=new CompanyPPDMEvidenceReader(company,editor,columns,defaults,metadata,database,company.Company.Scope,company.Company.UserId);await reader.OpenAsync();
        async Task<Dictionary<string,string>> Parents(List<AppUserAssetAccess> current)
        {
            var constraints=(await GetReadConstraintsAsync(typeof(WELL),"WELL")).Concat(new[]{CompanyPPDMEvidenceReader.Equal("ACTIVE_IND","Y")}).ToArray();var found=new Dictionary<string,string>(StringComparer.Ordinal);
            foreach(var group in Predicates(parentPaths,current,PPDMRowMutation.Insert,read:true))
            {
                var headers=await reader.ReadAsync(typeof(WELL),group.Concat(constraints).ToArray());
                foreach(var well in headers.Cast<WELL>())
                {
                    if(!CompanyPPDMEvidenceReader.Identifier(well.UWI) || !CompanyPPDMEvidenceReader.Identifier(well.ASSIGNED_FIELD))throw RefusalException.Conflict("The recorded intervention parent WELL identity is unavailable.");
                    var revision=CompanyPPDMEvidenceReader.CanonicalRecord(well);
                    if(found.TryGetValue(well.UWI,out var previous) && previous!=revision)throw RefusalException.Conflict("The recorded intervention parent WELL is ambiguous.");found[well.UWI]=revision;
                }
            }
            if(found.Count>256)throw RefusalException.Conflict("Narrow the reviewed intervention WELL population.");return found;
        }
        var initial=await Parents(grants);var closing=await Parents(await GrantsAsync(parentPaths));
        if(initial.Count!=closing.Count || initial.Any(pair=>!closing.TryGetValue(pair.Key,out var value) || pair.Value!=value))throw RefusalException.Conflict("Intervention parent WELL evidence or authority changed during review.");
        await WellActivityDefinitionAsync();await reader.RequireCurrentAsync();
        return initial.Keys.Order(StringComparer.Ordinal).Select(id=>(IReadOnlyList<AppFilter>)new[]{CompanyPPDMEvidenceReader.Equal("UWI",id)}).ToArray();
    }
}
