using Beep.OilandGas.PermitsAndApplications.Models;
using System.Reflection;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Web.Services;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class WorkOrderGrantAdministrationTests
{
    private static AssetAccess Row(bool inherit = false) => new()
    {
        UserId = "technician", OrganizationId = "company", AssetType = "WORK_ORDER", AssetId = "order/one",
        AccessLevel = "WRITE", Active = true, Inherit = inherit
    };
    private static CompanyClientContext Company(bool administrator = true)
    {
        var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), administrator ? ["Administrator"] : [], [])));
        return company;
    }
    private static AssetGrantAdministrationState State(CompanyClientContext company,
        Func<string, CancellationToken, Task<List<AssetAccess>?>> read,
        Func<string, GrantAccessRequest, CancellationToken, Task<bool>>? grant = null,
        Func<string, RevokeAccessRequest, CancellationToken, Task<bool>>? revoke = null) =>
        new(company, read, grant ?? ((_, _, _) => throw new InvalidOperationException("Unexpected write")),
            revoke ?? ((_, _, _) => throw new InvalidOperationException("Unexpected revoke")),
            (_, _, sentence) => sentence,
            DispatchProxy.Create<Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService, UserAssetAccessAdmissionTests.BoundaryProxy>());

    [Fact]
    public async Task DirectOrderGrantUsesOriginalSubjectAndCompanyAndRequiresReadback()
    {
        using var company = Company(); var reads = 0; var writes = 0;
        using var state = State(company, (path, _) =>
        {
            Assert.Equal("/api/AccessControl/user/technician/assets?includeInherited=false&organizationId=company", path);
            return Task.FromResult<List<AssetAccess>?>(++reads == 1 ? [] : [Row()]);
        }, (path, request, _) =>
        {
            writes++; Assert.Equal("/api/AccessControl/grant-access", path);
            Assert.Equal("technician", request.UserId); Assert.Equal("company", request.OrganizationId);
            Assert.Equal("WORK_ORDER", request.AssetType); Assert.Equal("order/one", request.AssetId);
            Assert.False(request.Inherit); Assert.Null(request.FacilityType); return Task.FromResult(true);
        });
        state.SelectUser("technician"); await state.LoadAsync(); state.SelectAssetType("WORK_ORDER");
        Assert.False(state.Inherit); state.AssetId = "order/one"; state.AccessLevel = "WRITE";
        await state.GrantAsync(); Assert.Equal(1, writes); Assert.Equal(2, reads);
        Assert.NotNull(state.Confirmation); Assert.True(state.CanRevoke(Assert.Single(state.Grants)));
    }

    [Fact]
    public async Task InheritedOrderResponseCannotEnableGrantAdministration()
    {
        using var company = Company(); using var state = State(company, (_, _) => Task.FromResult<List<AssetAccess>?>([Row(true)]));
        state.SelectUser("technician"); await state.LoadAsync();
        Assert.False(state.CanEdit); Assert.Empty(state.Grants); Assert.NotNull(state.Error);
    }

    [Fact]
    public async Task InheritedOrderDraftCannotSendACommand()
    {
        using var company = Company(); using var state = State(company, (_, _) => Task.FromResult<List<AssetAccess>?>([]));
        state.SelectUser("technician"); await state.LoadAsync(); state.SelectAssetType("WORK_ORDER");
        state.AssetId = "order/one"; state.Inherit = true; await state.GrantAsync();
        Assert.NotNull(state.Error); Assert.Null(state.Confirmation);
    }

    [Fact]
    public async Task DirectOrderRevocationRequiresRecordedAbsence()
    {
        using var company = Company(); var reads = 0;
        using var state = State(company, (_, _) => Task.FromResult<List<AssetAccess>?>(++reads == 1 ? [Row()] : []), revoke: (path, request, _) =>
        {
            Assert.Equal("/api/AccessControl/revoke-access", path); Assert.Equal("WORK_ORDER", request.AssetType);
            Assert.Equal("order/one", request.AssetId); Assert.Equal("technician", request.UserId); Assert.Null(request.FacilityType);
            return Task.FromResult(true);
        });
        state.SelectUser("technician"); await state.LoadAsync(); await state.RevokeAsync(Assert.Single(state.Grants));
        Assert.Empty(state.Grants); Assert.NotNull(state.Confirmation); Assert.Equal(2, reads);
    }

    [Fact]
    public async Task CompanySwitchRejectsLateOrderGrantReadback()
    {
        using var company = Company(); var pending = new TaskCompletionSource<List<AssetAccess>?>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var state = State(company, (_, _) => pending.Task); state.SelectUser("technician"); var load = state.LoadAsync();
        company.Clear(); pending.SetResult([Row()]); await load;
        Assert.Empty(state.Grants); Assert.False(state.CanEdit); Assert.False(state.Loaded); Assert.Empty(state.UserId);
    }

    [Fact]
    public async Task PersonaOrOrdinaryCompanyMembershipDoesNotGrantAdministration()
    {
        using var company = Company(false); using var state = State(company, (_, _) => throw new InvalidOperationException("Unexpected read"));
        state.SelectUser("technician"); await state.LoadAsync(); await state.GrantAsync();
        Assert.False(state.CanEdit); Assert.NotNull(state.Error);
    }
}
