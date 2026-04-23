using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.Core.Interfaces
{
    /// <summary>
    /// Plugin contract that every domain module must implement to participate in
    /// schema creation and data seeding.
    ///
    /// Registration: add each implementation as <c>IModuleSetup</c> in DI.
    /// Discovery:    <c>ModuleSetupOrchestrator</c> resolves <c>IEnumerable&lt;IModuleSetup&gt;</c>.
    /// </summary>
    public interface IModuleSetup
    {
        /// <summary>Stable unique identifier used in logs and result reporting.</summary>
        string ModuleId { get; }

        /// <summary>Human-readable display name.</summary>
        string ModuleName { get; }

        /// <summary>
        /// Execution order — lower runs first.
        /// Core modules should use 0–40; domain modules 50+.
        /// FK dependencies must be declared at lower order than their dependents.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// PPDM entity types owned by this module.
        /// These are collected by the orchestrator and passed to the migration manager
        /// to build the schema DDL plan. Must not contain types already declared by
        /// a lower-order module.
        /// </summary>
        IReadOnlyList<Type> EntityTypes { get; }

        /// <summary>
        /// Seeds the reference / default data this module requires.
        /// Implementations must be idempotent (skip-if-exists for every row).
        /// Partial success is allowed: insert as many rows as possible and
        /// record failures in <see cref="ModuleSetupResult.Errors"/>.
        /// Only throw <see cref="ModuleSetupAbortException"/> for unrecoverable states.
        /// </summary>
        Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Supporting types
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Result returned by a single <see cref="IModuleSetup.SeedAsync"/> call.
    /// </summary>
    public sealed class ModuleSetupResult
    {
        /// <summary>Module identifier — copy from <see cref="IModuleSetup.ModuleId"/>.</summary>
        public string ModuleId { get; set; } = string.Empty;

        /// <summary>Module display name.</summary>
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// True when the module completed its full loop, even if some rows failed.
        /// False only when the module threw an unhandled exception and did not complete.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>Total records inserted by this module during this run.</summary>
        public int RecordsInserted { get; set; }

        /// <summary>Number of tables touched by this module during this run.</summary>
        public int TablesSeeded { get; set; }

        /// <summary>
        /// Per-row or per-table error messages.
        /// Non-empty does not imply <see cref="Success"/> is false —
        /// a module may succeed partially.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// When non-null the module was intentionally skipped (e.g. already seeded).
        /// </summary>
        public string? SkipReason { get; set; }
    }

    /// <summary>
    /// Aggregate result returned by <see cref="ModuleSetupOrchestrator"/>.
    /// </summary>
    public sealed class OrchestratorSetupResult
    {
        /// <summary>True when every module reported <see cref="ModuleSetupResult.Success"/>.</summary>
        public bool AllSucceeded { get; set; }

        /// <summary>Total modules that were attempted.</summary>
        public int ModulesRun { get; set; }

        /// <summary>Modules where <see cref="ModuleSetupResult.Success"/> is true.</summary>
        public int ModulesSucceeded { get; set; }

        /// <summary>Sum of <see cref="ModuleSetupResult.RecordsInserted"/> across all modules.</summary>
        public int TotalRecordsInserted { get; set; }

        /// <summary>Per-module results in execution order.</summary>
        public List<ModuleSetupResult> ModuleResults { get; set; } = new List<ModuleSetupResult>();
    }

    /// <summary>
    /// Carries the four standard DI dependencies shared by all module implementations,
    /// plus the active connection name and a logger.
    /// Pass one instance per DI scope instead of injecting each dependency separately.
    /// </summary>
    public sealed class ModuleSetupContext
    {
        public IDMEEditor Editor { get; init; } = null!;
        public ICommonColumnHandler CommonColumnHandler { get; init; } = null!;
        public IPPDM39DefaultsRepository Defaults { get; init; } = null!;
        public IPPDMMetadataRepository Metadata { get; init; } = null!;
        public string ConnectionName { get; init; } = "PPDM39";
        public ILogger Logger { get; init; } = null!;
    }
}
