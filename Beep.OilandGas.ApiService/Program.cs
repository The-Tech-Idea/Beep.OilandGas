using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Repositories;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using Serilog;
using Serilog.Events;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.Utils;
using TheTechIdea.Beep;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Container;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Addin;
using Microsoft.OpenApi;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/beep-oilgas-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Beep Oil and Gas PPDM39 API",
        Version = "v1",
        Description = "API for PPDM39 data management and oil & gas operations"
    });
});

// ============================================
// REGISTER BEEP FRAMEWORK SERVICES
// ============================================
// Use the modern BeepService registration pattern
var beepService = builder.Services.AddBeepServices(options =>
{
    options.DirectoryPath = builder.Configuration["Beep:ConfigPath"] ?? Path.Combine(AppContext.BaseDirectory, "Config");
    options.ContainerName = builder.Configuration["Beep:ContainerName"] ?? "PPDM39ApiContainer";
    options.ConfigType = BeepConfigType.DataConnector;
    options.ServiceLifetime = ServiceLifetime.Singleton;
    options.EnableAutoMapping = true;
    options.EnableAssemblyLoading = false; // We'll load assemblies after app startup
    options.EnableConfigurationValidation = true;
});

// Log Beep configuration
Log.Information("Beep Service Configuration: {Summary}", BeepServiceRegistration.GetConfigurationSummary());

// Access DMEEditor and related services from BeepService
builder.Services.AddSingleton(sp => beepService.DMEEditor);
builder.Services.AddSingleton(sp => beepService.Config_editor);
builder.Services.AddSingleton(sp => beepService.lg);
builder.Services.AddSingleton(sp => beepService.util);
builder.Services.AddSingleton(sp => beepService.Erinfo);
builder.Services.AddSingleton(sp => beepService.LLoader);

// ============================================
// REGISTER PPDM39 SERVICES
// ============================================
var connectionName = builder.Configuration["ConnectionStrings:PPDM39"] ?? "PPDM39";

// Common Column Handler
builder.Services.AddSingleton<ICommonColumnHandler, CommonColumnHandler>();

// Metadata Repository
builder.Services.AddSingleton<IPPDMMetadataRepository>(sp =>
{
    Log.Information("Initializing PPDM Metadata Repository");
    return PPDMMetadataRepository.FromGeneratedClass();
});

// Defaults Repository
builder.Services.AddScoped<IPPDM39DefaultsRepository>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    Log.Debug("Creating PPDM39 Defaults Repository for connection: {ConnectionName}", connectionName);
    return new PPDM39DefaultsRepository(editor, connectionName, metadata);
});

// Data Management Services
builder.Services.AddScoped<IPPDMDataValidationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new PPDMDataValidationService(editor, commonColumnHandler, defaults, metadata, connectionName);
});

builder.Services.AddScoped<IPPDMDataQualityService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new PPDMDataQualityService(editor, commonColumnHandler, defaults, metadata, connectionName);
});

builder.Services.AddScoped<IPPDMDataQualityDashboardService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var qualityService = sp.GetRequiredService<IPPDMDataQualityService>();
    return new PPDMDataQualityDashboardService(editor, commonColumnHandler, defaults, metadata, qualityService, connectionName);
});

builder.Services.AddScoped<IWellComparisonService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new WellComparisonService(editor, commonColumnHandler, defaults, metadata, connectionName);
});

builder.Services.AddScoped<IPPDMDataAccessAuditService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new PPDMDataAccessAuditService(editor, commonColumnHandler, defaults, metadata, connectionName);
});

builder.Services.AddScoped<IPPDMDataVersioningService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new PPDMDataVersioningService(editor, commonColumnHandler, defaults, metadata, connectionName);
});

// Well Repository
builder.Services.AddScoped<WellRepository>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new WellRepository(editor, commonColumnHandler, defaults, metadata, connectionName);
});

// PPDM39 Section Services
builder.Services.AddScoped<IPPDMStratigraphyService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new Beep.OilandGas.PPDM39.DataManagement.Services.Stratigraphy.PPDMStratigraphyService(
        editor, commonColumnHandler, defaults, metadata, connectionName);
});

builder.Services.AddScoped<IPPDMProductionService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new Beep.OilandGas.PPDM39.DataManagement.Services.Production.PPDMProductionService(
        editor, commonColumnHandler, defaults, metadata, connectionName);
});

builder.Services.AddScoped<IPPDMSeismicService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new Beep.OilandGas.PPDM39.DataManagement.Services.Seismic.PPDMSeismicService(
        editor, commonColumnHandler, defaults, metadata, connectionName);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============================================
// INITIALIZE BEEP FRAMEWORK
// ============================================
// Load assemblies asynchronously during startup
using (var scope = app.Services.CreateScope())
{
    var beepSvc = scope.ServiceProvider.GetRequiredService<IBeepService>();
    var logger = scope.ServiceProvider.GetRequiredService<IDMLogger>();
    
    Log.Information("Loading Beep assemblies and configurations...");
    
    try
    {
        // Load assemblies if needed
        if (builder.Configuration.GetValue<bool>("Beep:LoadAssemblies", true))
        {
            var progress = new Progress<PassedArgs>(args =>
            {
                Log.Debug("Assembly loading: {Message}", args.Messege);
            });
            
            await beepSvc.LoadAssembliesAsync(progress);
            
            // IMPORTANT: Register loaded assemblies with Config_editor
            // This ensures all loaded assemblies are tracked and available
            beepSvc.Config_editor.LoadedAssemblies = beepSvc.LLoader.Assemblies.Select(c => c.DllLib).ToList();
            
            Log.Information("Loaded {Count} assemblies", beepSvc.LLoader.Assemblies?.Count ?? 0);
            Log.Debug("Registered assemblies: {AssemblyNames}", 
                string.Join(", ", beepSvc.Config_editor.LoadedAssemblies.Select(a => a.FullName)));
        }
        
        // Validate Beep configuration
        if (BeepServiceRegistration.ValidateConfiguration())
        {
            Log.Information("Beep configuration validated successfully");
        }
        else
        {
            Log.Warning("Beep configuration validation failed - some features may not work correctly");
        }
        
        // Log DMEEditor status
        var editor = scope.ServiceProvider.GetRequiredService<IDMEEditor>();
        Log.Information("DMEEditor initialized with {DataSourceCount} data sources", 
            editor.ConfigEditor?.DataConnections?.Count ?? 0);
            
        // Log available data source types
        if (editor.ConfigEditor?.DataDriversClasses?.Count > 0)
        {
            Log.Information("Available data source drivers: {DriverCount}", 
                editor.ConfigEditor.DataDriversClasses.Count);
            Log.Debug("Driver types: {DriverTypes}", 
                string.Join(", ", editor.ConfigEditor.DataDriversClasses.Select(d => d.classHandler)));
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to initialize Beep framework");
        throw;
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Beep Oil and Gas PPDM39 API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

Log.Information("Beep Oil and Gas PPDM39 API started successfully");

app.Run();
