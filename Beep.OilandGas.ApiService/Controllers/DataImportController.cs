using System;using System.IO;using System.Threading;using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;using Microsoft.AspNetCore.Mvc;using Microsoft.Extensions.Logging;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Controllers
{
    [ApiController][Route("api/data-import")][Authorize]
    public class DataImportController:ControllerBase
    {
        private readonly DataImportService _svc;private readonly ILogger<DataImportController> _log;private readonly IFailureReporter _failures;
        public DataImportController(DataImportService svc,ILogger<DataImportController> log,IFailureReporter failures){_svc=svc;_log=log;_failures=failures;}

        [HttpPost("csv/{tableName}")][RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> ImportCsv(string tableName,IFormFile file,CancellationToken token)
        {
            var userId=User.ActingUserId();
            if(file==null||file.Length==0)return BadRequest(new{error="No file uploaded."});
            if(!file.FileName.EndsWith(".csv",StringComparison.OrdinalIgnoreCase))return BadRequest(new{error="Only .csv files accepted."});
            var tempDir=Path.Combine(Path.GetTempPath(),"BeepDataImport");Directory.CreateDirectory(tempDir);
            // The uploaded file's own name is the sender's text and never part of a path here: a name carrying "..\" would
            // write outside the import folder.
            var tempPath=Path.Combine(tempDir,$"{Guid.NewGuid():N}.csv");
            try{
                await using(var stream=new FileStream(tempPath,FileMode.Create)){await file.CopyToAsync(stream,token);}
                var progress=new Progress<int>(p=>_log.LogDebug("Import: {Pct}%",p));
                var result=await _svc.ImportCsvAsync(tempPath,tableName,userId,progress:progress,token:token);
                return Ok(new{
                    message=result.RecordsFailed==0?"Import complete.":$"{result.RecordsFailed} of {result.RecordsRead} rows were not imported.",
                    recordsRead=result.RecordsRead,recordsInserted=result.RecordsInserted,recordsFailed=result.RecordsFailed});
            }
            finally{DeleteTemporaryFile(tempPath);}
        }

        // The import has already answered; a temporary copy left behind costs disk, not the result.
        private void DeleteTemporaryFile(string tempPath)
        {
            try{if(System.IO.File.Exists(tempPath))System.IO.File.Delete(tempPath);}
            catch(Exception cleanup) when (cleanup is IOException or UnauthorizedAccessException)
            {
                _failures.ReportHandled(cleanup,"deleting a CSV import's temporary copy",
                    "the import's result stands; the temporary copy stays in the import folder until removed",FailureSeverity.Degraded);
            }
        }

        [HttpGet("profile/{tableName}")]
        public IActionResult ProfileTable(string tableName){return Ok(new{tableName,message="Profiling available via /api/ppdm39/data/{tableName}/export"});}
    }
}
