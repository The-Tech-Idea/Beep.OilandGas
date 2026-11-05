using System.Text.Json;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Moq;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class FacilityEditIntentTests
{
    private static (DevelopmentFacilityEdit Command,FACILITY Saved) Original()
    {
        var command=DevelopmentFacilityEditContractTests.Original();var row=CompanyInventorySourceReview.Copy(command.Review.Original);
        row.FACILITY_LONG_NAME=command.LongName;row.DESCRIPTION=command.Description;row.ROW_CHANGED_BY=command.Review.Context.ActorUserId;row.ROW_CHANGED_DATE=DateTime.UtcNow;
        return(command,row);
    }
    [Fact] public void OriginalAndExactResultRoundTripWithActorAndCompany()
    {
        var (command,row)=Original();var intent=FACILITY_EDIT_INTENT.Create(command,row);var restored=intent.Restore(command.Review.Context.Company,command.Review.Context.ActorUserId);
        Assert.Equal(command.Revision(),restored.Command.Revision());Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(restored.Result));
        Assert.Equal(command.IntentId.ToString("N"),intent.FACILITY_EDIT_INTENT_ID);Assert.Equal(row.ROW_CHANGED_DATE,intent.ROW_CREATED_DATE);
        Assert.Throws<InvalidOperationException>(()=>intent.Restore(new("tenant","other"),command.Review.Context.ActorUserId));
        Assert.Throws<InvalidOperationException>(()=>intent.Restore(command.Review.Context.Company,"other"));
    }
    [Fact] public void ExplicitlyClearingDescriptionRetainsTheExactNullValue()
    {
        var (command,row)=Original();command=command with{Description=null};row.DESCRIPTION=null!;
        var restored=FACILITY_EDIT_INTENT.Create(command,row).Restore(command.Review.Context.Company,command.Review.Context.ActorUserId);
        Assert.Null(restored.Command.Description);Assert.Null(restored.Result.DESCRIPTION);
    }
    [Theory][InlineData("name")][InlineData("type")][InlineData("field")][InlineData("remark")][InlineData("actor")][InlineData("time")]
    public void ResultMustMatchExactlyTheReviewedEditAndOriginalSource(string fault)
    {
        var (command,row)=Original();switch(fault){case "name":row.FACILITY_LONG_NAME="different";break;case "type":row.FACILITY_TYPE="STORAGE";break;
            case "field":row.PRIMARY_FIELD_ID="other";break;case "remark":row.REMARK="changed";break;case "actor":row.ROW_CHANGED_BY="other";break;case "time":row.ROW_CHANGED_DATE=DateTime.SpecifyKind(DateTime.Now,DateTimeKind.Local);break;}
        Assert.Throws<InvalidOperationException>(()=>FACILITY_EDIT_INTENT.Create(command,row));
    }
    [Theory][InlineData("digest")][InlineData("id")][InlineData("field")][InlineData("result")][InlineData("command")][InlineData("audit")]
    public void RetainedEvidenceCannotBeSubstitutedEvenAfterItsOuterDigestIsRecomputed(string fault)
    {
        var (command,row)=Original();var entry=FACILITY_EDIT_INTENT.Create(command,row);
        switch(fault){case "digest":entry.RESULT_REVISION=new string('B',64);break;case "id":entry.FACILITY_EDIT_INTENT_ID=Guid.NewGuid().ToString("N");break;
            case "field":entry.FIELD_ID="other";break;case "result":row.REMARK="forged";entry.RESULT_JSON=JsonSerializer.Serialize(row);entry.RESULT_REVISION=DevelopmentFacilityEditReview.SourceDigest(row);break;
            case "command":entry.COMMAND_JSON=JsonSerializer.Serialize(command with{IntentId=Guid.NewGuid()});break;case "audit":entry.ROW_CHANGED_BY="other";break;}
        entry.RECORDED_REVISION=FACILITY_EDIT_INTENT.EntryRevision(entry);
        Assert.ThrowsAny<Exception>(()=>entry.Restore(command.Review.Context.Company,command.Review.Context.ActorUserId));
    }
    [Fact] public void BeepEntityMetadataContainsTheIntentKeyAndRequiredProofColumns()
    {
        var shape=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(FACILITY_EDIT_INTENT));
        Assert.Single(shape.Fields.Where(row=>row.IsKey));Assert.True(shape.Fields.Single(row=>row.FieldName=="FACILITY_EDIT_INTENT_ID").IsKey);
        foreach(var name in new[]{"OWNER_TENANT_ID","OWNER_COMPANY_BA_ID","FIELD_ID","FACILITY_ID","FACILITY_TYPE","COMMAND_JSON","RESULT_JSON","RESULT_REVISION","RECORDED_REVISION"})Assert.Contains(shape.Fields,row=>row.FieldName==name);
    }
}
