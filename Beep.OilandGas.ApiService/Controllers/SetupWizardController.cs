using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.SetUp.Adapters;
using TheTechIdea.Beep.SetUp.Seeding;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// Runs the BeepDM SetupWizard with PPDM module seeders integrated.
    /// Uses WebApiSetupWizardAdapter for background execution with polling status.
    /// </summary>
    [ApiController][Route("api/setup/wizard")][Authorize(Roles="Admin,Administrator")]
    public class SetupWizardController:ControllerBase
    {
        private readonly IDMEEditor _editor;
        private readonly PpdmModuleSeedingService _ppdmService;
        private readonly ILogger<SetupWizardController> _logger;
        private static SetupAdapterStatus? _status; // shared across requests (wizard runs once)

        public SetupWizardController(
            IDMEEditor editor,
            PpdmModuleSeedingService ppdmService,
            ILogger<SetupWizardController> logger)
        {
            _editor=editor;
            _ppdmService=ppdmService;
            _logger=logger;
        }

        /// <summary>Starts the BeepDM SetupWizard with PPDM modules registered as seeders.</summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartWizard([FromQuery] string connectionName="PPDM39")
        {
            if(_status?.State==SetupRunState.Running)
                return Conflict(new{message="Setup wizard is already running"});

            _status=new SetupAdapterStatus{State=SetupRunState.Running,CurrentStepName="Initializing..."};

            try
            {
                // Build wizard with PPDM modules registered as seeders
                var factory = new DefaultSetupWizardFactory(
                    _logger as ILogger<DefaultSetupWizardFactory>);

                var (wizard, context) = factory.Create(_editor, new SetupOptions
                {
                    Environment = "PPDM39",
                    StateFilePath = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Beep.OilandGas", "setup-wizard-state.json")
                }, builder =>
                {
                    // Register PPDM modules into the SeedingStep's registry
                    // by adding them after the standard steps
                    builder.AddStep(new PpdmWizardFinalStep(_ppdmService, _logger));
                });

                // Run via WebApi adapter (background thread + polling)
                var adapter = new WebApiSetupWizardAdapter(_status, _logger);
                var report = await adapter.RunAsync(wizard, context, CancellationToken.None);

                _status.State = SetupRunState.Completed;
                _status.Report = report;
                return Ok(new{message="Setup complete", report});
            }
            catch(Exception ex)
            {
                _status.State=SetupRunState.Failed;
                _status.CurrentMessage=ex.Message;
                _logger.LogError(ex,"Setup wizard failed");
                return StatusCode(500,new{error=ex.Message});
            }
        }

        /// <summary>Polls the current wizard execution status.</summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            if(_status==null)
                return Ok(new{state="Idle"});

            return Ok(new{
                state=_status.State.ToString(),
                step=_status.CurrentStepName,
                message=_status.CurrentMessage,
                percent=_status.PercentComplete
            });
        }
    }

    /// <summary>
    /// Final ISetupStep that runs PPDM module seeding after the standard steps.
    /// Inserted at the end of the SetupWizard pipeline.
    /// </summary>
    internal class PpdmWizardFinalStep:ISetupStep
    {
        private readonly PpdmModuleSeedingService _svc;
        private readonly ILogger _log;
        public string StepId=>"ppdm-modules";public string StepName=>"PPDM Module Seeding";
        public string Description=>"Seeds all PPDM domain modules (Exploration, Development, Production, Accounting, etc.)";
        public IReadOnlyList<string> DependsOn=>new[]{"schema-setup"};

        public PpdmWizardFinalStep(PpdmModuleSeedingService svc,ILogger log){_svc=svc;_log=log;}
        public bool CanSkip(SetupContext ctx)=>false;
        public IErrorsInfo Validate(SetupContext ctx)=>new TheTechIdea.Beep.ConfigUtil.ErrorsInfo();
        public IErrorsInfo Execute(SetupContext ctx,IProgress<TheTechIdea.Beep.PassedArgs>? progress=null)
        {
            var report=_svc.SeedAllAsync().GetAwaiter().GetResult();
            var errors=new TheTechIdea.Beep.ConfigUtil.ErrorsInfo();
            if(!report.Completed)
                errors.FlagError($"PPDM seeding aborted at {report.AbortModuleId}",StepId);
            return errors;
        }
    }
}
