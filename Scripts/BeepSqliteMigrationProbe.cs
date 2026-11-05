using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using System.Data;
using System.Data.SQLite;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Migration;
using TheTechIdea.Beep.Helpers.RDBMSHelpers;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Report;
var errors=new ErrorsInfo{Flag=Errors.Ok};var editor=new Mock<IDMEEditor>();editor.SetupGet(x=>x.ErrorObject).Returns(errors);editor.SetupGet(x=>x.classCreator).Returns(new ClassCreator(editor.Object));
var driver=new ConnectionDriversConfig{AdapterType="probe-adapter",CommandBuilderType="probe-builder"};
var util=new Mock<IUtil>();util.Setup(x=>x.LinkConnection2Drivers(It.IsAny<IConnectionProperties>())).Returns(driver);editor.SetupGet(x=>x.Utilfunction).Returns(util.Object);
var handler=new Mock<IAssemblyHandler>();handler.Setup(x=>x.GetType("probe-builder")).Returns(typeof(SQLiteCommandBuilder));handler.Setup(x=>x.GetInstance("probe-adapter")).Returns(new SQLiteDataAdapter());handler.Setup(x=>x.GetInstance("probe-builder")).Returns(new SQLiteCommandBuilder());editor.SetupGet(x=>x.assemblyHandler).Returns(handler.Object);
using var source=new SQLiteDataSource("probe",null,editor.Object,DataSourceType.SqlLite,errors);
using var physical=new SQLiteConnection("Data Source=:memory:");physical.Open();var rdb=(RDBDataConnection)source.Dataconnection;rdb.DbConn=physical;rdb.ConnectionStatus=ConnectionState.Open;rdb.ConnectionProp=new ConnectionProperties{SchemaName=""};rdb.DataSourceDriver=driver;
source.GetListofEntitiesSql=DatabaseQueryRepositoryHelper.GetQuery(DataSourceType.SqlLite,Sqlcommandtype.getlistoftables);
if(string.IsNullOrEmpty(source.GetListofEntitiesSql))throw new Exception("The Beep SQLite entity-list query is unavailable.");
var manager=new MigrationManager(editor.Object,source);manager.RegisterAssembly(typeof(FACILITY_EQUIPMENT_INSTALL_INTENT).Assembly);
var result=manager.EnsureDatabaseCreatedForTypes(new[]{typeof(FACILITY_EQUIPMENT_INSTALL_INTENT)},detectRelationships:false);
Console.WriteLine($"Migration result {result.Flag}: {result.Message}");if(result.Flag!=Errors.Ok || !source.CheckEntityExist(nameof(FACILITY_EQUIPMENT_INSTALL_INTENT)))throw new Exception("The native intent table was not confirmed.");

var actual=source.GetEntityStructure(new EntityStructure{EntityName=nameof(FACILITY_EQUIPMENT_INSTALL_INTENT),DatasourceEntityName=nameof(FACILITY_EQUIPMENT_INSTALL_INTENT)},true);
Console.WriteLine($"Physical fields={actual?.Fields?.Count}");
if(actual?.Fields is null)throw new Exception("The native physical schema was unavailable.");
foreach(var field in actual.Fields)Console.WriteLine($"FIELD {field.FieldName} {field.Fieldtype} key={field.IsKey} size={field.Size}/{field.Size1} nullable={field.AllowDBNull}");
var expected=editor.Object.classCreator.ConvertToEntityStructure(typeof(FACILITY_EQUIPMENT_INSTALL_INTENT));
var mismatches=expected.Fields.Where(e=>actual.Fields.Count(a=>a.FieldName==e.FieldName && a.Fieldtype==e.Fieldtype)!=1).Select(e=>$"{e.FieldName} expected {e.Fieldtype} actual {actual.Fields.FirstOrDefault(a=>a.FieldName==e.FieldName)?.Fieldtype}").ToArray();
foreach(var mismatch in mismatches)Console.WriteLine($"MISMATCH {mismatch}");
Console.WriteLine($"CompleteModelTypeMatch={mismatches.Length==0}");
var context=new FieldSelectionReview(new("tenant","company"),new("core","fingerprint"),"actor","field-review","field");
var proposal=new DevelopmentEquipmentInstallationProposal(context,"facility","PROCESSING","pump","Engineer","Recorded installation");
var review=new DevelopmentEquipmentInstallationReview(proposal,new("company",0,0,new string('A',64)),new string('B',64),DateTimeOffset.UtcNow);
var original=new DevelopmentEquipmentInstallation(review,Guid.NewGuid(),Guid.NewGuid(),DateTime.UtcNow.AddMinutes(-1),true);
var nativeRow=original.Expected(1,DateTime.UtcNow);var entry=FACILITY_EQUIPMENT_INSTALL_INTENT.Create(original,nativeRow);
var inserted=source.InsertEntity(nameof(FACILITY_EQUIPMENT_INSTALL_INTENT),entry);Console.WriteLine($"Insert={inserted.Flag}: {inserted.Message}");if(inserted.Flag!=Errors.Ok)throw new Exception("Native datasource insert failed.");
var found=source.GetEntity(nameof(FACILITY_EQUIPMENT_INSTALL_INTENT),new List<AppFilter>{new(){FieldName="FACILITY_EQUIPMENT_INSTALL_INTENT_ID",Operator="=",FilterValue=entry.FACILITY_EQUIPMENT_INSTALL_INTENT_ID}}).ToArray();Console.WriteLine($"ReturnedRows={found.Length} Type={found.FirstOrDefault()?.GetType().FullName} Error={source.ErrorObject.Flag}");
if(found.Length!=1)throw new Exception("The inserted intent did not round-trip.");
var raw=found[0].GetType().GetProperty("ROW_CREATED_DATE")!.GetValue(found[0]);var decoded=(DateTime)Convert.ChangeType(raw!,typeof(DateTime),System.Globalization.CultureInfo.InvariantCulture);
Console.WriteLine($"Audit raw={raw} decoded-kind={decoded.Kind} exact={decoded==entry.ROW_CREATED_DATE} expected-kind={entry.ROW_CREATED_DATE?.Kind}");
var typedRows=new List<FACILITY_EQUIPMENT_INSTALL_INTENT>();
await foreach(var typed in source.GetEntityStreamAsync<FACILITY_EQUIPMENT_INSTALL_INTENT>(nameof(FACILITY_EQUIPMENT_INSTALL_INTENT),new List<AppFilter>{new(){FieldName="FACILITY_EQUIPMENT_INSTALL_INTENT_ID",Operator="=",FilterValue=entry.FACILITY_EQUIPMENT_INSTALL_INTENT_ID}}))typedRows.Add(typed);
Console.WriteLine($"TypedStream rows={typedRows.Count} audit={typedRows.FirstOrDefault()?.ROW_CREATED_DATE:O} kind={typedRows.FirstOrDefault()?.ROW_CREATED_DATE?.Kind} exact={typedRows.FirstOrDefault()?.ROW_CREATED_DATE==entry.ROW_CREATED_DATE}");




if(mismatches.Length!=0 || decoded.Kind!=DateTimeKind.Utc || decoded!=entry.ROW_CREATED_DATE)
{
    Console.Error.WriteLine("FAIL: The migrated schema or returned audit timestamp does not meet the installation owner exact UTC contract.");Environment.ExitCode=1;return;
}
Console.WriteLine("PASS: Migrated intent schema and native datasource preserve the exact UTC audit contract.");


