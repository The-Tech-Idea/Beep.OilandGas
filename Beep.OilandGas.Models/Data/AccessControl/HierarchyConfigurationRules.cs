using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.AccessControl;

public static class HierarchyConfigurationRules
{
    public static string? Validate(string company,IReadOnlyList<HierarchyConfig>? rows)
    {
        string[] children=["WELL","POOL","FACILITY"];
        if(rows is null || rows.Count is <4 or >6 || rows.Any(row=>row is null) ||
            rows.Select(row=>row.AssetType).Distinct(StringComparer.Ordinal).Count()!=rows.Count ||
            rows.Any(row=>row.AssetType is not ("FIELD" or "WELL" or "POOL" or "FACILITY" or "WORK_ORDER" or "EQUIPMENT")) ||
            new[]{"FIELD","WELL","POOL","FACILITY"}.Any(type=>!rows.Any(row=>row.AssetType==type)))
            return "Configure each FIELD, WELL, POOL and FACILITY category once; WORK_ORDER and EQUIPMENT are optional independent categories.";
        if(rows.Any(row=>row.OrganizationId!=company || string.IsNullOrWhiteSpace(row.LevelName) || row.LevelName.Length>128 || row.LevelName.Any(char.IsControl)))
            return "Every category requires the selected company and a display name of at most 128 characters.";
        if(rows.Any(row=>row.AssetType is "FIELD" or "WORK_ORDER" or "EQUIPMENT" ? row.HierarchyLevel!=0 || row.ParentLevel is not null : row.HierarchyLevel!=1 || row.ParentLevel!=0))
            return "FIELD, WORK_ORDER and EQUIPMENT are independent root categories; WELL, POOL and FACILITY are FIELD children.";
        if(!rows.Single(row=>row.AssetType=="FIELD").Active && rows.Any(row=>children.Contains(row.AssetType,StringComparer.Ordinal) && row.Active))
            return "FIELD children cannot be visible when FIELD is hidden.";
        return null;
    }
}
