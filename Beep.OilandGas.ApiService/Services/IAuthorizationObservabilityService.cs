using System.Threading.Tasks;

namespace Beep.OilandGas.ApiService.Services;

public interface IAuthorizationObservabilityService
{
    Task RecordPolicyEvaluationAsync(AuthorizationObservation observation);
}

public sealed class AuthorizationObservation
{
    public string PolicyName { get; init; } = string.Empty;
    public string? UserId { get; init; }
    public string? AssetId { get; init; }
    public string? AssetType { get; init; }
    public string? RequiredPermission { get; init; }
    public string Decision { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public string? Endpoint { get; init; }
    public string? HttpMethod { get; init; }
    public string? CorrelationId { get; init; }
    public string? ClientIp { get; init; }
}