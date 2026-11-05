using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Processes;
namespace Beep.OilandGas.ApiService.Services;
/// <summary>Existing definition-list orchestration, including the configured financial category.</summary>
public static class ProcessDefinitionCatalog
{
    public static async Task<List<ProcessDefinition>> ReadAsync(Func<string,Task<List<ProcessDefinition>>> read)
    {
        string[] categories=["WORK_ORDER","GATE_REVIEW","HSE","COMPLIANCE","FINANCIAL","WELL_LIFECYCLE","FACILITY_LIFECYCLE","RESERVOIR","PIPELINE"];
        var rows=new List<ProcessDefinition>();
        foreach(var category in categories)
        {
            var found=await read(category);
            if(found is null || found.Any(row=>row is null || string.IsNullOrWhiteSpace(row.ProcessId) || row.ProcessType!=category))
                throw new InvalidOperationException("The configured process category could not be confirmed.");
            rows.AddRange(found);
        }
        if(rows.Select(row=>row.ProcessId).Distinct(StringComparer.Ordinal).Count()!=rows.Count)
            throw new InvalidOperationException("The configured process catalog contains ambiguous definition identities.");
        return rows;
    }
}
