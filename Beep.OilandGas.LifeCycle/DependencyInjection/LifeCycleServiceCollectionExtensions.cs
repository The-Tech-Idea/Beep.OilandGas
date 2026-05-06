using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.LifeCycle.Core.Interfaces;
using Beep.OilandGas.LifeCycle.Services.Seeding;

namespace Beep.OilandGas.LifeCycle.DependencyInjection;

/// <summary>
/// Extension methods for registering LifeCycle services in the DI container.
/// Must be called after AddBeepServices (which provides IDMEEditor, ICommonColumnHandler, etc.).
/// </summary>
public static class LifeCycleServiceCollectionExtensions
{
    /// <summary>
    /// Registers core LifeCycle services with the DI container.
    /// Additional phase services, process services, and lifecycle services should be
    /// registered in Program.cs with their specific constructor requirements.
    /// </summary>
    public static IServiceCollection AddLifeCycleServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionName = configuration.GetValue("BeepOg:DatabaseConnectionName", "PPDM39");

        // ── Seed Service ────────────────────────────────────────────────────
        services.AddScoped<ILifeCycleSeedService, LifeCycleSeedService>(sp =>
        {
            var editor = sp.GetRequiredService<IDMEEditor>();
            var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
            var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
            var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
            var logger = sp.GetRequiredService<ILogger<LifeCycleSeedService>>();

            return new LifeCycleSeedService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
        });

        return services;
    }
}
