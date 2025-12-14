# Critical Assembly Loading Pattern for Beep Framework

## ?? IMPORTANT: Complete Assembly Registration

When loading assemblies in a Beep application (API, Blazor, WinForms, etc.), you **MUST** complete both steps:

### ? Correct Pattern (Complete)

```csharp
// Step 1: Load assemblies into memory
await beepService.LoadAssembliesAsync(progress);

// Step 2: Register assemblies with Config_editor (CRITICAL!)
beepService.Config_editor.LoadedAssemblies = beepService.LLoader.Assemblies
    .Select(c => c.DllLib)
    .ToList();
```

### ? Incorrect Pattern (Incomplete)

```csharp
// Only Step 1 - assemblies loaded but NOT registered
await beepService.LoadAssembliesAsync(progress);

// Missing Step 2 - Config_editor doesn't know about loaded assemblies!
```

## Why Both Steps Are Required

### Step 1: `LoadAssembliesAsync()`
- Loads DLLs into application memory
- Scans configured directories for assemblies
- Initializes assembly metadata
- **BUT** does not automatically register with the configuration system

### Step 2: `Config_editor.LoadedAssemblies = ...`
- Registers loaded assemblies with the configuration editor
- Enables type resolution across the framework
- Makes data source drivers discoverable
- Allows extension methods and plugins to be found
- Required for dynamic instantiation of types

## What Breaks Without Step 2

### Symptoms
- ? Data source drivers not found
- ? "No suitable driver for connection type" errors
- ? Extension methods not discovered
- ? Type resolution failures
- ? Plugin functionality unavailable
- ? Assembly count shows 0 in logs

### Example Error Messages
```
Error: No data source driver found for type 'SqlServer'
Error: Unable to resolve type 'MyCustomExtension'
Warning: 0 assemblies registered with configuration system
```

## Implementation Examples

### ASP.NET Core / API

```csharp
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var beepService = scope.ServiceProvider.GetRequiredService<IBeepService>();
    
    // Step 1: Load assemblies
    var progress = new Progress<PassedArgs>(args =>
    {
        Log.Debug("Loading: {Message}", args.Messege);
    });
    
    await beepService.LoadAssembliesAsync(progress);
    
    // Step 2: Register with Config_editor (CRITICAL!)
    beepService.Config_editor.LoadedAssemblies = beepService.LLoader.Assemblies
        .Select(c => c.DllLib)
        .ToList();
    
    // Log verification
    Log.Information("Loaded {Count} assemblies", 
        beepService.Config_editor.LoadedAssemblies.Count);
}
```

### Blazor Server

```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app, IBeepService beepService)
    {
        // Step 1: Load assemblies
        var progress = new Progress<PassedArgs>();
        beepService.LoadAssemblies(progress);
        
        // Step 2: Register with Config_editor (CRITICAL!)
        beepService.Config_editor.LoadedAssemblies = beepService.LLoader.Assemblies
            .Select(c => c.DllLib)
            .ToList();
    }
}
```

### WinForms

```csharp
public static void StartLoading(string[] namespacesToInclude)
{
    var progress = new Progress<PassedArgs>(percent =>
    {
        visManager.PasstoWaitForm(percent);
    });
    
    // Step 1: Load assemblies
    beepService.LoadAssemblies(progress);
    
    // Step 2: Register with Config_editor (CRITICAL!)
    beepService.Config_editor.LoadedAssemblies = beepService.LLoader.Assemblies
        .Select(c => c.DllLib)
        .ToList();
}
```

## Verification Steps

### 1. Check Assembly Count
```csharp
var count = beepService.Config_editor.LoadedAssemblies.Count;
Console.WriteLine($"Loaded assemblies: {count}");
// Should be > 0
```

### 2. Verify Assembly Names
```csharp
var names = beepService.Config_editor.LoadedAssemblies
    .Select(a => a.DllName)
    .ToList();
    
Console.WriteLine($"Assemblies: {string.Join(", ", names)}");
```

### 3. Check Data Source Drivers
```csharp
var drivers = beepService.Config_editor.DataDriversClasses.Count;
Console.WriteLine($"Available drivers: {drivers}");
// Should match expected driver count
```

### 4. Verify Specific Driver
```csharp
var hasDriver = beepService.Config_editor.DataDriversClasses
    .Any(d => d.className.Contains("SqlServer"));
    
Console.WriteLine($"SQL Server driver available: {hasDriver}");
```

## Troubleshooting

### Issue: Assembly count is 0 after loading

**Diagnosis:**
```csharp
// Check if Step 1 worked
Console.WriteLine($"LLoader assemblies: {beepService.LLoader.Assemblies?.Count ?? 0}");

// Check if Step 2 was done
Console.WriteLine($"Config_editor assemblies: {beepService.Config_editor.LoadedAssemblies?.Count ?? 0}");
```

**Solution:**
If `LLoader.Assemblies` > 0 but `Config_editor.LoadedAssemblies` = 0, you're missing Step 2!

```csharp
// Add the missing registration step
beepService.Config_editor.LoadedAssemblies = beepService.LLoader.Assemblies
    .Select(c => c.DllLib)
    .ToList();
```

### Issue: Some assemblies not appearing

**Diagnosis:**
```csharp
// Compare what was loaded vs what was registered
var loaded = beepService.LLoader.Assemblies.Select(a => a.DllLib.DllName);
var registered = beepService.Config_editor.LoadedAssemblies.Select(a => a.DllName);

var missing = loaded.Except(registered).ToList();
Console.WriteLine($"Missing: {string.Join(", ", missing)}");
```

**Solution:**
Re-run Step 2 to ensure all assemblies are registered.

## Best Practices

### ? DO
- Always perform both steps in sequence
- Log assembly counts after registration
- Verify critical drivers are available
- Add verification logging in production
- Check assembly count > 0 before proceeding

### ? DON'T
- Skip the registration step
- Assume assemblies are auto-registered
- Ignore assembly count warnings
- Deploy without verification
- Forget to check logs for confirmation

## Quick Reference Card

```csharp
// ALWAYS USE THIS COMPLETE PATTERN
// ================================

// 1. Load
await beepService.LoadAssembliesAsync(progress);

// 2. Register (CRITICAL - DON'T SKIP!)
beepService.Config_editor.LoadedAssemblies = 
    beepService.LLoader.Assemblies.Select(c => c.DllLib).ToList();

// 3. Verify
Log.Information("Loaded {Count} assemblies", 
    beepService.Config_editor.LoadedAssemblies.Count);
```

## Additional Resources

- See `BeepAppServices.cs` for WinForms reference implementation
- See `Program.cs` (ApiService) for ASP.NET Core implementation
- See `BeepService.cs` for the core loading implementation
- See `PROGRAM_CS_REVISION_SUMMARY.md` for detailed documentation
