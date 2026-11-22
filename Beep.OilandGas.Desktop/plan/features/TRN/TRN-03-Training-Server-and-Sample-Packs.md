# TRN-03 — Training server and sample packs

| Field | Value |
|---|---|
| Stage | STG-18 — Training, reporting and release |
| Area | Training |
| Status | Gate met - see [evidence](../../evidence/STG-18-TRN-03/observed-evidence.json) |
| Owner projects | `TheTechIdea.OilGas.Training` |
| Desktop project | `Modules/Training/Server/` |
| External packages | `TheTechIdea.OilGas.Training` |
| Depends on | TRN-02 |
| Blocks | — |
| Web reference | `OilandGasTools/docs/customer/training.html`; `wellbore-config training-serve` |

## 1. Purpose

Offer two things: the built-in sample courses so a new installation can demonstrate the product
immediately, and an optional local training server for trainees on other machines.

## 2. Source components

| Source | Reuse |
|---|---|
| `Training/SamplePacks.cs` | built-in sample courses |
| `Training/TrainingServer.cs`, `TrainingServerOptions` | local web server |
| `Training/Course.cs` | course pack shape |
| `tools/config-cli` verb `training-sample-pack` | parity for generating packs |
| `Training/ScenarioLibrary.cs` | folder loading |

## 3. Screens

`Training → Server and packs`

- Sample packs: list with "install into my scenarios folder" action.
- Server panel: enabled, port, bind address, allowed trainees, start/stop, status, URL to share,
  active sessions.
- Pack import/export: zip a folder of scenarios and courses.

## 4. Data and contracts

`OgTrainingServer { Running, Url, Port, Sessions[] }`
The server is local-only by default; binding to a network interface requires explicit confirmation.

## 5. States

| State | Behaviour |
|---|---|
| Port in use | refused with the port named |
| Server running | status, URL and session count visible |
| Sample pack already installed | offers to reinstall with a version check |
| Bind to all interfaces | explicit warning about network exposure |
| Server failure | reported with a reference; state returns to stopped |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Install sample pack | `training.admin` | files |
| Start/stop server | `training.admin` | no |
| Export pack | `training.admin` | file |

## 7. Acceptance Criteria

- AC-1 Sample packs install into the configured scenario folder and appear in TRN-01.
- AC-2 The server starts on a free port and stops cleanly.
- AC-3 Binding beyond localhost requires explicit confirmation.
- AC-4 A port conflict is reported with the port.
- AC-5 Server sessions are visible and countable.
- AC-6 Exporting and re-importing a pack preserves scenarios and courses.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~TRN-03"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-18-TRN-03/observed-evidence.json).

**The pack is the tools' and the server is the tools'.** `SamplePacks.Write` into the folder `training.scenarioFolders` names — the
folder `ScenarioLibrary.LoadFolder` reads — and `TrainingServer` over `HttpListener`. The page adds where the pack goes, the zip round
trip, the exposure decision, the port check and the session view.

| What the page shows | Where it comes from |
|---|---|
| The pack install, and whether it was a reinstall | `SamplePacks.Write`, then the engine's loaders |
| The server's address, port, running state | `TrainingServer.Url` and the service's own record |
| The exposure sentence for the bound address | `TrainingServer.IsLoopback` |
| The trainees seen, with what each did | the request observer, plus the ledger's attempts |
| The pack file | a zip of the exercise files |

**AC-5 had nothing to read.** The server **tracks no sessions at all** — there is no connection or trainee registry, and the name lives
in the browser's `sessionStorage` and rides each request. So `TrainingServerOptions` gained an **additive `Observe` callback** (an
observer that throws is caught, so it can never break a request), and the page defines a session as **a name the server has heard
from**, with its request and attempt counts — named for what it is rather than dressed up as authentication.

**AC-4 avoids the exception the engine raises.** `TrainingServer.Start` lets a held port surface as an `HttpListenerException` whose
message need not name the port, so the service **asks the operating system's own listener list first** and refuses with the port named
**before anything is opened** — with the runtime's refusal still caught for the case the list cannot predict (an address needing a URL
reservation), the state returning to stopped.

**AC-6 found a leak worth closing.** The ledger's default home can easily be the scenario folder, so a zip of that folder would carry
**every trainee's attempts** to another machine. The export selects the exercise files by name and **excludes `results.json`**, which the
test proves by planting one beside a stray text file.

**The feature's risk note is answered rather than noted.** An embedded web server in a desktop application is unusual, so it is **off
until it is asked for**, bound to **loopback unless the exposure is confirmed**, and the page prints the security sentence for whichever
address is in force — in those words, because there is no sign-in.

**One environment fact is recorded.** On this machine `HttpListener` on loopback starts without an administrator URL reservation, so the
gateway test asserts the server is **actually running** rather than skipping; a machine that refuses the bind cannot verify this feature,
and saying so is better than a green tick over an untested state.

**Not built, and stated as gaps:** a URL reservation helper (an administrator step, not a button); TLS; a session that survives a restart
(the list is cleared on start); subfolders in a pack; an overwrite prompt on import (a file of the same name is replaced); a background
start; live update of the courses page when the server records an attempt; and separate `training.admin` gating.

## 9. Open questions and risks

- An embedded web server inside a desktop app is unusual; consider making it opt-in and off by
  default, and state the security implication in the UI.
