# Reproduce the scoped component transport check

Run the recorded focused gate from the repository root. Use the PurchaseOwnership.dll path emitted by its Test run for line as FixtureAssemblyPath:

```powershell
./Scripts/Start-WorkflowProtectedTab.ps1 -FixtureAssemblyPath <focused-runtime>/PurchaseOwnership.dll
```

The script creates a dedicated temporary Web SDK host and prints its local loopback URL. It uses actual recorder components/state/journal/transport from that focused runtime, controlled native-approved fixture data and mocked typed-client business outcomes. It does not run the production Web/IdentityServer/API/provider host. Do not expose the diagnostic host externally.

Open the printed URL with the installed Playwright CLI and a dedicated browser session. Snapshot before using element references. `/fixture/metadata` supplies the controlled UTC timestamps, unit, role and counters. Read its raw JSON: PowerShell object projection can reserialize timestamp strings with the local offset. Fill the actual report form, freeze it and confirm. The controlled client records a model row and throws a lost acknowledgement; the actual app retains/reconciles its original. Switch to the read-only unmapped profile and reload the browser. Verify the pending original loads without another source/profile lookup, then recover its exact receipt. Retain the start through the diagnostic control to exercise its separate actual journal and actual start recovery component. Company/account controls exercise the actual client-state fences with controlled source refusals.

Use owned `oilgas-test-quota-` session-storage filler keys only in this isolated origin to reach actual quota; verify record counters do not advance on retention failure, confirm absence, then remove only those filler keys. To test corrupted proof, preserve that owned ciphertext in browser memory, replace only its journal value, reload the component and verify unavailable state without source/write calls. Restore the same ciphertext, recover and clear it. A new tab with no opener should not receive the parent's retained original. This does not qualify duplicated tabs/opener cloning or concurrent circuits.

`browser/browser-verification.json` identifies twelve saved-observation assertions and their source snapshots/metadata. These assertions check captured browser observations; they do not substitute for driving the browser. The current final focused gate and browser checks pass. Baseline/construction failures are separate logs. The selected browser is local Headless Chromium 151, 1280x720. The original standalone baseline used framework ProtectedSessionStorage with the default component registration and disconnected on its large read; its retained/circuit-error snapshot is under browser/.

All test-owned browsers and temporary hosts were stopped after collection. No production database, domain migration, SQL, business EF or external messages were used. Full host/OIDC/RBAC/route guards, provider behavior, branding, browser engines/mobile/RTL/a11y/complete keyboard acceptance and production load remain open. Memory-only retention feedback after quota failure remains an app UX task.
