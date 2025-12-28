using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.LifeCycle.Services;
using Beep.OilandGas.Models.DTOs;
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
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Container;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Addin;
using Microsoft.OpenApi;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ProductionAccounting.Services;

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
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Beep Oil and Gas PPDM39 API",
        Version = "v1",
        Description = "API for PPDM39 data management and oil & gas operations"
    });
});

// ============================================
// ADD AUTHENTICATION WITH JWT BEARER
// ============================================
// Add Authentication with JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Use Aspire service discovery in development, or config-based URL in production
    // Aspire provides: services:identityserver:https:0 or services:identityserver:http:0
    var identityServerUrl = builder.Configuration["services:identityserver:https:0"] 
        ?? builder.Configuration["IdentityServer:Authority"] 
        ?? "https://localhost:7062/";
    
    options.Authority = identityServerUrl;
    options.Audience = builder.Configuration["IdentityServer:Audience"] ?? "beep-api";
    options.TokenValidationParameters.ValidateAudience = true;
    options.TokenValidationParameters.ValidAudience = builder.Configuration["IdentityServer:Audience"] ?? "beep-api";
    
    // In development, we might need to disable HTTPS requirement if running on HTTP
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    
    // For development with self-signed certs
    if (builder.Environment.IsDevelopment())
    {
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    }
    
    // Add JWT Bearer events for debugging
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("JwtBearer");
            
            var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization");
            var authHeader = context.Request.Headers["Authorization"].ToString();
            
            Log.Information("JWT: OnMessageReceived - Path: {Path}, HasAuthHeader: {HasAuth}, HeaderValue: {Header}", 
                context.Request.Path, 
                hasAuthHeader,
                hasAuthHeader ? (authHeader.Length > 50 ? authHeader.Substring(0, 50) + "..." : authHeader) : "(none)");
            
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("JwtBearer");
            
            Log.Error(context.Exception, "JWT: Authentication failed - {Message}", context.Exception.Message);
            
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("JwtBearer");
            
            var userId = context.Principal?.FindFirst("sub")?.Value;
            var claims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
            
            Log.Information("JWT: Token validated successfully - UserId: {UserId}, Claims: {Claims}", 
                userId, string.Join(", ", claims ?? new List<string>()));
            
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("JwtBearer");
            
            Log.Warning("JWT: Challenge triggered - Path: {Path}, Error: {Error}, ErrorDescription: {Desc}", 
                context.Request.Path,
                context.Error ?? "(none)",
                context.ErrorDescription ?? "(none)");
            
            return Task.CompletedTask;
        }
    };
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

// PPDM Mapping Service
builder.Services.AddScoped<PPDMMappingService>();

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
    return new Beep.OilandGas.LifeCycle.Services.WellComparisonService(editor, commonColumnHandler, defaults, metadata, connectionName);
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
builder.Services.AddScoped<IPPDMProductionService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var mappingService = sp.GetRequiredService<PPDMMappingService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService>();
    return new Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService(
        editor, commonColumnHandler, defaults, metadata, mappingService, connectionName, logger);
});

// LifeCycle Process Services
builder.Services.AddScoped<Beep.OilandGas.LifeCycle.Services.Processes.IProcessService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.LifeCycle.Services.Processes.ProcessServiceBase>();
    return new Beep.OilandGas.LifeCycle.Services.Processes.PPDMProcessService(
        editor, commonColumnHandler, defaults, metadata, connectionName, connectionName, logger);
});

// Exploration Process Service
builder.Services.AddScoped<Beep.OilandGas.LifeCycle.Services.Exploration.Processes.ExplorationProcessService>(sp =>
{
    var processService = sp.GetRequiredService<Beep.OilandGas.LifeCycle.Services.Processes.IProcessService>();
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var mappingService = sp.GetRequiredService<PPDMMappingService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.LifeCycle.Services.Exploration.Processes.ExplorationProcessService>();
    var explorationService = new Beep.OilandGas.LifeCycle.Services.Exploration.PPDMExplorationService(
        editor, commonColumnHandler, defaults, metadata, mappingService, connectionName, logger);
    return new Beep.OilandGas.LifeCycle.Services.Exploration.Processes.ExplorationProcessService(
        processService, explorationService, logger);
});

// Development Process Service
builder.Services.AddScoped<Beep.OilandGas.LifeCycle.Services.Development.Processes.DevelopmentProcessService>(sp =>
{
    var processService = sp.GetRequiredService<Beep.OilandGas.LifeCycle.Services.Processes.IProcessService>();
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var mappingService = sp.GetRequiredService<PPDMMappingService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.LifeCycle.Services.Development.Processes.DevelopmentProcessService>();
    var developmentService = new Beep.OilandGas.LifeCycle.Services.Development.PPDMDevelopmentService(
        editor, commonColumnHandler, defaults, metadata, mappingService, connectionName, logger);
    return new Beep.OilandGas.LifeCycle.Services.Development.Processes.DevelopmentProcessService(
        processService, developmentService, logger);
});

