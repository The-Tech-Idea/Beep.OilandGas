using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Models;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// API controller for connection management operations
    /// Exposes IDMEEditor configured connections for first-login flow
    /// </summary>
    [ApiController]
    [Route("api/connections")]
    public class ConnectionController : ControllerBase
    {
        private readonly ConnectionService _connectionService;
        private readonly ILogger<ConnectionController> _logger;

        public ConnectionController(
            ConnectionService connectionService,
            ILogger<ConnectionController> logger)
        {
            _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all IDMEEditor configured connections
        /// </summary>
        [HttpGet]
        public ActionResult<List<ConnectionInfo>> GetAllConnections()
        {
            try
            {
                var connections = _connectionService.GetAllConnections();
                return Ok(connections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all connections");
                return StatusCode(500, new { error = "Failed to get connections", details = ex.Message });
            }
        }

        /// <summary>
        /// Get connection details by name
        /// </summary>
        [HttpGet("{connectionName}")]
        public ActionResult<ConnectionInfo> GetConnection(string connectionName)
        {
            try
            {
                var connection = _connectionService.GetConnection(connectionName);
                if (connection == null)
                {
                    return NotFound(new { error = $"Connection '{connectionName}' not found" });
                }
                return Ok(connection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connection {ConnectionName}", connectionName);
                return StatusCode(500, new { error = "Failed to get connection", details = ex.Message });
            }
        }

        /// <summary>
        /// Test a database connection
        /// </summary>
        [HttpPost("test")]
        public async Task<ActionResult<ConnectionTestResult>> TestConnection([FromBody] TestConnectionRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.ConnectionName))
                {
                    return BadRequest(new { error = "Connection name is required" });
                }

                var result = await _connectionService.TestConnectionAsync(request.ConnectionName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection");
                return StatusCode(500, new ConnectionTestResult
                {
                    Success = false,
                    Message = "Connection test failed",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Get current connection for user
        /// </summary>
        [HttpGet("current")]
        public ActionResult<CurrentConnectionResponse> GetCurrentConnection()
        {
            try
            {
                var response = _connectionService.GetCurrentConnection();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current connection");
                return StatusCode(500, new { error = "Failed to get current connection", details = ex.Message });
            }
        }

        /// <summary>
        /// Set current connection for user
        /// </summary>
        [HttpPost("set-current")]
        public ActionResult<SetCurrentConnectionResult> SetCurrentConnection([FromBody] SetCurrentConnectionRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.ConnectionName))
                {
                    return BadRequest(new { error = "Connection name is required" });
                }

                var result = _connectionService.SetCurrentConnection(request.ConnectionName, request.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting current connection {ConnectionName}", request?.ConnectionName);
                return StatusCode(500, new SetCurrentConnectionResult
                {
                    Success = false,
                    Message = "Failed to set current connection",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Create a new database connection (via DataManagement API)
        /// This endpoint delegates to PPDM39SetupController for database creation
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<CreateConnectionResult>> CreateConnection([FromBody] CreateConnectionRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.ConnectionName))
                {
                    return BadRequest(new { error = "Connection name is required" });
                }

                // Note: Database creation is handled by PPDM39SetupController
                // This endpoint can redirect or call the setup service
                // For now, return a response indicating the connection should be created via setup endpoints
                return Ok(new CreateConnectionResult
                {
                    Success = true,
                    ConnectionName = request.ConnectionName,
                    Message = "Use /api/ppdm39/setup endpoints to create database connections"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating connection");
                return StatusCode(500, new CreateConnectionResult
                {
                    Success = false,
                    ConnectionName = request?.ConnectionName ?? string.Empty,
                    Message = "Failed to create connection",
                    ErrorDetails = ex.Message
                });
            }
        }
    }
}

