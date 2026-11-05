#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class WorkOrderPersonnelSourceTests
{
    [Theory][InlineData("current")][InlineData("unlinked")][InlineData("missing-license")][InlineData("foreign")][InlineData("duplicate")][InlineData("drift")][InlineData("authority")][InlineData("schema")]
    public async Task NativePersonnelSourceClosesCompanyMembershipBeforeLicencesAndNeverWrites(string mode)
    {
        var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();var proof=new WorkOrderMaterialAcceptanceFixture();var context=proof.Command.Review.PermitReview.Context;var query=new CompanyWorkOrderPersonnelSourceQuery(proof.Command.Review.Query.Permit,"employee","license");var utc=DateTime.UtcNow;var employee=new BA_EMPLOYEE{EMPLOYER_BA_ID="company",EMPLOYEE_BA_ID="employee",EMPLOYEE_OBS_NO=1,ACTIVE_IND="Y",EMPLOYEE_POSITION="Recorded operator",STATUS="ACTIVE",EFFECTIVE_DATE=utc.AddDays(-2),EXPIRY_DATE=utc.AddDays(2)};var license=new BA_LICENSE{LICENSEE_BA_ID="employee",LICENSE_ID="license",ACTIVE_IND="Y",LICENSE_TYPE="Recorded certificate",GRANTED_BY_BA_ID="reported-authority",GRANTED_DATE=utc.AddDays(-1),EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(2)};if(mode!="unlinked")f.Rows.Add(employee);if(mode!="missing-license")f.Rows.Add(license);if(mode=="duplicate")f.Rows.Add(CompanyInventorySourceReview.Copy(employee));if(mode=="foreign")employee.EMPLOYER_BA_ID="other";
        var metadata=new Mock<IPPDMMetadataRepository>();foreach(var (table,type,keys) in new[]{("BA_EMPLOYEE",typeof(BA_EMPLOYEE),new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"}),("BA_LICENSE",typeof(BA_LICENSE),new[]{"LICENSEE_BA_ID","LICENSE_ID"})}){metadata.Setup(m=>m.GetTableMetadataAsync(table)).ReturnsAsync(new PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=string.Join(',',keys)});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync(keys);f.Source.Setup(s=>s.CheckEntityExist(table)).Returns(true);}
        var creator=new ClassCreator(f.Editor.Object);f.Source.Setup(s=>s.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool _)=>{var shape=creator.ConvertToEntityStructure(request.EntityName=="BA_EMPLOYEE"?typeof(BA_EMPLOYEE):typeof(BA_LICENSE));foreach(var field in shape.Fields){field.IsKey=request.EntityName=="BA_EMPLOYEE"?new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"}.Contains(field.FieldName):new[]{"LICENSEE_BA_ID","LICENSE_ID"}.Contains(field.FieldName);if(field.IsKey)field.AllowDBNull=false;}if(mode=="schema")shape.Fields.First(field=>field.IsKey).AllowDBNull=true;return shape;});
        var licenceReads=0;f.Source.Setup(s=>s.GetEntityAsync("BA_LICENSE",It.IsAny<List<AppFilter>>())).ReturnsAsync((string _,List<AppFilter> filters)=>{licenceReads++;if(mode=="drift" && licenceReads==2)license.LICENSE_TYPE="changed";return f.Rows.OfType<BA_LICENSE>().Select(CompanyInventorySourceReview.Copy).ToArray();});
        if(mode=="foreign")f.Source.Setup(s=>s.GetEntityAsync("BA_EMPLOYEE",It.IsAny<List<AppFilter>>())).ReturnsAsync(()=>new[]{employee});
        var owner=new ContractorManagementService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((context.Company,context.ActorUserId,context.Database)),NullLogger<ContractorManagementService>.Instance);Task Guard()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;
        var error=await Record.ExceptionAsync(async()=>{var result=await owner.ReadPersonnelSourceAsync(context,query,Guard);result.Require();if(mode=="unlinked"){Assert.Null(result.Employment);Assert.Null(result.License);Assert.Equal(0,licenceReads);}else{Assert.Equal("Recorded operator",result.Employment!.Position);Assert.Equal(mode=="missing-license",result.License is null);}});
        if(mode is "current" or "unlinked" or "missing-license")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(0,f.Writes);Assert.Empty(f.Transactions);
    }
    [Fact] public async Task LegacyQualificationShortcutRefusesUnrecordedLevelsAndCountryDefaultsBeforePersonalReads()
    {
        var f=new PersonaTaskRoutingTests.Fixture();var owner=new ContractorManagementService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),()=>Task.FromResult((new CompanyScope("tenant","company"),"reviewer",new AssetDatabaseScope("lifecycle","fingerprint"))),NullLogger<ContractorManagementService>.Instance);
        var error=await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>owner.ValidateContractorAsync("employee","unconfigured-type","reported-jurisdiction"));Assert.Equal(Beep.OilandGas.Models.Core.Refusals.RefusalKind.Conflict,error.Kind);f.Source.Verify(source=>source.GetEntityAsync("BA_LICENSE",It.IsAny<List<AppFilter>>()),Times.Never);Assert.Equal(0,f.Writes);
    }
}
#endif
