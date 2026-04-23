using System;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.UserManagement.Models.Audit;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Services;

public sealed class AuthorizationObservabilityService : IAuthorizationObservabilityService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly ILogger<AuthorizationObservabilityService> _logger;
    private readonly string _connectionName;

    public AuthorizationObservabilityService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        ILogger<AuthorizationObservabilityService> logger,
        string connectionName = "PPDM39")
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
        _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionName = connectionName;
    }

    public async Task RecordPolicyEvaluationAsync(AuthorizationObservation observation)
    {
        if (observation == null)
        {
            return;
        }

        var payload = JsonSerializer.Serialize(new
        {
            observation.PolicyName,
            observation.UserId,
            observation.AssetId,
            observation.AssetType,
            observation.RequiredPermission,
            observation.Decision,
            observation.Reason,
            observation.Endpoint,
            observation.HttpMethod,
            observation.CorrelationId,
            observation.ClientIp,
            EventUtc = DateTime.UtcNow
        });

        if (string.Equals(observation.Decision, "Denied", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(observation.Decision, "Error", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Authorization decision {Decision} for policy {PolicyName}. User={UserId}, Asset={AssetType}:{AssetId}, Permission={Permission}, Reason={Reason}, CorrelationId={CorrelationId}",
                observation.Decision,
                observation.PolicyName,
                observation.UserId,
                observation.AssetType,
                observation.AssetId,
                observation.RequiredPermission,
                observation.Reason,
                observation.CorrelationId);
        }
        else
        {
            _logger.LogInformation(
                "Authorization decision {Decision} for policy {PolicyName}. User={UserId}, Asset={AssetType}:{AssetId}, Permission={Permission}, CorrelationId={CorrelationId}",
                observation.Decision,
                observation.PolicyName,
                observation.UserId,
                observation.AssetType,
                observation.AssetId,
                observation.RequiredPermission,
                observation.CorrelationId);
        }

        try
        {
            var eventType = observation.Decision switch
            {
                "Denied" => "AccessDenied",
                "Error" => "PolicyEvaluationError",
                _ => "PolicyEvaluation"
            };

            var auditRepo = new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                typeof(UserAccessAuditEvent),
                _connectionName,
                "USER_ACCESS_AUDIT_EVENT");

            var ev = new UserAccessAuditEvent
            {
                EVENT_ID = Guid.NewGuid().ToString(),
                USER_ID = observation.UserId ?? "anonymous",
                EVENT_TYPE = eventType,
                TARGET_RESOURCE = $"{observation.AssetType}:{observation.AssetId}",
                RESULT = $"{observation.Decision}:{observation.Reason}",
                EVENT_UTC = DateTime.UtcNow,
                DETAILS_JSON = payload,
                CORRELATION_ID = observation.CorrelationId,
                CLIENT_IP = observation.ClientIp,
                SCOPE_CONTEXT = observation.Endpoint
            };

            await auditRepo.InsertAsync(ev, observation.UserId ?? "system");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to persist authorization audit event for policy {PolicyName} and user {UserId}",
                observation.PolicyName,
                observation.UserId);
        }
    }
}