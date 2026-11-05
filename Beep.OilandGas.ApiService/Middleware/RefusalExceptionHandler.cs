using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Middleware
{
    /// <summary>
    /// Answers application refusals and known unsupported transaction-driver capabilities as problem details.
    /// Everything else goes on to the failure service's handler: reported, answered 500 with its reference.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. ASP.NET Core's own extension point (<see cref="IExceptionHandler"/>, run by
    /// <c>UseExceptionHandler</c>) in place of the hand-written middleware that mapped the framework's exception types to
    /// refusals. A refusal is not reported: it is the application's answer to what was sent, as a returned outcome would
    /// be, and the person is told why. It is logged at Information.
    /// </remarks>
    public sealed class RefusalExceptionHandler(ILogger<RefusalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var refusal = exception switch
            {
                RefusalException existing => existing,
                DataSourceTransactionCapabilityException => RefusalException.Conflict("The selected data source cannot verify transactions. Ask an administrator to update its Beep driver."),
                NativeSchemaCompatibilityException => RefusalException.Conflict("The selected data source schema is incompatible with this workflow. Ask an administrator to review the Beep migration and driver."),
                _ => null
            };
            if (refusal is null) return false;

            var status = refusal.Kind switch
            {
                RefusalKind.Invalid => StatusCodes.Status400BadRequest,
                RefusalKind.NotFound => StatusCodes.Status404NotFound,
                RefusalKind.Conflict => StatusCodes.Status409Conflict,
                RefusalKind.Forbidden => StatusCodes.Status403Forbidden,
            };

            logger.LogInformation("{Method} {Path} refused ({Kind}): {Sentence}",
                httpContext.Request.Method, httpContext.Request.Path.Value, refusal.Kind, refusal.Sentence);

            await TypedResults.Problem(detail: refusal.Sentence, statusCode: status).ExecuteAsync(httpContext);
            return true;
        }
    }
}
