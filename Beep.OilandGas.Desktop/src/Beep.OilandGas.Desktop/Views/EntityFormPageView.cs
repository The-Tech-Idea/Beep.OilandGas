using System.Windows;
using System.Windows.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// DAT-05: a form generated from a table's metadata - typed fields, keys and audit columns read-only, and
/// validation that runs before anything is written.
/// </summary>
public sealed class EntityFormPageView : OgPageBase
{
    public const string Route = "entityform";

    private readonly OgApplication _application;
    private readonly IOgPpdmTableGateway _gateway;
    private readonly OgEntityFormService _service;
    private readonly TextBlock _context;
    private readonly TextBlock _notice;
    private readonly StackPanel _fields;
    private readonly ListBox _problems;
    private readonly Dictionary<string, TextBox> _editors = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ComboBox> _pickers = new(StringComparer.OrdinalIgnoreCase);

    private OgFormDefinition? _form;

    public EntityFormPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _gateway = application.TableGateway;
        _service = new OgEntityFormService(_gateway);

        _context = Body(string.Empty);
        _notice = Body(string.Empty);

        _fields = new StackPanel { Margin = new Thickness(0, 4, 0, 8) };
        // REL-03: a focusable list must be reachable by Tab.
        _problems = new ListBox { MaxHeight = 140, Margin = new Thickness(0, 0, 0, 8), IsTabStop = true };

