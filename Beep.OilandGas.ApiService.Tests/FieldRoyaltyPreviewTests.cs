using Beep.OilandGas.LifeCycle.Services.Accounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class FieldRoyaltyPreviewTests
{
    [Fact]
    public async Task SelectsFieldAndInclusiveDatesThenUsesPreviewOnly()
    {
        var editor = new Mock<IDMEEditor>();
        var source = new Mock<IDataSource>();
        editor.Setup(e => e.GetDataSource("selected-db")).Returns(source.Object);
        source.Setup(s => s.GetEntityAsync("RUN_TICKET", It.Is<List<AppFilter>>(filters =>
            filters.Any(f => f.FieldName == "FIELD_ID" && f.FilterValue == "field-a") &&
            filters.Any(f => f.FieldName == "ACTIVE_IND" && f.FilterValue == "Y") &&
            filters.Any(f => f.FieldName == "TICKET_DATE_TIME" && f.Operator == ">=" && f.FilterValue == "2026-09-01") &&
            filters.Any(f => f.FieldName == "TICKET_DATE_TIME" && f.Operator == "<" && f.FilterValue == "2026-09-03"))))
            .ReturnsAsync(new object[] { new RUN_TICKET { RUN_TICKET_ID = "ticket" } });
        var allocations = new Mock<IAllocationService>(MockBehavior.Strict);
        allocations.Setup(a => a.GetHistoryAsync("ticket", "selected-db")).ReturnsAsync(new List<ALLOCATION_RESULT>
            { new() { ALLOCATION_RESULT_ID = "allocation", ALLOCATION_REQUEST_ID = "ticket" } });
        var detail = new ALLOCATION_DETAIL { ALLOCATION_DETAIL_ID = "detail", ALLOCATION_RESULT_ID = "allocation" };
        allocations.Setup(a => a.GetDetailsAsync("allocation", "selected-db")).ReturnsAsync(new List<ALLOCATION_DETAIL> { detail });
        var royalties = new Mock<IRoyaltyService>(MockBehavior.Strict);
        royalties.Setup(r => r.PreviewAsync(detail, "actor", "selected-db")).ReturnsAsync(new ROYALTY_CALCULATION
            { ROYALTY_STATUS = "PREVIEW", ROYALTY_AMOUNT = 100m });
        var service = new PPDMAccountingService(editor.Object, new Mock<ICommonColumnHandler>().Object,
            new Mock<IPPDM39DefaultsRepository>().Object, new Mock<IPPDMMetadataRepository>().Object,
            royaltyService: royalties.Object, allocationService: allocations.Object, resolveProductionConnection: () => Task.FromResult("selected-db"));

        var result = await service.PreviewRoyaltiesAsync("field-a", new DateTime(2026, 9, 1), new DateTime(2026, 9, 2), "actor");

        Assert.Equal(100m, Assert.Single(result).ROYALTY_AMOUNT);
        source.VerifyAll();
        royalties.Verify(r => r.PreviewAsync(detail, "actor", "selected-db"), Times.Once);
        royalties.VerifyNoOtherCalls();
        source.Verify(s => s.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }
}
