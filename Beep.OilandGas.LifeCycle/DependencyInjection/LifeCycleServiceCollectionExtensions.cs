using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.LifeCycle.DependencyInjection;

public static class LifeCycleServiceCollectionExtensions
{
    public static IServiceCollection AddLifeCycleServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionName = configuration.GetValue("BeepOg:DatabaseConnectionName", "PPDM39");

        services.AddScoped<IDoAEvaluationService>(sp =>
        {
            var editor = sp.GetRequiredService<IDMEEditor>();
            var cch = sp.GetRequiredService<ICommonColumnHandler>();
            var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
            var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<DoAEvaluationService>();
            return new DoAEvaluationService(editor, cch, defaults, metadata, connectionName, logger);
        });

        services.AddSingleton<IDynamicRoutingService>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<DynamicRoutingService>();
            return new DynamicRoutingService(logger);
        });

        services.AddScoped<IEscalationActionService>(sp =>
        {
            var editor = sp.GetRequiredService<IDMEEditor>();
            var cch = sp.GetRequiredService<ICommonColumnHandler>();
            var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
            var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<EscalationActionService>();
            return new EscalationActionService(editor, cch, defaults, metadata, connectionName, logger);
        });

        services.AddScoped<IWorkflowVersioningService>(sp =>
        {
            var editor = sp.GetRequiredService<IDMEEditor>();
            var cch = sp.GetRequiredService<ICommonColumnHandler>();
            var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
            var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<WorkflowVersioningService>();
            return new WorkflowVersioningService(editor, cch, defaults, metadata, connectionName, logger);
        });

        services.AddScoped<ICrossPersonaTaskRouter>(sp =>
        {
            var editor = sp.GetRequiredService<IDMEEditor>();
            var cch = sp.GetRequiredService<ICommonColumnHandler>();
            var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
            var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<CrossPersonaTaskRouter>();
            return new CrossPersonaTaskRouter(editor, cch, defaults, metadata, connectionName, logger);
        });

        services.AddScoped<IHandoffValidationService>(sp =>
        {
            var editor = sp.GetRequiredService<IDMEEditor>();
            var cch = sp.GetRequiredService<ICommonColumnHandler>();
            var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
            var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<HandoffValidationService>();
            return new HandoffValidationService(editor, cch, defaults, metadata, connectionName, logger);
        });

        services.AddScoped<IWorkflowDependencyGraphService>(sp =>
        {
            var editor = sp.GetRequiredService<IDMEEditor>();
            var cch = sp.GetRequiredService<ICommonColumnHandler>();
            var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
            var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<WorkflowDependencyGraphService>();
            return new WorkflowDependencyGraphService(editor, cch, defaults, metadata, connectionName, logger);
        });

        services.AddScoped<IBusinessEventTriggerService>(sp =>
        {
            var editor = sp.GetRequiredService<IDMEEditor>();
            var cch = sp.GetRequiredService<ICommonColumnHandler>();
            var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
            var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<BusinessEventTriggerService>();
            return new BusinessEventTriggerService(editor, cch, defaults, metadata, connectionName, logger, sp);
        });

        services.AddScoped<ISodEvaluationEngine>(sp =>
        {
            var editor = sp.GetRequiredService<IDMEEditor>();
            var cch = sp.GetRequiredService<ICommonColumnHandler>();
            var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
            var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<SodEvaluationEngine>();
            return new SodEvaluationEngine(editor, cch, defaults, metadata, connectionName, logger);
        });

        // SLA Monitor background service
        services.AddHostedService<SlaMonitorService>();

        // Phase 3: Multi-entity workflow chain orchestrator
        services.AddScoped<IMultiEntityWorkflowOrchestrator>(sp =>
        {
            var processService = sp.GetRequiredService<IProcessService>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<MultiEntityWorkflowOrchestrator>();
            return new MultiEntityWorkflowOrchestrator(processService, logger);
        });

        // Phase 3: Domain event publisher (wires events → triggers → workflows)
        services.AddScoped<IDomainEventPublisher>(sp =>
        {
            var triggerService = sp.GetRequiredService<IBusinessEventTriggerService>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<DomainEventPublisher>();
            return new DomainEventPublisher(triggerService, logger);
        });

        // Phase 4: Compliance reporting
        services.AddScoped<IComplianceReportService>(sp =>
        {
            var editor = sp.GetRequiredService<IDMEEditor>();
            var cch = sp.GetRequiredService<ICommonColumnHandler>();
            var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
            var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<ComplianceReportService>();
            return new ComplianceReportService(editor, cch, defaults, metadata, connectionName, logger);
        });

        // Phase 4: Report templates (SOX ITGC, SEC reserves)
        services.AddScoped<IReportTemplateService>(sp =>
        {
            var complianceService = sp.GetRequiredService<IComplianceReportService>();
            var auditChain = sp.GetService<Beep.OilandGas.UserManagement.Services.IAuditChainService>();
            return new ReportTemplateService(complianceService, auditChain);
        });

        return services;
    }
}
