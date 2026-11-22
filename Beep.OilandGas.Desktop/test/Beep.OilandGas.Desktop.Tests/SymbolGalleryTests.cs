using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.WellSchematic;
using TheTechIdea.OilGas.Wellbore.Data;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-08: the gallery reports the catalog it loaded, filters by category and search, renders a symbol to a
/// frame, assigns a symbol to a component so the schematic changes, undoes it, and refuses a bad catalog
/// without losing the active one.
/// </summary>
public sealed class SymbolGalleryTests
{
    [Fact]
    public void The_catalog_reports_its_symbols_and_categories()
    {
        UseDefaultEnvironment();

        var catalog = SymbolCatalog.Current;
        var summary = OgSymbolGallery.Summarise(catalog, "default");

        // AC-1: the loaded count and the category count, from the catalog itself.
        Assert.True(summary.Symbols > 100, $"Only {summary.Symbols} symbols.");
        Assert.True(summary.Categories > 5, $"Only {summary.Categories} categories.");
        Assert.Equal(summary.Symbols, summary.Shown);
        Assert.Equal("default", summary.Source);
    }

    [Fact]
    public void Filtering_by_category_keeps_only_that_category()
    {
        UseDefaultEnvironment();

        var catalog = SymbolCatalog.Current;
        var category = OgSymbolGallery.Categories(catalog)[0];

        var rows = OgSymbolGallery.Rows(catalog, category);

        // AC-2: category filter.
        Assert.NotEmpty(rows);
        Assert.All(rows, row => Assert.Equal(category, row.Category, ignoreCase: true));
        Assert.True(rows.Count < catalog.Symbols.Count, "The category filter returned everything.");
    }

    [Fact]
    public void Search_matches_an_id_or_a_name_case_insensitively()
    {
        UseDefaultEnvironment();

        var catalog = SymbolCatalog.Current;
        var sample = catalog.Symbols.First(symbol => symbol.Name.Length > 3);

        var byId = OgSymbolGallery.Rows(catalog, search: sample.Id);
        Assert.Contains(byId, row => string.Equals(row.Id, sample.Id, StringComparison.OrdinalIgnoreCase));

        var byName = OgSymbolGallery.Rows(catalog, search: sample.Name[..3].ToLowerInvariant());
        Assert.Contains(byName, row => string.Equals(row.Id, sample.Id, StringComparison.Ordinal));

        // AC-2: both filters at once.
        var both = OgSymbolGallery.Rows(catalog, sample.Category, sample.Id);
        Assert.Contains(both, row => row.Id == sample.Id);

        Assert.Empty(OgSymbolGallery.Rows(catalog, search: "zzzz-no-such-symbol"));
    }

    [Fact]
    public void A_symbol_renders_to_a_non_empty_frame()
    {
        UseDefaultEnvironment();

        var symbol = SymbolCatalog.Current.Symbols.First(item => item.IsEquipment);

        var frame = OgSymbolGallery.Preview(symbol, 96, 96);

        Assert.NotNull(frame);
        Assert.Equal(96 * 96 * 4, frame!.Length);

        // Something other than the white background must have been painted.
        var painted = 0;
        for (var i = 0; i < frame.Length; i += 4)
        {
            if (frame[i] != 0xFF || frame[i + 1] != 0xFF || frame[i + 2] != 0xFF)
            {
                painted++;
            }
        }

        Assert.True(painted > 0, "The preview frame is blank.");
    }

    [Fact]
    public void A_preview_size_of_zero_is_refused_rather_than_throwing()
    {
        UseDefaultEnvironment();

        Assert.Null(OgSymbolGallery.Preview(SymbolCatalog.Current.Symbols[0], 0, 96));
    }

