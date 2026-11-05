using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DevelopmentFacilityEditContractTests
{
    internal static DevelopmentFacilityEdit Original()
    {
        var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"editor","field-revision","field");
        var facility=new FACILITY{FACILITY_ID="shared-id",FACILITY_TYPE="PROCESSING",PRIMARY_FIELD_ID="field",FACILITY_LONG_NAME="Original",DESCRIPTION="Original description",REMARK="Operational note"};
        var review=new DevelopmentFacilityEditReview(context,facility,"ConfiguredEditor",DevelopmentFacilityEditReview.SourceDigest(facility),new string('A',64),"",DateTimeOffset.UtcNow);
        review=review with{Revision=review.ContentRevision()};return new(review,Guid.NewGuid(),"Revised name","Revised description","Correct recorded asset description",true);
    }
    [Fact]
    public void RevisionBindsTheExactCompanyActorFieldCompositeIdentitySourceAndProposal()
    {
        var value=Original();value.Require();var original=value.Revision();
        Assert.NotEqual(original,(value with{LongName="Another name"}).Revision());
        Assert.NotEqual(original,(value with{IntentId=Guid.NewGuid()}).Revision());
        var changed=value.Review.Original;changed.REMARK="changed after review";Assert.Throws<InvalidOperationException>(()=>value.Require());
    }
    [Theory][InlineData("field")][InlineData("facility")][InlineData("type")][InlineData("role")][InlineData("authority")][InlineData("source")][InlineData("review")][InlineData("time")][InlineData("future")]
    public void IncompleteOrChangedReviewedSourceIsRefused(string fault)
    {
        var value=Original();var review=value.Review;
        switch(fault){case "field":review.Original.PRIMARY_FIELD_ID="another";break;case "facility":review.Original.FACILITY_ID="";break;case "type":review.Original.FACILITY_TYPE="";break;
            case "role":review=review with{EditorRole=""};break;case "authority":review=review with{AuthorityRevision="invalid"};break;
            case "source":review=review with{SourceRevision=new string('B',64)};break;case "review":review=review with{Revision=new string('C',64)};break;
            case "time":review=review with{CheckedAtUtc=DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3))};break;case "future":review=review with{CheckedAtUtc=DateTimeOffset.UtcNow.AddDays(1)};break;}
        Assert.Throws<InvalidOperationException>(()=>(value with{Review=review}).Require());
    }
    [Theory][InlineData("intent")][InlineData("confirmation")][InlineData("name")][InlineData("reason")][InlineData("control")][InlineData("noop")][InlineData("length")]
    public void ProposedEditRequiresExplicitBoundedChangeAndConfirmation(string fault)
    {
        var value=Original();value=fault switch{
            "intent"=>value with{IntentId=Guid.Empty},"confirmation"=>value with{Confirmed=false},"name"=>value with{LongName=" "},
            "reason"=>value with{Reason=""},"control"=>value with{Reason="bad\u0001reason"},
            "noop"=>value with{LongName=value.Review.Original.FACILITY_LONG_NAME,Description=value.Review.Original.DESCRIPTION},
            _=>value with{LongName=new string('x',256)}};
        Assert.Throws<InvalidOperationException>(value.Require);
    }
}
