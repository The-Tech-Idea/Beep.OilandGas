using System;using System.IO;using System.Threading;using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;using Microsoft.AspNetCore.Mvc;using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers
{
    [ApiController][Route("api/data-import")][Authorize]
    public class DataImportController:ControllerBase
    {
        private readonly DataImportService _svc;private readonly ILogger<DataImportController> _log;
        public DataImportController(DataImportService svc,ILogger<DataImportController> log){_svc=svc;_log=log;}

        [HttpPost("csv/{tableName}")][RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> ImportCsv(string tableName,IFormFile file,CancellationToken token)
        {
            if(file==null||file.Length==0)return BadRequest(new{error="No file uploaded."});
            if(!file.FileName.EndsWith(".csv",StringComparison.OrdinalIgnoreCase))return BadRequest(new{error="Only .csv files accepted."});
            var tempDir=Path.Combine(Path.GetTempPath(),"BeepDataImport");Directory.CreateDirectory(tempDir);
            var tempPath=Path.Combine(tempDir,$"{Guid.NewGuid()}_{file.FileName}");
            try{
                await using var stream=new FileStream(tempPath,FileMode.Create);await file.CopyToAsync(stream,token);
                var progress=new Progress<int>(p=>_log.LogDebug("Import: {Pct}%",p));
                var result=await _svc.ImportCsvAsync(tempPath,tableName,progress:progress,token:token);
                if(result.Success)return Ok(new{message="Import complete",recordsRead=result.RecordsRead,recordsInserted=result.RecordsInserted,recordsFailed=result.RecordsFailed});
                return StatusCode(500,new{error=result.ErrorMessage??"Import failed",recordsRead=result.RecordsRead,recordsFailed=result.RecordsFailed});
            }
            finally{try{if(System.IO.File.Exists(tempPath))System.IO.File.Delete(tempPath);}catch(Exception ex){_log.LogWarning(ex,"Failed to clean temp file: {Path}",tempPath);}}
        }

        [HttpGet("profile/{tableName}")]
        public IActionResult ProfileTable(string tableName){return Ok(new{tableName,message="Profiling available via /api/ppdm39/data/{tableName}/export"});}
    }
}
