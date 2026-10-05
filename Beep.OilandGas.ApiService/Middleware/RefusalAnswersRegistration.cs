using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Beep.OilandGas.ApiService.Middleware
{
    /// <summary>The API's answers to an exception a request did not catch (OILGAS-CATCH-01).</summary>
    public static class RefusalAnswersRegistration
    {
        /// <summary>
        /// Problem details (RFC 9457) for every answer <c>UseExceptionHandler</c> writes, and the refusal handler ahead of
        /// every other exception handler whatever order the registrations are made in: a refusal is answered before the
        /// failure service's handler takes it for a failure and reports it.
        /// </summary>
        public static IServiceCollection AddRefusalAnswers(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddProblemDetails();
            services.Insert(0, ServiceDescriptor.Singleton<IExceptionHandler, RefusalExceptionHandler>());
            return services;
        }
    }
}
