using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.AccessControl;

/// <summary>Stored, active PPDM relationships visible under a mandatory company and subject row policy.</summary>
public sealed class AssetHierarchyService : IAssetHierarchyService
{
    private readonly IDMEEditor editor;
    private readonly ICommonColumnHandler columns;
    private readonly IPPDM39DefaultsRepository defaults;
    private readonly IPPDMMetadataRepository metadata;
    private readonly Func<Task<AssetDatabaseScope>> resolveScope;
    private readonly Func<string?, IPPDMRowAccessPolicy> policies;
    // Null subject requires a current company Administrator; other subjects require self or current company Administrator.
    private readonly Func<string?, Task<string>> resolveCompany;
    private readonly IHierarchyConfigurationStore configurations;
    private readonly Func<string, Task<List<WorkOrderComponentReference>>> orderComponents;

    public AssetHierarchyService(IDMEEditor editor, ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata, Func<Task<AssetDatabaseScope>> resolveScope,
        Func<string?, IPPDMRowAccessPolicy> policies, Func<string?, Task<string>> resolveCompany, IHierarchyConfigurationStore configurations,
        Func<string, Task<List<WorkOrderComponentReference>>> orderComponents)
    {
        this.editor = editor ?? throw new ArgumentNullException(nameof(editor));
        this.columns = columns ?? throw new ArgumentNullException(nameof(columns));
        this.defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
        this.metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        this.resolveScope = resolveScope ?? throw new ArgumentNullException(nameof(resolveScope));
        this.policies = policies ?? throw new ArgumentNullException(nameof(policies));
        this.resolveCompany = resolveCompany ?? throw new ArgumentNullException(nameof(resolveCompany));
        this.configurations = configurations ?? throw new ArgumentNullException(nameof(configurations));
        this.orderComponents = orderComponents ?? throw new ArgumentNullException(nameof(orderComponents));
    }

    public Task<AssetHierarchyNode?> GetAssetHierarchyAsync(string organizationId, string? rootAssetId = null,
        string? rootAssetType = null, string? rootFacilityType = null) =>
        HierarchyAsync(null, organizationId, rootAssetId, rootAssetType, rootFacilityType);

    public Task<AssetHierarchyNode?> GetAssetHierarchyForUserAsync(string userId, string? organizationId = null,
        string? rootAssetId = null, string? rootAssetType = null, string? rootFacilityType = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return HierarchyAsync(userId, organizationId, rootAssetId, rootAssetType, rootFacilityType);
    }

    private async Task<AssetHierarchyNode?> HierarchyAsync(string? subject, string? organization,
        string? id, string? type, string? facility)
    {
        var context = await BeginAsync(subject, organization);
        if (id is not null || type is not null || facility is not null)
        {
            var root = await FindAsync(context, id!, type!, facility);
            if (root is null) { await FinishAsync(context); return null; }
            root.Children = await ChildrenAsync(context, root);
            var visibleRoot = ApplyConfiguration(context, root);
            await FinishAsync(context);
            return visibleRoot;
        }
        // A company container is a presentation node, never an asset grant or a fabricated PPDM entity.
        var container = new AssetHierarchyNode { AssetId = context.Company, AssetType = "ORGANIZATION" };
        var nodes = new List<AssetHierarchyNode>();
        foreach (var rowType in new[] { typeof(FIELD), typeof(WELL), typeof(POOL), typeof(FACILITY), typeof(WORK_ORDER), typeof(EQUIPMENT) })
            nodes.AddRange((await ReadAsync(context, rowType, [])).Select(Node));
        foreach(var order in nodes.Where(node=>node.AssetType=="WORK_ORDER"))await PopulateOrderAsync(context,order);
        foreach (var field in nodes.Where(node => node.AssetType == "FIELD"))
        {
            field.ParentAssetId = context.Company; field.ParentAssetType = "ORGANIZATION";
            field.Children = nodes.Where(node => node.ParentAssetType == "FIELD" && node.ParentAssetId == field.AssetId).ToList();
            container.Children.Add(field);
        }
        // A direct child grant does not confer access to its parent. Keep it visible without inventing a parent row.
        container.Children.AddRange(nodes.Where(node => node.AssetType != "FIELD" &&
            !nodes.Any(parent => parent.AssetType == "FIELD" && parent.AssetId == node.ParentAssetId)));
        ApplyConfiguration(context, container);
        await FinishAsync(context);
        return container;
    }

