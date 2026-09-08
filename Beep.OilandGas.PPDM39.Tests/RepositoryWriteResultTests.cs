using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.PPDM39.Tests;

public class RepositoryWriteResultTests
{
    public static IEnumerable<object?[]> Results()
    {
        yield return new object?[] { null, false };
        foreach (var flag in Enum.GetValues<Errors>())
            yield return new object?[] { new ErrorsInfo { Flag = flag, Message = "provider detail" }, flag == Errors.Ok };
        yield return new object?[]
        {
            new ErrorsInfo { Flag = Errors.Ok, Errors = new() { new ErrorsInfo { Flag = Errors.Failed, Message = "row failed" } } }, false
        };
    }

    [Theory]
    [MemberData(nameof(Results))]
    public async Task WritesRequireExplicitSuccess(IErrorsInfo? result, bool success)
    {
        var source = new Mock<IDataSource>();
        source.Setup(s => s.InsertEntity("WELL", It.IsAny<object>())).Returns(result!);
        source.Setup(s => s.UpdateEntity("WELL", It.IsAny<object>())).Returns(result!);
        source.Setup(s => s.DeleteEntity("WELL", It.IsAny<object>())).Returns(result!);
        var editor = new Mock<IDMEEditor>();
        editor.Setup(e => e.GetDataSource("PPDM39")).Returns(source.Object);
        var repository = new PPDMGenericRepository(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), typeof(WELL));
        var well = new WELL { UWI = "TEST-WELL" };
        Func<Task>[] writes =
        {
            () => repository.InsertAsync(well, "actor"),
            () => repository.UpdateAsync(well, "actor"),
            () => repository.InsertBatchAsync(new[] { well }, "actor"),
            () => repository.UpdateBatchAsync(new[] { well }, "actor")
        };
        foreach (var write in writes)
        {
            if (success)
                await write();
            else
                await Assert.ThrowsAsync<InvalidOperationException>(write);
        }
        Assert.Equal(success, await repository.DeleteAsync(well));
        source.Verify(s => s.InsertEntity("WELL", well), Times.Exactly(2));
        source.Verify(s => s.UpdateEntity("WELL", well), Times.Exactly(2));
        source.Verify(s => s.DeleteEntity("WELL", well), Times.Once);
    }
}
