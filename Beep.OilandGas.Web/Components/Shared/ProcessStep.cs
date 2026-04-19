namespace Beep.OilandGas.Web.Components.Shared;

/// <summary>Used by ProcessTimeline to represent a single process step.</summary>
public record ProcessStep(
    string Name,
    string Date,
    bool IsCompleted,
    bool IsActive,
    string Icon = "");
