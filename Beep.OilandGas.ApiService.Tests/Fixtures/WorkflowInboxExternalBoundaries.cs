#if WORKFLOW_INBOX_FOCUSED
// Explicit test-only external seams. No OIDC, production PartyIdClaims or failure store certification.
namespace Beep.OilandGas.ApiService.Services
{
    public static class WorkflowInboxResolvedAccountFixture
    {
        public static string? FindActingUserId(this System.Security.Claims.ClaimsPrincipal user) => user.FindFirst("party_id")?.Value;
        public static string ActingUserId(this System.Security.Claims.ClaimsPrincipal user) =>
            user.FindFirst("party_id")?.Value ?? throw new UnauthorizedAccessException("No resolved test account.");
    }
}
namespace TheTechIdeaWeb.Diagnostics
{
    public enum FailureSeverity { Degraded }
    public interface IFailureReporter
    {
        void ReportHandled(Exception failure, string operation, string consequence, FailureSeverity severity = FailureSeverity.Degraded);
    }
    public static class FailureReferenceFormat
    {
        // Failure reference formatting is outside this focused fixture's contract.
        public static bool IsWellFormed(string? value) => false;
    }
}
namespace TheTechIdeaWeb.Http
{
    public static class HttpResponseErrorMessage
    {
        public static string? TryReadFromBody(string body, string? reasonPhrase, TheTechIdeaWeb.Diagnostics.IFailureReporter failures)
        {
            try
            {
                using var document=System.Text.Json.JsonDocument.Parse(body);
                foreach(var name in new[]{"detail","error"})
                    if(document.RootElement.TryGetProperty(name,out var value) && value.ValueKind==System.Text.Json.JsonValueKind.String)return value.GetString();
                return null;
            }
            catch(System.Text.Json.JsonException){return null;}
        }
    }
}
namespace Beep.OilandGas.Web.Services
{
    // Explicit wording fixture for complete component composition; the production diagnostics wording is not certified.
    public sealed class OilGasCallFailures(TheTechIdeaWeb.Diagnostics.IFailureReporter failures)
    {
        public string Explain(Exception error,string operation,string whatDidNotHappen)
        {
            failures.ReportHandled(error,operation,whatDidNotHappen);
            return error is OilGasApiException api && api.Sentence is not null ? api.Sentence : whatDidNotHappen;
        }
    }
}
#endif
