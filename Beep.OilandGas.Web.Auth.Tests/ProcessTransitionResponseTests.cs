using System.Text.Json;
using System.Text.Json.Nodes;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ProcessTransitionResponseTests
{
    private static readonly JsonSerializerOptions Options=new(){PropertyNamingPolicy=JsonNamingPolicy.CamelCase,PropertyNameCaseInsensitive=true};
    private static CompanyScope Company()=>new("tenant","company");
    private static ProcessTransitionReview Review()=>new(Company(),"actor","field","process/a?b","review","step",new string('A',64),[new("action","review","accepted","submit-review",true)],WorkOrder:new("order/a?b","field","WO-42","WO-CORRECTIVE",null,null,"Recorded instructions",[new("order/a?b","component","field","well","equipment")],[],DateTimeOffset.UtcNow));
    private static ProcessTransitionReview Decode(JsonNode json)=>JsonSerializer.Deserialize<ProcessTransitionReview>(json.ToJsonString(),Options)!;
    [Fact] public void CurrentMaintenanceResponseRoundTripsOriginalSourceFacts()
    {
        var review=Decode(JsonSerializer.SerializeToNode(Review(),Options)!);review.Require(Company(),"actor","process/a?b");
        Assert.Equal("WO-42",review.WorkOrder!.RecordedNumber);Assert.Null(review.WorkOrder.DueDate);Assert.Empty(review.WorkOrder.Inspections);
        Assert.Equal("well",Assert.Single(review.WorkOrder.Components).WellId);Assert.Equal("/ppdm39/workorder/order%2Fa%3Fb",review.WorkOrder.Route);
        Assert.Equal("submit-review",Assert.Single(review.Choices).Trigger);
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("instance")][InlineData("field")][InlineData("revision")][InlineData("choices")][InlineData("null-choice")][InlineData("duplicate-choice")][InlineData("ambiguous-choice")][InlineData("choice-state")][InlineData("choice-id")][InlineData("choice-target")][InlineData("choice-trigger")]
    public void MalformedOrForeignJsonCannotExposeReviewedChoices(string fault)
    {
        var json=JsonSerializer.SerializeToNode(Review(),Options)!;
        switch(fault)
        {
            case "company":json["company"]!["businessAssociateId"]="foreign";break;
            case "actor":json["actorUserId"]="foreign";break;
            case "instance":json["instanceId"]="foreign";break;
            case "field":json["fieldId"]=null;break;
            case "revision":json["revision"]=new string('Z',64);break;
            case "choices":json["choices"]=null;break;
            case "null-choice":json["choices"]![0]=null;break;
            case "duplicate-choice":json["choices"]!.AsArray().Add(json["choices"]![0]!.DeepClone());break;
            case "ambiguous-choice":var alias=json["choices"]![0]!.DeepClone();alias["transitionId"]="alias";json["choices"]!.AsArray().Add(alias);break;
            case "choice-state":json["choices"]![0]!["fromStateId"]="changed";break;
            case "choice-id":json["choices"]![0]!["transitionId"]="";break;
            case "choice-target":json["choices"]![0]!["toStateId"]="";break;
            default:json["choices"]![0]!["trigger"]="";break;
        }
        var response=Decode(json);Assert.Throws<InvalidOperationException>(()=>response.Require(Company(),"actor","process/a?b"));
    }
    [Theory][InlineData("order")][InlineData("field")][InlineData("time")][InlineData("components")][InlineData("null-component")][InlineData("foreign-component")][InlineData("duplicate-component")][InlineData("inspections")]
    public void MalformedMaintenanceSourceJsonCannotPassContractReview(string fault)
    {
        var json=JsonSerializer.SerializeToNode(Review(),Options)!;var source=json["workOrder"]!;
        switch(fault)
        {
            case "order":source["workOrderId"]="";break;
            case "field":source["fieldId"]="foreign";break;
            case "time":source["retrievedAtUtc"]=JsonSerializer.SerializeToNode(default(DateTimeOffset),Options);break;
            case "components":source["components"]=null;break;
            case "null-component":source["components"]![0]=null;break;
            case "foreign-component":source["components"]![0]!["workOrderId"]="foreign";break;
            case "duplicate-component":source["components"]!.AsArray().Add(source["components"]![0]!.DeepClone());break;
            default:source["inspections"]=null;break;
        }
        var response=Decode(json);Assert.Throws<InvalidOperationException>(()=>response.Require(Company(),"actor","process/a?b"));
    }
    [Fact] public void TwoDifferentSourceFamiliesCannotEnterOneActionReview()
    {
        var response=Review() with{Journal=new("journal",null,null,null,null,null,null,null,null,null,null,null,DateTimeOffset.UtcNow,[])};
        Assert.Throws<InvalidOperationException>(()=>response.Require(Company(),"actor","process/a?b"));
    }
    [Fact] public void CompletedProcessWithoutActiveStepCanStillShowReadOnlySourceReview()
    {
        var response=Review() with{CurrentStepId="",Choices=[]};response.Require(Company(),"actor","process/a?b");Assert.Empty(response.Choices);
    }
    [Fact] public void ActionsWithoutConfirmedActiveStepRefuse()
    {
        var response=Review() with{CurrentStepId=""};Assert.Throws<InvalidOperationException>(()=>response.Require(Company(),"actor","process/a?b"));
    }
}
