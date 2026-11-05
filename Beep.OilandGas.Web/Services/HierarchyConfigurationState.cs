using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Web.Services;

/// <summary>Confirmed configuration and revision-bound editing for the selected company.</summary>
public sealed class HierarchyConfigurationState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly Func<string, CancellationToken, Task<List<HierarchyConfig>?>> load;
    private readonly Func<Exception, string, string, string> explain;
    private readonly Func<string, List<HierarchyConfig>, CancellationToken, Task<bool>> save;
    private CancellationTokenSource? pending;
    private long generation;
    private bool disposed;
    private (long Version, CompanyClientSelection? Selection)? loadedScope;
    private string? revision;
    public IReadOnlyList<HierarchyConfig> Configuration { get; private set; } = [];
    public bool Loading { get; private set; }
    public bool Loaded { get; private set; }
    public bool Saving { get; private set; }
    public string? Confirmation { get; private set; }
    public bool CanSave => !disposed && Loaded && !Loading && !Saving && loadedScope == company.Snapshot && Configuration.Count is >=4 and <=6;
    public bool CanAddOrderCategory => CanSave && Configuration.All(row=>row.AssetType!="WORK_ORDER");
    public bool CanAddEquipmentCategory => CanSave && Configuration.All(row=>row.AssetType!="EQUIPMENT");
    public string? Error { get; private set; }
    public event Action? Changed;

    public HierarchyConfigurationState(CompanyClientContext company,
        Func<string, CancellationToken, Task<List<HierarchyConfig>?>> load, Func<Exception, string, string, string> explain,
        Func<string, List<HierarchyConfig>, CancellationToken, Task<bool>> save)
    {
        this.company = company; this.load = load; this.explain = explain;
        this.save = save;
        company.Changed += Invalidate;
    }

    public async Task LoadAsync()
    {
        if (disposed) return;
        Invalidate(); var version = generation; var scope = company.Snapshot;
        if (scope.Selection is null) { Error = "Select a company before loading its hierarchy configuration."; Changed?.Invoke(); return; }
        if (!scope.Selection.Roles.Contains("Administrator", StringComparer.Ordinal))
        { Error = "A company Administrator is required to view hierarchy configuration."; Changed?.Invoke(); return; }
        using var cancellation = new CancellationTokenSource(); pending = cancellation;
        Loading = true; Changed?.Invoke();
        bool Current() => !disposed && generation == version && company.Snapshot == scope;
        try
        {
            var id = scope.Selection.Scope.BusinessAssociateId;
            var rows = await load("/api/AssetHierarchy/organization/" + Uri.EscapeDataString(id) + "/config", cancellation.Token);
            if (!Current()) return;
            if (!ValidResponse(id, rows))
                throw new InvalidOperationException("The configuration response does not match the selected company.");
            Configuration = rows!.OrderBy(row => row.HierarchyLevel).ThenBy(row => row.AssetType).ToList(); Loaded = true;
            loadedScope = scope; revision = rows!.FirstOrDefault()?.Revision;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested || !Current()) { }
        catch (Exception failure)
        {
            var sentence = explain(failure, "loading hierarchy configuration", "Hierarchy configuration could not be loaded");
            if (Current()) Error = sentence;
        }
        finally
        {
            if (ReferenceEquals(pending, cancellation)) pending = null;
            if (Current()) { Loading = false; Changed?.Invoke(); }
        }
    }

    public void BeginDraft()
    {
        if (disposed || !Loaded || Loading || Saving || Configuration.Count != 0 || loadedScope is not { Selection: not null } scope || scope != company.Snapshot) return;
        var id = scope.Selection.Scope.BusinessAssociateId;
        Configuration = new[] { "FIELD", "WELL", "POOL", "FACILITY" }.Select(type => new HierarchyConfig
        { OrganizationId = id, AssetType = type, HierarchyLevel = type == "FIELD" ? 0 : 1, ParentLevel = type == "FIELD" ? null : 0,
            LevelName = type switch { "FIELD" => "Fields", "WELL" => "Wells", "POOL" => "Pools", _ => "Facilities" }, Active = true }).ToList();
        revision = null; Error = null; Confirmation = null; Changed?.Invoke();
    }

    public void SetDisplayName(HierarchyConfig level, string? name)
    {
        if (!CanSave || !Configuration.Contains(level)) return;
        level.LevelName = name ?? ""; Confirmation = null; Changed?.Invoke();
    }

    public void AddOrderCategory(){if(CanAddOrderCategory)AddIndependentCategory("WORK_ORDER","Work orders");}
    public void AddEquipmentCategory(){if(CanAddEquipmentCategory)AddIndependentCategory("EQUIPMENT","Equipment");}
    private void AddIndependentCategory(string type,string name)
    {
        Configuration=[..Configuration,new HierarchyConfig{OrganizationId=company.Selection!.Scope.BusinessAssociateId,
            AssetType=type,HierarchyLevel=0,ParentLevel=null,LevelName=name,Active=true,Revision=revision}];
        Confirmation=null;Changed?.Invoke();
    }

    public void SetVisible(HierarchyConfig level, bool visible)
    {
        if (!CanSave || !Configuration.Contains(level)) return;
        if (level.AssetType is not ("FIELD" or "WORK_ORDER" or "EQUIPMENT") && visible && !Configuration.Single(row => row.AssetType == "FIELD").Active) return;
        level.Active = visible;
        if (level.AssetType == "FIELD" && !visible) foreach (var child in Configuration.Where(row=>row.AssetType is not ("WORK_ORDER" or "EQUIPMENT"))) child.Active = false;
        Confirmation = null; Changed?.Invoke();
    }

    public async Task SaveAsync()
    {
        if (!CanSave) return;
        var scope = loadedScope!.Value; var version = generation; var id = scope.Selection!.Scope.BusinessAssociateId;
        // Preserve the confirmed revision and company, rather than accepting mutable DTO audit/identity fields as authority.
        var snapshot = Configuration.Select(row => new HierarchyConfig { OrganizationId = id, AssetType = row.AssetType, HierarchyLevel = row.HierarchyLevel,
            ParentLevel = row.ParentLevel, LevelName = row.LevelName, Active = row.Active, Revision = revision }).ToList();
        if (Beep.OilandGas.Models.Data.AccessControl.HierarchyConfigurationRules.Validate(id,snapshot) is not null)
        { Error = "Enter a display name of at most 128 characters for every category."; Changed?.Invoke(); return; }
        using var cancellation = new CancellationTokenSource(); pending = cancellation; Saving = true; Error = null; Confirmation = null; Changed?.Invoke();
        bool Current() => !disposed && generation == version && company.Snapshot == scope;
        var committed = false;
        try
        {
            var path = "/api/AssetHierarchy/organization/" + Uri.EscapeDataString(id) + "/config";
            var result = await save(path, snapshot, cancellation.Token);
            if (!Current()) return;
            if (!result) throw new InvalidOperationException("The configuration API did not confirm the save.");
            committed = true;
            var rows = await load(path, cancellation.Token);
            if (!Current()) return;
            if (!ValidResponse(id, rows) || rows!.Count != snapshot.Count || rows[0].Revision == revision ||
                !rows.OrderBy(row=>row.AssetType,StringComparer.Ordinal).Select(Content).SequenceEqual(snapshot.OrderBy(row=>row.AssetType,StringComparer.Ordinal).Select(Content)))
                throw new InvalidOperationException("The saved configuration revision could not be confirmed.");
            Configuration = rows.OrderBy(row => row.HierarchyLevel).ThenBy(row => row.AssetType).ToList(); revision = rows[0].Revision;
            Confirmation = "Hierarchy configuration saved and refreshed.";
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested || !Current()) { }
        catch (Exception failure)
        {
            var sentence = explain(failure, committed ? "refreshing saved hierarchy configuration" : "saving hierarchy configuration",
                committed ? "Saved configuration could not be refreshed. Refresh before editing" : "Configuration save could not be confirmed. Refresh before editing");
            if (Current()) { Configuration = []; Loaded = false; loadedScope = null; revision = null; Error = sentence; }
        }
        finally
        {
            if (ReferenceEquals(pending, cancellation)) pending = null;
            if (Current()) { Saving = false; Changed?.Invoke(); }
        }
    }

    private static (string,string,int,int?,string,bool) Content(HierarchyConfig row)=>
        (row.OrganizationId,row.AssetType,row.HierarchyLevel,row.ParentLevel,row.LevelName,row.Active);

    private static bool ValidResponse(string id,List<HierarchyConfig>? rows) => rows is not null &&
        (rows.Count==0 || Beep.OilandGas.Models.Data.AccessControl.HierarchyConfigurationRules.Validate(id,rows) is null &&
         rows.Select(row=>row.Revision).Distinct(StringComparer.Ordinal).Count()==1 &&
         Guid.TryParseExact(rows[0].Revision,"D",out _));

    public void Invalidate()
    {
        ++generation; var previous = pending; pending = null;
        Configuration = []; Loaded = false; Loading = false; Saving = false; Error = null; Confirmation = null; loadedScope = null; revision = null; previous?.Cancel();
        if (!disposed) Changed?.Invoke();
    }
    public void Dispose() { if (disposed) return; disposed = true; company.Changed -= Invalidate; Invalidate(); }
}
