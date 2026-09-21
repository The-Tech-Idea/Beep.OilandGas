# Website persona workspace navigation - 2026-09-21

## Visible website changes

The header now offers My workspace for the active persona, using its configured application home route. Workspace selection, the navigation link and the dashboard persona badge react to the same context changes. A persona preference personalizes navigation; it does not add roles or bypass page/API authorization.

The selector uses the existing PersonaContextService as its profile/catalog source instead of independently loading another profile. Failed loads show Retry personas. Failed saves ask the user to reload authoritative settings before retrying. Once saving succeeds, a notification reconnection failure displays a separate warning and preserves the successful selection, rather than claiming the persona was not changed. Inactive catalog entries are excluded.

Workspace destinations must be local root-relative paths. External, protocol-relative, encoded and backslash/control-character paths return to the dashboard. The active persona menu is refreshed on save/reload and cleared on authentication changes. The dashboard remains usable when persona loading fails and its label updates without requiring a fresh page instance.

## Supporting framework

PersonaContextService serializes concurrent loads, publishes changes and rejects stale profile/catalog responses after sign-out, reload or disposal. It clears the prior profile before reload so a failed request cannot preserve another session's persona. Profile writes retain the existing concurrency stamp and preference values. Empty transport payloads are errors; the explicit nullable profile envelope still represents a valid absent profile.

No changes were made to IdentityServer or app-role grants. The existing standard ASP.NET authorization mechanisms remain the authority. No new navigation catalog, authorization system, CSS file or JavaScript path was introduced.

## Validation

Website test project: **112 passed, zero failed**. Nine new regressions cover saved profile publication without granting roles, inactive selection rejection, late responses after sign-out, failed reload/retry, and safe workspace destinations. Existing 103 website/authentication tests remain passing. The test build compiles the updated selector, navigation and dashboard Razor components.

Command: `dotnet test Beep.OilandGas.Web.Auth.Tests/Beep.OilandGas.Web.Auth.Tests.csproj --no-restore -v quiet --logger 'trx;LogFileName=persona-workspace.trx'`. Evidence: Beep.OilandGas.Web.Auth.Tests/TestResults/persona-workspace.trx; log `%TEMP%/beep-persona-workspace.log`. Whitespace check passed. Existing build warnings remain. Prior API test results are unchanged; the API suite was not rerun for this website-only slice.

Live OIDC/browser, responsive header overflow and keyboard/screen-reader acceptance remain open. The tests exercise context/client behavior and Razor compilation, not rendered browser interactions or notification failure presentation. Next website work should verify the persona-to-workspace journey in the running app and improve the selected workbench, keeping role enforcement independent of persona choice.
