using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Client.Authentication;
using Beep.OilandGas.Client.Connection;
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
        protected T GetLocalService<T>() where T : class
        {
            if (AccessMode != ServiceAccessMode.Local)
                throw new InvalidOperationException($"Cannot resolve local service {typeof(T).Name} when AccessMode is {AccessMode}");

            return _app.GetService<T>() 
                ?? throw new InvalidOperationException($"Failed to resolve local service {typeof(T).Name}");
        }

        #endregion

        #region HTTP Methods for Remote Mode

        protected Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default) =>
            _app.GetApiTransport().GetAsync<T>(endpoint, cancellationToken);

        protected Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest content, CancellationToken cancellationToken = default) =>
            _app.GetApiTransport().PostAsync<TRequest, TResponse>(endpoint, content, cancellationToken);

        protected Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest content, CancellationToken cancellationToken = default) =>
            _app.GetApiTransport().PutAsync<TRequest, TResponse>(endpoint, content, cancellationToken);

        protected Task<T> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default) =>
            _app.GetApiTransport().DeleteAsync<T>(endpoint, cancellationToken);

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

        protected string BuildRequestUriWithParams(
            string endpoint,
            Dictionary<string, string>? queryParams = null,
            string connectionName = "PPDM39")
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

    }
}
