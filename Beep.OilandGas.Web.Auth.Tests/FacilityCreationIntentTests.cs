using Beep.OilandGas.PPDM39.Models;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class FacilityCreationIntentTests
{
    internal static DevelopmentEquipmentInstallation InstallationOriginal()
    {
        var context=new FieldSelectionReview(new("tenant","company"),new("core","fingerprint"),"actor","field-review","field");
        var proposal=new DevelopmentEquipmentInstallationProposal(context,"facility","PROCESSING","pump","Engineer","Install reviewed pump");
        var review=new DevelopmentEquipmentInstallationReview(proposal,new("company",1,0,new string('A',64)),new string('B',64),DateTimeOffset.UtcNow);
        return new(review,Guid.NewGuid(),Guid.NewGuid(),DateTime.UtcNow.AddMinutes(-1),true);
    }
    [Fact]
    public void EquipmentInstallationOriginalAndExactResultRoundTrip()
    {
        var command=InstallationOriginal();var expected=command.Expected(8,DateTime.UtcNow);var entry=FACILITY_EQUIPMENT_INSTALL_INTENT.Create(command,expected);var restored=entry.Restore(command.Review.Proposal.Context.Company,"actor");
        Assert.Equal(command.Revision(),restored.Command.Revision());Assert.Equal(8,restored.Installation.INSTALL_OBS_NO);Assert.Equal(command.InstalledAtUtc,restored.Installation.INSTALL_DATE);Assert.Equal("pump",restored.Installation.EQUIPMENT_ID);
        var shape=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(FACILITY_EQUIPMENT_INSTALL_INTENT));Assert.Contains(shape.Fields,value=>value.FieldName=="FACILITY_EQUIPMENT_INSTALL_INTENT_ID");Assert.Contains(shape.Fields,value=>value.FieldName=="INSTALLATION_JSON");
    }
    [Theory][InlineData("result-key")][InlineData("result-extra")][InlineData("result-date")][InlineData("company")][InlineData("actor")][InlineData("digest")][InlineData("json")][InlineData("id")][InlineData("confirmation")][InlineData("local")][InlineData("future")]
    public void EquipmentInstallationOriginalAndRecoveryRejectTampering(string mode)
    {
        var command=InstallationOriginal();if(mode=="id")command=command with{InstallationGuid=command.IntentId};if(mode=="confirmation")command=command with{Confirmed=false};if(mode=="local")command=command with{InstalledAtUtc=DateTime.Now};if(mode=="future")command=command with{InstalledAtUtc=DateTime.UtcNow.AddDays(1)};
        if(mode is "id" or "confirmation" or "local" or "future"){Assert.Throws<InvalidOperationException>(()=>command.Require());return;}
        var row=command.Expected(8,DateTime.UtcNow);if(mode=="result-key")row.FACILITY_TYPE="other";if(mode=="result-extra")row.REMARK="unapproved";if(mode=="result-date")row.INSTALL_DATE=DateTime.UtcNow;
        if(mode.StartsWith("result",StringComparison.Ordinal)){Assert.Throws<InvalidOperationException>(()=>FACILITY_EQUIPMENT_INSTALL_INTENT.Create(command,row));return;}
        var entry=FACILITY_EQUIPMENT_INSTALL_INTENT.Create(command,row);if(mode=="digest")entry.RECORDED_REVISION=new string('C',64);if(mode=="json")entry.INSTALLATION_JSON="{}";
        Assert.Throws<InvalidOperationException>(()=>entry.Restore(mode=="company"?new("tenant","other"):command.Review.Proposal.Context.Company,mode=="actor"?"other":"actor"));
    }

    internal static DevelopmentFacilityCreation Original()
    {
        var context=new FieldSelectionReview(new("tenant","company"),new("core","fingerprint"),"actor","field-review","field");
        var draft=new DevelopmentFacilityCreationDraft(context,"Engineer","facility","PROCESSING","explicit-operator","New plant",null,"Expansion");
        var review=new DevelopmentFacilityCreationReview(draft,new R_FACILITY_TYPE{FACILITY_TYPE="PROCESSING",ACTIVE_IND="Y"},new string('A',64),new string('B',64),"",DateTimeOffset.UtcNow);review=review with{Revision=review.ContentRevision()};
        return new(review,Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),true);
    }
    [Fact]
    public void ExactOriginalFacilityAndFieldLinkRoundTripWithExplicitOperator()
    {
        var command=Original();var expected=command.Expected(DateTime.UtcNow);var entry=FACILITY_CREATE_INTENT.Create(command,expected.Facility,expected.Link);
        var restored=entry.Restore(command.Review.Draft.Context.Company,"actor");Assert.Equal(command.Revision(),restored.Command.Revision());Assert.Equal("explicit-operator",restored.Facility.CURRENT_OPERATOR);Assert.Equal("field",restored.Link.FIELD_ID);Assert.Null(restored.Facility.DESCRIPTION);
    }
    [Theory][InlineData("facility")][InlineData("link")][InlineData("operator")][InlineData("actor")][InlineData("audit")][InlineData("extra")][InlineData("local")]
    public void AlteredResultsAndNonUtcAuditAreRejected(string fault)
    {
        var command=Original();var rows=command.Expected(DateTime.UtcNow);if(fault=="facility")rows.Facility.FACILITY_ID="other";if(fault=="link")rows.Link.FIELD_ID="other";if(fault=="operator")rows.Facility.CURRENT_OPERATOR="other";if(fault=="actor")rows.Link.ROW_CREATED_BY="other";if(fault=="audit")rows.Link.ROW_CHANGED_DATE=rows.Link.ROW_CHANGED_DATE!.Value.AddSeconds(1);if(fault=="extra")rows.Facility.REMARK="unapproved";if(fault=="local")rows.Facility.ROW_CREATED_DATE=DateTime.Now;
        Assert.Throws<InvalidOperationException>(()=>FACILITY_CREATE_INTENT.Create(command,rows.Facility,rows.Link));
    }
    [Theory][InlineData("unconfirmed")][InlineData("empty-id")][InlineData("duplicate-id")][InlineData("operator")][InlineData("type")]
    public void OriginalRequiresExplicitConfirmationAndUntamperedReview(string fault)
    {
        var command=Original();command=fault switch{"unconfirmed"=>command with{Confirmed=false},"empty-id"=>command with{IntentId=Guid.Empty},"duplicate-id"=>command with{FieldLinkGuid=command.FacilityGuid},"operator"=>command with{Review=command.Review with{Draft=command.Review.Draft with{OperatorBusinessAssociateId=""}}},_=>command with{Review=command.Review with{TypeSource=new R_FACILITY_TYPE{FACILITY_TYPE="STORAGE",ACTIVE_IND="Y"}}}};
        Assert.Throws<InvalidOperationException>(()=>command.Require());
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("json")][InlineData("digest")]
    public void RestoreRejectsForeignOrAlteredOriginal(string fault)
    {
        var command=Original();var rows=command.Expected(DateTime.UtcNow);var entry=FACILITY_CREATE_INTENT.Create(command,rows.Facility,rows.Link);if(fault=="json")entry.FIELD_LINK_JSON="{}";if(fault=="digest")entry.RECORDED_REVISION=new string('B',64);
        Assert.Throws<InvalidOperationException>(()=>entry.Restore(fault=="company"?new("tenant","other"):command.Review.Draft.Context.Company,fault=="actor"?"other":"actor"));
    }
    [Fact]
    public void BeepClassCreatorDeclaresTheIntentKeyAndBodyFields()
    {
        var shape=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(FACILITY_CREATE_INTENT));Assert.NotNull(shape);Assert.Contains(shape.Fields,value=>value.FieldName=="FACILITY_CREATE_INTENT_ID");Assert.Contains(shape.Fields,value=>value.FieldName=="FIELD_LINK_JSON");
    }
}
