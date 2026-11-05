using Microsoft.Extensions.DependencyInjection;

namespace Beep.OilandGas.Web.Services;

/// <summary>The component circuit must receive encrypted retained originals from browser storage.</summary>
public static class OilGasComponentTransport
{
    // The report contract bounds its original at 16 MiB. Encryption/base64 and the JS-result
    // envelope need additional room. Browser quota failures still stop retention before submission.
    public const long MaximumRetainedOriginalMessageBytes = 32L * 1024 * 1024;

    public static IServerSideBlazorBuilder AddOilGasInteractiveServerComponents(this IRazorComponentsBuilder components) =>
        components.AddInteractiveServerComponents().AddHubOptions(options =>
            options.MaximumReceiveMessageSize = MaximumRetainedOriginalMessageBytes);
}
