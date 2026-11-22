using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// FND-06 / ADR-012: severity mapping, stickiness, and the refusal of a failure toast without a
/// reference.
/// </summary>
public sealed class ToastPipelineTests
{
    private static (OgNotificationHub Hub, OgToastService Toast) Build()
    {
        var toast = new OgToastService();
        var hub = new OgNotificationHub(toast, new OgLoc());
        return (hub, toast);
    }

    [Fact]
    public void A_failure_toast_without_a_reference_is_refused()
    {
        var toast = new OgToastService();

        Assert.Throws<ArgumentException>(() =>
            toast.Show(new OgToast(OgToastKind.Failure, "Fail.Body")));
    }

    [Fact]
    public void An_error_becomes_a_failure_toast_with_the_reference()
    {
        var (hub, toast) = Build();
        var reference = FailureReferenceFormat.New();

        hub.Publish(new OgDiagnosticEvent(reference, FailureSeverity.Error, "loading a page"));

        var shown = Assert.Single(toast.History);
        Assert.Equal(OgToastKind.Failure, shown.Kind);
        Assert.Equal(reference, shown.Reference);
        Assert.Equal(TimeSpan.FromSeconds(10), shown.Duration);
    }

    [Fact]
    public void A_critical_failure_is_sticky()
    {
        var (hub, toast) = Build();

        hub.Publish(new OgDiagnosticEvent(
            FailureReferenceFormat.New(),
            FailureSeverity.Critical,
            "starting"));

        var shown = Assert.Single(toast.History);
        Assert.Null(shown.Duration);
    }

    [Fact]
    public void A_degraded_result_becomes_a_warning()
    {
        var (hub, toast) = Build();

        hub.Publish(new OgDiagnosticEvent(
            FailureReferenceFormat.New(),
            FailureSeverity.Degraded,
            "saving a preference"));

        var shown = Assert.Single(toast.History);
        Assert.Equal(OgToastKind.Warning, shown.Kind);
    }

    [Fact]
    public void An_audit_only_event_does_not_toast()
    {
        var (hub, toast) = Build();

        hub.Publish(new OgDiagnosticEvent(null, FailureSeverity.Degraded, "audit write"));

        Assert.Empty(toast.History);
    }

    [Fact]
    public void The_hub_raises_the_same_toast_it_showed()
    {
        var (hub, toast) = Build();
        OgToast? raised = null;
        hub.Raised += (_, item) => raised = item;

        hub.Publish(new OgDiagnosticEvent(
            FailureReferenceFormat.New(),
            FailureSeverity.Error,
            "loading"));

        Assert.NotNull(raised);
        Assert.Single(toast.History);
        Assert.Equal(toast.History[0], raised);
    }

    [Fact]
    public void A_toast_text_never_carries_the_exception_message()
    {
        var (hub, toast) = Build();
        var reference = FailureReferenceFormat.New();

        hub.Publish(new OgDiagnosticEvent(reference, FailureSeverity.Error, "loading a page"));

        var shown = Assert.Single(toast.History);
        Assert.Equal("Fail.Body", shown.MessageKey);
        Assert.DoesNotContain("Exception", shown.MessageKey, StringComparison.Ordinal);
    }
}
