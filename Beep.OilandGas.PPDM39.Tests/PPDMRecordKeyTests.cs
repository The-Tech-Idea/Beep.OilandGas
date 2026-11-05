using System.Globalization;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Xunit;

namespace Beep.OilandGas.PPDM39.Tests;

/// <summary>
/// The one way a PPDM record is named on a data-management address, and the one answer to which class a table's rows are
/// (2026-10-10): the web's generic table page writes the key, the API reads it, and both read the rows as the same class.
/// </summary>
public class PPDMRecordKeyTests
{
    private static readonly PPDMTableMetadata Activity = new()
    {
        TableName = "well_activity",
        EntityTypeName = "WELL_ACTIVITY",
        PrimaryKeyColumn = "UWI, SOURCE, ACTIVITY_OBS_NO"
    };

    [Fact]
    public void A_key_is_its_columns_values_in_key_order_written_in_the_invariant_culture()
    {
        var culture = CultureInfo.CurrentCulture;
        try
        {
            // A culture writing "2,5" would put the separator inside a value.
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            var record = new WELL_ACTIVITY { SOURCE = "SRC", UWI = "W1", ACTIVITY_OBS_NO = 2.5m };

            Assert.Equal("W1,SRC,2.5", PPDMRecordKey.Of(record, Activity));
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("A,B")]
    public void A_record_without_an_addressable_value_for_a_key_column_has_no_key(string? source)
    {
        var record = new WELL_ACTIVITY { UWI = "W1", SOURCE = source!, ACTIVITY_OBS_NO = 1m };

        Assert.Null(PPDMRecordKey.Of(record, Activity));
    }

    [Fact]
    public void A_table_naming_no_key_gives_its_records_none()
    {
        Assert.Null(PPDMRecordKey.Of(new WELL { UWI = "W1" }, new PPDMTableMetadata { TableName = "well" }));
        Assert.Empty(PPDMRecordKey.Columns(null));
    }

    [Fact]
    public void A_key_reads_back_as_each_columns_value()
    {
        var parts = PPDMRecordKey.Read("W1,SRC,2", Activity);

        Assert.NotNull(parts);
        Assert.Equal(new[] { "UWI", "SOURCE", "ACTIVITY_OBS_NO" }, parts!.Select(part => part.Key));
        Assert.Equal(new[] { "W1", "SRC", "2" }, parts.Select(part => part.Value));
    }

    [Theory]
    [InlineData("W1,SRC")]
    [InlineData("W1,SRC,2,3")]
    [InlineData("W1,,2")]
    [InlineData("")]
    public void A_key_that_does_not_give_one_value_for_every_key_column_reads_as_none(string key)
    {
        Assert.Null(PPDMRecordKey.Read(key, Activity));
    }

    [Theory]
    [InlineData("WELL", "anything", typeof(WELL))]
    [InlineData(null, "well_activity", typeof(WELL_ACTIVITY))]
    [InlineData("NO_SUCH_CLASS", "WELL", typeof(WELL))]
    [InlineData(null, "no_such_table", null)]
    public void A_tables_class_is_the_one_its_metadata_names_else_the_one_named_like_it(string? entityTypeName, string table, Type? expected)
    {
        Assert.Equal(expected, PPDMEntityTypes.Find(entityTypeName, table));
    }
}