// Production Process Service
builder.Services.AddScoped<Beep.OilandGas.LifeCycle.Services.Production.Processes.ProductionProcessService>(sp =>
{
    var processService = sp.GetRequiredService<Beep.OilandGas.LifeCycle.Services.Processes.IProcessService>();
    var productionService = sp.GetRequiredService<IPPDMProductionService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.LifeCycle.Services.Production.Processes.ProductionProcessService>();
    return new Beep.OilandGas.LifeCycle.Services.Production.Processes.ProductionProcessService(
        processService, productionService as Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService, logger);
});

// Decommissioning Process Service
builder.Services.AddScoped<Beep.OilandGas.LifeCycle.Services.Decommissioning.Processes.DecommissioningProcessService>(sp =>
{
    var processService = sp.GetRequiredService<Beep.OilandGas.LifeCycle.Services.Processes.IProcessService>();
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var mappingService = sp.GetRequiredService<PPDMMappingService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.LifeCycle.Services.Decommissioning.Processes.DecommissioningProcessService>();
    var decommissioningService = new Beep.OilandGas.LifeCycle.Services.Decommissioning.PPDMDecommissioningService(
        editor, commonColumnHandler, defaults, metadata, mappingService, connectionName, logger);
    return new Beep.OilandGas.LifeCycle.Services.Decommissioning.Processes.DecommissioningProcessService(
        processService, decommissioningService, logger);
});

// Field Orchestrator Service (scoped per request)
builder.Services.AddScoped<IFieldOrchestrator>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var mappingService = sp.GetRequiredService<PPDMMappingService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.LifeCycle.Services.FieldOrchestrator>();
    var accessControlService = sp.GetService<IAccessControlService>();
    var httpContextAccessor = sp.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
    return new Beep.OilandGas.LifeCycle.Services.FieldOrchestrator(
        editor, commonColumnHandler, defaults, metadata, mappingService, connectionName, logger, accessControlService, httpContextAccessor);
});

// Calculation Service
builder.Services.AddScoped<ICalculationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService>();
    return new Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Accounting Service
builder.Services.AddScoped<IAccountingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.LifeCycle.Services.Accounting.PPDMAccountingService>();
    return new Beep.OilandGas.LifeCycle.Services.Accounting.PPDMAccountingService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Production Accounting Service
builder.Services.AddScoped<ProductionAccountingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new ProductionAccountingService(
        editor, commonColumnHandler, defaults, metadata, loggerFactory, connectionName);
});

// GL Integration Services
builder.Services.AddScoped<GLAccountMappingService>(sp =>
{
    var glAccountManager = sp.GetRequiredService<ProductionAccountingService>()
        .TraditionalAccounting.GeneralLedger;
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<GLAccountMappingService>();
    return new GLAccountMappingService(glAccountManager, logger);
});

builder.Services.AddScoped<GLIntegrationService>(sp =>
{
    var journalEntryManager = sp.GetRequiredService<ProductionAccountingService>()
        .TraditionalAccounting.JournalEntry;
    var accountMapping = sp.GetRequiredService<GLAccountMappingService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<GLIntegrationService>();
    return new GLIntegrationService(journalEntryManager, accountMapping, logger);
});

// ============================================
// REGISTER ACCESS CONTROL SERVICES
// ============================================
builder.Services.AddScoped<IAccessControlService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var mappingService = sp.GetRequiredService<PPDMMappingService>();
    return new Beep.OilandGas.LifeCycle.Services.AccessControl.UserAssetAccessService(
        editor, commonColumnHandler, defaults, metadata, mappingService, connectionName);
});

builder.Services.AddScoped<IAssetHierarchyService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var mappingService = sp.GetRequiredService<PPDMMappingService>();
    var accessControlService = sp.GetRequiredService<IAccessControlService>();
    return new Beep.OilandGas.LifeCycle.Services.AccessControl.AssetHierarchyService(
        editor, commonColumnHandler, defaults, metadata, mappingService, accessControlService, connectionName);
});

builder.Services.AddScoped<IUserProfileService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var mappingService = sp.GetRequiredService<PPDMMappingService>();
    var accessControlService = sp.GetRequiredService<IAccessControlService>();
    return new Beep.OilandGas.LifeCycle.Services.AccessControl.UserProfileService(
        editor, commonColumnHandler, defaults, metadata, mappingService, accessControlService, connectionName);
});

// PPDM39 Setup Service
builder.Services.AddScoped<Beep.OilandGas.ApiService.Services.PPDM39SetupService>();

// PPDM Database Creator Service
builder.Services.AddScoped<Beep.OilandGas.PPDM39.DataManagement.Services.IPPDMDatabaseCreatorService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.PPDM39.DataManagement.Services.PPDMDatabaseCreatorService>();
    return new Beep.OilandGas.PPDM39.DataManagement.Services.PPDMDatabaseCreatorService(editor, logger);
});

