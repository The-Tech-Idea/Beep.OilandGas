using System;
using System.Net.Http;
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

            // Register authentication provider if credentials provided
            if (!string.IsNullOrEmpty(options.Username) && !string.IsNullOrEmpty(options.Password))
            {
                services.AddScoped<IAuthenticationProvider>(sp =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient();
                    return new CredentialsAuthenticationProvider(
                        httpClient,
                        options.IdentityServerUrl ?? throw new ArgumentException("IdentityServerUrl is required"),
                        options.Username,
                        options.Password,
                        options.ClientId,
                        options.ClientSecret);
                });
            }

            // Register AppClass
            services.AddScoped<IBeepOilandGasApp>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();
                var authProvider = sp.GetService<IAuthenticationProvider>();
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
        {
            var section = configuration.GetSection("BeepOilandGas");
            var options = new AppOptions();
            section.Bind(options);

            if (options.AccessMode == ServiceAccessMode.Auto)
            {
                var serviceProvider = services.BuildServiceProvider();
                try
                {
                    var dmeEditor = serviceProvider.GetService<IDMEEditor>();
                    if (dmeEditor != null && options.UseLocalServices)
                    {
                        options.AccessMode = ServiceAccessMode.Local;
                        return services.AddBeepOilandGasAppLocal(opts =>
                        {
                            opts.DefaultConnectionName = options.DefaultConnectionName;
                        });
                    }
                }
                catch { }
                options.AccessMode = ServiceAccessMode.Remote;
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
