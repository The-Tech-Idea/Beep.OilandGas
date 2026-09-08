using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.DependencyInjection;

/// <summary>
/// Reduces Program.cs bloat by providing a generic factory for services
/// that follow the standard BeepDM constructor pattern.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a service with the standard BeepDM constructor pattern:
    /// (IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, string, ILogger?)
    /// </summary>
    public static IServiceCollection AddBeepService<TService>(
        this IServiceCollection services,
        string connectionName) where TService : class
    {
        services.AddScoped(sp =>
        {
            var editor = sp.GetRequiredService<IDMEEditor>();
            var cch = sp.GetRequiredService<ICommonColumnHandler>();
            var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
            var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<TService>();

            return ActivatorUtilities.CreateInstance<TService>(sp, editor, cch, defaults, metadata, connectionName, logger);
        });
        return services;
    }

    /// <summary>
    /// Registers a service with its interface using the standard BeepDM constructor pattern.
    /// </summary>
    public static IServiceCollection AddBeepService<TInterface, TService>(
        this IServiceCollection services,
        string connectionName)
        where TInterface : class
        where TService : class, TInterface
    {
        services.AddScoped<TInterface>(sp =>
        {
            var editor = sp.GetRequiredService<IDMEEditor>();
            var cch = sp.GetRequiredService<ICommonColumnHandler>();
            var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
            var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<TService>();

            return ActivatorUtilities.CreateInstance<TService>(sp, editor, cch, defaults, metadata, connectionName, logger);
        });
        return services;
    }
}
