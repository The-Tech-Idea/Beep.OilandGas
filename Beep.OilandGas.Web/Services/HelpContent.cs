using System.Collections.Generic;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Provides contextual help content for each workflow area in the application.
/// Help content is organized by page route and section within the page.
/// </summary>
public static class HelpContent
{
    public static readonly Dictionary<string, PageHelp> Pages = new()
    {
        // ── Landing & Dashboard ───────────────────────────────────────────────
        ["/"] = new PageHelp
        {
            Title = "Welcome to Beep Oil & Gas",
            Content = "This platform is designed for petroleum engineers to manage the full asset lifecycle from exploration through decommissioning. Use the navigation menu to access your workflow areas."
        },
        ["/dashboard"] = new PageHelp
        {
            Title = "Dashboard",
            Content = "Your personalized dashboard shows key metrics and quick access to your most-used workflows. Cards are filtered based on your assigned persona and permissions."
        },

        // ── Exploration ──────────────────────────────────────────────────────
        ["/exploration"] = new PageHelp
        {
            Title = "Exploration Dashboard",
            Content = "Manage prospect identification, seismic survey planning, and well program approvals. Use the prospect board to track prospects through the evaluation pipeline."
        },
        ["/exploration/prospect-board"] = new PageHelp
        {
            Title = "Prospect Board",
            Content = "Drag and drop prospects between evaluation stages. Click a prospect card to view details, risk assessment, and volumetric estimates."
        },

        // ── Development ──────────────────────────────────────────────────────
        ["/development"] = new PageHelp
        {
            Title = "Development Dashboard",
            Content = "Plan field development including FDP creation, well design, and construction scheduling. The FDP wizard guides you through the full development planning process."
        },
        ["/development/fdp"] = new PageHelp
        {
            Title = "Field Development Plan",
            Content = "Create and manage Field Development Plans. Each FDP includes well count, facility requirements, production forecasts, and economic justification."
        },

        // ── Production ───────────────────────────────────────────────────────
        ["/production"] = new PageHelp
        {
            Title = "Production Dashboard",
            Content = "Monitor daily production, optimize well performance, and manage interventions. The well performance optimizer recommends actions based on decline curve analysis."
        },
        ["/production/well-performance"] = new PageHelp
        {
            Title = "Well Performance Optimizer",
            Content = "Analyze well performance using decline curve analysis, nodal analysis, and lift optimization. Recommendations can be converted directly to work orders."
        },

        // ── Reservoir ─────────────────────────────────────────────────────────
        ["/reservoir"] = new PageHelp
        {
            Title = "Reservoir Overview",
            Content = "View reservoir characterization, EOR screening results, and reserves classification. EOR screening evaluates water flooding, gas injection, and chemical EOR methods."
        },

        // ── Economics ─────────────────────────────────────────────────────────
        ["/economics/evaluation"] = new PageHelp
        {
            Title = "Economic Evaluation",
            Content = "Perform economic analysis including NPV, IRR, payback period, and profitability index. Results can be exported to JSON for reporting."
        },
        ["/economics/afe"] = new PageHelp
        {
            Title = "AFE Management",
            Content = "Create and track Authorization for Expenditure (AFE) requests. AFEs can be linked to work orders and production interventions."
        },

        // ── Accounting ────────────────────────────────────────────────────────
        ["/ppdm39/accounting/dashboard"] = new PageHelp
        {
            Title = "Accounting Dashboard",
            Content = "Monitor accounting activity including volume reconciliation, royalty calculations, cost allocation, and joint interest billing."
        },
        ["/ppdm39/accounting/joint-interest-billing"] = new PageHelp
        {
            Title = "Joint Interest Billing",
            Content = "Track and manage JIB invoices for working interest partners. Generate JIB runs, track payments, and manage disputes."
        },
        ["/ppdm39/accounting/period-close"] = new PageHelp
        {
            Title = "Period Close",
            Content = "Execute period-end close procedures. Complete the pre-close checklist before locking the accounting period."
        },
        ["/ppdm39/accounting/revenue-recognition"] = new PageHelp
        {
            Title = "Revenue Recognition",
            Content = "Monitor revenue recognition by well, product, and period under ASC 606 / IFRS 15. Track recognized vs deferred revenue."
        },

        // ── Compliance ────────────────────────────────────────────────────────
        ["/ppdm39/compliance"] = new PageHelp
        {
            Title = "Compliance Dashboard",
            Content = "Track regulatory obligations, GHG reporting, and royalty compliance. Overdue obligations are highlighted for immediate attention."
        },

        // ── Work Orders ───────────────────────────────────────────────────────
        ["/ppdm39/workorder"] = new PageHelp
        {
            Title = "Work Order Dashboard",
            Content = "Manage work orders from creation through completion. Work orders can be created from production interventions, optimizer recommendations, or directly."
        },

        // ── Data Management ───────────────────────────────────────────────────
        ["/ppdm39/data-management"] = new PageHelp
        {
            Title = "Data Management Hub",
            Content = "Access PPDM39 data management tools including table browsing, schema management, and business associate management. This area is for data administrators."
        },
        ["/ppdm39/setup"] = new PageHelp
        {
            Title = "Database Setup Wizard",
            Content = "Configure your database connection, create the PPDM39 schema, seed reference data, and generate dummy data for testing."
        },

        // ── HSE ───────────────────────────────────────────────────────────────
        ["/hse"] = new PageHelp
        {
            Title = "HSE Operations",
            Content = "Manage health, safety, and environmental operations including incident reporting, HAZOP studies, and compliance calendar."
        },

        // ── Decommissioning ───────────────────────────────────────────────────
        ["/decommissioning"] = new PageHelp
        {
            Title = "Decommissioning",
            Content = "Manage well plugging and abandonment (P&A) and facility decommissioning workflows. Decommissioning records can trigger compliance intake and closeout AFEs."
        }
    };

    public static PageHelp? GetHelpForRoute(string route)
    {
        // Try exact match first
        if (Pages.TryGetValue(route, out var help))
            return help;

        // Try prefix match for sub-routes
        foreach (var kvp in Pages)
        {
            if (route.StartsWith(kvp.Key) && kvp.Key.Length > 1)
                return kvp.Value;
        }

        return null;
    }
}

public class PageHelp
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
