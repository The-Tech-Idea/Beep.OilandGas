using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Views;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// REL-03: WCAG contrast arithmetic and the shipped palette, the command palette's filter and movement, the
/// accessible-name audit, the chart's text alternative, and the global key map.
/// </summary>
public sealed class AccessibilityTests
{
    [Fact]
    public void Contrast_arithmetic_matches_the_wcag_reference_values()
    {
        // The two anchors every implementation must reproduce.
        Assert.Equal(21.0, OgContrast.Ratio("#000000", "#FFFFFF"), 3);
        Assert.Equal(1.0, OgContrast.Ratio("#123456", "#123456"), 3);

        // Symmetry: order must not matter.
        Assert.Equal(
            OgContrast.Ratio("#16202B", "#FFFFFF"),
            OgContrast.Ratio("#FFFFFF", "#16202B"),
            6);

        // A known mid-grey pair (WCAG worked example): #777 on white is 4.48:1, just under AA body text.
        var midGrey = OgContrast.Check("#777777", "#FFFFFF");
        Assert.InRange(midGrey.Ratio, 4.4, 4.5);
        Assert.False(midGrey.Passes);
    }

    [Fact]
    public void A_colour_parses_with_or_without_alpha_and_reports_bad_input()
    {
        Assert.Equal(new OgRgb(0x16, 0x20, 0x2B), OgRgb.Parse("#16202B"));
        Assert.Equal(new OgRgb(0x16, 0x20, 0x2B), OgRgb.Parse("16202b"));
        Assert.Equal(new OgRgb(0x16, 0x20, 0x2B), OgRgb.Parse("#FF16202B"));
        Assert.Equal("#16202B", OgRgb.Parse("#16202B").ToString());

        Assert.Throws<FormatException>(() => OgRgb.Parse("#12345"));
        Assert.Throws<FormatException>(() => OgRgb.Parse("not a colour"));
    }

