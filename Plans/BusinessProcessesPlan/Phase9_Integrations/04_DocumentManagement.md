# Phase 9 — Document Management Integration
## SharePoint / OpenText → RM_INFORMATION_ITEM Sync

---

## IDocumentManagementAdapter Interface

```csharp
public interface IDocumentManagementAdapter
{
    Task<DocumentSyncResult> SyncFolderAsync(
        string folderPath, string fieldId, string userId);

    Task<DocumentSyncResult> SyncDocumentAsync(
        string documentId, string fieldId, string userId);

    Task<List<DocumentSummary>> GetAvailableDocumentsAsync(
        string folderPath, DateTime? modifiedAfter = null);

    Task<byte[]> GetDocumentContentAsync(string documentId);
}

public record DocumentSyncResult(
    bool Success, int RecordsWritten, int RecordsUpdated, string? ErrorMessage);

public record DocumentSummary(
    string DocumentId, string Name, string ContentType, long SizeBytes,
    DateTime LastModified, string Author, string Folder);
```

---

## RM_INFORMATION_ITEM Column Mapping

| SharePoint/OpenText Field | PPDM Column | Notes |
|---|---|---|
| Document ID | `RM_INFORMATION_ITEM.INFO_ITEM_ID` | Use DMS ID; prefix with adapter code |
| Title / Name | `RM_INFORMATION_ITEM.ITEM_NAME` | |
| Content Type | `RM_INFORMATION_ITEM.ITEM_TYPE` | e.g., `WELL_REPORT`, `DRAWING`, `PROCEDURE` |
| File extension | `RM_INFORMATION_ITEM.INFO_ITEM_SUBTYPE` | `PDF`, `XLSX`, `DWG` |
| Library URL / path | `RM_INFORMATION_ITEM.SOURCE_DOCUMENT_URL` | Full DMS URL |
| Author | `RM_INFORMATION_ITEM.AUTHOR_BA_ID` | Lookup by name → `BUSINESS_ASSOCIATE.BA_ID` |
| Modified date | `RM_INFORMATION_ITEM.EFFECTIVE_DATE` | |
| Field association | `RM_INFORMATION_ITEM.FIELD_ID` | Set from `fieldId` parameter |
| Related entity | `RM_INFORMATION_ITEM.RELATED_TABLE_NAME` | e.g., `WELL`, `EQUIPMENT` |
| Related entity ID | `RM_INFORMATION_ITEM.RELATED_IDENTIFIER` | |

---

## INFO_ITEM_SUBTYPE Mapping

| DMS Content Type or Extension | `INFO_ITEM_SUBTYPE` |
|---|---|
| Well completion report (.pdf) | `WELL_COMPLETION_RPT` |
| Drilling program (.docx) | `DRILLING_PROGRAM` |
| P&ID drawing (.dwg / .svg) | `PID_DRAWING` |
| HSE procedure (.pdf) | `HSE_PROCEDURE` |
| Inspection report (.pdf) | `INSPECTION_REPORT` |
| Financial report (.xlsx) | `AFE_REPORT` |

---

## SharePoint Configuration

```json
"Integrations": {
    "SharePoint": {
        "SiteUrl": "https://company.sharepoint.com/sites/OilGasOps",
        "TenantId": "{{env:SHAREPOINT_TENANT_ID}}",
        "ClientId": "{{env:SHAREPOINT_CLIENT_ID}}",
        "ClientSecret": "{{env:SHAREPOINT_CLIENT_SECRET}}",
        "LibraryName": "Documents",
        "SyncFolders": ["/WellReports", "/HSEDocuments", "/Drawings"]
    }
}
```

Uses `Microsoft.Identity.Client` for app-only OAuth 2.0 (`client_credentials` grant); no user token required.

---

## Sync Schedule

Documents synced via `IHostedService` daily at 03:00 UTC, pulling all DMS items with `LastModified > LastSyncTimestamp`. The `LastSyncTimestamp` is stored in `APP_CONFIG` table (custom) or `appsettings` override.

---

## Security Notes

- Only document **metadata** is stored in PPDM (`RM_INFORMATION_ITEM`); document binary content remains in SharePoint/OpenText
- `SOURCE_DOCUMENT_URL` links back to the authoritative copy
- Access to document download endpoint (`GET /api/integrations/documents/{id}/content`) requires `Manager` or `ComplianceOfficer` role to prevent unauthorized data exfiltration
