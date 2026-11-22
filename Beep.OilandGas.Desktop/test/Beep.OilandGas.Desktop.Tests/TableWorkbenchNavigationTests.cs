using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-03 AC-3 and DAT-04's page contract: the tree records the table and filter, the shell is asked for the
/// workbench, the request survives the page that recorded it, and a second visit does not silently reuse it.
/// </summary>
public sealed class TableWorkbenchNavigationTests
{
    [Fact]
    public void The_navigator_records_the_route_and_reports_when_no_shell_is_installed()
    {
        var navigator = new OgPageNavigator();

        // Headless: no handler, so the caller must be told the shell did not take it.
        Assert.False(navigator.Navigate("tableworkbench"));
        Assert.Equal(["tableworkbench"], navigator.Requested);

        var shown = new List<string>();
        navigator.Handler = shown.Add;

        Assert.True(navigator.Navigate("tableworkbench"));
        Assert.Equal(["tableworkbench"], shown);
    }

    [Fact]
    public void A_navigation_with_no_route_is_refused()
    {
        var navigator = new OgPageNavigator();

        Assert.Throws<ArgumentException>(() => navigator.Navigate(" "));
        Assert.Empty(navigator.Requested);
    }

    [Fact]
    public void The_pending_request_carries_the_table_and_the_filter_and_is_taken_once()
    {
        var navigation = new OgWorkbenchNavigation();

        navigation.Pending = new OgTableOpenRequest("T_WELL", "WELL_NAME ALPHA");

        var taken = navigation.Take();

        Assert.NotNull(taken);
        Assert.Equal("T_WELL", taken!.TableName);
        Assert.Equal("WELL_NAME ALPHA", taken.Filter);

        // DAT-04 must not open the previous table on a second visit.
        Assert.Null(navigation.Take());
    }

    [Fact]
    public void Both_data_pages_are_declared_and_reachable_by_the_factory_contract()
    {
        var tree = OgModuleCatalog.Find("ppdmtree");
        var workbench = OgModuleCatalog.Find("tableworkbench");

        Assert.NotNull(tree);
        Assert.NotNull(workbench);
        Assert.Equal(OgModuleKeys.Data, workbench!.ModuleKey);
        Assert.Equal("tableworkbench", TheTechIdea.Beep.OilandGas.Desktop.Views.TableWorkbenchPageView.Route);
        Assert.Equal("ppdmtree", TheTechIdea.Beep.OilandGas.Desktop.Views.PpdmTreePageView.Route);
        Assert.Equal(workbench.Route, TheTechIdea.Beep.OilandGas.Desktop.Views.PpdmTreePageView.WorkbenchRoute);
    }

    [Fact]
    public void The_application_exposes_one_navigator_one_workbench_context_and_one_gateway()
    {
        using var application = new OgBootstrapper().Create();

        Assert.Same(application.Pages, application.Pages);
        Assert.Same(application.Workbench, application.Workbench);
        Assert.NotNull(application.TableGateway);
    }
}
