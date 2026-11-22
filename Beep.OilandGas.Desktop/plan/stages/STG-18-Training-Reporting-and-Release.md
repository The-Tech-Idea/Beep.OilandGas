# STG-18 — Training, reporting and release

| Field | Value |
|---|---|
| Stage | STG-18 |
| Features | 11 (TRN-01…03, RPT-01…03, REL-01…05) |
| Depends on | all previous stages |
| Status | Gate met for what a build can prove - REL-02, RPT-03 and REL-04 gate met; the training module is complete (TRN-01, TRN-02, TRN-03); the reporting module is complete (RPT-01, RPT-02); REL-01, REL-03 and REL-05 gate met for what a build can show. **Release is not claimed:** the clean-VM run, the screen-reader and 200 % observations and the thirty release attestations are the owner's items, recorded in the evidence. |
| Exit gate | An installer produces a signed build that passes the release acceptance checklist |

---

## 1. Objective

Ship it. Three things are bundled here because they are all "last mile": the training module
(which is a product capability but not on the operational critical path), reporting and printing,
and the release engineering work that turns a working build into a deliverable.

---

## 2. Scope

**In — training**

- Scenario runner: load a scenario, present the view, score an answer, show the solution.
- Course packs, attempt policy, scoring, results ledger.
- Optional local training server and the built-in sample packs.

**In — reporting**

- Report catalogue with parameter forms and a viewer.
- Document templates (invoice/permit style) and the token renderer.
- Print packs and batch export across wells.
- Chart rendering through `IRenderingService` into the Skia host.

**In — release**

- Packaging and installer, including native Skia and `wkhtmltopdf` assets.
- Licensing and entitlement with grace period.
- Accessibility and keyboard completeness.
- Performance budgets and large-data handling.
- Release acceptance gates and the evidence pack.

**Out**

- Auto-update (proposed for 1.1).
- Cloud licence activation (file-based only in 1.0).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `TheTechIdea.OilGas.Training` | `ScenarioLibrary`, `Scenario`, `ScenarioScoring`, `ScenarioRender`, `CourseLibrary`, `CoursePack`, `CourseResult`, `ResultsLedger`, `SamplePacks`, `MapExhibit`, `TrainingServer`, `TrainingToolbox` |
| `Beep.OilandGas.Models` | `IRenderingService`, `RenderChartRequest/Result`, `CHART_CONFIGURATION` |
| `Beep.OilandGas.PPDM39.DataManagement` | Report sources, document template storage |
| `Beep.OilandGas.Drawing` | `ImageExporter`, `SvgExporter`, `PdfExporter`, `GeoJsonExporter` |
| `TheTechIdea.OilGas.Wellbore` | `WellboreExport`, `PanelExport`, `PrintPlanner` |
| `Beep.OilandGas.Diagnostics` | Failure store used in the release acceptance evidence |
| Reference `ErpLicense`, `ErpEntitlementService`, `ErpBackupService` | Licensing and backup patterns |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| TRN-01 | Scenario runner | [TRN-01](../features/TRN/TRN-01-Scenario-Runner.md) |
| TRN-02 | Courses, scoring and results ledger | [TRN-02](../features/TRN/TRN-02-Courses-Scoring-and-Results-Ledger.md) |
| TRN-03 | Training server and sample packs | [TRN-03](../features/TRN/TRN-03-Training-Server-and-Sample-Packs.md) |
| RPT-01 | Report catalogue and document templates | [RPT-01](../features/RPT/RPT-01-Report-Catalogue-and-Document-Templates.md) |
| RPT-02 | Print packs and batch export | [RPT-02](../features/RPT/RPT-02-Print-Packs-and-Batch-Export.md) |
| RPT-03 | Chart rendering service | [RPT-03](../features/RPT/RPT-03-Chart-Rendering-Service.md) |
| REL-01 | Packaging and installer | [REL-01](../features/REL/REL-01-Packaging-and-Installer.md) |
| REL-02 | Licensing and entitlements | [REL-02](../features/REL/REL-02-Licensing-and-Entitlements.md) |
| REL-03 | Accessibility and keyboard | [REL-03](../features/REL/REL-03-Accessibility-and-Keyboard.md) |
| REL-04 | Performance and large-data handling | [REL-04](../features/REL/REL-04-Performance-and-Large-Data.md) |
| REL-05 | Release acceptance gates | [REL-05](../features/REL/REL-05-Release-Acceptance-Gates.md) |

---

## 5. New code introduced

`.Modules/Training`, `.Modules/Reporting`; host `Security/OgEntitlementService`,
`OgLicenceVerifier`; `installer/` with a `.wixproj` or Inno Setup script; `plan/evidence/STG-18-*`.

---

## 6. Architecture notes

- **Training is isolated.** A scenario opens its own `WellboreDocument`; it never mutates the
  operator's real well data.
- **Reports read; they do not compute.** A report that needs a calculation calls the owning
  service first, then formats the result.
- **Print is a first-class path.** `PrintPlanner` decides pagination; the desktop must not
  re-paginate.
- **Licence check fails closed for writes, open for reads** with a visible banner — the ERP
  behaviour (`IsLicensed`, `IsExpired`, `IsInGrace`, `IsUsable`).
- **No telemetry leaves the machine** unless the operator opts in and the privacy notice in
  `Beep.OilandGas.Web/Pages/Legal/Privacy.razor` covers it. Default: off.

---

## 7. Dependencies and sequencing

TRN-01 → TRN-02 → TRN-03. RPT-01 → RPT-02; RPT-03 independent. REL-01 after REL-02,
REL-03, REL-04. REL-05 last.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-18-1 | A scenario loads, is scored, and shows the solution | test `TRN-01/T03` |
| SG-18-2 | Results ledger enforces the attempt policy | test `TRN-02/T04` |
| SG-18-3 | A report renders with parameters and exports to PDF | test `RPT-01/T03` |
| SG-18-4 | Batch export produces one file per well and a manifest | test `RPT-02/T02` |
| SG-18-5 | Every chart type renders through `IRenderingService` | test `RPT-03/T02` |
| SG-18-6 | The installer installs, launches, and the smoke run passes on a clean machine | install log + smoke result |
| SG-18-7 | An expired licence blocks writes and allows reads with a banner | test `REL-02/T03` |
| SG-18-8 | All interactive controls are keyboard reachable, with a visible focus ring | accessibility check |
| SG-18-9 | Performance budgets from REL-04 are met | timing capture |
| SG-18-10 | The acceptance checklist is complete with evidence links | signed checklist |

---

## 9. Test plan

`TRN-01/T03`, `TRN-02/T04`, `RPT-01/T03`, `RPT-02/T02`, `RPT-03/T02`, `REL-02/T03`,
`REL-03/T02`, `REL-04/T01…T05`, `REL-05/T01`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| Native assets (Skia, wkhtmltopdf) complicate the installer | Test on a clean VM early in REL-01, not at the end |
| Licensing is a commercial decision not yet made | Implement the interface and file format; keep enforcement permissive until decided |
| 40-culture localization coverage is large | Prioritise the top 8 cultures for 1.0; track the rest in REL-05 |
| Privacy/telemetry wording must match the existing Web notice | Reference `Pages/Legal/Privacy.razor`; do not invent new terms |
