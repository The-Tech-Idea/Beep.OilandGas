using TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LSE-01: the derived expiry state, the acreage unit rule, the register filter and the export - the parts that
/// do not need a database. The merged read is proven against real tables in the gateway test.
/// </summary>
public sealed class LeaseRegisterTests
{
    private static readonly DateTime AsOf = new(2026, 6, 1);

    [Fact]
    public void The_state_comes_from_the_dates_not_a_stored_column()
    {
        Assert.Equal(LeaseStatus.Expired, LeaseStatus.Of(new DateTime(2025, 1, 1), new DateTime(2026, 1, 1), AsOf));
        Assert.Equal(LeaseStatus.Pending, LeaseStatus.Of(new DateTime(2026, 9, 1), new DateTime(2027, 9, 1), AsOf));
        Assert.Equal(LeaseStatus.Expiring, LeaseStatus.Of(new DateTime(2025, 1, 1), new DateTime(2026, 7, 1), AsOf));
        Assert.Equal(LeaseStatus.Active, LeaseStatus.Of(new DateTime(2025, 1, 1), new DateTime(2027, 1, 1), AsOf));
        Assert.Equal(LeaseStatus.Unknown, LeaseStatus.Of(null, null, AsOf));
    }

    [Fact]
    public void An_acreage_without_a_unit_says_so_rather_than_showing_a_bare_number()
    {
        var lease = Lease(Acreage: 120m, AcreageUnit: null);

        Assert.False(lease.HasAcreageUnit);
        Assert.Equal("120 (unit not recorded)", lease.AcreageText);
        Assert.Equal("(no acreage) acres", Lease(Acreage: null, AcreageUnit: "acres").AcreageText);
    }

    [Fact]
    public void A_filter_matches_every_field_that_is_set()
    {
        var leases = new[]
        {
            Lease(Id: "L-1", Kind: "FEE_MINERAL", Region: "NORTH", Operator: "OP-A", Expiry: new DateTime(2026, 7, 1)),
            Lease(Id: "L-2", Kind: "GOVERNMENT", Region: "SOUTH", Operator: "OP-B", Expiry: new DateTime(2027, 7, 1)),
        };

        var shown = OgLeaseRegisterService.Filter(
            leases,
            new OgLeaseFilter(Kind: "FEE_MINERAL", Region: "NORTH", Operator: "OP-A", ExpiringWithinDays: 60),
            AsOf).ToList();

        Assert.Equal("L-1", Assert.Single(shown).Id);
    }

    [Fact]
    public void The_export_is_built_from_the_rows_the_register_holds()
    {
        var register = new OgLeaseRegister(
            [Lease(Id: "L-1", LeaseNumber: "LN-1", Kind: "FEE_MINERAL", Region: "NORTH", Expiry: new DateTime(2026, 7, 1))],
            ["FEE_MINERAL"],
            ["NORTH"],
            ["OP-A"],
            AsOf);

        var csv = register.ToCsv();

        Assert.Contains("Lease number,Type,Name,Acreage,Acreage unit", csv, StringComparison.Ordinal);
        Assert.Contains("LN-1,FEE_MINERAL", csv, StringComparison.Ordinal);
        Assert.Contains("expiring", csv, StringComparison.Ordinal);
        Assert.Contains("12.5%", csv, StringComparison.Ordinal);
        Assert.Contains("80%", csv, StringComparison.Ordinal);
    }

    private static OgLeaseSummary Lease(
        string Id = "L-1",
        string? LeaseNumber = null,
        string Kind = "LEASE",
        string? Region = "NORTH",
        string? Operator = "OP-A",
        decimal? Acreage = 100m,
        string? AcreageUnit = "acres",
        DateTime? Expiry = null) =>
        new(
            Id,
            LeaseNumber,
            Kind,
            Name: "Lease " + Id,
            Acreage,
            AcreageUnit,
            Region,
            County: null,
            Operator,
            Effective: new DateTime(2025, 1, 1),
            Expiry,
            RoyaltyRate: 0.125m,
            NetRevenueInterest: 0.8m,
            FieldId: null,
            StoredStatus: null,
            Status: LeaseStatus.Of(new DateTime(2025, 1, 1), Expiry, AsOf),
            Source: Id);

}
