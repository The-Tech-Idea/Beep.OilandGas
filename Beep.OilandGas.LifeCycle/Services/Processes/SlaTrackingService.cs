using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// SLA tracking service for process steps.
/// Manages SLA timers, breach detection, escalation, and reporting.
/// </summary>
public class SlaTrackingService
{
    private readonly ILogger<SlaTrackingService>? _logger;
    private readonly Dictionary<string, SlaConfiguration> _configurations = new();

    public SlaTrackingService(ILogger<SlaTrackingService>? logger = null)
    {
        _logger = logger;
        InitializeDefaultConfigurations();
    }

    /// <summary>
    /// Registers an SLA configuration for a step type.
    /// </summary>
    public void RegisterConfiguration(SlaConfiguration config)
    {
        _configurations[config.StepType] = config;
        _logger?.LogInformation("Registered SLA configuration for step type {StepType}: {Hours}h", config.StepType, config.TargetHours);
    }

    /// <summary>
    /// Creates an SLA timer for a process step.
    /// </summary>
    public SlaTimer CreateTimer(string stepInstanceId, string stepType, DateTime startTime)
    {
        var config = GetConfiguration(stepType);

        return new SlaTimer
        {
            StepInstanceId = stepInstanceId,
            StepType = stepType,
            StartTime = startTime,
            TargetHours = config.TargetHours,
            EscalationHours = config.EscalationHours,
            Status = "ACTIVE",
            RemainingHours = config.TargetHours
        };
    }

    /// <summary>
    /// Updates an SLA timer and checks for breaches.
    /// </summary>
    public SlaTimerStatus UpdateTimer(SlaTimer timer, DateTime currentTime)
    {
        if (timer.Status != "ACTIVE")
            return new SlaTimerStatus { Timer = timer, IsBreached = false, IsEscalated = false };

        var elapsed = (currentTime - timer.StartTime).TotalHours;
        timer.RemainingHours = Math.Max(0, timer.TargetHours - elapsed);

        var result = new SlaTimerStatus { Timer = timer };

        if (elapsed >= timer.TargetHours)
        {
            timer.Status = "BREACHED";
            result.IsBreached = true;
            _logger?.LogWarning("SLA breached for step {StepInstance}. Elapsed: {Elapsed}h, Target: {Target}h",
                timer.StepInstanceId, elapsed, timer.TargetHours);
        }
        else if (timer.EscalationHours.HasValue && elapsed >= timer.EscalationHours.Value)
        {
            timer.Status = "ESCALATED";
            result.IsEscalated = true;
            _logger?.LogWarning("SLA escalated for step {StepInstance}. Elapsed: {Elapsed}h, Escalation: {Escalation}h",
                timer.StepInstanceId, elapsed, timer.EscalationHours.Value);
        }

        return result;
    }

    /// <summary>
    /// Completes an SLA timer.
    /// </summary>
    public void CompleteTimer(SlaTimer timer, DateTime completionTime)
    {
        timer.Status = "COMPLETED";
        timer.CompletionTime = completionTime;
        timer.ActualHours = (completionTime - timer.StartTime).TotalHours;
        timer.RemainingHours = 0;

        _logger?.LogInformation("SLA completed for step {StepInstance}. Actual: {Actual}h, Target: {Target}h",
            timer.StepInstanceId, timer.ActualHours, timer.TargetHours);
    }

    /// <summary>
    /// Pauses an SLA timer (e.g., when step is on hold).
    /// </summary>
    public void PauseTimer(SlaTimer timer, DateTime pauseTime)
    {
        if (timer.Status == "ACTIVE")
        {
            timer.Status = "PAUSED";
            timer.PauseTime = pauseTime;
            timer.ElapsedBeforePause = (pauseTime - timer.StartTime).TotalHours;
        }
    }

    /// <summary>
    /// Resumes a paused SLA timer.
    /// </summary>
    public void ResumeTimer(SlaTimer timer, DateTime resumeTime)
    {
        if (timer.Status == "PAUSED")
        {
            timer.Status = "ACTIVE";
            var pauseDuration = (resumeTime - (timer.PauseTime ?? resumeTime)).TotalHours;
            timer.StartTime = timer.StartTime.AddHours(pauseDuration);
            timer.PauseTime = null;
        }
    }

