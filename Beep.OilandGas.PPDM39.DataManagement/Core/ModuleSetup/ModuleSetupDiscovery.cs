using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup
{
    /// <summary>
    /// Discovers and registers module setup plugins from loaded Beep.OilandGas assemblies.
    /// Any concrete <see cref="ModuleSetupBase"/> or <see cref="IModuleSetup"/> implementation
    /// is treated as a database setup module.
    /// </summary>
    public static class ModuleSetupDiscovery
    {
        private const string AssemblyPrefix = "Beep.OilandGas";

        public static IServiceCollection AddDiscoveredModuleSetups(this IServiceCollection services)
        {
            foreach (var moduleType in DiscoverModuleSetupTypes())
            {
                services.AddScoped(typeof(IModuleSetup), provider =>
                    (IModuleSetup)ActivatorUtilities.CreateInstance(provider, moduleType));
            }

            return services;
        }

        public static IReadOnlyList<Type> DiscoverModuleSetupTypes()
        {
            LoadCandidateAssemblies();

            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(IsCandidateAssembly)
                .SelectMany(GetLoadableTypes)
                .Where(IsModuleSetupType)
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .ToList();
        }

        private static void LoadCandidateAssemblies()
        {
            var baseDirectory = AppContext.BaseDirectory;
            if (string.IsNullOrWhiteSpace(baseDirectory) || !Directory.Exists(baseDirectory))
                return;

            foreach (var path in Directory.EnumerateFiles(baseDirectory, $"{AssemblyPrefix}*.dll", SearchOption.TopDirectoryOnly))
            {
                var assemblyName = Path.GetFileNameWithoutExtension(path);
                if (AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
                    string.Equals(assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                try
                {
                    Assembly.Load(new AssemblyName(assemblyName));
                }
                catch
                {
                    // Optional module assemblies are plugin-like; a failed load must not break setup startup.
                }
            }
        }

        private static bool IsCandidateAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            return !string.IsNullOrWhiteSpace(name) &&
                   name.StartsWith(AssemblyPrefix, StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null)!;
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        private static bool IsModuleSetupType(Type type)
        {
            return type.IsClass &&
                   !type.IsAbstract &&
                   !type.IsGenericTypeDefinition &&
                   typeof(IModuleSetup).IsAssignableFrom(type) &&
                   (typeof(ModuleSetupBase).IsAssignableFrom(type) || type.GetInterfaces().Contains(typeof(IModuleSetup)));
        }
    }
}