        Named(_problems, "Validation problems");

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Button("Save as new row", async (_, _) => await SaveAsync(isNew: true)));
        toolbar.Children.Add(Button("Save changes", async (_, _) => await SaveAsync(isNew: false)));

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_context);
        panel.Children.Add(_notice);
        panel.Children.Add(_fields);
        panel.Children.Add(_problems);

        ShowContent(panel);

        Loaded += (_, _) => OpenPending();
        OpenPending();
    }

    /// <summary>The table whose form is shown, or null.</summary>
    public string? TableName { get; private set; }

    public OgFormDefinition? Form => _form;

    public IReadOnlyList<OgFormField> Fields => _form?.Fields ?? [];

    /// <summary>How many times a save reached the gateway; a blocked form leaves this at zero (AC-1).</summary>
    public int WriteAttempts => _service.WriteAttempts;

    public IReadOnlyList<OgFormProblem> Problems { get; private set; } = [];

    public string NoticeText => _notice.Text;

    /// <summary>Opens the table the tree left for the workbench, if any.</summary>
    public bool OpenPending()
    {
        var pending = _application.Workbench.Take();

        return pending is not null && Open(pending.TableName);
    }

    /// <summary>Builds the form for a table and lays out one control per field.</summary>
    public bool Open(string tableName)
    {
        var form = _gateway.FormAsync(tableName).GetAwaiter().GetResult();

        if (!form.Success || form.Value is null)
        {
            TableName = null;
            _form = null;
            _fields.Children.Clear();
            _editors.Clear();
            _pickers.Clear();
            _context.Text = $"Table {tableName}";
            _notice.Text = form.Message ?? "The form could not be generated.";

            ShowState(OgViewState.Failed, "The form could not be generated.", form.Reference);
            return false;
        }

        TableName = tableName;
        _form = form.Value;
        Problems = [];
        _problems.ItemsSource = null;

        _fields.Children.Clear();
        _editors.Clear();

        foreach (var field in _form.Fields)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };

            var label = new TextBlock
            {
                Text = field.Label + (field.Required && !field.ReadOnly ? " *" : string.Empty),
                Width = 220,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
            };

            if (field.Lookup is { Count: > 0 } choices)
            {
                // AC-3: a reference column gets a picker listing only the active codes the owner supplied.
                var picker = new ComboBox { MinWidth = 260, IsEnabled = !field.ReadOnly, Tag = field };
                picker.ItemsSource = choices.Select(choice => choice.Code).ToList();

                if (field.Default is { } preset && choices.Any(choice => choice.Code == preset))
                {
                    picker.SelectedItem = preset;
                }

                Named(picker, field.Label);
                _pickers[field.Column] = picker;

                row.Children.Add(label);
                row.Children.Add(picker);
                _fields.Children.Add(row);
                continue;
            }

            var editor = new TextBox
            {
                MinWidth = 260,
                IsReadOnly = field.ReadOnly,
                ToolTip = field.Note,
                Tag = field,
                Text = field.Default ?? string.Empty,
            };

            Named(editor, field.Label);

            _editors[field.Column] = editor;

            row.Children.Add(label);
            row.Children.Add(editor);
            _fields.Children.Add(row);
        }

        _context.Text = $"{_form.TableName} · {_form.Fields.Count} field(s) · key " +
                        (_form.KeyColumns.Count == 0 ? "(none recorded)" : string.Join("+", _form.KeyColumns));

        _notice.Text = "Fields marked * are required. Key and audit columns are read-only.";

        ShowState(OgViewState.Ready, string.Empty);
        return true;
    }

    /// <summary>Reads the editors into the form's value map.</summary>
    public IReadOnlyDictionary<string, string?> Values()
    {
        var values = _editors.ToDictionary(
            pair => pair.Key,
            pair => (string?)pair.Value.Text,
            StringComparer.OrdinalIgnoreCase);

        foreach (var (column, picker) in _pickers)
        {
            values[column] = picker.SelectedItem as string;
        }

        return values;
    }

    /// <summary>How many fields were rendered as reference pickers (AC-3).</summary>
    public int PickerCount => _pickers.Count;

    /// <summary>Fills an editor, as a person or a test would.</summary>
    public void SetValue(string column, string? value)
    {
        if (_pickers.TryGetValue(column, out var picker))
        {
            picker.SelectedItem = value;
            return;
        }

        if (_editors.TryGetValue(column, out var editor))
        {
            editor.Text = value ?? string.Empty;
        }
    }

    /// <summary>Validates and, only if valid, writes (AC-1).</summary>
    public async Task<bool> SaveAsync(bool isNew)
    {
        if (_form is null)
        {
            _notice.Text = "No form is open.";
            return false;
        }

        // DAT-08 AC-6: a governed write is refused while the audit chain is broken and unacknowledged.
        if (!_application.ChainGate.Allows(_application.AuditChain))
        {
            _notice.Text =
                "The audit chain is broken, so writes to governed tables are blocked until it is acknowledged " +
                "on the Governance page.";
            return false;
        }

        var values = Values();

        if (isNew)
        {
            var saved = await _service.InsertAsync(_form, values).ConfigureAwait(true);

            if (saved.Success)
            {
                RecordVersion(values);
            }

            return Finish(saved);
        }

        // Update needs the row as it was read; the workbench row is not held here, so the key values are used as
        // the guard and the caller is expected to have loaded them from the grid.
        var original = _form.KeyColumns
            .Where(values.ContainsKey)
            .ToDictionary(key => key, key => (object?)values[key]?.Trim(), StringComparer.OrdinalIgnoreCase);

        var updated = await _service.UpdateAsync(_form, values, original).ConfigureAwait(true);

        if (updated.Success)
        {
            RecordVersion(values);
        }

        return Finish(updated);
    }

    /// <summary>DAT-08: appends the saved state as a row version and chains the change.</summary>
    private void RecordVersion(IReadOnlyDictionary<string, string?> values)
    {
        if (_form is null || TableName is null)
        {
            return;
        }

        var key = string.Join(
            "+",
            _form.KeyColumns.Select(column => values.TryGetValue(column, out var value) ? value ?? string.Empty : string.Empty));

        var stored = values
            .Where(pair => pair.Value is not null)
            .ToDictionary(pair => pair.Key, pair => (object?)pair.Value, StringComparer.OrdinalIgnoreCase);

        _application.Versions.Record(TableName, key, stored, _application.Session.UserId);
        _application.AuditChain.Append(
            "row.save",
            $"{TableName}/{key} saved from the form",
            _application.Session.UserId);
    }

    private bool Finish(OgResult result)
    {
        Problems = [];

        if (!result.Success)
        {
            _problems.ItemsSource = new[] { result.Message ?? "The form could not be saved." };
            _notice.Text = result.Reference == "form-invalid"
                ? "Nothing was written: fix the fields named above."
                : result.Message ?? "The row was not saved.";

            _application.Diagnostics.Add(
                "entity.form",
                "table",
                TableName ?? "(none)",
                "refused",
                result.Message ?? string.Empty);

            return false;
        }

        _notice.Text = result.Message ?? "Row saved.";
        _application.AuditLog.Add("entity.save", "table", TableName ?? "(none)", "ok", result.Message ?? string.Empty);
        return true;
    }

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
}
