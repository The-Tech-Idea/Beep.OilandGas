using System;
using System.Data;
using System.Reflection;
using TheTechIdea.Beep.DataBase;
var type=typeof(SQLiteDataSource);
var evidence=type.GetProperty("ActiveTransaction",BindingFlags.Public|BindingFlags.Instance);
Console.WriteLine($"SQLite={type.Assembly.Location}");
for(var current=type;current!=null;current=current.BaseType)Console.WriteLine($"Base={current.FullName} Assembly={current.Assembly.Location}");
Console.WriteLine($"UsableActiveTransaction={evidence is {CanRead:true} && evidence.GetIndexParameters().Length==0 && typeof(IDbTransaction).IsAssignableFrom(evidence.PropertyType)}");
