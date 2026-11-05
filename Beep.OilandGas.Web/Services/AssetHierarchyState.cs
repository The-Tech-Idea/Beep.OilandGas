using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Web.Services;

/// <summary>One asset tree's confirmed company/selector snapshot; never a local authorization authority.</summary>
public sealed class AssetHierarchyState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly Func<string, CancellationToken, Task<AssetHierarchyNode?>> load;
    private readonly Func<Exception, string, string, string> explain;
    private CancellationTokenSource? pending;
    private long generation;
    private bool disposed;
    private (long Version, CompanyClientSelection? Selection)? loadedScope;
    private readonly Dictionary<AssetHierarchyNode, (string Type, string Id, string? Facility, string? ParentType, string? ParentId)> selectable = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<AssetHierarchyNode, Beep.OilandGas.Models.Data.WorkOrder.WorkOrderComponentReference[]> componentSnapshots = new(ReferenceEqualityComparer.Instance);
    public AssetHierarchyNode? Hierarchy { get; private set; }
    public bool Loading { get; private set; }
    public bool NeedsCompany { get; private set; }
    public string? Error { get; private set; }
    public event Action? Changed;

    public AssetHierarchyState(CompanyClientContext company,
        Func<string, CancellationToken, Task<AssetHierarchyNode?>> load,
        Func<Exception, string, string, string> explain)
    {
        this.company = company; this.load = load; this.explain = explain;
        company.Changed += Invalidate;
    }

    public async Task LoadAsync(string? organizationId = null, string? rootAssetId = null,
        string? rootAssetType = null, string? rootFacilityType = null)
    {
        if (disposed) return;
        Invalidate();
        var version = generation;
        var scope = company.Snapshot;
        if (scope.Selection is null) { NeedsCompany = true; Changed?.Invoke(); return; }
        using var cancellation = new CancellationTokenSource(); pending = cancellation;
        Loading = true; NeedsCompany = false; Changed?.Invoke();
        bool Current() => !disposed && generation == version && company.Snapshot == scope;
        try
        {
            var selected = scope.Selection;
            if (organizationId is not null && organizationId != selected.Scope.BusinessAssociateId)
                throw new InvalidOperationException("The hierarchy organization must match the selected company.");
            var type = rootAssetType?.ToUpperInvariant();
            if (rootAssetId is not null || type is not null || rootFacilityType is not null)
                ValidateKey(rootAssetId, type, rootFacilityType);
            var query = new List<string> { "organizationId=" + Uri.EscapeDataString(selected.Scope.BusinessAssociateId) };
            if (rootAssetId is not null)
            {
                query.Add("rootAssetId=" + Uri.EscapeDataString(rootAssetId));
                query.Add("rootAssetType=" + Uri.EscapeDataString(type!));
                if (rootFacilityType is not null) query.Add("rootFacilityType=" + Uri.EscapeDataString(rootFacilityType));
            }
            var result = await load("/api/AssetHierarchy/user/" + Uri.EscapeDataString(selected.UserId) + "?" + string.Join('&', query), cancellation.Token);
            if (!Current()) return;
            if (result is null) throw new InvalidOperationException("The hierarchy API returned no result.");
            if (rootAssetId is null ? result.AssetType != "ORGANIZATION" || result.AssetId != selected.Scope.BusinessAssociateId :
                result.AssetId != rootAssetId || result.AssetType != type || result.FacilityType != rootFacilityType)
                throw new InvalidOperationException("The hierarchy response does not match its company or requested root.");
            var nodes = new HashSet<AssetHierarchyNode>(ReferenceEqualityComparer.Instance);
            var keys = new HashSet<(string Type, string Id, string? Facility)>();
            ValidateNode(result, null, selected.Scope.BusinessAssociateId, nodes, keys, 0);
            if (!Current()) return;
            foreach (var node in nodes.Where(node => node.AssetType != "ORGANIZATION"))
            {
                selectable.Add(node, Identity(node));
                componentSnapshots.Add(node,node.WorkOrderComponents.ToArray());
            }
            Hierarchy = result; loadedScope = scope;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested || !Current()) { }
        catch (Exception failure)
        {
            var sentence = explain(failure, "loading the asset hierarchy", "The asset hierarchy could not be confirmed");
            if (Current()) Error = sentence;
        }
        finally
        {
            if (ReferenceEquals(pending, cancellation)) pending = null;
            if (Current()) { Loading = false; Changed?.Invoke(); }
        }
    }

    public bool CanSelect(AssetHierarchyNode node) => !disposed && !Loading && Error is null &&
        loadedScope is { } scope && scope == company.Snapshot && node.UserHasAccess &&
        selectable.TryGetValue(node, out var key) && key == Identity(node) &&
        componentSnapshots.TryGetValue(node,out var components) && node.WorkOrderComponents is not null && components.SequenceEqual(node.WorkOrderComponents);

    private static (string, string, string?, string?, string?) Identity(AssetHierarchyNode node) =>
        (node.AssetType, node.AssetId, node.FacilityType, node.ParentAssetType, node.ParentAssetId);

    private static void ValidateNode(AssetHierarchyNode node, AssetHierarchyNode? parent, string company,
        HashSet<AssetHierarchyNode> nodes, HashSet<(string Type, string Id, string? Facility)> keys, int depth)
    {
        if (node is null || depth > 2 || nodes.Count >= 4096 || !nodes.Add(node) ||
            !keys.Add((node.AssetType, node.AssetId, node.FacilityType)) || node.Children is null || node.WorkOrderComponents is null)
            throw new InvalidOperationException("The hierarchy contains an invalid, duplicated or recursive node.");
        if (node.AssetType == "ORGANIZATION")
        {
            if (parent is not null || node.AssetId != company || node.FacilityType is not null || node.UserHasAccess)
                throw new InvalidOperationException("The hierarchy company container is inconsistent.");
        }
        else
        {
            ValidateKey(node.AssetId, node.AssetType, node.FacilityType);
            if(node.AssetType=="WORK_ORDER")
            {
                Beep.OilandGas.Models.Data.AccessControl.WorkOrderParentAccess.Fields(node.AssetId,node.WorkOrderComponents);
                if(node.ParentAssetId is not null || node.ParentAssetType is not null)
                    throw new InvalidOperationException("A work order cannot be presented with an inferred single parent.");
            }
            if (!node.UserHasAccess || node.AssetType != node.AssetType.ToUpperInvariant() ||
                node.AssetType != "FIELD" && node.Children.Count != 0)
                throw new InvalidOperationException("The hierarchy contains an unconfirmed asset or relationship.");
            if (parent?.AssetType == "FIELD" && (node.AssetType == "FIELD" || node.ParentAssetId != parent.AssetId || node.ParentAssetType != "FIELD") ||
                parent?.AssetType == "ORGANIZATION" && node.AssetType == "FIELD" &&
                    (node.ParentAssetId != company || node.ParentAssetType != "ORGANIZATION"))
                throw new InvalidOperationException("The hierarchy child does not match its stored parent relationship.");
        }
        if(node.AssetType=="EQUIPMENT" && (node.ParentAssetId is not null || node.ParentAssetType is not null || parent?.AssetType=="FIELD"))
            throw new InvalidOperationException("Equipment requires separate reviewed installation evidence before it can be attached to a FIELD.");
        if(node.AssetType!="WORK_ORDER" && node.WorkOrderComponents.Count!=0)
            throw new InvalidOperationException("Recorded order components belong only to order nodes.");
        foreach (var child in node.Children) ValidateNode(child, node, company, nodes, keys, depth + 1);
    }

    private static void ValidateKey(string? id, string? type, string? facility)
    {
        if (string.IsNullOrWhiteSpace(id) || type is not ("FIELD" or "WELL" or "POOL" or "FACILITY" or "WORK_ORDER" or "EQUIPMENT") ||
            (type == "FACILITY" ? string.IsNullOrWhiteSpace(facility) : facility is not null))
            throw new InvalidOperationException("A hierarchy selection requires a reviewed asset type and its complete key.");
    }

    public void Invalidate()
    {
        ++generation; var previous = pending; pending = null;
        Hierarchy = null; loadedScope = null; selectable.Clear(); componentSnapshots.Clear(); Loading = false; Error = null;
        NeedsCompany = company.Selection is null;
        previous?.Cancel();
        if (!disposed) Changed?.Invoke();
    }
    public void Dispose() { if (disposed) return; disposed = true; company.Changed -= Invalidate; Invalidate(); }
}
