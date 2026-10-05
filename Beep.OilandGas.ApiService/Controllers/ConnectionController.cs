using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ConnectionInfo = Beep.OilandGas.Models.Data.DataManagement.ConnectionInfo;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// API controller for connection management operations
    /// Exposes IDMEEditor configured connections for first-login flow
    /// </summary>
    [ApiController]
    [Route("api/connections")]
    [Authorize(Roles = "Admin,Administrator")]
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
            var connections = _connectionService.GetAllConnections();
            return Ok(connections);
        }

        /// <summary>
        /// Get connection details by name
        /// </summary>
        [HttpGet("{connectionName}")]
        public ActionResult<ConnectionInfo> GetConnection(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName)) return BadRequest(new { error = "Connection name is required." });
            var connection = _connectionService.GetConnection(connectionName);
            if (connection == null)
            {
                    return NotFound(new { error = $"Connection '{connectionName}' not found." });
            }
            return Ok(connection);
        }

        /// <summary>
        /// Test a database connection
        /// </summary>
        [HttpPost("test")]
        public async Task<ActionResult<ConnectionTestResult>> TestConnection([FromBody] TestConnectionRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ConnectionName))
            {
                    return BadRequest(new { error = "Connection name is required." });
            }

            var result = await _connectionService.TestConnectionAsync(request.ConnectionName);
            return Ok(result);
        }

        /// <summary>
        /// Get current connection for user
        /// </summary>
        [HttpGet("current")]
        public ActionResult<CurrentConnectionResponse> GetCurrentConnection()
        {
            var response = _connectionService.GetCurrentConnection();
            return Ok(response);
        }

        /// <summary>
        /// Set current connection for user
        /// </summary>
        [HttpPost("set-current")]
        public ActionResult<SetCurrentConnectionResult> SetCurrentConnection([FromBody] SetCurrentConnectionRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ConnectionName))
            {
                    return BadRequest(new { error = "Connection name is required." });
            }

            var result = _connectionService.SetCurrentConnection(request.ConnectionName);
            return Ok(result);
        }

        /// <summary>
        /// Create a new database connection (via DataManagement API)
        /// This endpoint delegates to PPDM39SetupController for database creation
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<CreateConnectionResult>> CreateConnection([FromBody] CreateConnectionRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ConnectionName))
            {
                    return BadRequest(new { error = "Connection name is required." });
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
    }
}
