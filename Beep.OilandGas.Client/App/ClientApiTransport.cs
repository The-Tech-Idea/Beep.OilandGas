using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Client.Authentication;
using Beep.OilandGas.Client.Exceptions;
using Beep.OilandGas.Client.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App;

/// <summary>The facade's remote HTTP path, with captured company scope and retries for GET requests only.</summary>
internal sealed class ClientApiTransport(HttpClient client, AppOptions options, CompanyClientContext company,
    IAuthenticationProvider? authentication = null, ILogger? logger = null)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    internal CompanyScope SelectedCompanyScope => company.RequireSelection().Scope;

    public Task<T> GetAsync<T>(string endpoint, CancellationToken token = default) => SendAsync<T>(HttpMethod.Get, endpoint, null, token);
    public Task<T> PostAsync<TRequest, T>(string endpoint, TRequest body, CancellationToken token = default) =>
        SendAsync<T>(HttpMethod.Post, endpoint, body, token);
    public Task<T> PutAsync<TRequest, T>(string endpoint, TRequest body, CancellationToken token = default) =>
        SendAsync<T>(HttpMethod.Put, endpoint, body, token);
    public Task<T> DeleteAsync<T>(string endpoint, CancellationToken token = default) => SendAsync<T>(HttpMethod.Delete, endpoint, null, token);

    private async Task<T> SendAsync<T>(HttpMethod method, string endpoint, object? body, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var uri = Address(endpoint);
        using var captured = new HttpRequestMessage(method, uri);
        company.Prepare(captured);
        var json = body is null ? null : JsonSerializer.Serialize(body, JsonOptions);
        if (authentication is not null)
        {
            string accessToken;
            try { accessToken = await authentication.GetAccessTokenAsync(); }
            catch (Exception error)
            {
                logger?.LogError(error, "Failed to get access token");
                throw new ApiAuthenticationException("Failed to get access token", error);
            }
            if (!string.IsNullOrEmpty(accessToken)) captured.Headers.Authorization = new("Bearer", accessToken);
        }
        // A failed mutation may already have persisted. Even read-only POSTs are single attempts unless an explicit
        // idempotency contract is implemented by their endpoint. HttpRequestMessage itself must never be resent.
        var retries = method == HttpMethod.Get ? Math.Max(0, options.MaxRetries) : 0;
        for (var attempt = 0; ; attempt++)
        {
            token.ThrowIfCancellationRequested();
            using var request = Copy(captured, json);
            try
            {
                using var response = await company.SendAsync(client, request, token);
                var content = await response.Content.ReadAsStringAsync(token);
                company.EnsureCurrent(response);
                if (!response.IsSuccessStatusCode)
                    throw Error(response, content);
                if (string.IsNullOrEmpty(content)) return default!;
                try
                {
                    var result = JsonSerializer.Deserialize<T>(content, JsonOptions)!;
                    company.EnsureCurrent(response);
                    return result;
                }
                catch (JsonException error)
                {
                    logger?.LogError(error, "Failed to deserialize response as {Type}", typeof(T).Name);
                    throw new ApiClientException("The API returned an invalid response.", error);
                }
            }
            catch (TaskCanceledException error) when (!token.IsCancellationRequested && error.InnerException is TimeoutException)
            {
                if (attempt >= retries) throw new ApiTimeoutException("Request timed out", options.Timeout, error);
            }
            catch (HttpRequestException error)
            {
                if (attempt >= retries) throw new ApiClientException("The API request did not complete.", error);
                logger?.LogWarning(error, "Transient GET error, retrying ({Attempt}/{MaxRetries})", attempt + 1, retries);
            }
            // Recheck the original capture before and after the retry delay. A switched session must not send again.
            company.Prepare(captured);
            await Task.Delay(options.RetryDelay * (attempt + 1), token);
            company.Prepare(captured);
        }
    }

    private string Address(string endpoint)
    {
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var absolute) && absolute.Scheme is "http" or "https")
        {
            if (!Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out var configured) ||
                !string.Equals(absolute.GetLeftPart(UriPartial.Authority), configured.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("An API request must use the configured API origin.");
            return absolute.AbsoluteUri;
        }
        return (options.ApiBaseUrl ?? "").TrimEnd('/') + "/" + endpoint.TrimStart('/');
    }

    private static HttpRequestMessage Copy(HttpRequestMessage captured, string? json)
    {
        var request = new HttpRequestMessage(captured.Method, captured.RequestUri);
        foreach (var header in captured.Headers) request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        foreach (var option in captured.Options) request.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        if (json is not null) request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return request;
    }

    private static ApiException Error(HttpResponseMessage response, string content)
    {
        ApiError? error;
        try { error = JsonSerializer.Deserialize<ApiError>(content, JsonOptions); }
        catch (JsonException) { return new(response.StatusCode, response.ReasonPhrase ?? "API request failed.", content); }
        return new(response.StatusCode, error?.Message ?? error?.Error ?? response.ReasonPhrase ?? "API request failed.", content);
    }
}
