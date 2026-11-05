using TheTechIdea.Data.OilGas;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.ModuleSetup;
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PermitsAndApplications.Constants;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PermitsAndApplications.Modules
{
    /// <summary>
    /// Module order 110 — owns permits and applications extension schema registration.
    /// PPDM39 foundation tables such as APPLIC_BA, APPLIC_DESC, APPLIC_REMARK,
    /// BA_PERMIT, FACILITY_LICENSE, WELL_PERMIT_TYPE, and APPLICATION_COMPONENT
    /// are installed by the required PPDM39 foundation module, not by this optional module.
    /// </summary>
    public sealed class PermitsModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            // ── Core permit extension table (owned by this module) ─────────────────────
            typeof(PERMIT_APPLICATION),
            typeof(PERMIT_APPLICATION_COMMAND),
            // ── Supporting permit extension tables ─────────────────────────────────────
            typeof(PERMIT_STATUS_HISTORY),
            typeof(DRILLING_PERMIT_APPLICATION),
            typeof(ENVIRONMENTAL_PERMIT_APPLICATION),
            typeof(INJECTION_PERMIT_APPLICATION),
            typeof(JURISDICTION_REQUIREMENTS),
            typeof(MIT_RESULT),
            typeof(REQUIRED_FORM),
            typeof(APPLICATION_ATTACHMENT),
            typeof(R_PERMITS_REFERENCE_CODE),
            // Note: All PPDM39 foundation tables (e.g., APPLICATION_COMPONENT, APPLIC_BA, etc.)
            // are registered by the PPDM39 foundation module, not here.
        };

        public PermitsModule(ModuleSetupContext context)
            : base(context) { }

        public override string ModuleId   => "PERMITS";
        public override string ModuleName => "Permits & Applications";
        public override int    Order      => 110;
        public static IReadOnlyList<Type> OwnedEntities => _entityTypes;
        public override IReadOnlyList<Type> EntityTypes => OwnedEntities;

        public override async Task<ModuleSetupResult> SeedAsync(
            string connectionName, string userId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
            if (string.IsNullOrWhiteSpace(userId) || userId.Length > 128 || userId.Any(char.IsControl))
                throw new ArgumentException("A current setup account is required.", nameof(userId));
            cancellationToken.ThrowIfCancellationRequested();
            var source = _ctx.Editor.GetDataSource(connectionName)
                ?? throw new InvalidOperationException("The permit setup datasource is unavailable.");
            if (_ctx.Editor.OpenDataSource(connectionName) != ConnectionState.Open)
                throw new InvalidOperationException("The permit setup datasource could not be opened.");
            PPDMDataSourceTransaction.RequireIdle(source);
            MigrationConnectionTarget.Validate(_ctx.Editor, source, connectionName);
            var target = MigrationConnectionTarget.Fingerprint(source.Dataconnection.ConnectionProp);
            void Guard(bool owned)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!ReferenceEquals(source, _ctx.Editor.GetDataSource(connectionName)))
                    throw new InvalidOperationException("The permit setup datasource changed.");
                MigrationConnectionTarget.Validate(_ctx.Editor, source, connectionName);
                if (MigrationConnectionTarget.Fingerprint(source.Dataconnection.ConnectionProp) != target)
                    throw new InvalidOperationException("The permit setup target changed. Review the current connection before retrying.");
                if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
                else PPDMDataSourceTransaction.RequireIdle(source);
            }
            var seeds = PermitsReferenceCodeSeed.GetAllSeedRows().ToArray();
            if (seeds.Length == 0 || seeds.Select(row => (row.ReferenceSet, row.ReferenceCode)).Distinct().Count() != seeds.Length ||
                seeds.Any(row => string.IsNullOrWhiteSpace(row.ReferenceSet) || row.ReferenceSet.Length > 128 || row.ReferenceSet.Any(char.IsControl) ||
                    string.IsNullOrWhiteSpace(row.ReferenceCode) || row.ReferenceCode.Length > 128 || row.ReferenceCode.Any(char.IsControl) ||
                    string.IsNullOrWhiteSpace(row.LongName) || row.ActiveInd is not ("Y" or "N")))
                throw new InvalidOperationException("The permit reference catalog has missing or duplicate definitions.");
            const string table = nameof(R_PERMITS_REFERENCE_CODE);
            var required = new[] { "REFERENCE_SET", "REFERENCE_CODE", "LONG_NAME", "ACTIVE_IND", "PPDM_GUID",
                "ROW_CREATED_BY", "ROW_CHANGED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_DATE" };
            var shape = source.GetEntityStructure(table, true);
            if (shape?.Fields is null || !shape.Fields.Where(field => field.IsKey).Select(field => field.FieldName).Order(StringComparer.Ordinal)
                .SequenceEqual(new[] { "REFERENCE_CODE", "REFERENCE_SET" }))
                throw new NativeSchemaCompatibilityException("Install the canonical permit reference composite key using Beep Migration.");
            foreach (var name in required)
            {
                var columns = shape.Fields.Where(field => field.FieldName == name).ToArray();
                var date = name is "ROW_CREATED_DATE" or "ROW_CHANGED_DATE";
                var width = name == "LONG_NAME" ? seeds.Max(row => row.LongName.Length) : name == "ACTIVE_IND" ? 1 : name == "PPDM_GUID" ? 36 : 128;
                if (columns.Length != 1 || columns[0].Fieldtype != (date ? typeof(DateTime) : typeof(string)).FullName ||
                    !date && Math.Max(columns[0].Size, columns[0].Size1) < width)
                    throw new NativeSchemaCompatibilityException("The native permit reference schema cannot preserve the catalog or audit. Review its Beep Migration.");
            }
            var repo = new PPDMGenericRepository(_ctx.Editor, _ctx.CommonColumnHandler, _ctx.Defaults, _ctx.Metadata,
                typeof(R_PERMITS_REFERENCE_CODE), connectionName, table, null, null, ["REFERENCE_SET", "REFERENCE_CODE"], required);
            async Task<Dictionary<(string, string), R_PERMITS_REFERENCE_CODE>> Read(bool owned)
            {
                Guard(owned);
                var rows = (await repo.GetAsync(new List<AppFilter>())).ToArray();
                var result = new Dictionary<(string, string), R_PERMITS_REFERENCE_CODE>();
                foreach (var item in rows)
                {
                    if (item is not R_PERMITS_REFERENCE_CODE row || !result.TryAdd((row.REFERENCE_SET, row.REFERENCE_CODE), row))
                        throw new InvalidOperationException("The native permit reference population is malformed or duplicated.");
                }
                Guard(owned);
                return result;
            }
            static void RequireCatalog(R_PERMITS_REFERENCE_CODE row, PermitsReferenceCodeSeed.SeedRow seed)
            {
                if (row.REFERENCE_SET != seed.ReferenceSet || row.REFERENCE_CODE != seed.ReferenceCode || row.LONG_NAME != seed.LongName || row.ACTIVE_IND != seed.ActiveInd ||
                    !Guid.TryParseExact(row.PPDM_GUID, "D", out var guid) || guid == Guid.Empty || string.IsNullOrWhiteSpace(row.ROW_CREATED_BY) || string.IsNullOrWhiteSpace(row.ROW_CHANGED_BY) ||
                    row.ROW_CREATED_BY.Length > 128 || row.ROW_CHANGED_BY.Length > 128 || row.ROW_CREATED_BY.Any(char.IsControl) || row.ROW_CHANGED_BY.Any(char.IsControl) ||
                    row.ROW_CREATED_DATE is not { Kind: DateTimeKind.Utc } created || row.ROW_CHANGED_DATE is not { Kind: DateTimeKind.Utc } changed || changed < created)
                    throw new InvalidOperationException("The stored permit reference catalog differs from the application definition or has incomplete audit. Review it before retrying setup.");
            }
            var inserted = new Dictionary<(string, string), R_PERMITS_REFERENCE_CODE>();
            await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
            {
                var existing = await Read(true);
                // Refuse drift before writing anything; preserve unrelated custom reference sets.
                foreach (var seed in seeds)
                    if (existing.TryGetValue((seed.ReferenceSet, seed.ReferenceCode), out var row)) RequireCatalog(row, seed);
                foreach (var seed in seeds)
                {
                    Guard(true);
                    if (existing.ContainsKey((seed.ReferenceSet, seed.ReferenceCode))) continue;
                    var row = new R_PERMITS_REFERENCE_CODE { REFERENCE_SET = seed.ReferenceSet, REFERENCE_CODE = seed.ReferenceCode,
                        LONG_NAME = seed.LongName, ACTIVE_IND = seed.ActiveInd, PPDM_GUID = Guid.NewGuid().ToString() };
                    var guid = row.PPDM_GUID;
                    _ctx.CommonColumnHandler.PrepareForInsert(row, userId);
                    RequireCatalog(row, seed);
                    if (row.PPDM_GUID != guid || row.ROW_CREATED_BY != userId || row.ROW_CHANGED_BY != userId)
                        throw new InvalidOperationException("The permit reference insertion changed its reviewed audit account or identity.");
                    await PPDMDataSourceTransaction.WithSourceAsync(source, () =>
                    {
                        Guard(true);
                        BeepOperationResult.EnsureSuccess(source.InsertEntity(table, row), "Seed permit reference", table);
                        Guard(true);
                        return Task.FromResult(true);
                    });
                    inserted.Add((seed.ReferenceSet, seed.ReferenceCode), row);
                }
                await Verify(true);
            });
            await Verify(false);
            var completed = NewResult();
            completed.Success = true;
            completed.RecordsInserted = inserted.Count;
            completed.TablesSeeded = inserted.Count == 0 ? 0 : 1;
            if (inserted.Count == 0) completed.SkipReason = "Reference catalog already seeded and verified.";
            return completed;

            async Task Verify(bool owned)
            {
                var saved = await Read(owned);
                foreach (var seed in seeds)
                {
                    var key = (seed.ReferenceSet, seed.ReferenceCode);
                    if (!saved.TryGetValue(key, out var row)) throw new InvalidOperationException("The permit reference catalog could not be read back completely.");
                    RequireCatalog(row, seed);
                    if (inserted.TryGetValue(key, out var expected) && (row.PPDM_GUID != expected.PPDM_GUID || row.ROW_CREATED_BY != expected.ROW_CREATED_BY ||
                        row.ROW_CHANGED_BY != expected.ROW_CHANGED_BY || row.ROW_CREATED_DATE != expected.ROW_CREATED_DATE || row.ROW_CHANGED_DATE != expected.ROW_CHANGED_DATE))
                        throw new InvalidOperationException("The persisted permit reference audit differs from its setup insertion.");
                }
            }
        }
    }
}
