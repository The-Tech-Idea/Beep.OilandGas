using System.Net.Http.Headers;
using System.Net.Http.Json;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public sealed class RepositoryAccountClient(HttpClient http)
{
    public async Task<RepositoryReadiness> GetReadinessAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await http.GetAsync("health/repository", cancellationToken);
            if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.ServiceUnavailable)
                return RepositoryReadiness.Unavailable;
            var payload = await response.Content.ReadFromJsonAsync<RepositoryStatusResponse>(cancellationToken);
            if (payload is null || !Enum.GetNames<RepositoryReadiness>().Contains(payload.Status) ||
                !Enum.TryParse<RepositoryReadiness>(payload.Status, out var status)) return RepositoryReadiness.Unavailable;
            if ((status == RepositoryReadiness.Ready) != response.IsSuccessStatusCode) return RepositoryReadiness.Unavailable;
            return status;
        }
        catch (Exception exception) when (exception is HttpRequestException or System.Text.Json.JsonException or NotSupportedException)
        { return RepositoryReadiness.Unavailable; }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { return RepositoryReadiness.Unavailable; }
    }

    public async Task RegisterAsync(string token, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/setup/repository/register");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<RepositoryUserAccess> GetAccessAsync(string token, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/repository/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RepositoryUserAccess>(cancellationToken)
            ?? throw new InvalidOperationException("The API returned no repository access record.");
    }
}
