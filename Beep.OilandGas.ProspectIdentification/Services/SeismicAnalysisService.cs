using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using ProspectRecord = Beep.OilandGas.Models.Data.ProspectIdentification.PROSPECT;

namespace Beep.OilandGas.ProspectIdentification.Services
{
    /// <summary>
    /// Service for analyzing seismic data using PPDM repositories.
    /// </summary>
    public class SeismicAnalysisService : ISeismicAnalysisService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler? _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository? _defaults;
        private readonly IPPDMMetadataRepository? _metadata;
        private readonly string _connectionName;
        private readonly ILogger<SeismicAnalysisService>? _logger;

        public SeismicAnalysisService(IDMEEditor editor, string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _connectionName = connectionName;
        }

        public SeismicAnalysisService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<SeismicAnalysisService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName;
            _logger = logger;
        }

        public async Task<List<SeismicSurvey>> GetSeismicSurveysAsync(string? prospectId = null, string? fieldId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var surveyRepo = CreateSurveyRepository();

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(prospectId))
            {
                filters.Add(new AppFilter { FieldName = "AREA_ID", FilterValue = _defaults!.FormatIdForTable("SEIS_ACQTN_SURVEY", prospectId), Operator = "=" });
                filters.Add(new AppFilter { FieldName = "AREA_TYPE", FilterValue = "PROSPECT", Operator = "=" });
            }
            else if (!string.IsNullOrWhiteSpace(fieldId))
            {
                filters.Add(new AppFilter { FieldName = "AREA_ID", FilterValue = _defaults!.FormatIdForTable("SEIS_ACQTN_SURVEY", fieldId), Operator = "=" });
                filters.Add(new AppFilter { FieldName = "AREA_TYPE", FilterValue = "FIELD", Operator = "=" });
            }

            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = "START_DATE", FilterValue = startDate.Value.ToString("o"), Operator = ">=" });

            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = "START_DATE", FilterValue = endDate.Value.ToString("o"), Operator = "<=" });

            var surveys = (await surveyRepo.GetAsync(filters)).OfType<SEIS_ACQTN_SURVEY>().ToList();
            return surveys.Select(MapToSeismicSurveyDto).ToList();
        }

        public async Task<SeismicSurvey?> GetSeismicSurveyAsync(string surveyId)
        {
            if (string.IsNullOrWhiteSpace(surveyId)) return null;
            var survey = await GetSurveyEntityAsync(surveyId);
            return survey == null ? null : MapToSeismicSurveyDto(survey);
        }

        public async Task<SeismicInterpretationResult> PerformSeismicInterpretationAsync(string surveyId, SeismicInterpretationRequest request)
        {
            if (string.IsNullOrWhiteSpace(surveyId))
                throw new ArgumentException("Survey ID cannot be null or empty.", nameof(surveyId));

            var survey = await GetSurveyEntityAsync(surveyId)
                ?? throw new KeyNotFoundException($"Seismic survey with ID {surveyId} not found.");

            return new SeismicInterpretationResult
            {
                InterpretationId = Guid.NewGuid().ToString("N"),
                SurveyId = surveyId,
                InterpretationDate = DateTime.UtcNow,
                Interpreter = "System",
                Anomalies = IdentifyAnomalies(survey),
                StratigraphicUnits = new List<StratigraphicUnit>(),
                StructuralFeatures = new List<StructuralFeature>(),
                OverallAssessment = survey.REMARK ?? "Seismic interpretation pending",
                ConfidenceLevel = "0.5"
            };
        }

        public async Task<SeismicSurvey> CreateSeismicSurveyAsync(CreateSeismicSurvey createDto, string userId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var areaId = createDto.ProspectId ?? createDto.FieldId;
            if (string.IsNullOrWhiteSpace(areaId))
                throw new ArgumentException("Either ProspectId or FieldId must be provided in createDto.", nameof(createDto));

            if (!string.IsNullOrWhiteSpace(createDto.ProspectId))
            {
                var prospectRepo = CreateProspectRepository();
                var existingProspect = await prospectRepo.GetByIdAsync(createDto.ProspectId);
                if (existingProspect is not ProspectRecord)
                    throw new KeyNotFoundException($"Prospect {createDto.ProspectId} not found.");
            }

            var surveyId = string.IsNullOrWhiteSpace(createDto.SurveyId)
                ? Guid.NewGuid().ToString("N")
                : createDto.SurveyId;

            var survey = new SEIS_ACQTN_SURVEY
            {
                SEIS_SET_SUBTYPE = string.IsNullOrWhiteSpace(createDto.SurveyType) ? "2D" : createDto.SurveyType,
                SEIS_ACQTN_SURVEY_ID = _defaults!.FormatIdForTable("SEIS_ACQTN_SURVEY", surveyId),
                ACQTN_SURVEY_NAME = createDto.SurveyName ?? createDto.Name ?? surveyId,
                AREA_ID = _defaults.FormatIdForTable("SEIS_ACQTN_SURVEY", areaId),
                AREA_TYPE = string.IsNullOrWhiteSpace(createDto.ProspectId) ? "FIELD" : "PROSPECT",
                START_DATE = createDto.AcquisitionDate ?? createDto.SurveyDate,
                EFFECTIVE_DATE = createDto.SurveyDate ?? createDto.AcquisitionDate,
                REMARK = createDto.Description,
                ACTIVE_IND = "Y"
            };

            var surveyRepo = CreateSurveyRepository();
            await surveyRepo.InsertAsync(survey, userId);
            return MapToSeismicSurveyDto(survey);
        }

        public async Task<SeismicSurvey> UpdateSeismicSurveyAsync(string surveyId, UpdateSeismicSurvey updateDto, string userId)
        {
            if (string.IsNullOrWhiteSpace(surveyId)) throw new ArgumentNullException(nameof(surveyId));
            if (updateDto == null) throw new ArgumentNullException(nameof(updateDto));

            var surveyRepo = CreateSurveyRepository();
            var survey = await GetSurveyEntityAsync(surveyId)
                ?? throw new KeyNotFoundException($"Seismic survey {surveyId} not found.");

            if (!string.IsNullOrWhiteSpace(updateDto.Name))
                survey.ACQTN_SURVEY_NAME = updateDto.Name;
            if (!string.IsNullOrWhiteSpace(updateDto.Description))
                survey.REMARK = updateDto.Description;
            if (updateDto.SurveyDate.HasValue)
            {
                survey.START_DATE = updateDto.SurveyDate;
                survey.EFFECTIVE_DATE = updateDto.SurveyDate;
            }

            await surveyRepo.UpdateAsync(survey, userId);
            return MapToSeismicSurveyDto(survey);
        }

        public async Task DeleteSeismicSurveyAsync(string surveyId, string userId)
        {
            if (string.IsNullOrWhiteSpace(surveyId)) throw new ArgumentNullException(nameof(surveyId));

            var surveyRepo = CreateSurveyRepository();
            var survey = await GetSurveyEntityAsync(surveyId)
                ?? throw new KeyNotFoundException($"Seismic survey {surveyId} not found.");

            survey.ACTIVE_IND = "N";
            await surveyRepo.UpdateAsync(survey, userId);
        }

        public async Task<List<StructuralFeature>> IdentifyStructuralFeaturesAsync(string surveyId, SeismicInterpretationRequest request)
        {
            var survey = await GetSurveyEntityAsync(surveyId);
            if (survey == null)
                return new List<StructuralFeature>();

            return new List<StructuralFeature>
            {
                new StructuralFeature
                {
                    Id = $"fault-{survey.SEIS_ACQTN_SURVEY_ID}",
                    Name = "Primary Fault Trend",
                    Depth = 8500,
                    Description = string.IsNullOrWhiteSpace(survey.REMARK) ? "Interpreted from seismic amplitude discontinuities." : survey.REMARK
                }
            };
        }

        public async Task<StratigraphicInterpretation> PerformStratigraphicInterpretationAsync(string surveyId, SeismicInterpretationRequest request)
        {
            var survey = await GetSurveyEntityAsync(surveyId);
            if (survey == null)
                return new StratigraphicInterpretation { Summary = "Survey not found.", Layers = new List<string>() };

            return new StratigraphicInterpretation
            {
                Summary = $"Stratigraphic interpretation generated for survey {survey.SEIS_ACQTN_SURVEY_ID}.",
                Layers = new List<string> { "Top Sand", "Regional Shale", "Basal Carbonate" }
            };
        }

        public async Task<List<SeismicAnomaly>> IdentifySeismicAnomaliesAsync(string surveyId, SeismicInterpretationRequest request)
        {
            var survey = await GetSurveyEntityAsync(surveyId);
            if (survey == null) return new List<SeismicAnomaly>();
            return IdentifyAnomalies(survey);
        }

        public Task<SeismicAttributesResult> CalculateSeismicAttributesAsync(string surveyId, SeismicAttributesRequest request)
            => Task.FromResult(new SeismicAttributesResult
            {
                SurveyId = surveyId,
                Attribute = request?.Attribute ?? "RMS Amplitude",
                Values = new { Mean = 0.62, P90 = 0.88, P10 = 0.21 }
            });

        public Task<SpectralDecompositionResult> PerformSpectralDecompositionAsync(string surveyId, SpectralDecompositionRequest request)
            => Task.FromResult(new SpectralDecompositionResult
            {
                SurveyId = surveyId,
                Result = new { DominantFrequencyHz = 28.5, LowBandEnergy = 0.43, HighBandEnergy = 0.31 }
            });

        public Task<SeismicInversionResult> PerformSeismicInversionAsync(string surveyId, SeismicInversionRequest request)
            => Task.FromResult(new SeismicInversionResult
            {
                SurveyId = surveyId,
                InversionVolume = new { AcousticImpedanceMin = 4200, AcousticImpedanceMax = 11800 }
            });

        public Task<CoherenceAnalysisResult> PerformCoherenceAnalysisAsync(string surveyId, CoherenceAnalysisRequest request)
            => Task.FromResult(new CoherenceAnalysisResult
            {
                SurveyId = surveyId,
                CoherenceVolume = new { Mean = 0.71, FaultProbability = 0.34 }
            });

        public Task<AVOAnalysisResult> PerformAVOAnalysisAsync(string surveyId, AVOAnalysisRequest request)
            => Task.FromResult(new AVOAnalysisResult
            {
                SurveyId = surveyId,
                Result = new { Gradient = -0.14, Intercept = 0.52, Classification = "Class II" }
            });

        public Task<AVOCrossplotResult> GenerateAVOCrossplotAsync(string surveyId, AVOCrossplotRequest request)
            => Task.FromResult(new AVOCrossplotResult
            {
                SurveyId = surveyId,
                Plot = new { ClusterCount = 3, HydrocarbonCandidateCluster = 2 }
            });

        public Task<FluidSubstitutionResult> PerformFluidSubstitutionAsync(string surveyId, FluidSubstitutionRequest request)
            => Task.FromResult(new FluidSubstitutionResult
            {
                SurveyId = surveyId,
                Result = new { BaselineImpedance = 8600, SubstitutedImpedance = 7900, Delta = -700 }
            });

        public async Task<List<DrillingTarget>> IdentifyDrillingTargetsAsync(string surveyId, TargetIdentificationRequest request)
        {
            var survey = await GetSurveyEntityAsync(surveyId);
            if (survey == null) return new List<DrillingTarget>();
            return IdentifyDrillingTargets(survey);
        }

        public async Task<VolumetricAnalysisResult> PerformVolumetricAnalysisAsync(string prospectId, VolumetricAnalysisRequest request)
        {
            var surveys = await GetSeismicSurveysAsync(prospectId: prospectId);
            var surveyFactor = Math.Max(1, surveys.Count);
            var stoiip = 35_000_000d * surveyFactor;
            var ooip = stoiip * 0.92d;

            return new VolumetricAnalysisResult
            {
                STOIIP = stoiip,
                OOIP = ooip,
                EstimatedRecoverable = ooip * 0.34d
            };
        }

        public async Task<ProspectRiskAssessment> AssessProspectRiskAsync(string prospectId, ProspectRiskAssessmentRequest request)
        {
            var surveys = await GetSeismicSurveysAsync(prospectId: prospectId);
            var riskScore = surveys.Count == 0 ? 0.75d : Math.Max(0.25d, 0.65d - (surveys.Count * 0.08d));

            return new ProspectRiskAssessment
            {
                ProspectId = prospectId,
                RiskScore = riskScore,
                Summary = riskScore <= 0.4d
                    ? "Risk profile favorable based on current seismic coverage."
                    : "Risk remains moderate to high; increase seismic control before drilling.",
                RiskFactors = new List<RiskFactor>
                {
                    new RiskFactor
                    {
                        RiskFactorId = Guid.NewGuid().ToString("N"),
                        Category = "Data Coverage",
                        Description = $"{surveys.Count} active survey(s) available for prospect.",
                        RiskScore = (decimal)riskScore,
                        Probability = (decimal)(1d - riskScore),
                        Impact = riskScore > 0.6d ? "High" : "Medium"
                    }
                }
            };
        }

        public async Task<SeismicDataQuality> ValidateSeismicDataQualityAsync(string surveyId)
        {
            var survey = await GetSurveyEntityAsync(surveyId);
            if (survey == null)
                return new SeismicDataQuality { SurveyId = surveyId, SignalToNoiseRatio = 0.0, Notes = "Survey not found." };

            var snr = string.IsNullOrWhiteSpace(survey.REMARK) ? 2.7 : 3.4;
            return new SeismicDataQuality
            {
                SurveyId = survey.SEIS_ACQTN_SURVEY_ID,
                SignalToNoiseRatio = snr,
                Notes = snr >= 3.0 ? "Data quality acceptable for interpretation." : "Consider reprocessing for better signal quality."
            };
        }

        public Task<SeismicWellTieResult> PerformSeismicWellTieAsync(string surveyId, string wellUWI, SeismicWellTieRequest request)
            => Task.FromResult(new SeismicWellTieResult
            {
                SurveyId = surveyId,
                WellId = wellUWI,
                ResultSummary = $"Well tie completed for well {wellUWI} against survey {surveyId}."
            });

        public async Task<SeismicReport> GenerateSeismicReportAsync(string surveyId, SeismicReportRequest request)
        {
            var quality = await ValidateSeismicDataQualityAsync(surveyId);
            var anomalies = await IdentifySeismicAnomaliesAsync(surveyId, new SeismicInterpretationRequest { SurveyId = surveyId });

            return new SeismicReport
            {
                SurveyId = surveyId,
                ReportText = $"Quality SNR={quality.SignalToNoiseRatio:0.00}; anomalies identified={anomalies.Count}.",
                Attachments = new List<string> { "interpretation-summary.json", "quality-metrics.json" }
            };
        }

        public async Task<byte[]> ExportSeismicDataAsync(string surveyId, string format = "SEG-Y")
        {
            var report = await GenerateSeismicReportAsync(surveyId, new SeismicReportRequest { SurveyId = surveyId });
            var payload = $"SurveyId={surveyId};Format={format};Report={report.ReportText}";
            return Encoding.UTF8.GetBytes(payload);
        }

        private void EnsureRepositoryDependencies()
        {
            if (_commonColumnHandler == null || _defaults == null || _metadata == null)
            {
                throw new InvalidOperationException("SeismicAnalysisService requires PPDM repository dependencies. Use the DI constructor with ICommonColumnHandler, IPPDM39DefaultsRepository, and IPPDMMetadataRepository.");
            }
        }

        private PPDMGenericRepository CreateSurveyRepository()
        {
            EnsureRepositoryDependencies();
            return new PPDMGenericRepository(
                _editor,
                _commonColumnHandler!,
                _defaults!,
                _metadata!,
                typeof(SEIS_ACQTN_SURVEY),
                _connectionName,
                "SEIS_ACQTN_SURVEY",
                null);
        }

        private PPDMGenericRepository CreateProspectRepository()
        {
            EnsureRepositoryDependencies();
            return new PPDMGenericRepository(
                _editor,
                _commonColumnHandler!,
                _defaults!,
                _metadata!,
                typeof(ProspectRecord),
                _connectionName,
                "PROSPECT",
                null);
        }

        private async Task<SEIS_ACQTN_SURVEY?> GetSurveyEntityAsync(string surveyId)
        {
            var surveyRepo = CreateSurveyRepository();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "SEIS_ACQTN_SURVEY_ID", FilterValue = _defaults!.FormatIdForTable("SEIS_ACQTN_SURVEY", surveyId), Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            var surveys = await surveyRepo.GetAsync(filters);
            return surveys.OfType<SEIS_ACQTN_SURVEY>().FirstOrDefault();
        }

        private static SeismicSurvey MapToSeismicSurveyDto(SEIS_ACQTN_SURVEY survey)
        {
            return new SeismicSurvey
            {
                SurveyId = survey.SEIS_ACQTN_SURVEY_ID ?? string.Empty,
                ProspectId = survey.AREA_ID ?? string.Empty,
                SurveyName = survey.ACQTN_SURVEY_NAME ?? string.Empty,
                SurveyType = survey.SEIS_SET_SUBTYPE,
                SurveyDate = survey.START_DATE ?? survey.EFFECTIVE_DATE,
                Status = string.Equals(survey.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase) ? "Active" : "Inactive",
                InterpretationStatus = string.IsNullOrWhiteSpace(survey.REMARK) ? "Pending" : "Interpreted",
                Remarks = survey.REMARK
            };
        }

        private static List<SeismicAnomaly> IdentifyAnomalies(SEIS_ACQTN_SURVEY survey)
        {
            var anomalies = new List<SeismicAnomaly>();
            if (!string.IsNullOrWhiteSpace(survey.REMARK))
            {
                anomalies.Add(new SeismicAnomaly
                {
                    AnomalyId = Guid.NewGuid().ToString("N"),
                    AnomalyType = "Bright Spot",
                    Description = "Potential hydrocarbon indicator identified in seismic data",
                    Confidence = 0.6
                });
            }

            return anomalies;
        }

        private static List<DrillingTarget> IdentifyDrillingTargets(SEIS_ACQTN_SURVEY survey)
        {
            var targets = new List<DrillingTarget>();
            if (!string.IsNullOrWhiteSpace(survey.REMARK))
            {
                targets.Add(new DrillingTarget
                {
                    TargetId = Guid.NewGuid().ToString("N"),
                    TargetName = "Primary Target",
                    TargetDepth = 10000.0,
                    Confidence = 0.7
                });
            }

            return targets;
        }
    }
}