    [Fact]
    public void An_assignment_changes_the_schematic_and_undo_restores_the_previous_symbol()
    {
        UseDefaultEnvironment();

        var document = OgSchematicMapper.Demo();
        var view = new WellSchematicView { Schematic = document };
        using var renderer = new ViewSurfaceRenderer { Surface = view };

        Assert.True(renderer.Render(420, 320));
        var before = renderer.Frame!.ToArray();

        var equipment = document.Equipment.First(item => !string.IsNullOrWhiteSpace(item.SymbolId));
        var original = equipment.SymbolId;
        var replacement = SymbolCatalog.Current.Symbols
            .First(symbol => symbol.IsEquipment && !string.Equals(symbol.Id, original, StringComparison.Ordinal))
            .Id;

        var history = new OgLogEditHistory();
        var assigned = OgSymbolGallery.Assign(document, equipment.Id, replacement, history);

        Assert.True(assigned.Success, assigned.Message);
        Assert.Equal(replacement, equipment.SymbolId);
        Assert.Single(history.Entries);
        Assert.Equal(OgLogEditKind.Symbol, history.Entries[0].Kind);

        // AC-3: the change is on the drawing, not only in the model. The layout is cached against the view
        // document's revision, so an in-place edit must be committed first.
        view.CommitDocumentChanges();
        Assert.True(renderer.Render(420, 320));
        Assert.False(before.AsSpan().SequenceEqual(renderer.Frame!), "The assigned symbol did not change the frame.");

        // AC-4: undo restores the previous symbol and the previous picture.
        Assert.True(history.Undo());
        Assert.Equal(original, equipment.SymbolId);
        view.CommitDocumentChanges();

        Assert.True(renderer.Render(420, 320));
        Assert.True(before.AsSpan().SequenceEqual(renderer.Frame!), "Undo did not restore the picture.");
    }

    [Fact]
    public void Assigning_to_an_equipment_that_is_not_on_the_schematic_is_refused()
    {
        UseDefaultEnvironment();

        var result = OgSymbolGallery.Assign(
            OgSchematicMapper.Demo(),
            "no-such-equipment",
            "eq_wellhead",
            new OgLogEditHistory());

        Assert.False(result.Success);
        Assert.Equal("symbol-equipment-not-found", result.Reference);
    }

    [Fact]
    public void An_invalid_catalog_is_rejected_without_losing_the_active_one()
    {
        UseDefaultEnvironment();

        var state = new OgSymbolCatalogState();
        var activeBefore = state.Active;

        // AC-5: a parse failure propagates (the page reports it) and the active catalog is unchanged.
        Assert.ThrowsAny<JsonException>(() => state.LoadCustom("{ not a catalog"));
        Assert.Same(activeBefore, state.Active);
        Assert.Equal("default", state.Source);

        // A catalog that parses but holds nothing is refused with a reason.
        var empty = state.LoadCustom("""{ "symbols": [] }""");
        Assert.False(empty.Success);
        Assert.Equal(OgSymbolGallery.CodeCatalogEmpty, empty.Reference);
        Assert.Same(activeBefore, state.Active);
        Assert.Equal("default", state.Source);
    }

    [Fact]
    public void A_valid_custom_catalog_replaces_the_active_one_and_the_default_is_restored_on_demand()
    {
        UseDefaultEnvironment();

        var state = new OgSymbolCatalogState();
        var sample = SymbolCatalog.Current.Symbols.First(symbol => symbol.IsEquipment);

        var json = JsonSerializer.Serialize(new
        {
            symbols = new[]
            {
                new
                {
                    id = sample.Id,
                    name = sample.Name,
                    category = sample.Category,
                    group = sample.Group,
                },
            },
        });

        var loaded = state.LoadCustom(json);

        Assert.True(loaded.Success, loaded.Message);
        Assert.Equal("custom", state.Source);
        Assert.Single(state.Active.Symbols);

        // AC-6: an explicit action brings the tools' default back.
        state.RestoreDefault();

        Assert.Equal("default", state.Source);
        Assert.True(state.Active.Symbols.Count > 100, $"Only {state.Active.Symbols.Count} symbols after restore.");
    }

    [Fact]
    public void The_component_choices_come_from_the_schematic()
    {
        UseDefaultEnvironment();

        var document = OgSchematicMapper.Demo();
        var choices = OgSymbolGallery.EquipmentChoices(document);

        Assert.Equal(document.Equipment.Count, choices.Count);
        Assert.All(choices, choice => Assert.False(string.IsNullOrWhiteSpace(choice)));
        Assert.NotNull(OgSymbolGallery.FindEquipment(document, choices[0]));
    }

    private static void UseDefaultEnvironment()
    {
        OgToolboxBootstrap.RegisterAll();
        WellboreServices.Global = new WellboreServices();
    }
}
