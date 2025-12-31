using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.DrillingAndConstruction.Services
{
    /// <summary>
    /// Service for managing drilling operations.
    /// Uses UnitOfWork directly for data access.
    /// </summary>
    public class DrillingOperationService : IDrillingOperationService
    {
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;

        public DrillingOperationService(IDMEEditor editor, string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _connectionName = connectionName;
        }

        private List<T> ConvertToList<T>(object units) where T : class
        {
            var result = new List<T>();
            if (units == null) return result;
            
            if (units is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is T entity)
                    {
                        result.Add(entity);
                    }
                }
            }
            return result;
        }

        private IUnitOfWorkWrapper GetWellUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(WELL), _editor, _connectionName, "WELL", "UWI");
        }

        private IUnitOfWorkWrapper GetDrillReportUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(WELL_DRILL_REPORT), _editor, _connectionName, "WELL_DRILL_REPORT", "REPORT_ID");
        }

        public async Task<List<DrillingOperationDto>> GetDrillingOperationsAsync(string? wellUWI = null)
        {
            var wellUow = GetWellUnitOfWork();
            List<WELL> wells;

            if (!string.IsNullOrWhiteSpace(wellUWI))
            {
                var well = wellUow.Read(wellUWI) as WELL;
                wells = well != null ? new List<WELL> { well } : new List<WELL>();
            }
            else
            {
                var units = await wellUow.Get();
                List<WELL> allWells = ConvertToList<WELL>(units);
                wells = allWells.Where(w => w.ACTIVE_IND == "Y").ToList();
            }

            var operations = new List<DrillingOperationDto>();
            var drillReportUow = GetDrillReportUnitOfWork();

            foreach (var well in wells)
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "UWI", FilterValue = well.UWI, Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };
                var reportUnits = await drillReportUow.Get(filters);
                var reports = ConvertToList<WELL_DRILL_REPORT>(reportUnits);

                var operation = MapToDrillingOperationDto(well, reports);
                operations.Add(operation);
            }

            return operations;
        }

        public async Task<DrillingOperationDto?> GetDrillingOperationAsync(string operationId)
        {
            // operationId is the UWI
            if (string.IsNullOrWhiteSpace(operationId))
                return null;

            var wellUow = GetWellUnitOfWork();
            var well = wellUow.Read(operationId) as WELL;
            if (well == null)
                return null;

            var drillReportUow = GetDrillReportUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = operationId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var reportUnits = await drillReportUow.Get(filters);
            var reports = ConvertToList<WELL_DRILL_REPORT>(reportUnits);

            return MapToDrillingOperationDto(well, reports);
        }

        public async Task<DrillingOperationDto> CreateDrillingOperationAsync(CreateDrillingOperationDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var wellUow = GetWellUnitOfWork();
            var well = wellUow.Read(createDto.WellUWI) as WELL;
            if (well == null)
            {
                // Create new well if it doesn't exist
                well = new WELL
                {
                    UWI = createDto.WellUWI,
                    ACTIVE_IND = "Y",
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CHANGED_DATE = DateTime.UtcNow
                };
                var wellResult = await wellUow.InsertDoc(well);
                if (wellResult.Flag != Errors.Ok)
                    throw new InvalidOperationException($"Failed to create well: {wellResult.Message}");
                await wellUow.Commit();
            }

            var drillReportUow = GetDrillReportUnitOfWork();
            var report = new WELL_DRILL_REPORT
            {
                REPORT_ID = Guid.NewGuid().ToString(),
                UWI = createDto.WellUWI,
                ACTIVE_IND = "Y",
                EFFECTIVE_DATE = createDto.PlannedSpudDate ?? DateTime.UtcNow,
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_DATE = DateTime.UtcNow
            };

            var result = await drillReportUow.InsertDoc(report);
            if (result.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to create drilling operation: {result.Message}");

            await drillReportUow.Commit();

            return MapToDrillingOperationDto(well, new List<WELL_DRILL_REPORT> { report });
        }

        public async Task<DrillingOperationDto> UpdateDrillingOperationAsync(string operationId, UpdateDrillingOperationDto updateDto)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                throw new ArgumentException("Operation ID cannot be null or empty.", nameof(operationId));

            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            var wellUow = GetWellUnitOfWork();
            var well = wellUow.Read(operationId) as WELL;
            if (well == null)
                throw new KeyNotFoundException($"Drilling operation with ID {operationId} not found.");

            well.ROW_CHANGED_DATE = DateTime.UtcNow;

            var result = await wellUow.UpdateDoc(well);
            if (result.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to update drilling operation: {result.Message}");

            await wellUow.Commit();

            var drillReportUow = GetDrillReportUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = operationId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var reportUnits = await drillReportUow.Get(filters);
            var reports = ConvertToList<WELL_DRILL_REPORT>(reportUnits);

            return MapToDrillingOperationDto(well, reports);
        }

        public async Task<List<DrillingReportDto>> GetDrillingReportsAsync(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                return new List<DrillingReportDto>();

            var drillReportUow = GetDrillReportUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = operationId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var units = await drillReportUow.Get(filters);
            List<WELL_DRILL_REPORT> reports = ConvertToList<WELL_DRILL_REPORT>(units);

            return reports.Select(r => new DrillingReportDto
            {
                ReportId = r.REPORT_ID ?? string.Empty,
                OperationId = operationId,
                ReportDate = r.EFFECTIVE_DATE.Value,
                Remarks = r.REMARK
            }).ToList();
        }

        public async Task<DrillingReportDto> CreateDrillingReportAsync(string operationId, CreateDrillingReportDto createDto)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                throw new ArgumentException("Operation ID cannot be null or empty.", nameof(operationId));

            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var drillReportUow = GetDrillReportUnitOfWork();
            var report = new WELL_DRILL_REPORT
            {
                REPORT_ID = Guid.NewGuid().ToString(),
                UWI = operationId,
                ACTIVE_IND = "Y",
                EFFECTIVE_DATE = createDto.ReportDate,
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_DATE = DateTime.UtcNow,
                REMARK = createDto.Remarks
            };

            var result = await drillReportUow.InsertDoc(report);
            if (result.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to create drilling report: {result.Message}");

            await drillReportUow.Commit();

            return new DrillingReportDto
            {
                ReportId = report.REPORT_ID ?? string.Empty,
                OperationId = operationId,
                ReportDate = createDto.ReportDate,
                Depth = createDto.Depth,
                Activity = createDto.Activity,
                Hours = createDto.Hours,
                Remarks = createDto.Remarks
            };
        }

        private DrillingOperationDto MapToDrillingOperationDto(WELL well, List<WELL_DRILL_REPORT> reports)
        {
            var firstReport = reports.FirstOrDefault();
            return new DrillingOperationDto
            {
                OperationId = well.UWI ?? string.Empty,
                WellUWI = well.UWI ?? string.Empty,
                WellName = well.UWI ?? string.Empty,
                SpudDate = firstReport?.EFFECTIVE_DATE,
                CompletionDate = firstReport?.END_DATE,
                Status = well.ACTIVE_IND == "Y" ? "Active" : "Inactive",
                TargetDepth = well.BASE_DEPTH,
                Reports = reports.Select(r => new DrillingReportDto
                {
                    ReportId = r.REPORT_ID ?? string.Empty,
                    OperationId = well.UWI ?? string.Empty,
                    ReportDate = r.EFFECTIVE_DATE.Value,
                    Remarks = r.REMARK
                }).ToList()
            };
        }
    }
}

