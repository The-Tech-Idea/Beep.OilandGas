using Microsoft.AspNetCore.Components;
namespace Beep.OilandGas.Web.Components.Workflow;
// Explicitly excluded non-HSE branches. Fail if an excluded panel is accidentally exercised.
public sealed class DeliverySavedWorkQueue:ComponentBase{protected override void OnInitialized()=>throw new InvalidOperationException("Delivery qualification is outside this HSE fixture.");}
public sealed class MaintenanceRecordedWorkQueue:ComponentBase{protected override void OnInitialized()=>throw new InvalidOperationException("Maintenance qualification is outside this HSE fixture.");}
