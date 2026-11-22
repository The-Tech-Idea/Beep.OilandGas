# SETUP-02 — Database connection (machine)

**Document ID:** SETUP-02
**Status:** Built (AC-7 blocked on an enterprise server)
**Actor:** Installer
**Level:** Machine

---

## 1. Purpose

Make one working connection to the server that holds the company database, or — for a personal installation — create
the local database file. Everything the company owns is reached through this connection, so it is proved open before
any company fact is written.

The connection is a machine fact. The credentials in it belong to the machine's service account, not to a person, and
they cannot be used to sign in to the application.

## 2. Actor

The installer, who knows the server name, port, database and service account.

## 3. Preconditions

- SETUP-01 chose a deployment.
- The driver for that database product is installed or can be downloaded from the wizard.
- For a company database: the database exists on the server and the service account has been granted access to it
  (creating the database itself is the DBA's job, not the wizard's).

## 4. Steps

1. **List what this build can reach.** The catalogue reads the configured connections and the installed drivers
   (`ProvidersAsync`). No product name is hard-coded.
2. **Choose one of two paths:**
   - *Connection wizard* — the shared connection editor (driver, host, port, database, authentication, TLS) saves a
     named connection.
   - *Enter the server directly* — provider, host, port, database, user, password, Windows authentication, trust
     certificate → `EnsureRemoteAsync` registers and opens it.
3. **Missing driver** — "Download driver…" and "Download data source…" install the package without leaving the
   wizard; the provider list is refreshed afterwards.
4. **Prove it.** `TestAsync` opens the connection and reads the table list. Opening is not enough: a connection that
   cannot list tables is not usable.
5. **Adopt the name.** At Review the chosen connection is renamed to the installation's own connection name
   (`OgPaths.ConnectionName`) so every service bound to that name reads the chosen database. A name already used by
   another definition is refused rather than overwritten.
6. **Personal path** — no server; `EnsureLocalAsync` creates the file through whichever installed driver advertises
   local creation, and `AdoptAsync` adopts an existing local file if one was chosen.

## 5. Records written

| Store | Record | Notes |
|---|---|---|
| BeepDM configuration (per machine) | one `ConnectionProperties` | `IsRemote=true`/`IsLocal=true` as appropriate, `SavePassword=true`; the password is protected by the host cipher (`__beepenc__:1:dpapi-user:`) — **never plain text** |
| — | nothing else is persisted | this connection is the only local fact; the database holds everything else |

## 6. Failure states

| Code | Meaning | Sentence |
|---|---|---|
| `connection-not-found` | the chosen name is not configured | "No connection named '…' is configured." |
| `connection-driver-missing` | no installed driver can open the product | "No installed driver can open X. Install the driver, then try again." |
| `connection-host-unreachable` | DNS, refused, timeout | "The server could not be reached. Check the host name, the port and the network." |
| `connection-credentials-rejected` | login failed / not authorised | "The server refused these credentials. Check the user and the password." |
| `connection-database-missing` | a file database's file is gone | "The database file is missing: …" |
| `connection-name-in-use` | the installation's name is taken by another definition | refused; the other definition is not replaced |
| `connection-in-use` | removal attempted while open | refused |
| `local-file-not-created` | the driver refused to create the file | the path and the driver name |

Provider text never appears on screen; the failure store keeps the detail and the screen shows the reference.

## 7. Security requirements

- **Credentials are not user accounts.** OWASP Authentication Cheat Sheet: a backend/database account must not be a
  front-end login. The service account in the connection cannot authenticate a person; sign-in is SETUP-09.
- Password at rest is protected by the host cipher; a connection with a plain-text password is refused by the store.
- TLS: `TrustServerCertificate` defaults to **false**; a person may opt out, and the opt-out is visible on the form.
- The connection is machine-scoped: a second workstation needs its own definition (there is no shared credential
  file on a network share).
- No connection string is written to a log or to any settings document.

## 8. UX requirements

- The provider list is what is installed, not a hard-coded list of products.
- A failed test names the next action (install the driver, check the host, check the credentials) rather than only
  the failure.
- The driver download is available in place, so a missing driver does not end the setup.
- The form says what happens to the password ("stored by the connection store with the host's own encryption").

## 9. Acceptance criteria

| # | Criterion | Evidence |
|---|---|---|
| AC-1 | A local file database is created, reopened by a fresh runtime, and its row is readable | `ConnectionCatalogIntegrationTests.Creates_reopens_and_reads_a_local_database` |
| AC-2 | Testing an unknown connection is refused with `connection-not-found` | `ConnectionCatalogIntegrationTests.Testing_an_unknown_connection_is_refused_with_a_reason` |
| AC-3 | A database whose type has no driver is refused with `connection-driver-missing` before the server is contacted | `BeepDmConnectionCatalog.EnsureRemoteAsync`, `OgConnectionFailureClassifier.HasDriverFor` |
| AC-4 | Removing an open connection is refused | `ConnectionCatalogIntegrationTests.Deleting_a_connection_that_is_open_is_refused` |
| AC-5 | The chosen connection is adopted under the installation name; a taken name is refused | `BeepDmConnectionCatalog.AdoptAsync` |
| AC-6 | The password is never stored in clear text | `OgBeepHost` connection store (`ConnectionCredentialProtection`), smoke `credential-has-no-plaintext` |
| AC-7 | A server connection is exercised against a real enterprise server | **Blocked on an environment** — there is no SQL Server/PostgreSQL/Oracle here, so the path is reviewed and compiled, not run (G-9) |

## 10. What exists today

- `src/Beep.OilandGas.Desktop.Data/Connections/OgConnectionCatalog.cs` — `ListAsync`, `ProvidersAsync`,
  `TestAsync`, `EnsureLocalAsync`, `EnsureRemoteAsync`, `AdoptAsync`, `RemoveAsync`.
- `src/Beep.OilandGas.Desktop.Data/Connections/OgConnectionFailureClassifier.cs` — the code table above.
- `src/Beep.OilandGas.Desktop.Data/Drivers/OgDriverDiscovery.cs` — packaged folder and referenced assemblies.
- The wizard's Database step hosts the shared connection catalogue, the connection editor and both downloaders.

Missing: AC-7 only. It is blocked, not abandoned: it needs an enterprise database the owner controls, and the
criterion stays in this document so it is not mistaken for proof. Everything else here is proved by
`ConnectionCatalogIntegrationTests` and the smoke run.
