using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;
public sealed partial class DevelopmentDraftIntentStore
{
    public async Task<DrillingDailyReportInput?> ReadDailyReportAsync(CompanyScope company, string actor)
    {
        var saved = await storage.GetAsync<DrillingDailyReportInput>(Key(company, actor) + ".drilling-report");
        if (!saved.Success) return null;
        var original = saved.Value ?? throw new InvalidOperationException("The original drilling report input is unavailable.");
        original.Require();
        if (original.Basis.Scope().Company != company || original.Basis.Scope().ActorUserId != actor)
            throw new InvalidOperationException("The retained report belongs to another company/account.");
        return original;
    }
    public async Task RetainDailyReportAsync(DrillingDailyReportInput original)
    {
        original.Require(); await gate.WaitAsync();
        try
        {
            var scope = original.Basis.Scope(); var previous = await ReadDailyReportAsync(scope.Company, scope.ActorUserId);
            if (previous is not null && previous.Revision() != original.Revision()) throw new InvalidOperationException("Recover the original drilling report before preparing another.");
            await storage.SetAsync(Key(scope.Company, scope.ActorUserId) + ".drilling-report", original);
            if ((await ReadDailyReportAsync(scope.Company, scope.ActorUserId))?.Revision() != original.Revision()) throw new InvalidOperationException("The exact drilling report original was not retained.");
        }
        finally { gate.Release(); }
    }
    public async Task ClearDailyReportAsync(DrillingDailyReportInput original)
    {
        original.Require(); await gate.WaitAsync();
        try
        {
            var scope = original.Basis.Scope(); var previous = await ReadDailyReportAsync(scope.Company, scope.ActorUserId);
            if (previous is not null && previous.Revision() != original.Revision()) throw new InvalidOperationException("Another retained original was preserved.");
            await storage.DeleteAsync(Key(scope.Company, scope.ActorUserId) + ".drilling-report");
        }
        finally { gate.Release(); }
    }
}