// PPDM Reference Data Seeder
builder.Services.AddScoped<Beep.OilandGas.PPDM39.DataManagement.SeedData.PPDMReferenceDataSeeder>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new Beep.OilandGas.PPDM39.DataManagement.SeedData.PPDMReferenceDataSeeder(
        editor, commonColumnHandler, defaults, metadata, connectionName);
});

// LOV Management Service
builder.Services.AddScoped<Beep.OilandGas.PPDM39.DataManagement.Services.LOVManagementService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.PPDM39.DataManagement.Services.LOVManagementService>();
    return new Beep.OilandGas.PPDM39.DataManagement.Services.LOVManagementService(
        editor, commonColumnHandler, defaults, metadata, logger);
});

// CSV LOV Importer
builder.Services.AddScoped<Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.CSVLOVImporter>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.CSVLOVImporter>();
    return new Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.CSVLOVImporter(
        editor, commonColumnHandler, defaults, metadata, logger);
});

// PPDM Standard Value Importer
builder.Services.AddScoped<Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.PPDMStandardValueImporter>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.PPDMStandardValueImporter>();
    return new Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.PPDMStandardValueImporter(
        editor, commonColumnHandler, defaults, metadata, logger);
});

// IHS Standard Value Importer
builder.Services.AddScoped<Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.IHSStandardValueImporter>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.IHSStandardValueImporter>();
    return new Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.IHSStandardValueImporter(
        editor, commonColumnHandler, defaults, metadata, logger);
});

// Standard Value Mapper
builder.Services.AddScoped<Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.StandardValueMapper>(sp =>
{
    var lovManagementService = sp.GetRequiredService<Beep.OilandGas.PPDM39.DataManagement.Services.LOVManagementService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.StandardValueMapper>();
    return new Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.StandardValueMapper(lovManagementService, logger);
});

// PPDM39 Data Service
builder.Services.AddScoped<Beep.OilandGas.ApiService.Services.PPDM39DataService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.ApiService.Services.PPDM39DataService>();
    var progressTracking = sp.GetService<IProgressTrackingService>();
    return new Beep.OilandGas.ApiService.Services.PPDM39DataService(
        editor, commonColumnHandler, defaults, metadata, logger, loggerFactory, progressTracking);
});

// PPDM39 Workflow Service
builder.Services.AddScoped<Beep.OilandGas.ApiService.Services.PPDM39WorkflowService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.ApiService.Services.PPDM39WorkflowService>();
    var progressTracking = sp.GetService<IProgressTrackingService>();
    return new Beep.OilandGas.ApiService.Services.PPDM39WorkflowService(
        editor, commonColumnHandler, defaults, metadata, logger, progressTracking);
});

// Add HttpContextAccessor for accessing HttpContext in services
builder.Services.AddHttpContextAccessor();

// SignalR for progress tracking
builder.Services.AddSignalR();

// Progress tracking service
builder.Services.AddSingleton<IProgressTrackingService, ProgressTrackingService>();

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

// Authentication must come before Authorization
app.UseAuthentication();

// Add Asset Access Middleware (after authentication, before authorization)
app.UseMiddleware<Beep.OilandGas.ApiService.Middleware.AssetAccessMiddleware>();

app.UseAuthorization();

app.MapControllers();

// SignalR hub for progress tracking
app.MapHub<ProgressHub>("/progressHub");

// Add authentication diagnostic endpoint
app.MapGet("/api/auth-test", (HttpContext context) =>
{
    var user = context.User;
    var authHeader = context.Request.Headers.Authorization.ToString();
    
    return Results.Ok(new { 
        IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
        AuthenticationType = user.Identity?.AuthenticationType,
        UserName = user.Identity?.Name,
        UserId = user.FindFirst("sub")?.Value ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
        Claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList(),
        HasAuthHeader = !string.IsNullOrEmpty(authHeader),
        AuthHeaderPrefix = authHeader.Length > 20 ? authHeader.Substring(0, 20) + "..." : authHeader,
        Timestamp = DateTime.UtcNow
    });
})
.RequireAuthorization() // This requires auth - will return 401 if no valid token
.WithName("AuthTest");

// Add anonymous auth check (doesn't require auth, just reports status)
app.MapGet("/api/auth-check", (HttpContext context) =>
{
    var user = context.User;
    var authHeader = context.Request.Headers.Authorization.ToString();
    
    return Results.Ok(new { 
        IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
        HasAuthHeader = !string.IsNullOrEmpty(authHeader),
        AuthHeaderLength = authHeader.Length,
        Message = user.Identity?.IsAuthenticated == true 
            ? $"Authenticated as {user.FindFirst("sub")?.Value}" 
            : "Not authenticated"
    });
})
.WithName("AuthCheck");

Log.Information("Beep Oil and Gas PPDM39 API started successfully");

app.Run();
