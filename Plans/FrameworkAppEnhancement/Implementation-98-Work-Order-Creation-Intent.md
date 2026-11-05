# Implementation 98: Original work-order creation intent

## Persona journey and form audit

Maintenance planners and production users arrive at the existing work-order board, confirm their company/FIELD and open its create form. The original form had title, description, subtype, equipment and planned start, but no jurisdiction input, inherited USA from the request initializer and had no submission guard. Its API overwrote request FIELD with current FIELD. A failed create could be repeated, and automatic AFE follow-up could use changed context.

## Implemented existing path

The create dialog captures the confirmed FIELD when opened and displays company/FIELD. It includes a required applicable-jurisdiction field; a new request has no assumed country. Existing title/subtype inputs remain. Validation and submission disable the form and submit action while running. Context changes close the dialog. Before transport, WorkOrderCreateState freezes the actual request and captures the company/account selection version. It records an attempt before sending. Repeat attempts in the same actor/company/FIELD are blocked if the outcome is unknown, including after selecting another company and returning. List reloads do not clear this state.

Only a returned nonempty order identity with matching FIELD, title, subtype and recorded DRAFT state confirms the attempt. Changed context cannot trigger navigation or follow-up actions. The confirmed order opens through its encoded exact detail route for review. The former automatic create-then-AFE command is replaced by an explicit recorded-detail handoff; requested AFE linking is explained in the screen notification. No AFE success is fabricated.

The existing create API requests FIELD WRITE authority through the existing attribute and refuses a missing/different submitted FIELD instead of replacing it. It requires title and jurisdiction. It uses the existing work-order service/store. There is no new data lane, role grant, SQL, schema or migration.

## Focused verification

The existing Test-WorkOrderListState.ps1 now includes the actual creation state and actual current DTO source. **29 passed**, zero failures/skips, terminal exit 0, 1.5322 seconds: 14 previous list/JSON cases plus 15 creation cases. New coverage includes copied intent, repeat/away-and-back company selection, FIELD invalidation, null/wrong identity/FIELD/subtype/title/state responses, exact matching confirmation, missing company/FIELD/title/jurisdiction and absence of an assumed country. Log: `%TEMP%/oilgas-work-order-creation.log`. Whitespace diff check passed.

Current DTO source wins over older cached model copies in the isolated project; CS0436 is suppressed as documented in the script. Tests do not execute the parent Razor screen, actual client transport, controller/filter, underlying provider, physical company isolation or browser focus/validation/navigation.

## Remaining gates

Page-lifetime repeat protection is not durable command idempotency. Leaving/reconnecting/restarting can lose it; the warning makes this explicit. Matching response is not independent committed read-back. Add durable original-context command receipts, provider concurrency and read-back before certifying create completion. Native create API FIELD WRITE/resource authority and database binding must be tested. Jurisdiction needs authoritative company/FIELD configuration and checklist applicability validation; the separate production-controller service path still hardcodes USA and needs correction through its own reviewed intervention request. AFE authority/review/read-back and the complete materials/permit/execution/supervisor return-to-service/cost journey remain open. Existing detail commands are not certified by this change.

All 13 persona plans and accountant finance gates remain active. No phase or overall goal is closed.
