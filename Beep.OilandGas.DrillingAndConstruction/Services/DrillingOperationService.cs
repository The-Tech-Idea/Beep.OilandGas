using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.DataBase;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.DrillingAndConstruction.Services
{
    /// <summary>
    /// Service for managing drilling operations.
    /// Uses PPDMGenericRepository for data access following LifeCycle patterns.
    /// </summary>
    public class DrillingOperationService : IDrillingOperationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<DrillingOperationService>? _logger;

        // Cached repositories
        private PPDMGenericRepository? _wellRepository;
        private PPDMGenericRepository? _drillReportRepository;

        public DrillingOperationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<DrillingOperationService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        #region Repository Helpers

        private async Task<PPDMGenericRepository> GetWellRepositoryAsync()
        {
            if (_wellRepository == null)
            {
                _wellRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    typeof(WELL),
                    _connectionName,
                    "WELL",
                    null);
            }
            return _wellRepository;
        }

        private async Task<PPDMGenericRepository> GetDrillReportRepositoryAsync()
        {
            if (_drillReportRepository == null)
            {
                _drillReportRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    typeof(WELL_DRILL_REPORT),
                    _connectionName,
                    "WELL_DRILL_REPORT",
                    null);
            }
            return _drillReportRepository;
        }

        private List<T> ConvertToList<T>(IEnumerable<object>? entities) where T : class
        {
            var result = new List<T>();
            if (entities == null) return result;
            
            foreach (var item in entities)
            {
                if (item is T entity)
                {
                    result.Add(entity);
                }
            }
            return result;
        }

        #endregion

        public async Task<List<DrillingOperationDto>> GetDrillingOperationsAsync(string? wellUWI = null)
        {
            _logger?.LogInformation("Getting drilling operations for well UWI: {WellUWI}", wellUWI ?? "all");

            var wellRepo = await GetWellRepositoryAsync();
            List<WELL> wells;

            if (!string.IsNullOrWhiteSpace(wellUWI))
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "UWI", FilterValue = wellUWI, Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };
                var wellEntities = await wellRepo.GetAsync(filters);
                wells = ConvertToList<WELL>(wellEntities);
            }
            else
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };
                var wellEntities = await wellRepo.GetAsync(filters);
                wells = ConvertToList<WELL>(wellEntities);
            }

            var operations = new List<DrillingOperationDto>();
            var drillReportRepo = await GetDrillReportRepositoryAsync();

            foreach (var well in wells)
            {
                var reportFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "UWI", FilterValue = well.UWI, Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };
                var reportEntities = await drillReportRepo.GetAsync(reportFilters);
                var reports = ConvertToList<WELL_DRILL_REPORT>(reportEntities);

                var operation = MapToDrillingOperationDto(well, reports);
                operations.Add(operation);
            }

            _logger?.LogInformation("Retrieved {Count} drilling operations", operations.Count);
            return operations;
        }

        public async Task<DrillingOperationDto?> GetDrillingOperationAsync(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
            {
                _logger?.LogWarning("GetDrillingOperationAsync called with null or empty operationId");
                return null;
            }

            _logger?.LogInformation("Getting drilling operation for UWI: {OperationId}", operationId);

            var wellRepo = await GetWellRepositoryAsync();
            var wellFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = operationId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var wellEntities = await wellRepo.GetAsync(wellFilters);
            var wells = ConvertToList<WELL>(wellEntities);
            var well = wells.FirstOrDefault();

            if (well == null)
            {
                _logger?.LogWarning("Well not found for UWI: {OperationId}", operationId);
                return null;
            }

            var drillReportRepo = await GetDrillReportRepositoryAsync();
            var reportFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = operationId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var reportEntities = await drillReportRepo.GetAsync(reportFilters);
            var reports = ConvertToList<WELL_DRILL_REPORT>(reportEntities);

            return MapToDrillingOperationDto(well, reports);
        }

        public async Task<DrillingOperationDto> CreateDrillingOperationAsync(CreateDrillingOperationDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrWhiteSpace(createDto.WellUWI))
                throw new ArgumentException("Well UWI cannot be null or empty", nameof(createDto));

            _logger?.LogInformation("Creating drilling operation for well UWI: {WellUWI}", createDto.WellUWI);

            var wellRepo = await GetWellRepositoryAsync();
            
            // Check if well exists
            var wellFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = createDto.WellUWI, Operator = "=" }
            };
            var existingWells = await wellRepo.GetAsync(wellFilters);
            var existingWell = ConvertToList<WELL>(existingWells).FirstOrDefault();

            WELL well;
            if (existingWell == null)
            {
                // Create new well if it doesn't exist
                _logger?.LogInformation("Creating new well for UWI: {WellUWI}", createDto.WellUWI);
                well = new WELL
                {
                    UWI = createDto.WellUWI,
                    ACTIVE_IND = "Y",
                    BASE_DEPTH = createDto.TargetDepth ?? 0m
                };
                await wellRepo.InsertAsync(well, "SYSTEM");
            }
            else
            {
                well = existingWell;
            }

            // Create drilling report
            var drillReportRepo = await GetDrillReportRepositoryAsync();
            var report = new WELL_DRILL_REPORT
            {
                REPORT_ID = _defaults.FormatIdForTable("WELL_DRILL_REPORT", Guid.NewGuid().ToString()),
                UWI = createDto.WellUWI,
                ACTIVE_IND = "Y",
                EFFECTIVE_DATE = createDto.PlannedSpudDate ?? DateTime.UtcNow
            };

            await drillReportRepo.InsertAsync(report, "SYSTEM");

            _logger?.LogInformation("Successfully created drilling operation for well UWI: {WellUWI}", createDto.WellUWI);

            return MapToDrillingOperationDto(well, new List<WELL_DRILL_REPORT> { report });
        }

        public async Task<DrillingOperationDto> UpdateDrillingOperationAsync(string operationId, UpdateDrillingOperationDto updateDto)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                throw new ArgumentException("Operation ID cannot be null or empty.", nameof(operationId));

            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            _logger?.LogInformation("Updating drilling operation for UWI: {OperationId}", operationId);

            var wellRepo = await GetWellRepositoryAsync();
            var wellFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = operationId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var wellEntities = await wellRepo.GetAsync(wellFilters);
            var wells = ConvertToList<WELL>(wellEntities);
            var well = wells.FirstOrDefault();

            if (well == null)
                throw new KeyNotFoundException($"Drilling operation with ID {operationId} not found.");

            // Update well properties if provided
            if (updateDto.Status != null)
            {
                // Status could be stored in a custom field or derived from ACTIVE_IND
                // For now, we'll update ACTIVE_IND based on status
                if (updateDto.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    well.ACTIVE_IND = "N";
                else if (updateDto.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    well.ACTIVE_IND = "Y";
            }

            await wellRepo.UpdateAsync(well, "SYSTEM");

            var drillReportRepo = await GetDrillReportRepositoryAsync();
            var reportFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = operationId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var reportEntities = await drillReportRepo.GetAsync(reportFilters);
            var reports = ConvertToList<WELL_DRILL_REPORT>(reportEntities);

            _logger?.LogInformation("Successfully updated drilling operation for UWI: {OperationId}", operationId);

            return MapToDrillingOperationDto(well, reports);
        }

        public async Task<List<DrillingReportDto>> GetDrillingReportsAsync(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
            {
                _logger?.LogWarning("GetDrillingReportsAsync called with null or empty operationId");
                return new List<DrillingReportDto>();
            }

            _logger?.LogInformation("Getting drilling reports for operation UWI: {OperationId}", operationId);

            var drillReportRepo = await GetDrillReportRepositoryAsync();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = operationId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var reportEntities = await drillReportRepo.GetAsync(filters);
            var reports = ConvertToList<WELL_DRILL_REPORT>(reportEntities);

            var result = reports.Select(r => new DrillingReportDto
            {
                ReportId = r.REPORT_ID ?? string.Empty,
                OperationId = operationId,
                ReportDate = r.EFFECTIVE_DATE.Value,
                Remarks = r.REMARK
            }).ToList();

            _logger?.LogInformation("Retrieved {Count} drilling reports for operation UWI: {OperationId}", result.Count, operationId);

            return result;
        }

        public async Task<DrillingReportDto> CreateDrillingReportAsync(string operationId, CreateDrillingReportDto createDto)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                throw new ArgumentException("Operation ID cannot be null or empty.", nameof(operationId));

            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            _logger?.LogInformation("Creating drilling report for operation UWI: {OperationId}", operationId);

            var drillReportRepo = await GetDrillReportRepositoryAsync();
            var report = new WELL_DRILL_REPORT
            {
                REPORT_ID = _defaults.FormatIdForTable("WELL_DRILL_REPORT", Guid.NewGuid().ToString()),
                UWI = operationId,
                ACTIVE_IND = "Y",
                EFFECTIVE_DATE = createDto.ReportDate,
                REMARK = createDto.Remarks
            };

            await drillReportRepo.InsertAsync(report, "SYSTEM");

            _logger?.LogInformation("Successfully created drilling report {ReportId} for operation UWI: {OperationId}", 
                report.REPORT_ID, operationId);

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