    public async Task<List<AssetHierarchyNode>> GetAssetChildrenAsync(string assetId, string assetType,
        string? organizationId = null, string? facilityType = null)
    {
        var context = await BeginAsync(null, organizationId);
        var parent = await FindAsync(context, assetId, assetType, facilityType);
        var children = parent is null ? [] : await ChildrenAsync(context, parent);
        children = children.Select(node => ApplyConfiguration(context, node)).OfType<AssetHierarchyNode>().ToList();
        await FinishAsync(context);
        return children;
    }

    private async Task<List<AssetHierarchyNode>> ChildrenAsync(Context context, AssetHierarchyNode parent)
    {
        var children = new List<AssetHierarchyNode>();
        if (parent.AssetType != "FIELD") return children;
        foreach (var (type, column) in new[] { (typeof(WELL), "ASSIGNED_FIELD"), (typeof(POOL), "FIELD_ID"), (typeof(FACILITY), "PRIMARY_FIELD_ID") })
            children.AddRange((await ReadAsync(context, type, [Equal(column, parent.AssetId)])).Select(Node));
        return children;
    }

    public async Task<List<AssetHierarchyNode>> GetAssetPathAsync(string assetId, string assetType,
        string? organizationId = null, string? facilityType = null)
    {
        var context = await BeginAsync(null, organizationId);
        var leaf = await FindAsync(context, assetId, assetType, facilityType);
        var path = new List<AssetHierarchyNode>();
        if (leaf is not null)
        {
            if (leaf.ParentAssetType == "FIELD" && leaf.ParentAssetId is not null)
            {
                var parent = await FindAsync(context, leaf.ParentAssetId, "FIELD", null);
                if (parent is not null) path.Add(parent);
            }
            path.Add(leaf);
        }
        await FinishAsync(context);
        return path;
    }

    public async Task<bool> ValidateAccessAsync(string userId, List<AssetHierarchyNode> assetPath)
    {
        if (string.IsNullOrWhiteSpace(userId) || assetPath is null || assetPath.Count is 0 or > 3 ||
            assetPath.Any(node => node is null || string.IsNullOrWhiteSpace(node.AssetId) || string.IsNullOrWhiteSpace(node.AssetType))) return false;
        var context = await BeginAsync(userId, null);
        var stored = new List<AssetHierarchyNode>();
        var valid = true;
        foreach (var supplied in assetPath)
        {
            if (supplied.AssetType == "ORGANIZATION")
            {
                if (stored.Count != 0 || supplied.AssetId != context.Company || supplied.FacilityType is not null || assetPath.Count == 1)
                { valid = false; break; }
                stored.Add(new() { AssetId = context.Company, AssetType = "ORGANIZATION" });
                continue;
            }
            var node = await FindAsync(context, supplied.AssetId, supplied.AssetType, supplied.FacilityType);
            if (node is null || stored.Any(previous => previous.AssetId == node.AssetId && previous.AssetType == node.AssetType && previous.FacilityType == node.FacilityType))
            { valid = false; break; }
            if (stored.Count != 0)
            {
                var previous = stored[^1];
                if (previous.AssetType == "ORGANIZATION" ? node.AssetType != "FIELD" :
                    previous.AssetType != "FIELD" || node.ParentAssetType != "FIELD" || node.ParentAssetId != previous.AssetId)
                { valid = false; break; }
            }
            var parentId = node.AssetType == "FIELD" ? context.Company : node.ParentAssetId;
            var parentType = node.AssetType == "FIELD" ? "ORGANIZATION" : node.ParentAssetType;
            if (supplied.ParentAssetId is not null && supplied.ParentAssetId != parentId ||
                supplied.ParentAssetType is not null && supplied.ParentAssetType != parentType)
            { valid = false; break; }
            if(node.AssetType=="WORK_ORDER" && (supplied.WorkOrderComponents is null ||
                !node.WorkOrderComponents.OrderBy(row=>row.ComponentId,StringComparer.Ordinal).SequenceEqual(supplied.WorkOrderComponents.OrderBy(row=>row.ComponentId,StringComparer.Ordinal))))
            {valid=false;break;}
            stored.Add(node);
        }
        await FinishAsync(context);
        return valid;
    }

