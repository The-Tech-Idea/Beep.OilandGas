# Website well comparison - 2026-09-21

## Implemented

The authenticated /ppdm39/wells/compare workflow now uses the existing CompareWellsRequest contract rather than an anonymous request. It trims UWIs and requires at least two distinct identifiers before calling the API. The page explicitly states that comparisons use supplied UWIs and are not filtered by the active field; the API accepts no field filter. Removed the separate field-name lookup and misleading field attribution from exports.

WellComparisonState holds the completed request identifiers with its results. Editing inputs clears the result and disables export. Starting a comparison clears prior results, and safe inline failure messages replace silent stale results. A response must contain exactly the requested identifiers before it becomes available for display/export. Empty attribute sets have explicit feedback. Field changes clear both input and results; late responses and responses after disposal cannot restore them. Export failures receive feedback.

## Verification

147 website tests passed, zero failed, including eight new cases for duplicate/missing identifiers, trimmed request snapshots, failure clearing, partial responses, context reset, disposal and competing requests. The test build compiles the actual Razor page. Evidence: Beep.OilandGas.Web.Auth.Tests/TestResults/well-comparison-website.trx and %TEMP%/beep-well-comparison-tests.log. Whitespace check passed. Existing dependency/analyzer warnings remain.

API tests were not rerun for this website-only change. State tests do not establish browser interaction or backend well-resource authorization. Live browser/OIDC and download acceptance remain open. Verify two real UWIs, edit one identifier and confirm old export is unavailable, compare again and inspect exported identifiers, then switch fields during a delayed comparison. Other workbenches remain to be audited.
