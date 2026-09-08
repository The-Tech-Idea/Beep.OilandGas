using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ModuleMigrationScopeTests
{
    [Fact]
    public void ModuleSchemasRejectLegacyAndCanonicalIdentityEntities()
    {
        foreach (var entity in new[]
        {
            typeof(Beep.OilandGas.Models.Data.Security.USER),
            typeof(Beep.OilandGas.UserManagement.Models.Identity.AppRole),
            typeof(TheTechIdea.Data.OilGas.AppRoleExtension),
            typeof(Microsoft.AspNetCore.Identity.IdentityRole)
        })
            Assert.Throws<ArgumentException>(() => ModuleSchemaBoundary.Validate([entity]));
        ModuleSchemaBoundary.Validate([typeof(Beep.OilandGas.PPDM39.Models.WELL)]);
    }

    [Fact]
    public void FluidModulesDeclareCompositionAndResultTables()
    {
        var oil = new Beep.OilandGas.OilProperties.Modules.OilPropertiesModule();
        var gas = new Beep.OilandGas.GasProperties.Modules.GasPropertiesModule();
        Assert.Equal("OIL_PROPERTIES", oil.ModuleId);
        Assert.Equal("GAS_PROPERTIES", gas.ModuleId);
        Assert.Equal(new[] { typeof(Beep.OilandGas.Models.Data.Common.OIL_COMPOSITION), typeof(Beep.OilandGas.Models.Data.Common.OIL_PROPERTY_RESULT) }, oil.EntityTypes);
        Assert.Equal(new[] { typeof(Beep.OilandGas.Models.Data.Common.GAS_COMPOSITION), typeof(Beep.OilandGas.Models.Data.Common.GAS_COMPOSITION_COMPONENT) }, gas.EntityTypes);
    }

    [Fact]
    public void FlashManifestIncludesPersistedResult()
    {
        var module = new Beep.OilandGas.FlashCalculations.Modules.FlashCalculationsModule(new Beep.OilandGas.PPDM39.Core.Interfaces.ModuleSetupContext());
        Assert.Contains(typeof(Beep.OilandGas.Models.Data.FlashCalculations.FLASH_CALCULATION_RESULT), module.EntityTypes);
        Assert.Equal(2, module.EntityTypes.Count);
    }

    [Fact]
    public void EconomicsManifestIncludesResultAndOnlyItsOwnReferences()
    {
        var module = new Beep.OilandGas.EconomicAnalysis.Modules.EconomicsModule(new Beep.OilandGas.PPDM39.Core.Interfaces.ModuleSetupContext());
        Assert.Contains(typeof(Beep.OilandGas.Models.Data.EconomicAnalysis.ECONOMIC_ANALYSIS_RESULT), module.EntityTypes);
        Assert.Equal(4, module.EntityTypes.Count);
    }

    [Fact]
    public void GasLiftManifestIncludesPersistedEntitiesNotCalculationDtos()
    {
        var module = new Beep.OilandGas.GasLift.Modules.GasLiftModule(new Beep.OilandGas.PPDM39.Core.Interfaces.ModuleSetupContext());
        Assert.Contains(typeof(Beep.OilandGas.Models.Data.GasLift.GAS_LIFT_DESIGN), module.EntityTypes);
        Assert.Contains(typeof(Beep.OilandGas.Models.Data.GasLift.GAS_LIFT_PERFORMANCE), module.EntityTypes);
        Assert.DoesNotContain(typeof(Beep.OilandGas.Models.Data.GasLift.GAS_LIFT_WELL_PROPERTIES), module.EntityTypes);
        Assert.Equal(3, module.EntityTypes.Count);
    }

    private static readonly (string ModuleId, string ModuleName, int Order, IReadOnlyList<Type> EntityTypes)[] Modules =
    [ ("PRODUCTION", "Production", 20, new[] { typeof(string) }),
      ("EXPLORATION", "Exploration", 10, new[] { typeof(int) }),
      ("SECURITY", "Legacy security", 1, new[] { typeof(bool) }) ];

    [Fact]
    public void OnlySelectedModuleEntitiesAreIncludedInDeclaredOrder()
    {
        Assert.Equal(new[] { typeof(int), typeof(string) },
            ModuleMigrationScope.Resolve(["production", "EXPLORATION", "PRODUCTION"], Modules));
        Assert.Equal(new[] { typeof(string) }, ModuleMigrationScope.Resolve(["PRODUCTION"], Modules));
    }

    [Theory]
    [InlineData("SECURITY")]
    [InlineData("unknown")]
    [InlineData("")]
    public void InvalidSelectionCannotFallBackToAllModules(string module)
        => Assert.Throws<ArgumentException>(() => ModuleMigrationScope.Resolve([module], Modules));

    [Fact]
    public void EmptyExplicitSelectionIsRejected()
        => Assert.Throws<ArgumentException>(() => ModuleMigrationScope.Resolve([], Modules));

    [Fact]
    public void CoreSelectionUsesCanonicalSchemaAndDeduplicatesReferenceModules()
    {
        var available = new (string ModuleId, string ModuleName, int Order, IReadOnlyList<Type> EntityTypes)[]
        {
            ("PPDM_CORE", "Core", 0, Array.Empty<Type>()),
            ("REFERENCES", "References", 10, [typeof(string)]),
            ("FEATURE", "Feature", 20, [typeof(decimal)])
        };
        Assert.Equal(new[] { typeof(int), typeof(string) },
            ModuleMigrationScope.Resolve(["ppdm_core", "REFERENCES"], available, [typeof(int), typeof(string)]));
        Assert.Equal(new[] { typeof(decimal) },
            ModuleMigrationScope.Resolve(["FEATURE"], available, [typeof(int), typeof(string)]));
    }
}