    public Task<List<HierarchyConfig>> GetHierarchyConfigAsync(string organizationId) =>
        configurations.ReadAsync(organizationId, requireAdministrator: true);

    public Task<bool> UpdateHierarchyConfigAsync(string organizationId, List<HierarchyConfig> config) =>
        configurations.UpdateAsync(organizationId, config);

    private static AssetHierarchyNode? ApplyConfiguration(Context context, AssetHierarchyNode node)
    {
        var setting = context.Configuration.SingleOrDefault(level => level.AssetType == node.AssetType);
        if (setting is not null && !setting.Active) return null;
        node.LevelName = setting?.LevelName;
        node.Children = node.Children.Select(child => ApplyConfiguration(context, child)).OfType<AssetHierarchyNode>().ToList();
        return node;
    }

    private async Task<Context> BeginAsync(string? subject, string? organization)
    {
        var company = await resolveCompany(subject);
        if (string.IsNullOrWhiteSpace(company)) throw RefusalException.Forbidden("An active company selection is required for the hierarchy.");
        if (organization is not null && organization != company)
            throw RefusalException.Forbidden("The hierarchy organization must match the selected company.");
        var scope = await resolveScope();
        if (scope is null || string.IsNullOrWhiteSpace(scope.ConnectionName) || string.IsNullOrWhiteSpace(scope.Fingerprint))
            throw new InvalidOperationException("A reviewed module database scope is required for the hierarchy.");
        return new(scope, company, subject, policies(subject) ?? throw new InvalidOperationException("A subject row policy is required for the hierarchy."),
            await configurations.ReadAsync(company));
    }

    private async Task<List<object>> ReadAsync(Context context, Type type, List<AppFilter> filters)
    {
        if (context.Scope != await resolveScope() || context.Company != await resolveCompany(context.Subject))
            throw RefusalException.Conflict("The hierarchy company or database binding changed during this operation.");
        var source = editor.GetDataSource(context.Scope.ConnectionName)
            ?? throw new InvalidOperationException("The hierarchy datasource is unavailable.");
        MigrationConnectionTarget.Validate(editor, source, context.Scope.ConnectionName);
        filters = [.. filters, Equal("ACTIVE_IND", "Y")];
        var repository = new PPDMGenericRepository(editor, columns, defaults, metadata, type,
            context.Scope.ConnectionName, type.Name, rowAccess: context.Policy);
        var rows = (await repository.GetAsync(filters)).ToList();
        MigrationConnectionTarget.Validate(editor, source, context.Scope.ConnectionName);
        context.Rows.AddRange(rows);
        return rows;
    }

