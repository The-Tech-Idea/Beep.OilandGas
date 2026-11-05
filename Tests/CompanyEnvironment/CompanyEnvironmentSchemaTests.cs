using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Xunit;

namespace CompanyEnvironment.Tests;

public sealed class CompanyEnvironmentSchemaTests
{
    [Fact]
    public void CompanyEnvironmentUsesOneBoundedIdentityAndPreservesRevisionFacts()
    {
        var properties = typeof(COMPANY_OILGAS_ENVIRONMENT).GetProperties();
        Assert.Equal("COMPANY_OILGAS_ENVIRONMENT_ID", Assert.Single(properties.Where(p => p.GetCustomAttribute<KeyAttribute>() is not null)).Name);
        foreach (var (name, length) in new[] { ("COMPANY_OILGAS_ENVIRONMENT_ID", 64), ("OWNER_TENANT_ID", 64),
            ("OWNER_COMPANY_BA_ID", 128), ("COUNTRY_CODE", 2), ("OVERRIDES_JSON", CompanyEnvironmentIdentity.MaxOverridesLength),
            ("REVISION", 64), ("PREVIOUS_REVISION", 64), ("INTENT_ID", 64), ("TOOLS_VERSION", 128) })
        {
            var property = typeof(COMPANY_OILGAS_ENVIRONMENT).GetProperty(name)!;
            Assert.Equal(typeof(string), property.PropertyType);
            if (name == "PREVIOUS_REVISION") Assert.Null(property.GetCustomAttribute<RequiredAttribute>());
            else Assert.NotNull(property.GetCustomAttribute<RequiredAttribute>());
            Assert.Equal(length, property.GetCustomAttribute<MaxLengthAttribute>()!.Length);
        }
    }
}
