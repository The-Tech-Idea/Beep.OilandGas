using System.Linq;using System.Threading.Tasks;using Microsoft.AspNetCore.Http;using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Middleware
{
    /// <summary>Blocks all non-setup routes until a "PPDM39" datasource is configured.
    /// Returns 503 with setup instructions for API calls. Lets /setup, /health, /swagger, /api/auth pass through.</summary>
    public class SetupGateMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] ExemptPaths=new[]{"/health","/api/setup","/swagger","/api/auth","/api/connections","/.well-known"};

        public SetupGateMiddleware(RequestDelegate next){_next=next;}

        public async Task InvokeAsync(HttpContext context)
        {
            var path=context.Request.Path.Value??"";
            if(ExemptPaths.Any(e=>path.StartsWith(e,System.StringComparison.OrdinalIgnoreCase))){await _next(context);return;}

            var editor=context.RequestServices.GetService(typeof(IDMEEditor)) as IDMEEditor;
            bool hasDatasource=editor?.ConfigEditor?.DataConnections?.Any(c=>c.ConnectionName=="PPDM39")==true;

            if(!hasDatasource)
            {
                context.Response.StatusCode=503;context.Response.ContentType="application/json";
                await context.Response.WriteAsync("{\"error\":\"Setup required. Database not configured.\",\"setupUrl\":\"/api/setup/wizard/preflight\",\"wizardPage\":\"/ppdm39/setup/beep-wizard\"}");
                return;
            }
            await _next(context);
        }
    }
}
