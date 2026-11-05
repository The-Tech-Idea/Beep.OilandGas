using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;
using Moq;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyAccountingProcessReadTests
{
    private static readonly CompanyScope Scope=new("tenant","company");
    private static readonly ClaimsPrincipal User=new(new ClaimsIdentity([new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"));
    private static ProcessInstance Source(string type="AP_INVOICE")=>new(){InstanceId="process",ProcessId="definition",EntityType=type,EntityId="source/α",FieldId="",
        StartDate=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc),StartedBy="maker",ProcessData=new(){InstanceId="process",Data=new(){
            [CompanyAccountingProcessBinding.PayloadKey]=new CompanyAccountingProcessBinding(Scope,"process",type,"source/α")}}};
    private sealed class Fixture
    {
        public Mock<IProcessService> Processes=new(MockBehavior.Strict);
        public Mock<IAccessControlService> Resources=new(MockBehavior.Strict);
        public ProcessInstance? Recorded=Source();
        public int AuthorityCalls,SourceReads;
        public Func<int,(RepositoryCompanyAccess Access,string Binding,string Actor)>? Authority;
        public Func<int,string>? Revision;
        public Func<int,ProcessInstance?>? Process;
        private int processReads;
        public Fixture(){Processes.Setup(row=>row.GetProcessInstanceAsync("process")).ReturnsAsync(()=>Process is null?Recorded:Process(++processReads));}
        public Task<ProcessInstance?> Read()=>new CompanyProcessReadService(Resources.Object).ReadAccountingAsync(Processes.Object,User,"actor",Scope,"production+lifecycle","process",
            ()=>{AuthorityCalls++;return Task.FromResult(Authority is null?(new RepositoryCompanyAccess(Scope,["ConfiguredCompanyRole"],["Accounting.View","Procurement.View"]),"production+lifecycle","actor"):Authority(AuthorityCalls));},
            (scope,actor,type,id)=>{Assert.Equal(Scope,scope);Assert.Equal("actor",actor);Assert.Equal(Recorded!.EntityType,type);Assert.Equal("source/α",id);
                SourceReads++;return Task.FromResult(Revision?.Invoke(SourceReads) ?? new string('A',64));});
    }
    [Theory][InlineData("AP_INVOICE")][InlineData("PURCHASE_ORDER")]
    public async Task ExplicitCompanyProcessUsesNativeSourceAndNeverSelectedFieldAccess(string type)
    {
        var fixture=new Fixture{Recorded=Source(type)};var returned=await fixture.Read();Assert.Equal("",returned!.FieldId);Assert.Equal("source/α",returned.EntityId);
        Assert.Equal(6,fixture.AuthorityCalls);Assert.Equal(2,fixture.SourceReads);fixture.Resources.VerifyNoOtherCalls();
        fixture.Recorded!.EntityId="mutated";Assert.Equal("source/α",returned.EntityId);
        var payload=JsonSerializer.Deserialize<Dictionary<string,object>>(JsonSerializer.Serialize(returned.ProcessData.Data))!;
        CompanyAccountingProcessBinding.FromJson(JsonSerializer.Serialize(payload[CompanyAccountingProcessBinding.PayloadKey]),Scope,"process",type,"source/α");
    }
    [Theory][InlineData("tenant")][InlineData("company")][InlineData("missing")][InlineData("field")][InlineData("entity")]
    [InlineData("instance")][InlineData("payload-instance")][InlineData("type")]
    public async Task ForeignUnownedOrAmbiguousProcessScopeStopsBeforeNativeAccountingRead(string fault)
    {
        var f=new Fixture();var source=f.Recorded!;
        if(fault is "tenant" or "company")source.ProcessData.Data[CompanyAccountingProcessBinding.PayloadKey]=new CompanyAccountingProcessBinding(new(fault=="tenant"?"foreign":"tenant",fault=="company"?"foreign":"company"),"process",source.EntityType,source.EntityId);
        if(fault=="missing")source.ProcessData.Data.Clear();if(fault=="field")source.FieldId="unrelated-field";if(fault=="entity")source.EntityId="other";
        if(fault=="instance")source.InstanceId="other";if(fault=="payload-instance")source.ProcessData.InstanceId="other";if(fault=="type")source.EntityType="WELL";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Read());Assert.Equal(0,f.SourceReads);f.Resources.VerifyNoOtherCalls();
    }
    [Theory][InlineData(1,"company")][InlineData(2,"actor")][InlineData(3,"binding")][InlineData(4,"role")][InlineData(5,"permission")][InlineData(6,"permission")]
    public async Task AuthorityRevocationAtReadBoundariesPreventsPublication(int stage,string fault)
    {
        var f=new Fixture();f.Authority=call=>{
            var company=call==stage && fault=="company"?new CompanyScope("tenant","foreign"):Scope;
            return(new(company,call==stage && fault=="role"?[]:["ConfiguredCompanyRole"],call==stage && fault=="permission"?[]:["Accounting.View","Procurement.View"]),
                call==stage && fault=="binding"?"changed":"production+lifecycle",call==stage && fault=="actor"?"other":"actor");};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Read());Assert.Equal(stage,f.AuthorityCalls);
        Assert.Equal(stage<=2?0:stage<=5?1:2,f.SourceReads);
    }
    [Theory][InlineData("source")][InlineData("process")][InlineData("removed")][InlineData("invalid-source")]
    public async Task ChangedOrUnavailableClosingEvidenceCannotReturnACompanyProcess(string fault)
    {
        var f=new Fixture();if(fault=="source")f.Revision=read=>new string(read==1?'A':'B',64);
        if(fault=="invalid-source")f.Revision=_=>"";
        if(fault is "process" or "removed")f.Process=read=>{var source=Source();if(read==2){if(fault=="removed")return null;source.CurrentState="changed";}return source;};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Read());
    }
    [Fact] public async Task MissingRecordedCompanyProcessDoesNotReadAnAccountingSource()
    {var f=new Fixture{Recorded=null};Assert.Null(await f.Read());Assert.Equal(0,f.SourceReads);}
    [Theory][InlineData("")][InlineData("changed")]
    public async Task FixedProcessServiceRefusesUnconfirmedExpectedLifecycleConnection(string connection)
    {
        var factories=0;var bound=new Beep.OilandGas.LifeCycle.Services.Processes.BoundProcessService(()=>Task.FromResult(connection),_=>{factories++;return Mock.Of<IProcessService>();});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>bound.CreateBoundServiceAsync("lifecycle"));Assert.Equal(0,factories);
    }
    [Fact] public async Task FixedProcessServiceCreatesTheCapturedLifecycleTarget()
    {
        var process=Mock.Of<IProcessService>();var bound=new Beep.OilandGas.LifeCycle.Services.Processes.BoundProcessService(()=>Task.FromResult("lifecycle"),connection=>{Assert.Equal("lifecycle",connection);return process;});
        Assert.Same(process,await bound.CreateBoundServiceAsync("lifecycle"));Assert.Same(process,await bound.CreateBoundServiceAsync());
    }
    [Theory][InlineData("duplicate")][InlineData("missing")][InlineData("extra")][InlineData("camel")][InlineData("malformed")]
    public void StoredBindingJsonRequiresOneExactUnambiguousOwnershipShape(string fault)
    {
        var json=JsonSerializer.Serialize(new CompanyAccountingProcessBinding(Scope,"process","AP_INVOICE","source/α"));
        if(fault=="duplicate")json=json.Replace("\"InstanceId\":","\"InstanceId\":\"process\",\"InstanceId\":");
        if(fault=="missing")json=json.Replace("\"TenantId\":\"tenant\",","");
        if(fault=="extra")json=json.Replace("\"Company\":","\"Unknown\":true,\"Company\":");
        if(fault=="camel")json=json.Replace("\"Company\":","\"company\":");if(fault=="malformed")json="{";
        Assert.Throws<InvalidOperationException>(()=>CompanyAccountingProcessBinding.FromJson(json,Scope,"process","AP_INVOICE","source/α"));
    }
}
