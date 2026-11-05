using System.Net.Http.Json;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

/// <summary>Uses an operator-supplied key only for the individual setup request.</summary>
public sealed class SetupBootstrapDriverClient(HttpClient http)
{
    public async Task<List<SetupDriverItem>> ListAsync(string key, CancellationToken token = default)
    {
        using var request = Request(HttpMethod.Get, "/api/setup/bootstrap/drivers", key);
        using var response = await http.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SetupDriverItem>>(cancellationToken: token)
            ?? throw new InvalidOperationException("The driver catalog response was empty.");
    }

    public async Task<SetupDriverItem> InstallAsync(string key, SetupDriverItem reviewed, CancellationToken token = default)
    {
        using var request = Request(HttpMethod.Post, $"/api/setup/bootstrap/drivers/{Uri.EscapeDataString(reviewed.Id)}/install", key);
        request.Content = JsonContent.Create(reviewed);
        using var response = await http.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SetupDriverItem>(cancellationToken: token)
            ?? throw new InvalidOperationException("The installation response was empty.");
    }

    public async Task<SetupRepositoryMigrationPlan> PlanRepositoryAsync(string key, CancellationToken token = default)
    {
        using var request = Request(HttpMethod.Get, "/api/setup/bootstrap/repository/migration", key);
        using var response = await http.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SetupRepositoryMigrationPlan>(cancellationToken: token)
            ?? throw new InvalidOperationException("The migration plan was empty.");
    }

    public async Task<List<SetupRepositoryConnectionOption>> RepositoryConnectionsAsync(string key, CancellationToken token = default)
    {
        using var request = Request(HttpMethod.Get, "/api/setup/bootstrap/repository/connections", key);
        using var response = await http.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SetupRepositoryConnectionOption>>(cancellationToken: token)
            ?? throw new InvalidOperationException("The connection catalog was empty.");
    }

    public async Task<SetupRepositoryConnectionSelection> CreateRepositoryConnectionAsync(string key, SetupRepositoryConnectionCreate connection, CancellationToken token = default)
    {
        using var request = Request(HttpMethod.Post, "/api/setup/bootstrap/repository/connections", key);
        request.Content = JsonContent.Create(connection);
        using var response = await http.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SetupRepositoryConnectionSelection>(cancellationToken: token)
            ?? throw new InvalidOperationException("The connection creation was not confirmed.");
    }

    public async Task<SetupRepositoryConnectionSelection> SelectRepositoryConnectionAsync(string key, string name, CancellationToken token = default)
    {
        using var request = Request(HttpMethod.Post, "/api/setup/bootstrap/repository/connection", key);
        request.Content = JsonContent.Create(new SetupRepositoryConnectionSelection(name));
        using var response = await http.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SetupRepositoryConnectionSelection>(cancellationToken: token)
            ?? throw new InvalidOperationException("The selected connection was not confirmed.");
    }

    public async Task<SetupRepositoryConnectionTest> TestRepositoryConnectionAsync(string key, string name, CancellationToken token = default)
    {
        using var request = Request(HttpMethod.Post, "/api/setup/bootstrap/repository/connections/test", key);
        request.Content = JsonContent.Create(new SetupRepositoryConnectionSelection(name));
        using var response = await http.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SetupRepositoryConnectionTest>(cancellationToken: token)
            ?? throw new InvalidOperationException("The connection test was not confirmed.");
        if (!string.Equals(result.ConnectionName, name, StringComparison.Ordinal))
            throw new InvalidOperationException("The connection test returned a different connection.");
        return result;
    }

    public async Task<SetupRepositoryMigrationResult> ApplyRepositoryAsync(string key, SetupRepositoryMigrationPlan plan, CancellationToken token = default)
    {
        using var request = Request(HttpMethod.Post, "/api/setup/bootstrap/repository/migration", key);
        request.Content = JsonContent.Create(new SetupRepositoryMigrationRequest(plan.Provider, plan.TargetToken, plan.CreatesDatabase, plan.PendingMigrations));
        using var response = await http.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SetupRepositoryMigrationResult>(cancellationToken: token)
            ?? throw new InvalidOperationException("The migration result was empty.");
    }

    private static HttpRequestMessage Request(HttpMethod method, string path, string key)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-Setup-Key", key);
        return request;
    }
}
