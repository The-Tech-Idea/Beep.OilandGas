using System;using System.Collections.Generic;using System.Globalization;using System.IO;using System.Linq;using System.Threading;using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Services
{
    public class DataImportService
    {
        private readonly IDMEEditor _editor;private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;private readonly ILogger<DataImportService> _logger;

        public DataImportService(IDMEEditor editor,ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata,
            string connectionName="PPDM39",ILogger<DataImportService>? logger=null)
        {_editor=editor;_commonColumnHandler=commonColumnHandler;_defaults=defaults;_metadata=metadata;_connectionName=connectionName;_logger=logger;}

        public async Task<DataImportResult> ImportCsvAsync(string csvFilePath,string tableName,
            DataImportOptions? options=null,IProgress<int>? progress=null,CancellationToken token=default)
        {
            if(string.IsNullOrWhiteSpace(csvFilePath))throw new ArgumentException("CSV path required");
            if(string.IsNullOrWhiteSpace(tableName))throw new ArgumentException("Table name required");
            if(!File.Exists(csvFilePath))throw new FileNotFoundException($"CSV not found: {csvFilePath}");

            var result=new DataImportResult();
            try
            {
                _logger.LogInformation("Importing {File} → {Table}",csvFilePath,tableName);

                // 1. Read CSV lines
                var lines=await File.ReadAllLinesAsync(csvFilePath,token);
                if(lines.Length<2){result.ErrorMessage="CSV has no data rows";return result;}

                // 2. Parse header
                var headers=ParseCsvLine(lines[0]);
                var rows=new List<string[]>();
                for(int i=1;i<lines.Length;i++)
                {if(string.IsNullOrWhiteSpace(lines[i]))continue;rows.Add(ParseCsvLine(lines[i]));}
                result.RecordsRead=rows.Count;
                if(rows.Count==0){result.ErrorMessage="No data rows found";return result;}

                // 3. Get PPDM table metadata and entity type
                var metadata=await _metadata.GetTableMetadataAsync(tableName);
                if(metadata==null){result.ErrorMessage=$"Table '{tableName}' not found in PPDM metadata";return result;}
                var entityType=Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ??Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}");
                if(entityType==null){result.ErrorMessage=$"Entity type for '{tableName}' not found";return result;}

                // 4. Create repository and insert records
                var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,entityType,_connectionName,tableName);
                int inserted=0,failed=0;
                var qualityRules=options?.QualityRules??new List<IDataQualityRule>();

                for(int i=0;i<rows.Count;i++)
                {
                    token.ThrowIfCancellationRequested();
                    if(progress!=null && i%100==0)progress.Report(i*100/rows.Count);
                    try
                    {
                        var entity=Activator.CreateInstance(entityType);
                        for(int c=0;c<Math.Min(headers.Length,rows[i].Length);c++)
                        {var prop=entityType.GetProperty(headers[c],System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.IgnoreCase);if(prop!=null&&prop.CanWrite){try{prop.SetValue(entity,Convert.ChangeType(rows[i][c],Nullable.GetUnderlyingType(prop.PropertyType)??prop.PropertyType));}catch{}}}
                        // Set PPDM standard columns
                        var activeInd=entityType.GetProperty("ACTIVE_IND");if(activeInd!=null)activeInd.SetValue(entity,"Y");
                        var ppdmGuid=entityType.GetProperty("PPDM_GUID");if(ppdmGuid!=null)ppdmGuid.SetValue(entity,Guid.NewGuid().ToString());
                        // Run quality rules
                        bool passed=true;
                        foreach(var rule in qualityRules){if(!rule.Evaluate(entity)){failed++;passed=false;break;}}
                        if(passed){await repo.InsertAsync(entity,"SYSTEM");inserted++;}
                    }
                    catch(Exception ex){_logger.LogWarning(ex,"Row {Row} failed",i+2);failed++;}
                }
                if(progress!=null)progress.Report(100);
                result.RecordsInserted=inserted;result.RecordsFailed=failed;result.Success=inserted>0;
            }
            catch(OperationCanceledException){throw;}
            catch(Exception ex){_logger.LogError(ex,"Import failed");result.ErrorMessage=ex.Message;}
            return result;
        }

        private static string[] ParseCsvLine(string line)
        {var r=new List<string>();bool inQuotes=false;var current="";
        for(int i=0;i<line.Length;i++){var c=line[i];
        if(c=='"')inQuotes=!inQuotes;else if(c==','&&!inQuotes){r.Add(current.Trim());current="";}
        else current+=c;}r.Add(current.Trim());return r.ToArray();}

        public void Dispose(){}
    }

    public class DataImportOptions{public List<IDataQualityRule> QualityRules{get;set;}=new();public int? BatchSize{get;set;}}
    public class DataImportResult{public bool Success{get;set;}public string? ContextKey{get;set;}public string? ErrorMessage{get;set;}public int RecordsRead{get;set;}public int RecordsInserted{get;set;}public int RecordsFailed{get;set;}public int RecordsSkipped{get;set;}public TimeSpan Duration{get;set;}public string? ErrorStorePath{get;set;}}
    public interface IDataQualityRule{bool Evaluate(object entity);}

    public class NotNullRule:IDataQualityRule{public string FieldName{get;set;}="";public bool Evaluate(object e){var p=e.GetType().GetProperty(FieldName);return p!=null&&p.GetValue(e)!=null;}}
    public class RangeRule:IDataQualityRule{public string FieldName{get;set;}="";public decimal Min{get;set;}public decimal Max{get;set;}=decimal.MaxValue;public bool Evaluate(object e){var p=e.GetType().GetProperty(FieldName);if(p==null)return true;var v=p.GetValue(e);return v==null||(Convert.ToDecimal(v)>=Min&&Convert.ToDecimal(v)<=Max);}}
    public class AcceptedValuesRule:IDataQualityRule{public string FieldName{get;set;}="";public HashSet<string> Values{get;set;}=new();public bool Evaluate(object e){var p=e.GetType().GetProperty(FieldName);if(p==null)return true;var v=p.GetValue(e);return v==null||Values.Contains(v.ToString()??"");}}
}
