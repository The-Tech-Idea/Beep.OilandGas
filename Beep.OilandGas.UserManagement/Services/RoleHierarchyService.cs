using System.Collections.Concurrent;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.UserManagement.Models.Identity;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Services;

/// <summary>
/// Resolves inherited permissions based on the ROLE_HIERARCHY table.
/// Used at JWT issuance time to expand a user's direct permissions to include
/// all inherited permissions from parent roles.
/// Registered as singleton — hierarchy changes rarely.
/// </summary>
public interface IRoleHierarchyService
{
    /// <summary>
    /// Given a set of direct role names, returns all role names including inherited parent roles.
    /// </summary>
    Task<HashSet<string>> ExpandRolesWithInheritanceAsync(HashSet<string> directRoleNames);

    /// <summary>
    /// Refresh the in-memory hierarchy cache. Called after ROLE_HIERARCHY changes.
    /// </summary>
    Task RefreshCacheAsync();

    /// <summary>
    /// Get all parent role names (recursively) for a given child role.
    /// </summary>
    HashSet<string> GetParentRoles(string childRoleName);

    /// <summary>
    /// Get all child role names (recursively) for a given parent role.
    /// </summary>
    HashSet<string> GetChildRoles(string parentRoleName);
}

public class RoleHierarchyService : IRoleHierarchyService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly ILogger<RoleHierarchyService> _logger;
    private readonly string _connectionName;

    // In-memory cache: child → set of parent role names
    private ConcurrentDictionary<string, HashSet<string>> _childToParents = new(StringComparer.OrdinalIgnoreCase);
    private ConcurrentDictionary<string, HashSet<string>> _parentToChildren = new(StringComparer.OrdinalIgnoreCase);
    private DateTime _lastRefresh = DateTime.MinValue;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public RoleHierarchyService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<RoleHierarchyService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public HashSet<string> GetParentRoles(string childRoleName)
    {
        return _childToParents.TryGetValue(childRoleName, out var parents)
            ? new HashSet<string>(parents, StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public HashSet<string> GetChildRoles(string parentRoleName)
    {
        return _parentToChildren.TryGetValue(parentRoleName, out var children)
            ? new HashSet<string>(children, StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public async Task<HashSet<string>> ExpandRolesWithInheritanceAsync(HashSet<string> directRoleNames)
    {
        await EnsureCacheAsync();

        var expanded = new HashSet<string>(directRoleNames, StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>(directRoleNames);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (_childToParents.TryGetValue(current, out var parents))
            {
                foreach (var parent in parents)
                {
                    if (expanded.Add(parent))
                    {
                        queue.Enqueue(parent);
                    }
                }
            }
        }

        return expanded;
    }

    public async Task RefreshCacheAsync()
    {
        await _refreshLock.WaitAsync();
        try
        {
            await LoadHierarchyAsync();
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task EnsureCacheAsync()
    {
        if (_lastRefresh != DateTime.MinValue)
            return;

        await _refreshLock.WaitAsync();
        try
        {
            if (_lastRefresh != DateTime.MinValue)
                return;

            await LoadHierarchyAsync();
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task LoadHierarchyAsync()
    {
        try
        {
            var repo = new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                typeof(ROLE_HIERARCHY),
                _connectionName,
                "ROLE_HIERARCHY",
                null);

            var all = (await repo.GetAsync(new List<AppFilter>()))
                .OfType<ROLE_HIERARCHY>()
                .Where(rh => string.Equals(rh.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var childToParents = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            var parentToChildren = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var hierarchy in all)
            {
                // child → parent mapping
                childToParents.AddOrUpdate(
                    hierarchy.CHILD_ROLE_NAME,
                    _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { hierarchy.PARENT_ROLE_NAME },
                    (_, set) => { set.Add(hierarchy.PARENT_ROLE_NAME); return set; });

                // parent → child mapping
                parentToChildren.AddOrUpdate(
                    hierarchy.PARENT_ROLE_NAME,
                    _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { hierarchy.CHILD_ROLE_NAME },
                    (_, set) => { set.Add(hierarchy.CHILD_ROLE_NAME); return set; });
            }

            // Expand recursively: if A→B and B→C, then A→C
            foreach (var (child, parents) in childToParents)
            {
                var allParents = new HashSet<string>(parents, StringComparer.OrdinalIgnoreCase);
                var queue = new Queue<string>(parents);
                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    if (childToParents.TryGetValue(current, out var grandParents))
                    {
                        foreach (var gp in grandParents)
                        {
                            if (allParents.Add(gp))
                                queue.Enqueue(gp);
                        }
                    }
                }
                childToParents[child] = allParents;
            }

            _childToParents = childToParents;
            _parentToChildren = parentToChildren;
            _lastRefresh = DateTime.UtcNow;

            _logger?.LogInformation(
                "RoleHierarchy cache refreshed: {EntryCount} hierarchy entries, {ChildCount} child roles loaded",
                all.Count, childToParents.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load role hierarchy from ROLE_HIERARCHY table");
            // Don't throw — service degrades gracefully (no inheritance)
        }
    }
}
