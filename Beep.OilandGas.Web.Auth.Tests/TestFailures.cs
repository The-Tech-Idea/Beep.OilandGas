using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TheTechIdeaWeb.Diagnostics;
using TheTechIdeaWeb.Diagnostics.Notifications;

namespace Beep.OilandGas.Web.Auth.Tests;

/// <summary>
/// The Web's failure wording built as the host builds it — the shared library's own sentences, read from its resources — for
/// tests of services and page states that answer a failure with a sentence (OILGAS-CATCH-01).
/// </summary>
internal static class TestFailures
{
    public static FailureWording Wording { get; } = new(new StringLocalizer<FailureWording>(
        new ResourceManagerStringLocalizerFactory(Options.Create(new LocalizationOptions()), NullLoggerFactory.Instance)));

    public static OilGasCallFailures Calls(IFailureReporter reporter) =>
        new(new OilGasApiFailures(reporter, Wording), reporter, Wording);
}
