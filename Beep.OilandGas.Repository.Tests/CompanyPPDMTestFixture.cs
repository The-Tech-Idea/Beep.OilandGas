using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Tests.Infrastructure;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TheTechIdea.Beep.Utilities;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

// Test-only evidence for the fixed Beep driver's public transaction/connection properties.
public interface IBeepTransactionEvidence { System.Data.IDbTransaction? ActiveTransaction { get; } }
public interface IBeepConnectionEvidence { System.Data.IDbConnection DbConn { get; } }

public sealed partial class CompanyPPDMRowAccessTests
{
    internal sealed class Context(DbContextOptions<Context> options) : RepositoryDbContext(options);
    internal sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        private readonly ServiceProvider identity;
        // One configured physical datasource for this fixture, preserved across fresh company requests.
        private ConnectionProperties recordedConnection = new() { ConnectionName = "selected-tenant", DatabaseType = DataSourceType.SqlServer, Host = "server", Database = "assets-tenant" };
        public DefaultHttpContext Request { get; } = new();
        public HttpContextAccessor Http { get; } = new();
        public RepositoryRequestAccount Account { get; }

        public Context Db { get; }
        public CompanyModuleConnection Gate { get; private set; } = null!;
        public CompanyPPDMRowAccessPolicy Policy { get; private set; } = null!;
        public PPDM39GenericDataService Service { get; private set; } = null!;
        public Mock<IDMEEditor> Editor { get; } = new();
        public Mock<IDataSource> Source { get; } = new();
        public Mock<ICommonColumnHandler> Columns { get; } = new();
        public Mock<IPPDM39DefaultsRepository> Defaults { get; } = new();
        public Mock<IPPDMMetadataRepository> Metadata { get; } = new();
        public List<object> Rows { get; } = [];
        public List<object> Writes { get; } = [];
        public List<List<AppFilter>> Queries { get; } = [];
        public List<(string Table, List<AppFilter> Filters)> EligibilityQueries { get; } = [];
        public List<(string Table, List<AppFilter> Filters)> LeaseQueries { get; } = [];
        public bool IgnoreLeaseFilters { get; set; }
        public Func<Task>? AfterLeaseRead { get; set; }
        public Action<object>? AfterLeaseWrite { get; set; }
        public bool PersistLeaseWrites { get; private set; }
        public bool PersistCorrectiveActionWrites { get; private set; }
        public List<string> LeaseTransactions { get; } = [];
        public bool LeaseBeginNoOp { get; set; }
        public System.Data.IsolationLevel TransactionIsolation { get; set; } = System.Data.IsolationLevel.Serializable;
        public bool LeaseBeginFailed { get; set; }
        public bool LeaseCommitFailed { get; set; }
        public bool LeaseCommitAfterFailure { get; set; }
        public bool LeaseRollbackFailed { get; set; }
        public int LeaseFailWriteNumber { get; set; }
        public System.Data.IDbTransaction? LeaseActiveTransaction { get; set; }
        public Mock<System.Data.IDbConnection> LeasePhysicalConnection { get; } = new();
        private readonly List<Action> leaseUndo = [];
        public List<string> Connections { get; } = [];
        public bool IgnoreFilters { get; set; }
        public bool ObligationProjectionAsDictionary { get; set; }
#if BEEP_SECURE_CONNECTIONS
        public bool IncidentProjectionEnabled { get; set; }
#endif
        public bool IgnoreEligibilityFilters { get; set; }
        public bool IgnoreInterestFilters { get; set; }
        public List<(string Table, List<AppFilter> Filters)> InterestQueries { get; } = [];
        public Func<Task>? AfterInterestRead { get; set; }
        public Func<Task>? AfterRead { get; set; }
        public Func<Task>? AfterEligibilityRead { get; set; }
        public PPDMFieldEligibilityConfiguration EligibilityConfiguration { get; } = new();
        public ICompanyFieldEligibility Eligibility => new PPDMCompanyFieldEligibility(Gate, Editor.Object, Columns.Object,
            Defaults.Object, Metadata.Object, Options.Create(EligibilityConfiguration));

