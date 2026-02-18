using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.LifeCycle.Services;
using Beep.OilandGas.Models.Data;
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
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Container;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Addin;
using Microsoft.OpenApi;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Data;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ProspectIdentification.Services;
using Beep.OilandGas.ProductionOperations.Services;
using Beep.OilandGas.LeaseAcquisition.Services;
using Beep.OilandGas.PlungerLift.Services;
using Beep.OilandGas.HydraulicPumps.Services;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ApiService.Services;

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
// ============================================
// REGISTER BEEP FRAMEWORK SERVICES
// ============================================
// Use the modern BeepService registration pattern for Web API
builder.Services.AddBeepForWeb(options =>
{
    options.DirectoryPath = builder.Configuration["Beep:ConfigPath"] ?? Path.Combine(AppContext.BaseDirectory, "Config");
    options.AppRepoName = builder.Configuration["Beep:ContainerName"] ?? "PPDM39ApiContainer";
    options.ConfigType = BeepConfigType.DataConnector;
    // ServiceLifetime is Scoped by default for Web
    options.EnableAutoMapping = true;
    options.EnableAssemblyLoading = false; // We'll load assemblies after app startup
    options.EnableConfigurationValidation = true;
});

// Log Beep configuration
Log.Information("Beep Service Configuration: {Summary}", BeepServiceRegistration.GetConfigurationSummary());

// Access DMEEditor and related services from BeepService via DI (Scoped)
builder.Services.AddScoped(sp => sp.GetRequiredService<IBeepService>().DMEEditor);
builder.Services.AddScoped(sp => sp.GetRequiredService<IBeepService>().Config_editor);
builder.Services.AddScoped(sp => sp.GetRequiredService<IBeepService>().lg);
builder.Services.AddScoped(sp => sp.GetRequiredService<IBeepService>().util);
builder.Services.AddScoped(sp => sp.GetRequiredService<IBeepService>().Erinfo);
builder.Services.AddScoped(sp => sp.GetRequiredService<IBeepService>().LLoader);

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
    
    // Optionally inject ProductionAccounting services
            var royaltyService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.IRoyaltyService>();
            var allocationService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.IAllocationService>();
            var amortizationService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.IAmortizationService>();
    
    return new Beep.OilandGas.LifeCycle.Services.Accounting.PPDMAccountingService(
        editor, commonColumnHandler, defaults, metadata, 
        connectionName: connectionName, logger: logger,
        royaltyService: royaltyService,
        allocationService: allocationService,
        amortizationService: amortizationService);
});

// Production Accounting Services
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IAllocationEngine>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.AllocationEngine(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.AllocationEngine>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IAllocationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var allocationEngine = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IAllocationEngine>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.AllocationService(
        editor, commonColumnHandler, defaults, metadata, allocationEngine,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.AllocationService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IRoyaltyService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var glService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IJournalEntryService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var accountingServices = sp.GetService<Beep.OilandGas.Accounting.Services.IAccountingServices>();
    return new Beep.OilandGas.ProductionAccounting.Services.RoyaltyService(
        editor, commonColumnHandler, defaults, metadata, glService,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.RoyaltyService>(),
        accountingServices);
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IJointInterestBillingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.JointInterestBillingService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.JointInterestBillingService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IImbalanceService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.ImbalanceService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.ImbalanceService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.ISuccessfulEffortsService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.SuccessfulEffortsService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.SuccessfulEffortsService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IFullCostService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.FullCostService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.FullCostService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IAmortizationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.AmortizationService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.AmortizationService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IRevenueService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var leaseEconomicInterestService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.ILeaseEconomicInterestService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.RevenueService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.RevenueService>(),
        leaseEconomicInterestService);
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IMeasurementService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.MeasurementService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.MeasurementService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IPricingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.PricingService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.PricingService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IInventoryService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.InventoryService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.InventoryService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IPeriodClosingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var accountingServices = sp.GetService<Beep.OilandGas.Accounting.Services.IAccountingServices>();
    var amortizationService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.IAmortizationService>();
    var fullCostService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.IFullCostService>();
    var reserveAccountingService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.IReserveAccountingService>();
    var impairmentTestingService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.IImpairmentTestingService>();
    var decommissioningService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.IDecommissioningService>();
    var functionalCurrencyService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.IFunctionalCurrencyService>();
    var leasingService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.ILeasingService>();
    var financialInstrumentsService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.IFinancialInstrumentsService>();
    var emissionsTradingService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.IEmissionsTradingService>();
    var reserveDisclosureService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.IReserveDisclosureService>();
    return new Beep.OilandGas.ProductionAccounting.Services.PeriodClosingService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.PeriodClosingService>(),
        accountingServices,
        amortizationService,
        fullCostService,
        reserveAccountingService,
        impairmentTestingService,
        decommissioningService,
        functionalCurrencyService,
        leasingService,
        financialInstrumentsService,
        emissionsTradingService,
        reserveDisclosureService);
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IAuthorizationWorkflowService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.AuthorizationWorkflowService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.AuthorizationWorkflowService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.ILeaseEconomicInterestService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.LeaseEconomicInterestService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.LeaseEconomicInterestService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IReserveAccountingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.ReserveAccountingService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.ReserveAccountingService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IReserveDisclosureService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.ReserveDisclosureService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.ReserveDisclosureService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IExplorationEvaluationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.ExplorationEvaluationService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.ExplorationEvaluationService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IBorrowingCostCapitalizationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.BorrowingCostCapitalizationService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.BorrowingCostCapitalizationService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IAssetSwapService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.AssetSwapService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.AssetSwapService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IProductionSharingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.ProductionSharingService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.ProductionSharingService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IDecommissioningService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.DecommissioningService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.DecommissioningService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IImpairmentTestingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.ImpairmentTestingService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.ImpairmentTestingService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IFunctionalCurrencyService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.FunctionalCurrencyService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.FunctionalCurrencyService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.ILeasingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.LeasingService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.LeasingService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IFinancialInstrumentsService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.FinancialInstrumentsService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.FinancialInstrumentsService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IEmissionsTradingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.ProductionAccounting.Services.EmissionsTradingService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.ProductionAccounting.Services.EmissionsTradingService>());
});

