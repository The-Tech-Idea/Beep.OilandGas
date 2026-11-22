# DAT-05 — Entity form and validation

| Field | Value |
|---|---|
| Stage | STG-01 — Data platform and PPDM core |
| Area | Data |
| Status | Gate met - AC-1..AC-6: validation blocks the save before any write, decimals exact, reference pickers list only active codes from the LOV service, keys/audit read-only, defaults from the defaults service scoped to the table, messages localised and naming the field. See [evidence](../../evidence/STG-01-DAT-05/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement` |
| Desktop project | `src/Beep.OilandGas.Desktop.Controls/OgFormBuilder`, `Modules/Data/EntityForm/` |
| External packages | none |
| Depends on | DAT-04 |
| Blocks | DAT-06, DAT-07 and most domain editors |
| Web reference | `Components/Data/EntityFormWizard.razor`, `Components/Shared/PPDMEntityForm.razor` |

## 1. Purpose

Generate a usable, validated, sectioned form from table metadata, so a domain editor is a
configuration rather than a hand-written screen.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39/Core/Metadata/PPDMTableMetadata.cs`, `PPDMEntityTypes.cs` | field types, lengths, keys, LOVs |
| `PPDM39.DataManagement/Services/PPDMDataValidationService.cs` | validation rules |
| `PPDM39.DataManagement/Services/PPDMDefaultValueService.cs` | defaults |
| `PPDM39.DataManagement/Services/LOVManagementService.cs` | reference-value pickers |
| `PPDM39.DataManagement/Services/PPDMMappingService.cs` | code mapping |
| `PPDM39/Core/CommonColumnHandler.cs` | audit columns excluded from the form |

## 3. Screens

`OgFormBuilder` renders sections from a `FormDefinition`:

- Section header, two-column field grid, help text per field.
- Field types: text, multiline, integer, decimal (with precision), date, date-time with offset,
  boolean, reference picker, code picker, unit + value pair, geometry placeholder, read-only key.
- Validation summary at the top, inline messages per field, first invalid field focused on save.
- Unsaved-changes indicator in `PageFrame`.

`Data → Entity form` page hosts it for any table; domain editors reuse the control with a curated
`FormDefinition`.

## 4. Data and contracts

```csharp
public sealed record OgFieldDefinition(
    string Name, string LabelKey, OgFieldKind Kind, bool Required,
    int? MaxLength, int? Precision, string? ReferenceSet, string? Unit,
    string? HelpKey, bool ReadOnly, object? DefaultValue, Func<object?, OgResult>? Validate);

public sealed record OgFormDefinition(string Table, IReadOnlyList<OgFormSection> Sections);
```

## 5. States

| State | Behaviour |
|---|---|
| New record | defaults applied from `PPDMDefaultValueService`; keys editable or generated per metadata |
| Load failed | failed state, no blank form |
| Validation failed | summary plus inline messages, no write |
| Concurrency conflict | original values shown side by side |
| Unsaved and navigating away | confirm dialog with save / discard / cancel |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Save | `data.write` + table policy | yes |
| Save and new | as Save | yes |
| Revert | any editor | no |

## 7. Acceptance criteria

- AC-1 Mandatory violation blocks the save before any service call.
- AC-2 Decimal precision is preserved exactly (no `double` round trip).
- AC-3 Reference pickers list only active codes for the correct reference set.
- AC-4 Audit and system columns are never editable.
- AC-5 Defaults come from the owning service, not from the form.
- AC-6 Validation messages are localized and name the field.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~DAT-05"
```

## 9. Open questions and risks

- Metadata may not carry enough information (labels, units, sections). Provide per-table
  `FormDefinition` overrides instead of guessing.
- Geometry and image fields need a placeholder in 1.0; do not fake an editor.
