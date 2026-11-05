using Beep.OilandGas.Models.Data.Company;
// Focused test-only contract for the unavailable shared diagnostics project.
// The production controller's unrelated decommissioning failure path is not certified here.
namespace TheTechIdeaWeb.Http
{
    public static class HttpResponseErrorMessage
    {
        public static string? TryReadFromBody(string body,string? reasonPhrase,TheTechIdeaWeb.Diagnostics.IFailureReporter failures)
        {
            try{using var json=System.Text.Json.JsonDocument.Parse(body);foreach(var key in new[]{"detail","error"})if(json.RootElement.TryGetProperty(key,out var value) && value.ValueKind==System.Text.Json.JsonValueKind.String)return value.GetString();return null;}
            catch(System.Text.Json.JsonException){return null;}
        }
    }
}
namespace Beep.OilandGas.Web.Services
{
    // Explicit test wording seam; production diagnostics/store/wording are not certified.
    public sealed class OilGasCallFailures(TheTechIdeaWeb.Diagnostics.IFailureReporter failures)
    {
        public string Explain(Exception error,string operation,string message){failures.ReportHandled(error,operation,message);return error is OilGasApiException api && api.Sentence is not null?api.Sentence:message;}
    }
}
