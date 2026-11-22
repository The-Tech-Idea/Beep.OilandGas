using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// The composition root builds with <c>ValidateOnBuild</c>, so an unregistered dependency is a startup
/// failure rather than a runtime surprise. This test drives that path directly: it is what caught
/// <c>OgSignInService</c> asking for an <c>IOgAuditService</c> that was never registered.
/// </summary>
public sealed class CompositionTests
{
    [Fact]
    public void The_composition_root_builds_and_every_placeholder_is_resolvable()
    {
        using var application = new OgBootstrapper().Create();

        Assert.NotNull(application.Session);
        Assert.NotNull(application.Permissions);
        Assert.NotNull(application.Diagnostics);
        Assert.NotNull(application.Notifications);
        Assert.NotNull(application.Navigation);
        Assert.NotNull(application.Loc);
        Assert.NotNull(application.Failures);
        Assert.NotNull(application.Audit);
    }

    [Fact]
    public void The_reporter_and_the_audit_contract_are_the_same_pipeline()
    {
        using var application = new OgBootstrapper().Create();

        var pipeline = application.Diagnostics;

        // A save or a sign-in must write its audit row to the same store the failures go to; two
        // instances behind the two interfaces would silently split the evidence.
        Assert.Same(pipeline, application.Services.GetRequiredService<IFailureReporter>());
        Assert.Same(pipeline, application.Services.GetRequiredService<IOgAuditService>());
    }

    [Fact]
    public void The_wellbore_lifetime_is_lazy_so_startup_does_not_pay_for_it()
    {
        using var application = new OgBootstrapper().Create();

        var lifetime = application.Services
            .GetRequiredService<TheTechIdea.Beep.OilandGas.Desktop.Services.OgWellboreLifetime>();

        // Merely composing the application must not have registered toolboxes or read an environment —
        // that is the first visual screen's cost, and VSB-02 depends on it happening before any read.
        Assert.False(lifetime.IsCreated);
    }
}
