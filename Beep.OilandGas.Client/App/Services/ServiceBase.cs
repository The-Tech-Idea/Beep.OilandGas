using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Client.Authentication;
using Beep.OilandGas.Client.Connection;
using Beep.OilandGas.Client.Exceptions;
using Beep.OilandGas.Client.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App.Services
{
    /// <summary>
    /// Base class for all unified service classes
    /// Handles both remote (HTTP API) and local (direct service) modes
    /// </summary>
    public abstract class ServiceBase
    {
        protected readonly BeepOilandGasApp _app;
        protected readonly ILogger? _logger;

        protected ServiceBase(BeepOilandGasApp app, ILogger? logger = null)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _logger = logger;
        }

        #region AppClass Access

        /// <summary>
        /// Gets the current access mode (Remote or Local)
        /// </summary>
        protected ServiceAccessMode AccessMode => _app.AccessMode;

        /// <summary>
        /// Gets the current connection name from AppClass
        /// </summary>
        protected string? CurrentConnectionName => _app.GetCurrentConnectionName();

        /// <summary>
        /// Gets the connection manager for multi-database operations
        /// </summary>
        protected ConnectionManager ConnectionManager => _app.GetConnectionManager();

        /// <summary>
        /// Gets the authentication provider
        /// </summary>
        protected IAuthenticationProvider? AuthProvider => _app.GetAuthProvider();

        /// <summary>
        /// Gets a service from the DI container (local mode only)
        /// </summary>
        protected T? GetLocalService<T>() => _app.GetService<T>();

        #endregion

        #region HTTP Methods for Remote Mode

        /// <summary>
        /// Makes an HTTP GET request (remote mode)
        /// </summary>
        protected async Task<T> GetAsync<T>(
            string endpoint,
            string? connectionName = null,
            CancellationToken cancellationToken = default)
        {
            EnsureRemoteMode();
            var requestUri = BuildRequestUri(endpoint, ResolveConnectionName(connectionName));
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            await AddAuthHeaderAsync(request);
            return await SendRequestAsync<T>(request, cancellationToken);
        }

        /// <summary>
        /// Makes an HTTP POST request (remote mode)
        /// </summary>
        protected async Task<TResponse> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest content,
            string? connectionName = null,
            CancellationToken cancellationToken = default)
        {
            EnsureRemoteMode();
            var requestUri = BuildRequestUri(endpoint, ResolveConnectionName(connectionName));
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            if (content != null)
            {
                var json = JsonSerializer.Serialize(content, GetJsonOptions());
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            await AddAuthHeaderAsync(request);
            return await SendRequestAsync<TResponse>(request, cancellationToken);
        }

        /// <summary>
        /// Makes an HTTP PUT request (remote mode)
        /// </summary>
        protected async Task<TResponse> PutAsync<TRequest, TResponse>(
            string endpoint,
            TRequest content,
            string? connectionName = null,
            CancellationToken cancellationToken = default)
        {
            EnsureRemoteMode();
            var requestUri = BuildRequestUri(endpoint, ResolveConnectionName(connectionName));
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri);

            if (content != null)
            {
                var json = JsonSerializer.Serialize(content, GetJsonOptions());
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            await AddAuthHeaderAsync(request);
            return await SendRequestAsync<TResponse>(request, cancellationToken);
        }

        /// <summary>
        /// Makes an HTTP DELETE request (remote mode)
        /// </summary>
        protected async Task<T> DeleteAsync<T>(
            string endpoint,
            string? connectionName = null,
            CancellationToken cancellationToken = default)
        {
            EnsureRemoteMode();
            var requestUri = BuildRequestUri(endpoint, ResolveConnectionName(connectionName));
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            await AddAuthHeaderAsync(request);
            return await SendRequestAsync<T>(request, cancellationToken);
        }

        #endregion

        #region Connection Resolution

        /// <summary>
        /// Resolves connection name using priority: explicit > AppClass current > default
        /// </summary>
        protected string ResolveConnectionName(string? explicitConnection)
        {
            return explicitConnection
                ?? CurrentConnectionName
                ?? _app.GetOptions().DefaultConnectionName
                ?? "PPDM39";
        }

        #endregion

        #region Internal HTTP Helpers

        private void EnsureRemoteMode()
        {
            if (_app.AccessMode != ServiceAccessMode.Remote)
                throw new InvalidOperationException("HTTP operations are only available in remote mode");
        }

        private HttpClient GetHttpClient()
        {
            return _app.GetHttpClient() 
                ?? throw new InvalidOperationException("HttpClient is not available");
        }

        private string BuildRequestUri(string endpoint, string? connectionName = null)
        {
            var baseUrl = _app.GetOptions().ApiBaseUrl ?? "";
            var uri = endpoint.StartsWith("/") ? endpoint : "/" + endpoint;

            if (!string.IsNullOrEmpty(connectionName))
            {
                var separator = uri.Contains("?") ? "&" : "?";
                uri += $"{separator}connectionName={Uri.EscapeDataString(connectionName)}";
            }

            return baseUrl.TrimEnd('/') + uri;
        }

        protected string BuildRequestUriWithParams(
            string endpoint,
            Dictionary<string, string>? queryParams = null,
            string? connectionName = null)
        {
            var uri = endpoint.StartsWith("/") ? endpoint : "/" + endpoint;
            var hasParams = endpoint.Contains("?");

            if (queryParams != null && queryParams.Count > 0)
            {
                var separator = hasParams ? "&" : "?";
                uri += separator + string.Join("&", 
                    queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                hasParams = true;
            }

            if (!string.IsNullOrEmpty(connectionName))
            {
                var separator = hasParams ? "&" : "?";
                uri += $"{separator}connectionName={Uri.EscapeDataString(connectionName)}";
            }

            var baseUrl = _app.GetOptions().ApiBaseUrl ?? "";
            return baseUrl.TrimEnd('/') + uri;
        }

        private async Task AddAuthHeaderAsync(HttpRequestMessage request)
        {
            var authProvider = AuthProvider;
            if (authProvider != null)
            {
                try
                {
                    var token = await authProvider.GetAccessTokenAsync();
                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to get access token");
                    throw new ApiAuthenticationException("Failed to get access token", ex);
                }
            }
        }

        private async Task<T> SendRequestAsync<T>(
            HttpRequestMessage request,
            CancellationToken cancellationToken = default)
        {
            var httpClient = GetHttpClient();
            var options = _app.GetOptions();
            var maxRetries = options.MaxRetries;
            var retryDelay = options.RetryDelay;
            var attempt = 0;
            Exception? lastException = null;

            while (attempt <= maxRetries)
            {
                try
                {
                    _logger?.LogDebug("Sending {Method} request to {Uri}", request.Method, request.RequestUri);

                    var response = await httpClient.SendAsync(request, cancellationToken);

                    _logger?.LogDebug("Received {StatusCode} response", response.StatusCode);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync(cancellationToken);

                        if (string.IsNullOrEmpty(content))
                        {
                            return default(T)!;
                        }

                        try
                        {
                            return JsonSerializer.Deserialize<T>(content, GetJsonOptions())!;
                        }
                        catch (JsonException ex)
                        {
                            _logger?.LogError(ex, "Failed to deserialize response as {Type}", typeof(T).Name);
                            throw new ApiClientException($"Failed to deserialize response: {ex.Message}", ex);
                        }
                    }
                    else
                    {
                        await HandleErrorResponse(response);
                    }
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    lastException = new ApiTimeoutException("Request timed out", options.Timeout, ex);
                    attempt++;

                    if (attempt <= maxRetries)
                    {
                        await Task.Delay(retryDelay, cancellationToken);
                        continue;
                    }

                    throw lastException;
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    attempt++;

                    if (attempt <= maxRetries)
                    {
                        _logger?.LogWarning(ex, "Transient error, retrying ({Attempt}/{MaxRetries})", attempt, maxRetries);
                        await Task.Delay(retryDelay * attempt, cancellationToken);
                        continue;
                    }

                    throw new ApiClientException($"HTTP request failed: {ex.Message}", ex);
                }
            }

            throw lastException ?? new ApiClientException("Request failed after retries");
        }

        private async Task HandleErrorResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var error = JsonSerializer.Deserialize<ApiError>(content, GetJsonOptions());
                throw new ApiException(response.StatusCode, 
                    error?.Message ?? error?.Error ?? response.ReasonPhrase ?? "Unknown error", content);
            }
            catch (JsonException)
            {
                throw new ApiException(response.StatusCode, response.ReasonPhrase ?? "Unknown error", content);
            }
        }

        private JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        #endregion
    }
}

