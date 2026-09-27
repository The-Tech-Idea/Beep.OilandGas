using System;
using System.Net.Http;
using System.Linq;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Client.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering Beep Oil & Gas client services
    /// </summary>
    public static class ClientServiceCollectionExtensions
    {
        /// <summary>
        /// Register AppClass in remote mode
        /// </summary>
        public static IServiceCollection AddBeepOilandGasApp(
            this IServiceCollection services,
            AppOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.AccessMode == ServiceAccessMode.Remote && string.IsNullOrEmpty(options.ApiBaseUrl))
                throw new ArgumentException("ApiBaseUrl is required for remote mode", nameof(options));

            services.AddSingleton(options);
            services.AddHttpClient();

            // The API admits only signed-in people, so remote calls carry a person's access token, which the host supplies
            // (IAuthenticationProvider — the Web's is the signed-in person's own). It signed in here with a username and
            // password (the resource-owner password grant), which the identity server does not offer, or sent no token.
            services.AddScoped<IBeepOilandGasApp>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();
                var authProvider = sp.GetService<IAuthenticationProvider>()
                    ?? throw new InvalidOperationException(
                        "The Beep Oil & Gas client in remote mode needs an IAuthenticationProvider supplying the signed-in person's access token; register one.");
                var logger = sp.GetService<ILogger<BeepOilandGasApp>>();
                return new BeepOilandGasApp(httpClient, options, authProvider, logger);
            });

            return services;
        }

        /// <summary>
        /// Register AppClass in local mode
        /// </summary>
        public static IServiceCollection AddBeepOilandGasAppLocal(
            this IServiceCollection services,
            Action<AppOptions>? configureOptions = null)
        {
            var options = new AppOptions
            {
                AccessMode = ServiceAccessMode.Local,
                DefaultConnectionName = "PPDM39"
            };
            configureOptions?.Invoke(options);

            services.AddSingleton(options);

            services.AddScoped<IBeepOilandGasApp>(sp =>
            {
                var dmeEditor = sp.GetRequiredService<IDMEEditor>();
                var logger = sp.GetService<ILogger<BeepOilandGasApp>>();
                return new BeepOilandGasApp(sp, dmeEditor, options, logger);
            });

            return services;
        }

        /// <summary>
        /// Register AppClass with auto-detection mode
        /// </summary>
        public static IServiceCollection AddBeepOilandGasAppAuto(
            this IServiceCollection services,
            IConfiguration configuration)
            => RegisterConfiguredApp(services, configuration, remoteOnly: false);

        public static IServiceCollection AddBeepOilandGasAppRemote(
            this IServiceCollection services,
            IConfiguration configuration)
            => RegisterConfiguredApp(services, configuration, remoteOnly: true);

        private static IServiceCollection RegisterConfiguredApp(
            IServiceCollection services, IConfiguration configuration, bool remoteOnly)
        {
            var section = configuration.GetSection("BeepOilandGas");
            var options = new AppOptions();
            section.Bind(options);

            if (string.IsNullOrWhiteSpace(options.ApiBaseUrl))
            {
                options.ApiBaseUrl = configuration["ApiService:BaseUrl"];
            }

            if (remoteOnly)
            {
                options.AccessMode = ServiceAccessMode.Remote;
                options.UseLocalServices = false;
            }
            else if (options.AccessMode == ServiceAccessMode.Auto)
            {
                // Inspect registrations without constructing a second container or editor.
                options.AccessMode = options.UseLocalServices && services.Any(d => d.ServiceType == typeof(IDMEEditor))
                    ? ServiceAccessMode.Local
                    : ServiceAccessMode.Remote;
            }

            if (options.AccessMode == ServiceAccessMode.Remote)
            {
                return services.AddBeepOilandGasApp(options);
            }
            else
            {
                return services.AddBeepOilandGasAppLocal(opts =>
                {
                    opts.DefaultConnectionName = options.DefaultConnectionName;
                });
            }
        }
    }
}
