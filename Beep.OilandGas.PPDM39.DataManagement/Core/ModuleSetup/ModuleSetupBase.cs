using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup
{
    /// <summary>
    /// Abstract base for <see cref="IModuleSetup"/> implementations.
    /// Provides factory helpers for <see cref="PPDMGenericRepository"/> and
    /// idiomatic per-row insert/skip utilities so modules stay concise.
    /// </summary>
    public abstract class ModuleSetupBase : IModuleSetup
    {
        protected readonly ModuleSetupContext _ctx;

        protected ModuleSetupBase(ModuleSetupContext context)
        {
            _ctx = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ── IModuleSetup contract ─────────────────────────────────────────────

        public abstract string ModuleId { get; }
        public abstract string ModuleName { get; }
        public abstract int Order { get; }
        public abstract IReadOnlyList<Type> EntityTypes { get; }

        public abstract Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default);

        // ── Protected helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="PPDMGenericRepository"/> for the given entity type
        /// and table name, wired with the context dependencies.
        /// </summary>
        protected PPDMGenericRepository CreateRepo(Type entityType, string tableName) =>
            new PPDMGenericRepository(
                _ctx.Editor,
                _ctx.CommonColumnHandler,
                _ctx.Defaults,
                _ctx.Metadata,
                entityType,
                _ctx.ConnectionName,
                tableName);

        /// <summary>
        /// Typed convenience wrapper that calls <see cref="CreateRepo"/>.
        /// </summary>
        protected PPDMGenericRepository GetRepo<T>(string tableName) =>
            CreateRepo(typeof(T), tableName);

        /// <summary>
        /// Returns <c>true</c> when at least one row matching <paramref name="idFilter"/>
        /// already exists in the table, i.e. this row can be skipped.
        /// Catches repository exceptions and returns <c>false</c> so callers attempt the insert.
        /// </summary>
        protected async Task<bool> SkipIfExistsAsync(
            PPDMGenericRepository repo,
            AppFilter idFilter)
        {
            try
            {
                var existing = await repo.GetAsync(new List<AppFilter> { idFilter });
                bool hasRows = false;
                foreach (var _ in existing) { hasRows = true; break; }
                return hasRows;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Inserts <paramref name="entity"/> via <paramref name="repo"/>.
        /// On success increments <c>result.RecordsInserted</c>.
        /// On failure appends a message to <c>result.Errors</c> — does NOT rethrow.
        /// </summary>
        protected async Task TryInsertAsync(
            PPDMGenericRepository repo,
            object entity,
            string userId,
            ModuleSetupResult result,
            string? rowDescription = null)
        {
            try
            {
                await repo.InsertAsync(entity, userId);
                result.RecordsInserted++;
            }
            catch (OperationCanceledException)
            {
                throw; // propagate cancellation
            }
            catch (Exception ex)
            {
                var label = rowDescription ?? entity.GetType().Name;
                result.Errors.Add($"[{repo.TableName}/{label}] {ex.Message}");
                _ctx.Logger.LogWarning(ex,
                    "Module {ModuleId}: insert failed for {Table}/{Row}",
                    ModuleId, repo.TableName, label);
            }
        }

        /// <summary>
        /// Convenience helper: checks existence and inserts when the row is absent.
        /// Returns <c>true</c> when insertion was attempted (whether it succeeded or not),
        /// <c>false</c> when the row already existed and was skipped.
        /// </summary>
        protected async Task<bool> UpsertIfMissingAsync(
            PPDMGenericRepository repo,
            object entity,
            AppFilter idFilter,
            string userId,
            ModuleSetupResult result,
            string? rowDescription = null)
        {
            if (await SkipIfExistsAsync(repo, idFilter))
                return false;

            await TryInsertAsync(repo, entity, userId, result, rowDescription);
            return true;
        }

        /// <summary>
        /// Builds a result shell pre-populated with the module identity.
        /// Call at the start of <see cref="SeedAsync"/> and return it at the end.
        /// </summary>
        protected ModuleSetupResult NewResult() => new ModuleSetupResult
        {
            ModuleId   = ModuleId,
            ModuleName = ModuleName
        };
    }
}
