using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// WEL-02: one well's header - identity, location, classification, dates, participants and identifiers.
/// </summary>
/// <remarks>
/// Status is shown and never written (AC-6, WEL-04 owns it). A location change needs the coordinate system that
/// produced it, which is recorded with the actor and time (AC-3). A save is guarded by the values the header was
/// read with, so a stale edit is a conflict rather than an overwrite (AC-1). Two identifiers of one type that
/// disagree are not auto-chosen; a person picks one and the choice is recorded (AC-2).
/// </remarks>
public sealed class WellHeaderPageView : OgPageBase
{
    public const string Route = "well";

    private readonly OgApplication _application;
    private readonly IOgWellHeader _header;
    private readonly Diagnostics.OgWellHeaderNotesStore _notes;

    private readonly TextBlock _context;
    private readonly TextBlock _notice;
    private readonly TextBlock _banner;
    private readonly TextBlock _status;
    private readonly TextBlock _locationChip;
    private readonly ListBox _participants;
    private readonly ListBox _identifiers;
    private readonly ComboBox _identifierChoice;
    private readonly Dictionary<string, TextBox> _editors = new(StringComparer.OrdinalIgnoreCase);
    private readonly TextBox _coordinateSystem;
    private readonly TextBox _participantId;
    private readonly TextBox _participantDescription;

    private OgWellHeaderBundle? _bundle;

    public WellHeaderPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _header = application.WellHeader;
        _notes = application.WellHeaderNotes;

        _context = Body(string.Empty);
        _notice = Body(string.Empty);
        _banner = Body(string.Empty);
        _banner.FontWeight = FontWeights.SemiBold;
        _status = Body(string.Empty);
        _locationChip = Body(string.Empty);

        var tabs = new TabControl { Margin = new Thickness(0, 6, 0, 8), MinHeight = 320, IsTabStop = true };
        Named(tabs, "Well header sections");

        tabs.Items.Add(Tab("Identity", Editor("Name", "WELL_NAME"), Editor("Government id", "WELL_GOVERNMENT_ID"),
            Editor("Field", "ASSIGNED_FIELD"), Editor("Operator", "OPERATOR")));

        _coordinateSystem = Input("Coordinate system (required with a location change)");
        Named(_coordinateSystem, "Coordinate system");
        tabs.Items.Add(Tab("Location",
            _coordinateSystem,
            Editor("Latitude", "SURFACE_LATITUDE"),
            Editor("Longitude", "SURFACE_LONGITUDE"),
            Editor("Ground elevation", "GROUND_ELEV"),
            Editor("Kelly bushing", "KB_ELEV"),
            Editor("Final TD", "FINAL_TD")));

        tabs.Items.Add(Tab("Classification", _status, _locationChip));

        var dates = new WrapPanel();
        dates.Children.Add(Caption("Spud"));
        dates.Children.Add(ReadOnly("SPUD_DATE"));
        dates.Children.Add(Caption("Completed"));
        dates.Children.Add(ReadOnly("COMPLETION_DATE"));
        dates.Children.Add(Caption("Abandoned"));
        dates.Children.Add(ReadOnly("ABANDONMENT_DATE"));
        dates.Children.Add(Caption("Last activity"));
        dates.Children.Add(ReadOnly("CURRENT_STATUS_DATE"));
        tabs.Items.Add(Tab("Dates", dates));

        _participants = new ListBox { MinHeight = 150, IsTabStop = true, Margin = new Thickness(0, 0, 0, 6) };
        Named(_participants, "Participants");

        _participantId = Input("Business associate id");
        _participantDescription = Input("Role or description");
        Named(_participantId, "Participant business associate id");
        Named(_participantDescription, "Participant role");

        var participantsPanel = new StackPanel();
        participantsPanel.Children.Add(_participants);
        participantsPanel.Children.Add(Row(_participantId, _participantDescription,
            Button("Add participant", (_, _) => _ = AddParticipantAsync()),
            Button("Remove selected", (_, _) => _ = RemoveParticipantAsync())));
        tabs.Items.Add(Tab("Participants", participantsPanel));

        _identifiers = new ListBox { MinHeight = 120, IsTabStop = true, Margin = new Thickness(0, 0, 0, 6) };
        Named(_identifiers, "Identifiers");

        _identifierChoice = new ComboBox { MinWidth = 220, Margin = new Thickness(0, 0, 8, 0) };
        Named(_identifierChoice, "Identifier to use");

