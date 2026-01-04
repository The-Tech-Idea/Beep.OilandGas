using System.Reflection;
using Beep.OilandGas.DataManager.Core.Interfaces;
using Beep.OilandGas.DataManager.Core.Implementations;
using Beep.OilandGas.DataManager.Core.Implementations.Modules;

namespace Beep.OilandGas.DataManager.Core.Registry
{
    /// <summary>
    /// Registry for discovering and providing all IModuleData implementations
    /// </summary>
    public class ModuleDataRegistry
    {
        private static readonly Lazy<Dictionary<string, IModuleData>> _modules = new Lazy<Dictionary<string, IModuleData>>(DiscoverModules);
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Gets all registered modules
        /// </summary>
        public static IEnumerable<IModuleData> GetAllModules()
        {
            return _modules.Value.Values;
        }

        /// <summary>
        /// Gets a specific module by name (case-insensitive)
        /// </summary>
        public static IModuleData? GetModule(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                return null;
            }

            _modules.Value.TryGetValue(moduleName, out var module);
            if (module != null)
            {
                return module;
            }

            // Try case-insensitive lookup
            var matchingKey = _modules.Value.Keys.FirstOrDefault(k => 
                string.Equals(k, moduleName, StringComparison.OrdinalIgnoreCase));

            return matchingKey != null ? _modules.Value[matchingKey] : null;
        }

        /// <summary>
        /// Gets only required modules
        /// </summary>
        public static IEnumerable<IModuleData> GetRequiredModules()
        {
            return _modules.Value.Values.Where(m => m.IsRequired);
        }

        /// <summary>
        /// Gets modules sorted by execution order
        /// </summary>
        public static IEnumerable<IModuleData> GetModulesByExecutionOrder()
        {
            return _modules.Value.Values.OrderBy(m => m.ExecutionOrder);
        }

        /// <summary>
        /// Gets modules sorted by execution order with dependencies resolved
        /// </summary>
        public static IEnumerable<IModuleData> GetModulesWithDependenciesResolved()
        {
            var allModules = _modules.Value.Values.ToList();
            var sorted = new List<IModuleData>();
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();

            void Visit(IModuleData module)
            {
                if (visiting.Contains(module.ModuleName))
                {
                    throw new InvalidOperationException($"Circular dependency detected involving module: {module.ModuleName}");
                }

                if (visited.Contains(module.ModuleName))
                {
                    return;
                }

                visiting.Add(module.ModuleName);

                foreach (var depName in module.GetDependencies())
                {
                    var depModule = allModules.FirstOrDefault(m => 
                        string.Equals(m.ModuleName, depName, StringComparison.OrdinalIgnoreCase));
                    if (depModule != null)
                    {
                        Visit(depModule);
                    }
                }

                visiting.Remove(module.ModuleName);
                visited.Add(module.ModuleName);
                sorted.Add(module);
            }

            foreach (var module in allModules.OrderBy(m => m.ExecutionOrder))
            {
                if (!visited.Contains(module.ModuleName))
                {
                    Visit(module);
                }
            }

            return sorted;
        }

