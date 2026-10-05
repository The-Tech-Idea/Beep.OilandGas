using System;using System.Collections.Generic;using System.ComponentModel;using System.Globalization;using System.IO;using System.Linq;using System.Reflection;using System.Threading;using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdeaWeb.Diagnostics;
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
        private readonly string _connectionName;private readonly ILogger<DataImportService>? _logger;
        private readonly IFailureReporter _failures;

        public DataImportService(IDMEEditor editor,ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata,IFailureReporter failures,
            string connectionName="PPDM39",ILogger<DataImportService>? logger=null)
        {_editor=editor;_commonColumnHandler=commonColumnHandler;_defaults=defaults;_metadata=metadata;_failures=failures??throw new ArgumentNullException(nameof(failures));_connectionName=connectionName;_logger=logger;}

        /// <summary>
        /// Imports a CSV file's rows into a PPDM table. A file with no data rows, or naming a table PPDM does not have, is
        /// refused; a row with a value its column cannot hold, a row a quality rule rejects, and a row the store refuses are
        /// not imported, and the result says how many (OILGAS-CATCH-01: the import answered a failure's own text, left an
        /// unconvertible value unset and inserted the row anyway, and reported nothing).
        /// </summary>
        public async Task<DataImportResult> ImportCsvAsync(string csvFilePath,string tableName,string userId,
            DataImportOptions? options=null,IProgress<int>? progress=null,CancellationToken token=default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(csvFilePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
            if(!File.Exists(csvFilePath))throw new FileNotFoundException("The CSV file to import was not found.",csvFilePath);

            var result=new DataImportResult();
            _logger?.LogInformation("Importing {File} → {Table}",csvFilePath,tableName);

            // 1. Read CSV lines
            var lines=await File.ReadAllLinesAsync(csvFilePath,token);

            // 2. Parse header
            if(lines.Length<2)throw RefusalException.Invalid("The CSV file has no data rows.");
            var headers=ParseCsvLine(lines[0]);
            var rows=new List<string[]>();
            for(int i=1;i<lines.Length;i++)
            {if(string.IsNullOrWhiteSpace(lines[i]))continue;rows.Add(ParseCsvLine(lines[i]));}
            result.RecordsRead=rows.Count;
            if(rows.Count==0)throw RefusalException.Invalid("The CSV file has no data rows.");

            // 3. Get PPDM table metadata and entity type
            var metadata=await _metadata.GetTableMetadataAsync(tableName);
            if(metadata==null)throw RefusalException.NotFound($"Table '{tableName}' is not a PPDM table.");
            var entityType=Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ??Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}");
            if(entityType==null)
                throw new InvalidOperationException($"PPDM table '{tableName}' maps to entity type '{metadata.EntityTypeName}', which this application does not have.");

            // 4. Create repository and insert records
            var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,entityType,_connectionName,tableName);
            int inserted=0,failed=0;
            var qualityRules=options?.QualityRules??new List<IDataQualityRule>();

            for(int i=0;i<rows.Count;i++)
            {
                token.ThrowIfCancellationRequested();
                if(progress!=null && i%100==0)progress.Report(i*100/rows.Count);
                var entity=Activator.CreateInstance(entityType)
                    ??throw new InvalidOperationException($"Entity type '{entityType.Name}' could not be created.");
                if(!TryFill(entity,entityType,headers,rows[i])){failed++;continue;}
                // Set PPDM standard columns
                var activeInd=entityType.GetProperty("ACTIVE_IND");if(activeInd!=null)activeInd.SetValue(entity,"Y");
                var ppdmGuid=entityType.GetProperty("PPDM_GUID");if(ppdmGuid!=null)ppdmGuid.SetValue(entity,Guid.NewGuid().ToString());
                // Run quality rules
                if(qualityRules.Any(rule=>!rule.Evaluate(entity))){failed++;continue;}
                try{await repo.InsertAsync(entity,userId);inserted++;}
                // Whatever the store refuses a row with — a key already there, a constraint, a value too long — that row is
                // not imported and the import goes on with the next; the caller is told how many were not. Cancellation is
                // the request ending, not a row failing.
                catch(Exception rowFailure) when (rowFailure is not OperationCanceledException)
                {
                    failed++;
                    _failures.ReportHandled(rowFailure,$"importing CSV row {i+2} into {tableName}",
                        "the row is not imported; the import goes on and its result counts the row as not imported",FailureSeverity.Degraded);
                }
            }
            if(progress!=null)progress.Report(100);
            result.RecordsInserted=inserted;result.RecordsFailed=failed;result.Success=failed==0;
            return result;
        }

        // A cell is set only when its property's type can hold it — asked of the type's converter, in the invariant culture a
        // CSV is written in — and an empty cell leaves a non-text property unset. A row with a cell that cannot be held is
        // not imported at all: setting the rest and inserting it would store a row that is not what the file said.
        private static bool TryFill(object entity,Type entityType,string[] headers,string[] row)
        {
            for(int c=0;c<Math.Min(headers.Length,row.Length);c++)
            {
                var prop=entityType.GetProperty(headers[c],BindingFlags.Public|BindingFlags.Instance|BindingFlags.IgnoreCase);
                if(prop==null||!prop.CanWrite)continue;
                var cell=row[c];
                var target=Nullable.GetUnderlyingType(prop.PropertyType)??prop.PropertyType;
                if(target==typeof(string)){prop.SetValue(entity,cell);continue;}
                if(cell.Length==0)continue;
                var converter=TypeDescriptor.GetConverter(target);
                if(!converter.IsValid(cell))return false;
                prop.SetValue(entity,converter.ConvertFromInvariantString(cell));
            }
            return true;
        }

        private static string[] ParseCsvLine(string line)
        {var r=new List<string>();bool inQuotes=false;var current="";
        for(int i=0;i<line.Length;i++){var c=line[i];
        if(c=='"')inQuotes=!inQuotes;else if(c==','&&!inQuotes){r.Add(current.Trim());current="";}
        else current+=c;}r.Add(current.Trim());return r.ToArray();}

        public void Dispose(){}
    }

    public class DataImportOptions{public List<IDataQualityRule> QualityRules{get;set;}=new();public int? BatchSize{get;set;}}
    public class DataImportResult{public bool Success{get;set;}public string? ContextKey{get;set;}public int RecordsRead{get;set;}public int RecordsInserted{get;set;}public int RecordsFailed{get;set;}public int RecordsSkipped{get;set;}public TimeSpan Duration{get;set;}public string? ErrorStorePath{get;set;}}
    public interface IDataQualityRule{bool Evaluate(object entity);}

    public class NotNullRule:IDataQualityRule{public string FieldName{get;set;}="";public bool Evaluate(object e){var p=e.GetType().GetProperty(FieldName);return p!=null&&p.GetValue(e)!=null;}}
    public class RangeRule:IDataQualityRule{public string FieldName{get;set;}="";public decimal Min{get;set;}public decimal Max{get;set;}=decimal.MaxValue;public bool Evaluate(object e){var p=e.GetType().GetProperty(FieldName);if(p==null)return true;var v=p.GetValue(e);return v==null||(Convert.ToDecimal(v)>=Min&&Convert.ToDecimal(v)<=Max);}}
    public class AcceptedValuesRule:IDataQualityRule{public string FieldName{get;set;}="";public HashSet<string> Values{get;set;}=new();public bool Evaluate(object e){var p=e.GetType().GetProperty(FieldName);if(p==null)return true;var v=p.GetValue(e);return v==null||Values.Contains(v.ToString()??"");}}
}
