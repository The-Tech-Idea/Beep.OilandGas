using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.Process;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.ProspectIdentification;

namespace Beep.OilandGas.LifeCycle.Services.Processes
{
    /// <summary>
    /// Helper class to initialize default process definitions
    /// </summary>
    public partial class ProcessDefinitionInitializer
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

                // Work order workflows
                await InitializeWorkOrderWorkflowsAsync(userId);

                // Approval & gate reviews
                await InitializeGateReviewWorkflowsAsync(userId);

                // HSE & safety workflows
                await InitializeHSEWorkflowsAsync(userId);

                // Compliance & regulatory workflows
                await InitializeComplianceWorkflowsAsync(userId);

                // Well lifecycle workflows
                await InitializeWellLifecycleWorkflowsAsync(userId);

                // Facility lifecycle workflows
                await InitializeFacilityLifecycleWorkflowsAsync(userId);

                // Reservoir management workflows
                await InitializeReservoirManagementWorkflowsAsync(userId);

                // Pipeline & infrastructure workflows
                await InitializePipelineInfrastructureWorkflowsAsync(userId);

                // Administrative & RBAC workflows (Phase 1)
                await InitializeRoleAssignmentApprovalAsync(userId);
                await InitializeTempRoleElevationApprovalAsync(userId);

                // Cross-role workflows — Finance (Phase 3)
                await InitializeCrossRoleFinanceWorkflowsAsync(userId);

                // Cross-role workflows — Operations (Phase 3)
                await InitializeCrossRoleOperationsWorkflowsAsync(userId);

                // Cross-role workflows — HSE & Compliance (Phase 3)
                await InitializeCrossRoleHSEComplianceWorkflowsAsync(userId);

                // Cross-role workflows — Asset Lifecycle (Phase 3)
                await InitializeCrossRoleAssetLifecycleWorkflowsAsync(userId);

                // SOD waiver workflow (Phase 4)
                await InitializeSodWaiverWorkflowAsync(userId);

                // O&G Accounting Standards workflows (Phase A — Critical Compliance)
                await InitializeAccountingStandardsWorkflowsAsync(userId);

                // O&G Accounting Standards workflows (Phases B, C, D)
                await InitializeAccountingStandardsPhaseBCDAsync(userId);

