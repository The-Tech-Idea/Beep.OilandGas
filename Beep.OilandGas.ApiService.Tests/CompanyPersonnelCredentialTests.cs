#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyPersonnelCredentialTests
{
    [Theory][InlineData("current")][InlineData("unlinked")][InlineData("missing-license")][InlineData("zero")][InlineData("foreign")][InlineData("duplicate")][InlineData("condition-drift")][InlineData("membership-drift")][InlineData("authority")][InlineData("schema")][InlineData("borrowed")][InlineData("tracked-status")][InlineData("transient-licence")]
    public async Task NativeCredentialReadClosesMembershipBeforeStatusConditionsAndRefusesUnknownOrChangingEvidence(string mode)
    {
        var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();var proof=WorkOrderPersonnelReviewFixture.Original();var context=proof.Review.Personnel.Context;var query=proof.Review.Personnel.Query;var utc=DateTime.UtcNow;
        var employee=new BA_EMPLOYEE{EMPLOYER_BA_ID="company",EMPLOYEE_BA_ID=query.EmployeeBaId,EMPLOYEE_OBS_NO=1,ACTIVE_IND="Y",EMPLOYEE_POSITION="Recorded operator",STATUS="ACTIVE",EFFECTIVE_DATE=utc.AddDays(-2),EXPIRY_DATE=utc.AddDays(2)};
        var license=new BA_LICENSE{LICENSEE_BA_ID=query.EmployeeBaId,LICENSE_ID=query.LicenseId,ACTIVE_IND="Y",LICENSE_TYPE="Recorded licence",GRANTED_BY_BA_ID="authority",GRANTED_DATE=utc.AddDays(-1),EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(2)};
        var status=new BA_LICENSE_STATUS{LICENSEE_BA_ID=query.EmployeeBaId,LICENSE_ID=query.LicenseId,STATUS_ID="status",ACTIVE_IND="Y",LICENSE_STATUS="VALID",LICENSE_STATUS_TYPE="CERTIFICATION",EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(2)};
        var condition=new BA_LICENSE_COND{LICENSEE_BA_ID=query.EmployeeBaId,LICENSE_ID=query.LicenseId,CONDITION_ID="condition",ACTIVE_IND="Y",CONDITION_CODE="TRAINING_HOURS",CONDITION_TYPE="TRAINING",CONDITION_VALUE=mode=="zero"?0:8m,CONDITION_VALUE_UOM="HOUR",EXEMPT_IND="N",PPDM_GUID="recorded-competency-document",EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(2)};
        if(mode!="unlinked")f.Rows.Add(employee);if(mode!="missing-license")f.Rows.Add(license);f.Rows.Add(status);f.Rows.Add(condition);if(mode=="duplicate")f.Rows.Add(CompanyInventorySourceReview.Copy(condition));if(mode=="foreign")condition.LICENSEE_BA_ID="other";
        var metadata=new Mock<IPPDMMetadataRepository>();var creator=new ClassCreator(f.Editor.Object);
        foreach(var (table,type,keys) in new[]{("BA_EMPLOYEE",typeof(BA_EMPLOYEE),new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"}),("BA_LICENSE",typeof(BA_LICENSE),new[]{"LICENSEE_BA_ID","LICENSE_ID"}),("BA_LICENSE_STATUS",typeof(BA_LICENSE_STATUS),new[]{"LICENSEE_BA_ID","LICENSE_ID","STATUS_ID"}),("BA_LICENSE_COND",typeof(BA_LICENSE_COND),new[]{"LICENSEE_BA_ID","LICENSE_ID","CONDITION_ID"})})
        {
            metadata.Setup(m=>m.GetTableMetadataAsync(table)).ReturnsAsync(new PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=string.Join(',',keys)});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync(keys);f.Source.Setup(source=>source.CheckEntityExist(table)).Returns(true);
            f.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(value=>value.EntityName==table),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(type);foreach(var field in shape.Fields){field.IsKey=keys.Contains(field.FieldName);if(field.IsKey)field.AllowDBNull=false;}if(mode=="schema" && table=="BA_LICENSE_STATUS")shape.Fields.First(field=>field.IsKey).AllowDBNull=true;return shape;});
        }
        var conditionReads=0;f.Source.Setup(source=>source.GetEntityAsync("BA_LICENSE_COND",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(()=>
        {
            conditionReads++;if(conditionReads==2){if(mode=="condition-drift")condition.CONDITION_VALUE=9m;if(mode=="membership-drift")employee.EMPLOYER_BA_ID="other";}
            if(mode=="tracked-status")status.LICENSE_STATUS="COMMON_AFTER_PROVIDER_READ";
            if(mode=="transient-licence")license.LICENSE_TYPE=conditionReads==1?"Changed between credential observations":"Recorded licence";
            return f.Rows.OfType<BA_LICENSE_COND>().Select(CompanyInventorySourceReview.Copy).ToArray();
        });
        if(mode=="tracked-status")
        {
            var statusReads=0;f.Source.Setup(source=>source.GetEntityAsync("BA_LICENSE_STATUS",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(()=>
            {status.LICENSE_STATUS=++statusReads==1?"VALID":"SUSPENDED";return new[]{status};});
        }
        if(mode=="transient-licence")f.Source.Setup(source=>source.GetEntityAsync("BA_LICENSE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(()=>new[]{license});
        if(mode=="borrowed")f.Borrowed=true;var owner=new ContractorManagementService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((context.Company,context.ActorUserId,context.Database)),NullLogger<ContractorManagementService>.Instance);
        Task Guard()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;
        var error=await Record.ExceptionAsync(async()=>
        {
            var result=await owner.ReadPersonnelCredentialsAsync(context,query,Guard);result.Require();
            if(mode is "unlinked" or "missing-license"){Assert.Empty(result.Statuses);Assert.Empty(result.Conditions);Assert.Equal(0,conditionReads);f.Source.Verify(source=>source.GetEntityAsync("BA_LICENSE_STATUS",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()),Times.Never);}
            else{Assert.Single(result.Statuses);var value=Assert.Single(result.Conditions);Assert.Equal(mode=="zero"?null:8m,value.Value);}
        });
        if(mode is "current" or "unlinked" or "missing-license" or "zero")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(0,f.Writes);Assert.Empty(f.Transactions);
        if(mode is "current" or "zero")
        {
            f.Source.Verify(source=>source.GetEntityAsync("BA_EMPLOYEE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()),Times.Exactly(3));
            f.Source.Verify(source=>source.GetEntityAsync("BA_LICENSE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()),Times.Exactly(3));
            Assert.Equal(2,conditionReads);
        }
    }
}
#endif
