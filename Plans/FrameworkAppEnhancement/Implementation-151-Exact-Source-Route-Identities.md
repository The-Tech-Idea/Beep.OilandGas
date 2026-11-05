# Implementation 151: Exact source route identities

## Deep links shared by accountant and procurement

The existing AP/PO detail links encode recorded IDs with Uri.EscapeDataString. New HTTP tests exposed that ASP.NET route binding preserves an encoded slash, so an original ID such as id/a?b arrived at the source reader as id%2Fa?b and refused valid ownership/detail review. A literal recorded ID containing %2F must remain distinct from a slash-containing ID; unescaping the already-bound parameter would decode that literal twice.

The existing AP and PO controllers now read source identity parameters from the original escaped request target and the matched endpoint route pattern. RecordedSourceRouteIds selects only the named single-segment parameter, verifies request base/shape and passes it to one shared Data URI-boundary decoder. Invoice, order, line and existing supplier routes use it before source reads. Missing raw target, invalid route/escape, blank/overlong/control-containing identity refuses with 400. There is no partially decoded parameter fallback.

Data-owned RecordedSourceId decodes the original escaped segment exactly once and preserves international characters, encoded slash/question/fragment punctuation and literal percent escapes. Its detail-location reader validates the app base and expected route prefix before decoding the final segment. The existing PayablesWorkbench and ProcurementWorkbench apply this reader to their current NavigationManager location, so the typed client and state receive the actual source identity rather than a partially decoded Blazor parameter. Invalid locations clear evidence and show a fixed source-address message.

## Real HTTP transport evidence

TestServer omits IHttpRequestFeature.RawTarget, which cannot establish the original wire path. The existing focused controller fixture now uses loopback Kestrel on an ephemeral port and ordinary HttpClient, with explicit authentication/current-company/provider fixtures. Each host is disposed after its test. No real application database, business write or deployment is involved. The unused TestHost package reference is removed.

This verifies the raw HTTP decoding behavior needed by generated detail links. Authentication remains a test scheme using standard roles; production OIDC/app-role mapping, current-company adapter/Program and the complete Web transport/parent components remain outside this gate. Reverse-proxy/path rewriting and complete browser base-path behavior still need host acceptance.

## Focused verification

The current complete Data source build passes with zero warnings/errors in 1.56 seconds. The final affected actual-source gate passes **347 tests**, zero failed/skipped, terminal exit 0, in 2.8242 seconds. Log: `%TEMP%/oilgas-source-route-151.log`. This includes the previous 323 cases plus 24 new checks; overlapping totals are not summed. The existing 175 dependency/source-precedence warnings remain.

Eleven new loopback HTTP cases cover invoice/order detail IDs with international characters, encoded slash/question punctuation and literal %2F/%25 values; both existing encoded supplier routes; and an encoded invoice/line -> owned order -> receipt allocation read. Thirteen shared URI-contract cases cover both Web detail locations under an app subdirectory with six distinct IDs, five invalid segments and two wrong-root/extra-segment refusals. Earlier 22 HTTP authorization/company/filter/binding/failure/fence/handoff checks now also run over Kestrel. Actual complete controllers, route reader, API helpers, current AP/PO services/repository, Data contracts, states and shared panels compile in the gate.

The complete parent pages and typed Web client remain source-only; this is not browser or production role-bridge proof. No source entity/schema, migration application, SQL, business EF path, role grant or live business command is added.

## Remaining journey

Verify the complete browser/client/parent flow and configured production proxy/base path, current company/account/binding adapter and real app-role bridge. Apply/reconcile AP line/allocation Beep schema; implement canonical allocation consumption, matching tolerances/unit/currency/receipt acceptance and independently configured preparer/reviewer commands, approval/post/payment and durable outcomes/recovery. Complete every other company/persona/multinational requirement. No phase closes.
