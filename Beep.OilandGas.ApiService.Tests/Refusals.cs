using Beep.OilandGas.Models.Core.Refusals;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>Asserts that a call is refused, and how (OILGAS-CATCH-01).</summary>
internal static class Refusals
{
    public static RefusalException Forbidden(Action call) =>
        Of(RefusalKind.Forbidden, Assert.Throws<RefusalException>(call));

    public static async Task<RefusalException> ForbiddenAsync(Func<Task> call) =>
        Of(RefusalKind.Forbidden, await Assert.ThrowsAsync<RefusalException>(call));

    public static async Task<RefusalException> RefusedAsync(RefusalKind kind, Func<Task> call) =>
        Of(kind, await Assert.ThrowsAsync<RefusalException>(call));

    public static RefusalException Refused(RefusalKind kind, Action call) =>
        Of(kind, Assert.Throws<RefusalException>(call));

    private static RefusalException Of(RefusalKind kind, RefusalException refusal)
    {
        Assert.Equal(kind, refusal.Kind);
        Assert.False(string.IsNullOrWhiteSpace(refusal.Sentence));
        return refusal;
    }
}
