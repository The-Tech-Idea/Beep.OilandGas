# REL-02 — Licensing and entitlements

| Field | Value |
|---|---|
| Stage | STG-18 — Training, reporting and release |
| Area | Release |
| Status | Gate met - signed licence (RSA-2048/SHA-256), entitlement as the second gate, expiry/grace with write blocking, licence page and shell banner. See [evidence](../../evidence/STG-18-REL-02/observed-evidence.json) |
| Owner projects | none (desktop host), following the ERP pattern |
| Desktop project | `src/Beep.OilandGas.Desktop/Security/` |
| External packages | `System.Security.Cryptography.ProtectedData` |
| Depends on | FND-03 |
| Blocks | REL-01, REL-05 |
| Web reference | `Beep.DoitAll.Erp.Wpf/Security/ErpLicense.cs`, `ErpEntitlementService.cs`, `ErpLicenseVerifier` |

## 1. Purpose

Gate commercial modules by entitlement, always as a *second* gate after roles — and never let a
licence problem corrupt data.

## 2. Source components

| Source | Reuse |
|---|---|
| ERP `Security/ErpLicense.cs`, `ErpLicenseVerifier.cs` | signed licence file, verification |
| ERP `Services/ErpEntitlementService.cs` | `IsLicensed`, `Edition`, `IsExpired`, `IsInGrace`, `IsUsable`, `StatusText`, `Allows(moduleKey)` |
| ERP `Modules/ErpModuleKeys.cs` | `OgModuleKeys` |
| ERP `--entitlement-report` and `--licence-check <file>` verbs | diagnostics |
| `Beep.OilandGas.UserManagement` personas/roles | the **first** gate |

## 3. Screens

- `Settings → Licence`: edition, customer, modules, expiry, grace state, seats in use, import licence.
- Shell banner: expired / grace / unlicensed with the affected modules.
- `--entitlement-report` console output.
- `--licence-check <file>` verification without starting the UI.

## 4. Data and contracts

```csharp
public sealed record OgEntitlement(string Edition, string Customer, string[] Modules,
    DateTimeOffset? ExpiresAt, bool IsLicensed, bool IsExpired, bool IsInGrace, bool IsUsable,
    string StatusText, string? Error);

public interface IOgEntitlementService { OgEntitlement Current { get; } bool Allows(string moduleKey); }
```

## 5. States

| State | Behaviour |
|---|---|
| No licence | core modules only; a banner explains |
| Expired | **writes blocked, reads allowed**, banner; recovery is import a valid licence |
| In grace | everything works, banner with days remaining |
| Corrupt signature | treated as no licence; the reason is shown, not the exception |
| Seat limit reached | sign-in refused with the seat count |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Import licence | Administration + local confirmation | file |
| View licence | any user | no |

## 7. Acceptance Criteria

- AC-1 A module is available only when both the entitlement and the role allow it.
- AC-2 An expired licence blocks writes and permits reads, with a visible banner.
- AC-3 An invalid signature is treated as unlicensed with a stated reason.
- AC-4 The entitlement report lists edition, customer and modules exactly as the file says.
- AC-5 A licence never contains a plaintext secret.
- AC-6 Changing a licence takes effect without reinstalling.

## 8. Verification

```
dotnet run --project src/Beep.OilandGas.Desktop -- --entitlement-report
dotnet run --project src/Beep.OilandGas.Desktop -- --licence-check <file>
```

## 9. Open questions and risks

- Commercial terms are undecided. Implement the interface and file format, default to permissive, and
  keep the enforcement switchable until the business decides.

### Delivered 2026-11-06

**The file is signed, not just readable.** A licence is an envelope:

```json
{ "schema": 2, "keyId": "dev-2026", "payload": "<base64url of the claims JSON>", "signature": "<base64url>" }
```

The signature is RSA-2048 / SHA-256 (PKCS#1) over the payload's **exact bytes**, so issuer and verifier never
have to agree on a serializer. The trusted public key is an **embedded resource**, never a file the operator
can edit — a trust anchor in a writable folder is not a trust anchor. `--licence-check` now verifies it and
names the key; an unsigned or altered file exits 2 with `licence-unsigned` / `licence-signature-invalid`.

**Two gates, in the right order.** `OgEntitlement.Allows(moduleKey)` is the commercial gate, asked by
`OgNavigationService` *after* roles. A module the licence does not grant is denied even for a system
administrator, which the tests assert in both directions.

**States, as implemented:**

| State | Behaviour |
|---|---|
| No licence, enforcement off (default) | every module available; the status says enforcement is off |
| No licence, enforced | core only; shell banner explains |
| Expired, inside grace | everything works; the banner counts the grace days |
| Expired, past grace, enforced | **reads continue, writes blocked**; banner names the expiry |
| Corrupt / altered / unknown key | treated as unlicensed, with the code shown; the install is untouched |
| Import of an unverifiable file | refused; the installed licence keeps working |

The fallback is deliberate: losing access to the customer's own records over a licence would be a worse
failure than the licence. Core is never blocked, so licensing cannot break the product's own housekeeping.

**Changing a licence needs no restart.** `Import` copies the file, re-reads it and raises `Changed`; the shell
subscribes, rebuilds the navigation and re-evaluates the banner. Enforcement is a setting
(`licence.settings.json`) and each change writes one audited `licence.enforcement` row with the previous value.

**Where it lives:** `Security/OgLicenceVerifier.cs` (format + verification), `Security/OgLicenceService.cs`
(state, import, enforcement), `Core/OgEntitlement.cs` (the gate), `Views/LicencePageView.cs` (route `licence`).

**Key custody — a release action, not a code action.** The repository contains a **development** key pair:
the public half is embedded in the application, the private half is a test fixture used to sign test licences.
The production vendor key must replace `Security/licence-keys.json` before release, and its private half must
never enter this repository. A guard test fails the build if any file under `src` contains `PRIVATE KEY`.

### Still open

- **Seat limits are read, not enforced.** The licence carries `seats`, the page shows it, and sign-in does not
  yet refuse beyond it (§5's seat state). That needs the local user store's active-session count, which FND-07
  does not keep.
- **The banner was not seen in a shown window.** Its visibility rule and text are computed from the
  entitlement and covered by the state tests; no screenshot exists.
- **No licence editor or issuance tool.** Licences are signed outside the product; a vendor-side signer is a
  release task.
- The development key id is `dev-2026`; a production licence signed with any other key is refused until the
  real key is embedded.
