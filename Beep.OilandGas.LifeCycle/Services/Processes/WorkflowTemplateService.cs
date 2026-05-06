using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Workflow template service for creating and managing reusable workflow definitions.
/// Supports versioning, inheritance, and parameterization.
/// </summary>
public class WorkflowTemplateService
{
    private readonly Dictionary<string, WorkflowTemplate> _templates = new();
    private readonly ILogger<WorkflowTemplateService>? _logger;

    public WorkflowTemplateService(ILogger<WorkflowTemplateService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a new workflow template.
    /// </summary>
    public WorkflowTemplate CreateTemplate(WorkflowTemplateDefinition definition)
    {
        var template = new WorkflowTemplate
        {
            TemplateId = Guid.NewGuid().ToString(),
            Name = definition.Name,
            Description = definition.Description,
            Category = definition.Category,
            Version = "1.0",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            Steps = definition.Steps.Select((s, i) => new WorkflowTemplateStep
            {
                StepId = Guid.NewGuid().ToString(),
                Name = s.Name,
                Description = s.Description,
                Sequence = i + 1,
                StepType = s.StepType,
                RequiredRoles = s.RequiredRoles,
                SlaHours = s.SlaHours,
                EscalationHours = s.EscalationHours,
                ApprovalRequired = s.ApprovalRequired,
                Approvers = s.Approvers,
                Parameters = s.Parameters
            }).ToList(),
            Transitions = definition.Transitions
        };

        _templates[template.TemplateId] = template;
        _logger?.LogInformation("Created workflow template {Name} v{Version} with {Steps} steps",
            template.Name, template.Version, template.Steps.Count);

        return template;
    }

    /// <summary>
    /// Creates a new version of an existing template.
    /// </summary>
    public WorkflowTemplate CreateNewVersion(string templateId, string changeDescription)
    {
        if (!_templates.TryGetValue(templateId, out var existing))
            throw new ArgumentException($"Template {templateId} not found.");

        var newVersion = IncrementVersion(existing.Version);
        var newTemplate = new WorkflowTemplate
        {
            TemplateId = Guid.NewGuid().ToString(),
            Name = existing.Name,
            Description = existing.Description,
            Category = existing.Category,
            Version = newVersion,
            IsActive = true,
            IsLatestVersion = true,
            ParentTemplateId = templateId,
            ChangeDescription = changeDescription,
            CreatedDate = DateTime.UtcNow,
            Steps = existing.Steps.Select(s => new WorkflowTemplateStep
            {
                StepId = Guid.NewGuid().ToString(),
                ParentStepId = s.StepId,
                Name = s.Name,
                Description = s.Description,
                Sequence = s.Sequence,
                StepType = s.StepType,
                RequiredRoles = new List<string>(s.RequiredRoles),
                SlaHours = s.SlaHours,
                EscalationHours = s.EscalationHours,
                ApprovalRequired = s.ApprovalRequired,
                Approvers = s.Approvers != null ? new List<string>(s.Approvers) : null,
                Parameters = s.Parameters != null ? new Dictionary<string, string>(s.Parameters) : null
            }).ToList(),
            Transitions = existing.Transitions != null ? new List<WorkflowTransition>(existing.Transitions) : null
        };

        // Mark old template as not latest
        existing.IsLatestVersion = false;
        _templates[newTemplate.TemplateId] = newTemplate;

        _logger?.LogInformation("Created new version {Version} of template {Name}", newVersion, existing.Name);
        return newTemplate;
    }

    /// <summary>
    /// Creates a specialized template that inherits from a base template.
    /// </summary>
    public WorkflowTemplate CreateSpecializedTemplate(
        string baseTemplateId,
        string name,
        string description,
        Action<WorkflowTemplate> customize)
    {
        if (!_templates.TryGetValue(baseTemplateId, out var baseTemplate))
            throw new ArgumentException($"Base template {baseTemplateId} not found.");

        var specialized = new WorkflowTemplate
        {
            TemplateId = Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            Category = baseTemplate.Category,
            Version = "1.0",
            IsActive = true,
            IsSpecialized = true,
            BaseTemplateId = baseTemplateId,
            CreatedDate = DateTime.UtcNow,
            Steps = baseTemplate.Steps.Select(s => new WorkflowTemplateStep
            {
                StepId = Guid.NewGuid().ToString(),
                ParentStepId = s.StepId,
                Name = s.Name,
                Description = s.Description,
                Sequence = s.Sequence,
                StepType = s.StepType,
                RequiredRoles = new List<string>(s.RequiredRoles),
                SlaHours = s.SlaHours,
                EscalationHours = s.EscalationHours,
                ApprovalRequired = s.ApprovalRequired,
                Approvers = s.Approvers != null ? new List<string>(s.Approvers) : null,
                Parameters = s.Parameters != null ? new Dictionary<string, string>(s.Parameters) : null
            }).ToList(),
            Transitions = baseTemplate.Transitions?.Select(t => new WorkflowTransition
            {
                TransitionId = Guid.NewGuid().ToString(),
                FromStepId = t.FromStepId,
                ToStepId = t.ToStepId,
                Condition = t.Condition,
                ConditionExpression = t.ConditionExpression
            }).ToList()
        };

        // Apply customizations
        customize(specialized);

        _templates[specialized.TemplateId] = specialized;
        _logger?.LogInformation("Created specialized template {Name} from base {BaseName}", name, baseTemplate.Name);

        return specialized;
    }

    /// <summary>
    /// Gets a template by ID.
    /// </summary>
    public WorkflowTemplate? GetTemplate(string templateId)
    {
        return _templates.TryGetValue(templateId, out var template) ? template : null;
    }

    /// <summary>
    /// Gets all templates, optionally filtered by category.
    /// </summary>
    public List<WorkflowTemplate> GetTemplates(string? category = null)
    {
        var query = _templates.Values.AsEnumerable();
        if (!string.IsNullOrEmpty(category))
            query = query.Where(t => t.Category == category);

        return query.OrderByDescending(t => t.CreatedDate).ToList();
    }

    /// <summary>
    /// Gets the latest version of a template by name.
    /// </summary>
    public WorkflowTemplate? GetLatestTemplateByName(string name)
    {
        return _templates.Values
            .Where(t => t.Name == name && t.IsActive && t.IsLatestVersion)
            .OrderByDescending(t => t.Version)
            .FirstOrDefault();
    }

    /// <summary>
    /// Deactivates a template.
    /// </summary>
    public bool DeactivateTemplate(string templateId)
    {
        if (_templates.TryGetValue(templateId, out var template))
        {
            template.IsActive = false;
            _logger?.LogInformation("Deactivated template {Name} v{Version}", template.Name, template.Version);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Instantiates a workflow instance from a template.
    /// </summary>
    public WorkflowInstance Instantiate(string templateId, string entityType, string entityId, string userId, Dictionary<string, string>? parameters = null)
    {
        if (!_templates.TryGetValue(templateId, out var template))
            throw new ArgumentException($"Template {templateId} not found.");

        var instance = new WorkflowInstance
        {
            InstanceId = Guid.NewGuid().ToString(),
            TemplateId = templateId,
            TemplateName = template.Name,
            TemplateVersion = template.Version,
            EntityType = entityType,
            EntityId = entityId,
            Status = "DRAFT",
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow,
            CurrentStepIndex = 0,
            Steps = template.Steps.Select(s => new WorkflowInstanceStep
            {
                InstanceStepId = Guid.NewGuid().ToString(),
                TemplateStepId = s.StepId,
                Name = s.Name,
                Sequence = s.Sequence,
                Status = "NOT_STARTED",
                RequiredRoles = new List<string>(s.RequiredRoles),
                SlaHours = s.SlaHours,
                ApprovalRequired = s.ApprovalRequired,
                Parameters = s.Parameters != null ? new Dictionary<string, string>(s.Parameters) : null
            }).ToList()
        };

        // Apply instance-level parameters
        if (parameters != null)
        {
            instance.Parameters = new Dictionary<string, string>(parameters);
        }

        _logger?.LogInformation("Instantiated workflow instance {InstanceId} from template {TemplateName} v{Version}",
            instance.InstanceId, template.Name, template.Version);

        return instance;
    }

    private static string IncrementVersion(string version)
    {
        var parts = version.Split('.');
        if (parts.Length == 2 && int.TryParse(parts[1], out var minor))
        {
            return $"{parts[0]}.{minor + 1}";
        }
        return $"{version}.1";
    }
}

/// <summary>
/// Definition for creating a new workflow template.
/// </summary>
public class WorkflowTemplateDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<WorkflowTemplateStepDefinition> Steps { get; set; } = new();
    public List<WorkflowTransition>? Transitions { get; set; }
}

/// <summary>
/// Step definition for a workflow template.
/// </summary>
public class WorkflowTemplateStepDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty;
    public List<string> RequiredRoles { get; set; } = new();
    public double? SlaHours { get; set; }
    public double? EscalationHours { get; set; }
    public bool ApprovalRequired { get; set; }
    public List<string>? Approvers { get; set; }
    public Dictionary<string, string>? Parameters { get; set; }
}

/// <summary>
/// A reusable workflow template.
/// </summary>
public class WorkflowTemplate
{
    public string TemplateId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsLatestVersion { get; set; } = true;
    public bool IsSpecialized { get; set; }
    public string? ParentTemplateId { get; set; }
    public string? BaseTemplateId { get; set; }
    public string? ChangeDescription { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<WorkflowTemplateStep> Steps { get; set; } = new();
    public List<WorkflowTransition>? Transitions { get; set; }
}

/// <summary>
/// A step within a workflow template.
/// </summary>
public class WorkflowTemplateStep
{
    public string StepId { get; set; } = string.Empty;
    public string? ParentStepId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string StepType { get; set; } = string.Empty;
    public List<string> RequiredRoles { get; set; } = new();
    public double? SlaHours { get; set; }
    public double? EscalationHours { get; set; }
    public bool ApprovalRequired { get; set; }
    public List<string>? Approvers { get; set; }
    public Dictionary<string, string>? Parameters { get; set; }
}

/// <summary>
/// A running workflow instance.
/// </summary>
public class WorkflowInstance
{
    public string InstanceId { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string TemplateVersion { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Status { get; set; } = "DRAFT";
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int CurrentStepIndex { get; set; }
    public List<WorkflowInstanceStep> Steps { get; set; } = new();
    public Dictionary<string, string>? Parameters { get; set; }
}

/// <summary>
/// A step within a workflow instance.
/// </summary>
public class WorkflowInstanceStep
{
    public string InstanceStepId { get; set; } = string.Empty;
    public string TemplateStepId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string Status { get; set; } = "NOT_STARTED";
    public List<string> RequiredRoles { get; set; } = new();
    public double? SlaHours { get; set; }
    public bool ApprovalRequired { get; set; }
    public Dictionary<string, string>? Parameters { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Outcome { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// A state transition within a workflow.
/// </summary>
public class WorkflowTransition
{
    public string TransitionId { get; set; } = string.Empty;
    public string FromStepId { get; set; } = string.Empty;
    public string ToStepId { get; set; } = string.Empty;
    public string Condition { get; set; } = "ALWAYS"; // ALWAYS, CONDITIONAL, APPROVAL_REQUIRED
    public string? ConditionExpression { get; set; }
    public List<string>? Actions { get; set; }
}
