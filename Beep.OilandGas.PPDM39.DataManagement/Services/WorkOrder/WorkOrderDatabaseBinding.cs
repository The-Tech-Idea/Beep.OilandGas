using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Core.Refusals;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

/// <summary>Uses the API's existing company/binding resolver; retains one operation's original target.</summary>
internal sealed class WorkOrderDatabaseBinding(Func<Task<(CompanyScope Company,string Actor,AssetDatabaseScope Database)>> resolve)
{
    private readonly object gate=new();
    private (CompanyScope Company,string Actor,AssetDatabaseScope Database)? original;
    public async Task<T> ReadAsync<T>(Func<Task<T>> read)
    {
        await RequireAsync();
        var result=await read();
        await RequireAsync();
        return result;
    }
    public async Task<string> ActorAsync()
    {
        await RequireAsync();
        lock(gate)return original!.Value.Actor;
    }
    public async Task RequireContextAsync(CompanyScope company,string actor,AssetDatabaseScope expected)
    {
        await RequireAsync(actor);lock(gate)if(original is not {} captured || captured.Company!=company || captured.Actor!=actor || captured.Database!=expected)throw RefusalException.Forbidden("Original native company/account/database context differs from the retained command.");
    }
    public async Task<string> RequireAsync(string? actor=null)
    {
        var current=await resolve();
        if(current.Company is null || string.IsNullOrWhiteSpace(current.Company.TenantId) || string.IsNullOrWhiteSpace(current.Company.BusinessAssociateId) ||
           string.IsNullOrWhiteSpace(current.Actor) || current.Database is null || string.IsNullOrWhiteSpace(current.Database.ConnectionName) || string.IsNullOrWhiteSpace(current.Database.Fingerprint))
            throw RefusalException.Conflict("The work-order company database binding is unavailable.");
        if(actor is not null && actor!=current.Actor)throw RefusalException.Forbidden("The work-order change requires the selected company's authenticated account.");
        lock(gate)
        {
            if(original is {} captured && captured!=current)
                throw RefusalException.Conflict("The work-order company, account or physical database binding changed. Review before continuing.");
            original=current;
        }
        return current.Database.ConnectionName;
    }
}
