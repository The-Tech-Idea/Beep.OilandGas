using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// DTO for prospect information.
    /// </summary>
    public class Prospect : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string ProspectNameValue = string.Empty;

        public string ProspectName

        {

            get { return this.ProspectNameValue; }

            set { SetProperty(ref ProspectNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string? LocationValue;

        public string? Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
        private decimal? LatitudeValue;

        public decimal? Latitude

        {

            get { return this.LatitudeValue; }

            set { SetProperty(ref LatitudeValue, value); }

        }
        private decimal? LongitudeValue;

        public decimal? Longitude

        {

            get { return this.LongitudeValue; }

            set { SetProperty(ref LongitudeValue, value); }

        }
        private string? CountryValue;

        public string? Country

        {

            get { return this.CountryValue; }

            set { SetProperty(ref CountryValue, value); }

        }
        private string? StateProvinceValue;

        public string? StateProvince

        {

            get { return this.StateProvinceValue; }

            set { SetProperty(ref StateProvinceValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private DateTime? CreatedDateValue;

        public DateTime? CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private DateTime? EvaluationDateValue;

        public DateTime? EvaluationDate

        {

            get { return this.EvaluationDateValue; }

            set { SetProperty(ref EvaluationDateValue, value); }

        }
        private string? EvaluatedByValue;

        public string? EvaluatedBy

        {

            get { return this.EvaluatedByValue; }

            set { SetProperty(ref EvaluatedByValue, value); }

        }
        private decimal? EstimatedResourcesValue;

        public decimal? EstimatedResources

        {

            get { return this.EstimatedResourcesValue; }

            set { SetProperty(ref EstimatedResourcesValue, value); }

        }
        private string? ResourceUnitValue;

        public string? ResourceUnit

        {

            get { return this.ResourceUnitValue; }

            set { SetProperty(ref ResourceUnitValue, value); }

        }
        private decimal? RiskScoreValue;

        public decimal? RiskScore

        {

            get { return this.RiskScoreValue; }

            set { SetProperty(ref RiskScoreValue, value); }

        }
        private string? RiskLevelValue;

        public string? RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        }
        private List<SeismicSurvey> SeismicSurveysValue = new();

        public List<SeismicSurvey> SeismicSurveys

        {

            get { return this.SeismicSurveysValue; }

            set { SetProperty(ref SeismicSurveysValue, value); }

        }
        private ProspectEvaluation? EvaluationValue;

        public ProspectEvaluation? Evaluation

        {

            get { return this.EvaluationValue; }

            set { SetProperty(ref EvaluationValue, value); }

        }
    }

    /// <summary>
    /// DTO for seismic survey information.
    /// </summary>
    public class SeismicSurvey : ModelEntityBase
    {
        private string SurveyIdValue = string.Empty;

        public string SurveyId

        {

            get { return this.SurveyIdValue; }

            set { SetProperty(ref SurveyIdValue, value); }

        }
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string SurveyNameValue = string.Empty;

        public string SurveyName

        {

            get { return this.SurveyNameValue; }

            set { SetProperty(ref SurveyNameValue, value); }

        }
        private string? SurveyTypeValue;

        public string? SurveyType

        {

            get { return this.SurveyTypeValue; }

            set { SetProperty(ref SurveyTypeValue, value); }

        }
        private DateTime? SurveyDateValue;

        public DateTime? SurveyDate

        {

            get { return this.SurveyDateValue; }

            set { SetProperty(ref SurveyDateValue, value); }

        }
        private string? ContractorValue;

        public string? Contractor

        {

            get { return this.ContractorValue; }

            set { SetProperty(ref ContractorValue, value); }

        }
        private decimal? AreaCoveredValue;

        public decimal? AreaCovered

        {

            get { return this.AreaCoveredValue; }

            set { SetProperty(ref AreaCoveredValue, value); }

        }
        private string? AreaUnitValue;

        public string? AreaUnit

        {

            get { return this.AreaUnitValue; }

            set { SetProperty(ref AreaUnitValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? InterpretationStatusValue;

        public string? InterpretationStatus

        {

            get { return this.InterpretationStatusValue; }

            set { SetProperty(ref InterpretationStatusValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }

    /// <summary>
    /// DTO for prospect evaluation results.
    /// </summary>
    public class ProspectEvaluation : ModelEntityBase
    {
        private string EvaluationIdValue = string.Empty;

        public string EvaluationId

        {

            get { return this.EvaluationIdValue; }

            set { SetProperty(ref EvaluationIdValue, value); }

        }
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private DateTime EvaluationDateValue;

        public DateTime EvaluationDate

        {

            get { return this.EvaluationDateValue; }

            set { SetProperty(ref EvaluationDateValue, value); }

        }
        private string EvaluatedByValue = string.Empty;

        public string EvaluatedBy

        {

            get { return this.EvaluatedByValue; }

            set { SetProperty(ref EvaluatedByValue, value); }

        }
        private decimal? EstimatedOilResourcesValue;

        public decimal? EstimatedOilResources

        {

            get { return this.EstimatedOilResourcesValue; }

            set { SetProperty(ref EstimatedOilResourcesValue, value); }

        }
        private decimal? EstimatedGasResourcesValue;

        public decimal? EstimatedGasResources

        {

            get { return this.EstimatedGasResourcesValue; }

            set { SetProperty(ref EstimatedGasResourcesValue, value); }

        }
        private string? ResourceUnitValue;

        public string? ResourceUnit

        {

            get { return this.ResourceUnitValue; }

            set { SetProperty(ref ResourceUnitValue, value); }

        }
        private decimal? ProbabilityOfSuccessValue;

        public decimal? ProbabilityOfSuccess

        {

            get { return this.ProbabilityOfSuccessValue; }

            set { SetProperty(ref ProbabilityOfSuccessValue, value); }

        }
        private decimal? RiskScoreValue;

        public decimal? RiskScore

        {

            get { return this.RiskScoreValue; }

            set { SetProperty(ref RiskScoreValue, value); }

        }
        private string? RiskLevelValue;

        public string? RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        }
        private string? RecommendationValue;

        public string? Recommendation

        {

            get { return this.RecommendationValue; }

            set { SetProperty(ref RecommendationValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
        private List<RiskFactor> RiskFactorsValue = new();

        public List<RiskFactor> RiskFactors

        {

            get { return this.RiskFactorsValue; }

            set { SetProperty(ref RiskFactorsValue, value); }

        }
    }

    /// <summary>
    /// DTO for risk factors in prospect evaluation.
    /// </summary>
    public class RiskFactor : ModelEntityBase
    {
        private string RiskFactorIdValue = string.Empty;

        public string RiskFactorId

        {

            get { return this.RiskFactorIdValue; }

            set { SetProperty(ref RiskFactorIdValue, value); }

        }
        private string CategoryValue = string.Empty;

        public string Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private decimal? RiskScoreValue;

        public decimal? RiskScore

        {

            get { return this.RiskScoreValue; }

            set { SetProperty(ref RiskScoreValue, value); }

        }
        private string? MitigationValue;

        public string? Mitigation

        {

            get { return this.MitigationValue; }

            set { SetProperty(ref MitigationValue, value); }

        }
    }

    /// <summary>
    /// DTO for creating a new prospect.
    /// </summary>
    public class CreateProspect : ModelEntityBase
    {
        private string ProspectNameValue = string.Empty;

        public string ProspectName

        {

            get { return this.ProspectNameValue; }

            set { SetProperty(ref ProspectNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string? LocationValue;

        public string? Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
        private decimal? LatitudeValue;

        public decimal? Latitude

        {

            get { return this.LatitudeValue; }

            set { SetProperty(ref LatitudeValue, value); }

        }
        private decimal? LongitudeValue;

        public decimal? Longitude

        {

            get { return this.LongitudeValue; }

            set { SetProperty(ref LongitudeValue, value); }

        }
        private string? CountryValue;

        public string? Country

        {

            get { return this.CountryValue; }

            set { SetProperty(ref CountryValue, value); }

        }
        private string? StateProvinceValue;

        public string? StateProvince

        {

            get { return this.StateProvinceValue; }

            set { SetProperty(ref StateProvinceValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
    }

    /// <summary>
    /// DTO for updating a prospect.
    /// </summary>
     public class UpdateProspect : ModelEntityBase
     {
         private string? ProspectNameValue;

         public string? ProspectName

         {

             get { return this.ProspectNameValue; }

             set { SetProperty(ref ProspectNameValue, value); }

         }
         private string? DescriptionValue;

         public string? Description

         {

             get { return this.DescriptionValue; }

             set { SetProperty(ref DescriptionValue, value); }

         }
         private string? StatusValue;

         public string? Status

         {

             get { return this.StatusValue; }

             set { SetProperty(ref StatusValue, value); }

         }
         private decimal? EstimatedResourcesValue;

         public decimal? EstimatedResources

         {

             get { return this.EstimatedResourcesValue; }

             set { SetProperty(ref EstimatedResourcesValue, value); }

         }
         private string? ResourceUnitValue;

         public string? ResourceUnit

         {

             get { return this.ResourceUnitValue; }

             set { SetProperty(ref ResourceUnitValue, value); }

         }
     }

     /// <summary>
     /// DTO for prospect ranking results
     /// </summary>
     public class ProspectRanking : ModelEntityBase
     {
         private string ProspectIdValue = string.Empty;

         public string ProspectId

         {

             get { return this.ProspectIdValue; }

             set { SetProperty(ref ProspectIdValue, value); }

         }
         private string ProspectNameValue = string.Empty;

         public string ProspectName

         {

             get { return this.ProspectNameValue; }

             set { SetProperty(ref ProspectNameValue, value); }

         }
         private int RankValue;

         public int Rank

         {

             get { return this.RankValue; }

             set { SetProperty(ref RankValue, value); }

         }
         private decimal ScoreValue;

         public decimal Score

         {

             get { return this.ScoreValue; }

             set { SetProperty(ref ScoreValue, value); }

         }
         private decimal WeightedScoreValue;

         public decimal WeightedScore

         {

             get { return this.WeightedScoreValue; }

             set { SetProperty(ref WeightedScoreValue, value); }

         }
         private List<CriteriaScoring> CriteriaScoresValue = new();

         public List<CriteriaScoring> CriteriaScores

         {

             get { return this.CriteriaScoresValue; }

             set { SetProperty(ref CriteriaScoresValue, value); }

         }
     }

     /// <summary>
     /// DTO for seismic data interpretation analysis
     /// </summary>
     public class SeismicInterpretationAnalysis : ModelEntityBase
     {
         private string AnalysisIdValue = string.Empty;

         public string AnalysisId

         {

             get { return this.AnalysisIdValue; }

             set { SetProperty(ref AnalysisIdValue, value); }

         }
         private string ProspectIdValue = string.Empty;

         public string ProspectId

         {

             get { return this.ProspectIdValue; }

             set { SetProperty(ref ProspectIdValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private string SurveyIdValue = string.Empty;

         public string SurveyId

         {

             get { return this.SurveyIdValue; }

             set { SetProperty(ref SurveyIdValue, value); }

         }
         private int HorizonCountValue;

         public int HorizonCount

         {

             get { return this.HorizonCountValue; }

             set { SetProperty(ref HorizonCountValue, value); }

         }
         private int FaultCountValue;

         public int FaultCount

         {

             get { return this.FaultCountValue; }

             set { SetProperty(ref FaultCountValue, value); }

         }
         private List<Horizon> HorizonsValue = new();

         public List<Horizon> Horizons

         {

             get { return this.HorizonsValue; }

             set { SetProperty(ref HorizonsValue, value); }

         }
         private List<Fault> FaultsValue = new();

         public List<Fault> Faults

         {

             get { return this.FaultsValue; }

             set { SetProperty(ref FaultsValue, value); }

         }
         private decimal InterpretationConfidenceValue;

         public decimal InterpretationConfidence

         {

             get { return this.InterpretationConfidenceValue; }

             set { SetProperty(ref InterpretationConfidenceValue, value); }

         }
         private string InterpretationStatusValue = string.Empty;

         public string InterpretationStatus

         {

             get { return this.InterpretationStatusValue; }

             set { SetProperty(ref InterpretationStatusValue, value); }

         }
     }

     /// <summary>
     /// DTO for seismic horizon
     /// </summary>
     public class Horizon : ModelEntityBase
     {
         private string HorizonIdValue = string.Empty;

         public string HorizonId

         {

             get { return this.HorizonIdValue; }

             set { SetProperty(ref HorizonIdValue, value); }

         }
         private string HorizonNameValue = string.Empty;

         public string HorizonName

         {

             get { return this.HorizonNameValue; }

             set { SetProperty(ref HorizonNameValue, value); }

         }
         private string GeologicalAgeValue = string.Empty;

         public string GeologicalAge

         {

             get { return this.GeologicalAgeValue; }

             set { SetProperty(ref GeologicalAgeValue, value); }

         }
         private decimal DepthValue;

         public decimal Depth

         {

             get { return this.DepthValue; }

             set { SetProperty(ref DepthValue, value); }

         }
         private decimal ThicknessValue;

         public decimal Thickness

         {

             get { return this.ThicknessValue; }

             set { SetProperty(ref ThicknessValue, value); }

         }
         private string LithologyTypeValue = string.Empty;

         public string LithologyType

         {

             get { return this.LithologyTypeValue; }

             set { SetProperty(ref LithologyTypeValue, value); }

         }
         private string ReservoirQualityValue = string.Empty;

         public string ReservoirQuality

         {

             get { return this.ReservoirQualityValue; }

             set { SetProperty(ref ReservoirQualityValue, value); }

         }
     }

     /// <summary>
     /// DTO for seismic fault
     /// </summary>
     public class Fault : ModelEntityBase
     {
         private string FaultIdValue = string.Empty;

         public string FaultId

         {

             get { return this.FaultIdValue; }

             set { SetProperty(ref FaultIdValue, value); }

         }
         private string FaultNameValue = string.Empty;

         public string FaultName

         {

             get { return this.FaultNameValue; }

             set { SetProperty(ref FaultNameValue, value); }

         }
         private decimal ThrowValue;

         public decimal Throw

         {

             get { return this.ThrowValue; }

             set { SetProperty(ref ThrowValue, value); }

         }
         private string FaultTypeValue = string.Empty;

         public string FaultType

         {

             get { return this.FaultTypeValue; }

             set { SetProperty(ref FaultTypeValue, value); }

         }
         private string SealingPotentialValue = string.Empty;

         public string SealingPotential

         {

             get { return this.SealingPotentialValue; }

             set { SetProperty(ref SealingPotentialValue, value); }

         }
     }

     /// <summary>
     /// DTO for resource estimation
     /// </summary>
     public class ResourceEstimationResult : ModelEntityBase
     {
         private string EstimationIdValue = string.Empty;

         public string EstimationId

         {

             get { return this.EstimationIdValue; }

             set { SetProperty(ref EstimationIdValue, value); }

         }
         private string ProspectIdValue = string.Empty;

         public string ProspectId

         {

             get { return this.ProspectIdValue; }

             set { SetProperty(ref ProspectIdValue, value); }

         }
         private DateTime EstimationDateValue;

         public DateTime EstimationDate

         {

             get { return this.EstimationDateValue; }

             set { SetProperty(ref EstimationDateValue, value); }

         }
         private string EstimatedByValue = string.Empty;

         public string EstimatedBy

         {

             get { return this.EstimatedByValue; }

             set { SetProperty(ref EstimatedByValue, value); }

         }
         private decimal GrossRockVolumeValue;

         public decimal GrossRockVolume

         {

             get { return this.GrossRockVolumeValue; }

             set { SetProperty(ref GrossRockVolumeValue, value); }

         }
         private decimal NetRockVolumeValue;

         public decimal NetRockVolume

         {

             get { return this.NetRockVolumeValue; }

             set { SetProperty(ref NetRockVolumeValue, value); }

         }
         private decimal PorosityValue;

         public decimal Porosity

         {

             get { return this.PorosityValue; }

             set { SetProperty(ref PorosityValue, value); }

         }
         private decimal WaterSaturationValue;

         public decimal WaterSaturation

         {

             get { return this.WaterSaturationValue; }

             set { SetProperty(ref WaterSaturationValue, value); }

         }
         private decimal OilRecoveryFactorValue;

         public decimal OilRecoveryFactor

         {

             get { return this.OilRecoveryFactorValue; }

             set { SetProperty(ref OilRecoveryFactorValue, value); }

         }
         private decimal GasRecoveryFactorValue;

         public decimal GasRecoveryFactor

         {

             get { return this.GasRecoveryFactorValue; }

             set { SetProperty(ref GasRecoveryFactorValue, value); }

         }
         private decimal EstimatedOilVolumeValue;

         public decimal EstimatedOilVolume

         {

             get { return this.EstimatedOilVolumeValue; }

             set { SetProperty(ref EstimatedOilVolumeValue, value); }

         }
         private decimal EstimatedGasVolumeValue;

         public decimal EstimatedGasVolume

         {

             get { return this.EstimatedGasVolumeValue; }

             set { SetProperty(ref EstimatedGasVolumeValue, value); }

         }
         private string VolumeUnitValue = string.Empty;

         public string VolumeUnit

         {

             get { return this.VolumeUnitValue; }

             set { SetProperty(ref VolumeUnitValue, value); }

         }
         private string EstimationMethodValue = string.Empty;

         public string EstimationMethod

         {

             get { return this.EstimationMethodValue; }

             set { SetProperty(ref EstimationMethodValue, value); }

         }
         private List<string> AssumptionsAndLimitationsValue = new();

         public List<string> AssumptionsAndLimitations

         {

             get { return this.AssumptionsAndLimitationsValue; }

             set { SetProperty(ref AssumptionsAndLimitationsValue, value); }

         }
     }

     /// <summary>
     /// DTO for trap geometry analysis
     /// </summary>
     public class TrapGeometryAnalysis : ModelEntityBase
     {
         private string AnalysisIdValue = string.Empty;

         public string AnalysisId

         {

             get { return this.AnalysisIdValue; }

             set { SetProperty(ref AnalysisIdValue, value); }

         }
         private string ProspectIdValue = string.Empty;

         public string ProspectId

         {

             get { return this.ProspectIdValue; }

             set { SetProperty(ref ProspectIdValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private string TrapTypeValue = string.Empty;

         public string TrapType

         {

             get { return this.TrapTypeValue; }

             set { SetProperty(ref TrapTypeValue, value); }

         } // Structural, Stratigraphic, Combination
         private decimal ClosureValue;

         public decimal Closure

         {

             get { return this.ClosureValue; }

             set { SetProperty(ref ClosureValue, value); }

         }
         private decimal CrestDepthValue;

         public decimal CrestDepth

         {

             get { return this.CrestDepthValue; }

             set { SetProperty(ref CrestDepthValue, value); }

         }
         private decimal SpillPointDepthValue;

         public decimal SpillPointDepth

         {

             get { return this.SpillPointDepthValue; }

             set { SetProperty(ref SpillPointDepthValue, value); }

         }
         private decimal AreaValue;

         public decimal Area

         {

             get { return this.AreaValue; }

             set { SetProperty(ref AreaValue, value); }

         }
         private string AreaUnitValue = string.Empty;

         public string AreaUnit

         {

             get { return this.AreaUnitValue; }

             set { SetProperty(ref AreaUnitValue, value); }

         }
         private decimal VolumeValue;

         public decimal Volume

         {

             get { return this.VolumeValue; }

             set { SetProperty(ref VolumeValue, value); }

         }
         private string VolumeUnitValue = string.Empty;

         public string VolumeUnit

         {

             get { return this.VolumeUnitValue; }

             set { SetProperty(ref VolumeUnitValue, value); }

         }
         private string TrapGeometryValue = string.Empty;

         public string TrapGeometry

         {

             get { return this.TrapGeometryValue; }

             set { SetProperty(ref TrapGeometryValue, value); }

         }
         private string SourceRockProximityValue = string.Empty;

         public string SourceRockProximity

         {

             get { return this.SourceRockProximityValue; }

             set { SetProperty(ref SourceRockProximityValue, value); }

         }
     }

     /// <summary>
     /// DTO for migration path analysis
     /// </summary>
     public class MigrationPathAnalysis : ModelEntityBase
     {
         private string AnalysisIdValue = string.Empty;

         public string AnalysisId

         {

             get { return this.AnalysisIdValue; }

             set { SetProperty(ref AnalysisIdValue, value); }

         }
         private string ProspectIdValue = string.Empty;

         public string ProspectId

         {

             get { return this.ProspectIdValue; }

             set { SetProperty(ref ProspectIdValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private string SourceRockIdValue = string.Empty;

         public string SourceRockId

         {

             get { return this.SourceRockIdValue; }

             set { SetProperty(ref SourceRockIdValue, value); }

         }
         private decimal SourceRockMaturityLevelValue;

         public decimal SourceRockMaturityLevel

         {

             get { return this.SourceRockMaturityLevelValue; }

             set { SetProperty(ref SourceRockMaturityLevelValue, value); }

         }
         private string MigrationPathwayValue = string.Empty;

         public string MigrationPathway

         {

             get { return this.MigrationPathwayValue; }

             set { SetProperty(ref MigrationPathwayValue, value); }

         }
         private decimal MigrationDistanceValue;

         public decimal MigrationDistance

         {

             get { return this.MigrationDistanceValue; }

             set { SetProperty(ref MigrationDistanceValue, value); }

         }
         private string DistanceUnitValue = string.Empty;

         public string DistanceUnit

         {

             get { return this.DistanceUnitValue; }

             set { SetProperty(ref DistanceUnitValue, value); }

         }
         private decimal MigrationEfficiencyValue;

         public decimal MigrationEfficiency

         {

             get { return this.MigrationEfficiencyValue; }

             set { SetProperty(ref MigrationEfficiencyValue, value); }

         }
         private string SealIntegrityValue = string.Empty;

         public string SealIntegrity

         {

             get { return this.SealIntegrityValue; }

             set { SetProperty(ref SealIntegrityValue, value); }

         }
         private string LateralMigrationRiskValue = string.Empty;

         public string LateralMigrationRisk

         {

             get { return this.LateralMigrationRiskValue; }

             set { SetProperty(ref LateralMigrationRiskValue, value); }

         }
         private List<string> MigrationBarriersValue = new();

         public List<string> MigrationBarriers

         {

             get { return this.MigrationBarriersValue; }

             set { SetProperty(ref MigrationBarriersValue, value); }

         }
     }

     /// <summary>
     /// DTO for seal and source rock assessment
     /// </summary>
     public class SealSourceAssessment : ModelEntityBase
     {
         private string AssessmentIdValue = string.Empty;

         public string AssessmentId

         {

             get { return this.AssessmentIdValue; }

             set { SetProperty(ref AssessmentIdValue, value); }

         }
         private string ProspectIdValue = string.Empty;

         public string ProspectId

         {

             get { return this.ProspectIdValue; }

             set { SetProperty(ref ProspectIdValue, value); }

         }
         private DateTime AssessmentDateValue;

         public DateTime AssessmentDate

         {

             get { return this.AssessmentDateValue; }

             set { SetProperty(ref AssessmentDateValue, value); }

         }
         private string SealRockTypeValue = string.Empty;

         public string SealRockType

         {

             get { return this.SealRockTypeValue; }

             set { SetProperty(ref SealRockTypeValue, value); }

         }
         private decimal SealRockThicknessValue;

         public decimal SealRockThickness

         {

             get { return this.SealRockThicknessValue; }

             set { SetProperty(ref SealRockThicknessValue, value); }

         }
         private string SealQualityValue = string.Empty;

         public string SealQuality

         {

             get { return this.SealQualityValue; }

             set { SetProperty(ref SealQualityValue, value); }

         }
         private decimal SealIntegrityScoreValue;

         public decimal SealIntegrityScore

         {

             get { return this.SealIntegrityScoreValue; }

             set { SetProperty(ref SealIntegrityScoreValue, value); }

         }
         private string SourceRockTypeValue = string.Empty;

         public string SourceRockType

         {

             get { return this.SourceRockTypeValue; }

             set { SetProperty(ref SourceRockTypeValue, value); }

         }
         private decimal SourceRockMaturityValue;

         public decimal SourceRockMaturity

         {

             get { return this.SourceRockMaturityValue; }

             set { SetProperty(ref SourceRockMaturityValue, value); }

         }
         private string GenerationStatusValue = string.Empty;

         public string GenerationStatus

         {

             get { return this.GenerationStatusValue; }

             set { SetProperty(ref GenerationStatusValue, value); }

         }
         private decimal SourceRockProductivityValue;

         public decimal SourceRockProductivity

         {

             get { return this.SourceRockProductivityValue; }

             set { SetProperty(ref SourceRockProductivityValue, value); }

         }
         private string SystemStatusValue = string.Empty;

         public string SystemStatus

         {

             get { return this.SystemStatusValue; }

             set { SetProperty(ref SystemStatusValue, value); }

         } // Active, Inactive, Marginal
     }

     /// <summary>
     /// DTO for risk category details
     /// </summary>
     public class RiskCategory : ModelEntityBase
     {
         private string CategoryNameValue = string.Empty;

         public string CategoryName

         {

             get { return this.CategoryNameValue; }

             set { SetProperty(ref CategoryNameValue, value); }

         }
         private decimal RiskScoreValue;

         public decimal RiskScore

         {

             get { return this.RiskScoreValue; }

             set { SetProperty(ref RiskScoreValue, value); }

         }
         private string RiskLevelValue = string.Empty;

         public string RiskLevel

         {

             get { return this.RiskLevelValue; }

             set { SetProperty(ref RiskLevelValue, value); }

         }
         private List<string> MitigationStrategiesValue = new();

         public List<string> MitigationStrategies

         {

             get { return this.MitigationStrategiesValue; }

             set { SetProperty(ref MitigationStrategiesValue, value); }

         }
     }

     /// <summary>
     /// DTO for economic viability analysis
     /// </summary>
     public class EconomicViabilityAnalysis : ModelEntityBase
     {
         private string AnalysisIdValue = string.Empty;

         public string AnalysisId

         {

             get { return this.AnalysisIdValue; }

             set { SetProperty(ref AnalysisIdValue, value); }

         }
         private string ProspectIdValue = string.Empty;

         public string ProspectId

         {

             get { return this.ProspectIdValue; }

             set { SetProperty(ref ProspectIdValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private decimal EstimatedCapitalCostValue;

         public decimal EstimatedCapitalCost

         {

             get { return this.EstimatedCapitalCostValue; }

             set { SetProperty(ref EstimatedCapitalCostValue, value); }

         }
         private decimal EstimatedOperatingCostValue;

         public decimal EstimatedOperatingCost

         {

             get { return this.EstimatedOperatingCostValue; }

             set { SetProperty(ref EstimatedOperatingCostValue, value); }

         }
         private decimal OilPriceValue;

         public decimal OilPrice

         {

             get { return this.OilPriceValue; }

             set { SetProperty(ref OilPriceValue, value); }

         }
         private decimal GasPriceValue;

         public decimal GasPrice

         {

             get { return this.GasPriceValue; }

             set { SetProperty(ref GasPriceValue, value); }

         }
         private decimal DiscountRateValue;

         public decimal DiscountRate

         {

             get { return this.DiscountRateValue; }

             set { SetProperty(ref DiscountRateValue, value); }

         }
         private int ProjectLifeYearsValue;

         public int ProjectLifeYears

         {

             get { return this.ProjectLifeYearsValue; }

             set { SetProperty(ref ProjectLifeYearsValue, value); }

         }
         private decimal NetPresentValueValue;

         public decimal NetPresentValue

         {

             get { return this.NetPresentValueValue; }

             set { SetProperty(ref NetPresentValueValue, value); }

         }
         private decimal InternalRateOfReturnValue;

         public decimal InternalRateOfReturn

         {

             get { return this.InternalRateOfReturnValue; }

             set { SetProperty(ref InternalRateOfReturnValue, value); }

         }
         private decimal PaybackPeriodYearsValue;

         public decimal PaybackPeriodYears

         {

             get { return this.PaybackPeriodYearsValue; }

             set { SetProperty(ref PaybackPeriodYearsValue, value); }

         }
         private decimal ProfitabilityIndexValue;

         public decimal ProfitabilityIndex

         {

             get { return this.ProfitabilityIndexValue; }

             set { SetProperty(ref ProfitabilityIndexValue, value); }

         }
         private string ViabilityStatusValue = string.Empty;

         public string ViabilityStatus

         {

             get { return this.ViabilityStatusValue; }

             set { SetProperty(ref ViabilityStatusValue, value); }

         } // Viable, Marginal, Non-Viable
     }

     /// <summary>
     /// DTO for criteria scoring in ranking
     /// </summary>
     public class CriteriaScoring : ModelEntityBase
     {
         private string CriteriaNameValue = string.Empty;

         public string CriteriaName

         {

             get { return this.CriteriaNameValue; }

             set { SetProperty(ref CriteriaNameValue, value); }

         }
         private decimal WeightValue;

         public decimal Weight

         {

             get { return this.WeightValue; }

             set { SetProperty(ref WeightValue, value); }

         }
         private decimal RawScoreValue;

         public decimal RawScore

         {

             get { return this.RawScoreValue; }

             set { SetProperty(ref RawScoreValue, value); }

         }
         private decimal WeightedScoreValue;

         public decimal WeightedScore

         {

             get { return this.WeightedScoreValue; }

             set { SetProperty(ref WeightedScoreValue, value); }

         }
         private string CategoryValue = string.Empty;

         public string Category

         {

             get { return this.CategoryValue; }

             set { SetProperty(ref CategoryValue, value); }

         }
     }

     /// <summary>
     /// DTO for prospect risk analysis result
     /// </summary>
     public class ProspectRiskAnalysisResult : ModelEntityBase
     {
         private string AnalysisIdValue = string.Empty;

         public string AnalysisId

         {

             get { return this.AnalysisIdValue; }

             set { SetProperty(ref AnalysisIdValue, value); }

         }
         private string ProspectIdValue = string.Empty;

         public string ProspectId

         {

             get { return this.ProspectIdValue; }

             set { SetProperty(ref ProspectIdValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private string AssessedByValue = string.Empty;

         public string AssessedBy

         {

             get { return this.AssessedByValue; }

             set { SetProperty(ref AssessedByValue, value); }

         }
         private decimal TrapRiskValue;

         public decimal TrapRisk

         {

             get { return this.TrapRiskValue; }

             set { SetProperty(ref TrapRiskValue, value); }

         }
         private decimal SealRiskValue;

         public decimal SealRisk

         {

             get { return this.SealRiskValue; }

             set { SetProperty(ref SealRiskValue, value); }

         }
         private decimal SourceRiskValue;

         public decimal SourceRisk

         {

             get { return this.SourceRiskValue; }

             set { SetProperty(ref SourceRiskValue, value); }

         }
         private decimal MigrationRiskValue;

         public decimal MigrationRisk

         {

             get { return this.MigrationRiskValue; }

             set { SetProperty(ref MigrationRiskValue, value); }

         }
         private decimal CharacterizationRiskValue;

         public decimal CharacterizationRisk

         {

             get { return this.CharacterizationRiskValue; }

             set { SetProperty(ref CharacterizationRiskValue, value); }

         }
         private decimal OverallRiskValue;

         public decimal OverallRisk

         {

             get { return this.OverallRiskValue; }

             set { SetProperty(ref OverallRiskValue, value); }

         }
         private decimal ProbabilityOfSuccessValue;

         public decimal ProbabilityOfSuccess

         {

             get { return this.ProbabilityOfSuccessValue; }

             set { SetProperty(ref ProbabilityOfSuccessValue, value); }

         }
         private string OverallRiskLevelValue = string.Empty;

         public string OverallRiskLevel

         {

             get { return this.OverallRiskLevelValue; }

             set { SetProperty(ref OverallRiskLevelValue, value); }

         }
         private List<RiskCategory> RiskCategoriesValue = new();

         public List<RiskCategory> RiskCategories

         {

             get { return this.RiskCategoriesValue; }

             set { SetProperty(ref RiskCategoriesValue, value); }

         }
     }

     /// <summary>
     /// DTO for portfolio optimization
     /// </summary>
     public class PortfolioOptimizationResult : ModelEntityBase
     {
         private string OptimizationIdValue = string.Empty;

         public string OptimizationId

         {

             get { return this.OptimizationIdValue; }

             set { SetProperty(ref OptimizationIdValue, value); }

         }
         private DateTime OptimizationDateValue;

         public DateTime OptimizationDate

         {

             get { return this.OptimizationDateValue; }

             set { SetProperty(ref OptimizationDateValue, value); }

         }
         private List<string> RecommendedProspectsValue = new();

         public List<string> RecommendedProspects

         {

             get { return this.RecommendedProspectsValue; }

             set { SetProperty(ref RecommendedProspectsValue, value); }

         }
         private List<string> MarginallProspectsValue = new();

         public List<string> MarginallProspects

         {

             get { return this.MarginallProspectsValue; }

             set { SetProperty(ref MarginallProspectsValue, value); }

         }
         private List<string> RejectedProspectsValue = new();

         public List<string> RejectedProspects

         {

             get { return this.RejectedProspectsValue; }

             set { SetProperty(ref RejectedProspectsValue, value); }

         }
         private decimal TotalPortfolioRiskValue;

         public decimal TotalPortfolioRisk

         {

             get { return this.TotalPortfolioRiskValue; }

             set { SetProperty(ref TotalPortfolioRiskValue, value); }

         }
         private decimal TotalExpectedValueValue;

         public decimal TotalExpectedValue

         {

             get { return this.TotalExpectedValueValue; }

             set { SetProperty(ref TotalExpectedValueValue, value); }

         }
         private decimal RiskAdjustedReturnValue;

         public decimal RiskAdjustedReturn

         {

             get { return this.RiskAdjustedReturnValue; }

             set { SetProperty(ref RiskAdjustedReturnValue, value); }

         }
         private string OptimizationStrategyValue = string.Empty;

         public string OptimizationStrategy

         {

             get { return this.OptimizationStrategyValue; }

             set { SetProperty(ref OptimizationStrategyValue, value); }

         }
         private List<string> OptimizationRecommendationsValue = new();

         public List<string> OptimizationRecommendations

         {

             get { return this.OptimizationRecommendationsValue; }

             set { SetProperty(ref OptimizationRecommendationsValue, value); }

         }
     }
}







