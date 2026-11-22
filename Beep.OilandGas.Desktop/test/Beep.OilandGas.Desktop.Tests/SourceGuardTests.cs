using System.Text.RegularExpressions;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// PLAN-07 §6 guards. These read the source, so they fail the build the moment a rule is broken in a
/// new file — before a reviewer has to find it.
/// </summary>
public sealed class SourceGuardTests
{
    /// <summary>Files allowed to name the toast service: its contract, its implementation, the hub and the composition root.</summary>
    private static readonly string[] ToastServiceAllowList =
    [
        "IOgToastService.cs",
        "OgToastService.cs",
        "OgNotificationHub.cs",
        "OgBootstrapper.cs",
    ];

    [Fact]
    public void The_toast_service_is_named_only_by_the_hub_and_its_owner()
    {
        var offenders = SourceRoot.AllSources()
            .Where(file => File.ReadAllText(file).Contains("IOgToastService", StringComparison.Ordinal))
            .Where(file => !ToastServiceAllowList.Contains(Path.GetFileName(file)))
            .Select(Path.GetFileName)
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            $"Only the notification hub may use IOgToastService. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void MessageBox_is_used_only_by_the_approved_startup_dialog()
    {
        var offenders = SourceRoot.AllSources()
            .Where(file => File.ReadAllText(file).Contains("MessageBox.Show", StringComparison.Ordinal))
            .Where(file => !string.Equals(Path.GetFileName(file), "App.xaml.cs", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            $"MessageBox.Show is allowed only in App.xaml.cs. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void The_desktop_data_layer_contains_no_sql_statement_text()
    {
        var separator = Path.DirectorySeparatorChar;
        var dataAssembly = $"{separator}Beep.OilandGas.Desktop.Data{separator}";
        // A statement, not a sentence that happens to start with "Select". Each arm needs the keyword's
        // companion (a FROM, an INTO, a SET) so the UI copy stays clear of the guard.
        var statement = new Regex(
            @"""\s*(SELECT\s+[^""]+?\s+FROM\s|INSERT\s+INTO\s|UPDATE\s+[A-Za-z_]+\s+SET\s|DELETE\s+FROM\s" +
            @"|CREATE\s+TABLE\s|DROP\s+TABLE\s|ALTER\s+TABLE\s)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        var offenders = SourceRoot.AllSources()
            .Where(file => file.Contains(dataAssembly, StringComparison.OrdinalIgnoreCase))
            .Where(file =>
            {
                var text = File.ReadAllText(file);

                return text.Contains("RunQuery(", StringComparison.Ordinal)
                       || text.Contains("ExecuteSql(", StringComparison.Ordinal)
                       || text.Contains("GetScalar(", StringComparison.Ordinal)
                       || statement.IsMatch(text);
            })
            .Select(file => Path.GetFileName(file))
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            "Every domain read and write must go through a UnitofWork or the datasource's entity API; no " +
            $"statement text belongs in the desktop data layer. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void No_catch_is_left_without_reporting_or_rethrowing()
    {
        var offenders = new List<string>();

        foreach (var file in SourceRoot.AllSources())
        {
            var text = File.ReadAllText(file);
            if (!text.Contains("catch", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var body in CatchBodies(text))
            {
                var reports = body.Contains("ReportHandled", StringComparison.Ordinal)
                              || body.Contains("LastResortLog", StringComparison.Ordinal)
                              || body.Contains("throw", StringComparison.Ordinal);

                if (!reports)
                {
                    offenders.Add($"{Path.GetFileName(file)}: {Squash(body)}");
                }
            }
        }

        Assert.True(
            offenders.Count == 0,
            "Every catch must report through IFailureReporter (or LastResortLog) or rethrow. Offenders:\n"
            + string.Join("\n", offenders));
    }

    [Fact]
    public void No_private_key_material_is_committed_under_src()
    {
        // REL-02: the application ships the public half of the vendor key as an embedded resource. A private
        // key in src would let anyone mint a licence, so its presence is a build failure, not a code review
        // note. The development private key lives only in the test project's fixtures.
        var root = Path.Combine(SourceRoot.Locate(), "src");

        var offenders = Directory
            .GetFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                           && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                           || path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                           || path.EndsWith(".pem", StringComparison.OrdinalIgnoreCase)
                           || path.EndsWith(".config", StringComparison.OrdinalIgnoreCase)
                           || path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            .Where(path => File.ReadAllText(path).Contains("PRIVATE KEY", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            $"Private key material must never ship. Offenders: {string.Join(", ", offenders)}");
    }
    [Fact]
    public void No_type_outside_the_data_project_references_IDMEEditor()
    {
        // FND-02 AC-4: BeepDM's editor stays behind IOgConnectionCatalog / IOgServiceAccess. A view model
        // that took IDMEEditor could open a database directly and bypass every gate.
        var offenders = SourceRoot.AllSources()
            .Where(file => !file.Contains(
                $"{Path.DirectorySeparatorChar}Beep.OilandGas.Desktop.Data{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal))
            .Where(file => File.ReadAllText(file).Contains("IDMEEditor", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            $"Only Beep.OilandGas.Desktop.Data may use IDMEEditor. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void The_shell_is_created_only_after_the_startup_gate()
    {
        var app = File.ReadAllText(
            Path.Combine(SourceRoot.Locate(), "src", "Beep.OilandGas.Desktop", "App.xaml.cs"));

        var gate = app.IndexOf("OgStartupGate.Decide", StringComparison.Ordinal);
        var shell = app.IndexOf("new OgShellWindow", StringComparison.Ordinal);

        // FND-07 AC-1: an installation that has not been set up or signed in must never reach the shell.
        Assert.True(gate >= 0, "App.xaml.cs does not consult OgStartupGate.");
        Assert.True(shell > gate, "The shell is created before the setup/sign-in gate decides.");
        Assert.Contains("FirstRunWindow", app, StringComparison.Ordinal);
        Assert.Contains("SignInWindow", app, StringComparison.Ordinal);
    }

    [Fact]
    public void Every_catalog_route_has_a_view_factory_case()
    {
        var factory = File.ReadAllText(
            Path.Combine(SourceRoot.Locate(), "src", "Beep.OilandGas.Desktop", "Views", "OgPageFactory.cs"));

        var missing = OgModuleCatalog.Pages
            .Where(page => !factory.Contains($"\"{page.Route}\"", StringComparison.Ordinal))
            .Select(page => page.Route)
            .ToArray();

        Assert.True(missing.Length == 0, $"Routes with no factory case: {string.Join(", ", missing)}");
    }

    [Fact]
    public void Catalog_routes_are_unique()
    {
        var duplicates = OgModuleCatalog.Pages
            .GroupBy(page => page.Route, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        Assert.True(duplicates.Length == 0, $"Duplicate routes: {string.Join(", ", duplicates)}");
    }

    [Fact]
    public void Every_catalog_page_declares_a_known_module_key()
    {
        var unknown = OgModuleCatalog.Pages
            .Where(page => !OgModuleKeys.All.Contains(page.ModuleKey, StringComparer.OrdinalIgnoreCase))
            .Select(page => $"{page.Route} → {page.ModuleKey}")
            .ToArray();

        Assert.True(unknown.Length == 0, $"Unknown module keys: {string.Join(", ", unknown)}");
    }

    /// <summary>
    /// Extracts the brace body of every <c>catch</c> in a file. A heuristic, deliberately: it catches the
    /// real mistake (a catch that silently continues) without needing a full parse, and it is documented
    /// here so its blind spot is known — it does not verify that the report names an operation or that the
    /// caller can tell success from failure.
    /// </summary>
    private static IEnumerable<string> CatchBodies(string text)
    {
        // Line comments are removed first: prose such as "this catches a broken resource" is not a catch.
        var withoutComments = Regex.Replace(text, @"//[^\r\n]*", string.Empty);
        var matches = Regex.Matches(withoutComments, @"\bcatch\b");

        foreach (Match match in matches)
        {
            var cursor = match.Index + match.Length;

            while (cursor < withoutComments.Length && char.IsWhiteSpace(withoutComments[cursor]))
            {
                cursor++;
            }

            if (cursor < withoutComments.Length && withoutComments[cursor] == '(')
            {
                var depth = 0;
                while (cursor < withoutComments.Length)
                {
                    if (withoutComments[cursor] == '(')
                    {
                        depth++;
                    }
                    else if (withoutComments[cursor] == ')')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            cursor++;
                            break;
                        }
                    }

                    cursor++;
                }
            }

            while (cursor < withoutComments.Length && withoutComments[cursor] != '{')
            {
                cursor++;
            }

            if (cursor >= withoutComments.Length)
            {
                yield break;
            }

            var start = cursor;
            var braces = 0;
            while (cursor < withoutComments.Length)
            {
                if (withoutComments[cursor] == '{')
                {
                    braces++;
                }
                else if (withoutComments[cursor] == '}')
                {
                    braces--;
                    if (braces == 0)
                    {
                        cursor++;
                        break;
                    }
                }

                cursor++;
            }

            yield return withoutComments[start..cursor];
        }
    }

    [Fact]
    public void Company_settings_are_one_database_table_and_only_the_system_administrator_prepares_them()
    {
        // The settings a company shares are rows in BEEPSETTINGS, and the first administrator's setup run creates
        // them — the company record, the module set, the RBAC schema and the environment. A later user reads them;
        // only the SystemAdmin changes them.
        Assert.Equal("BEEPSETTINGS", Data.Settings.OgSharedSetup.TableName);

        var root = SourceRoot.Locate();

        var settings = File.ReadAllText(
            Path.Combine(root, "src", "Beep.OilandGas.Desktop", "Views", "SettingsView.cs"));

        // The gate is a role OR a permission, because the database may grant Admin.ConfigureSystem to another role.
        Assert.Contains("private bool CanEditCompany => _application.Permissions.CanConfigureSystem;", settings, StringComparison.Ordinal);

        var composition = File.ReadAllText(
            Path.Combine(root, "src", "Beep.OilandGas.Desktop.Data", "Setup", "OgSetupComposition.cs"));

        // SECURITY must be discovered and its seed service registered, or a company database never gets the
        // USER/ROLE/PERMISSION tables the other users are authorised against.
        Assert.Contains("AddDiscoveredModuleSetups(_ => true)", composition, StringComparison.Ordinal);
        Assert.Contains("IDefaultSecuritySeedService", composition, StringComparison.Ordinal);
    }

    [Fact]
    public void The_write_paths_are_gated_on_a_role_or_a_permission()
    {
        // SETUP-05 AC-8: a capability is grantable through a permission, so a write path must not ask for the role
        // alone. These two assertions fail the build if a page is narrowed back to roles-only.
        var root = SourceRoot.Locate();

        var users = File.ReadAllText(
            Path.Combine(root, "src", "Beep.OilandGas.Desktop", "Views", "UsersView.cs"));
        Assert.Contains("Permissions.CanManageUsers", users, StringComparison.Ordinal);

        var settings = File.ReadAllText(
            Path.Combine(root, "src", "Beep.OilandGas.Desktop", "Views", "SettingsView.cs"));
        Assert.Contains("Permissions.CanConfigureSystem", settings, StringComparison.Ordinal);

        var identity = File.ReadAllText(
            Path.Combine(root, "src", "Beep.OilandGas.Desktop.Data", "Security", "OgDatabaseIdentity.cs"));
        Assert.Contains("ROLE_PERMISSION", identity, StringComparison.Ordinal);
        Assert.Contains("PERMISSION_CODE", identity, StringComparison.Ordinal);
    }

    private static string Squash(string body) =>
        Regex.Replace(body.Replace('\n', ' ').Replace('\r', ' '), @"\s+", " ").Trim();
}