builder.Services.AddScoped<ProductionAccountingService>(sp =>
{
    var allocationService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IAllocationService>();
    var royaltyService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IRoyaltyService>();
    var jibService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IJointInterestBillingService>();
    var imbalanceService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IImbalanceService>();
    var seService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.ISuccessfulEffortsService>();
    var fcService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IFullCostService>();
    var amortizationService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IAmortizationService>();
    var glService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IJournalEntryService>();
    var revenueService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IRevenueService>();
    var measurementService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IMeasurementService>();
    var pricingService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IPricingService>();
    var inventoryService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IInventoryService>();
    var periodClosingService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IPeriodClosingService>();
    var authorizationWorkflowService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IAuthorizationWorkflowService>();
    var leaseEconomicInterestService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.ILeaseEconomicInterestService>();
    var reserveAccountingService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IReserveAccountingService>();
    var productionSharingService = sp.GetService<Beep.OilandGas.Models.Core.Interfaces.IProductionSharingService>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var logger = sp.GetService<ILogger<ProductionAccountingService>>();
    var accountingServices = sp.GetService<Beep.OilandGas.Accounting.Services.IAccountingServices>();

    return new ProductionAccountingService(
        allocationService,
        royaltyService,
        jibService,
        imbalanceService,
        seService,
        fcService,
        amortizationService,
        glService,
        revenueService,
        measurementService,
        pricingService,
        inventoryService,
        periodClosingService,
        authorizationWorkflowService,
        leaseEconomicInterestService,
        reserveAccountingService,
        productionSharingService,
        metadata,
        editor,
        commonColumnHandler,
        defaults,
        logger,
        accountingServices);
});

// GL Integration Services
// Register IJournalEntryService
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IJournalEntryService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var glAccountService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.GLAccountService>();
    return new JournalEntryService(
        editor, commonColumnHandler, defaults, metadata, glAccountService,
        loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.JournalEntryService>());
});

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
            var journalEntryService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IJournalEntryService>();
    var accountMapping = sp.GetRequiredService<GLAccountMappingService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<GLIntegrationService>();
    return new GLIntegrationService(journalEntryService, accountMapping, logger);
});

// Work Order Accounting Service
builder.Services.AddScoped<Beep.OilandGas.LifeCycle.Services.Accounting.WorkOrderAccountingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var glIntegrationService = sp.GetService<GLIntegrationService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.LifeCycle.Services.Accounting.WorkOrderAccountingService>();
    
    return new Beep.OilandGas.LifeCycle.Services.Accounting.WorkOrderAccountingService(
        editor, commonColumnHandler, defaults, metadata,
        glIntegrationService: glIntegrationService,
        connectionName: connectionName, logger: logger);
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
builder.Services.AddScoped<PPDM39SetupService>();

// Connection Service (for exposing IDMEEditor connections)
builder.Services.AddScoped<ConnectionService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var setupService = sp.GetRequiredService<PPDM39SetupService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<ConnectionService>();
    return new ConnectionService(editor, setupService, logger);
});

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
builder.Services.AddScoped<PPDM39DataService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<PPDM39DataService>();
    var progressTracking = sp.GetService<IProgressTrackingService>();
    return new PPDM39DataService(
        editor, commonColumnHandler, defaults, metadata, logger, loggerFactory, progressTracking);
});

