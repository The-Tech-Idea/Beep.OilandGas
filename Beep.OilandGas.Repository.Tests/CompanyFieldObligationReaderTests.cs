using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Models;
using Moq;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyFieldObligationReaderTests
{
    [Theory]
    [InlineData("write")]
    [InlineData("read")]
    [InlineData("revoked")]
    public async Task ObligationWriterUsesActualCompanyFieldPolicyWithinTheOwnedTransaction(string mode)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", mode == "read" ? "READ" : "WRITE", inherit: true);
        f.ObligationProjectionAsDictionary = true;
        f.EnableCorrectiveActionTransactions();
        f.Rows.AddRange([new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" }, new R_OBLIG_TYPE { LAND_OBLIG_TYPE = "REPORT", ACTIVE_IND = "Y" },
            new R_OBLIG_COMPONENT_TYPE { OBLIGATION_COMPONENT_TYPE = "FIELD", ACTIVE_IND = "Y" }]);
        var types = new[] { typeof(OBLIGATION), typeof(OBLIGATION_COMPONENT), typeof(R_OBLIG_TYPE), typeof(R_OBLIG_COMPONENT_TYPE) };
        f.Source.Setup(source => source.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns((EntityStructure requested, bool _) => {
            var type = types.Single(t => t.Name == requested.EntityName); var schema = new ClassCreator(f.Editor.Object).ConvertToEntityStructure(type);
            var keys = f.Metadata.Object.GetPrimaryKeyColumnsAsync(type.Name).GetAwaiter().GetResult().ToArray();
            foreach (var field in schema.Fields) { field.IsKey = keys.Contains(field.FieldName); if (field.Fieldtype == typeof(string).FullName) { field.Size = 512; field.Size1 = 512; } }
            return schema;
        });
        var review = new FieldSelectionReview(f.Gate.Company.Scope, await f.Gate.ResolveAsync(), "actor", "revision", "A");
        var command = new CreateFieldObligation(review, Guid.NewGuid(), "REPORT", "FIELD", "Recorded fact", null);
        var scope = new CompanyFieldObligationCommandScope(review, () => Task.CompletedTask, () => Task.CompletedTask);
        var writer = new CompanyFieldObligationWriter(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        if (mode == "revoked") f.AfterLeaseWrite = _ => f.Rows.OfType<ENT_COMPONENT>().Single().ACTIVE_IND = "N";
        if (mode == "write")
        {
            var result = await writer.CreateAsync(command, scope); Assert.Equal(review, result.Context);
            Assert.Single(f.Rows.OfType<OBLIGATION>()); Assert.Single(f.Rows.OfType<OBLIGATION_COMPONENT>()); Assert.Contains("commit", f.LeaseTransactions);
        }
        else
        {
            await Assert.ThrowsAnyAsync<Exception>(() => writer.CreateAsync(command, scope));
            Assert.Empty(f.Rows.OfType<OBLIGATION>()); Assert.Empty(f.Rows.OfType<OBLIGATION_COMPONENT>());
            if (mode == "revoked") Assert.Contains("rollback", f.LeaseTransactions);
        }
    }

    [Theory][InlineData("valid")][InlineData("denied")][InlineData("ignored-filter")][InlineData("changed")][InlineData("source-changed")]
    public async Task ActualBeepReaderBindsFieldAndRejectsUnconfirmedEvidence(string mode)
    {
        using var f=await Fixture.Create();
        f.ObligationProjectionAsDictionary = true;
        f.Source.Setup(source => source.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns((EntityStructure requested, bool _) =>
            new ClassCreator(f.Editor.Object).ConvertToEntityStructure(requested.EntityName == "OBLIGATION" ? typeof(OBLIGATION) : typeof(OBLIGATION_COMPONENT)));
        var review=new FieldSelectionReview(f.Gate.Company.Scope,await f.Gate.ResolveAsync(),"actor","revision","A");var current=review;
        var selections=new Mock<ICompanyFieldSelectionService>();selections.Setup(x=>x.GetCurrentFieldSelectionAsync()).ReturnsAsync(()=>new FieldResponse{FieldId="A",SelectionAvailable=true,SelectionContext=current});
        var access=new Mock<IAccessControlService>();access.Setup(x=>x.CheckAssetAccessAsync("actor","A","FIELD",PermissionConstants.Regulatory.ViewCompliance,null)).ReturnsAsync(()=>new AccessCheckResponse{HasAccess=mode!="denied"});
        f.Rows.AddRange([new OBLIGATION{OBLIGATION_ID="local",OBLIGATION_SEQ_NO=2,ACTIVE_IND="Y",OBLIGATION_TYPE="recorded",DESCRIPTION="source"},
            new OBLIGATION_COMPONENT{OBLIGATION_ID="local",OBLIGATION_SEQ_NO=2,COMPONENT_OBS_NO=1,ACTIVE_IND="Y",FIELD_ID="A"},
            new OBLIGATION{OBLIGATION_ID="foreign",OBLIGATION_SEQ_NO=2,ACTIVE_IND="Y"},new OBLIGATION_COMPONENT{OBLIGATION_ID="foreign",OBLIGATION_SEQ_NO=2,COMPONENT_OBS_NO=1,ACTIVE_IND="Y",FIELD_ID="B"}]);
        f.IgnoreFilters=mode=="ignored-filter";
        if(mode=="changed")f.AfterRead=()=>{current=review with{Revision="changed"};return Task.CompletedTask;};
        var reads=0;if(mode=="source-changed")f.AfterRead=()=>{if(++reads==3)f.Rows.OfType<OBLIGATION_COMPONENT>().Single(row=>row.FIELD_ID=="A").DESCRIPTION="changed";return Task.CompletedTask;};
        var reader=new CompanyFieldObligationReader(f.Gate,selections.Object,access.Object,f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object);
        if(mode=="valid")
        {
            var result=await reader.ReadAsync("actor");var item=Assert.Single(result.Obligations);Assert.Equal("local",item.ObligationId);Assert.Equal(2,item.ObligationSequence);Assert.Equal("recorded",item.ObligationType);Assert.Empty(f.Writes);
            f.Source.Verify(x=>x.GetEntityAsync("OBLIGATION_COMPONENT",It.Is<List<AppFilter>>(filters=>filters.Any(filter=>filter.FieldName=="FIELD_ID" && filter.FilterValue=="A"))),Times.Exactly(2));
        }
        else
        {
            await Assert.ThrowsAsync<RefusalException>(()=>reader.ReadAsync("actor"));Assert.Empty(f.Writes);
            if(mode=="denied")f.Source.Verify(x=>x.GetEntityAsync("OBLIGATION_COMPONENT",It.IsAny<List<AppFilter>>()),Times.Never);
        }
    }
}
