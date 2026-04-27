using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.ProspectIdentification;

namespace Beep.OilandGas.ProspectIdentification.Modules
{
    /// <summary>
    /// Module order 50 — declares Exploration entity types for schema migration
    /// and owns exploration-specific setup orchestration for the ProspectIdentification project.
    /// Shared reference-data import infrastructure remains in PPDM39.DataManagement.
    ///
    /// SeedScope (maintenance map):
    /// - Tables: <c>R_LEAD_STATUS</c>, <c>R_PLAY_TYPE</c>, <c>R_EXPLORATION_REFERENCE_CODE</c>
    /// - Projections: seeded codes consumed by projection defaults and projection/status mapping
    ///   (e.g. <c>PROSPECT_STATUS</c>, <c>RISK_LEVEL</c>) via <c>R_EXPLORATION_REFERENCE_CODE</c>
    /// - Core: workflow/process constants and module/category tokens from
    ///   <c>ExplorationReferenceCodes</c> and <c>ExplorationReferenceCodes.ProcessEngine</c>,
    ///   persisted under <c>R_EXPLORATION_REFERENCE_CODE</c>
    /// </summary>
    public sealed class ExplorationModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            // ── Core prospect tables ──────────────────────────────────────────
            typeof(PROSPECT),
            typeof(PROSPECT_WORKFLOW_STAGE),
            // ── Exploration tables ───────────────────────────────────────────
            typeof(EXPLORATION_PERMIT),
            typeof(EXPLORATION_COSTS),
            typeof(EXPLORATION_BUDGET),
            typeof(EXPLORATION_PROGRAM),
            // ── Play and lead tables ─────────────────────────────────────────
            typeof(PLAY),
            typeof(LEAD),
            typeof(R_PLAY_TYPE),
            typeof(R_LEAD_STATUS),
            typeof(R_EXPLORATION_REFERENCE_CODE),
            // ── Prospect relationship tables ─────────────────────────────────
            typeof(PROSPECT_ANALOG),
            typeof(PROSPECT_BA),
            typeof(PROSPECT_DISCOVERY),
            typeof(PROSPECT_ECONOMIC),
            typeof(PROSPECT_FIELD),
            typeof(PROSPECT_HISTORY),
            typeof(PROSPECT_MIGRATION),
            typeof(PROSPECT_PLAY),
            typeof(PROSPECT_PORTFOLIO),
            typeof(PROSPECT_RANKING),
            typeof(PROSPECT_RESERVOIR),
            typeof(PROSPECT_RISK_ASSESSMENT),
            typeof(PROSPECT_SEIS_SURVEY),
            typeof(PROSPECT_SOURCE_ROCK),
            typeof(PROSPECT_TRAP),
            typeof(PROSPECT_VOLUME_ESTIMATE),
            typeof(PROSPECT_WELL),
        };

        private readonly PPDMReferenceDataSeeder _referenceSeeder;

        public ExplorationModule(ModuleSetupContext context, PPDMReferenceDataSeeder referenceSeeder)
            : base(context)
        {
            _referenceSeeder = referenceSeeder ?? throw new ArgumentNullException(nameof(referenceSeeder));
        }

        public override string ModuleId => ExplorationReferenceCodes.ExplorationModuleRegistryId;
        public override string ModuleName => "Exploration";
        public override int Order => 50;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override async Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var seed = await _referenceSeeder.SeedAnalysisReferenceDataAsync(
                    connectionName,
                    tableNames: null,
                    skipExisting: true,
                    userId: userId);

                result.Success = seed.Success;
                result.TablesSeeded = seed.TablesSeeded;
                result.RecordsInserted = seed.RecordsInserted;

                if (!seed.Success && !string.IsNullOrWhiteSpace(seed.Message))
                    result.Errors.Add(seed.Message);

                if (seed.Errors != null)
                    result.Errors.AddRange(seed.Errors);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                result.Success = false;
                result.Errors.Add(ex.Message);
            }

            try
            {
                await SeedRLeadStatusReferenceRowsAsync(connectionName, userId, result, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                result.Errors.Add($"R_LEAD_STATUS: {ex.Message}");
            }

            try
            {
                await SeedRPlayTypeReferenceRowsAsync(connectionName, userId, result, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                result.Errors.Add($"R_PLAY_TYPE: {ex.Message}");
            }

            try
            {
                await SeedExplorationReferenceCodeRowsAsync(connectionName, userId, result, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                result.Errors.Add($"R_EXPLORATION_REFERENCE_CODE: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Idempotent rows for <c>R_LEAD_STATUS</c> so <c>LEAD.LEAD_STATUS</c> updates (e.g. promotion to <c>PROSPECT</c>) satisfy reference integrity.
        /// </summary>
        private async Task SeedRLeadStatusReferenceRowsAsync(
            string connectionName,
            string userId,
            ModuleSetupResult result,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var repo = GetRepo<R_LEAD_STATUS>("R_LEAD_STATUS", connectionName);
            result.TablesSeeded++;

            foreach (var row in BuildDefaultLeadStatuses())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UpsertIfMissingAsync(
                    repo,
                    row,
                    new AppFilter
                    {
                        FieldName = "LEAD_STATUS",
                        Operator = "=",
                        FilterValue = row.LEAD_STATUS ?? string.Empty
                    },
                    userId,
                    result,
                    $"R_LEAD_STATUS/{row.LEAD_STATUS}").ConfigureAwait(false);
            }
        }

        private static IEnumerable<R_LEAD_STATUS> BuildDefaultLeadStatuses()
        {
            yield return new R_LEAD_STATUS
            {
                LEAD_STATUS = ExplorationReferenceCodes.LeadStatusActive,
                LONG_NAME = "Active lead",
                SHORT_NAME = "Active",
                ABBREVIATION = "ACT",
                ACTIVE_IND = "Y",
                SOURCE = "APPLICATION"
            };
            yield return new R_LEAD_STATUS
            {
                LEAD_STATUS = ExplorationReferenceCodes.LeadStatusPromotedToProspect,
                LONG_NAME = "Promoted to prospect",
                SHORT_NAME = "Prospect",
                ABBREVIATION = "PROSP",
                ACTIVE_IND = "Y",
                SOURCE = "APPLICATION"
            };
            yield return new R_LEAD_STATUS
            {
                LEAD_STATUS = ExplorationReferenceCodes.LeadStatusClosed,
                LONG_NAME = "Closed lead",
                SHORT_NAME = "Closed",
                ABBREVIATION = "CLS",
                ACTIVE_IND = "Y",
                SOURCE = "APPLICATION"
            };
        }

        /// <summary>
        /// Idempotent rows for <c>R_PLAY_TYPE</c> to keep <c>PLAY.PLAY_TYPE</c> values
        /// and exploration screening workflows aligned with a known catalog.
        /// </summary>
        private async Task SeedRPlayTypeReferenceRowsAsync(
            string connectionName,
            string userId,
            ModuleSetupResult result,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var repo = GetRepo<R_PLAY_TYPE>("R_PLAY_TYPE", connectionName);
            result.TablesSeeded++;

            foreach (var row in BuildDefaultPlayTypes())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UpsertIfMissingAsync(
                    repo,
                    row,
                    new AppFilter
                    {
                        FieldName = "PLAY_TYPE",
                        Operator = "=",
                        FilterValue = row.PLAY_TYPE ?? string.Empty
                    },
                    userId,
                    result,
                    $"R_PLAY_TYPE/{row.PLAY_TYPE}").ConfigureAwait(false);
            }
        }

        private static IEnumerable<R_PLAY_TYPE> BuildDefaultPlayTypes()
        {
            yield return PlayType("CONVENTIONAL", "Conventional play", "Conventional", "CNV");
            yield return PlayType("UNCONVENTIONAL", "Unconventional play", "Unconventional", "UNC");
            yield return PlayType("SHALE", "Shale play", "Shale", "SHL");
            yield return PlayType("TIGHT", "Tight play", "Tight", "TGT");
            yield return PlayType("DEEPWATER", "Deepwater play", "Deepwater", "DWT");
            yield return PlayType("OFFSHORE", "Offshore play", "Offshore", "OFS");
            yield return PlayType("ONSHORE", "Onshore play", "Onshore", "ONS");
            yield return PlayType("FRONTIER", "Frontier play", "Frontier", "FRT");
            yield return PlayType("MATURE", "Mature basin play", "Mature", "MTR");
        }

        private static R_PLAY_TYPE PlayType(
            string value,
            string longName,
            string shortName,
            string abbreviation) =>
            new()
            {
                PLAY_TYPE = value,
                LONG_NAME = longName,
                SHORT_NAME = shortName,
                ABBREVIATION = abbreviation,
                ACTIVE_IND = "Y",
                SOURCE = "APPLICATION"
            };

        private async Task SeedExplorationReferenceCodeRowsAsync(
            string connectionName,
            string userId,
            ModuleSetupResult result,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var repo = GetRepo<R_EXPLORATION_REFERENCE_CODE>("R_EXPLORATION_REFERENCE_CODE", connectionName);
            result.TablesSeeded++;

            foreach (var row in BuildDefaultExplorationReferenceCodes())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UpsertExplorationReferenceCodeIfMissingAsync(
                    repo,
                    row,
                    userId,
                    result)
                    .ConfigureAwait(false);
            }
        }

        private static IEnumerable<R_EXPLORATION_REFERENCE_CODE> BuildDefaultExplorationReferenceCodes()
        {
            const string source = "APPLICATION";

            yield return Code("PROCESS_NAME", ExplorationReferenceCodes.ProcessNameLeadToProspect, "Lead To Prospect", source);
            yield return Code("PROCESS_NAME", ExplorationReferenceCodes.ProcessNameProspectToDiscovery, "Prospect To Discovery", source);
            yield return Code("PROCESS_NAME", ExplorationReferenceCodes.ProcessNameDiscoveryToDevelopment, "Discovery To Development", source);
            yield return Code("PROCESS_NAME", ExplorationReferenceCodes.ProcessNameExplorationGateReview, "Exploration Gate Review", source);

            yield return Code("PROCESS_ID", ExplorationReferenceCodes.ProcessIdLeadToProspect, "Lead To Prospect", source);
            yield return Code("PROCESS_ID", ExplorationReferenceCodes.ProcessIdProspectToDiscovery, "Prospect To Discovery", source);
            yield return Code("PROCESS_ID", ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment, "Discovery To Development", source);
            yield return Code("PROCESS_ID", ExplorationReferenceCodes.ProcessIdGateExplorationReview, "Gate Exploration Review", source);

            yield return Code("PROCESS_TYPE", ExplorationReferenceCodes.ProcessTypeGateReview, "Gate Review", source);
            yield return Code("PROCESS_TYPE", ExplorationReferenceCodes.ProcessTypeExploration, "Exploration", source);

            yield return Code("ENTITY_TYPE", ExplorationReferenceCodes.EntityTypeLead, "Lead", source);
            yield return Code("ENTITY_TYPE", ExplorationReferenceCodes.EntityTypeProspect, "Prospect", source);
            yield return Code("ENTITY_TYPE", ExplorationReferenceCodes.EntityTypeDiscovery, "Discovery", source);
            yield return Code("ENTITY_TYPE", ExplorationReferenceCodes.EntityTypeExplorationGateReview, "Exploration Gate Anchor Entity", source);

            yield return Code("STEP_ID", ExplorationReferenceCodes.StepLeadCreation, "Lead Creation", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepLeadEvaluation, "Lead Evaluation", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepLeadApproval, "Lead Approval", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepProspectCreation, "Prospect Creation", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepProspectAssessment, "Prospect Assessment", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepRiskAssessment, "Risk Assessment", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepVolumeEstimation, "Volume Estimation", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepEconomicEvaluation, "Economic Evaluation", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepDrillingDecision, "Drilling Decision", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepDiscoveryRecording, "Discovery Recording", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepAppraisal, "Appraisal", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepReserveEstimation, "Reserve Estimation", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepDevelopmentEconomicAnalysis, "Development Economic Analysis", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepDevelopmentApproval, "Development Approval", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepGateExplorationPackage, "Gate Exploration Package", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepGateExplorationResources, "Gate Exploration Resources", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepGateExplorationEconomics, "Gate Exploration Economics", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepGateExplorationApproval, "Gate Exploration Approval", source);
            yield return Code("STEP_ID", ExplorationReferenceCodes.StepGateExplorationCommit, "Gate Exploration Commit", source);

            yield return Code("OUTCOME", ExplorationReferenceCodes.OutcomeSuccess, "Success", source);
            yield return Code("OUTCOME", ExplorationReferenceCodes.OutcomeApproved, "Approved", source);
            yield return Code("OUTCOME", ExplorationReferenceCodes.OutcomeRejected, "Rejected", source);

            yield return Code("LEAD_STATUS", ExplorationReferenceCodes.LeadStatusActive, "Active Lead", source);
            yield return Code("LEAD_STATUS", ExplorationReferenceCodes.LeadStatusPromotedToProspect, "Promoted To Prospect", source);
            yield return Code("LEAD_STATUS", ExplorationReferenceCodes.LeadStatusClosed, "Closed Lead", source);
            yield return Code("PROSPECT_STATUS", "NEW", "New Prospect", source);
            yield return Code("PROSPECT_STATUS", "IDENTIFIED", "Identified Prospect", source);
            yield return Code("PROSPECT_STATUS", "EVALUATED", "Evaluated Prospect", source);
            yield return Code("PROSPECT_STATUS", "APPROVED", "Approved Prospect", source);
            yield return Code("PROSPECT_STATUS", "REJECTED", "Rejected Prospect", source);
            yield return Code("RISK_LEVEL", "LOW", "Low Risk", source);
            yield return Code("RISK_LEVEL", "MEDIUM", "Medium Risk", source);
            yield return Code("RISK_LEVEL", "HIGH", "High Risk", source);

            yield return Code("CATEGORY_TOKEN", ExplorationReferenceCodes.ExplorationCategoryToken, "Exploration Category Token", source);
            yield return Code("MODULE_ID", ExplorationReferenceCodes.ExplorationModuleRegistryId, "Exploration Module Registry Id", source);
            yield return Code("WELL_TYPE", ExplorationReferenceCodes.PpdmWellTypeExploration, "Exploration Well Type", source);
            yield return Code("FIELD_LIFECYCLE_PHASE", ExplorationReferenceCodes.FieldLifecyclePhaseExploration, "Field Lifecycle Exploration Phase", source);
        }

        private static R_EXPLORATION_REFERENCE_CODE Code(
            string set,
            string value,
            string name,
            string source) =>
            new()
            {
                REFERENCE_SET = set,
                REFERENCE_CODE = value,
                LONG_NAME = name,
                SHORT_NAME = name,
                ACTIVE_IND = "Y",
                SOURCE = source
            };

        private async Task UpsertExplorationReferenceCodeIfMissingAsync(
            PPDMGenericRepository repo,
            R_EXPLORATION_REFERENCE_CODE row,
            string userId,
            ModuleSetupResult result)
        {
            var existing = await repo.GetAsync(
                new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "REFERENCE_SET",
                        Operator = "=",
                        FilterValue = row.REFERENCE_SET ?? string.Empty
                    },
                    new AppFilter
                    {
                        FieldName = "REFERENCE_CODE",
                        Operator = "=",
                        FilterValue = row.REFERENCE_CODE ?? string.Empty
                    }
                });

            foreach (var _ in existing)
                return;

            await TryInsertAsync(
                repo,
                row,
                userId,
                result,
                $"R_EXPLORATION_REFERENCE_CODE/{row.REFERENCE_SET}/{row.REFERENCE_CODE}");
        }
    }
}
