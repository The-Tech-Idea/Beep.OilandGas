# Implementation 258 — Paged saved commercial proposal recovery

## Large company histories

The saved commercial proposal landing component now reads native Beep pages instead of the bounded whole queue. It offers a page size of 1, 4, 8, 16 or 32, reload, previous/next navigation and the original process recovery links. The full-queue endpoint retains its original complete-set contract and refuses oversized histories; the landing path does not call it.

Each page is an independently reviewed capture. The screen reports the provider's count of saved journal entries and the selected page, while unresolved/all views count only the rows on that page. An empty unresolved page never claims that the entire company account has no pending work; it directs the preparer to other pages. No discovery read executes an amendment or enables retry.

## Canonical API-first ownership

TheTechIdea.Data owns CompanyCommercialAmendmentPage around the existing canonical queue. It binds original company/account, request number/size, complete row count, total saved entries, page navigation and the queue revision into a page digest. Requests require a positive page, size 1–32 and safe integer offsets; responses refuse negative counts, impossible page numbers, incomplete rows and changed request/revision evidence. The eight-million-command-character safety envelope remains per page; smaller page sizes support larger individual proposals.

ProductionAccountingService extends the existing commercial journal owner with IDataSource.GetEntity paging through the Beep source guard and PPDM typed conversion. It validates current Beep journal metadata before every capture, exact requested provider page/size, total/page counts, next/previous flags, expected row count, unique intent/process identities, full restored original commands/audit and original tenant/company/account. It does not filter out inactive owned journals. Missing, malformed, foreign, duplicate or too-large page evidence refuses. Provider ErrorObject failures cannot be accepted as empty pages.

Two native captures must contain the same ordered entries and total under fresh authority and the same datasource. ApiService then runs each selected entry through the existing closed original process, saved journal and historical source owners, including current FIELD authority. It repeats the full page/source review and compares total plus exact canonical queue. Shared discovery-authority helpers and the existing queue core serve the full and paged routes; no parallel business persistence or authorization model is added.

The standard app roles, Accounting.View, ProductionAccounting.Adjust, account/company and production/lifecycle binding guards remain independent API authority. The page total counts actor-owned saved journal identities under that company discovery authority; it does not certify an unresolved count, business amount or authorization of unreviewed source rows on other pages. Each returned source row still requires its own current evidence/authority. Role, grant or selected FIELD denial refuses the selected page rather than returning a successful partial result.

The typed client validates original request and current company/account after transport. The old queue API remains available for complete bounded discovery. No client reads a database; no new SQL, schema, migration, EF business lane, grant or live business operation was added. Beep Migration remains the provisioning owner for the existing journal.

## Screen and navigation fences

Changing the page size resets to the first page. Reload keeps the selected page; company/account/authentication changes clear displayed rows/page audit and reset to the first page. Every read verifies authentication/selected standard app roles before transport and before publishing, and compares the original company generation, requested page and size. Failed or late navigation cannot leave earlier-page entries displayed as current evidence. Loading disables the navigation and size controls. Existing MudBlazor/theme behavior remains in use, with no CSS or JavaScript changes.

Pages are fresh independent reviews, not a frozen snapshot spanning several clicks. The native interface does not provide a cross-page snapshot token or a requested ordering contract here. Current tests use a deterministic mock provider order and repeated same-page validation. Actual driver ordering, cross-page concurrent insert behavior and stable continuation guarantees remain physical-provider acceptance work; no global snapshot claim is made.

## Focused verification

Canonical Data builds with zero warnings/errors. The first source compile caught Razor treating page.TotalRecords and page.PageNumber as page directives; explicit expressions fixed the parser errors. The corrected existing source/Razor/local HTTP gate passes all 241 cases, zero failures/skips, in one minute six seconds, terminal exit 0.

The new paging checkpoint passes 29 cases, zero failures/skips, in eight seconds, terminal exit 0. Native page cases verify first/last reads and refuse wrong page/size/page counts/navigation flags, incomplete or duplicate rows, foreign/inactive journals, provider error, changed totals, null responses and invalid requests. The actual native writer saves 65 distinct proposals; its final page returns the exact final saved UUID and total while the unchanged whole-queue limit refuses. This uses synthetic process identities and a mock Beep datasource to test journal/native paging, not 65 real lifecycle workflows or a physical deployment.

SDK cases refuse foreign company/account, wrong request number/size, count/revision, null and late responses; invalid requests make no HTTP call. Rendered component handlers change size and navigate two pages, verify page-local counts/original-process links, and clear prior rows on failed or changed-account reads. The landing never invokes whole-queue discovery. Existing real HTTP/SDK commercial journeys now additionally read pending and recorded native pages, test invalid route requests and nonexistent pages, and refuse missing roles, command grants and FIELD access.

Final affected source/Razor/local HTTP/native/migration-metadata gate passes all 446 tests, zero failures/skips, in one minute seventeen seconds, terminal exit 0. Scopes overlap checkpoints; counts are not summed. Reproduce through Scripts/Test-PurchaseOrderOwnership.ps1 with canonical Data DLL, IncludeWorkflowInbox and FieldRevenueSourceTests|CompanyProcessStart|CompanyStep|PersonaInbox|CommercialIntentMigration|ModuleSchemaVerification. Logs: oilgas-258-data.log, oilgas-258-source.log, oilgas-258-corrected-source.log, oilgas-258-pages.log and oilgas-258-final-source.log. Focused source/dependency warnings remain; no full solution, browser or physical-provider result is claimed.

## Remaining delivery

Continue actual landing-to-original-process recovery across further independently reviewed lifecycle amendments, then approved delivery/effective terms -> invoice/recognition -> ledger read-back. Continue royalty/partner queues, thirteen-persona workflow/landing/dashboard delivery and multinational acceptance.

Physical paging/snapshot/concurrency/performance, current grants across regional providers, host/OIDC/browser reconnect, keyboard/screen-reader/responsive/RTL and live Beep provisioning remain open. No CA/FG phase or overall goal closes. The user's commit/push instruction remains final delivery after the complete goal and plan pass their completion audit.
