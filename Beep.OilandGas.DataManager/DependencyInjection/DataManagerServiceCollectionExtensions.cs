using Beep.OilandGas.DataManager.Core.Interfaces;
using Beep.OilandGas.DataManager.Core.Registry;
using Beep.OilandGas.DataManager.Core.State;
using Beep.OilandGas.DataManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.DataManager.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering DataManager services
    /// </summary>
    public static class DataManagerServiceCollectionExtensions
    {
        /// <summary>
        /// Adds DataManager services to the service collection
        /// </summary>
        public static IServiceCollection AddDataManager(this IServiceCollection services, Action<DataManagerOptions>? configure = null)
        {
            var options = new DataManagerOptions();
            configure?.Invoke(options);

            // Register core services
            services.AddScoped<IDataManager, Beep.OilandGas.DataManager.Services.DataManager>();
            services.AddScoped<ScriptValidator>();

            // Register all IModuleData implementations
            var allModules = ModuleDataRegistry.GetAllModules();
            foreach (var module in allModules)
            {
                services.AddSingleton(typeof(IModuleData), sp =>
                {
                    // Create new instance with optional scripts base path from options
                    var moduleType = module.GetType();
                    var ctor = moduleType.GetConstructor(new[] { typeof(string) });
                    if (ctor != null)
                    {
                        return (IModuleData)Activator.CreateInstance(moduleType, new object?[] { options.StateDirectory })!;
                    }
                    return module;
                });
            }

            // Register state store
            if (options.StateStoreType == StateStoreType.File)
            {
                services.AddSingleton<IExecutionStateStore>(sp =>
                {
                    var logger = sp.GetService<ILogger<FileExecutionStateStore>>();
                    return new FileExecutionStateStore(options.StateDirectory, logger);
                });
            }
            else if (options.StateStoreType == StateStoreType.Database)
            {
                // Database state store requires IDataSource, so it should be registered per-request
                services.AddScoped<IExecutionStateStore>(sp =>
                {
                    // Note: IDataSource should be provided when creating DatabaseExecutionStateStore
                    // This is a placeholder - actual implementation should inject IDataSource from context
                    throw new InvalidOperationException("DatabaseExecutionStateStore requires IDataSource. Use AddDataManagerWithDatabaseStateStore instead.");
                });
            }

            return services;
        }

        /// <summary>
        /// Adds DataManager services with database-based state store
        /// </summary>
        public static IServiceCollection AddDataManagerWithDatabaseStateStore(
            this IServiceCollection services,
            Func<IServiceProvider, IDataSource> dataSourceFactory,
            Action<DataManagerOptions>? configure = null)
        {
            var options = new DataManagerOptions();
            configure?.Invoke(options);

            services.AddScoped<IDataManager, Beep.OilandGas.DataManager.Services.DataManager>();
            services.AddScoped<ScriptValidator>();

            services.AddScoped<IExecutionStateStore>(sp =>
            {
                var dataSource = dataSourceFactory(sp);
                var logger = sp.GetService<ILogger<DatabaseExecutionStateStore>>();
                return new DatabaseExecutionStateStore(dataSource, logger);
            });

            return services;
        }
    }

    /// <summary>
    /// Options for configuring DataManager
    /// </summary>
    public class DataManagerOptions
    {
        public StateStoreType StateStoreType { get; set; } = StateStoreType.File;
        public string? StateDirectory { get; set; }
    }

    /// <summary>
    /// Type of state store to use
    /// </summary>
    public enum StateStoreType
    {
        File,
        Database
    }
}
