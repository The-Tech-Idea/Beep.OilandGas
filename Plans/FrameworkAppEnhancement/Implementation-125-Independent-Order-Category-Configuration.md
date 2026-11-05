# Implementation 125: Independent order category configuration

## Company-admin screen and recorded configuration

The existing hierarchy configuration screen now offers an explicit Add work-order category action for a confirmed company Administrator configuration. It adds one WORK_ORDER presentation category to the existing aggregate, retaining the confirmed revision for the ordinary save command. Existing configured FIELD/WELL/POOL/FACILITY categories remain the same active contract; WORK_ORDER is an optional independent category. No automatic persisted category, second configuration store or compatibility path is introduced.

The Data-owned HierarchyConfigurationRules is now the shared shape validator for the existing API store and Web state. It requires each existing category once, permits one optional WORK_ORDER category, and refuses unsupported/duplicate categories, foreign company identities, invalid names or parent shapes. FIELD and WORK_ORDER are independent roots. WELL/POOL/FACILITY remain FIELD children. Hiding FIELD hides its child categories and leaves work-order visibility independently editable. Recorded FIELD references never become a claimed single FIELD parent.

The actual screen explains this relationship and allows editing the order label/visibility through the existing controls. Standard Administrator route and selected-company checks remain. App-owned configuration persistence still uses the existing JSON aggregate and immutable revision history; no business schema change or new business EF path is introduced. IdentityServer remains authentication-only.

## Exact outcome confirmation

The existing save state previously accepted any structurally valid response with a new revision, even if the returned labels or visibility differed from the submitted command. It now compares the complete company/type/level/parent/name/visibility content with the frozen submission before confirming success. A mismatched saved label clears editable evidence and reports the unconfirmed result. Revision and server audit provenance remain independently checked/owned.

## Focused verification

Actual isolated TheTechIdea.Data build: zero warnings/errors, 1.53 seconds. The final affected company gate passed **202 cases**, zero failed/skipped, terminal exit 0, reported duration 49 seconds. It selects CompanyHierarchyConfigurationTests, CompanyHierarchyUiTests and CompanyAssetHierarchyTests using the actual current Data assembly. Log: `%TEMP%/oilgas-order-config-125.log`. Five new cases cover revision-bound addition to the existing aggregate with independent order visibility and Arabic naming, parent/duplicate refusal, and exact or mismatched save readback. SQLite executes the actual app-owned aggregate/history store; controlled Beep fixtures cover affected hierarchy reads. Existing nullable/source-precedence/analyzer warnings remain.

The initial gate reported **201 passed, one failed**: an existing empty-draft save fixture returned hardcoded type labels instead of the submitted display names. The new exact comparison correctly refused it. The fixture now records and returns the actual submitted content with its server revision; the final gate passes. The intentional mismatched-label case separately remains refused.

Scripts/Test-HierarchyWebComponents.ps1 -RenderWorkOrders compiles three actual components plus the test-only MudPopover render host cleanly against current compiled state/Data and the actual Web MudBlazor version. **Nine static HtmlRenderer checks pass**: five FIELD-interest states/locales, two recorded/unconfirmed order-tree cases and two configuration-page cases for explicit addition versus saved Arabic order naming/visibility. Log: `%TEMP%/oilgas-order-config-razor-125.log`. Evidence: `%TEMP%/oilgas-hierarchy-razor-fa59980d062a484489804bc8a50e38fc-render`. The test-only popover host supplies the actual MudBlazor provider without changing production screens. Static rendering does not execute browser click events, live HTTP, OIDC or route authorization. Whitespace check passed.

## Remaining delivery

Hosted API/DI/browser authorization, real provider/date-window/concurrency acceptance and the full configuration-to-recorded-order runtime journey remain open. Maintenance duties still need approved app-role mapping and standard role controls. Complete grant -> landing/task -> order detail -> inspection requirement/result -> independent acceptance -> return-to-service/accounting; native Beep inspection migration/transactions and durable outcomes/corrections remain open. All other persona plans remain active. No handwritten SQL, live grants/configuration writes, migration execution, deployment, business EF lane or persona/phase/goal closure.
