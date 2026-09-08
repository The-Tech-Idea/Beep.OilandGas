using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public class RepositoryRegistrationTests
{
    [Theory]
    [InlineData("SqlServer", "Microsoft.EntityFrameworkCore.SqlServer")]
    [InlineData("PostgreSql", "Npgsql.EntityFrameworkCore.PostgreSQL")]
    [InlineData("Oracle", "Oracle.EntityFrameworkCore")]
    public void RegistrationResolvesIdentityStoresAndKeepsModuleConnectionSeparate(string provider, string providerName)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Repository:Provider"] = provider,
            ["Repository:ConnectionString"] = "Data Source=repository",
            ["ConnectionStrings:PPDM39"] = "UnrelatedModuleConnection"
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOilGasRepository(config);
        using var root = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = root.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RepositoryDbContext>();
        Assert.Equal(providerName, context.Database.ProviderName);
        var connection = new System.Data.Common.DbConnectionStringBuilder
        {
            ConnectionString = context.Database.GetConnectionString()
        };
        Assert.Equal("repository", connection["Data Source"]);
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<UserManager<OilGasUser>>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<RepositoryReadinessService>());
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("SqlServer", null)]
    [InlineData("Sqlite", "Data Source=repository")]
    public void MissingOrUnsupportedSettingsFailExplicitly(string? provider, string? connection)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Repository:Provider"] = provider,
            ["Repository:ConnectionString"] = connection,
            ["ConnectionStrings:PPDM39"] = "MustNotBeUsedAsRepository"
        }).Build();
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddOilGasRepository(config));
    }
}
