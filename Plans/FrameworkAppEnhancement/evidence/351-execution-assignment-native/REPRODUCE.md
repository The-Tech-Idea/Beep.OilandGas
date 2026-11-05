# Reproduce native execution assignment and affected regressions

From the repository root in PowerShell:

```powershell
dotnet build TheTechIdea.Data/TheTechIdea.Data.csproj --no-restore
Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~DrillingExecutionAssignmentNativeTests|FullyQualifiedName~DrillingExecutionStartNativeTests|FullyQualifiedName~DrillingDailyReportNativeTests|FullyQualifiedName~DrillingProgramReviewStartNativeTests|FullyQualifiedName~DrillingProgramReviewDecisionNativeTests|FullyQualifiedName~CompanyProcessStartTests|FullyQualifiedName~PersonaTaskRoutingTests|FullyQualifiedName~WorkflowTaskProcessAdmissionTests|FullyQualifiedName~WorkflowInboxHttpTests|FullyQualifiedName~PersonaTaskInboxStateTests|FullyQualifiedName~EngineeringWorkspaceLandingTests|FullyQualifiedName~DrillingProgramReviewProgressTests'
```

The final gate passes 401 distinct tests, including 52 new assignment cases. It uses the canonical Data DLL, linked current native owners, the real common-column producer in new assignment cases, controlled external source/authority/driver seams and the affected existing HTTP/state/rendered-landing fixtures. Each new case has private source copies and a fresh lifecycle transaction store; approval/start source evidence is produced once by existing native owners.

This is a native foundation. The current sender/recipient authority API, typed client/protected journal, sender and assigned-reader screens, new task projection and new landing counts are not implemented or qualified by these tests. The declared assigned URL is a contract target, not a delivered page. No new execution-assignment protected HTTP or browser journey ran. Provider migration/constraints/isolation/concurrency/LOB, full host/OIDC, notification delivery, performance and multinational qualification remain open. No SQL, new business EF, migration, production write, staging, commit or push occurred.

Construction logs retain two test-namespace compile errors, one intentionally stopped superseded run with a JSON readback failure, and the completed 394-case construction run with 393 passes and one aliased-fixture-original failure. These receive no final gate credit. Initial process data now uses the shared canonical snapshot owner; the fixture independently freezes the original source. Actual creation audit values are preserved. Negative assertions and generic bypass guards were not weakened.
