using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Hubs;

public sealed class WorkflowNotificationLifetimeManager(
    ILogger<DefaultHubLifetimeManager<WorkflowNotificationHub>> logger, IServiceScopeFactory scopes,
    IFailureReporter failures) : CompanyHubLifetimeManager<WorkflowNotificationHub>(logger, scopes)
{
    protected override bool Supports(CompanyHubChannel channel) => channel.Kind is
        CompanyChannelKind.User or CompanyChannelKind.Persona or CompanyChannelKind.Process;
    protected override string? RequiredRole(CompanyHubChannel channel, object?[] args) =>
        channel.Kind is CompanyChannelKind.User or CompanyChannelKind.Persona ? ((RoleNotification)args[0]!).RequiredRole : null;
    protected override object?[] DeliveryArguments(CompanyHubChannel channel, object?[] args)
    {
        if (channel.Kind == CompanyChannelKind.Process) return args;
        if (args.Length != 1 || args[0] is not RoleNotification notification ||
            channel.Kind == CompanyChannelKind.Persona && string.IsNullOrWhiteSpace(notification.RequiredRole))
            throw new InvalidOperationException("User and persona publishers must declare their delivery requirement.");
        return [notification.Payload];
    }
    protected override Task<bool> ResourceAccessAsync(IServiceProvider services, CompanyHubChannel channel,
        RepositoryCompanyContext company, object?[]? args)
    {
        var authorization = services.GetRequiredService<WorkflowNotificationAuthorization>();
        return channel.Kind switch
        {
            CompanyChannelKind.Persona => authorization.CanAccessPersonaAsync(company.UserId, channel.Resource, channel.Field!),
            CompanyChannelKind.Process => authorization.CanAccessProcessAsync(company.UserId, channel.Resource),
            _ => Task.FromResult(false)
        };
    }
    protected override void ReportFailure(Exception exception, string operation, string outcome) =>
        failures.ReportHandled(exception, operation, outcome);
}

public static class WorkflowNotificationServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflowNotificationAuthorization(this IServiceCollection services)
    {
        services.AddScoped<WorkflowNotificationAuthorization>();
        services.AddScoped<CompanyHubScope>();
        Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddSingleton<Func<ClaimsPrincipal, string?>>(services,
            (Func<ClaimsPrincipal, string?>)(principal => principal.FindActingUserId()));
        Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddScoped<INotificationPersonaReader, RepositoryNotificationPersonaReader>(services);
        services.AddSingleton<HubLifetimeManager<WorkflowNotificationHub>, WorkflowNotificationLifetimeManager>();
        return services;
    }
}
