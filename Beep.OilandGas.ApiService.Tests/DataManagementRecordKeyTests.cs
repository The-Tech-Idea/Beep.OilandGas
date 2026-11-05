using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// The generic data-management endpoints name a record by its whole key (<see cref="PPDMRecordKey"/>) — 2026-10-10. The
/// delete handed the key to the repository's <c>DeleteAsync</c>, which takes the record itself, so it never deleted
/// anything; the web's generic table page never reached it, because it found no key on its untyped rows.
/// </summary>
public class DataManagementRecordKeyTests
{
    private static readonly PPDMTableMetadata Activity = new()
    {
        TableName = "well_activity",
        EntityTypeName = "WELL_ACTIVITY",
        PrimaryKeyColumn = "UWI,SOURCE,ACTIVITY_OBS_NO"
    };

    [Fact]
    public async Task Delete_SoftDeletesTheOneRecordItsCompositeKeyNames()
    {
        var kept = new WELL_ACTIVITY { UWI = "W1", SOURCE = "SRC", ACTIVITY_OBS_NO = 1m, ACTIVE_IND = "Y" };
        var named = new WELL_ACTIVITY { UWI = "W1", SOURCE = "SRC", ACTIVITY_OBS_NO = 2m, ACTIVE_IND = "Y" };
        var harness = new Harness(Activity, kept, named);

        var answer = await harness.Controller.DeleteTableRecord("well_activity", "W1,SRC,2");

        Assert.IsType<NoContentResult>(answer);
        harness.Columns.Verify(c => c.SoftDelete(named, "user-1"), Times.Once);
        harness.Columns.Verify(c => c.SoftDelete(kept, It.IsAny<string>()), Times.Never);
        harness.Source.Verify(s => s.UpdateEntity("well_activity", named), Times.Once);
        harness.Source.Verify(s => s.DeleteEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        // A number in a key is matched as the key writes it, never handed to the data source as text.
        Assert.DoesNotContain(harness.Filters, filter => filter.FieldName == "ACTIVITY_OBS_NO");
        Assert.Contains(harness.Filters, filter => filter.FieldName == "UWI" && filter.FilterValue == "W1");
    }

    [Fact]
    public async Task Delete_FindsAKeyHoldingASlash()
    {
        var well = new WELL { UWI = "100/06-11-033-24W4/00", ACTIVE_IND = "Y" };
        var harness = new Harness(new PPDMTableMetadata { TableName = "well", EntityTypeName = "WELL", PrimaryKeyColumn = "UWI" }, well);

        // Kestrel leaves an escaped slash escaped in a route value.
        var answer = await harness.Controller.DeleteTableRecord("well", "100%2F06-11-033-24W4%2F00");

        Assert.IsType<NoContentResult>(answer);
        harness.Source.Verify(s => s.UpdateEntity("well", well), Times.Once);
    }

    [Fact]
    public async Task Delete_AnswersNotFoundWhenNoRecordHasTheKey()
    {
        var harness = new Harness(Activity, new WELL_ACTIVITY { UWI = "W1", SOURCE = "SRC", ACTIVITY_OBS_NO = 1m });

        var answer = await harness.Controller.DeleteTableRecord("well_activity", "W1,SRC,9");

        Assert.IsType<NotFoundObjectResult>(answer);
        harness.Source.Verify(s => s.UpdateEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Delete_RefusesAKeyThatDoesNotNameEveryKeyColumn()
    {
        var harness = new Harness(Activity);

        var refusal = await Assert.ThrowsAsync<RefusalException>(() => harness.Controller.DeleteTableRecord("well_activity", "W1,SRC"));

        Assert.Equal(RefusalKind.Invalid, refusal.Kind);
        harness.Source.Verify(s => s.UpdateEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Update_RefusesABodyNamingAnotherRecordThanItsAddress()
    {
        var harness = new Harness(Activity);
        var body = JsonSerializer.SerializeToElement(new WELL_ACTIVITY { UWI = "W1", SOURCE = "SRC", ACTIVITY_OBS_NO = 3m });

        var refusal = await Assert.ThrowsAsync<RefusalException>(() =>
            harness.Controller.UpdateTableRecord("well_activity", "W1,SRC,2", body));

        Assert.Equal(RefusalKind.Invalid, refusal.Kind);
        harness.Source.Verify(s => s.UpdateEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Update_ChangesTheRecordItsAddressNames()
    {
        var harness = new Harness(Activity);
        var body = JsonSerializer.SerializeToElement(new WELL_ACTIVITY { UWI = "W1", SOURCE = "SRC", ACTIVITY_OBS_NO = 2m, REMARK = "changed" });

        var answer = await harness.Controller.UpdateTableRecord("well_activity", "W1,SRC,2", body);

        Assert.IsType<OkObjectResult>(answer.Result);
        harness.Source.Verify(s => s.UpdateEntity("well_activity",
            It.Is<object>(row => row is WELL_ACTIVITY && ((WELL_ACTIVITY)row).REMARK == "changed")), Times.Once);
    }

    private sealed class Harness
    {
        public Mock<IDataSource> Source { get; } = new();
        public Mock<ICommonColumnHandler> Columns { get; } = new();
        public List<AppFilter> Filters { get; } = new();
        public DataManagementController Controller { get; }

        public Harness(PPDMTableMetadata metadata, params object[] rows)
        {
            Source.Setup(s => s.GetEntityAsync(metadata.TableName, It.IsAny<List<AppFilter>>()))
                .Callback<string, List<AppFilter>>((_, filters) => Filters.AddRange(filters))
                .ReturnsAsync(rows);
            Source.Setup(s => s.UpdateEntity(metadata.TableName, It.IsAny<object>()))
                .Returns(new ErrorsInfo { Flag = Errors.Ok });
            var editor = new Mock<IDMEEditor>();
            editor.Setup(e => e.GetDataSource("PPDM39")).Returns(Source.Object);
            Columns.Setup(c => c.SoftDelete(It.IsAny<IPPDMEntity>(), It.IsAny<string>()))
                .Callback<IPPDMEntity, string>((entity, _) => entity.ACTIVE_IND = "N");
            var tables = new Mock<IPPDMMetadataRepository>();
            tables.Setup(m => m.GetTableMetadataAsync(It.IsAny<string>())).ReturnsAsync(metadata);

            Controller = new DataManagementController(editor.Object, Columns.Object, Mock.Of<IPPDM39DefaultsRepository>(),
                tables.Object, Mock.Of<IPPDMDataValidationService>(), Mock.Of<IPPDMDataQualityService>(),
                Mock.Of<IPPDMDataQualityDashboardService>(), NullLogger<DataManagementController>.Instance)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "user-1")], "TestAuth"))
                    }
                }
            };
        }
    }
}
