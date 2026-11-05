using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
namespace Beep.OilandGas.Web.Services;
public sealed partial class DevelopmentDraftIntentStore
{
    public async Task<DrillingReportingClosure?> ReadReportingClosureAsync(CompanyScope company,string actor)
    {
        var saved=await storage.GetAsync<DrillingReportingClosure>(Key(company,actor)+".drilling-reporting-closure");if(!saved.Success)return null;
        var value=saved.Value??throw new InvalidOperationException("The original reporting closure input is unavailable.");value.Require();
        if(value.Scope().Company!=company || value.Scope().ActorUserId!=actor)throw new InvalidOperationException("The retained reporting closure belongs to another company or account.");
        return value;
    }
    public async Task RetainReportingClosureAsync(DrillingReportingClosure original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadReportingClosureAsync(scope.Company,scope.ActorUserId);
            if(await ReadReportingReassignmentAsync(scope.Company,scope.ActorUserId) is not null)throw new InvalidOperationException("Recover the retained reporting transfer before retaining a closure.");
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original reporting closure before preparing another.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".drilling-reporting-closure",original);
            if((await ReadReportingClosureAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original reporting closure was not confirmed retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearReportingClosureAsync(DrillingReportingClosure original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadReportingClosureAsync(scope.Company,scope.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The retained reporting closure changed and was preserved.");
            await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".drilling-reporting-closure");
        }
        finally{gate.Release();}
    }
}
