# Multinational and multi-company requirements

Detailed implementation documents: [company plan index](Company-Plan-Index.md). Covers all reviewed recommendations with feature-level plans; existing phases and CA/FG task owners remain authoritative.

Added 2026-10-05 following explicit user direction. Applies to every company roadmap phase, rather than a late localization feature. The root master tracker owns execution status.

## Organization model and isolation

Distinguish customer tenant (security/operational boundary), corporate group, legal entity (statutory books and obligations), operating unit and asset. A tenant may operate multiple legal entities across countries. Independent customer companies must be isolated even when hosted by the same product. Joint ventures and shared operators require explicit scoped grants and ownership interests; they do not imply unrestricted cross-company access.

Existing TheTechIdea.Data/OilGas/IdentityExtensions.cs has a nullable TenantId and PersonaModels.cs includes TimeZone. These fields are source evidence only, not proof of tenant enforcement or multinational readiness. Inventory existing organization/company/PPDM business-associate owners before extending contracts. Shared business contracts remain in TheTechIdea.Data; no second tenant or company authority.

Resolve active company from authenticated membership through the API. Never trust a supplied tenant header, selected persona, URL, OIDC role or client company identifier as authorization. Switching company must refresh standard app roles, permitted assets, selections and cached data. API checks validate tenant, legal entity and asset independently for every operation. Audit support/platform access, including its justification and expiry.

Target hosting: customer tenant is an explicit deployment/data boundary with tenant-specific domain connection bindings and regional placement. Multiple legal entities can share that tenant's governed stores while retaining separate books. Reuse the existing module-binding/setup infrastructure; do not route all tenants through a global mutable current connection. Document actual deployment topology under FG-01 before implementation. Any shared control-plane store must enforce tenant scope, and shared-domain database hosting is not approved by merely adding TenantId.

## Required delivery tasks

| ID | Deliverable | Acceptance |
|---|---|---|
| CA-05 | Organization and tenancy ownership inventory, membership, API request context and company switching | Two independent tenants with identical asset/document IDs remain isolated; one group spans two legal entities; role changes take effect on switch/revocation; forged context fails closed. |
| CA-06 | Tenant isolation through repositories, module bindings, jobs, caches, search, exports, files, diagnostics and SignalR | Automated negative tests cover direct object references, batch/import/export, attachments, background jobs, hub subscriptions and error references; no data or credentials cross tenant boundary. Connection and cache keys include immutable tenant context. |
| CA-15 | Multi-currency and legal-entity accounting | Separate books, functional and presentation currencies, effective-dated recorded exchange rates, realized/unrealized differences, intercompany balances and consolidation eliminations reconcile. No summing unlike currencies; no guessed rates. Each legal entity has fiscal calendar, close authority and accounting policy. |
| CA-16 | Effective-dated country/jurisdiction configuration | Existing tax, royalty, invoicing, retention and accounting services consume reviewed jurisdiction policies with source/version and approval. Unsupported jurisdiction produces explicit unavailable status. Country/language/IP does not infer tax jurisdiction. |
| CA-24 | Regional engineering units and operational calendars | Store canonical quantities with unit/dimension and declared basis; convert display and imports explicitly; distinguish standard/actual gas volume and pressure/temperature reference conditions. Preserve UTC instant plus operating timezone/business date; daylight-saving and midnight boundaries tested. |
| CA-45 | Language, locale and regional deployment | User language, date/number formatting, timezone and unit preferences separate from entity accounting/jurisdiction policy. Arabic RTL and English LTR pilot, mixed-language documents and invariant serialization pass. Tenant-specific residency placement, backups, attachments, logs and restore behavior are documented and tested. |
| CA-46 | Multinational release pilot | Two independent tenants and one multinational group complete engineer-to-accountant workflows in different locales/timezones/currencies; consolidated results reconcile; unauthorized cross-company requests fail across every channel. |

## Jurisdiction and deployment policy

Configure country and applicable jurisdiction per legal entity, asset and transaction as required by reviewed rules. Use effective-dated policy records and approved configuration of existing services rather than separate country-specific copies of the application. Regional reviewers establish launch-country requirements and expected outputs; research current official rules during implementation. This planning document makes no legal or regulatory compliance claim.

Capture hosting region, permitted transfers, retention, deletion/legal-hold behavior, document numbering, language and contractual constraints for each launch tenant. Include logs/diagnostics, backups, analytics and disaster recovery in residency review. A regional database alone does not prove residency. Tenant provisioning, migration, backup/restore, export/offboarding and service quotas must preserve isolation and resource fairness. Secrets remain server-side; platform operators do not automatically gain business-data privileges.

## Sequencing changes

CA-05/06 precede production mutations and cross-role pilots. CA-04 may proceed only within explicit tenant/legal-entity context. CA-15/16 gate finance promotion; CA-24 gates engineering/production data; CA-45/46 gate company release. Review existing FG role/setup/workflow tasks against these constraints. Do not defer isolation until after adding business workflows.

Launch-market selection, supported accounting policies/currencies/languages, regional topology and retention obligations remain discovery inputs. Record them in CA-01 without blocking tenant-safe architecture work. Every new task must test two companies, not only multiple users in one company.

Implementation 139: [Recorded procurement currency and units](Implementation-139-Recorded-Procurement-Currency-and-Units.md) extends the same PO source review/screen with nullable recorded header currency and separate line/receipt quantity-unit codes, preserving differing international units without conversion or aggregation. Existing PRODUCTION Beep types declare the fields; missing/duplicate fresh schema columns refuse reads. 81 focused Data/service/metadata/content/JSON/state/render checks pass; Data builds cleanly. Migration/reconciliation, authoritative reference/conversion evidence, AP ownership and exact invoice/PO/receipt links, full HTTP/browser/provider and controlled handoffs remain open; no phase closes.
