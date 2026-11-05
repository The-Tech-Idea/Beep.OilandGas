using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Data.OilGas;
namespace TheTechIdea.Data.OilGas;
/// <summary>Preflight digest of recorded activity history; not an atomic provider compare-and-swap.</summary>
public static class ProductionInterventionActivityChecks
{
    public static WELL_ACTIVITY OriginalSource(IEnumerable<WELL_ACTIVITY> activities,string well)
    {
        var rows=activities.ToArray();
        if(rows.Any(x=>x is null || x.UWI!=well) || rows.Select(x=>x.ACTIVITY_OBS_NO).Distinct().Count()!=rows.Length)
            throw new InvalidOperationException("Recorded source activity is ambiguous.");
        return rows.Where(x=>!IsDecisionActivity(x.ACTIVITY_TYPE_ID)).OrderByDescending(x=>x.ACTIVITY_OBS_NO).FirstOrDefault()
            ?? throw new InvalidOperationException("An original source activity is unavailable; a decision record cannot replace it.");
    }
    public static void RequireDecision(IEnumerable<WELL_ACTIVITY> activities,ProductionInterventionDecisionRequest request,CompanyScope company,string actor,string field,string well)
    {
        var rows=activities.ToArray();
        var decision=ProductionInterventionDecisionChecks.Require(request,field);
        ProductionInterventionDecisionChecks.RequireRevision(request.ExpectedSourceRevision,Revision(rows,company,actor,field,well));
        var source=OriginalSource(rows,well);
        if(request.InterventionType!=(source.ACTIVITY_TYPE_ID ?? string.Empty) || request.Problem!=RemoveWorkflowMarkers(source.REMARK ?? string.Empty))
            throw new InvalidOperationException("Submitted intervention content differs from the reviewed source activity.");
        if(decision=="APPROVED")ProductionInterventionDecisionChecks.RequireIndependentApprover(source.ROW_CREATED_BY,actor);
    }
        public static bool IsDecisionActivity(string? activityTypeId)
        {
            return string.Equals(activityTypeId, "DECOMMISSIONING_TRIGGERED", StringComparison.OrdinalIgnoreCase)
                || (activityTypeId?.StartsWith("WORKOVER_", StringComparison.OrdinalIgnoreCase) == true
                    && (activityTypeId.Contains("APPROVED", StringComparison.OrdinalIgnoreCase)
                        || activityTypeId.Contains("DEFERRED", StringComparison.OrdinalIgnoreCase)
                        || activityTypeId.Contains("REJECTED", StringComparison.OrdinalIgnoreCase)
                        || activityTypeId.Contains("REVIEWED", StringComparison.OrdinalIgnoreCase)));
        }

    public static string RemoveWorkflowMarkers(string remark)=>ProductionInterventionMarkers.Note(remark);

    public static string Revision(IEnumerable<WELL_ACTIVITY>? activities,CompanyScope company,string actor,string field,string well)
    {
        if(activities is null || string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(well))
            throw new InvalidOperationException("Original activity review is unavailable.");
        var rows=activities.ToArray();
        if(rows.Length==0 || rows.Any(x=>x is null || x.UWI!=well) || rows.Select(x=>x.ACTIVITY_OBS_NO).Distinct().Count()!=rows.Length)
            throw new InvalidOperationException("Recorded activity history is missing or ambiguous.");
        var payload=JsonSerializer.Serialize(new { Company=company, Actor=actor, Field=field, Well=well, Activities=rows.OrderBy(x=>x.ACTIVITY_OBS_NO).ToArray() });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }
}
