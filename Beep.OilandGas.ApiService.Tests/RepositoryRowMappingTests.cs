using System.Data;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class RepositoryRowMappingTests
{
    [Fact]
    public void GeneratedRowsMapCaseInsensitiveColumnsAndScalarTypes()
    {
        var repository = new MappingRepository();
        var row = Assert.Single(repository.Map(new[] { new { id = "row", count = 4L, optional = (object)DBNull.Value } }));
        Assert.Equal("row", row.Id);
        Assert.Equal(4, row.Count);
        Assert.Null(row.Optional);
    }

    [Fact]
    public void DataTablesAndSingleDictionariesAreRowsNotUnrelatedObjects()
    {
        var repository = new MappingRepository();
        var table = new DataTable();
        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("Count", typeof(long));
        table.Rows.Add("table-row", 3L);
        Assert.Equal("table-row", Assert.Single(repository.Map(table)).Id);
        var values = new Dictionary<string, object> { ["Id"] = "dictionary-row", ["Count"] = 2L };
        Assert.Equal("dictionary-row", Assert.Single(repository.Map(values)).Id);
    }

    [Fact]
    public void InvalidRowsDoNotDisappearOrBecomeDefaultValuedSuccess()
    {
        var repository = new MappingRepository();
        Assert.Throws<InvalidOperationException>(() => repository.Map(new[] { new { Unrelated = "value" } }));
        Assert.Throws<InvalidOperationException>(() => repository.Map(new[] { new { Count = (object)DBNull.Value } }));
        Assert.Throws<FormatException>(() => repository.Map(new[] { new { Count = "invalid" } }));
    }

    public sealed class Row
    {
        public string Id { get; set; } = "";
        public int Count { get; set; }
        public int? Optional { get; set; }
    }

    private sealed class MappingRepository() : PPDMGenericRepository(Mock.Of<IDMEEditor>(),
        Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
        typeof(PROCESS_INSTANCE), "unused", "PROCESS_INSTANCE")
    {
        public List<Row> Map(object rows) => ConvertToTypedList(rows, typeof(Row)).Cast<Row>().ToList();
    }
}
