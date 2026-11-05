# Foundation implementation: dependency preflight and surface inventory

Date: 2026-10-05. Source baseline: `6e32d266` plus working changes. Tasks CA-01/02, FG-00/60. Status: partial, focused script verification only. Company isolation and application feature work remain open.

## Delivered

- CI now triggers master push/PR, with read-only repository permissions.
- Module ownership guard runs independently of application build, so missing dependencies cannot suppress it.
- Scripts/Test-ProjectDependencies.ps1 evaluates actual MSBuild ProjectReference items recursively, including imported props/targets and external projects. Reports missing sources before restore; does not install dependencies or manufacture replacements.
- Scripts/Test-ProjectDependencies.Tests.ps1 verifies successful graph traversal, imported missing reference rejection and termination on reference cycles. Temporary fixtures are removed only after checking their absolute path remains in the intended temp fixture directory.
- Scripts/Export-AppSurface.ps1 exports source-declared page routes and HTTP attributes with file/line evidence. CI saves the inventory artifact. This is an audit starting point, not composed runtime routing or authorization certification.

## Executed evidence

| Check | Outcome | Boundary |
|---|---|---|
| Dependency behavior fixtures | Passed all three cases | No application restore/build or supported-provider checks |
| Actual API/Web reference traversal | 60 distinct projects, 314 references, five missing projects; gate correctly fails | Evaluates current local MSBuild settings, not every CI/configuration permutation |
| App source inventory | 918 declarations: 178 page routes, 740 HTTP attributes | Regex source scan; dynamic/multiline/composed routes and comments require manual review |
| Existing module ownership guard | Passed, 109 module files | Existing pattern guard only |
| git diff --check | Passed | Whitespace check |

Missing current references: Beep.IdentityServer.Analyzers, Beep.IdentityServer.Contracts, TheTechIdeaWeb.ThemeBranding, TheTechIdeaWeb.Diagnostics and TheTechIdeaWeb.Diagnostics.Abstractions. Configured Beep.Web directory is absent locally. Available sibling IdentityServer and BeepWeb remote trees do not contain these missing projects; a separate older ThemeBranding source was located under fahadTheTechIdea/MyWebSite but is not substituted without establishing contract/version alignment. Source location requested from the user.

## Next implementation

### Continued foundation verification

The preflight now defaults to every C# project declared by `Beep.OilandGas.sln` and evaluates Release configuration. Explicit project lists remain available for focused diagnosis. New fixtures prove that a dependency referenced only by a test project is included and that Release-only references are excluded in Debug. All five fixture scenarios pass.

The actual solution scan starts from **70 solution projects**, traverses **82 distinct projects and 348 references**, and still identifies the same five missing shared projects. This supersedes the narrower API/Web traversal for current dependency coverage. Multi-target framework/property permutations remain outside this check's scope.

`dotnet build Beep.OilandGas.Repository/Beep.OilandGas.Repository.csproj --no-restore -p:GeneratePackageOnBuild=false -v quiet` fails with two CS0246 errors for the missing diagnostics namespace and IFailureReporter. `TheTechIdea.Data` alone compiles with MSB9008 for the absent analyzer; this is not an analyzer-complete baseline. No dependencies were stubbed or quality gates disabled.

Ownership inspection found PPDM BUSINESS_ASSOCIATE/BA_ORGANIZATION and the existing OrganizationSetupData/ORGANIZATION_HIERARCHY_CONFIG. Those existing owners must be mapped to tenancy before adding organization models. Repository roles and module bindings remain global; tenant enforcement has not been implemented or claimed.

Dependency audit after three consecutive goal turns: all five configured project paths remain absent and the Repository build still fails on diagnostics types. Implementation is blocked awaiting current source repository URLs or checkout paths. Existing changes remain preserved; this does not mark any business phase or the overall objective complete.

Resolve reproducible sources for the shared dependencies, then run clean API/Web/repository baseline and validate CI in an actual runner. Continue CA-01 owner/role/route matrix and CA-05/06 membership/request-context/module-binding isolation. Current RepositoryAccessService resolves global AspNetUserRoles and permissions; ModuleDatabaseBindings key is ModuleId alone. Neither is sufficient tenant-scoped authority/routing. Do not introduce a company selector until the entire active role/data path enforces that context.
