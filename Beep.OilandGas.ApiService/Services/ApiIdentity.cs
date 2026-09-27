using Beep.Foundation.IdentityServer.Shared.Identity;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>
/// Who an API request is from, and what OilGas lets them do — the one wiring the API and its tests share.
/// </summary>
/// <remarks>
/// <para>
/// Signed access tokens from the identity server, validated here — issuer, this API's identifier as the audience
/// (<c>IdentityServer:Audience</c>, registered under Admin &gt; APIs), optionally <c>IdentityServer:AllowedClients</c> — by
/// the identity server's client library. Each person resolves to their OilGas account (<c>party_id</c>: found by the
/// identity server's subject, relinked after a reseed on a verified address held by exactly one account, else created —
/// the first administers); the repository's roles and permissions follow (<see cref="RepositoryRolesClaimsTransformation"/>),
/// and an endpoint admits only an active account (<see cref="RepositoryAuthorization"/>).
/// </para>
/// <para>
/// It validated with its own JWT bearer handler against an audience that defaulted to the shared <c>beep-api</c>, or
/// introspected with credentials no configuration supplied, and resolved the account from issuer and subject itself
/// (S3-06 §5, §6).
/// </para>
/// </remarks>
public static class ApiIdentity
{
    public static IServiceCollection AddOilGasApiIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization(RepositoryAuthorization.Configure);
        services.AddBeepFoundationIdentityServerResourceValidation(configuration);
        services.AddScoped<ICanonicalUserStore<string>, RepositoryCanonicalUserStore>();
        services.AddPartyIdResolution<string>(autoProvision: true);
        services.AddChainedClaimsTransformation<RepositoryRolesClaimsTransformation>();
        return services;
    }
}
