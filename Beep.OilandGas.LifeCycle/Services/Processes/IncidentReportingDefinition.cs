using System.Collections.Generic;
using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>The canonical incident definition used by setup and native workflow qualification.</summary>
public static class IncidentReportingDefinition
{
    public static ProcessDefinition Create() => new ProcessDefinition
            {
                ProcessId    = "HSE_INCIDENT_REPORTING",
                ProcessName  = "IncidentReporting",
                ProcessType  = "HSE",
                EntityType   = "HSE_INCIDENT",
                Description  = "API RP 754 Tier 1-4 incident reporting — report, RCA, corrective actions, close-out.",
                IsActive     = true,
                Steps        = new List<ProcessStepDefinition>
                {
                    new() { StepId = "INC_REPORT",         StepName = "Incident Report",                SequenceNumber = 1, StepType = "ACTION",   IsRequired = true, RequiredRoles = new() { "HSEOfficer" },  NextStepId = "INC_CLASSIFICATION" },
                    new() { StepId = "INC_CLASSIFICATION", StepName = "Tier Classification (API RP 754)",SequenceNumber = 2, StepType = "ACTION",   IsRequired = true, RequiredRoles = new() { "HSEOfficer" },  NextStepId = "INC_NOTIFICATION" },
                    new() { StepId = "INC_NOTIFICATION",   StepName = "Management & Regulatory Notification",SequenceNumber = 3, StepType = "ACTION", IsRequired = true, RequiredRoles = new() { "HSEOfficer" }, NextStepId = "INC_INVESTIGATION" },
                    new() { StepId = "INC_INVESTIGATION",  StepName = "Investigation",                   SequenceNumber = 4, StepType = "ACTION",   IsRequired = true, RequiredRoles = new() { "HSEOfficer" },  NextStepId = "INC_RCA" },
                    new() { StepId = "INC_RCA",            StepName = "Root Cause Analysis",             SequenceNumber = 5, StepType = "ACTION",   IsRequired = true, RequiredRoles = new() { "HSEOfficer" },  NextStepId = "INC_CORRECTIVE_ACTIONS" },
                    new() { StepId = "INC_CORRECTIVE_ACTIONS", StepName = "Corrective Actions",          SequenceNumber = 6, StepType = "ACTION",   IsRequired = true, RequiredRoles = new() { "HSEOfficer" },  NextStepId = "INC_VERIFICATION" },
                    new() { StepId = "INC_VERIFICATION",   StepName = "Action Effectiveness Verification",SequenceNumber = 7, StepType = "APPROVAL", IsRequired = true, RequiredRoles = new() { "HSEOfficer" },  RequiresApproval = true, NextStepId = "INC_CLOSURE" },
                    new() { StepId = "INC_CLOSURE",        StepName = "Incident Closure",                SequenceNumber = 8, StepType = "APPROVAL", IsRequired = true, RequiredRoles = new() { "HSEOfficer" },  RequiresApproval = true, NextStepId = string.Empty }
                },
                Transitions   = new Dictionary<string, ProcessTransition>(),
                Configuration = new Dictionary<string, object>()
            };
}
