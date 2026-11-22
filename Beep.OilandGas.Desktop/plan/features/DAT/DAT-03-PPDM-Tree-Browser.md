# DAT-03 — PPDM tree browser

| Field | Value |
|---|---|
| Stage | STG-01 — Data platform and PPDM core |
| Area | Data |
| Status | In progress - AC-1, AC-2, AC-3, AC-4, AC-5 and AC-6 met: the tree builds 2712 tables in 56 categories from the registry, filters by name and description, reads no row count on open, marks restricted tables, fails rather than showing an empty tree, and **opens the workbench with the same table and filter** (shell navigation, verified in the smoke). See [evidence](../../evidence/STG-01-DAT-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.Branchs`, `Beep.OilandGas.PPDM39` |
| Desktop project | `src/Beep.OilandGas.Desktop.Data/Ppdm/`, `Modules/Data/PpdmTree/` |
| External packages | `Beep.TreeNodes`, `Beep.Vis.Modules` (via Branchs) |
| Depends on | DAT-02 |
| Blocks | DAT-04, many domain pages that deep-link |
| Web reference | `Components/PPDMTreeView.razor`, `Components/Data/DataModuleTreeView.razor`, `Pages/PPDM39/DataManagementHub.razor` |

## 1. Purpose

The spatial index of the database: browse every PPDM table by category, with counts, and open a
table or a well from the tree.

## 2. Source components

| Source | Reuse |
|---|---|
| `Beep.OilandGas.Branchs/PPDM39/PPDM39RootNode.cs`, `PPDM39CategoryNode.cs`, `PPDM39TableNode.cs` | node model |
| `Beep.OilandGas.Branchs/BusinessProcess/BusinessProcess*Node.cs` | process subtree |
| `Beep.OilandGas.Branchs/Data/PPDM39*.json`, `ppdm39TableRelations.csv` | category and relationship data |
| `PPDM39/Core/Tree/IPPDMTreeBuilder.cs`, `PPDMTreeNode.cs` | tree construction |
| `PPDM39/Core/Metadata/IPPDMMetadataRepository.cs` | table metadata |
| `Web/Services/DataManagementService.cs` | tree + metadata shape |

## 3. Screens

`Data → PPDM browser`

- Left: tree (categories → tables; business-process subtree as a second root).
- Right top: table summary — row count, key columns, related tables, FK edges.
- Right bottom: relationship graph (simple two-level, not a full ERD).
- Search box filters the tree by table name or description.
- Context menu: Open in workbench, Copy key list, Show relations.

## 4. Data and contracts

```csharp
public interface IOgPpdmTreeBuilder
{
    Task<OgResult<OgTreeNode>> BuildAsync(OgTreeOptions options, CancellationToken ct);
    Task<OgResult<OgTableSummary>> SummarizeAsync(string tableName, CancellationToken ct);
}
```

`OgTreeNode`: `Key`, `Title`, `Kind` (`Root|Category|Table|BusinessProcess`), `Count`,
`Children`, `TableName`.

## 5. States

| State | Behaviour |
|---|---|
| No active connection | empty state linking to DAT-01 |
| Table list unavailable | failed, not an empty tree |
| Count query slow | show node with "count pending" and fill asynchronously |
| Policy filters rows | node shows `n of m` and a policy note |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| Browse | any signed-in user with `data.read` |
| Open in workbench | `data.read` |
| See system tables | `data.system.read` |

## 7. Acceptance criteria

- AC-1 Node count equals the registry count for the active connection.
- AC-2 Search finds a table by name and by description.
- AC-3 Opening a table navigates to DAT-04 with the same filter context.
- AC-4 A failed metadata read shows failed, never an empty tree.
- AC-5 Row counts are not fetched for every node on first open (performance).
- AC-6 Policy-restricted tables are hidden or marked, never silently missing.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.IntegrationTests --filter "FullyQualifiedName~DAT-03"
```

## 9. Open questions and risks

- `PPDM.Models` has 2 740 entity files; the tree must be virtualised and lazily populated.
- `Beep.Vis.Modules` is a WinForms-era tree library. If it cannot be referenced from WPF, use a
  plain `TreeView` with the `PPDM39*Node` data model only.
