using Beep.OilandGas.Models.Data.Company;
using System.Net;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.Exceptions;
using Beep.OilandGas.Models.Data;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class WorkOrderAccessClientTests
{
    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => send(request);
    }
    private static HttpResponseMessage Json(object data) => new(HttpStatusCode.OK)
        { Content = new StringContent(JsonSerializer.Serialize(data, new JsonSerializerOptions(JsonSerializerDefaults.Web))) };

    [Theory][InlineData(false)][InlineData(true)]
    public async Task DirectOrderListUsesCompanyTransportAndRefusesInheritedOrder(bool inherited)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), [], [])));
        using var http = new HttpClient(new Handler(request =>
        {
            Assert.Equal("/api/AccessControl/user/technician/assets?includeInherited=false&assetType=WORK_ORDER&organizationId=company", request.RequestUri!.PathAndQuery);
            Assert.Equal("company", Assert.Single(request.Headers.GetValues(CompanyRequestHeaders.Company)));
            Assert.Equal("tenant", Assert.Single(request.Headers.GetValues(CompanyRequestHeaders.Tenant)));
            return Task.FromResult(Json(new[] { new AssetAccess { UserId = "technician", OrganizationId = "company",
                AssetType = "WORK_ORDER", AssetId = "order", AccessLevel = "WRITE", Active = true, Inherit = inherited } }));
        }));
        var client = new Beep.OilandGas.Client.App.Services.AccessControl.AccessControlService(
            new ClientApiTransport(http, new AppOptions { ApiBaseUrl = "https://api.test", MaxRetries = 0 }, company));
        if (inherited) await Assert.ThrowsAsync<ApiClientException>(() => client.GetUserAccessibleAssetsAsync("technician", "work_order", includeInherited: false, organizationId: "company"));
        else Assert.Equal("order", Assert.Single(await client.GetUserAccessibleAssetsAsync("technician", "work_order", includeInherited: false, organizationId: "company")).AssetId);
    }

    [Theory][InlineData("WRITE",true)][InlineData("READ",false)]
    public async Task PositiveOrderWriteDecisionMustMeetRequestedResourceLevel(string level, bool accepted)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), [], [])));
        using var http = new HttpClient(new Handler(async request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method); Assert.Equal("/api/AccessControl/check-access", request.RequestUri!.AbsolutePath);
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            Assert.Equal("WORK_ORDER", body.RootElement.GetProperty("assetType").GetString());
            Assert.Equal("WRITE", body.RootElement.GetProperty("requiredPermission").GetString());
            return Json(new AccessCheckResponse { HasAccess = true, AccessLevel = level });
        }));
        var client = new Beep.OilandGas.Client.App.Services.AccessControl.AccessControlService(
            new ClientApiTransport(http, new AppOptions { ApiBaseUrl = "https://api.test", MaxRetries = 0 }, company));
        if (accepted) Assert.True(await client.CheckAssetAccessAsync("technician", "order", "work_order", requiredPermission: "WRITE"));
        else await Assert.ThrowsAsync<ApiClientException>(() => client.CheckAssetAccessAsync("technician", "order", "work_order", requiredPermission: "WRITE"));
    }
}
