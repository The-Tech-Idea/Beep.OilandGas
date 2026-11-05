# CompanyEnvironment.Tests - data ownership restoration plan

Project: `Tests/CompanyEnvironment/CompanyEnvironment.Tests.csproj`.

The nested consumer was found during the completion audit after the initial 65-project planning scan. It owns no migrated domain definitions. Eight moved-definition source links have been removed: company environment preview/identity/setup/GHG selections now come from the existing PPDM39.DataManagement project, and GHG catalog/arithmetic contracts from the existing HSE project. Existing Models and OilandGasTools references are retained. No replacement contracts, forwarding types or data project are added.

The remaining API orchestration and repository context source links are fixture composition, not replacements for moved domain definitions. Compile and run this fixture against the actual current owner outputs, with `BuildProjectReferences=false` only when those outputs have already been built. Record unavailable host/authentication/provider behavior separately from this controlled consumer test.

Current verification: all 65 tests pass against actual owner references. Fixtures bind reviewed baseline revisions, valid PPDM mappings and declared source configurations; calculation verification does not claim to execute external sources. Exact removed paths and owners are in [nested caller evidence](evidence/canonical-dependency-recovery/nested-caller-restoration.json). Current aggregate evidence is [here](evidence/canonical-dependency-recovery/observed-evidence.json). This fixture does not certify API/Web startup, OIDC, production databases or real providers.
