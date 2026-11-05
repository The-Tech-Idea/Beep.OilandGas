using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Beep.OilandGas.ApiService.Hubs;

public sealed class ProgressHubLifetimeManager(ILogger<DefaultHubLifetimeManager<ProgressHub>> logger,
    IServiceScopeFactory scopes, Action<Exception, string, string> reportFailure) : CompanyHubLifetimeManager<ProgressHub>(logger, scopes)
{
    protected override bool Supports(CompanyHubChannel channel) => channel.Kind is
        CompanyChannelKind.Operation or CompanyChannelKind.Workflow;
    protected override string? RequiredRole(CompanyHubChannel channel, object?[] args) => null;
    protected override object?[] DeliveryArguments(CompanyHubChannel channel, object?[] args)
    {
        if (args.Length != 1 || args[0] is not CompanyProgressNotification notification ||
            notification.Scope != channel.Scope || notification.Kind != channel.Kind || notification.Resource != channel.Resource)
            throw new InvalidOperationException("Progress publishers must supply the matching company/account ownership envelope.");
        return [notification.Payload];
    }
    protected override Task<bool> ResourceAccessAsync(IServiceProvider services, CompanyHubChannel channel,
        RepositoryCompanyContext company, object?[]? args)
    {
        var state = services.GetRequiredService<CompanyProgressState>();
        return Task.FromResult(args is null ? state.CanRead(company, channel) :
            args.Length == 1 && args[0] is CompanyProgressNotification notification && state.CanDeliver(company, channel, notification));
    }
    protected override void ReportFailure(Exception exception, string operation, string outcome) =>
        reportFailure(exception, operation, outcome);
}
