using System;using System.Collections.Generic;using System.Linq;using System.Threading;using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;using TheTechIdea.Beep.SetUp;using TheTechIdea.Beep.SetUp.Adapters;

namespace Beep.OilandGas.ApiService.Controllers
{
    [ApiController][Route("api/setup/wizard")][Authorize(Roles="Admin,Administrator")]
    public class SetupWizardController:ControllerBase
    {
        private readonly IDMEEditor _editor;private readonly PpdmModuleSeedingService _ppdmService;
        private readonly ILogger<SetupWizardController> _logger;private readonly ILoggerFactory _loggerFactory;
        private static WebApiSetupWizardAdapter? _adapter;
        private static IReadOnlyList<ISetupStep>? _wizardSteps;
        private static CancellationTokenSource? _wizardCts;
        private static DateTime _startedAt;

        public SetupWizardController(IDMEEditor editor,PpdmModuleSeedingService ppdmService,
            ILogger<SetupWizardController> logger,ILoggerFactory loggerFactory)
        {_editor=editor;_ppdmService=ppdmService;_logger=logger;_loggerFactory=loggerFactory;}

        [HttpGet("preflight")][AllowAnonymous]
        public IActionResult Preflight([FromQuery]string? connectionName=null)
        {
            var dsService=new TheTechIdea.Beep.Services.DatasourceManagement.DatasourceManagementService(_editor);
            var connections=dsService.GetAllDatasources();
            var connList=connections.Select(c=>new{name=c.ConnectionName,type=c.DatabaseType.ToString(),server=c.Host,category=c.Category.ToString()}).ToList();

            // Check if target datasource exists and probe for WELL table
            int tableCount=0;bool hasExistingTables=false;
            var target=string.IsNullOrWhiteSpace(connectionName)?connections.FirstOrDefault():connections.FirstOrDefault(c=>string.Equals(c.ConnectionName,connectionName,StringComparison.OrdinalIgnoreCase));
            if(target!=null){try{var ds=_editor.GetDataSource(target.ConnectionName);if(ds!=null){var well=ds.GetEntity("WELL",new List<TheTechIdea.Beep.Report.AppFilter>());hasExistingTables=well!=null&&((System.Collections.IEnumerable)well).Cast<object>().Any();tableCount=hasExistingTables?1:0;}}catch{}}

            var drivers=_editor.ConfigEditor?.DataDriversClasses?.Select(d=>new{name=d.DatasourceType.ToString(),loaded=!string.IsNullOrEmpty(d.classHandler),package=d.PackageName}).ToList()??new();

            return Ok(new{
                isFirstRun=connList.Count==0,
                hasExistingDatasource=connList.Count>0,
                datasources=connList,
                hasExistingTables,existingTableCount=tableCount,
                installedDrivers=drivers,
                connectionName=connectionName??"PPDM39"
            });
        }

        [HttpPost("start")]
        public IActionResult StartWizard([FromQuery]string connectionName="PPDM39")
        {
            if(_adapter?.Status?.State=="Running")
                return Conflict(new{message="Setup wizard is already running"});
            _adapter=new WebApiSetupWizardAdapter();_wizardCts=new CancellationTokenSource();_startedAt=DateTime.UtcNow;
            _=Task.Run(async()=>{
                try{
                    var wl=_loggerFactory.CreateLogger<DefaultSetupWizardFactory>();
                    var factory=new DefaultSetupWizardFactory(wl);
                    var (wizard,context)=factory.Create(_editor,new SetupOptions{
                        Environment="PPDM39",
                        StateFilePath=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Beep.OilandGas","setup-wizard-state.json")
                    },builder=>builder.AddStep(new PpdmWizardFinalStep(_ppdmService,_loggerFactory.CreateLogger<PpdmWizardFinalStep>())));
                    _wizardSteps=wizard.Steps;
                    await _adapter.RunAsync(wizard,context,_wizardCts.Token);
                    _logger.LogInformation("Setup wizard completed: {State}",_adapter.Status.State);
                }catch(OperationCanceledException){_logger.LogWarning("Setup wizard cancelled");}
                catch(Exception ex){_logger.LogError(ex,"Setup wizard failed");_adapter.Status.State="Failed";_adapter.Status.CurrentMessage=ex.Message;}
            });
            return Accepted(new{message="Setup wizard started",statusEndpoint="/api/setup/wizard/status"});
        }

