using Beep.OilandGas.ProspectIdentification.Services;
using Beep.OilandGas.LeaseAcquisition.Services;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.DevelopmentPlanning.Services;
using Beep.OilandGas.DrillingAndConstruction.Services;
using Beep.OilandGas.ProductionOperations.Services;
using Beep.OilandGas.EnhancedRecovery.Services;
using Beep.OilandGas.Decommissioning.Services;
using Beep.OilandGas.API.Middleware;
using Beep.OilandGas.API.HealthChecks;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/beep-oilgas-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Beep Oil and Gas Field Lifecycle API",
        Version = "v1",
        Description = "API for managing all stages of oil and gas field lifecycle"
    });
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register IDMEEditor and dependencies (similar to ShellServiceProvider pattern)
builder.Services.AddSingleton<TheTechIdea.Beep.Logger.IDMLogger>(sp => new TheTechIdea.Beep.Logger.DMLogger());
builder.Services.AddSingleton<TheTechIdea.Beep.Logger.IErrorsInfo>(sp => new TheTechIdea.Beep.Logger.ErrorsInfo());
builder.Services.AddSingleton<TheTechIdea.Beep.ConfigUtil.IJsonLoader>(sp => new TheTechIdea.Beep.ConfigUtil.JsonLoader());
builder.Services.AddSingleton<TheTechIdea.Beep.ConfigUtil.IConfigEditor>(sp =>
{
    var logger = sp.GetRequiredService<TheTechIdea.Beep.Logger.IDMLogger>();
    var errorInfo = sp.GetRequiredService<TheTechIdea.Beep.Logger.IErrorsInfo>();
    var jsonLoader = sp.GetRequiredService<TheTechIdea.Beep.ConfigUtil.IJsonLoader>();
    return new TheTechIdea.Beep.ConfigUtil.ConfigEditor(logger, errorInfo, jsonLoader, "Config", null, TheTechIdea.Beep.ConfigUtil.BeepConfigType.Application);
});
builder.Services.AddSingleton<TheTechIdea.Beep.Tools.IAssemblyHandler>(sp =>
{
    var configEditor = sp.GetRequiredService<TheTechIdea.Beep.ConfigUtil.IConfigEditor>();
    var errorInfo = sp.GetRequiredService<TheTechIdea.Beep.Logger.IErrorsInfo>();
    var logger = sp.GetRequiredService<TheTechIdea.Beep.Logger.IDMLogger>();
    var util = sp.GetRequiredService<TheTechIdea.Beep.Utilities.IUtil>();
    return new TheTechIdea.Beep.Tools.AssemblyHandler(configEditor, errorInfo, logger, util);
});
builder.Services.AddSingleton<TheTechIdea.Beep.Utilities.IUtil>(sp =>
{
    var logger = sp.GetRequiredService<TheTechIdea.Beep.Logger.IDMLogger>();
    var errorInfo = sp.GetRequiredService<TheTechIdea.Beep.Logger.IErrorsInfo>();
    var configEditor = sp.GetRequiredService<TheTechIdea.Beep.ConfigUtil.IConfigEditor>();
    return new TheTechIdea.Beep.Utilities.Util(logger, errorInfo, configEditor);
});
builder.Services.AddSingleton<IDMEEditor>(sp =>
{
    var logger = sp.GetRequiredService<TheTechIdea.Beep.Logger.IDMLogger>();
    var util = sp.GetRequiredService<TheTechIdea.Beep.Utilities.IUtil>();
    var errorInfo = sp.GetRequiredService<TheTechIdea.Beep.Logger.IErrorsInfo>();
    var configEditor = sp.GetRequiredService<TheTechIdea.Beep.ConfigUtil.IConfigEditor>();
    var assemblyHandler = sp.GetRequiredService<TheTechIdea.Beep.Tools.IAssemblyHandler>();
    return new TheTechIdea.Beep.Editor.DMEEditor(logger, util, errorInfo, configEditor, assemblyHandler);
});

// Register business services (all using UnitOfWork directly, no repositories needed)
builder.Services.AddScoped<IProspectEvaluationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    return new ProspectEvaluationService(editor, "PPDM39");
});
builder.Services.AddScoped<ISeismicAnalysisService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    return new SeismicAnalysisService(editor, "PPDM39");
});
builder.Services.AddScoped<ILeaseManagementService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    return new LeaseManagementService(editor, "PPDM39");
});
builder.Services.AddScoped<IDevelopmentPlanService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    return new DevelopmentPlanService(editor, "PPDM39");
});
builder.Services.AddScoped<IDrillingOperationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    return new DrillingOperationService(editor, "PPDM39");
});
builder.Services.AddScoped<IProductionManagementService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    return new ProductionManagementService(editor, "PPDM39");
});
builder.Services.AddScoped<IEnhancedRecoveryService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    return new EnhancedRecoveryService(editor, "PPDM39");
});
builder.Services.AddScoped<IWellPluggingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    return new WellPluggingService(editor, "PPDM39");
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

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Beep Oil and Gas API v1");
    });
}

app.UseHttpsRedirection();

// Add global exception handler
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseCors();
app.UseAuthorization();

// Map health checks
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
