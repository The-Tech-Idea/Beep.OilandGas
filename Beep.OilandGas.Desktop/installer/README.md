# Installer

REL-01. Beep Oil & Gas installs **per user**: no elevation, no machine-wide folder, and no writes
outside the per-user data root. That is what lets a standard user install it (AC-6) and what makes an
uninstall able to keep a customer's data (AC-4).

| | |
|---|---|
| Program files | `%LOCALAPPDATA%\Programs\TheTechIdea\BeepOilGas` |
| Writable data | `%LOCALAPPDATA%\TheTechIdea\BeepOilGas` (see `OgPaths`) |
| Start-menu shortcut | `%APPDATA%\Microsoft\Windows\Start Menu\Programs\TheTechIdea` |
| Start-menu shortcut | `%APPDATA%\Microsoft\Windows\Start Menu\Programs\TheTechIdea` - installer only, removed by the uninstall, `-NoShortcut` to skip |
| Nothing is written to | `Program Files`, `ProgramData`, the install folder, or the roaming profile during normal use |

## Install

```powershell
dotnet publish ..\src\Beep.OilandGas.Desktop -c Release -r win-x64 --self-contained false -o .\publish
.\install.ps1 -Start
```

`install.ps1` refuses a payload that is missing `TheTechIdea.Beep.OilandGas.Desktop.exe` or
`libSkiaSharp.dll`, because without either the application cannot draw (see the health page). It
records the installed version under the data root, so the next run knows it is an upgrade.

## Uninstall

```powershell
.\uninstall.ps1                 # program files go, the data root stays
```

The data root holds the failure store (`Diagnostics\failures.db`), the logs (`Logs`), the local
database, the environment and the exports. Removing it needs the exact sentence that names the path:

```powershell
.\uninstall.ps1 -RemoveDataRoot -Confirmation "REMOVE C:\Users\you\AppData\Local\TheTechIdea\BeepOilGas"
```

## What is verified where

| Criterion | How it is met | Where it is evidenced |
|---|---|---|
| AC-1 clean VM installs, launches, completes setup | the per-user install above, then the setup wizard | **The owner's step: a clean VM.** The behaviour here is a developer machine, which the feature itself says is not evidence. |
| AC-2 no writes beside the executable, in the install folder, ProgramData or the roaming profile | every writable path resolves under `OgPaths.DataRoot`, and the probe refuses a data root inside the install folder | `InstallationTests` (guard + report), `OgPathsTests`, `InstallationGatewayTests` |
| AC-3 every required asset present and reported | the health page and its report | `InstallationTests.Every_required_asset_is_present…`, `installation-report.json` |
| AC-4 uninstall keeps the data root unless the exact words are typed | `uninstall.ps1` and the page refuse without the token | `InstallationTests`, `InstallationGatewayTests` |
| AC-5 an upgrade preserves the data root and the setup state | `install.ps1` never writes into the data root except the version record; the plan reads and writes nothing | `InstallationGatewayTests.Upgrade_preserves…` |
| AC-6 a non-admin user can install and run | `app.manifest` requests `asInvoker`; both folders are the user's own | `InstallationTests` (manifest and script source checks) |

## Open items that are not code

- **Code signing.** Without a certificate, SmartScreen shows "unknown publisher". The build is unsigned.
- **`wkhtmltopdf`.** The permits PDF renderer is not part of this build; the health page lists it as
  *not installed* and says which feature loses its PDF. Redistribution terms must be checked before it
  is ever bundled.
