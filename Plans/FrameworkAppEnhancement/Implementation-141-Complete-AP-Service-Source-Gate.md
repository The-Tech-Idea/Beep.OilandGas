# Implementation 141: Complete AP service source gate

## Strengthening the accountant source journey

The focused purchase-to-pay gate now compiles the complete current APInvoiceService with its actual AccountingBasisPostingService, JournalEntryService, GLAccountService, AccountMappingService, interface, constants and mapping sources. No fake posting service or replacement production project is created. This resolves the AP service compilation gap recorded in Implementation 140 while retaining the original accountant/procurement screens and API owners.

Compilation exposed stale Beep.OilandGas.PPDM39.Repositories imports in the actual journal, GL and account mapping dependencies. Those unused imports and duplicate Core imports were removed; the current repository owner remains Beep.OilandGas.PPDM39.Core. No business model/store duplication or fallback was introduced.

## Explicit owned invoice query

The AP detail service now queries the actual generic repository with exact AP_INVOICE_ID, OWNER_TENANT_ID and OWNER_COMPANY_BA_ID equality filters instead of fetching by a globally assumed identity and accepting the first row. Empty confirmed rows return no invoice; multiple rows, incompatible rows, foreign owners or mismatched identities refuse. Shared ownership validation remains mandatory after provider results. Supplier reads retain exact owner, supplier and active-indicator predicates and result validation.

Tests execute the canonical AP service through the actual cached native repository and IDataSource contract under controlled provider fixtures. They show two companies can request the same local invoice identity without the second scope changing the first review. The physical AP schema still requires its declared unique nonnullable native key; this fixture does not establish that colliding IDs can coexist in one physical store.

Internal AP receive/payment/posting methods compile but are not lifecycle-certified. No financial command is exercised or enabled by this gate. HTTP creation remains fenced. Currency-bound AP aging remains explicitly unavailable before provider access; no mixed-currency totals are produced.

## Focused evidence

`Scripts/Test-PurchaseOrderOwnership.ps1` final-source result: **129 passed**, zero failed/skipped, terminal exit 0, 1.2678 seconds. Log: `%TEMP%/oilgas-ap-service-141.log`. The prior 111-case source/UI gate is included, not additional unique coverage.

Eighteen new complete-service cases cover two missing/empty resolver refusals; four fresh missing/duplicate schema refusals; three missing/nullable/compound native key refusals; one successful original-owner detail query/review; three duplicate/foreign/wrong-ID provider refusals; one exact supplier predicate read; one propagated provider failure; one two-company colliding identity case; one genuinely empty detail/list read; and one aging refusal before datasource access.

The test build has zero errors and **100 existing dependency warnings**: 93 obsolete DefaultGlAccounts references, three nonexhaustive accounting-basis switches, two possible null assignments and two null literals. These are recorded, not suppressed or asserted resolved. Posting/currency/account-mapping lifecycle acceptance remains open.

The actual current Data assembly is reused from the clean Implementation 140 build. Cached native Models remains aliased so the Data-owned PO/AP types cannot be shadowed. Test-only aliases name the actual native interface and existing journal/GL domain types; actual JournalPostingEvidence and mapping/interface sources are compiled. These aliases are not added to production code. The gate uses a cached PPDM repository dependency, not a freshly compiled entire PPDM project.

## Remaining source and journey gates

Inspection of the current generic repository found that ConvertToTypedList returns an empty sequence for a null provider payload and skips null enumerable members. Such unavailable/partial evidence must not appear as a confirmed empty company source. Correction and tests of the active generic mapping path remain required before promotion; the successful empty-row test above uses an actual empty array, not null. Exceptions propagate in the verified AP read, but that does not prove null-payload handling.

APController, typed client, parent workbench/accounting landing, facade retirement and production host remain source-only. Actual HTTP/OIDC, browser/event/keyboard/RTL behavior, current PPDM assembly integration, native provider/schema/migration/reconciliation, atomic consistency and durable posting/payment recovery remain unverified. Recorded headers still do not supply exact invoice-line/PO/receipt matching evidence. Finish those connections and configured independent role duties, and preserve all remaining persona/multinational requirements. No SQL, schema migration, live write, role grant or phase closure occurred.
