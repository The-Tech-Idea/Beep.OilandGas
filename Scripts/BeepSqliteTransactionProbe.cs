using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.ConfigUtil;
var errors=new ErrorsInfo{Flag=Errors.Ok};
using var source=new SQLiteDataSource("probe",null,null,DataSourceType.SqlLite,errors);
using var physical=new SQLiteConnection("Data Source=:memory:");physical.Open();
var rdb=(RDBDataConnection)source.Dataconnection;rdb.DbConn=physical;rdb.ConnectionStatus=ConnectionState.Open;
PPDMDataSourceTransaction.RequireIdle(source);
await PPDMDataSourceTransaction.ExecuteAsync(source,()=>{
    PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
    if(source.ActiveTransaction is null || !ReferenceEquals(source.ActiveTransaction.Connection,physical))throw new Exception("Owned transaction not attached to physical connection");
    using var command=source.GetDataCommand();if(command is null || !ReferenceEquals(command.Transaction,source.ActiveTransaction))throw new Exception("Command not enlisted");
    Console.WriteLine($"Isolation={source.ActiveTransaction.IsolationLevel}");return Task.CompletedTask;
});
if(source.ActiveTransaction is not null)throw new Exception("Commit did not release transaction");PPDMDataSourceTransaction.RequireIdle(source);
try{await PPDMDataSourceTransaction.ExecuteAsync(source,()=>{PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);throw new ProbeRollback();});throw new Exception("Rollback probe did not fail");}catch(ProbeRollback){}
if(source.ActiveTransaction is not null)throw new Exception("Rollback did not release transaction");PPDMDataSourceTransaction.RequireIdle(source);
Console.WriteLine("PASS: app transaction owner with real SQLite driver begin, enlistment, commit and rollback on an in-memory connection.");
sealed class ProbeRollback:Exception;
