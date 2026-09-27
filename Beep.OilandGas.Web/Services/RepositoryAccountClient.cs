using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

/// <summary>What the OilGas API says about a person's account.</summary>
public enum RepositoryAccountState
{
    /// <summary>The account is active; <see cref="RepositoryAccountAnswer.Access"/> holds its roles.</summary>
    Active,

    /// <summary>The API refuses the account: switched off, gone, or not a person.</summary>
    Refused,

    /// <summary>The API could not answer — an outage, not a refusal.</summary>
    Unavailable
}

/// <summary>The API's answer about one person's account.</summary>
public sealed record RepositoryAccountAnswer(RepositoryAccountState State, RepositoryUserAccess? Access, string? Reason);

/// <summary>
/// The Web's view of the OilGas repository through the API: whether it is ready, and a person's account — asked with that
/// person's own access token from the identity server's client library, never a token the Web keeps for itself.
/// </summary>
/// <remarks>
/// The Web holds no user store: the API owns the repository, so the account is asked for over HTTP (the one host that
/// cannot resolve it in process). It read tokens from a dictionary the library no longer has, and answered a refused
/// account and an outage alike by throwing.
/// </remarks>
public sealed class RepositoryAccountClient(HttpClient http, IUserTokenManager tokens, ILogger<RepositoryAccountClient> logger)
{
    public async Task<RepositoryReadiness> GetReadinessAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await http.GetAsync("health/repository", cancellationToken);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.ServiceUnavailable)
                return Unavailable("the readiness check answered " + (int)response.StatusCode);
            var payload = await response.Content.ReadFromJsonAsync<RepositoryStatusResponse>(cancellationToken);
            if (payload is null || !Enum.GetNames<RepositoryReadiness>().Contains(payload.Status) ||
                !Enum.TryParse<RepositoryReadiness>(payload.Status, out var status))
                return Unavailable("the readiness check answered an unknown status");
            if ((status == RepositoryReadiness.Ready) != response.IsSuccessStatusCode)
                return Unavailable("the readiness check's status and code disagree");
            return status;
        }
        catch (Exception exception) when (exception is HttpRequestException or System.Text.Json.JsonException or NotSupportedException)
        {
            logger.LogWarning(exception, "The OilGas API's readiness could not be read; it is treated as unavailable.");
            return RepositoryReadiness.Unavailable;
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "The OilGas API's readiness check timed out; it is treated as unavailable.");
            return RepositoryReadiness.Unavailable;
        }
    }

    /// <summary>
    /// <paramref name="person"/>'s OilGas account, asked with their access token. Usable at sign-in, before the cookie
    /// exists: the library holds the ticket's tokens by then (AOR-SDK-15).
    /// </summary>
    public async Task<RepositoryAccountAnswer> GetAccountAsync(ClaimsPrincipal person, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(person);

        var token = await tokens.GetAccessTokenAsync(person, ct: cancellationToken);
        if (!token.WasSuccessful(out var user, out var failure))
        {
            logger.LogWarning("No access token for the OilGas API could be obtained for this person ({Error}).", failure.Error);
            return new(RepositoryAccountState.Unavailable, null, failure.Error);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/repository/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken.ToString());
            using var response = await http.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var access = await response.Content.ReadFromJsonAsync<RepositoryUserAccess>(cancellationToken);
                return access is null
                    ? new(RepositoryAccountState.Unavailable, null, "the API returned no account")
                    : new(RepositoryAccountState.Active, access, null);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden
                && await ReadTitleAsync(response, cancellationToken) is (RepositoryAccountRefusals.Deactivated
                    or RepositoryAccountRefusals.Removed or RepositoryAccountRefusals.NotAPerson) and var refusal)
            {
                return new(RepositoryAccountState.Refused, null, refusal);
            }

            logger.LogWarning("The OilGas API answered {Status} for a person's account; it is treated as unavailable.", (int)response.StatusCode);
            return new(RepositoryAccountState.Unavailable, null, "the API answered " + (int)response.StatusCode);
        }
        catch (Exception exception) when (exception is HttpRequestException or System.Text.Json.JsonException or NotSupportedException)
        {
            logger.LogWarning(exception, "The OilGas API could not be asked about a person's account.");
            return new(RepositoryAccountState.Unavailable, null, "the API could not be reached");
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "The OilGas API timed out answering about a person's account.");
            return new(RepositoryAccountState.Unavailable, null, "the API timed out");
        }
    }

    /// <summary>
    /// Why <paramref name="person"/> may not delete their account — OilGas's only active administrator — or null. Asked
    /// before the identity server deletes anything. Throws when the API could not answer, so nothing is deleted on an
    /// unanswered question.
    /// </summary>
    public async Task<string?> RefusalOfDeletionAsync(ClaimsPrincipal person, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(person);

        var token = await tokens.GetAccessTokenAsync(person, ct: cancellationToken);
        if (!token.WasSuccessful(out var user, out var failure))
            throw new InvalidOperationException($"No access token for the OilGas API could be obtained ({failure.Error}).");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/repository/me/deletion");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken.ToString());
        using var response = await http.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var problem = await ReadProblemAsync(response, cancellationToken);
            throw new InvalidOperationException(
                $"The OilGas API could not say whether the account may be deleted ({(int)response.StatusCode}): {problem?.Detail ?? "no reason given"}");
        }

        var answer = await response.Content.ReadFromJsonAsync<RepositoryAccountDeletion>(cancellationToken)
            ?? throw new InvalidOperationException("The OilGas API answered whether the account may be deleted with an empty body.");
        return answer.Refusal;
    }

    /// <summary>
    /// Switches <paramref name="person"/>'s OilGas account off, once they have deleted their account at the identity
    /// server. Throws when it could not be — the caller says the OilGas records may remain.
    /// </summary>
    public async Task DeactivateAsync(ClaimsPrincipal person, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(person);

        var token = await tokens.GetAccessTokenAsync(person, ct: cancellationToken);
        if (!token.WasSuccessful(out var user, out var failure))
            throw new InvalidOperationException($"No access token for the OilGas API could be obtained ({failure.Error}).");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/repository/me/deactivate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken.ToString());
        using var response = await http.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
            return;

        var problem = await ReadProblemAsync(response, cancellationToken);
        throw new InvalidOperationException(
            $"The OilGas API did not switch the account off ({(int)response.StatusCode}): {problem?.Detail ?? "no reason given"}");
    }

    private static async Task<string?> ReadTitleAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        (await ReadProblemAsync(response, cancellationToken))?.Title;

    private static async Task<ProblemDetails?> ReadProblemAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        }
        catch (System.Text.Json.JsonException)
        {
            // Handled: a 403 without the API's problem body is not one of its account refusals; the caller treats it as
            // an outage and asks again.
            return null;
        }
    }

    private RepositoryReadiness Unavailable(string why)
    {
        logger.LogWarning("The OilGas API's readiness is treated as unavailable: {Reason}.", why);
        return RepositoryReadiness.Unavailable;
    }
}
