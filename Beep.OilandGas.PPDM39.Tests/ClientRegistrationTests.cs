using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.PPDM39.Tests;

public class ClientRegistrationTests
{
    [Theory]
    [InlineData("Local")]
    [InlineData("Auto")]
    [InlineData("Remote")]
    public void RemoteRegistrationOverridesLocalConfigurationWithoutCreatingEditor(string mode)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDMEEditor>(_ => throw new InvalidOperationException("Editor must not be created"));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["BeepOilandGas:AccessMode"] = mode,
            ["BeepOilandGas:UseLocalServices"] = "true",
            ["ApiService:BaseUrl"] = "https://example.invalid/"
        }).Build();

        services.AddBeepOilandGasAppRemote(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<AppOptions>();
        Assert.Equal(ServiceAccessMode.Remote, options.AccessMode);
        Assert.False(options.UseLocalServices);
        Assert.Equal("https://example.invalid/", options.ApiBaseUrl);
    }

    [Fact]
    public void AutoRegistrationDoesNotInstantiateRegisteredEditor()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDMEEditor>(_ => throw new InvalidOperationException("Registration must not create editor"));
        services.AddBeepOilandGasAppAuto(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();
        Assert.Equal(ServiceAccessMode.Local, provider.GetRequiredService<AppOptions>().AccessMode);
    }
}
