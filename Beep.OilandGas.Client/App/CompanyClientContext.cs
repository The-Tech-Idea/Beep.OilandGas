using System.Collections.ObjectModel;
using System.Net.Http;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Client.App;

/// <summary>One client session's selected company. API membership is still checked on every request.</summary>
public sealed class CompanyClientContext : IDisposable
{
    private readonly object gate = new();
    private CancellationTokenSource changes = new();
    private long version;
    private bool disposed;
    private CompanyClientSelection? selection;
    private static readonly HttpRequestOptionsKey<RequestScope> ScopeKey = new("OilGas.CompanyRequestScope");

    public event Action? Changed;
    public long Version { get { lock (gate) return version; } }
    public CompanyClientSelection? Selection { get { lock (gate) return selection; } }
    public (long Version, CompanyClientSelection? Selection) Snapshot { get { lock (gate) return (version, selection); } }

    /// <summary>Immediately clears old scope and cancels outstanding work before validating a new selection.</summary>
    public long Clear()
    {
        CancellationTokenSource previous;
        long next;
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            previous = changes;
            changes = new();
            selection = null;
            next = ++version;
        }
        previous.Cancel();
        previous.Dispose();
        Changed?.Invoke();
        return next;
    }

    public bool TrySelect(long expectedVersion, string userId, RepositoryCompanyAccess access)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(access.Roles);
        ArgumentNullException.ThrowIfNull(access.Permissions);
        Validate(access.Scope);
        lock (gate)
        {
            if (disposed || version != expectedVersion || selection is not null) return false;
            selection = new(userId, access.Scope, Array.AsReadOnly(access.Roles.ToArray()),
                Array.AsReadOnly(access.Permissions.ToArray()));
        }
        Changed?.Invoke();
        return true;
    }

    public CompanyClientSelection RequireSelection() => Selection ??
        throw new InvalidOperationException("Select an available company before accessing business data.");

    /// <summary>Capture before token lookup or other awaits, so an old operation never adopts a new company.</summary>
    public void Prepare(HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Options.TryGetValue(ScopeKey, out var existing))
        {
            if (!ReferenceEquals(existing.Owner, this)) throw new InvalidOperationException("The request belongs to another client session.");
            EnsureCurrent(existing);
            return;
        }
        var uri = request.RequestUri ?? throw new InvalidOperationException("The API request has no address.");
        var path = uri.IsAbsoluteUri ? uri.AbsolutePath : "/" + uri.OriginalString.Split('?', '#')[0].TrimStart('/');
        request.Headers.Remove(CompanyRequestHeaders.Tenant);
        request.Headers.Remove(CompanyRequestHeaders.Company);
        if (!CompanyRequestPolicy.RequiresCompany(path)) return;
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            var current = selection ?? throw new InvalidOperationException("Select an available company before accessing business data.");
            request.Headers.Add(CompanyRequestHeaders.Tenant, current.Scope.TenantId);
            request.Headers.Add(CompanyRequestHeaders.Company, current.Scope.BusinessAssociateId);
            request.Options.Set(ScopeKey, new(this, version, changes.Token));
        }
    }

    public async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage request, CancellationToken token = default)
    {
        Prepare(request);
        if (!request.Options.TryGetValue(ScopeKey, out var scope)) return await client.SendAsync(request, token);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, scope.Changed);
        var response = await client.SendAsync(request, linked.Token);
        response.RequestMessage = request;
        try { EnsureCurrent(scope); return response; }
        catch { response.Dispose(); throw; }
    }

    public void EnsureCurrent(HttpResponseMessage response)
    {
        if (response.RequestMessage?.Options.TryGetValue(ScopeKey, out var scope) == true) EnsureCurrent(scope);
    }

    private void EnsureCurrent(RequestScope scope)
    {
        lock (gate)
            if (disposed || version != scope.Version || !ReferenceEquals(scope.Owner, this))
                throw new OperationCanceledException("The selected company changed.", scope.Changed);
    }

    private static void Validate(CompanyScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        if (string.IsNullOrWhiteSpace(scope.TenantId) || scope.TenantId.Length > 64 ||
            string.IsNullOrWhiteSpace(scope.BusinessAssociateId) || scope.BusinessAssociateId.Length > 128 ||
            scope.TenantId.Any(char.IsControl) || scope.BusinessAssociateId.Any(char.IsControl))
            throw new ArgumentException("The API returned an invalid company scope.", nameof(scope));
    }

    public void Dispose()
    {
        lock (gate) { if (disposed) return; disposed = true; selection = null; ++version; }
        changes.Cancel();
        changes.Dispose();
    }

    private sealed record RequestScope(CompanyClientContext Owner, long Version, CancellationToken Changed);
}

public sealed record CompanyClientSelection(string UserId, CompanyScope Scope,
    ReadOnlyCollection<string> Roles, ReadOnlyCollection<string> Permissions);
