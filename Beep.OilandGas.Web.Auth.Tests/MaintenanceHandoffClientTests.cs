#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class MaintenanceHandoffClientTests
{
    private sealed class Transport:HttpMessageHandler
    {
        internal CompanyMaintenanceProcessStartReview Review=new(new(new("tenant","company"),new("lifecycle","fingerprint"),"actor","revision","field"),"workflow",new string('A',64),new(new("tenant","company"),"actor","field","order/العالم%2F","well/الكويت%2F",Guid.NewGuid(),"original-reviewer","Recorded reason",DateTime.UtcNow,new string('B',64),new string('C',64),DateTimeOffset.UtcNow),"Receiving workflow","MaintenanceRole");
        internal TaskCompletionSource<bool>? Held;internal TaskCompletionSource<bool> Started=new(TaskCreationOptions.RunContinuationsAsynchronously);internal CompanyMaintenanceProcessStart? Body;internal int Writes;internal string? LastPath;internal bool EmptyChoices;internal CompanyMaintenanceAcceptanceCommand? AcceptanceBody;internal int AcceptanceWrites;internal Guid AssignmentId=Guid.NewGuid();
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            LastPath=request.RequestUri!.AbsolutePath;Started.TrySetResult(true);if(Held is {} held)await held.Task.WaitAsync(token);
            CompanyMaintenanceAcceptanceReview Receiving()=>new(Review.Context,new(AssignmentId,Review.Source.DecisionId,Review.Source.FieldId,Review.Source.WellId,Review.Source.WorkOrderId),CompanyMaintenanceProcessStart.InstanceIdFor(Review.Context.Company,AssignmentId),Review.ProcessId,Review.DefinitionRevision,new string('E',64),"receiving-step",Review.ReceivingRole,"original-starter",Review.Source);
            if(LastPath.EndsWith("/acceptance/task-review",StringComparison.Ordinal))return new(HttpStatusCode.OK){Content=JsonContent.Create(Receiving())};
            if(LastPath.EndsWith("/acceptance/recovery/reference",StringComparison.Ordinal))
            {
                var query=(await request.Content!.ReadFromJsonAsync<CompanyMaintenanceAcceptanceReferenceQuery>(token))!;var command=new CompanyMaintenanceAcceptanceCommand(Receiving(),query.IntentId,"Original receiving rationale");AcceptanceBody=command;
                return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyMaintenanceAcceptanceRecovery(command,new(command.Review.Context.Company,command.Review.Context.ActorUserId,command.IntentId,command.Review.InstanceId,command.Review.StepInstanceId,"history","approval",null,["receiving-task"],[],new string('D',64))))};
            }
            if(LastPath.EndsWith("/acceptance/review",StringComparison.Ordinal))
            {
                var query=(await request.Content!.ReadFromJsonAsync<CompanyMaintenanceRecoveryQuery>(token))!;
                return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyMaintenanceAcceptanceReview(Review.Context,query,CompanyMaintenanceProcessStart.InstanceIdFor(Review.Context.Company,query.IntentId),Review.ProcessId,Review.DefinitionRevision,new string('E',64),"receiving-step",Review.ReceivingRole,"original-starter",Review.Source))};
            }
            if(LastPath.EndsWith("/acceptance",StringComparison.Ordinal) || LastPath.EndsWith("/acceptance/recovery",StringComparison.Ordinal))
            {
                AcceptanceBody=(await request.Content!.ReadFromJsonAsync<CompanyMaintenanceAcceptanceCommand>(token))!;
                if(LastPath.EndsWith("/acceptance",StringComparison.Ordinal))AcceptanceWrites++;
                var command=AcceptanceBody;var review=command.Review;
                return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyMaintenanceAcceptanceReceipt(review.Context.Company,review.Context.ActorUserId,command.IntentId,review.InstanceId,review.StepInstanceId,"history","approval",null,["receiving-task"],[],new string('D',64)))};
            }
            if(LastPath.EndsWith("/choices",StringComparison.Ordinal))return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyMaintenanceStartCatalog(Review.Context,Review.Source,EmptyChoices?[]:[Review]))};
            if(LastPath.EndsWith("/recovery/reference",StringComparison.Ordinal))
            {
                var query=(await request.Content!.ReadFromJsonAsync<CompanyMaintenanceRecoveryQuery>(token))!;Body=Review.CreateIntent(query.IntentId);
                return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyMaintenanceRecovery(Body,new(Body.Context.Company,Body.Context.ActorUserId,Body.Context.FieldId!,Body.IntentId,Body.InstanceId(),Body.Source.WorkOrderId,Body.Source.DecisionId,"receiving-step",["receiving-task"],new string('D',64))))};
            }
            if(request.RequestUri!.AbsolutePath.EndsWith("/review",StringComparison.Ordinal))return new(HttpStatusCode.OK){Content=JsonContent.Create(Review)};
            Body=(await request.Content!.ReadFromJsonAsync<CompanyMaintenanceProcessStart>(token))!;Writes++;
            return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyMaintenanceProcessStartReceipt(Body.Context.Company,Body.Context.ActorUserId,Body.Context.FieldId!,Body.IntentId,Body.InstanceId(),Body.Source.WorkOrderId,Body.Source.DecisionId,"receiving-step",["receiving-task"],new string('D',64)))};
        }
    }
    [Theory][InlineData("task")][InlineData("reference")][InlineData("foreign-task")][InlineData("late-reference")]
    public async Task ActualIncomingTaskAndAcceptanceReferenceClientRefusesForeignOrLateEvidence(string fault)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["MaintenanceRole"],["Workflow.Approve"])));
        var transport=new Transport();var source=transport.Review.Source;var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(source.Company,transport.AssignmentId),source.FieldId,source.WorkOrderId);
        using var http=new HttpClient(transport){BaseAddress=new("http://fixture")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        if(fault=="foreign-task")transport.Review=transport.Review with{Context=transport.Review.Context with{Company=new("tenant","foreign")},Source=source with{Company=new("tenant","foreign")}};
        if(fault.EndsWith("task",StringComparison.Ordinal))
        {
            if(fault=="foreign-task")await Assert.ThrowsAnyAsync<Exception>(()=>client.ReviewMaintenanceReceivingTaskAsync(task));else {var review=await client.ReviewMaintenanceReceivingTaskAsync(task);review.Require();Assert.Equal(task.InstanceId,review.InstanceId);}
        }
        else
        {
            if(fault=="late-reference")transport.Held=new(TaskCreationOptions.RunContinuationsAsynchronously);
            var query=new CompanyMaintenanceAcceptanceReferenceQuery(task,Guid.NewGuid());var pending=client.RecoverMaintenanceAcceptanceReferenceAsync(query);
            if(fault=="late-reference"){await transport.Started.Task;company.Clear();transport.Held!.SetResult(true);await Assert.ThrowsAnyAsync<Exception>(()=>pending);}
            else {var result=await pending;result.Require(source.Company,source.Actor,query);Assert.Equal(query.IntentId,result.Command.IntentId);}
        }
        Assert.Equal(0,transport.AcceptanceWrites);
    }
    [Theory][InlineData("unknown")][InlineData("fresh")][InlineData("late-company")]
    public async Task ReceivingStateRetainsOriginalAcceptanceAndRestoresOnlyReadOnlyEvidence(string fault)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["MaintenanceRole"],["Workflow.Approve"])));
        var transport=new Transport();var source=transport.Review.Source;var assignment=new CompanyMaintenanceRecoveryQuery(transport.AssignmentId,source.DecisionId,source.FieldId,source.WellId,source.WorkOrderId);
        var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(source.Company,assignment.IntentId),source.FieldId,source.WorkOrderId);
        var review=new CompanyMaintenanceAcceptanceReview(transport.Review.Context,assignment,task.InstanceId,"workflow",new string('A',64),new string('B',64),"receiving-step","MaintenanceRole","original-starter",source);
        var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewMaintenanceReceivingTaskAsync(task,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        CompanyMaintenanceAcceptanceCommand? original=null;
        client.Setup(value=>value.AcceptMaintenanceAsync(It.IsAny<CompanyMaintenanceAcceptanceCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceAcceptanceCommand command,CancellationToken _) =>{original=command;return Task.FromException<CompanyMaintenanceAcceptanceReceipt>(new HttpRequestException("Unknown commit"));});
        using var state=new CompanyMaintenanceReceivingState(company);state.Attach(task,"source-revision");await state.ReviewAsync(client.Object);
        await Assert.ThrowsAsync<HttpRequestException>(()=>state.AcceptAsync(client.Object,"Original receiving rationale",id=>{Assert.Equal(id,state.Reference);Assert.NotNull(state.Intent);return Task.CompletedTask;}));
        Assert.NotNull(original);Assert.Null(state.Receipt);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.AcceptAsync(client.Object,"Replacement",_=>Task.CompletedTask));
        var target=fault=="fresh"?new CompanyMaintenanceReceivingState(company):state;target.Attach(task,"source-revision");
        var held=new TaskCompletionSource<CompanyMaintenanceAcceptanceRecovery>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.Setup(value=>value.RecoverMaintenanceAcceptanceReferenceAsync(It.IsAny<CompanyMaintenanceAcceptanceReferenceQuery>(),It.IsAny<CancellationToken>())).Returns(held.Task);
        var pending=target.RecoverAsync(client.Object,original.IntentId);if(fault=="late-company")company.Clear();
        held.SetResult(new(original,new(original.Review.Context.Company,original.Review.Context.ActorUserId,original.IntentId,original.Review.InstanceId,original.Review.StepInstanceId,"history","approval",null,["receiving-task"],[],new string('D',64))));await pending;
        if(fault=="late-company"){Assert.Null(target.Receipt);Assert.Null(target.VisibleReference);}else {Assert.Equal(original,target.Intent);Assert.NotNull(target.Receipt);}
        client.Verify(value=>value.AcceptMaintenanceAsync(It.IsAny<CompanyMaintenanceAcceptanceCommand>(),It.IsAny<CancellationToken>()),Times.Once);if(fault=="fresh")target.Dispose();
    }
    [Theory][InlineData("allowed")][InlineData("recovery")][InlineData("foreign-review")][InlineData("missing-permission")][InlineData("late-company")][InlineData("foreign-command")]
    public async Task ActualAcceptanceClientRequiresOriginalConfiguredReceivingRolePermissionAndCapturedCompany(string fault)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["MaintenanceRole"],fault=="missing-permission"?[]:["Workflow.Approve"])));
        var transport=new Transport();using var http=new HttpClient(transport){BaseAddress=new("http://fixture")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var source=transport.Review.Source;var query=new CompanyMaintenanceRecoveryQuery(Guid.NewGuid(),source.DecisionId,source.FieldId,source.WellId,source.WorkOrderId);
        if(fault=="foreign-review")transport.Review=transport.Review with{Context=transport.Review.Context with{Company=new("tenant","foreign")},Source=source with{Company=new("tenant","foreign")}};
        if(fault=="late-company")transport.Held=new(TaskCreationOptions.RunContinuationsAsynchronously);
        var pending=client.ReviewMaintenanceAcceptanceAsync(query);
        if(fault=="late-company"){await transport.Started.Task;company.Clear();transport.Held!.SetResult(true);}
        if(fault is "foreign-review" or "missing-permission" or "late-company"){await Assert.ThrowsAnyAsync<Exception>(()=>pending);Assert.Equal(0,transport.AcceptanceWrites);return;}
        var review=await pending;var command=new CompanyMaintenanceAcceptanceCommand(review,Guid.NewGuid(),"Original independent receiving rationale");
        if(fault=="foreign-command")
        {
            command=command with{Review=review with{Context=review.Context with{Company=new("tenant","other")},InstanceId=CompanyMaintenanceProcessStart.InstanceIdFor(new("tenant","other"),query.IntentId),Source=source with{Company=new("tenant","other")}}};
            await Assert.ThrowsAnyAsync<Exception>(()=>client.AcceptMaintenanceAsync(command));Assert.Equal(0,transport.AcceptanceWrites);return;
        }
        var receipt=fault=="recovery"?await client.RecoverMaintenanceAcceptanceAsync(command):await client.AcceptMaintenanceAsync(command);receipt.Require(command);Assert.Equal(command,transport.AcceptanceBody);
        Assert.Equal(fault=="recovery"?0:1,transport.AcceptanceWrites);Assert.Equal(fault=="recovery"?"/api/field/current/maintenance-handoffs/acceptance/recovery":"/api/field/current/maintenance-handoffs/acceptance",transport.LastPath);
    }
    [Theory][InlineData("choices")][InlineData("empty-choices")][InlineData("foreign-choices")][InlineData("reference")][InlineData("foreign-reference")][InlineData("late-reference")]
    public async Task ActualWorkflowDiscoveryAndReferenceRecoveryKeepOriginalCompanyAccountAndSource(string fault)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["MaintenanceRole"],[])));
        var transport=new Transport();using var http=new HttpClient(transport){BaseAddress=new("http://fixture")};
        var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var source=transport.Review.Source;var query=new CompanyMaintenanceSourceQuery(source.DecisionId,source.WellId,source.WorkOrderId);
        if(fault.StartsWith("foreign-",StringComparison.Ordinal))transport.Review=transport.Review with{Context=transport.Review.Context with{Company=new("tenant","foreign")},Source=source with{Company=new("tenant","foreign")}};
        if(fault=="empty-choices")transport.EmptyChoices=true;
        if(fault.EndsWith("choices",StringComparison.Ordinal))
        {
            if(fault=="foreign-choices")await Assert.ThrowsAnyAsync<Exception>(()=>client.GetMaintenanceHandoffChoicesAsync(query));
            else {var choices=await client.GetMaintenanceHandoffChoicesAsync(query);choices.Require(source.Company,source.Actor,query);Assert.Equal(fault=="empty-choices"?0:1,choices.Choices.Count);}
            Assert.Equal("/api/field/current/maintenance-handoffs/choices",transport.LastPath);
        }
        else
        {
            var recovery=new CompanyMaintenanceRecoveryQuery(Guid.NewGuid(),query.DecisionId,source.FieldId,query.WellId,query.WorkOrderId);
            if(fault=="late-reference")transport.Held=new(TaskCreationOptions.RunContinuationsAsynchronously);
            var pending=client.RecoverMaintenanceHandoffReferenceAsync(recovery);
            if(fault=="late-reference"){await transport.Started.Task;company.Clear();transport.Held!.SetResult(true);}
            if(fault is "foreign-reference" or "late-reference")await Assert.ThrowsAnyAsync<Exception>(()=>pending);
            else {var result=await pending;result.Require(source.Company,source.Actor,recovery);Assert.Equal(recovery.IntentId,result.Intent.IntentId);}
            Assert.Equal("/api/field/current/maintenance-handoffs/recovery/reference",transport.LastPath);
        }
        Assert.Equal(0,transport.Writes);
    }
    [Theory][InlineData("lost-response")][InlineData("company-change")][InlineData("source-change")]
    public async Task RetainedScreenIntentSurvivesUncertaintyAndRecoveryNeverSubmitsAgain(string fault)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["MaintenanceRole"],[])));
        using var state=new CompanyMaintenanceHandoffState(company);var review=new Transport().Review;state.AttachSource(review.Source);
        var client=new Mock<IBusinessProcessServiceClient>();
        client.Setup(value=>value.ReviewMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStartQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(value=>value.GetMaintenanceHandoffChoicesAsync(It.IsAny<CompanyMaintenanceSourceQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyMaintenanceStartCatalog(review.Context,review.Source,[review]));
        CompanyMaintenanceProcessStart? retained=null;
        client.Setup(value=>value.StartMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStart>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceProcessStart intent,CancellationToken _) =>
        {
            Assert.NotNull(state.Intent);Assert.Equal(intent,state.Intent);retained=intent;
            if(fault=="company-change")company.Clear();
            if(fault=="source-change")state.AttachSource(null);
            return Task.FromException<CompanyMaintenanceProcessStartReceipt>(new InvalidOperationException("Unconfirmed response"));
        });
        await state.LoadChoicesAsync(client.Object);await state.ReviewAsync(client.Object,"workflow");await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitAsync(client.Object));
        Assert.NotNull(retained);Assert.Equal(retained,state.Intent);Assert.Null(state.Receipt);
        if(fault is "company-change" or "source-change")Assert.Null(state.VisibleReference);else Assert.Equal(retained.IntentId,state.VisibleReference);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitAsync(client.Object));
        if(fault=="company-change")Assert.True(company.TrySelect(company.Snapshot.Version,"actor",new(new("tenant","company"),["MaintenanceRole"],[])));
        state.AttachSource(review.Source);
        Assert.Equal(retained.IntentId,state.VisibleReference);
        client.Setup(value=>value.RecoverMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStart>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceProcessStart intent,CancellationToken _) =>
        {
            Assert.Equal(retained,intent);
            return Task.FromResult(new CompanyMaintenanceProcessStartReceipt(intent.Context.Company,intent.Context.ActorUserId,intent.Context.FieldId!,intent.IntentId,intent.InstanceId(),intent.Source.WorkOrderId,intent.Source.DecisionId,"receiving-step",["receiving-task"],new string('D',64)));
        });
        await state.RecoverAsync(client.Object);Assert.NotNull(state.Receipt);state.Receipt.Require(retained!);
        client.Verify(value=>value.StartMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStart>(),It.IsAny<CancellationToken>()),Times.Once);
        client.Verify(value=>value.RecoverMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStart>(),It.IsAny<CancellationToken>()),Times.Once);
        state.AttachSource(review.Source with{WorkOrderId="another-order",DecisionId=Guid.NewGuid()});
        Assert.Null(state.Intent);Assert.Null(state.Receipt);
    }
    [Theory][InlineData("allowed")][InlineData("recovery")][InlineData("foreign-review")][InlineData("late-company")][InlineData("foreign-intent")]
    public async Task ActualWorkflowClientKeepsOriginalReviewIntentReceiptAndCapturedCompany(string change)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["MaintenanceRole"],[])));
        var transport=new Transport();using var http=new HttpClient(transport){BaseAddress=new("http://fixture")};
        var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var query=new CompanyMaintenanceProcessStartQuery("workflow",transport.Review.Source.DecisionId,transport.Review.Source.WellId,transport.Review.Source.WorkOrderId);
        if(change=="foreign-review")transport.Review=transport.Review with{Context=transport.Review.Context with{Company=new("tenant","foreign")}};
        if(change=="late-company")transport.Held=new(TaskCreationOptions.RunContinuationsAsynchronously);
        var pending=client.ReviewMaintenanceHandoffAsync(query);
        if(change=="late-company"){await transport.Started.Task;company.Clear();transport.Held!.SetResult(true);}
        if(change is "foreign-review" or "late-company"){await Assert.ThrowsAnyAsync<Exception>(()=>pending);Assert.Equal(0,transport.Writes);return;}
        var review=await pending;var intent=review.CreateIntent(Guid.NewGuid());
        if(change=="foreign-intent"){intent=intent with{Context=intent.Context with{Company=new("tenant","other")},Source=intent.Source with{Company=new("tenant","other")}};await Assert.ThrowsAnyAsync<Exception>(()=>client.StartMaintenanceHandoffAsync(intent));Assert.Equal(0,transport.Writes);return;}
        var receipt=change=="recovery" ? await client.RecoverMaintenanceHandoffAsync(intent) : await client.StartMaintenanceHandoffAsync(intent);receipt.Require(intent);Assert.Equal(intent,transport.Body);Assert.Equal(1,transport.Writes);Assert.Equal(query.WorkOrderId,receipt.WorkOrderId);
        Assert.Equal(change=="recovery" ? "/api/field/current/maintenance-handoffs/recovery" : "/api/field/current/maintenance-handoffs",transport.LastPath);
    }
    [Theory][InlineData("company")][InlineData("source")]
    public async Task LateRecoveryCannotPublishConfirmationForChangedScreenContext(string change)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["MaintenanceRole"],[])));
        using var state=new CompanyMaintenanceHandoffState(company);var review=new Transport().Review;state.AttachSource(review.Source);
        var client=new Mock<IBusinessProcessServiceClient>();
        client.Setup(value=>value.ReviewMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStartQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(value=>value.GetMaintenanceHandoffChoicesAsync(It.IsAny<CompanyMaintenanceSourceQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyMaintenanceStartCatalog(review.Context,review.Source,[review]));
        client.Setup(value=>value.StartMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStart>(),It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Unknown response"));
        await state.LoadChoicesAsync(client.Object);await state.ReviewAsync(client.Object,"workflow");await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitAsync(client.Object));
        var original=state.Intent!;var held=new TaskCompletionSource<CompanyMaintenanceProcessStartReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.Setup(value=>value.RecoverMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStart>(),It.IsAny<CancellationToken>())).Returns(held.Task);
        var pending=state.RecoverAsync(client.Object);Assert.True(state.Busy);
        if(change=="company")company.Clear();else state.AttachSource(null);
        held.SetResult(new(original.Context.Company,original.Context.ActorUserId,original.Context.FieldId!,original.IntentId,original.InstanceId(),original.Source.WorkOrderId,original.Source.DecisionId,"receiving-step",["receiving-task"],new string('D',64)));
        await pending;Assert.Null(state.Receipt);Assert.Equal(original,state.Intent);Assert.False(state.Busy);
    }
    [Theory][InlineData("allowed")][InlineData("missing")][InlineData("late-source")]
    public async Task FreshCircuitRestoresTheOriginalReferenceOnlyAfterReadOnlyConfirmation(string fault)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["MaintenanceRole"],[])));
        using var state=new CompanyMaintenanceHandoffState(company);var review=new Transport().Review;state.AttachSource(review.Source);
        var intent=review.CreateIntent(Guid.NewGuid());var result=new CompanyMaintenanceRecovery(intent,new(intent.Context.Company,intent.Context.ActorUserId,intent.Context.FieldId!,intent.IntentId,intent.InstanceId(),intent.Source.WorkOrderId,intent.Source.DecisionId,"receiving-step",["receiving-task"],new string('D',64)));
        var client=new Mock<ICompanyMaintenanceHandoffClient>();var held=new TaskCompletionSource<CompanyMaintenanceRecovery>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.Setup(value=>value.RecoverMaintenanceHandoffReferenceAsync(It.IsAny<CompanyMaintenanceRecoveryQuery>(),It.IsAny<CancellationToken>())).Returns(held.Task);
        var pending=state.RecoverReferenceAsync(client.Object,intent.IntentId);Assert.Equal(intent.IntentId,state.Reference);Assert.Null(state.Intent);Assert.True(state.Busy);
        if(fault=="missing")held.SetException(new InvalidOperationException("Original assignment missing"));
        else {if(fault=="late-source")state.AttachSource(null);held.SetResult(result);}
        if(fault=="missing")await Assert.ThrowsAsync<InvalidOperationException>(()=>pending);else await pending;
        if(fault=="allowed"){Assert.Equal(intent,state.Intent);Assert.NotNull(state.Receipt);}
        else {Assert.Null(state.Intent);Assert.Null(state.Receipt);Assert.Equal(intent.IntentId,state.Reference);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAsync(client.Object,"workflow"));}
        Assert.False(state.Busy);client.Verify(value=>value.StartMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStart>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Fact] public async Task OriginalReferenceMustBeRetainedBeforeSubmissionTransport()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["MaintenanceRole"],[])));
        using var state=new CompanyMaintenanceHandoffState(company);var review=new Transport().Review;state.AttachSource(review.Source);var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.GetMaintenanceHandoffChoicesAsync(It.IsAny<CompanyMaintenanceSourceQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyMaintenanceStartCatalog(review.Context,review.Source,[review]));
        client.Setup(value=>value.ReviewMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStartQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);
        await state.LoadChoicesAsync(client.Object);await state.ReviewAsync(client.Object,"workflow");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitAsync(client.Object,id=>{Assert.Equal(id,state.Reference);Assert.Equal(id,state.Intent!.IntentId);return Task.FromException(new InvalidOperationException("Reference persistence failed"));}));
        Assert.NotNull(state.Intent);Assert.False(state.Busy);client.Verify(value=>value.StartMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStart>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Fact] public async Task UnconfirmedReferenceCannotBeMovedToAnotherCompanySource()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["MaintenanceRole"],[])));
        using var state=new CompanyMaintenanceHandoffState(company);var review=new Transport().Review;state.AttachSource(review.Source);var id=Guid.NewGuid();
        var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.RecoverMaintenanceHandoffReferenceAsync(It.IsAny<CompanyMaintenanceRecoveryQuery>(),It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Unconfirmed original"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecoverReferenceAsync(client.Object,id));
        Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","other-company"),["MaintenanceRole"],[])));
        state.AttachSource(review.Source with{Company=new("tenant","other-company")});Assert.Equal(id,state.Reference);Assert.Null(state.VisibleReference);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecoverReferenceAsync(client.Object,id));Assert.Null(state.VisibleReference);
        client.Verify(value=>value.RecoverMaintenanceHandoffReferenceAsync(It.IsAny<CompanyMaintenanceRecoveryQuery>(),It.IsAny<CancellationToken>()),Times.Once);
    }
}
#endif
