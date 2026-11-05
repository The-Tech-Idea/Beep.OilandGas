using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
namespace Beep.OilandGas.ApiService.Services;
/// <summary>Preflight digest of recorded activity history; not an atomic provider compare-and-swap.</summary>
public static class ProductionInterventionSourceReviews
{
    public static WELL_ACTIVITY OriginalSource(IEnumerable<WELL_ACTIVITY> activities,string well)
    {
        return ProductionInterventionActivityChecks.OriginalSource(activities,well);
    }
    public static void RequireDecision(IEnumerable<WELL_ACTIVITY> activities,ProductionInterventionDecisionRequest request,CompanyScope company,string actor,string field,string well)
    {
        ProductionInterventionActivityChecks.RequireDecision(activities,request,company,actor,field,well);
    }
        public static bool IsDecisionActivity(string? activityTypeId)
        {
            return ProductionInterventionActivityChecks.IsDecisionActivity(activityTypeId);
        }

    public static string RemoveWorkflowMarkers(string remark)=>ProductionInterventionActivityChecks.RemoveWorkflowMarkers(remark);

    public static string Revision(IEnumerable<WELL_ACTIVITY>? activities,CompanyScope company,string actor,string field,string well)
    {
        return ProductionInterventionActivityChecks.Revision(activities,company,actor,field,well);
    }
}