        var identifierPanel = new StackPanel();
        identifierPanel.Children.Add(_identifiers);
        identifierPanel.Children.Add(Row(_identifierChoice,
            Button("Use this identifier", (_, _) => ResolveIdentifier())));
        tabs.Items.Add(Tab("Identifiers", identifierPanel));

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Button("Save", (_, _) => _ = SaveAsync()));
        toolbar.Children.Add(Button("Reload", (_, _) => _ = ReloadAsync()));
        toolbar.Children.Add(Button("Wellbores", (_, _) => _application.Pages.Navigate(WellboresPageView.Route)));
        toolbar.Children.Add(Button("Status", (_, _) => _application.Pages.Navigate(WellStatusPageView.Route)));
        toolbar.Children.Add(Button("Stratigraphy", (_, _) => _application.Pages.Navigate(StratigraphyPageView.Route)));
        toolbar.Children.Add(Button("Back to wells", (_, _) => _application.Pages.Navigate(WellSearchPageView.Route)));

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_context);
        panel.Children.Add(_banner);
        panel.Children.Add(_notice);
        panel.Children.Add(tabs);

        ShowContent(panel);

        OpenPending();
    }

    /// <summary>The well shown, or null before one is opened.</summary>
    public OgWellHeader? Header => _bundle?.Header;

    public IReadOnlyList<OgWellParticipant> Participants => _bundle?.Participants ?? [];

    public IReadOnlyList<OgWellIdentifier> Identifiers => _bundle?.Identifiers ?? [];

    public string NoticeText => _notice.Text;

    public string AmbiguityText => _banner.Text;

    public string LocationChip => _locationChip.Text;

    /// <summary>True when the last save found the row changed by somebody else (AC-1).</summary>
    public bool LastConflict { get; private set; }

    /// <summary>The identifier types that disagree and have no recorded choice yet (AC-2).</summary>
    public IReadOnlyList<string> UnresolvedAmbiguities { get; private set; } = [];

    /// <summary>AC-6: the status control is a label, never an editor.</summary>
    public bool StatusIsReadOnly => Header?.StatusIsReadOnly ?? true;

    /// <summary>Opens the well the index selected.</summary>
    public bool OpenPending()
    {
        var selected = _application.WellContext.Current;

        return selected is not null && Open(selected.Id);
    }

    /// <summary>Loads one well.</summary>
    public bool Open(string uwi)
    {
        var loaded = _header.LoadAsync(uwi).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            _bundle = null;
            _context.Text = $"Well {uwi}";
            _notice.Text = loaded.Message ?? "The well header could not be read.";
            ShowState(OgViewState.Failed, "The well header could not be read.", loaded.Reference);
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    /// <summary>Re-reads the open well, discarding unsaved edits.</summary>
    public Task<bool> ReloadAsync() =>
        Task.FromResult(Header is { } header && Open(header.Uwi));

    /// <summary>Saves the edited header (AC-1, AC-3, AC-6).</summary>
    public async Task<bool> SaveAsync()
    {
        if (_bundle is null)
        {
            _notice.Text = "No well is open.";
            return false;
        }

        if (!TryReadEditors(_bundle.Header, out var updated, out var reason))
        {
            _notice.Text = reason;
            return false;
        }

        var coordinatesChanged =
            updated.SurfaceLatitude != _bundle.Header.SurfaceLatitude
            || updated.SurfaceLongitude != _bundle.Header.SurfaceLongitude;

        var saved = await _header.SaveAsync(
            updated,
            _bundle.Header,
            _coordinateSystem.Text.Trim(),
            _application.Session.UserId).ConfigureAwait(true);

        LastConflict = saved.Reference == OgWellHeaderService.CodeConflict;
        _notice.Text = saved.Success
            ? saved.Message ?? "Well saved."
            : LastConflict
                ? "The well changed since it was read; reload the header to see the current values before saving again."
                : saved.Message ?? "The well was not saved.";

        if (!saved.Success)
        {
            return false;
        }

        if (coordinatesChanged)
        {
            // AC-3: the coordinate system travels with the change, recorded with who and when.
            var record = _notes.RecordLocationSystem(
                _bundle.Header.Uwi, _coordinateSystem.Text, _application.Session.UserId);
            _application.AuditLog.Add(
                "well.location",
                "well",
                _bundle.Header.Uwi,
                "ok",
                $"coordinateSystem={record.CoordinateSystem}");
        }

        _application.AuditLog.Add("well.header.save", "well", _bundle.Header.Uwi, "ok", saved.Message ?? string.Empty);
        await ReloadAsync().ConfigureAwait(true);
        return true;
    }

    /// <summary>Adds a participant, recording the actor and date (AC-4).</summary>
    public async Task<bool> AddParticipantAsync()
    {
        if (_bundle is null)
        {
            _notice.Text = "No well is open.";
            return false;
        }

        var added = await _header.AddParticipantAsync(
            _bundle.Header.Uwi,
            _participantId.Text,
            _participantDescription.Text,
            _application.Session.UserId).ConfigureAwait(true);

        _notice.Text = added.Message ?? (added.Success ? "Participant added." : "The participant was not added.");

        if (added.Success)
        {
            _application.AuditLog.Add(
                "well.participant.add",
                "well",
                _bundle.Header.Uwi,
                "ok",
                _participantId.Text.Trim());
            await ReloadAsync().ConfigureAwait(true);
        }

        return added.Success;
    }

    /// <summary>Removes the selected participant; the operator is refused (AC-5).</summary>
    public async Task<bool> RemoveParticipantAsync()
    {
        if (_bundle is null || _participants.SelectedItem is not OgWellParticipant participant)
        {
            _notice.Text = "Select a participant first.";
            return false;
        }

        var removed = await _header.RemoveParticipantAsync(
            _bundle.Header.Uwi,
            participant.BusinessAssociateId,
            _application.Session.UserId).ConfigureAwait(true);

        _notice.Text = removed.Message ?? (removed.Success ? "Participant removed." : "The participant was not removed.");

        if (removed.Success)
        {
            _application.AuditLog.Add(
                "well.participant.remove",
                "well",
                _bundle.Header.Uwi,
                "ok",
                participant.BusinessAssociateId);
            await ReloadAsync().ConfigureAwait(true);
        }

        return removed.Success;
    }

    /// <summary>Records the identifier a person chose for a conflicting type (AC-2).</summary>
    public bool ResolveIdentifier()
    {
        if (_bundle is null || _identifierChoice.SelectedItem is not OgWellIdentifier identifier)
        {
            _notice.Text = "Choose the identifier to use first.";
            return false;
        }

        _notes.Choose(
            _bundle.Header.Uwi,
            identifier.Type,
            identifier.XrefId,
            _application.Session.UserId);

        _application.AuditLog.Add(
            "well.identifier.resolve",
            "well",
            _bundle.Header.Uwi,
            "ok",
            $"{identifier.Type}={identifier.XrefId}");

        UpdateAmbiguities(_bundle);
        _notice.Text = $"{identifier.Type} is now {identifier.XrefId}.";
        return true;
    }

    private void Apply(OgWellHeaderBundle bundle)
    {
        _bundle = bundle;

        _context.Text =
            $"{bundle.Header.Name ?? bundle.Header.Uwi} · UWI {bundle.Header.Uwi}" +
            (string.IsNullOrWhiteSpace(bundle.Header.ApiNumber) ? string.Empty : $" · API {bundle.Header.ApiNumber}") +
            (string.IsNullOrWhiteSpace(bundle.Header.Field) ? string.Empty : $" · {bundle.Header.Field}");

        SetEditor("WELL_NAME", bundle.Header.Name);
        SetEditor("WELL_GOVERNMENT_ID", bundle.Header.GovernmentId);
        SetEditor("ASSIGNED_FIELD", bundle.Header.Field);
        SetEditor("OPERATOR", bundle.Header.Operator);
        SetEditor("SURFACE_LATITUDE", bundle.Header.SurfaceLatitude?.ToString(CultureInfo.InvariantCulture));
        SetEditor("SURFACE_LONGITUDE", bundle.Header.SurfaceLongitude?.ToString(CultureInfo.InvariantCulture));
        SetEditor("GROUND_ELEV", bundle.Header.GroundElevation?.ToString(CultureInfo.InvariantCulture));
        SetEditor("KB_ELEV", bundle.Header.KellyBushing?.ToString(CultureInfo.InvariantCulture));
        SetEditor("FINAL_TD", bundle.Header.FinalTd?.ToString(CultureInfo.InvariantCulture));

        SetReadOnly("SPUD_DATE", bundle.Header.SpudDate);
        SetReadOnly("COMPLETION_DATE", bundle.Header.CompletionDate);
        SetReadOnly("ABANDONMENT_DATE", bundle.Header.AbandonmentDate);
        SetReadOnly("CURRENT_STATUS_DATE", bundle.Header.LastActivity);

        _coordinateSystem.Text = _notes.LocationSystem(bundle.Header.Uwi)?.CoordinateSystem ?? string.Empty;

        // AC-6: status is shown with where it is changed, never as an editor.
        _status.Text =
            $"Status: {bundle.Header.Status ?? "(not recorded)"} · changed in WEL-04, not here. " +
            $"Type {bundle.Header.WellType ?? "-"} · class {bundle.Header.WellClass ?? "-"} · profile {bundle.Header.Profile ?? "-"}.";

        _locationChip.Text = bundle.Header.HasLocation
            ? string.Empty
            : "This well has no coordinates; the map and survey pages are disabled until a location is recorded.";

        _participants.ItemsSource = bundle.Participants
            .Select(participant =>
                $"{participant.Name} ({participant.BusinessAssociateId})" +
                (string.Equals(participant.BusinessAssociateId, bundle.Header.Operator, StringComparison.OrdinalIgnoreCase)
                    ? " · operator"
                    : string.Empty) +
                (string.IsNullOrWhiteSpace(participant.Description) ? string.Empty : $" · {participant.Description}"))
            .ToList();

        _identifiers.ItemsSource = bundle.Identifiers
            .Select(identifier =>
                $"{identifier.Type} {identifier.XrefId}" +
                (string.IsNullOrWhiteSpace(identifier.Source) ? string.Empty : $" ({identifier.Source})") +
                (identifier.Conflict ? "  <-- conflicting" : string.Empty))
            .ToList();

        UpdateAmbiguities(bundle);
        _notice.Text = "Edit a field and save; status changes belong to WEL-04.";
        ShowState(OgViewState.Ready, string.Empty);
    }

    private void UpdateAmbiguities(OgWellHeaderBundle bundle)
    {
        var chosen = _notes.Choices(bundle.Header.Uwi);

        UnresolvedAmbiguities = bundle.AmbiguousTypes
            .Where(type => chosen.All(choice => !string.Equals(choice.Type, type, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var conflicting = bundle.Identifiers.Where(identifier => identifier.Conflict).ToList();
        _identifierChoice.ItemsSource = conflicting;
        _identifierChoice.DisplayMemberPath = nameof(OgWellIdentifier.XrefId);

        _banner.Text = UnresolvedAmbiguities.Count == 0
            ? string.Empty
            : $"Conflicting identifiers: {string.Join(", ", UnresolvedAmbiguities)}. " +
              "No value is chosen automatically; pick the one to use.";
    }

    private bool TryReadEditors(OgWellHeader original, out OgWellHeader updated, out string? reason)
    {
        updated = original;
        reason = null;

        if (!TryNumber("SURFACE_LATITUDE", out var latitude) ||
            !TryNumber("SURFACE_LONGITUDE", out var longitude) ||
            !TryNumber("GROUND_ELEV", out var ground) ||
            !TryNumber("KB_ELEV", out var kb) ||
            !TryNumber("FINAL_TD", out var td))
        {
            reason = "A number field contains something that is not a number.";
            return false;
        }

        updated = original with
        {
            Name = Value("WELL_NAME"),
            GovernmentId = Value("WELL_GOVERNMENT_ID"),
            Field = Value("ASSIGNED_FIELD"),
            Operator = Value("OPERATOR"),
            SurfaceLatitude = latitude,
            SurfaceLongitude = longitude,
            GroundElevation = ground,
            KellyBushing = kb,
            FinalTd = td,
        };

        return true;
    }

    private bool TryNumber(string column, out decimal? value)
    {
        var text = Value(column);
        value = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }

    private string? Value(string column) =>
        _editors.TryGetValue(column, out var editor) && !string.IsNullOrWhiteSpace(editor.Text)
            ? editor.Text.Trim()
            : null;

    private void SetEditor(string column, string? value)
    {
        if (_editors.TryGetValue(column, out var editor))
        {
            editor.Text = value ?? string.Empty;
        }
    }

    private void SetReadOnly(string column, DateTime? value)
    {
        if (_editors.TryGetValue(column, out var editor))
        {
            editor.Text = value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        }
    }

    private WrapPanel Editor(string label, string column)
    {
        var input = Input(label);
        _editors[column] = input;
        Named(input, label);

        return Row(Caption(label), input);
    }

    private WrapPanel ReadOnly(string column)
    {
        var input = Input("read-only");
        input.IsReadOnly = true;
        input.IsTabStop = true;
        _editors[column] = input;
        Named(input, column);

        return Row(input);
    }

    private static TabItem Tab(string header, params object[] content)
    {
        var panel = new StackPanel { Margin = new Thickness(8) };

        foreach (var item in content)
        {
            switch (item)
            {
                case WrapPanel row:
                    panel.Children.Add(row);
                    break;
                case UIElement element:
                    panel.Children.Add(element);
                    break;
                case null:
                    break;
                default:
                    panel.Children.Add(new TextBlock { Text = item.ToString() });
                    break;
            }
        }

        return new TabItem
        {
            Header = header,
            // The scroll region is not a stop of its own; Tab moves between the fields it contains.
            Content = new ScrollViewer { Content = panel, Focusable = false },
        };
    }

    private static WrapPanel Row(params object[] items)
    {
        var row = new WrapPanel { Margin = new Thickness(0, 2, 0, 2) };

        foreach (var item in items)
        {
            if (item is UIElement element)
            {
                row.Children.Add(element);
            }
        }

        return row;
    }

    private static TextBox Input(string toolTip) =>
        new() { MinWidth = 220, Margin = new Thickness(0, 0, 8, 0), ToolTip = toolTip };

    private static Button Button(string text, RoutedEventHandler onClick)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(10, 3, 10, 3),
            Margin = new Thickness(0, 0, 8, 0),
        };

        button.Click += onClick;
        return button;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
        MinWidth = 110,
    };
}