                // Basic operational accounting workflows (P2P, O2C, R2R, Assets, Bank, Expenses, etc.)
                await InitializeOperationalAccountingWorkflowsAsync(userId);

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
                ProcessId = ExplorationReferenceCodes.ProcessIdLeadToProspect,
                ProcessName = ExplorationReferenceCodes.ProcessNameLeadToProspect,
                ProcessType = ExplorationReferenceCodes.ProcessTypeExploration,
                EntityType = ExplorationReferenceCodes.EntityTypeLead,
                Description = "Workflow for promoting a Lead to a Prospect",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepLeadCreation,
                        StepName = "Lead Creation",
                        SequenceNumber = 1,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = ExplorationReferenceCodes.StepLeadEvaluation
                    },
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepLeadEvaluation,
                        StepName = "Lead Evaluation",
                        SequenceNumber = 2,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = ExplorationReferenceCodes.StepLeadApproval
                    },
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepLeadApproval,
                        StepName = "Lead Approval",
                        SequenceNumber = 3,
                        StepType = "APPROVAL",
                        IsRequired = true,
                        RequiresApproval = true,
                        NextStepId = ExplorationReferenceCodes.StepProspectCreation
                    },
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepProspectCreation,
                        StepName = "Prospect Creation",
                        SequenceNumber = 4,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = ExplorationReferenceCodes.StepProspectAssessment
                    },
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepProspectAssessment,
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

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
        }

        private async Task CreateProcessDefinitionIfNotExistsAsync(ProcessDefinition definition, string userId)
        {
            var existing = await _processService.GetProcessDefinitionAsync(definition.ProcessId);
            if (existing != null)
            {
                _logger?.LogDebug("Process definition '{ProcessId}' already exists, skipping", definition.ProcessId);
                return;
            }

            await _processService.CreateProcessDefinitionAsync(definition, userId);
            _logger?.LogInformation("Created process definition '{ProcessId}'", definition.ProcessId);
        }

        #region RBAC & Administrative Process Definitions (Phase 1)

        /// <summary>
        /// Role Assignment Approval workflow — enforces 4-eyes principle on role grants.
        /// Any role assignment to a non-Viewer role must go through this approval chain.
        /// </summary>
        private async Task InitializeRoleAssignmentApprovalAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "RBAC_ROLE_ASSIGNMENT",
                ProcessName = "Role Assignment Approval",
                ProcessType = "ADMINISTRATIVE",
                EntityType = "USER_ROLE",
                Description = "Enforces 4-eyes principle on role grants. Manager approval + security review required before activation.",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new()
                    {
                        StepId = "RBAC_ROLE_REQUEST",
                        StepName = "Request Role Assignment",
                        SequenceNumber = 1,
                        StepType = "ACTION",
                        IsRequired = true,
                        Description = "Requester fills in: user, role, field scope, justification",
                        NextStepId = "RBAC_MANAGER_APPROVAL",
                    },
                    new()
                    {
                        StepId = "RBAC_MANAGER_APPROVAL",
                        StepName = "Manager Approval",
                        SequenceNumber = 2,
                        StepType = "APPROVAL",
                        IsRequired = true,
                        RequiresApproval = true,
                        RequiredRoles = new List<string> { "Manager", "Supervisor" },
                        SlaHours = 48,
                        Description = "User's line manager reviews and approves/denies the role request",
                        NextStepId = "RBAC_SOD_CHECK",
                        StepConfiguration = new Dictionary<string, object>
                        {
                            ["approvalType"] = "ANY",
                            ["escalationHours"] = 72,
                            ["escalationAction"] = "NOTIFY_MANAGER"
                        }
                    },
                    new()
                    {
                        StepId = "RBAC_SOD_CHECK",
                        StepName = "Segregation of Duties Check",
                        SequenceNumber = 3,
                        StepType = "SYSTEM",
                        IsRequired = true,
                        Description = "Automated: checks role combination against SoD rules. If conflict detected, routes to security review with flag.",
                        NextStepId = "RBAC_SECURITY_REVIEW",
                        StepConfiguration = new Dictionary<string, object>
                        {
                            ["autoValidate"] = true,
                            ["validationRules"] = new[] { "SOD_CONFLICT_CHECK" }
                        }
                    },
                    new()
                    {
                        StepId = "RBAC_SECURITY_REVIEW",
                        StepName = "Security Administrator Review",
                        SequenceNumber = 4,
                        StepType = "APPROVAL",
                        IsRequired = true,
                        RequiresApproval = true,
                        RequiredRoles = new List<string> { "Administrator", "Admin" },
                        SlaHours = 24,
                        Description = "Administrator verifies no SoD conflicts, validates field scope, confirms business need",
                        NextStepId = "RBAC_ACTIVATION",
                        StepConfiguration = new Dictionary<string, object>
                        {
                            ["approvalType"] = "SEQUENTIAL",
                            ["escalationHours"] = 48
                        }
                    },
                    new()
                    {
                        StepId = "RBAC_ACTIVATION",
                        StepName = "Activate Role Assignment",
                        SequenceNumber = 5,
                        StepType = "SYSTEM",
                        IsRequired = true,
                        Description = "System assigns the role, logs to audit trail, sends notification to user and manager",
                    }
                },
                Configuration = new Dictionary<string, object>
                {
                    ["category"] = "SECURITY",
                    ["auditRequired"] = true,
                    ["notificationTemplate"] = "ROLE_ASSIGNMENT_APPROVED"
                }
            };

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
        }

        /// <summary>
        /// Temporary Role Elevation Approval workflow — time-bound elevation for acting-manager / leave coverage.
        /// </summary>
        private async Task InitializeTempRoleElevationApprovalAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = "RBAC_TEMP_ROLE_ELEVATION",
                ProcessName = "Temporary Role Elevation Approval",
                ProcessType = "ADMINISTRATIVE",
                EntityType = "TEMP_ROLE_ELEVATION",
                Description = "Approval workflow for time-bound temporary role elevations (acting manager, leave coverage, emergency access). Auto-expires.",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new()
                    {
                        StepId = "ELEVATION_REQUEST",
                        StepName = "Request Temporary Elevation",
                        SequenceNumber = 1,
                        StepType = "ACTION",
                        IsRequired = true,
                        Description = "Requester specifies: user, elevated role, effective dates (max 90 days), business justification",
                        NextStepId = "ELEVATION_MANAGER_APPROVAL",
                    },
                    new()
                    {
                        StepId = "ELEVATION_MANAGER_APPROVAL",
                        StepName = "Manager Approval",
                        SequenceNumber = 2,
                        StepType = "APPROVAL",
                        IsRequired = true,
                        RequiresApproval = true,
                        RequiredRoles = new List<string> { "Manager", "Supervisor" },
                        SlaHours = 24,
                        Description = "Manager confirms the elevation is necessary and appropriate",
                        NextStepId = "ELEVATION_SECURITY_REVIEW",
                        StepConfiguration = new Dictionary<string, object>
                        {
                            ["approvalType"] = "ANY",
                            ["escalationHours"] = 48
                        }
                    },
                    new()
                    {
                        StepId = "ELEVATION_SECURITY_REVIEW",
                        StepName = "Security Review",
                        SequenceNumber = 3,
                        StepType = "APPROVAL",
                        IsRequired = true,
                        RequiresApproval = true,
                        RequiredRoles = new List<string> { "Administrator", "Admin" },
                        SlaHours = 24,
                        Description = "Administrator reviews: scope limitation, SOD impact, expiry date, compensating controls if needed",
                        NextStepId = "ELEVATION_ACTIVATION",
                    },
                    new()
                    {
                        StepId = "ELEVATION_ACTIVATION",
                        StepName = "Activate Elevation",
                        SequenceNumber = 4,
                        StepType = "SYSTEM",
                        IsRequired = true,
                        Description = "System activates the elevation, sets auto-expiry timer, logs to audit trail, notifies user and manager",
                    },
                    new()
                    {
                        StepId = "ELEVATION_EXPIRY",
                        StepName = "Auto-Expiry",
                        SequenceNumber = 5,
                        StepType = "SYSTEM",
                        IsRequired = true,
                        Description = "System automatically expires the elevation at EFFECTIVE_TO. User's token will no longer carry elevated permissions after next refresh.",
                    }
                },
                Configuration = new Dictionary<string, object>
                {
                    ["category"] = "SECURITY",
                    ["auditRequired"] = true,
                    ["maxDurationDays"] = 90,
                    ["autoExpiryEnabled"] = true
                }
            };

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
        }

        /// <summary>
        /// SOD_WAIVER — Segregation of Duties violation waiver workflow (Phase 4).
        /// Time-bound exception to an SoD rule. Requires independent approval, compensating control, auto-expires after 90 days.
        /// </summary>
        private async Task InitializeSodWaiverWorkflowAsync(string userId)
        {
            await CreateProcessDefinitionIfNotExistsAsync(new ProcessDefinition
            {
                ProcessId = "SOD_WAIVER",
                ProcessName = "SoD Violation Waiver & Compensating Control",
                ProcessType = "ADMINISTRATIVE",
                EntityType = "SOD_CONFLICT",
                Description = "Time-bound exception to SoD rule with compensating control. Independent approval required. Auto-expires 90 days.",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new() { StepId = "WAIVER_REQUEST", StepName = "Request SoD Waiver", SequenceNumber = 1, StepType = "DATA_ENTRY", IsRequired = true, NextStepId = "SOD_IMPACT_ASSESS", Description = "Manager requests waiver with business justification and proposed compensating control" },
                    new() { StepId = "SOD_IMPACT_ASSESS", StepName = "SoD Impact Assessment", SequenceNumber = 2, StepType = "SYSTEM", IsRequired = true, NextStepId = "SECURITY_REVIEW", Description = "System evaluates SoD rule violations, severity, and regulatory implications" },
                    new() { StepId = "SECURITY_REVIEW", StepName = "Security Admin Review", SequenceNumber = 3, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Administrator","Admin"}, SlaHours = 48, NextStepId = "INDEPENDENT_APPROVAL", Description = "Administrator reviews impact and validates compensating control adequacy" },
                    new() { StepId = "INDEPENDENT_APPROVAL", StepName = "Independent Approval", SequenceNumber = 4, StepType = "APPROVAL", IsRequired = true, RequiresApproval = true, RequiredRoles = new(){"Auditor","ComplianceOfficer"}, SlaHours = 72, NextStepId = "WAIVER_ACTIVATION", Description = "Independent reviewer approves (ensures approver ≠ requester)" },
                    new() { StepId = "WAIVER_ACTIVATION", StepName = "Activate Waiver", SequenceNumber = 5, StepType = "SYSTEM", IsRequired = true, NextStepId = "WAIVER_EXPIRY", Description = "System activates waiver, creates COMPENSATING_CONTROL with 90-day expiry" },
                    new() { StepId = "WAIVER_EXPIRY", StepName = "Auto-Expiry", SequenceNumber = 6, StepType = "SYSTEM", IsRequired = true, Description = "System auto-expires waiver at EFFECTIVE_TO. Renewal requires new request." },
                },
                Configuration = new() { ["category"] = "SECURITY", ["maxDurationDays"] = 90, ["regulation"] = "SOX 404, ISO 27001", ["autoExpiryEnabled"] = true }
            }, userId);
        }

        #endregion

        private async Task InitializeProspectToDiscoveryProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
                ProcessName = ExplorationReferenceCodes.ProcessNameProspectToDiscovery,
                ProcessType = ExplorationReferenceCodes.ProcessTypeExploration,
                EntityType = ExplorationReferenceCodes.EntityTypeProspect,
                Description = "Workflow for evaluating a Prospect and recording Discovery",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepProspectCreation,
                        StepName = "Prospect Readiness",
                        SequenceNumber = 1,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = ExplorationReferenceCodes.StepRiskAssessment
                    },
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepRiskAssessment,
                        StepName = "Risk Assessment",
                        SequenceNumber = 2,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = ExplorationReferenceCodes.StepVolumeEstimation
                    },
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepVolumeEstimation,
                        StepName = "Volume Estimation",
                        SequenceNumber = 3,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = ExplorationReferenceCodes.StepEconomicEvaluation
                    },
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepEconomicEvaluation,
                        StepName = "Economic Evaluation",
                        SequenceNumber = 4,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = ExplorationReferenceCodes.StepDrillingDecision
                    },
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepDrillingDecision,
                        StepName = "Drilling Decision",
                        SequenceNumber = 5,
                        StepType = "APPROVAL",
                        IsRequired = true,
                        RequiresApproval = true,
                        NextStepId = ExplorationReferenceCodes.StepDiscoveryRecording
                    },
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepDiscoveryRecording,
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

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
        }

        private async Task InitializeDiscoveryToDevelopmentProcessAsync(string userId)
        {
            var definition = new ProcessDefinition
            {
                ProcessId = ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment,
                ProcessName = ExplorationReferenceCodes.ProcessNameDiscoveryToDevelopment,
                ProcessType = ExplorationReferenceCodes.ProcessTypeExploration,
                EntityType = ExplorationReferenceCodes.EntityTypeDiscovery,
                Description = "Workflow for appraising Discovery and making Development decision",
                IsActive = true,
                Steps = new List<ProcessStepDefinition>
                {
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepDiscoveryRecording,
                        StepName = "Discovery Recording",
                        SequenceNumber = 1,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = ExplorationReferenceCodes.StepAppraisal
                    },
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepAppraisal,
                        StepName = "Appraisal",
                        SequenceNumber = 2,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = ExplorationReferenceCodes.StepReserveEstimation
                    },
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepReserveEstimation,
                        StepName = "Reserve Estimation",
                        SequenceNumber = 3,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = ExplorationReferenceCodes.StepDevelopmentEconomicAnalysis
                    },
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepDevelopmentEconomicAnalysis,
                        StepName = "Economic Analysis",
                        SequenceNumber = 4,
                        StepType = "ACTION",
                        IsRequired = true,
                        NextStepId = ExplorationReferenceCodes.StepDevelopmentApproval
                    },
                    new ProcessStepDefinition
                    {
                        StepId = ExplorationReferenceCodes.StepDevelopmentApproval,
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

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
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

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
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

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
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

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
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

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
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

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
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

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
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

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
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

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
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

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
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

            await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
        }

        #endregion
    }
}

