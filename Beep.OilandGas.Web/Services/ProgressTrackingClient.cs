using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

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
        private readonly string _hubUrl;
        private HubConnection? _hubConnection;
        private readonly ILogger<ProgressTrackingClient>? _logger;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public event Action<ProgressUpdate>? OnProgressUpdate;

        public ProgressTrackingClient(NavigationManager navigationManager, IConfiguration? configuration = null, ILogger<ProgressTrackingClient>? logger = null)
        {
            // Get API base URL from configuration - this should match the ApiClient base URL
            // In Blazor Server, SignalR hub runs on the API server
            var apiBaseUrl = configuration?["ApiService:BaseUrl"] 
                ?? configuration?["ApiSettings:BaseUrl"]
                ?? "https://localhost:7001"; // Default to API service default port
            
            // Ensure URL doesn't end with slash
            apiBaseUrl = apiBaseUrl.TrimEnd('/');
            
            // SignalR hub is on the API server
            _hubUrl = $"{apiBaseUrl}/progressHub";
            _logger = logger;
            
            _logger?.LogDebug("ProgressTrackingClient initialized with hub URL: {HubUrl}", _hubUrl);
        }

        public async Task ConnectAsync()
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                return;
            }

            try
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .WithAutomaticReconnect()
                    .Build();

                // Register progress update handler
                _hubConnection.On<ProgressUpdate>("ProgressUpdate", (progress) =>
                {
                    try
                    {
                        OnProgressUpdate?.Invoke(progress);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error handling progress update");
                    }
                });

                // Register workflow progress handler
                _hubConnection.On<WorkflowProgress>("WorkflowProgress", (workflowProgress) =>
                {
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
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error handling workflow progress update");
                    }
                });

                // Register multi-operation progress handler
                _hubConnection.On<MultiOperationProgress>("MultiOperationProgress", (multiProgress) =>
                {
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
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error handling multi-operation progress update");
                    }
                });

                // Handle reconnection
                _hubConnection.Reconnecting += (error) =>
                {
                    _logger?.LogWarning("Progress hub reconnecting: {Error}", error?.Message);
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += (connectionId) =>
                {
                    _logger?.LogInformation("Progress hub reconnected: {ConnectionId}", connectionId);
                    return Task.CompletedTask;
                };

                _hubConnection.Closed += (error) =>
                {
                    _logger?.LogWarning("Progress hub closed: {Error}", error?.Message);
                    return Task.CompletedTask;
                };

                await _hubConnection.StartAsync();
                _logger?.LogInformation("Connected to progress hub");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to connect to progress hub");
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                _logger?.LogInformation("Disconnected from progress hub");
            }
        }

        public async Task JoinOperationAsync(string operationId)
        {
            if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
            {
                await ConnectAsync();
            }

            await _hubConnection!.InvokeAsync("JoinOperationGroup", operationId);
            _logger?.LogDebug("Joined operation group: {OperationId}", operationId);
        }

        public async Task LeaveOperationAsync(string operationId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("LeaveOperationGroup", operationId);
                _logger?.LogDebug("Left operation group: {OperationId}", operationId);
            }
        }

        public async Task JoinWorkflowAsync(string workflowId)
        {
            if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
            {
                await ConnectAsync();
            }

            await _hubConnection!.InvokeAsync("JoinWorkflowGroup", workflowId);
            _logger?.LogDebug("Joined workflow group: {WorkflowId}", workflowId);
        }

        public async Task LeaveWorkflowAsync(string workflowId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("LeaveWorkflowGroup", workflowId);
                _logger?.LogDebug("Left workflow group: {WorkflowId}", workflowId);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
        }
    }
}