        [HttpGet("status")][AllowAnonymous]
        public IActionResult GetStatus()
        {
            if(_adapter?.Status==null)return Ok(new{state="Idle"});
            var s=_adapter.Status;
            var steps=BuildStepList();
            return Ok(new{state=s.State,currentStep=s.CurrentStepName,message=s.CurrentMessage,
                percent=s.PercentComplete,totalSteps=s.TotalSteps,currentStepIndex=s.CurrentStepIndex,
                steps,elapsedSeconds=(int)(DateTime.UtcNow-_startedAt).TotalSeconds});
        }

        [HttpPost("cancel")]
        public IActionResult CancelWizard()
        {
            if(_wizardCts==null||_adapter?.Status?.State!="Running")return NotFound(new{message="No wizard is running"});
            _wizardCts.Cancel();return Ok(new{message="Cancellation requested"});
        }

        private List<StepState> BuildStepList()
        {
            var r=new List<StepState>();
            if(_wizardSteps==null)return r;
            var s=_adapter!.Status;
            int currentIdx=s.CurrentStepIndex>=0?s.CurrentStepIndex:_wizardSteps.Count;
            bool failed=s.State=="Failed";

            for(int i=0;i<_wizardSteps.Count;i++)
            {
                var step=_wizardSteps[i];
                string status="pending";
                if(i<currentIdx)status="completed";
                else if(i==currentIdx)status=failed?"failed":"running";
                r.Add(new StepState{id=step.StepId,name=step.StepName,description=GetStepDescription(step.StepId),status=status});
            }
            return r;
        }

        private static string GetStepDescription(string stepId)=>stepId switch
        {
            "defaults-setup"=>"Audit timestamps & defaults","driver-provision"=>"Database driver packages",
            "connection-config"=>"Datasource connection","schema-setup"=>"Entity-to-table DDL",
            "seeding"=>"Reference data seeding","data-import"=>"Entity verification",
            "ppdm-modules"=>"PPDM domain modules","beep-web-user-wizard-defaults"=>"Audit timestamps & defaults",_=>"Setup step"
        };
    }

    public class StepState{public string id{get;set;}="";public string name{get;set;}="";public string description{get;set;}="";public string status{get;set;}="pending";}

    internal class PpdmWizardFinalStep:ISetupStep
    {
        private readonly PpdmModuleSeedingService _svc;private readonly ILogger _log;
        public string StepId=>"ppdm-modules";public string StepName=>"PPDM Modules";public string Description=>"Seeds all PPDM domain modules";
        public IReadOnlyList<string> DependsOn=>new[]{"schema-setup"};
        public PpdmWizardFinalStep(PpdmModuleSeedingService svc,ILogger log){_svc=svc;_log=log;}
        public bool CanSkip(SetupContext ctx)=>ctx.State?.CompletedStepIds?.Contains(StepId)==true;
        public IErrorsInfo Validate(SetupContext ctx){return new TheTechIdea.Beep.ConfigUtil.ErrorsInfo();}
        public IErrorsInfo Execute(SetupContext ctx,IProgress<TheTechIdea.Beep.PassedArgs>? progress=null)
        {
            progress?.Report(new TheTechIdea.Beep.PassedArgs{Messege="Seeding PPDM modules...",ParameterInt1=0,ParameterInt2=_svc.Modules.Count});
            var report=_svc.SeedAllAsync(progress:p=>progress?.Report(new TheTechIdea.Beep.PassedArgs{Messege=p.message,ParameterInt1=p.done,ParameterInt2=p.total})).ConfigureAwait(false).GetAwaiter().GetResult();
            var errors=new TheTechIdea.Beep.ConfigUtil.ErrorsInfo();
            if(!report.Completed)errors.FlagError($"PPDM seeding aborted at module {report.AbortModuleId}",StepId);
            else _log.LogInformation("PPDM seeding: {Succeeded}/{Total} modules",report.Succeeded,report.TotalModules);
            return errors;
        }
    }
}
