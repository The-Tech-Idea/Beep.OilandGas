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
    }
}