    private async Task<AssetHierarchyNode?> FindAsync(Context context, string id, string assetType, string? facility)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(assetType))
            throw RefusalException.Invalid("Asset ID and type are required for a hierarchy selection.");
        var type = assetType.ToUpperInvariant() switch
        {
            "FIELD" => typeof(FIELD), "WELL" => typeof(WELL), "POOL" => typeof(POOL), "FACILITY" => typeof(FACILITY), "WORK_ORDER" => typeof(WORK_ORDER), "EQUIPMENT" => typeof(EQUIPMENT),
            _ => throw RefusalException.Invalid("This asset type has no reviewed hierarchy relationship.")
        };
        if (type == typeof(FACILITY) ? string.IsNullOrWhiteSpace(facility) : facility is not null)
            throw RefusalException.Invalid("A facility requires FACILITY_ID and FACILITY_TYPE; the qualifier applies only to facilities.");
        var key = type.Name switch { "FIELD" => "FIELD_ID", "WELL" => "UWI", "POOL" => "POOL_ID", "WORK_ORDER" => "WORK_ORDER_ID", "EQUIPMENT" => "EQUIPMENT_ID", _ => "FACILITY_ID" };
        var filters = new List<AppFilter> { Equal(key, id) };
        if (facility is not null) filters.Add(Equal("FACILITY_TYPE", facility));
        var rows = await ReadAsync(context, type, filters);
        if (rows.Count > 1) throw RefusalException.Conflict("The hierarchy selection returned an ambiguous asset identity.");
        if(rows.Count==0)return null;
        var node=Node(rows[0]);if(type==typeof(WORK_ORDER))await PopulateOrderAsync(context,node);return node;
    }

    private async Task PopulateOrderAsync(Context context,AssetHierarchyNode node)
    {
        node.WorkOrderComponents=await orderComponents(node.AssetId);
        WorkOrderParentAccess.Fields(node.AssetId,node.WorkOrderComponents);
        context.OrderComponents[node.AssetId]=node.WorkOrderComponents.ToArray();
    }

    private async Task FinishAsync(Context context)
    {
        if (context.Scope != await resolveScope() || context.Company != await resolveCompany(context.Subject))
            throw RefusalException.Conflict("The hierarchy company or database binding changed during this operation.");
        foreach (var group in context.Rows.GroupBy(row => row.GetType()))
            await context.Policy.ValidateRowsAsync(group.Key, group.Key.Name, group.ToList());
        foreach(var order in context.OrderComponents)
            if(!order.Value.OrderBy(row=>row.ComponentId,StringComparer.Ordinal).SequenceEqual((await orderComponents(order.Key)).OrderBy(row=>row.ComponentId,StringComparer.Ordinal)))
                throw RefusalException.Conflict("Recorded order relationships changed during hierarchy review.");
        var orderRows=context.Rows.Where(row=>row is WORK_ORDER).ToList();
        if(orderRows.Count!=0)await context.Policy.ValidateRowsAsync(typeof(WORK_ORDER),"WORK_ORDER",orderRows);
        var currentConfiguration = await configurations.ReadAsync(context.Company);
        if (currentConfiguration.FirstOrDefault()?.Revision != context.Configuration.FirstOrDefault()?.Revision)
            throw RefusalException.Conflict("Hierarchy configuration changed during the asset read. Refresh before continuing.");
        // Policy validation itself awaits current grants and metadata; recheck the captured target after it.
        if (context.Scope != await resolveScope() || context.Company != await resolveCompany(context.Subject))
            throw RefusalException.Conflict("The hierarchy company or database binding changed during this operation.");
        if (context.Rows.Count != 0)
        {
            var source = editor.GetDataSource(context.Scope.ConnectionName)
                ?? throw new InvalidOperationException("The hierarchy datasource is unavailable.");
            MigrationConnectionTarget.Validate(editor, source, context.Scope.ConnectionName);
        }
    }

    private static AssetHierarchyNode Node(object row) => row switch
    {
        FIELD value => new() { AssetId = value.FIELD_ID, AssetType = "FIELD", AssetName = value.FIELD_NAME, UserHasAccess = true },
        WELL value => Child(value.UWI, "WELL", value.WELL_NAME, value.ASSIGNED_FIELD),
        POOL value => Child(value.POOL_ID, "POOL", value.POOL_NAME, value.FIELD_ID),
        FACILITY value => Child(value.FACILITY_ID, "FACILITY", value.FACILITY_LONG_NAME ?? value.FACILITY_SHORT_NAME, value.PRIMARY_FIELD_ID, value.FACILITY_TYPE),
        WORK_ORDER value => new() {AssetId=value.WORK_ORDER_ID,AssetType="WORK_ORDER",AssetName=value.WORK_ORDER_NUMBER,UserHasAccess=true},
        EQUIPMENT value => new() {AssetId=value.EQUIPMENT_ID,AssetType="EQUIPMENT",AssetName=value.EQUIPMENT_NAME,UserHasAccess=true},
        _ => throw RefusalException.Conflict("The hierarchy returned an unreviewed asset model.")
    };

    private static AssetHierarchyNode Child(string id, string type, string? name, string? field, string? facility = null) =>
        new() { AssetId = id, AssetType = type, FacilityType = facility, AssetName = name, ParentAssetId = field,
            ParentAssetType = string.IsNullOrWhiteSpace(field) ? null : "FIELD", UserHasAccess = true };
    private static AppFilter Equal(string column, string value) => new() { FieldName = column, Operator = "=", FilterValue = value };
    private sealed record Context(AssetDatabaseScope Scope, string Company, string? Subject, IPPDMRowAccessPolicy Policy, List<HierarchyConfig> Configuration)
    {
        public List<object> Rows { get; } = [];
        public Dictionary<string,WorkOrderComponentReference[]> OrderComponents {get;}=new(StringComparer.Ordinal);
    }
}
