using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Processes
{
    /// <summary>
    /// Helper class to initialize default process definitions
    /// </summary>
    public class ProcessDefinitionInitializer
    {
        private readonly IProcessService _processService;
        private readonly ILogger<ProcessDefinitionInitializer>? _logger;

        public ProcessDefinitionInitializer(
            IProcessService processService,
            ILogger<ProcessDefinitionInitializer>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _logger = logger;
        }

        /// <summary>
        /// Initialize all default process definitions
        /// </summary>
        public async Task InitializeDefaultProcessDefinitionsAsync(string userId)
        {
            try
            {
                _logger?.LogInformation("Initializing default process definitions...");

                // Exploration processes
                await InitializeLeadToProspectProcessAsync(userId);
                await InitializeProspectToDiscoveryProcessAsync(userId);
                await InitializeDiscoveryToDevelopmentProcessAsync(userId);

                // Development processes
                await InitializePoolDefinitionProcessAsync(userId);
                await InitializeFacilityDevelopmentProcessAsync(userId);
                await InitializeWellDevelopmentProcessAsync(userId);
                await InitializePipelineDevelopmentProcessAsync(userId);

                // Production processes
                await InitializeWellStartupProcessAsync(userId);
                await InitializeProductionOperationsProcessAsync(userId);
                await InitializeDeclineManagementProcessAsync(userId);
                await InitializeWorkoverProcessAsync(userId);

                // Decommissioning processes
                await InitializeWellAbandonmentProcessAsync(userId);
                await InitializeFacilityDecommissioningProcessAsync(userId);

                _logger?.LogInformation("Default process definitions initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing default process definitions");
                throw;
            }
        }

        #region Exploration Process Definitions

        private async Task InitializeLeadToProspectProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "LEAD_TO_PROSPECT",
                ProcessName = "LeadToProspect",
                ProcessType = "EXPLORATION",
                EntityType = "LEAD",
                Description = "Workflow for promoting a Lead to a Prospect",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition
                    {
                        StepId = "LEAD_CREATION",
                        StepName = "Lead Creation",
                        SequenceNumber = 1,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = "LEAD_EVALUATION"
                    },
                    new ProcessStepDefinition
                    {
                        StepId = "LEAD_EVALUATION",
                        StepName = "Lead Evaluation",
                        SequenceNumber = 2,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = "LEAD_APPROVAL"
                    },
                    new ProcessStepDefinition
                    {
                        StepId = "LEAD_APPROVAL",
                        StepName = "Lead Approval",
                        SequenceNumber = 3,
                        StepType = "APPROVAL",
                        IsRequired = true,
                        RequiresApproval = true,
                        NextStepId = "PROSPECT_CREATION"
                    },
                    new ProcessStepDefinition
                    {
                        StepId = "PROSPECT_CREATION",
                        StepName = "Prospect Creation",
                        SequenceNumber = 4,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = "PROSPECT_ASSESSMENT"
                    },
                    new ProcessStepDefinition
                    {
                        StepId = "PROSPECT_ASSESSMENT",
                        StepName = "Prospect Initial Assessment",
                        SequenceNumber = 5,
                        StepType = "ACTION",
                        IsRequired = false,
                        NextStepId = string.Empty
                    }
                },
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeProspectToDiscoveryProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "PROSPECT_TO_DISCOVERY",
                ProcessName = "ProspectToDiscovery",
                ProcessType = "EXPLORATION",
                EntityType = "PROSPECT",
                Description = "Workflow for evaluating a Prospect and recording Discovery",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition
                    {
                        StepId = "PROSPECT_CREATION",
                        StepName = "Prospect Creation",
                        SequenceNumber = 1,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = "RISK_ASSESSMENT"
                    },
                    new ProcessStepDefinition
                    {
                        StepId = "RISK_ASSESSMENT",
                        StepName = "Risk Assessment",
                        SequenceNumber = 2,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = "VOLUME_ESTIMATION"
                    },
                    new ProcessStepDefinition
                    {
                        StepId = "VOLUME_ESTIMATION",
                        StepName = "Volume Estimation",
                        SequenceNumber = 3,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = "ECONOMIC_EVALUATION"
                    },
                    new ProcessStepDefinition
                    {
                        StepId = "ECONOMIC_EVALUATION",
                        StepName = "Economic Evaluation",
                        SequenceNumber = 4,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = "DRILLING_DECISION"
                    },
                    new ProcessStepDefinition
                    {
                        StepId = "DRILLING_DECISION",
                        StepName = "Drilling Decision",
                        SequenceNumber = 5,
                        StepType = "APPROVAL",
                        IsRequired = true,
                        RequiresApproval = true,
                        NextStepId = "DISCOVERY_RECORDING"
                    },
                    new ProcessStepDefinition
                    {
                        StepId = "DISCOVERY_RECORDING",
                        StepName = "Discovery Recording",
                        SequenceNumber = 6,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = string.Empty
                    }
                },
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeDiscoveryToDevelopmentProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "DISCOVERY_TO_DEVELOPMENT",
                ProcessName = "DiscoveryToDevelopment",
                ProcessType = "EXPLORATION",
                EntityType = "DISCOVERY",
                Description = "Workflow for appraising Discovery and making Development decision",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition
                    {
                        StepId = "DISCOVERY_RECORDING",
                        StepName = "Discovery Recording",
                        SequenceNumber = 1,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = "APPRAISAL"
                    },
                    new ProcessStepDefinition
                    {
                        StepId = "APPRAISAL",
                        StepName = "Appraisal",
                        SequenceNumber = 2,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = "RESERVE_ESTIMATION"
                    },
                    new ProcessStepDefinition
                    {
                        StepId = "RESERVE_ESTIMATION",
                        StepName = "Reserve Estimation",
                        SequenceNumber = 3,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = "ECONOMIC_ANALYSIS"
                    },
                    new ProcessStepDefinition
                    {
                        StepId = "ECONOMIC_ANALYSIS",
                        StepName = "Economic Analysis",
                        SequenceNumber = 4,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = "DEVELOPMENT_APPROVAL"
                    },
                    new ProcessStepDefinition
                    {
                        StepId = "DEVELOPMENT_APPROVAL",
                        StepName = "Development Approval",
                        SequenceNumber = 5,
                        StepType = "APPROVAL",
                        IsRequired = true,
                        RequiresApproval = true,
                        NextStepId = string.Empty
                    }
                },
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        #endregion

        #region Development Process Definitions

        private async Task InitializePoolDefinitionProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "POOL_DEFINITION",
                ProcessName = "PoolDefinition",
                ProcessType = "DEVELOPMENT",
                EntityType = "POOL",
                Description = "Workflow for defining and approving a Pool",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition { StepId = "POOL_IDENTIFICATION", StepName = "Pool Identification", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, NextStepId = "POOL_DELINEATION" },
                    new ProcessStepDefinition { StepId = "POOL_DELINEATION", StepName = "Pool Delineation", SequenceNumber = 2, StepType = "ACTION", IsRequired = true, NextStepId = "RESERVE_ASSIGNMENT" },
                    new ProcessStepDefinition { StepId = "RESERVE_ASSIGNMENT", StepName = "Reserve Assignment", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, NextStepId = "POOL_APPROVAL" },
                    new ProcessStepDefinition { StepId = "POOL_APPROVAL", StepName = "Pool Approval", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = "POOL_ACTIVATION" },
                    new ProcessStepDefinition { StepId = "POOL_ACTIVATION", StepName = "Pool Activation", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, NextStepId = string.Empty }
                },
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeFacilityDevelopmentProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "FACILITY_DEVELOPMENT",
                ProcessName = "FacilityDevelopment",
                ProcessType = "DEVELOPMENT",
                EntityType = "FACILITY",
                Description = "Workflow for developing a Facility",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition { StepId = "FACILITY_PLANNING", StepName = "Facility Planning", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, NextStepId = "FACILITY_DESIGN" },
                    new ProcessStepDefinition { StepId = "FACILITY_DESIGN", StepName = "Facility Design", SequenceNumber = 2, StepType = "ACTION", IsRequired = true, NextStepId = "FACILITY_PERMITTING" },
                    new ProcessStepDefinition { StepId = "FACILITY_PERMITTING", StepName = "Permitting", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, NextStepId = "CONSTRUCTION" },
                    new ProcessStepDefinition { StepId = "CONSTRUCTION", StepName = "Construction", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, NextStepId = "FACILITY_TESTING" },
                    new ProcessStepDefinition { StepId = "FACILITY_TESTING", StepName = "Testing", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, NextStepId = "COMMISSIONING" },
                    new ProcessStepDefinition { StepId = "COMMISSIONING", StepName = "Commissioning", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = "FACILITY_ACTIVATION" },
                    new ProcessStepDefinition { StepId = "FACILITY_ACTIVATION", StepName = "Facility Activation", SequenceNumber = 7, StepType = "ACTION", IsRequired = true, NextStepId = string.Empty }
                },
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeWellDevelopmentProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "WELL_DEVELOPMENT",
                ProcessName = "WellDevelopment",
                ProcessType = "DEVELOPMENT",
                EntityType = "WELL",
                Description = "Workflow for developing a Well",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition { StepId = "WELL_PLANNING", StepName = "Well Planning", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, NextStepId = "DRILLING_PERMIT" },
                    new ProcessStepDefinition { StepId = "DRILLING_PERMIT", StepName = "Drilling Permit", SequenceNumber = 2, StepType = "ACTION", IsRequired = true, NextStepId = "DRILLING" },
                    new ProcessStepDefinition { StepId = "DRILLING", StepName = "Drilling", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, NextStepId = "COMPLETION" },
                    new ProcessStepDefinition { StepId = "COMPLETION", StepName = "Completion", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, NextStepId = "PRODUCTION_TESTING" },
                    new ProcessStepDefinition { StepId = "PRODUCTION_TESTING", StepName = "Production Testing", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, NextStepId = "PRODUCTION_HANDOVER" },
                    new ProcessStepDefinition { StepId = "PRODUCTION_HANDOVER", StepName = "Production Handover", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializePipelineDevelopmentProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "PIPELINE_DEVELOPMENT",
                ProcessName = "PipelineDevelopment",
                ProcessType = "DEVELOPMENT",
                EntityType = "PIPELINE",
                Description = "Workflow for developing a Pipeline",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition { StepId = "PIPELINE_PLANNING", StepName = "Pipeline Planning", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, NextStepId = "PIPELINE_DESIGN" },
                    new ProcessStepDefinition { StepId = "PIPELINE_DESIGN", StepName = "Pipeline Design", SequenceNumber = 2, StepType = "ACTION", IsRequired = true, NextStepId = "PIPELINE_PERMITTING" },
                    new ProcessStepDefinition { StepId = "PIPELINE_PERMITTING", StepName = "Permitting", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, NextStepId = "PIPELINE_CONSTRUCTION" },
                    new ProcessStepDefinition { StepId = "PIPELINE_CONSTRUCTION", StepName = "Construction", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, NextStepId = "PIPELINE_TESTING" },
                    new ProcessStepDefinition { StepId = "PIPELINE_TESTING", StepName = "Testing", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, NextStepId = "PIPELINE_COMMISSIONING" },
                    new ProcessStepDefinition { StepId = "PIPELINE_COMMISSIONING", StepName = "Commissioning", SequenceNumber = 6, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = "PIPELINE_ACTIVATION" },
                    new ProcessStepDefinition { StepId = "PIPELINE_ACTIVATION", StepName = "Pipeline Activation", SequenceNumber = 7, StepType = "ACTION", IsRequired = true, NextStepId = string.Empty }
                },
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        #endregion

        #region Production Process Definitions

        private async Task InitializeWellStartupProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "WELL_STARTUP",
                ProcessName = "WellStartup",
                ProcessType = "PRODUCTION",
                EntityType = "WELL",
                Description = "Workflow for starting well production",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition { StepId = "WELL_COMPLETION", StepName = "Well Completion", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, NextStepId = "PRODUCTION_TESTING" },
                    new ProcessStepDefinition { StepId = "PRODUCTION_TESTING", StepName = "Production Testing", SequenceNumber = 2, StepType = "ACTION", IsRequired = true, NextStepId = "PRODUCTION_APPROVAL" },
                    new ProcessStepDefinition { StepId = "PRODUCTION_APPROVAL", StepName = "Production Approval", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = "PRODUCTION_START" },
                    new ProcessStepDefinition { StepId = "PRODUCTION_START", StepName = "Production Start", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, NextStepId = "PRODUCING_CONFIRMATION" },
                    new ProcessStepDefinition { StepId = "PRODUCING_CONFIRMATION", StepName = "Producing Confirmation", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, NextStepId = string.Empty }
                },
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeProductionOperationsProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "PRODUCTION_OPERATIONS",
                ProcessName = "ProductionOperations",
                ProcessType = "PRODUCTION",
                EntityType = "WELL",
                Description = "Workflow for production operations and optimization",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition { StepId = "DAILY_PRODUCTION", StepName = "Daily Production", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, NextStepId = "PRODUCTION_MONITORING" },
                    new ProcessStepDefinition { StepId = "PRODUCTION_MONITORING", StepName = "Production Monitoring", SequenceNumber = 2, StepType = "ACTION", IsRequired = true, NextStepId = "PERFORMANCE_ANALYSIS" },
                    new ProcessStepDefinition { StepId = "PERFORMANCE_ANALYSIS", StepName = "Performance Analysis", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, NextStepId = "OPTIMIZATION_DECISION" },
                    new ProcessStepDefinition { StepId = "OPTIMIZATION_DECISION", StepName = "Optimization Decision", SequenceNumber = 4, StepType = "ACTION", IsRequired = false, NextStepId = "OPTIMIZATION_EXECUTION" },
                    new ProcessStepDefinition { StepId = "OPTIMIZATION_EXECUTION", StepName = "Optimization Execution", SequenceNumber = 5, StepType = "ACTION", IsRequired = false, NextStepId = string.Empty }
                },
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeDeclineManagementProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "DECLINE_MANAGEMENT",
                ProcessName = "DeclineManagement",
                ProcessType = "PRODUCTION",
                EntityType = "WELL",
                Description = "Workflow for managing production decline",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition { StepId = "DECLINE_DETECTION", StepName = "Decline Detection", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, NextStepId = "DCA_ANALYSIS" },
                    new ProcessStepDefinition { StepId = "DCA_ANALYSIS", StepName = "DCA Analysis", SequenceNumber = 2, StepType = "ACTION", IsRequired = true, NextStepId = "PRODUCTION_FORECAST" },
                    new ProcessStepDefinition { StepId = "PRODUCTION_FORECAST", StepName = "Production Forecast", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, NextStepId = "ECONOMIC_ANALYSIS" },
                    new ProcessStepDefinition { StepId = "ECONOMIC_ANALYSIS", StepName = "Economic Analysis", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, NextStepId = "WORKOVER_DECISION" },
                    new ProcessStepDefinition { StepId = "WORKOVER_DECISION", StepName = "Workover Decision", SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeWorkoverProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "WORKOVER",
                ProcessName = "Workover",
                ProcessType = "PRODUCTION",
                EntityType = "WELL",
                Description = "Workflow for well workover operations",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition { StepId = "WORKOVER_PLANNING", StepName = "Workover Planning", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, NextStepId = "WORKOVER_APPROVAL" },
                    new ProcessStepDefinition { StepId = "WORKOVER_APPROVAL", StepName = "Workover Approval", SequenceNumber = 2, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = "WORKOVER_EXECUTION" },
                    new ProcessStepDefinition { StepId = "WORKOVER_EXECUTION", StepName = "Workover Execution", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, NextStepId = "POST_WORKOVER_TESTING" },
                    new ProcessStepDefinition { StepId = "POST_WORKOVER_TESTING", StepName = "Post-Workover Testing", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, NextStepId = "PRODUCTION_RESTART" },
                    new ProcessStepDefinition { StepId = "PRODUCTION_RESTART", StepName = "Production Restart", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, NextStepId = string.Empty }
                },
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        #endregion

        #region Decommissioning Process Definitions

        private async Task InitializeWellAbandonmentProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "WELL_ABANDONMENT",
                ProcessName = "WellAbandonment",
                ProcessType = "DECOMMISSIONING",
                EntityType = "WELL",
                Description = "Workflow for well abandonment",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition { StepId = "ABANDONMENT_PLANNING", StepName = "Abandonment Planning", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, NextStepId = "REGULATORY_APPROVAL" },
                    new ProcessStepDefinition { StepId = "REGULATORY_APPROVAL", StepName = "Regulatory Approval", SequenceNumber = 2, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = "WELL_PLUGGING" },
                    new ProcessStepDefinition { StepId = "WELL_PLUGGING", StepName = "Well Plugging", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, NextStepId = "SITE_RESTORATION" },
                    new ProcessStepDefinition { StepId = "SITE_RESTORATION", StepName = "Site Restoration", SequenceNumber = 4, StepType = "ACTION", IsRequired = true, NextStepId = "ABANDONMENT_COMPLETION" },
                    new ProcessStepDefinition { StepId = "ABANDONMENT_COMPLETION", StepName = "Abandonment Completion", SequenceNumber = 5, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        private async Task InitializeFacilityDecommissioningProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "FACILITY_DECOMMISSIONING",
                ProcessName = "FacilityDecommissioning",
                ProcessType = "DECOMMISSIONING",
                EntityType = "FACILITY",
                Description = "Workflow for facility decommissioning",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition { StepId = "DECOMMISSIONING_PLANNING", StepName = "Decommissioning Planning", SequenceNumber = 1, StepType = "ACTION", IsRequired = true, NextStepId = "EQUIPMENT_REMOVAL" },
                    new ProcessStepDefinition { StepId = "EQUIPMENT_REMOVAL", StepName = "Equipment Removal", SequenceNumber = 2, StepType = "ACTION", IsRequired = true, NextStepId = "SITE_CLEANUP" },
                    new ProcessStepDefinition { StepId = "SITE_CLEANUP", StepName = "Site Cleanup", SequenceNumber = 3, StepType = "ACTION", IsRequired = true, NextStepId = "REGULATORY_CLOSURE" },
                    new ProcessStepDefinition { StepId = "REGULATORY_CLOSURE", StepName = "Regulatory Closure", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, NextStepId = "DECOMMISSIONING_COMPLETION" },
                    new ProcessStepDefinition { StepId = "DECOMMISSIONING_COMPLETION", StepName = "Decommissioning Completion", SequenceNumber = 5, StepType = "ACTION", IsRequired = true, NextStepId = string.Empty }
                },
                Transitions = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };

            await _processService.CreateProcessDefinitionAsync(definition, userId);
        }

        #endregion
    }
}

