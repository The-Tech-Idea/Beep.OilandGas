using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Development;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.Web.Services;

public interface IDevelopmentServiceClient
{
    Task<DevelopmentDashboardSummary?> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task<List<DevelopmentWellStatusDto>> GetDashboardWellsAsync(CancellationToken cancellationToken = default);
    Task<DevelopmentConstructionProgressDto?> GetConstructionProgressAsync(CancellationToken cancellationToken = default);
    Task<List<WELL>> GetWellsAsync(CancellationToken cancellationToken = default);
    Task<WELL?> GetWellAsync(string uwi, CancellationToken cancellationToken = default);
    Task<bool> AssignRigAsync(string uwi, DevelopmentAssignRigRequest request, CancellationToken cancellationToken = default);
    Task<FdpStatusResponse?> GetFdpStatusAsync(CancellationToken cancellationToken = default);
    Task<SubmitFdpDraftResponse?> SubmitFdpAsync(SubmitFdpDraftRequest request, CancellationToken cancellationToken = default);
    Task<List<POOL>> GetPoolsAsync(CancellationToken cancellationToken = default);
    Task<POOL?> CreatePoolAsync(PoolRequest request, string? userId = null, CancellationToken cancellationToken = default);
    Task<POOL?> UpdatePoolAsync(string poolId, PoolRequest request, string? userId = null, CancellationToken cancellationToken = default);
    Task<List<FACILITY>> GetFacilitiesAsync(CancellationToken cancellationToken = default);
    Task<FacilityResponse?> CreateFacilityAsync(FacilityRequest request, string? userId = null, CancellationToken cancellationToken = default);
    Task<FacilityResponse?> UpdateFacilityAsync(string facilityId, FacilityRequest request, string? userId = null, CancellationToken cancellationToken = default);
}

public sealed class DevelopmentConstructionProgressDto
{
    public List<DevelopmentWorkGroupDto> WorkGroups { get; set; } = new();
    public List<DevelopmentPunchItemDto> PunchItems { get; set; } = new();
}

public sealed class DevelopmentWorkGroupDto
{
    public string Title { get; set; } = string.Empty;
    public List<DevelopmentWorkPackageDto> Packages { get; set; } = new();
}

public sealed class DevelopmentWorkPackageDto
{
    public string Name { get; set; } = string.Empty;
    public string TargetDate { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsActive { get; set; }
    public double PercentComplete { get; set; }
    public string Note { get; set; } = string.Empty;
}

public sealed class DevelopmentPunchItemDto
{
    public string Description { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public sealed class DevelopmentAssignRigRequest
{
    public string RigName { get; set; } = string.Empty;
    public DateTime? MobDate { get; set; }
}