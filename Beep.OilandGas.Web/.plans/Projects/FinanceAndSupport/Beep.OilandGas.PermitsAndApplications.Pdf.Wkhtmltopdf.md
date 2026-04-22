# Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf

## Snapshot

- Category: Finance and support
- Scan depth: Medium
- Current role: permit document rendering helper
- Maturity signal: narrow infrastructure helper

## Observed Structure

- Root is intentionally small
- Main implementation files are `WkhtmltopdfRenderer.cs` and `WkhtmltopdfPacketWriter.cs`

## Representative Evidence

- Rendering implementation: `WkhtmltopdfRenderer.cs`
- Packet/output support: `WkhtmltopdfPacketWriter.cs`

## Planning Notes

- This project should stay an implementation detail behind permit/report rendering flows.
- Phase 9 should reference it only where permit export or document output becomes a user-visible requirement.
