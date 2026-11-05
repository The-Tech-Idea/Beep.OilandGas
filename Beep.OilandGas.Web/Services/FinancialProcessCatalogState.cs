using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Processes;
namespace Beep.OilandGas.Web.Services;
/// <summary>Display projection of the existing configured process catalog; grants no execution authority.</summary>
public sealed class FinancialProcessCatalogState:IDisposable
{
    private readonly CompanyClientContext company;
    private bool disposed;
    public int Version {get;private set;}
    public bool Confirmed {get;private set;}
    public string? FieldId {get;private set;}
    public DateTimeOffset? LoadedAtUtc {get;private set;}
    public IReadOnlyList<Item> Rows {get;private set;}=[];
    public FinancialProcessCatalogState(CompanyClientContext company){this.company=company;company.Changed+=Clear;}
    public void Clear(){++Version;Confirmed=false;FieldId=null;LoadedAtUtc=null;Rows=[];}
    public async Task LoadAsync(Func<Task<string?>> lookup,Func<Task<List<ProcessDefinition>>> read)
    {
        if(disposed)return;Clear();var version=Version;var scope=company.Snapshot;
        if(scope.Selection is null)return;
        var field=await lookup();if(disposed || Version!=version || scope!=company.Snapshot || string.IsNullOrWhiteSpace(field))return;
        var definitions=await read();if(disposed || Version!=version || scope!=company.Snapshot)return;
        if(definitions is null || definitions.Any(row=>row is null || string.IsNullOrWhiteSpace(row.ProcessId) || string.IsNullOrWhiteSpace(row.ProcessType)) ||
            definitions.Select(row=>row.ProcessId).Distinct(StringComparer.Ordinal).Count()!=definitions.Count)
            throw new InvalidOperationException("The configured process catalog is unavailable or ambiguous.");
        var financial=definitions.Where(row=>row.IsActive && row.ProcessType=="FINANCIAL").ToArray();
        if(financial.Any(row=>string.IsNullOrWhiteSpace(row.ProcessName) || string.IsNullOrWhiteSpace(row.EntityType)))
            throw new InvalidOperationException("Configured financial definitions have unconfirmed source identities.");
        Rows=Array.AsReadOnly(financial.Select(row=>new Item(row.ProcessId,row.ProcessName,row.Version,row.EntityType,row.Description,Metadata(row,"regulation")))
            .OrderBy(row=>row.Name,StringComparer.CurrentCulture).ThenBy(row=>row.Id,StringComparer.Ordinal).ToArray());
        FieldId=field;LoadedAtUtc=DateTimeOffset.UtcNow;Confirmed=true;
    }
    private static string? Metadata(ProcessDefinition row,string key)
    {
        if(row.Configuration?.TryGetValue(key,out var value)!=true)return null;
        return value switch{string text when !string.IsNullOrWhiteSpace(text)=>text,JsonElement json when json.ValueKind==JsonValueKind.String=>json.GetString(),_=>null};
    }
    public void Dispose(){disposed=true;company.Changed-=Clear;Clear();}
    public sealed record Item(string Id,string Name,string? Version,string EntityType,string? Description,string? Regulation)
    {public string Route=>"/ppdm39/process/definitions/"+Uri.EscapeDataString(Id);}
}
