using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Beep.OilandGas.ApiService.Hubs;

/// <summary>SignalR invocation scopes do not inherit request DI state; rebind from validated handshake evidence.</summary>
public sealed class CompanyHubFilter : IHubFilter
{
    private static async Task BindAsync(HubCallerContext caller, IServiceProvider services)
    {
        var handshake = caller.GetHttpContext()?.Features.Get<RepositoryCompanyContext>();
        if (await services.GetRequiredService<CompanyHubScope>().RestoreAsync(caller.User, handshake, caller.ConnectionAborted) is null)
        {
            caller.Abort();
            throw new HubException("The selected company is unavailable to this account. Reconnect after selecting an available company.");
        }
    }

    public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext context, Func<HubInvocationContext, ValueTask<object?>> next)
    { await BindAsync(context.Context, context.ServiceProvider); return await next(context); }
    public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    { await BindAsync(context.Context, context.ServiceProvider); await next(context); }
}
