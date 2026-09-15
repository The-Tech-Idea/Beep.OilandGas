using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenIddict.Validation.AspNetCore;
using OpenIddict.Abstractions;
using OpenIddict.Validation;

namespace Beep.OilandGas.ApiService.Services;

public static class ApiBearerAuthentication
{
    public static string Configure(IServiceCollection services, IConfiguration configuration)
    {
        var mode = configuration["IdentityServer:ValidationMode"] ?? "Jwt";
        if (mode is not ("Jwt" or "Introspection"))
            throw new InvalidOperationException("IdentityServer:ValidationMode must be Jwt or Introspection.");

        var authority = new[] { configuration["services:identityserver:https:0"], configuration["IdentityServer:Authority"] }
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        if (!Uri.TryCreate(authority, UriKind.Absolute, out var issuer) || issuer.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(issuer.UserInfo) || !string.IsNullOrEmpty(issuer.Query) || !string.IsNullOrEmpty(issuer.Fragment))
            throw new InvalidOperationException("Configure an absolute HTTPS IdentityServer:Authority without credentials, query, or fragment.");
        var audience = configuration["IdentityServer:Audience"] ?? "beep-api";
        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("IdentityServer:Audience must not be blank.");

        if (mode == "Jwt")
        {
            services.AddAuthentication().AddJwtBearer(options =>
            {
                options.Authority = issuer.AbsoluteUri;
                options.Audience = audience;
                options.MapInboundClaims = false;
                options.SaveToken = false;
                options.IncludeErrorDetails = false;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters.ValidateAudience = true;
                options.TokenValidationParameters.ValidAudience = audience;
                options.TokenValidationParameters.ValidateIssuer = true;
                options.TokenValidationParameters.ValidateLifetime = true;
                options.TokenValidationParameters.RequireExpirationTime = true;
                options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);
            });
            return JwtBearerDefaults.AuthenticationScheme;
        }

        var clientId = configuration["IdentityServer:Introspection:ClientId"];
        var clientSecret = configuration["IdentityServer:Introspection:ClientSecret"];
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("Configure IdentityServer:Introspection:ClientId and ClientSecret through secure configuration.");

        services.AddOpenIddict().AddValidation(options =>
        {
            options.SetIssuer(issuer);
            options.AddAudiences(audience);
            options.UseIntrospection().SetClientId(clientId).SetClientSecret(clientSecret);
            // Introspection omits the protocol issuer claim; the role bridge needs the validated authority.
            options.AddEventHandler<OpenIddictValidationEvents.HandleIntrospectionResponseContext>(handler => handler
                .SetOrder(OpenIddictValidationHandlers.Introspection.PopulateClaims.Descriptor.Order + 500)
                .UseInlineHandler(context =>
                {
                    context.Principal!.SetClaim(OpenIddictConstants.Claims.Issuer, context.Configuration.Issuer!.AbsoluteUri);
                    return ValueTask.CompletedTask;
                }));
            options.UseSystemNetHttp();
            options.UseAspNetCore();
        });
        return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    }
}
