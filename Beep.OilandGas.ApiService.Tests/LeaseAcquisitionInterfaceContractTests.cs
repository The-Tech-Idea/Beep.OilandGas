using System.Linq;
using Beep.OilandGas.Models.Core.Interfaces;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class LeaseAcquisitionInterfaceContractTests
{
    [Fact]
    public void CoreInterface_HasCanonicalSurfaceOnly()
    {
        var names = typeof(ILeaseAcquisitionService).GetMethods().Select(m => m.Name).OrderBy(n => n).ToArray();

        Assert.Equal(4, names.Length);
        Assert.Contains("CreateLeaseAcquisitionAsync", names);
        Assert.Contains("EvaluateLeaseAsync", names);
        Assert.Contains("GetAvailableLeasesAsync", names);
        Assert.Contains("UpdateLeaseStatusAsync", names);
    }

    [Fact]
    public void CoreInterface_DoesNotExposeAdvancedWorkflowMembers()
    {
        var names = typeof(ILeaseAcquisitionService).GetMethods().Select(m => m.Name).ToHashSet();

        Assert.DoesNotContain("InitiateLeaseAcquisitionAsync", names);
        Assert.DoesNotContain("SearchLeaseProspectsAsync", names);
        Assert.DoesNotContain("InitiateNegotiationAsync", names);
        Assert.DoesNotContain("SaveLeaseAcquisitionAsync", names);
    }
}
