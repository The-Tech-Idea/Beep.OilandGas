using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
namespace Beep.OilandGas.Web.Services;
public sealed partial class DevelopmentDraftIntentStore
{
    public async Task<DrillingReportingReassignment?> ReadReportingReassignmentAsync(CompanyScope company,string actor)
    {
        var saved=await storage.GetAsync<DrillingReportingReassignment>(Key(company,actor)+".drilling-reporting-transfer");if(!saved.Success)return null;
        var value=saved.Value??throw new InvalidOperationException("The original reporting transfer input is unavailable.");value.Require();
        if(value.Scope().Company!=company || value.Scope().ActorUserId!=actor)throw new InvalidOperationException("The retained reporting transfer belongs to another company or account.");return value;
    }
    public async Task RetainReportingReassignmentAsync(DrillingReportingReassignment original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadReportingReassignmentAsync(scope.Company,scope.ActorUserId);
            if(await ReadReportingClosureAsync(scope.Company,scope.ActorUserId) is not null)throw new InvalidOperationException("Recover the retained reporting closure before retaining a transfer.");
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original reporting transfer before preparing another.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".drilling-reporting-transfer",original);
            if((await ReadReportingReassignmentAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original reporting transfer was not confirmed retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearReportingReassignmentAsync(DrillingReportingReassignment original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadReportingReassignmentAsync(scope.Company,scope.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The retained reporting transfer changed and was preserved.");
            await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".drilling-reporting-transfer");
        }
        finally{gate.Release();}
    }
}