    [Fact]
    public void The_shipped_palette_passes_aa_on_every_pair_the_theme_uses()
    {
        var report = OgThemeContrast.Audit("tokens", OgThemeContrast.Default);

        Assert.True(report.Passes, Describe(report));
        Assert.Equal(0, report.Failures);
        Assert.Contains("AA pass", report.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_palette_that_fails_is_reported_with_the_pair_that_fails()
    {
        // Light grey body text on white is exactly the mistake an audit exists to catch.
        var bad = OgThemeContrast.Default with
        {
            TextSecondary = "#DDDDDD",
        };

        var report = OgThemeContrast.Audit("bad", bad);

        Assert.False(report.Passes);
        Assert.True(report.Failures > 0);

        var failing = report.Pairs.Where(pair => !pair.Passes).ToList();
        Assert.All(failing, pair => Assert.Contains("#DDDDDD", pair.Foreground, StringComparison.OrdinalIgnoreCase));
        Assert.Contains("FAIL", failing[0].Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_skin_audit_fails_a_low_contrast_palette_and_passes_a_good_one()
    {
        // REL-03 AC-3: the audit reads a skin's own brushes. This palette is the shape the library ships in
        // several skins: a bright accent on a near-white surface.
        var poor = new OgThemeContrast.OgSkinColours(
            Surface: "#FFFBFE",
            SurfaceVariant: "#F5F0F7",
            OnSurface: "#1C1B1F",
            OnSurfaceVariant: "#49454F",
            Accent: "#F9AB00",
            AccentForeground: "#FFFFFF",
            Danger: "#B3261E",
            Success: "#1E7A45",
            Warning: "#B26A00");

        var poorReport = OgThemeContrast.AuditSkin("poor", poor);

        Assert.False(poorReport.Passes);
        Assert.Contains(poorReport.Pairs, pair => !pair.Passes && pair.Foreground == "#F9AB00");

        var good = poor with { Accent = "#7A4F00" };

        Assert.True(OgThemeContrast.AuditSkin("good", good).Passes);
    }

    [Fact]
    public void A_colour_is_moved_only_as_far_as_the_threshold_needs()
    {
        // REL-03 AC-3: the amber the control library ships cannot carry text on a near-white surface. The guard keeps the hue and darkens it
        // until the ratio is reached - not to black, which would be a redesign.
        var adjusted = OgContrast.Ensure("#F9AB00", "#FFFBFE", OgContrast.AaUi, out var changed);

        Assert.True(changed);
        Assert.True(OgContrast.Ratio(adjusted, "#FFFBFE") >= OgContrast.AaUi,
            adjusted + " is still below the threshold");
        Assert.True(OgContrast.Ratio(adjusted, "#FFFBFE") <= OgContrast.AaUi + 0.35,
            adjusted + " was darkened further than it needed to be");

        // The hue survives: still amber, still ordered red over green over blue.
        var rgb = OgRgb.Parse(adjusted);

        Assert.True(rgb.R > rgb.G && rgb.G > rgb.B, adjusted + " is no longer amber");

        // One that already passes is returned untouched, which is what keeps a compliant skin exactly as designed.
        var passing = OgContrast.Ensure("#0B6E4F", "#FFFFFF", OgContrast.AaUi, out var untouched);

        Assert.False(untouched);
        Assert.Equal("#0B6E4F", passing, StringComparer.OrdinalIgnoreCase);

        // A dark background moves the foreground the other way.
        var onBlack = OgContrast.Ensure("#111111", "#000000", OgContrast.AaText, out var lightened);

        Assert.True(lightened);
        Assert.True(OgContrast.Ratio(onBlack, "#000000") >= OgContrast.AaText);
        Assert.True(OgRgb.Parse(onBlack).R > 0x11);
    }

    [Fact]
    public void The_contrast_guard_closes_every_pair_the_audit_fails_and_says_what_it_moved()
    {
        // The palette the existing audit test shows failing: a bright accent on a near-white surface.
        var poor = new OgThemeContrast.OgSkinColours(
            Surface: "#FFFBFE",
            SurfaceVariant: "#F5F0F7",
            OnSurface: "#1C1B1F",
            OnSurfaceVariant: "#49454F",
            Accent: "#F9AB00",
            AccentForeground: "#FFFFFF",
            Danger: "#B3261E",
            Success: "#1E7A45",
            Warning: "#B26A00");

        Assert.False(OgThemeContrast.AuditSkin("poor", poor).Passes);

        var (adjusted, changes) = OgThemeContrast.Enforce(poor);

        // Every audited pair now passes, and the report says so rather than the guard claiming it.
        Assert.True(OgThemeContrast.AuditSkin("adjusted", adjusted).Passes,
            string.Join(" | ", OgThemeContrast.AuditSkin("adjusted", adjusted).Pairs
                .Where(pair => !pair.Passes).Select(pair => pair.Describe())));

        // It moved the accent and left the rest alone, because only the accent failed.
        Assert.NotEmpty(changes);
        Assert.All(changes, change => Assert.Contains("needs", change, StringComparison.Ordinal));
        Assert.Contains(changes, change => change.StartsWith("#F9AB00 -> ", StringComparison.Ordinal));
        Assert.Equal("#1C1B1F", adjusted.OnSurface, StringComparer.OrdinalIgnoreCase);
        Assert.Equal("#B3261E", adjusted.Danger, StringComparer.OrdinalIgnoreCase);
        Assert.Equal("#1E7A45", adjusted.Success, StringComparer.OrdinalIgnoreCase);
        Assert.Equal("#B26A00", adjusted.Warning, StringComparer.OrdinalIgnoreCase);

        // A palette that already passes is returned byte-identical with nothing to report, so the guard cannot quietly restyle a skin.
        var good = poor with { Accent = "#7A4F00" };

        Assert.True(OgThemeContrast.AuditSkin("good", good).Passes);

        var (untouched, none) = OgThemeContrast.Enforce(good);

        Assert.Empty(none);
        Assert.Equal(good, untouched);

        // Accent text on the accent is a pair: white on amber cannot be lightened any further, so the accent is darkened under the label -
        // which is what a designer does, and is why the pair is the unit rather than the foreground alone.
        var whiteOnAmber = poor with { Accent = "#F9AB00", AccentForeground = "#FFFFFF" };
        var (fixedLabel, labelChanges) = OgThemeContrast.Enforce(whiteOnAmber);

        Assert.Equal("#FFFFFF", fixedLabel.AccentForeground, StringComparer.OrdinalIgnoreCase);
        Assert.True(OgContrast.Ratio(fixedLabel.AccentForeground, fixedLabel.Accent) >= OgContrast.AaText,
            fixedLabel.AccentForeground + " on " + fixedLabel.Accent + " still fails");
        Assert.True(OgRgb.Parse(fixedLabel.Accent).R < 0xF9, "the accent was not darkened under the label");
        Assert.True(OgThemeContrast.AuditSkin("label", fixedLabel).Passes);
        Assert.Contains(labelChanges, change => change.StartsWith("#F9AB00 -> ", StringComparison.Ordinal));
    }

    [Fact]
    public void The_palette_filter_is_case_insensitive_and_requires_every_term()
    {
        var entries = new List<OgPaletteEntry>
        {
            new("charts", "Charts", "reporting"),
            new("logs", "Well logs", "wells"),
            new("licence", "Licence", "administration"),
        };

        Assert.Equal(3, OgCommandPalette.Filter(entries, null).Count);
        Assert.Equal(3, OgCommandPalette.Filter(entries, "   ").Count);

        var chart = Assert.Single(OgCommandPalette.Filter(entries, "CHART"));
        Assert.Equal("charts", chart.Route);

        // Every term must match, in any of the three fields.
        var combined = Assert.Single(OgCommandPalette.Filter(entries, "reporting charts"));
        Assert.Equal("charts", combined.Route);

        Assert.Empty(OgCommandPalette.Filter(entries, "charts licence"));
        Assert.Empty(OgCommandPalette.Filter(entries, "nothing-like-this"));
    }

    [Fact]
    public void Palette_movement_wraps_at_both_ends_and_selection_is_safe()
    {
        Assert.Equal(0, OgCommandPalette.Move(0, 1, 1));
        Assert.Equal(1, OgCommandPalette.Move(0, 1, 3));
        Assert.Equal(0, OgCommandPalette.Move(2, 1, 3));
        Assert.Equal(2, OgCommandPalette.Move(0, -1, 3));
        Assert.Equal(0, OgCommandPalette.Move(5, 0, 0));

        var entries = new List<OgPaletteEntry> { new("a", "A", "g"), new("b", "B", "g") };

        Assert.Equal("a", OgCommandPalette.Selected(entries, -5)!.Route);
        Assert.Equal("b", OgCommandPalette.Selected(entries, 99)!.Route);
        Assert.Null(OgCommandPalette.Selected([], 0));
    }

    [Fact]
    public void The_palette_lists_every_catalog_page_with_its_title()
    {
        var entries = OgCommandPalette.FromCatalog(route => route.ToUpperInvariant());

        Assert.Equal(OgModuleCatalog.Pages.Count, entries.Count);
        Assert.Contains(entries, entry => entry.Route == "logs" && entry.Title == "LOGS");
        Assert.Contains(entries, entry => entry.Route == "licence");
    }

    [Fact]
    public void An_unnamed_input_is_reported_and_a_named_one_is_not()
    {
        RunOnSta(() =>
        {
            var unnamed = new TextBox();
            var named = new TextBox();
            AutomationProperties.SetName(named, "Licence file path");

            Assert.Contains(
                OgAccessibilityAudit.UnnamedInteractive(Wrap(unnamed)),
                problem => problem.Contains("TextBox", StringComparison.Ordinal));

            Assert.Empty(OgAccessibilityAudit.UnnamedInteractive(Wrap(named)));

            // A LabeledBy reference is a name too.
            var label = new TextBlock { Text = "Search" };
            var labelled = new ComboBox();
            AutomationProperties.SetLabeledBy(labelled, label);
            Assert.Empty(OgAccessibilityAudit.UnnamedInteractive(Wrap(labelled)));
        });
    }

    [Fact]
    public void A_button_is_named_by_its_text_and_an_icon_only_button_is_not()
    {
        RunOnSta(() =>
        {
            var withText = new Button { Content = "Export" };
            var withoutText = new Button();

            Assert.Empty(OgAccessibilityAudit.UnnamedInteractive(Wrap(withText)));
            Assert.Contains(
                OgAccessibilityAudit.UnnamedInteractive(Wrap(withoutText)),
                problem => problem.Contains("Button", StringComparison.Ordinal));
        });
    }

    [Fact]
    public void A_focusable_control_outside_the_tab_order_is_reported()
    {
        RunOnSta(() =>
        {
            var reachable = new TextBox { Focusable = true, IsTabStop = true };
            var unreachable = new TextBox { Focusable = true, IsTabStop = false };

            var skipped = OgAccessibilityAudit.NotInTabOrder(Wrap(reachable));
            Assert.Empty(skipped);

            skipped = OgAccessibilityAudit.NotInTabOrder(Wrap(unreachable));
            Assert.Contains("TextBox", skipped);
        });
    }

    [Fact]
    public void The_canvas_host_is_a_tab_stop_with_a_theme_focus_ring()
    {
        RunOnSta(() =>
        {
            using var host = new TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting.SkiaViewSurfaceHost();

            // AC-1: reachable by Tab.
            Assert.True(host.Focusable);
            Assert.True(host.IsTabStop);

            // AC-2: a visible ring, drawn as a border on the host (around the image, not over it).
            Assert.Equal(new Thickness(2), host.BorderThickness);
            Assert.NotNull(host.Style);

            var trigger = Assert.Single(host.Style!.Triggers.OfType<Trigger>());
            Assert.Equal(UIElement.IsKeyboardFocusedProperty, trigger.Property);

            Assert.Contains(
                trigger.Setters.OfType<Setter>(),
                setter => setter.Property == Control.BorderBrushProperty);
        });
    }

    [Fact]
    public void Every_page_constructed_by_the_factory_names_its_interactive_controls()
    {
        // The smoke run asserts the same count; this keeps the regression in the suite even when a page is
        // added later, because it walks the catalog rather than a hand-written list.
        RunOnSta(() =>
        {
            using var application = new OgBootstrapper().Create();
            var problems = new List<string>();
            var untabbable = new List<string>();
            var audited = 0;

            foreach (var page in OgModuleCatalog.Pages)
            {
                if (OgPageFactory.Create(application, page.Route) is DependencyObject view)
                {
                    audited++;
                    problems.AddRange(
                        OgAccessibilityAudit.UnnamedInteractive(view)
                            .Select(problem => $"{page.Route}: {problem}"));

                    untabbable.AddRange(
                        OgAccessibilityAudit.NotInTabOrder(view)
                            .Select(problem => $"{page.Route}: {problem}"));
                }
            }

            // Guard against a vacuous pass: every declared page must actually have been built and walked.
            Assert.Equal(OgModuleCatalog.Pages.Count, audited);

            Assert.True(
                problems.Count == 0,
                $"{problems.Count} unnamed control(s): {string.Join("; ", problems.Take(10))}");

            Assert.True(
                untabbable.Count == 0,
                $"{untabbable.Count} focusable control(s) outside the tab order: {string.Join("; ", untabbable.Take(10))}");
        });
    }

    [Fact]
    public void A_page_reflows_at_two_hundred_percent_without_horizontal_overflow()
    {
        // REL-03 AC-6: 200 % scaling halves the logical width. A 1240 px window therefore behaves like 620 DIP,
        // so laying a page out at 620 DIP is the same question with a deterministic answer.
        RunOnSta(() =>
        {
            using var application = new OgBootstrapper().Create();
            var overflow = new List<string>();

            // AC-6 is about every page, so the audit walks the catalog rather than a hand-picked sample.
            foreach (var route in OgModuleCatalog.Pages.Select(page => page.Route))
            {
                var page = OgPageFactory.Create(application, route) as FrameworkElement;

                Assert.NotNull(page);

                var host = new Border { Width = 620, Height = 800, Child = page };
                host.Measure(new Size(620, 800));
                host.Arrange(new Rect(0, 0, 620, 800));
                page.ApplyTemplate();
                host.UpdateLayout();

                var scroller = FindChild<ScrollViewer>(page) ?? FindLogical<ScrollViewer>(page);

                Assert.NotNull(scroller);

                // The page is hosted headlessly, so the scroll container is measured at the target width
                // directly. It is the element whose overflow is the question.
                scroller!.Measure(new Size(620, 760));
                scroller.Arrange(new Rect(0, 0, 620, 760));
                scroller.UpdateLayout();
                Assert.Equal(ScrollBarVisibility.Disabled, scroller.HorizontalScrollBarVisibility);

                // Guard against a vacuous pass: the page must have been laid out inside the narrow viewport.
                Assert.InRange(scroller.ViewportWidth, 400, 620);
                Assert.True(scroller.ExtentWidth > 0, $"{route} produced no laid-out content.");

                if (scroller.ScrollableWidth > 1)
                {
                    overflow.Add($"{route}: {scroller.ScrollableWidth:0.#} DIP of horizontal overflow");
                }
            }

            Assert.True(
                overflow.Count == 0,
                $"Primary content must reflow, not scroll sideways: {string.Join("; ", overflow)}");
        });
    }

    [Fact]
    public void The_shortcut_reference_lists_the_global_keys()
    {
        var keys = ShortcutsWindow.All.Select(entry => entry.Keys).ToList();

        Assert.Contains("Ctrl+K", keys);
        Assert.Contains("Ctrl+Tab", keys);
        Assert.Contains("Alt+Left", keys);
        Assert.Contains("F6", keys);
        Assert.Contains("F1", keys);
        Assert.Contains("Esc", keys);
        Assert.All(ShortcutsWindow.All, entry => Assert.False(string.IsNullOrWhiteSpace(entry.What)));
    }

    [Fact]
    public void The_shell_binds_the_documented_global_keys()
    {
        // A source guard: the shortcut list is a promise, and this is where the promise is kept. Without it the
        // reference could list a key nobody handles.
        var shell = File.ReadAllText(Path.Combine(
            SourceRoot.Locate(),
            "src",
            "Beep.OilandGas.Desktop",
            "Shell",
            "OgShellWindow.xaml.cs"));

        Assert.Contains("Key.K", shell, StringComparison.Ordinal);
        Assert.Contains("ModifierKeys.Control", shell, StringComparison.Ordinal);
        Assert.Contains("Key.F6", shell, StringComparison.Ordinal);
        Assert.Contains("Key.F1", shell, StringComparison.Ordinal);
        Assert.Contains("Key.Left", shell, StringComparison.Ordinal);
        Assert.Contains("ModifierKeys.Alt", shell, StringComparison.Ordinal);
        Assert.Contains("OpenPalette", shell, StringComparison.Ordinal);
    }

    private static T? FindLogical<T>(DependencyObject root) where T : DependencyObject
    {
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
        {
            if (child is T match)
            {
                return match;
            }

            if (FindLogical<T>(child) is { } nested)
            {
                return nested;
            }
        }

        return null;
    }

    private static T? FindChild<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);

            if (child is T match)
            {
                return match;
            }

            if (FindChild<T>(child) is { } nested)
            {
                return nested;
            }
        }

        return null;
    }

    /// <summary>The audit walks descendants, so a single control is inspected from inside a container.</summary>
    private static StackPanel Wrap(UIElement child)
    {
        var panel = new StackPanel();
        panel.Children.Add(child);
        return panel;
    }

    private static string Describe(OgContrastReport report) =>
        string.Join(" | ", report.Pairs.Where(pair => !pair.Passes).Select(pair => pair.Describe()));

    private static void RunOnSta(Action action)
    {
        Exception? failure = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            throw failure;
        }
    }
}