// PPDM39 Workflow Service
builder.Services.AddScoped<PPDM39WorkflowService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<PPDM39WorkflowService>();
    var progressTracking = sp.GetService<IProgressTrackingService>();
    return new PPDM39WorkflowService(
        editor, commonColumnHandler, defaults, metadata, logger, progressTracking);
});

// Add HttpContextAccessor for accessing HttpContext in services
builder.Services.AddHttpContextAccessor();

// ============================================
// DEMO DATABASE SERVICES
// ============================================
// Configure DemoDatabase settings
builder.Services.Configure<Beep.OilandGas.Models.Data.DataManagement.DemoDatabaseConfig>(
    builder.Configuration.GetSection("DemoDatabase"));

// Demo Database Repository
builder.Services.AddScoped<DemoDatabaseRepository>(sp =>
{
    var config = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Beep.OilandGas.Models.Data.DataManagement.DemoDatabaseConfig>>().Value;
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<DemoDatabaseRepository>();
    return new DemoDatabaseRepository(config.StoragePath, logger);
});

// Demo Database Service
builder.Services.AddScoped<DemoDatabaseService>(sp =>
{
    var config = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Beep.OilandGas.Models.Data.DataManagement.DemoDatabaseConfig>>();
    var repository = sp.GetRequiredService<DemoDatabaseRepository>();
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var setupService = sp.GetRequiredService<PPDM39SetupService>();
    var referenceDataSeeder = sp.GetService<Beep.OilandGas.PPDM39.DataManagement.SeedData.PPDMReferenceDataSeeder>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<DemoDatabaseService>();
    return new DemoDatabaseService(
        config, repository, editor, commonColumnHandler, defaults, metadata, setupService, referenceDataSeeder, logger);
});

// Demo Database Cleanup Service (Background Service)
builder.Services.AddHostedService<DemoDatabaseCleanupService>();

