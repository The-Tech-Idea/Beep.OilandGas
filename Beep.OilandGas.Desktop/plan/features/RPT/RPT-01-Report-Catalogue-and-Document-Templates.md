# RPT-01 — Report catalogue and document templates

| Field | Value |
|---|---|
| Stage | STG-18 — Training, reporting and release |
| Area | Reporting |
| Status | Gate met - see [evidence](../../evidence/STG-18-RPT-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement`, `Beep.OilandGas.Models` |
| Desktop project | `Modules/Reporting/Catalogue/` |
| External packages | none |
| Depends on | ACC-02, PRD-05 |
| Blocks | RPT-02 |
| Web reference | `Pages/PPDM39/Accounting/*` document rendering, `Beep.DoitAll.Erp.Wpf/Services/ErpDocumentTemplateService.cs` |

## 1. Purpose

Give every area a report it can run with parameters, rendered from a template with tokens, and
exported — instead of each page inventing its own print path.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement` report sources | data |
| ERP `ErpDocumentTemplateService` | template storage and token rendering |
| ERP `InvoiceDocumentRenderer` | HTML + EML rendering pattern |
| `Beep.OilandGas.Models/Core/Interfaces/IRenderingService.cs` | chart rendering (RPT-03) |

## 3. Screens

`Reporting → Reports`

- Catalogue grouped by area: production, accounting, HSE, engineering, reserves.
- Report parameter form generated from the report definition (period, field, pool, well, currency).
- Preview pane with pagination.
- Template tab: subject and body with token insertion, list of available tokens, default flag.
- Export: PDF, XLSX, CSV, HTML.
- Scheduled output is out of scope; "run now" only.

## 4. Data and contracts

`OgReportDefinition { Key, Name, Area, Parameters[], TemplateRequired }`
`OgReportResult { Title, Pages, GeneratedAt, Parameters }`
Tokens use the established `{{Token}}` syntax; an unknown token fails rendering with its name.

## 5. States

| State | Behaviour |
|---|---|
| Missing parameter | run disabled with the field named |
| Unknown token | rendering fails naming the token |
| No default template | generated fallback layout, labelled |
| Empty result set | the report renders stating there is no data for the parameters |
| Export failure | retained parameters, retry offered |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Run report | `reporting.run` + the source area's read permission | no |
| Edit template | `reporting.template.write` | yes |
| Export | `data.export` | file |

## 7. Acceptance Criteria

- AC-1 A report runs with parameters and previews with pagination.
- AC-2 An unknown token fails rendering with the token named.
- AC-3 An empty result renders an explicit "no data" statement.
- AC-4 Report access requires the source area's read permission.
- AC-5 Exported PDF matches the preview.
- AC-6 Templates are versioned and default template selection is explicit.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~RPT-01"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-18-RPT-01/observed-evidence.json).

**The open question is answered by writing the list down.** *"The Web has no unified report catalogue; the desktop defines one. Record
the report list in §4 as it is implemented."* The catalogue is five reports, each with its parameters and the read permission of the
area it draws on, and the page's form is generated from the definition rather than typed per report:

| Key | Report | Area | Permission | Parameters |
|---|---|---|---|---|
| `wells-register` | Wells register | exploration | `wells.read` | field (optional) |
| `wellbores` | Wellbores of a well | drilling | `drilling.read` | well |
| `formation-picks` | Formation picks | stratigraphy | `stratigraphy.read` | well |
| `survey-stations` | Survey stations | stratigraphy | `stratigraphy.read` | well |
| `daily-reports` | Daily drilling reports | drilling | `drilling.read` | well, from, to |

**A substitution, stated rather than implied.** The plan names the ERP's `ErpDocumentTemplateService` and `InvoiceDocumentRenderer` for
template storage and rendering, and those projects are **not in this repository** — so the catalogue, the `{{Token}}` renderer and the
template store are implemented in the desktop, with the templates as files under the data root.

**AC-2 is all-or-nothing.** An unknown token — including an **unclosed marker** — fails the render and **every offending name is
listed**; a document that silently drops a token would be quietly wrong.

**AC-3 needs words, not an empty table.** The no-data sentence **names the parameters asked**, so a reader can tell "nothing matched"
from "the report broke".

**AC-4 uses the permission model's own distinction.** `OgPermissions` separates **"the grant lookup failed"** from "granted nothing", so
an unreadable permission source is a **refusal with its own sentence** rather than a silent empty grant.

**AC-5 found that no text PDF exists in the referenced owners.** Every writer draws something specific — a well document, a
`DrawingEngine`, a map — so `OgReportPdf` is a small Skia text writer implementing **`IOgTextPdf`, declared in Core**, which lets the
reporting service and the drawing layer meet without referencing each other. The PDF is the preview's pages **by construction**, and the
gateway test asserts its page objects equal the preview's page count.

**Not built, and stated as gaps:** templates per report (one document is used by whichever report is run); template version history (the
latest body is kept with its version number); a control per parameter (the form is `key = value` lines); a searchable PDF text layer
(drawn, not asserted); a report the page can add; RBAC rows for the permission strings (a non-administrator without them is refused, as
intended); a laid-out document (the preview is fixed-width text); overwrite confirmation on export; and scheduling, which the feature
scopes out.

## 9. Open questions and risks

- The Web has no unified report catalogue; the desktop defines one. Record the report list in §4 as
  it is implemented.