        /// <summary>
        /// Registers a custom module implementation
        /// </summary>
        public static void RegisterModule(IModuleData module)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            lock (_lockObject)
            {
                _modules.Value[module.ModuleName] = module;
            }
        }

        /// <summary>
        /// Discovers all IModuleData implementations using reflection
        /// </summary>
        private static Dictionary<string, IModuleData> DiscoverModules()
        {
            var modules = new Dictionary<string, IModuleData>(StringComparer.OrdinalIgnoreCase);

            try
            {
                // Get the assembly containing the implementations
                var assembly = Assembly.GetAssembly(typeof(FileSystemModuleData));
                if (assembly == null)
                {
                    // Fallback: manually create instances
                    return CreateDefaultModules();
                }

                // Find all types that implement IModuleData and are not abstract
                var moduleTypes = assembly.GetTypes()
                    .Where(t => typeof(IModuleData).IsAssignableFrom(t) 
                        && !t.IsAbstract 
                        && !t.IsInterface
                        && t != typeof(FileSystemModuleData))
                    .ToList();

                foreach (var moduleType in moduleTypes)
                {
                    try
                    {
                        // Try to create instance with parameterless constructor or with null scriptsBasePath
                        IModuleData? instance = null;

                        // Try parameterless constructor first
                        var parameterlessCtor = moduleType.GetConstructor(Type.EmptyTypes);
                        if (parameterlessCtor != null)
                        {
                            instance = (IModuleData)Activator.CreateInstance(moduleType)!;
                        }
                        else
                        {
                            // Try constructor with string? parameter (scriptsBasePath)
                            var stringCtor = moduleType.GetConstructor(new[] { typeof(string) });
                            if (stringCtor != null)
                            {
                                instance = (IModuleData)Activator.CreateInstance(moduleType, new object?[] { null })!;
                            }
                        }

                        if (instance != null)
                        {
                            modules[instance.ModuleName] = instance;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue discovering other modules
                        System.Diagnostics.Debug.WriteLine($"Failed to create instance of {moduleType.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error discovering modules: {ex.Message}");
                // Fallback to default modules
                return CreateDefaultModules();
            }

            // If no modules found via reflection, use default implementations
            if (modules.Count == 0)
            {
                return CreateDefaultModules();
            }

            return modules;
        }

        /// <summary>
        /// Creates default module instances manually
        /// </summary>
        private static Dictionary<string, IModuleData> CreateDefaultModules()
        {
            var modules = new Dictionary<string, IModuleData>(StringComparer.OrdinalIgnoreCase);

            // Create PPDM39 module first (executes before all others)
            modules["PPDM39"] = new PPDM39ModuleData();
            
            // Create all module instances
            modules["Common"] = new CommonModuleData();
            modules["Security"] = new SecurityModuleData();
            modules["ProductionAccounting"] = new ProductionAccountingModuleData();
            modules["ProductionOperations"] = new ProductionOperationsModuleData();
            modules["ProductionForecasting"] = new ProductionForecastingModuleData();
            modules["EconomicAnalysis"] = new EconomicAnalysisModuleData();
            modules["NodalAnalysis"] = new NodalAnalysisModuleData();
            modules["FlashCalculations"] = new FlashCalculationsModuleData();
            modules["GasLift"] = new GasLiftModuleData();
            modules["HeatMap"] = new HeatMapModuleData();
            modules["PipelineAnalysis"] = new PipelineAnalysisModuleData();
            modules["Accounting"] = new AccountingModuleData();
            modules["Royalty"] = new RoyaltyModuleData();
            modules["Pricing"] = new PricingModuleData();
            modules["Trading"] = new TradingModuleData();
            modules["Agreement"] = new AgreementModuleData();
            modules["Storage"] = new StorageModuleData();
            modules["Ownership"] = new OwnershipModuleData();
            modules["Unitization"] = new UnitizationModuleData();
            modules["Lease"] = new LeaseModuleData();
            modules["PermitsAndApplications"] = new PermitsAndApplicationsModuleData();
            modules["ProspectIdentification"] = new ProspectIdentificationModuleData();
            modules["DevelopmentPlanning"] = new DevelopmentPlanningModuleData();
            modules["Decommissioning"] = new DecommissioningModuleData();
            modules["Imbalance"] = new ImbalanceModuleData();
            modules["Measurement"] = new MeasurementModuleData();
            modules["Analytics"] = new AnalyticsModuleData();
            modules["Calculations"] = new CalculationsModuleData();
            modules["Rendering"] = new RenderingModuleData();
            modules["Export"] = new ExportModuleData();
            modules["LifeCycle"] = new LifeCycleModuleData();
            modules["Validation"] = new ValidationModuleData();
            modules["DataManagement"] = new DataManagementModuleData();
            modules["ChokeAnalysis"] = new ChokeAnalysisModuleData();
            modules["CompressorAnalysis"] = new CompressorAnalysisModuleData();
            modules["PipelineAnalysis"] = new PipelineAnalysisModuleData();
            modules["PlungerLift"] = new PlungerLiftModuleData();
            modules["SuckerRodPumping"] = new SuckerRodPumpingModuleData();
            modules["HydraulicPumps"] = new HydraulicPumpsModuleData();
            modules["WellTestAnalysis"] = new WellTestAnalysisModuleData();
            modules["GasProperties"] = new GasPropertiesModuleData();
            modules["OilProperties"] = new OilPropertiesModuleData();
            modules["DCA"] = new DCAModuleData();
            modules["PumpPerformance"] = new PumpPerformanceModuleData();
            modules["ProductionForecasting"] = new ProductionForecastingModuleData();

            return modules;
        }
    }
}