        private Fixture(bool boundedPages = false)
        {
            if (boundedPages) Source.As<TheTechIdea.Beep.DataBase.IBoundedPagedDataSource>();
#if BEEP_SECURE_CONNECTIONS
            var projection = Source.As<TheTechIdea.Beep.DataBase.IOwnedEntityProjectionDataSource>();
            projection.SetupGet(value => value.SupportsOwnedEntityProjections).Returns(() => IncidentProjectionEnabled);
            projection.Setup(value => value.ReadEntityProjectionAsync(It.IsAny<string>(), It.IsAny<List<AppFilter>>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
                .Returns(async (string table, List<AppFilter> filters, IReadOnlyList<string> _, CancellationToken _) => {
                    var rows = (IEnumerable<object>)await Source.Object.GetEntityAsync(table, filters);
                    return new TheTechIdea.Beep.DataBase.EntityReadResult(new ErrorsInfo { Flag = Errors.Ok }, rows.Select(row =>
                        (object)row.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                            .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
                            .ToDictionary(property => property.Name, property => property.GetValue(row)!)));
                });
#endif
            Source.As<IBeepTransactionEvidence>().SetupGet(value => value.ActiveTransaction).Returns(() => LeaseActiveTransaction);
            LeasePhysicalConnection.SetupGet(value => value.State).Returns(System.Data.ConnectionState.Open);
            connection.Open(); Db = new(new DbContextOptionsBuilder<Context>().UseSqlite(connection).Options); Db.Database.EnsureCreated();
            Db.Users.Add(new OilGasUser { Id = "actor", UserName = "actor", IsActive = true });
            Db.Roles.Add(new IdentityRole { Id = "admin", Name = "Administrator", NormalizedName = "ADMINISTRATOR" });
            Db.UserRoles.Add(new IdentityUserRole<string> { UserId = "actor", RoleId = "admin" });
            Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = PermissionConstants.DataManagement.ImportData });
            Db.SaveChanges();
            var registration = new ServiceCollection(); registration.AddLogging(); registration.AddSingleton<RepositoryDbContext>(Db);
            registration.AddIdentityCore<OilGasUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<RepositoryDbContext>(); identity = registration.BuildServiceProvider();
            Request.RequestServices = identity; Request.Response.Body = new MemoryStream();
            var assignments = new RepositoryRoleAssignmentService(Db, identity.GetRequiredService<UserManager<OilGasUser>>(), identity.GetRequiredService<RoleManager<IdentityRole>>(), NullLogger<RepositoryRoleAssignmentService>.Instance);
            Account = new(Http, principal => principal.FindFirst("party_id")?.Value, new RepositoryAccessService(Db), assignments);
            Request.User = new(new ClaimsIdentity([new("party_id", "actor")], "test"));
            var tables = JsonSerializer.Deserialize<Dictionary<string, PPDMTableMetadata>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "PPDM39Metadata.json")))!;
            Metadata.Setup(value => value.GetTableMetadataAsync(It.IsAny<string>())).ReturnsAsync((string table) => tables.GetValueOrDefault(table)!);
            Metadata.Setup(value => value.GetPrimaryKeyColumnsAsync(It.IsAny<string>())).ReturnsAsync((string table) => tables[table].PrimaryKeyColumn.Split(',').Select(value => value.Trim()));
            Defaults.Setup(value => value.FormatIdForTable(It.IsAny<string>(), It.IsAny<object>())).Returns((string _, object id) => id.ToString()!);
            Editor.Setup(value => value.GetDataSource(It.IsAny<string>())).Returns((string name) => { Connections.Add(name); return Source.Object; });
            Source.Setup(value => value.GetEntityAsync(It.IsAny<string>(), It.IsAny<List<AppFilter>>())).Returns(async (string table, List<AppFilter> filters) =>
            {
                var evidence = table is "ENT_COMPONENT" or "ENTITLEMENT" or "R_ENT_TYPE" or "R_ENT_COMPONENT_TYPE" or "R_ACCESS_CONDITION" or "R_ENT_ACCESS_TYPE";
                var interest = table is "INTEREST_SET" or "INT_SET_COMPONENT" or "INT_SET_PARTNER" or "R_INT_SET_COMPONENT" or "R_INT_SET_TYPE" or "R_INT_SET_ROLE";
                var lease = table is "LAND_RIGHT" or "LAND_AGREEMENT" or "LAND_STATUS" or "LAND_RIGHT_FIELD" or "LAND_RIGHT_COMPONENT" or "R_LAND_COMPONENT_TYPE" or "R_LAND_AGREE_TYPE" or "R_LAND_RIGHT_STATUS";
                if (evidence) EligibilityQueries.Add((table, filters)); else if (interest) InterestQueries.Add((table, filters)); else if (lease) LeaseQueries.Add((table, filters)); else Queries.Add(filters);
                var result = Rows.Where(row => row.GetType().Name == table && ((evidence ? IgnoreEligibilityFilters : interest ? IgnoreInterestFilters : lease ? IgnoreLeaseFilters : IgnoreFilters) ||
                    filters.All(filter => {
                        var value = row.GetType().GetProperty(filter.FieldName)!.GetValue(row);
                        return value is decimal number ? number == decimal.Parse(filter.FilterValue, System.Globalization.CultureInfo.InvariantCulture) : Convert.ToString(value) == filter.FilterValue;
                    }))).ToList();
                if (PersistCorrectiveActionWrites || PersistLeaseWrites && lease && table.StartsWith("LAND_", StringComparison.Ordinal)) result = result.Select(CloneRecord).ToList();
                if (evidence) { if (AfterEligibilityRead != null) await AfterEligibilityRead(); }
                else if (interest) { if (AfterInterestRead != null) await AfterInterestRead(); }
                else if (lease) { if (AfterLeaseRead != null) await AfterLeaseRead(); }
                else if (AfterRead != null) await AfterRead();
                if (ObligationProjectionAsDictionary && table is "R_OBLIG_TYPE" or "R_OBLIG_COMPONENT_TYPE" or "OBLIGATION" or "OBLIGATION_COMPONENT")
                    return result.Select(row => (object)row.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
                        .Where(property => property.CanRead).ToDictionary(property => property.Name, property => property.GetValue(row))).ToArray();
                return (IEnumerable<object>)result;
            });
            Source.Setup(value => value.InsertEntity(It.IsAny<string>(), It.IsAny<object>())).Returns((string _, object row) => { Writes.Add(row); return new ErrorsInfo { Flag = Errors.Ok }; });
            Source.Setup(value => value.UpdateEntity(It.IsAny<string>(), It.IsAny<object>())).Returns((string _, object row) => { Writes.Add(row); return new ErrorsInfo { Flag = Errors.Ok }; });
        }

        public void EnableLeaseTransactions()
        {
            PersistLeaseWrites = true;
            Source.Setup(value => value.BeginTransaction(It.IsAny<TheTechIdea.Beep.Addin.PassedArgs>())).Returns(() => {
                LeaseTransactions.Add("begin");
                if (LeaseBeginFailed) return new ErrorsInfo { Flag = Errors.Failed };
                if (!LeaseBeginNoOp) {
                    var transaction = new Mock<System.Data.IDbTransaction>();
                    transaction.SetupGet(value => value.Connection).Returns(LeasePhysicalConnection.Object);
                    transaction.SetupGet(value => value.IsolationLevel).Returns(() => TransactionIsolation);
                    LeaseActiveTransaction = transaction.Object;
                }
                return new ErrorsInfo { Flag = Errors.Ok };
            });
            Source.Setup(value => value.Commit(It.IsAny<TheTechIdea.Beep.Addin.PassedArgs>())).Returns(() => {
                LeaseTransactions.Add("commit");
                if (LeaseCommitFailed && !LeaseCommitAfterFailure) return new ErrorsInfo { Flag = Errors.Failed };
                leaseUndo.Clear(); LeaseActiveTransaction = null;
                return new ErrorsInfo { Flag = LeaseCommitFailed ? Errors.Failed : Errors.Ok };
            });
            Source.Setup(value => value.EndTransaction(It.IsAny<TheTechIdea.Beep.Addin.PassedArgs>())).Returns(() => {
                LeaseTransactions.Add("rollback");
                if (LeaseRollbackFailed) return new ErrorsInfo { Flag = Errors.Failed };
                foreach (var undo in leaseUndo.AsEnumerable().Reverse()) undo();
                leaseUndo.Clear(); LeaseActiveTransaction = null; return new ErrorsInfo { Flag = Errors.Ok };
            });
            Source.Setup(value => value.InsertEntity(It.IsAny<string>(), It.IsAny<object>())).Returns((string table, object row) => PersistRecord(table, row, false));
            Source.Setup(value => value.UpdateEntity(It.IsAny<string>(), It.IsAny<object>())).Returns((string table, object row) => PersistRecord(table, row, true));
            Columns.Setup(value => value.PrepareForInsert(It.IsAny<IPPDMEntity>(), "actor")).Callback((IPPDMEntity row, string actor) => {
                row.GetType().GetProperty("ROW_CREATED_BY")!.SetValue(row, actor);
                row.GetType().GetProperty("ROW_CREATED_DATE")!.SetValue(row, DateTime.UtcNow);
            });
            Columns.Setup(value => value.PrepareForUpdate(It.IsAny<IPPDMEntity>(), "actor")).Callback((IPPDMEntity row, string actor) => {
                row.GetType().GetProperty("ROW_CHANGED_BY")!.SetValue(row, actor);
                row.GetType().GetProperty("ROW_CHANGED_DATE")!.SetValue(row, DateTime.UtcNow);
            });
        }
        public void EnableCorrectiveActionTransactions(bool cloneReads=true)
        {
            EnableLeaseTransactions(); // Same explicit provider transaction/connection and rollback ledger.
            PersistCorrectiveActionWrites = cloneReads;
            var handler = new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler();
            Columns.Setup(value => value.PrepareForInsert(It.IsAny<IPPDMEntity>(), "actor")).Callback((IPPDMEntity row, string actor) => handler.PrepareForInsert(row, actor));
            Columns.Setup(value => value.PrepareForUpdate(It.IsAny<IPPDMEntity>(), "actor")).Callback((IPPDMEntity row, string actor) => handler.PrepareForUpdate(row, actor));
            Source.Setup(value => value.UpdateEntity("PROJECT_STEP", It.IsAny<object>())).Returns((string table, object row) => PersistRecord(table, row, true));
            Source.Setup(value => value.InsertEntity("PROJECT_STATUS", It.IsAny<object>())).Returns((string table, object row) => PersistRecord(table, row, false));
            foreach(var table in new[]{"FACILITY_STATUS","EQUIPMENT_STATUS"})
            {
                Source.Setup(value=>value.UpdateEntity(table,It.IsAny<object>())).Returns((string name,object row)=>PersistRecord(name,row,true));
                Source.Setup(value=>value.InsertEntity(table,It.IsAny<object>())).Returns((string name,object row)=>PersistRecord(name,row,false));
            }
        }
        public void EnableProcessGraphWrites()
        {
            foreach (var table in new[] { "PROCESS_INSTANCE", "PROCESS_STEP_INSTANCE", "PROCESS_HISTORY", "CROSS_PERSONA_TASK" })
                Source.Setup(value => value.InsertEntity(table, It.IsAny<object>())).Returns((string name, object row) => PersistRecord(name, row, false));
        }
        public void EnableProcessGraphUpdates()
        {
            foreach (var table in new[] { "PROCESS_INSTANCE", "PROCESS_STEP_INSTANCE", "CROSS_PERSONA_TASK" })
                Source.Setup(value => value.UpdateEntity(table, It.IsAny<object>())).Returns((string name, object row) => PersistRecord(name, row, true));
        }
        private ErrorsInfo PersistRecord(string table, object row, bool update)
        {
            Writes.Add(row);
            if (LeaseFailWriteNumber == Writes.Count) return new ErrorsInfo { Flag = Errors.Failed };
            var keys = Metadata.Object.GetPrimaryKeyColumnsAsync(table).GetAwaiter().GetResult().ToArray();
            var matches = Rows.Where(candidate => candidate.GetType() == row.GetType() && keys.All(key => Equals(candidate.GetType().GetProperty(key)!.GetValue(candidate), row.GetType().GetProperty(key)!.GetValue(row)))).ToArray();
            if (matches.Length != (update ? 1 : 0)) return new ErrorsInfo { Flag = Errors.Failed };
            var stored = CloneRecord(row);
            if (update) {
                var before = matches[0]; var index = Rows.IndexOf(before); Rows[index] = stored;
                leaseUndo.Add(() => Rows[Rows.FindIndex(candidate => ReferenceEquals(candidate, stored))] = before);
            } else { Rows.Add(stored); leaseUndo.Add(() => Rows.RemoveAll(candidate => ReferenceEquals(candidate, stored))); }
            AfterLeaseWrite?.Invoke(stored);
            return new ErrorsInfo { Flag = Errors.Ok };
        }
        private static object CloneRecord(object row)
        {
            var clone = Activator.CreateInstance(row.GetType())!;
            var properties = row.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly).AsEnumerable();
            if (row is Beep.OilandGas.Models.Data.ModelEntityBase)
                properties = properties.Concat(typeof(Beep.OilandGas.Models.Data.ModelEntityBase).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly));
            foreach (var property in properties.Where(property => property.CanRead && property.CanWrite))
                property.SetValue(clone, property.GetValue(row));
            return clone;
        }
        public static async Task<Fixture> Create(ConnectionProperties? target=null, bool boundedPages=false) { var f = new Fixture(boundedPages); if(target is not null)f.recordedConnection=target; await f.Select("company"); return f; }
        internal RepositoryTenantContext CompanySelection {get;private set;} = null!;
        public async Task Select(string organization,string actor="actor")
        {
            Gate = await TestCompanyModules.CreateAsync(Db, actor, organization, connectionTarget: recordedConnection, selectionCaptured: value=>CompanySelection=value);
            ConfigureCompanyServices(organization);
        }
        internal async Task UseExistingRegistryAsync(string databaseFile, string actor = "actor")
        {
            if (!File.Exists(databaseFile)) throw new InvalidOperationException("The controlled registry must already exist.");
            connection.Close(); connection.ConnectionString = new SqliteConnectionStringBuilder { DataSource = databaseFile }.ConnectionString;
            connection.Open(); Db.ChangeTracker.Clear();
            var companies = new RepositoryCompanyContextResolver(new RepositoryAccessService(Db));
            var selected = await companies.ResolveAsync(actor, new("tenant", "company")) ?? throw new InvalidOperationException("The existing registry account must be active.");
            CompanySelection = new(); CompanySelection.SelectCompany(selected);
            Gate = new(new ModuleConnectionResolver(Db, Editor.Object, CompanySelection), CompanySelection, companies);
            Request.User = new(new ClaimsIdentity([new("party_id", actor)], "test"));
            ConfigureCompanyServices("company");
        }
        private void ConfigureCompanyServices(string organization)
        {
            var configured = recordedConnection;
            var config = new Mock<IConfigEditor>(); config.SetupGet(value => value.DataConnections).Returns([configured]);
            Editor.SetupGet(value => value.ConfigEditor).Returns(config.Object);
            var target = new Mock<IDataConnection>(); target.SetupGet(value => value.ConnectionProp).Returns(configured);
            target.As<IBeepConnectionEvidence>().SetupGet(value => value.DbConn).Returns(LeasePhysicalConnection.Object);
            Source.SetupGet(value => value.DatasourceName).Returns(configured.ConnectionName); Source.SetupGet(value => value.Dataconnection).Returns(target.Object);
            Editor.Setup(value => value.OpenDataSource("selected-tenant")).Returns(System.Data.ConnectionState.Open);
            if (!EligibilityConfiguration.Companies.Any(rule => rule.BusinessAssociateId == organization))
                EligibilityConfiguration.Companies.Add(new() { TenantId = "tenant", BusinessAssociateId = organization, TimeZoneId = "UTC",
                    EntitlementType = "test-app-data", ComponentType = "test-field", ComponentAccessConditionCode = "test-unconditional",
                    AccessTypes = new() { ["READ"] = "test-read", ["WRITE"] = "test-write", ["DELETE"] = "test-delete" } });
            Policy = new(Gate, new RepositoryAssetAccessStore(Db, Gate, Mock.Of<IAssetGrantTargetVerifier>()), Metadata.Object, Eligibility, Editor.Object, Columns.Object, Defaults.Object, Microsoft.Extensions.Options.Options.Create(EligibilityConfiguration));
            Service = new(Editor.Object, Columns.Object, Defaults.Object, Metadata.Object, Gate, NullLogger<PPDM39GenericDataService>.Instance, Policy);
        }
        public async Task Grant(string type, string id, string level = "READ", bool inherit = false, string? facilityType = null)
        {
            if (type == "FIELD") AddFieldEntitlement(id);
            Assert.True(await new RepositoryAssetAccessStore(Db, Gate, Mock.Of<IAssetGrantTargetVerifier>()).GrantAsync(Gate.Company.UserId, (await Gate.ResolveAsync()).Fingerprint, type, id, level, inherit, facilityType: facilityType));
        }
        public void AddFieldEntitlement(string id, string level = "DELETE")
        {
            // Explicit test PPDM evidence; no production configuration or domain defaults are seeded.
            var company = Gate.Company.Scope.BusinessAssociateId;
            if (!Rows.OfType<R_ENT_TYPE>().Any()) Rows.Add(new R_ENT_TYPE { ENTITLEMENT_TYPE = "test-app-data", ACTIVE_IND = "Y" });
            if (!Rows.OfType<R_ENT_COMPONENT_TYPE>().Any()) Rows.Add(new R_ENT_COMPONENT_TYPE { ENT_COMP_TYPE = "test-field", ACTIVE_IND = "Y" });
            if (!Rows.OfType<R_ACCESS_CONDITION>().Any()) Rows.Add(new R_ACCESS_CONDITION { ACCESS_CONDITION_CODE = "test-unconditional", ACTIVE_IND = "Y" });
            foreach (var code in new[] { "test-read", "test-write", "test-delete" })
                if (!Rows.OfType<R_ENT_ACCESS_TYPE>().Any(row => row.ACCESS_TYPE == code)) Rows.Add(new R_ENT_ACCESS_TYPE { ACCESS_TYPE = code, ACTIVE_IND = "Y" });
            var entitlementId = company + ":" + id;
            if (Rows.OfType<ENT_COMPONENT>().Any(row => row.ENTITLEMENT_ID == entitlementId)) return;
            Rows.Add(new ENT_COMPONENT { ENTITLEMENT_ID = entitlementId, COMPONENT_ID = "test-component", COMPONENT_OBS_NO = 1,
                FIELD_ID = id, BUSINESS_ASSOCIATE_ID = company, ENT_COMP_TYPE = "test-field", ACCESS_COND_CODE = "test-unconditional",
                ACTIVE_IND = "Y", EFFECTIVE_DATE = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
            Rows.Add(new ENTITLEMENT { ENTITLEMENT_ID = entitlementId, ENTITLEMENT_TYPE = "test-app-data", ACCESS_COND_CODE = "test-" + level.ToLowerInvariant(),
                ACTIVE_IND = "Y", EFFECTIVE_DATE = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
        }
        public PPDMGenericRepository Repository(Type type) => new(Editor.Object, Columns.Object, Defaults.Object, Metadata.Object, type, "selected-tenant", type.Name, rowAccess: Policy);
        public void Dispose() { identity.Dispose(); Db.Dispose(); connection.Dispose(); }
    }
}