// ============================================
// REGISTER OIL & GAS CALCULATION SERVICES
// ============================================
// Oil Properties Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IOilPropertiesService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.OilProperties.Services.OilPropertiesService>();
    return new Beep.OilandGas.OilProperties.Services.OilPropertiesService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Gas Properties Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IGasPropertiesService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.GasProperties.Services.GasPropertiesService>();
    return new Beep.OilandGas.GasProperties.Services.GasPropertiesService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Flash Calculation Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IFlashCalculationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.FlashCalculations.Services.FlashCalculationService>();
    return new Beep.OilandGas.FlashCalculations.Services.FlashCalculationService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Gas Lift Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IGasLiftService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.GasLift.Services.GasLiftService>();
    return new Beep.OilandGas.GasLift.Services.GasLiftService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Heat Map Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IHeatMapService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.HeatMap.Services.HeatMapService>();
    return new Beep.OilandGas.HeatMap.Services.HeatMapService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Nodal Analysis Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.INodalAnalysisService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.NodalAnalysis.Services.NodalAnalysisService>();
    return new Beep.OilandGas.NodalAnalysis.Services.NodalAnalysisService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Production Forecasting Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IProductionForecastingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.ProductionForecasting.Services.ProductionForecastingService>();
    return new Beep.OilandGas.ProductionForecasting.Services.ProductionForecastingService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Pipeline Analysis Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IPipelineAnalysisService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.PipelineAnalysis.Services.PipelineAnalysisService>();
    return new Beep.OilandGas.PipelineAnalysis.Services.PipelineAnalysisService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Production Operations Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.ProductionOperations.Services.ProductionOperationsService>();
    return new ProductionOperationsService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Prospect Identification Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IProspectIdentificationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.ProspectIdentification.Services.ProspectIdentificationService>();
    return new ProspectIdentificationService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Economic Analysis Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IEconomicAnalysisService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.EconomicAnalysis.Services.EconomicAnalysisService>();
    return new Beep.OilandGas.EconomicAnalysis.Services.EconomicAnalysisService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Enhanced Recovery Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.EnhancedRecovery.Services.EnhancedRecoveryService>();
    return new Beep.OilandGas.EnhancedRecovery.Services.EnhancedRecoveryService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Lease Acquisition Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.LeaseAcquisition.Services.LeaseAcquisitionService>();
    return new LeaseAcquisitionService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Drilling Operation Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IDrillingOperationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.DrillingAndConstruction.Services.DrillingOperationService>();
    return new Beep.OilandGas.DrillingAndConstruction.Services.DrillingOperationService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Hydraulic Pump Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IHydraulicPumpService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.HydraulicPumps.Services.HydraulicPumpService>();
    return new HydraulicPumpService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Plunger Lift Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IPlungerLiftService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.PlungerLift.Services.PlungerLiftService>();
    return new PlungerLiftService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Sucker Rod Pumping Service
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.ISuckerRodPumpingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Beep.OilandGas.SuckerRodPumping.Services.SuckerRodPumpingService>();
    return new Beep.OilandGas.SuckerRodPumping.Services.SuckerRodPumpingService(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// ============================================
// REGISTER ACCOUNTING SERVICES (17 Total)
// ============================================
// Foundation GL Services (9)
builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.GLAccountService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.GLAccountService(
        editor, commonColumnHandler, defaults, metadata, connectionName, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.GLAccountService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.JournalEntryService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var glAccountService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.GLAccountService>();
    return new Beep.OilandGas.Accounting.Services.JournalEntryService(
        editor, commonColumnHandler, defaults, metadata, glAccountService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.JournalEntryService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.TrialBalanceService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var glAccountService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.GLAccountService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.TrialBalanceService(
        editor, commonColumnHandler, defaults, metadata, glAccountService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.TrialBalanceService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.APInvoiceService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var journalEntryService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.JournalEntryService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.APInvoiceService(
        editor, commonColumnHandler, defaults, metadata, journalEntryService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.APInvoiceService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.APPaymentService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var journalEntryService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.JournalEntryService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.APPaymentService(
        editor, commonColumnHandler, defaults, metadata, journalEntryService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.APPaymentService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.PurchaseOrderService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.PurchaseOrderService(
        editor, commonColumnHandler, defaults, metadata, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.PurchaseOrderService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.InventoryService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var journalEntryService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.JournalEntryService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.InventoryService(
        editor, commonColumnHandler, defaults, metadata, journalEntryService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.InventoryService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.InventoryLcmService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var journalEntryService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.JournalEntryService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.InventoryLcmService(
        editor, commonColumnHandler, defaults, metadata,
        loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.InventoryLcmService>(),
        journalEntryService);
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.PeriodClosingService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var glAccountService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.GLAccountService>();
    var trialBalanceService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.TrialBalanceService>();
    var journalEntryService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.JournalEntryService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.PeriodClosingService(
        editor, commonColumnHandler, defaults, metadata, glAccountService, trialBalanceService, journalEntryService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.PeriodClosingService>());
});

// Advanced Reporting Services (8)
builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.FinancialStatementService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var glAccountService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.GLAccountService>();
    var trialBalanceService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.TrialBalanceService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.FinancialStatementService(
        editor, commonColumnHandler, defaults, metadata, glAccountService, trialBalanceService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.FinancialStatementService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.GeneralLedgerReportService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var glAccountService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.GLAccountService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.GeneralLedgerReportService(
        editor, commonColumnHandler, defaults, metadata, glAccountService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.GeneralLedgerReportService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.BankReconciliationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var glAccountService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.GLAccountService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.BankReconciliationService(
        editor, commonColumnHandler, defaults, metadata, glAccountService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.BankReconciliationService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.BudgetService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var glAccountService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.GLAccountService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.BudgetService(
        editor, commonColumnHandler, defaults, metadata, glAccountService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.BudgetService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.DashboardService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var glAccountService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.GLAccountService>();
    var trialBalanceService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.TrialBalanceService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.DashboardService(
        editor, commonColumnHandler, defaults, metadata, glAccountService, trialBalanceService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.DashboardService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.TaxCalculationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var glAccountService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.GLAccountService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.TaxCalculationService(
        editor, commonColumnHandler, defaults, metadata, glAccountService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.TaxCalculationService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.DepreciationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var glAccountService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.GLAccountService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.DepreciationService(
        editor, commonColumnHandler, defaults, metadata, glAccountService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.DepreciationService>());
});

builder.Services.AddScoped<Beep.OilandGas.Accounting.Services.CostAllocationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var glAccountService = sp.GetRequiredService<Beep.OilandGas.Accounting.Services.GLAccountService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.CostAllocationService(
        editor, commonColumnHandler, defaults, metadata, glAccountService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.CostAllocationService>());
});

// Accounting interface adapters (foundation services)
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.ICostAllocationService>(sp =>
    sp.GetRequiredService<CostAllocationService>());

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.ICostCenterService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.CostCenterService(
        editor, commonColumnHandler, defaults, metadata, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.CostCenterService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IARService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var journalEntryService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IJournalEntryService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.ARService(
        editor, commonColumnHandler, defaults, metadata, journalEntryService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.ARService>());
});

builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IInvoiceService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var journalEntryService = sp.GetRequiredService<Beep.OilandGas.Models.Core.Interfaces.IJournalEntryService>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new Beep.OilandGas.Accounting.Services.InvoiceService(
        editor, commonColumnHandler, defaults, metadata, journalEntryService, loggerFactory.CreateLogger<Beep.OilandGas.Accounting.Services.InvoiceService>());
});

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

// Add BeepForWeb middleware for connection cleanup
app.UseBeepForWeb();

app.Run();
