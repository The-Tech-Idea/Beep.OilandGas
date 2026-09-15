# LocalDB Identity Verification

## Verified Startup

On 2026-09-12, the current sibling IdentityServer project was built successfully:

`../Beep.IdentityServer/Beep.Foundation.IdentityServer/Beep.Foundation.IdentityServer.csproj`

A temporary process ran from a fresh content directory, not the IdentityServer
project directory. Its personal development configuration was not loaded.
Both `ConnectionStrings:DefaultConnection` and `ConnectionStrings:IdentityConnection`
were explicitly set to the same new SQL Server LocalDB database, with
`Database:Provider=SqlServer`. No PostgreSQL or Oracle server was contacted.

Retained database: `IdentityServerProbe_92f80a4170284bf6b6e0da432970df41` on
`(localdb)\MSSQLLocalDB`.

Startup applied 49 IdentityServer migrations. Independent SQL queries confirmed
zero users and zero OpenIddict applications. These are IdentityServer's own tables,
not the OilGas repository's six-migration Identity/extension schema.

HTTPS discovery at `https://localhost:7062/.well-known/openid-configuration`
returned issuer `https://localhost:7062/`, authorization endpoint `/connect/authorize`,
token endpoint `/connect/token`, code-flow support, S256 PKCE, and `beep-api` scope.
The browser rendered `/account/register` with email/password fields and a mandatory
terms/privacy consent checkbox. No registration form was submitted.

Logs: `TestResults/identityserver-startup-build.log` and
`TestResults/IdentityServerProbe_92f80a4170284bf6b6e0da432970df41/`.

## Remaining Sign-In Test

1. Obtain approval to accept the consent checkbox for synthetic local test accounts.
2. Restart the isolated IdentityServer against the retained probe database, again
   without loading personal development settings or any existing authentication DB.
3. Create a synthetic account through its supported registration flow. IdentityServer's
   first account becomes its console administrator; this is separate from OilGas RBAC.
4. Register an OAuth client through the console or supported authenticated client
   registration API. IdentityServer does not seed applications from configuration.
   OilGas currently names `beep_oilgas_web`, but the configured ID must match the
   actual registration. Configure code flow, S256 PKCE, the required scopes, and
   exact callback URIs `https://localhost:7066/signin-oidc` and
   `https://localhost:7066/signout-callback-oidc`.
5. Keep generated client credentials out of chat and source files. Supply the Web
   client secret through secure process configuration, such as
   `BEEP_OILGAS_OIDC_CLIENT_SECRET`. Verify the client authentication type matches
   the server registration. A running discovery endpoint does not prove this.
6. Verify token-format compatibility. IdentityServer enables access-token encryption
   by default. OilGas supports explicit introspection validation (see below);
   its default Jwt mode has no configured decryption key. Discovery/signature keys
   alone cannot decrypt access tokens. Register the separate API resource client
   before selecting introspection for this isolated run.
7. Create a separate isolated OilGas repository and apply its EF migrations. Never
   consume the real development repository's first-Administrator registration with
   a synthetic account. Run API/Web against that test repository and client.
8. Exercise real browser sign-in, verify the first local account receives only the
   canonical Administrator assignment, register a second subject without automatic
   roles, and test role assignment/revocation and inactive-user denial.
9. Verify selected BeepDM module binding/setup remains independent of both Identity
   databases. Stop only the test processes and retain their databases and logs.

## API Token Validation

`IdentityServer:ValidationMode` selects `Jwt` (default) or `Introspection`.
Both modes require an explicit HTTPS authority from Aspire's HTTPS discovery
setting or `IdentityServer:Authority`; neither silently defaults to localhost.
Invalid nonblank discovery takes precedence and fails startup, rather than
falling back to a different authority. Blank audiences are rejected. JWT keeps
issuer, audience, lifetime, and expiration validation enabled, preserves raw
claim names for the repository bridge, and uses normal TLS certificate validation
in Development as well as production. Trust the localhost development certificate
instead of disabling certificate checks. Only the selected validation mode is
registered, with no alternate bearer authentication fallback.

Introspection uses OpenIddict validation and the HTTPS authority's discovery and
introspection endpoints; it does not open IdentityServer's database or share its
private encryption key. The Web OIDC client remains separate.

Configure these API process settings through secure configuration:

- `IdentityServer__ValidationMode=Introspection`
- `IdentityServer__Authority=https://localhost:7062/`
- `IdentityServer__Audience=beep-api`
- `IdentityServer__Introspection__ClientId`: registered API resource client ID
- `IdentityServer__Introspection__ClientSecret`: that client's secret

Register the resource client with introspection endpoint permission and the
server-side audience/resource access needed for `beep-api`. Do not reuse the Web
client secret or commit credentials. Trust the development HTTPS certificate;
this mode does not bypass TLS certificate validation. Invalid mode, authority,
or missing credentials fails startup without falling back to Jwt.

The validated authority is restored as `iss` after OpenIddict's introspection
validation, because the library omits this protocol claim from its principal.
`RepositoryClaimsTransformation` then resolves the exact issuer/subject in the
OilGas repository and replaces external roles with local ASP.NET roles. An
inactive token or wrong audience is denied; an invalid issuer response is an
upstream protocol error (HTTP 500), not a successfully authenticated account.

The HTTP tests use a controlled introspection transport, not a real OAuth client.
They do not prove the pending full browser sign-in or server client registration.
Local database testing remains SQL Server LocalDB only.

Verification: `TestResults/api-introspection-full.log` records 849 passing API
tests, zero failures and zero skips, with `OILGAS_TEST_LOCALDB`,
`OILGAS_TEST_LOCALDB_DRIVER`, and `OILGAS_TEST_LOCALDB_MODULE` enabled. The 13
introspection cases cover local-only roles, inactive/expired tokens, wrong audience,
wrong issuer, omitted versus null issuer, malformed responses, and invalid startup
configuration. These results do not validate a deployed PostgreSQL/Oracle server.

The subsequent `TestResults/api-bearer-startup-full.log` records 861 passing API
tests with the same LocalDB flags, zero failures, and zero skips. Twelve added
startup cases verify invalid authorities, missing configuration, discovery
precedence, blank audiences, and secure JWT options. Real browser sign-in is
still pending; these checks do not establish end-to-end installation readiness.

## Safety Boundaries

Starting IdentityServer normally automatically applies migrations and seeds role
definitions/scopes. Do not run it against an existing configured database merely
to probe availability. The temporary startup test neither created users or clients
nor changed the OilGas development repository. Consent and full OIDC sign-in remain
unverified; no production or end-to-end readiness claim follows from this probe.
