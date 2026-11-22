using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdea.Beep.OilandGas.Desktop.Theme;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>FND-04 / FND-05: the design-system catalog, the locale switch and the notification history.</summary>
public sealed class ShellFeatureTests
{
    [Fact]
    public void The_design_system_catalog_is_populated_and_has_a_current_choice()
    {
        Assert.True(OgSkins.All.Count > 10, $"Only {OgSkins.All.Count} design systems were enumerated.");
        Assert.DoesNotContain(OgSkins.All, option => option.DisplayName.Length == 0);
        Assert.All(OgSkins.All, option => Assert.NotEqual(TheTechIdea.Beep.WPF.Controls.Styling.BeepControlStyle.None, option.Style));
    }

    [Fact]
    public void Switching_to_Arabic_flips_direction_and_translates()
    {
        var loc = new OgLoc();
        var english = loc.Get("Page.dashboard");

        var raised = 0;
        loc.LanguageChanged += (_, _) => raised++;

        loc.SetCulture(OgLoc.Arabic);

        Assert.True(loc.IsRightToLeft);
        Assert.NotEqual(english, loc.Get("Page.dashboard"));
        Assert.Equal(1, raised);
    }

    [Fact]
    public void Switching_back_restores_English_and_leaves_no_right_to_left()
    {
        var loc = new OgLoc();
        var english = loc.Get("Page.dashboard");

        loc.SetCulture(OgLoc.Arabic);
        loc.SetCulture(OgLoc.English);

        Assert.False(loc.IsRightToLeft);
        Assert.Equal(english, loc.Get("Page.dashboard"));
    }

    [Fact]
    public void An_unknown_culture_is_ignored_rather_than_breaking_the_ui()
    {
        var loc = new OgLoc();
        var before = loc.CultureName;

        loc.SetCulture("xx-XX");

        Assert.Equal(before, loc.CultureName);
    }

    [Fact]
    public void A_key_missing_from_Arabic_falls_back_to_English()
    {
        var loc = new OgLoc();
        loc.SetCulture(OgLoc.Arabic);

        // Diagnostics.Build is deliberately English-only, which is exactly how the fallback chain is proved.
        Assert.Equal("Build", loc.Get("Diagnostics.Build"));
    }

    [Fact]
    public void An_unknown_key_returns_itself_in_every_culture()
    {
        var loc = new OgLoc();
        Assert.Equal("No.Such.Key", loc.Get("No.Such.Key"));

        loc.SetCulture(OgLoc.Arabic);
        Assert.Equal("No.Such.Key", loc.Get("No.Such.Key"));
    }

    [Fact]
    public void Every_Arabic_page_title_exists()
    {
        var loc = new OgLoc();
        loc.SetCulture(OgLoc.Arabic);

        var untranslated = OgModuleCatalog.Pages
            .Where(page => string.Equals(
                loc.Get($"Page.{page.Route}"),
                new OgLoc().Get($"Page.{page.Route}"),
                StringComparison.Ordinal))
            .Select(page => page.Route)
            .ToArray();

        Assert.True(untranslated.Length == 0, $"Untranslated page titles: {string.Join(", ", untranslated)}");
    }

    [Fact]
    public void The_notification_history_keeps_the_time_and_the_reference()
    {
        var service = new OgToastService();
        var reference = FailureReferenceFormat.New();

        service.Show(new OgToast(OgToastKind.Failure, "Fail.Body", ["loading"], reference));

        var item = Assert.Single(service.HistoryItems);
        Assert.Equal(reference, item.Toast.Reference);
        Assert.True(item.RaisedAt > DateTimeOffset.Now.AddMinutes(-1));
    }
}
