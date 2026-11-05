using Beep.OilandGas.Models.Data.Company;
using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Authorization;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Client-side service for tracking progress via SignalR
    /// </summary>
    public interface IProgressTrackingClient
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task JoinOperationAsync(string operationId);
        Task LeaveOperationAsync(string operationId);
        Task JoinWorkflowAsync(string workflowId);
        Task LeaveWorkflowAsync(string workflowId);
        event Action<ProgressUpdate>? OnProgressUpdate;
        bool IsConnected { get; }
    }

    /// <summary>
    /// SignalR client for progress tracking
    /// </summary>
    public class ProgressTrackingClient : IProgressTrackingClient, IAsyncDisposable
    {
        private readonly Uri _hubUri;
        private readonly AuthenticationStateProvider _authentication;
        private readonly IUserTokenManager _tokens;
        private HubConnection? _hubConnection;
        private readonly ILogger<ProgressTrackingClient> _logger;
        private readonly IFailureReporter _failures;
        private readonly Beep.OilandGas.Client.App.CompanyClientContext _company;
        private readonly SemaphoreSlim _lifecycle = new(1, 1);
        private long _companyVersion = -1;
        private bool _disposed;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public event Action<ProgressUpdate>? OnProgressUpdate;

        /// <remarks>
        /// The hub is on the OilGas API, which admits only a signed-in person with an active account, so the connection
        /// carries the person's own access token. It connected with none — every connection refused — and fell back to
        /// <c>https://localhost:7001</c> when the API's address was not configured.
        /// </remarks>
        public ProgressTrackingClient(AuthenticationStateProvider authentication, IUserTokenManager tokens,
            OilGasApiAddress api, ILogger<ProgressTrackingClient> logger, IFailureReporter failures,
            Beep.OilandGas.Client.App.CompanyClientContext company)
        {
            _authentication = authentication;
            _tokens = tokens;
            _logger = logger;
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
            _hubUri = api.For("progressHub");
            _company = company;
            _company.Changed += CompanyChanged;
        }

        public async Task ConnectAsync()
        {
            await _lifecycle.WaitAsync();
            try
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                var snapshot = _company.Snapshot;
                var selected = snapshot.Selection ?? throw new InvalidOperationException("Select a company before tracking business progress.");
                if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected && _companyVersion == snapshot.Version)
                {
                    return;
                }

                try
                {
                    await DisconnectCoreAsync();
                    _companyVersion = snapshot.Version;
                    _hubConnection = new HubConnectionBuilder()
                        .WithUrl(_hubUri, options =>
                        {
                            options.Headers[Beep.OilandGas.Models.Data.Company.CompanyRequestHeaders.Tenant] = selected.Scope.TenantId;
                            options.Headers[Beep.OilandGas.Models.Data.Company.CompanyRequestHeaders.Company] = selected.Scope.BusinessAssociateId;
                            options.AccessTokenProvider = async () =>
                            {
                                var state = await _authentication.GetAuthenticationStateAsync();
                                if (_company.Version != snapshot.Version ||
                                    Beep.Foundation.IdentityServer.Shared.Identity.PartyIdClaims.Find(state.User) != selected.UserId)
                                    throw new OperationCanceledException("The signed-in account or selected company changed.");
                                var token = await _tokens.GetAccessTokenAsync(state.User);
                                return token.WasSuccessful(out var user, out var failure)
                                    ? user.AccessToken.ToString()
                                    : throw new InvalidOperationException($"The signed-in person's access token is unavailable ({failure.Error}).");
                            };
                        })
                        .WithAutomaticReconnect()
                        .Build();

                    // Register progress update handler
                    _hubConnection.On<ProgressUpdate>("ProgressUpdate", (progress) =>
                    {
                        if (_company.Version != snapshot.Version) return;
                        try
                        {
                            OnProgressUpdate?.Invoke(progress);
                        }
                        // A subscriber's failure must not end the hub's handler: it is reported, and the next update still arrives.
                        catch (Exception ex)
                        {
                            _failures.ReportHandled(
                                ex,
                                "handing a progress update to the page",
                                consequence: "the page did not show this update; the next update is still delivered");
                        }
                    });

                    // Register workflow progress handler
                    _hubConnection.On<WorkflowProgress>("WorkflowProgress", (workflowProgress) =>
                    {
                        if (_company.Version != snapshot.Version) return;
                        try
                        {
                            // Convert WorkflowProgress to ProgressUpdate for compatibility
                            var progress = new ProgressUpdate
                            {
                                OperationId = workflowProgress.OperationId,
                                OperationType = "Workflow",
                                ProgressPercentage = workflowProgress.OverallProgress,
                                StatusMessage = workflowProgress.StatusMessage,
                                IsComplete = workflowProgress.IsComplete,
                                HasError = workflowProgress.HasError,
                                ErrorMessage = workflowProgress.ErrorMessage,
                                Timestamp = workflowProgress.Timestamp
                            };
                            OnProgressUpdate?.Invoke(progress);
                        }
                        // A subscriber's failure must not end the hub's handler: it is reported, and the next update still arrives.
                        catch (Exception ex)
                        {
                            _failures.ReportHandled(
                                ex,
                                "handing a workflow progress update to the page",
                                consequence: "the page did not show this update; the next update is still delivered");
                        }
                    });

                    // Register multi-operation progress handler
                    _hubConnection.On<MultiOperationProgress>("MultiOperationProgress", (multiProgress) =>
                    {
                        if (_company.Version != snapshot.Version) return;
                        try
                        {
                            // Convert MultiOperationProgress to ProgressUpdate for compatibility
                            var progress = new ProgressUpdate
                            {
                                OperationId = multiProgress.OperationId,
                                OperationType = "OperationGroup",
                                ProgressPercentage = multiProgress.OverallProgress,
                                StatusMessage = multiProgress.StatusMessage,
                                IsComplete = multiProgress.IsComplete,
                                HasError = multiProgress.HasError,
                                ErrorMessage = multiProgress.ErrorMessage,
                                Timestamp = multiProgress.Timestamp
                            };
                            OnProgressUpdate?.Invoke(progress);
                        }
                        // A subscriber's failure must not end the hub's handler: it is reported, and the next update still arrives.
                        catch (Exception ex)
                        {
                            _failures.ReportHandled(
                                ex,
                                "handing a multi-operation progress update to the page",
                                consequence: "the page did not show this update; the next update is still delivered");
                        }
                    });

                    // Handle reconnection
                    _hubConnection.Reconnecting += (error) =>
                    {
                        _logger.LogWarning("Progress hub reconnecting: {Error}", error?.Message);
                        return Task.CompletedTask;
                    };

                    _hubConnection.Reconnected += (connectionId) =>
                    {
                        _logger.LogInformation("Progress hub reconnected: {ConnectionId}", connectionId);
                        return Task.CompletedTask;
                    };

                    _hubConnection.Closed += (error) =>
                    {
                        _logger.LogWarning("Progress hub closed: {Error}", error?.Message);
                        return Task.CompletedTask;
                    };

                    await _hubConnection.StartAsync();
                    if (_company.Version != snapshot.Version)
                    {
                        await DisconnectCoreAsync();
                        throw new OperationCanceledException("The selected company changed.");
                    }
                    _logger.LogInformation("Connected to progress hub");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to connect to progress hub");
                    await DisconnectCoreAsync();
                    throw;
                }
            }
            finally { _lifecycle.Release(); }
        }

        public async Task DisconnectAsync()
        {
            await _lifecycle.WaitAsync();
            try { await DisconnectCoreAsync(); }
            finally { _lifecycle.Release(); }
        }

        private async Task DisconnectCoreAsync()
        {
            _companyVersion = -1;
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                _logger.LogInformation("Disconnected from progress hub");
            }
        }

        public async Task JoinOperationAsync(string operationId)
        {
            var version = _company.Version;
            if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
            {
                await ConnectAsync();
            }

            if (version != _company.Version || _companyVersion != version) throw new OperationCanceledException("The selected company changed.");
            await _hubConnection!.InvokeAsync("JoinOperationGroup", operationId);
            _logger.LogDebug("Joined operation group: {OperationId}", operationId);
        }

        public async Task LeaveOperationAsync(string operationId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("LeaveOperationGroup", operationId);
                _logger.LogDebug("Left operation group: {OperationId}", operationId);
            }
        }

        public async Task JoinWorkflowAsync(string workflowId)
        {
            var version = _company.Version;
            if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
            {
                await ConnectAsync();
            }

            if (version != _company.Version || _companyVersion != version) throw new OperationCanceledException("The selected company changed.");
            await _hubConnection!.InvokeAsync("JoinWorkflowGroup", workflowId);
            _logger.LogDebug("Joined workflow group: {WorkflowId}", workflowId);
        }

        public async Task LeaveWorkflowAsync(string workflowId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("LeaveWorkflowGroup", workflowId);
                _logger.LogDebug("Left workflow group: {WorkflowId}", workflowId);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _disposed = true;
            _company.Changed -= CompanyChanged;
            await DisconnectAsync();
        }

        private void CompanyChanged()
        {
            _companyVersion = -1;
            if (!_disposed) _ = StopForCompanyChangeAsync();
        }

        private async Task StopForCompanyChangeAsync()
        {
            try { await DisconnectAsync(); }
            catch (Exception ex) { _failures.ReportHandled(ex, "stopping progress tracking after company selection changed", "progress updates from the previous company are ignored"); }
        }
    }
}