    /// <summary>
    /// Gets SLA configuration for a step type.
    /// </summary>
    public SlaConfiguration GetConfiguration(string stepType)
    {
        if (_configurations.TryGetValue(stepType, out var config))
            return config;

        // Return default configuration
        return new SlaConfiguration
        {
            StepType = stepType,
            TargetHours = 168, // 7 days default
            EscalationHours = 72 // 3 days default
        };
    }

    /// <summary>
    /// Calculates SLA compliance statistics.
    /// </summary>
    public SlaComplianceReport GenerateComplianceReport(List<SlaTimer> timers)
    {
        var report = new SlaComplianceReport
        {
            TotalTimers = timers.Count,
            CompletedOnTime = timers.Count(t => t.Status == "COMPLETED" && t.ActualHours <= t.TargetHours),
            Breached = timers.Count(t => t.Status == "BREACHED"),
            Escalated = timers.Count(t => t.Status == "ESCALATED"),
            Active = timers.Count(t => t.Status == "ACTIVE"),
            Paused = timers.Count(t => t.Status == "PAUSED")
        };

        report.ComplianceRate = report.TotalTimers > 0
            ? (double)report.CompletedOnTime / report.TotalTimers * 100
            : 100;

        if (timers.Any(t => t.ActualHours.HasValue && t.ActualHours.Value > 0))
        {
            report.AverageActualHours = timers.Where(t => t.ActualHours.HasValue && t.ActualHours.Value > 0).Average(t => t.ActualHours!.Value);
            report.AverageTargetHours = timers.Average(t => t.TargetHours);
        }

        return report;
    }

    private void InitializeDefaultConfigurations()
    {
        _configurations["Geological Review"] = new SlaConfiguration { StepType = "Geological Review", TargetHours = 336, EscalationHours = 168 };
        _configurations["Prospect Screening"] = new SlaConfiguration { StepType = "Prospect Screening", TargetHours = 168, EscalationHours = 72 };
        _configurations["Well Planning"] = new SlaConfiguration { StepType = "Well Planning", TargetHours = 504, EscalationHours = 336 };
        _configurations["Regulatory Approval"] = new SlaConfiguration { StepType = "Regulatory Approval", TargetHours = 720, EscalationHours = 360 };
        _configurations["Construction"] = new SlaConfiguration { StepType = "Construction", TargetHours = 4320, EscalationHours = 2160 };
        _configurations["Incident Investigation"] = new SlaConfiguration { StepType = "Incident Investigation", TargetHours = 168, EscalationHours = 72 };
        _configurations["Work Order Execution"] = new SlaConfiguration { StepType = "Work Order Execution", TargetHours = 336, EscalationHours = 168 };
        _configurations["Inspection"] = new SlaConfiguration { StepType = "Inspection", TargetHours = 168, EscalationHours = 72 };
        _configurations["Permit Application"] = new SlaConfiguration { StepType = "Permit Application", TargetHours = 720, EscalationHours = 360 };
        _configurations["Maintenance"] = new SlaConfiguration { StepType = "Maintenance", TargetHours = 336, EscalationHours = 168 };
    }
}

/// <summary>
/// SLA configuration for a step type.
/// </summary>
public class SlaConfiguration
{
    public string StepType { get; set; } = string.Empty;
    public double TargetHours { get; set; }
    public double? EscalationHours { get; set; }
    public string? EscalationUserId { get; set; }
    public string? NotificationTemplate { get; set; }
}

/// <summary>
/// SLA timer instance for a process step.
/// </summary>
public class SlaTimer
{
    public string StepInstanceId { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public DateTime? PauseTime { get; set; }
    public double? ElapsedBeforePause { get; set; }
    public double TargetHours { get; set; }
    public double? EscalationHours { get; set; }
    public double RemainingHours { get; set; }
    public double? ActualHours { get; set; }
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, PAUSED, COMPLETED, BREACHED, ESCALATED
}

/// <summary>
/// Status update from an SLA timer check.
/// </summary>
public class SlaTimerStatus
{
    public SlaTimer Timer { get; set; } = new();
    public bool IsBreached { get; set; }
    public bool IsEscalated { get; set; }
}

/// <summary>
/// SLA compliance report.
/// </summary>
public class SlaComplianceReport
{
    public int TotalTimers { get; set; }
    public int CompletedOnTime { get; set; }
    public int Breached { get; set; }
    public int Escalated { get; set; }
    public int Active { get; set; }
    public int Paused { get; set; }
    public double ComplianceRate { get; set; }
    public double AverageActualHours { get; set; }
    public double AverageTargetHours { get; set; }
}
